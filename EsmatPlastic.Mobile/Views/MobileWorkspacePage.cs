using System.Text.Json;
using EsmatPlastic.Shared.Models.Auth;
using EsmatPlastic.Shared.Services;

namespace EsmatPlastic.Mobile.Views;

/// <summary>Firebase-backed mobile workspace for the modules available in the desktop application.</summary>
public sealed class MobileWorkspacePage : ContentPage
{
    private readonly LoginResponse _user;
    private readonly Action _logout;
    private readonly FirebaseFirestoreClient _firestore = App.SharedFirestoreClient;
    private readonly VerticalStackLayout _content = new() { Spacing = 12, Padding = new Thickness(18, 8, 18, 24) };
    private readonly Label _message = new() { TextColor = Color.FromArgb("#64748B"), HorizontalTextAlignment = TextAlignment.Center };
    private string _section = "Products";

    public MobileWorkspacePage(LoginResponse user, Action logout, string initialSection = "Products")
    {
        _user = user;
        _logout = logout;
        _section = initialSection;
        Title = "Workspace";
        BackgroundColor = Color.FromArgb("#F1F5F9");
        FlowDirection = FlowDirection.RightToLeft;

        var sections = new[] { ("Products", "المنتجات"), ("Warehouse", "المخزون"), ("Reports", "التقارير"), ("Orders", "طلبات الحجز"), ("Users", "المستخدمون"), ("Permissions", "الصلاحيات"), ("Settings", "الإعدادات") };
        var nav = new HorizontalStackLayout { Spacing = 8, Padding = new Thickness(14, 12), BackgroundColor = Colors.White };
        foreach (var item in sections)
        {
            var button = new Button { Text = item.Item2, FontSize = 13, Padding = new Thickness(12, 8), CornerRadius = 18, BackgroundColor = Color.FromArgb("#E8F6F3"), TextColor = Color.FromArgb("#0F766E") };
            button.Clicked += async (_, _) => { _section = item.Item1; await LoadSectionAsync(); };
            nav.Children.Add(button);
        }

        var header = new VerticalStackLayout { Padding = new Thickness(20, 18, 20, 12), Spacing = 3, BackgroundColor = Colors.White };
        header.Children.Add(new Label { Text = "إسمت بلاستيك", FontSize = 23, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1E293B") });
        header.Children.Add(new Label { Text = string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName, FontSize = 14, TextColor = Color.FromArgb("#64748B") });
        var refresh = new Button { Text = "تحديث البيانات", BackgroundColor = Color.FromArgb("#0D9488"), TextColor = Colors.White, CornerRadius = 10 };
        refresh.Clicked += async (_, _) => await LoadSectionAsync();
        _content.Children.Add(_message);
        Content = new Grid { RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) }, Children = { header, nav, new ScrollView { Content = new VerticalStackLayout { Children = { refresh, _content } } } } };
        Grid.SetRow(nav, 1);
        Grid.SetRow((View)((Grid)Content).Children[2], 2);
    }

    protected override async void OnAppearing() { base.OnAppearing(); await LoadSectionAsync(); }

    private bool Has(string permission) => _user.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);

    private async Task LoadSectionAsync()
    {
        _message.Text = "جارٍ تحميل البيانات...";
        _content.Children.Clear();
        try
        {
            switch (_section)
            {
                case "Products": await LoadProductsAsync(); break;
                case "Warehouse": await LoadWarehouseAsync(); break;
                case "Reports": await LoadReportsAsync(); break;
                case "Orders": await LoadOrdersAsync(); break;
                case "Users": await LoadSimpleCollectionAsync("users", "Users.View", x => $"{x.GetProperty("FullName").GetString()} · {x.GetProperty("Username").GetString()}"); break;
                case "Permissions": await LoadSimpleCollectionAsync("permissions", "Permissions.Manage", x => $"{x.GetProperty("Name").GetString()} · {x.GetProperty("Description").GetString()}"); break;
                case "Settings": LoadSettings(); break;
            }
            _message.Text = string.Empty;
        }
        catch (Exception ex) { _message.Text = "تعذر تحميل البيانات. تحقق من اتصال Firebase والصلاحيات."; }
    }

    private async Task LoadProductsAsync()
    {
        if (!Has("Products.View")) { _message.Text = "ليس لديك صلاحية عرض المنتجات."; return; }
        var docs = await _firestore.GetCollectionAsync<ProductRecord>("products");
        var search = new SearchBar { Placeholder = "ابحث عن منتج", BackgroundColor = Colors.White };
        var list = new VerticalStackLayout { Spacing = 10 };
        void Filter() { list.Children.Clear(); foreach (var doc in docs.Where(d => string.IsNullOrWhiteSpace(search.Text) || d.Data.Name.Contains(search.Text, StringComparison.CurrentCultureIgnoreCase))) list.Children.Add(Card(doc.Data.Name, doc.Data.Description ?? "", doc.Data.IsActive ? "نشط" : "غير نشط")); }
        search.TextChanged += (_, _) => Filter();
        _content.Children.Add(search); _content.Children.Add(list); Filter();
        if (Has("Products.Create")) AddAction("إضافة منتج", async () => { var name = await DisplayPromptAsync("منتج جديد", "اسم المنتج"); if (string.IsNullOrWhiteSpace(name)) return; var description = await DisplayPromptAsync("منتج جديد", "الوصف (اختياري)"); var now = DateTime.UtcNow; await _firestore.WriteDocumentAsync("products", Guid.NewGuid().ToString("D"), new ProductRecord { Name = name.Trim(), Description = description, IsActive = true, CreatedAt = now, UpdatedAt = now }); await LoadSectionAsync(); });
        var variants = await _firestore.GetCollectionAsync<VariantRecord>("productVariants");
        _content.Children.Add(SectionTitle("الأصناف"));
        foreach (var variant in variants) _content.Children.Add(Card(variant.Data.Name, $"{variant.Data.Size} · {variant.Data.Color} · {variant.Data.Material}", variant.Data.IsActive ? "نشط" : "غير نشط"));
    }

    private async Task LoadWarehouseAsync()
    {
        if (!Has("Stock.View")) { _message.Text = "ليس لديك صلاحية عرض المخزون."; return; }
        var variants = await _firestore.GetCollectionAsync<VariantRecord>("productVariants");
        var txs = await _firestore.GetCollectionAsync<TransactionRecord>("stockTransactions");
        foreach (var v in variants)
        {
            var id = v.SyncId;
            var relevant = txs.Where(t => t.References.TryGetValue("productVariant", out var refId) && refId == id).ToList();
            var incoming = relevant.Where(t => t.Data.Type == 1).Sum(t => t.Data.Quantity);
            var outgoing = relevant.Where(t => t.Data.Type == 2).Sum(t => t.Data.Quantity);
            var card = new VerticalStackLayout { Spacing = 6 };
            card.Children.Add(new Label { Text = v.Data.Name, FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1E293B") });
            card.Children.Add(new Label { Text = $"الوارد {incoming:N0}   ·   الصادر {outgoing:N0}   ·   الرصيد {incoming - outgoing:N0}", TextColor = Color.FromArgb("#64748B") });
            if (Has("Stock.In") || Has("Stock.Out"))
            {
                var actions = new HorizontalStackLayout { Spacing = 8 };
                if (Has("Stock.In")) actions.Children.Add(ActionButton("إدخال", () => CreateTransactionAsync(v, 1)));
                if (Has("Stock.Out")) actions.Children.Add(ActionButton("إخراج", () => CreateTransactionAsync(v, 2)));
                card.Children.Add(actions);
            }
            _content.Children.Add(Panel(card));
        }
        _content.Children.Add(SectionTitle("آخر الحركات"));
        foreach (var tx in txs.OrderByDescending(t => t.Data.CreatedAt).Take(50)) _content.Children.Add(Card(tx.Data.Type == 1 ? "وارد" : "صادر", $"كمية {tx.Data.Quantity:N0} · {tx.Data.Notes}", tx.Data.CreatedAt.ToLocalTime().ToString("g")));
    }

    private async Task CreateTransactionAsync(FirestoreDataDocument<VariantRecord> variant, int type)
    {
        var raw = await DisplayPromptAsync(type == 1 ? "إدخال مخزون" : "إخراج مخزون", "الكمية", keyboard: Keyboard.Numeric);
        if (!decimal.TryParse(raw, out var quantity) || quantity <= 0) return;
        var notes = await DisplayPromptAsync("ملاحظات", "ملاحظات الحركة (اختياري)");
        var now = DateTime.UtcNow;
        var userSyncId = await FindUserSyncIdAsync();
        await _firestore.WriteDocumentAsync("stockTransactions", Guid.NewGuid().ToString("D"), new TransactionRecord { Type = type, Quantity = quantity, Notes = notes, CreatedAt = now, UpdatedAt = now }, new Dictionary<string, string> { ["productVariant"] = variant.SyncId, ["user"] = userSyncId });
        await LoadSectionAsync();
    }

    private async Task LoadReportsAsync()
    {
        if (!Has("Reports.View")) { _message.Text = "ليس لديك صلاحية عرض التقارير."; return; }
        var variants = await _firestore.GetCollectionAsync<VariantRecord>("productVariants");
        var txs = await _firestore.GetCollectionAsync<TransactionRecord>("stockTransactions");
        decimal totalIn = 0, totalOut = 0;
        foreach (var v in variants)
        {
            var matches = txs.Where(t => t.References.TryGetValue("productVariant", out var id) && id == v.SyncId).ToList();
            var ins = matches.Where(t => t.Data.Type == 1).Sum(t => t.Data.Quantity); var outs = matches.Where(t => t.Data.Type == 2).Sum(t => t.Data.Quantity); totalIn += ins; totalOut += outs;
            _content.Children.Add(Card(v.Data.Name, $"وارد {ins:N0} · صادر {outs:N0}", $"الرصيد {ins - outs:N0}"));
        }
        _content.Children.Insert(0, Card("ملخص المخزون", $"إجمالي الوارد {totalIn:N0} · إجمالي الصادر {totalOut:N0}", $"الرصيد الحالي {totalIn - totalOut:N0}"));
    }

    private async Task LoadOrdersAsync()
    {
        if (!Has("OrderRequests.View") && _user.Role is not ("Admin" or "Secretary")) { _message.Text = "ليس لديك صلاحية عرض طلبات الحجز."; return; }
        var orders = await _firestore.GetCollectionAsync<OrderRecord>("orderRequests");
        foreach (var order in orders.OrderByDescending(x => x.Data.RequestedAt)) _content.Children.Add(Card(order.Data.CustomerName, order.Data.CustomerPhone ?? "", $"{order.Data.Status} · {order.Data.RequestedAt.ToLocalTime():g}"));
        if (_user.Role is "Admin" or "Secretary") AddAction("إنشاء طلب حجز", CreateOrderAsync);
    }

    private async Task CreateOrderAsync()
    {
        var customer = await DisplayPromptAsync("طلب حجز", "اسم العميل"); if (string.IsNullOrWhiteSpace(customer)) return;
        var phone = await DisplayPromptAsync("طلب حجز", "رقم الهاتف");
        var variants = await _firestore.GetCollectionAsync<VariantRecord>("productVariants"); if (variants.Count == 0) { await DisplayAlert("تنبيه", "لا توجد أصناف متاحة.", "حسناً"); return; }
        var choice = await DisplayActionSheet("اختر الصنف", "إلغاء", null, variants.Select(v => v.Data.Name).ToArray()); var variant = variants.FirstOrDefault(v => v.Data.Name == choice); if (variant is null) return;
        var raw = await DisplayPromptAsync("طلب حجز", "الكمية", keyboard: Keyboard.Numeric); if (!decimal.TryParse(raw, out var quantity) || quantity <= 0) return;
        var now = DateTime.UtcNow; var orderId = Guid.NewGuid().ToString("D");
        var userSyncId = await FindUserSyncIdAsync();
        await _firestore.WriteDocumentAsync("orderRequests", orderId, new OrderRecord { CustomerName = customer.Trim(), CustomerPhone = phone, RequestedAt = now, UpdatedAt = now, Status = 1 }, new Dictionary<string, string> { ["user"] = userSyncId });
        await _firestore.WriteDocumentAsync("orderRequestItems", Guid.NewGuid().ToString("D"), new OrderItemRecord { Quantity = quantity, UpdatedAt = now }, new Dictionary<string, string> { ["orderRequest"] = orderId, ["productVariant"] = variant.SyncId });
        await LoadSectionAsync();
    }

    private async Task LoadSimpleCollectionAsync<T>(string collection, string permission, Func<JsonElement, string> title) where T : class
    {
        if (!Has(permission)) { _message.Text = "ليس لديك صلاحية الوصول إلى هذا القسم."; return; }
        var docs = await _firestore.GetCollectionAsync<JsonElement>(collection);
        foreach (var doc in docs) _content.Children.Add(Card(title(doc.Data), "", doc.References.TryGetValue("", out var value) ? value : ""));
        if (docs.Count == 0) _message.Text = "لا توجد بيانات حالياً.";
    }

    private void LoadSettings()
    {
        _content.Children.Add(Card("الحساب", string.IsNullOrWhiteSpace(_user.FullName) ? _user.Username : _user.FullName, _user.Role));
        _content.Children.Add(SectionTitle("الصلاحيات الفعالة"));
        foreach (var p in _user.Permissions.OrderBy(x => x)) _content.Children.Add(Card(p, "", "مسموح"));
        AddAction("تسجيل الخروج", async () => { await DisplayAlert("تسجيل الخروج", "استخدم زر تسجيل الخروج في لوحة التحكم.", "حسناً"); });
    }

    private async Task<string> FindUserSyncIdAsync()
    {
        var users = await _firestore.GetCollectionAsync<JsonElement>("users");
        var user = users.FirstOrDefault(x => x.Data.TryGetProperty("Username", out var username) && username.GetString() == _user.Username);
        return user?.SyncId ?? throw new InvalidOperationException("Your Firestore user record could not be found.");
    }

    private async Task LoadSimpleCollectionAsync(string collection, string permission, Func<JsonElement, string> title)
    {
        if (!Has(permission)) { _message.Text = "ليس لديك صلاحية الوصول إلى هذا القسم."; return; }
        var docs = await _firestore.GetCollectionAsync<JsonElement>(collection);
        foreach (var doc in docs) _content.Children.Add(Card(title(doc.Data), "", ""));
        if (docs.Count == 0) _message.Text = "لا توجد بيانات حالياً.";
    }

    private void AddAction(string text, Func<Task> action) { var b = ActionButton(text, action); b.BackgroundColor = Color.FromArgb("#0D9488"); b.TextColor = Colors.White; _content.Children.Insert(0, b); }
    private static Button ActionButton(string text, Func<Task> action) { var b = new Button { Text = text, CornerRadius = 9, Padding = new Thickness(14, 8), BackgroundColor = Color.FromArgb("#E8F6F3"), TextColor = Color.FromArgb("#0F766E") }; b.Clicked += async (_, _) => { try { await action(); } catch { await Application.Current!.Windows[0].Page!.DisplayAlert("تعذر الحفظ", "تحقق من اتصال Firebase وصلاحيات حسابك.", "حسناً"); } }; return b; }
    private static Label SectionTitle(string text) => new() { Text = text, FontSize = 19, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1E293B"), Margin = new Thickness(2, 10, 2, 2) };
    private static Border Card(string title, string detail, string badge) { var stack = new VerticalStackLayout { Spacing = 5 }; stack.Children.Add(new Label { Text = title, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1E293B") }); if (!string.IsNullOrWhiteSpace(detail)) stack.Children.Add(new Label { Text = detail, FontSize = 13, TextColor = Color.FromArgb("#64748B") }); if (!string.IsNullOrWhiteSpace(badge)) stack.Children.Add(new Label { Text = badge, FontSize = 12, TextColor = Color.FromArgb("#0F766E") }); return Panel(stack); }
    private static Border Panel(View content) => new() { Content = content, Padding = 15, Margin = new Thickness(0, 0, 0, 2), Stroke = Color.FromArgb("#E2E8F0"), StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 }, BackgroundColor = Colors.White };

    private sealed class ProductRecord { public string SyncId { get; set; } = ""; public string Name { get; set; } = ""; public string? Description { get; set; } public bool IsActive { get; set; } = true; public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } }
    private sealed class VariantRecord { public string SyncId { get; set; } = ""; public string Name { get; set; } = ""; public string? Size { get; set; } public string? Color { get; set; } public string? Material { get; set; } public bool IsActive { get; set; } = true; }
    private sealed class TransactionRecord { public int Type { get; set; } public decimal Quantity { get; set; } public string? Notes { get; set; } public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } }
    private sealed class OrderRecord { public string CustomerName { get; set; } = ""; public string? CustomerPhone { get; set; } public DateTime RequestedAt { get; set; } public DateTime UpdatedAt { get; set; } public int Status { get; set; } }
    private sealed class OrderItemRecord { public decimal Quantity { get; set; } public DateTime UpdatedAt { get; set; } }
}
