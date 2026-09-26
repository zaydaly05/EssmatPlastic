using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using EsmatPlastic.Shared.Models.Auth;
using EsmatPlastic.Shared.Services;

namespace EsmatPlastic.Mobile.Views;

/// <summary>A focused, permission-aware mobile screen backed directly by the synced Firebase collections.</summary>
public sealed class MobileWorkspacePage : ContentPage
{
    private readonly LoginResponse _user;
    private readonly Action _logout;
    private readonly FirebaseFirestoreClient _firestore = App.SharedFirestoreClient;
    private readonly VerticalStackLayout _content = new() { Spacing = 12, Padding = new Thickness(20, 16, 20, 32) };
    private readonly Label _message = new() { TextColor = Color.FromArgb("#64748B"), HorizontalTextAlignment = TextAlignment.Center };
    private string _section;

    public MobileWorkspacePage(LoginResponse user, Action logout, string initialSection = "Products")
    {
        _user = user;
        _logout = logout;
        _section = initialSection;
        Title = SectionHeading;
        BackgroundColor = Color.FromArgb("#F8FAFC");
        FlowDirection = FlowDirection.LeftToRight;
        NavigationPage.SetHasNavigationBar(this, false);

        var back = new Button { Text = "‹  Dashboard", BackgroundColor = Colors.Transparent, TextColor = Color.FromArgb("#0F766E"), Padding = new Thickness(4, 0), HorizontalOptions = LayoutOptions.Start };
        back.Clicked += async (_, _) => await Navigation.PopToRootAsync();
        var refresh = SmallButton("Refresh", async () => await LoadSectionAsync());
        var signOut = SmallButton("Sign out", () => { _logout(); return Task.CompletedTask; });
        signOut.BackgroundColor = Color.FromArgb("#FEE2E2");
        signOut.TextColor = Color.FromArgb("#B91C1C");

        var titleStack = new VerticalStackLayout { Spacing = 3, VerticalOptions = LayoutOptions.Center };
        titleStack.Children.Add(new Label { Text = SectionHeading, FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1E293B") });
        titleStack.Children.Add(new Label { Text = $"Esmat Plastic · {_user.FullName}", FontSize = 13, TextColor = Color.FromArgb("#64748B") });
        var header = new Grid { Padding = new Thickness(18, 12), ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 10, BackgroundColor = Colors.White };
        header.Add(back, 0, 0);
        header.Add(titleStack, 1, 0);
        header.Add(refresh, 2, 0);
        header.Add(signOut, 3, 0);

        var body = new VerticalStackLayout { Spacing = 12 };
        body.Children.Add(_message);
        body.Children.Add(_content);
        var refreshView = new RefreshView { Content = new ScrollView { Content = body }, Command = new Command(async () => await LoadSectionAsync()) };
        Content = new Grid { RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) }, Children = { header, refreshView } };
        Grid.SetRow((View)((Grid)Content).Children[1], 1);
    }

    private string SectionHeading => _section switch
    {
        "Products" => "Products & variants", "Warehouse" => "Warehouse", "Reports" => "Reports",
        "Orders" => "Order requests", "Users" => "Users", "Permissions" => "Permissions",
        "Settings" => "Settings", _ => "Esmat Plastic"
    };

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Cascade Entrance Animation
        Content.Opacity = 0;
        await Content.FadeTo(1, 300, Easing.CubicOut);

        await LoadSectionAsync();
    }
    private bool Has(string permission) => _user.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);

    private async Task HandleException(Exception ex)
    {
        string message = ex switch
        {
            System.Net.Http.HttpRequestException => "Network error: Please check your internet connection.",
            _ when ex.Message.Contains("PermissionDenied") => "Access denied: You do not have permission to perform this action.",
            _ when ex.Message.Contains("Unavailable") => "Firestore service is currently unavailable. Please try again later.",
            InvalidOperationException ioe => ioe.Message,
            _ => "An unexpected error occurred. Please try again or contact support."
        };

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", message, "OK");
        });
    }

    private async Task LoadSectionAsync()
    {
        _message.Text = "Loading from Firebase…";
        _content.Children.Clear();
        try
        {
            switch (_section)
            {
                case "Products": await LoadProductsAsync(); break;
                case "Warehouse": await LoadWarehouseAsync(); break;
                case "Reports": await LoadReportsAsync(); break;
                case "Orders": await LoadOrdersAsync(); break;
                case "Users": await LoadUsersAsync(); break;
                case "Permissions": await LoadPermissionsAsync(); break;
                case "Settings": await LoadSettingsAsync(); break;
            }
            if (_message.Text == "Loading from Firebase…") _message.Text = string.Empty;
        }
        catch (Exception ex)
        {
            await HandleException(ex);
            _message.Text = "Failed to load data.";
        }
        finally
        {
            foreach (var child in _content.Children)
            {
                if (child is Border panel)
                {
                    panel.TranslationY = 20;
                    panel.Opacity = 0;
                    _ = panel.FadeTo(1, 300, Easing.CubicOut);
                    _ = panel.TranslateTo(0, 0, 300, Easing.CubicOut);
                }
            }
        }
    }

    // Products and their variants
    private async Task LoadProductsAsync()
    {
        if (!Has("Products.View")) { _message.Text = "Your account cannot view products."; return; }
        var products = await _firestore.GetCollectionAsync<ProductRecord>("products");
        var variants = await _firestore.GetCollectionAsync<VariantRecord>("productVariants");
        var search = new SearchBar { Placeholder = "Search products and variants", BackgroundColor = Colors.White };
        var list = new VerticalStackLayout { Spacing = 10 };
        _content.Children.Add(search);
        _content.Children.Add(list);

        void Render()
        {
            list.Children.Clear();
            var query = search.Text?.Trim() ?? string.Empty;
            foreach (var product in products.Where(x => string.IsNullOrWhiteSpace(query) || x.Data.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) || (x.Data.Description?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false)))
            {
                var stack = new VerticalStackLayout { Spacing = 7 };
                stack.Children.Add(new Label { Text = product.Data.Name, FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Ink });
                stack.Children.Add(new Label { Text = string.IsNullOrWhiteSpace(product.Data.Description) ? "No description" : product.Data.Description, FontSize = 13, TextColor = Muted });
                stack.Children.Add(Badge(product.Data.IsActive ? "Active" : "Inactive", product.Data.IsActive ? "#D1FAE5" : "#F1F5F9"));
                if (Has("Products.Edit") || Has("Products.Delete"))
                {
                    var actions = new HorizontalStackLayout { Spacing = 8 };
                    if (Has("Products.Edit")) actions.Children.Add(SmallButton("Edit", async () => await EditProductAsync(product)));
                    if (Has("Products.Delete")) actions.Children.Add(SmallButton("Delete", async () => await DeleteProductAsync(product)));
                    stack.Children.Add(actions);
                }

                var productVariants = variants.Where(x => x.References.TryGetValue("product", out var id) && id == product.SyncId).ToList();
                if (productVariants.Count > 0)
                {
                    stack.Children.Add(SectionTitle($"Variants · {productVariants.Count}"));
                    foreach (var variant in productVariants.Where(x => string.IsNullOrWhiteSpace(query) || x.Data.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase)))
                        stack.Children.Add(VariantRow(product, variant));
                }
                if (Has("Products.Create")) stack.Children.Add(SmallButton("＋ Add variant", async () => await CreateVariantAsync(product)));
                list.Children.Add(Panel(stack));
            }
            if (Has("Products.Create")) list.Children.Insert(0, PrimaryButton("＋ Add product", async () => await CreateProductAsync()));
            if (list.Children.Count == 0) list.Children.Add(EmptyState("No matching products."));
        }

        search.TextChanged += (_, _) => Render();
        Render();
    }

    private View VariantRow(FirestoreDataDocument<ProductRecord> product, FirestoreDataDocument<VariantRecord> variant)
    {
        var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 6 };
        var info = new VerticalStackLayout { Spacing = 2 };
        info.Children.Add(new Label { Text = variant.Data.Name, FontAttributes = FontAttributes.Bold, TextColor = Ink });
        info.Children.Add(new Label { Text = string.Join(" · ", new[] { variant.Data.Size, variant.Data.Color, variant.Data.Material }.Where(x => !string.IsNullOrWhiteSpace(x))), FontSize = 12, TextColor = Muted });
        row.Add(info, 0, 0);
        if (Has("Products.Edit")) row.Add(SmallButton("Edit", async () => await EditVariantAsync(variant)), 1, 0);
        if (Has("Products.Delete")) row.Add(SmallButton("Delete", async () => await DeleteVariantAsync(product, variant)), 2, 0);
        return row;
    }

    private async Task CreateProductAsync()
    {
        var name = await DisplayPromptAsync("New product", "Product name");
        if (string.IsNullOrWhiteSpace(name)) return;

        var trimmedName = name.Trim();
        var duplicate = (await _firestore.GetCollectionAsync<ProductRecord>("products")).Any(x => x.Data.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase));
        if (duplicate) { await DisplayAlert("Duplicate product", "A product with this name already exists.", "OK"); return; }

        var description = await DisplayPromptAsync("New product", "Description (optional)");
        var imagePath = await DisplayPromptAsync("New product", "Image URL/path (optional)");
        var now = DateTime.UtcNow;

        try
        {
            await _firestore.WriteDocumentAsync("products", Guid.NewGuid().ToString("D"), new ProductRecord { Name = trimmedName, Description = description, ImagePath = imagePath, IsActive = true, CreatedAt = now, UpdatedAt = now });
            await LoadSectionAsync();
        }
        catch (Exception ex) { await HandleException(ex); }
    }

    private async Task EditProductAsync(FirestoreDataDocument<ProductRecord> item)
    {
        var name = await DisplayPromptAsync("Edit product", "Product name", initialValue: item.Data.Name);
        if (string.IsNullOrWhiteSpace(name)) return;
        var description = await DisplayPromptAsync("Edit product", "Description", initialValue: item.Data.Description ?? string.Empty);
        var image = await DisplayPromptAsync("Edit product", "Image URL/path", initialValue: item.Data.ImagePath ?? string.Empty);
        var status = await DisplayActionSheet("Product status", "Cancel", null, "Active", "Inactive");
        if (status == "Cancel" || string.IsNullOrEmpty(status)) return;
        item.Data.Name = name.Trim(); item.Data.Description = description; item.Data.ImagePath = image;
        item.Data.IsActive = status == "Active"; item.Data.UpdatedAt = DateTime.UtcNow;
        await _firestore.WriteDocumentAsync("products", item.SyncId, item.Data, item.References);
        await LoadSectionAsync();
    }

    private async Task DeleteProductAsync(FirestoreDataDocument<ProductRecord> product)
    {
        var variants = await _firestore.GetCollectionAsync<VariantRecord>("productVariants");
        var productVariantIds = variants.Where(x => x.References.GetValueOrDefault("product") == product.SyncId).Select(x => x.SyncId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var transactions = await _firestore.GetCollectionAsync<TransactionRecord>("stockTransactions");
        var orderItems = await _firestore.GetCollectionAsync<OrderItemRecord>("orderRequestItems");
        if (transactions.Any(x => productVariantIds.Contains(x.References.GetValueOrDefault("productVariant") ?? string.Empty)) || orderItems.Any(x => productVariantIds.Contains(x.References.GetValueOrDefault("productVariant") ?? string.Empty)))
        { await DisplayAlert("Product in use", "This product has stock history or reservations. Deactivate it instead.", "OK"); return; }
        if (!await DisplayAlert("Delete product", $"Delete {product.Data.Name} and its variants?", "Delete", "Cancel")) return;
        await _firestore.WriteTombstoneAsync("Product", Normalize(product.Data.Name));
        await LoadSectionAsync();
    }

    private async Task CreateVariantAsync(FirestoreDataDocument<ProductRecord> product)
    {
        var name = await DisplayPromptAsync("New variant", "Variant name"); if (string.IsNullOrWhiteSpace(name)) return;
        var size = await DisplayPromptAsync("New variant", "Size (optional)");
        var color = await DisplayPromptAsync("New variant", "Color (optional)");
        var cap = await DisplayPromptAsync("New variant", "Cap type (optional)");
        var material = await DisplayPromptAsync("New variant", "Material (optional)");
        var image = await DisplayPromptAsync("New variant", "Image URL/path (optional)");
        var now = DateTime.UtcNow;
        await _firestore.WriteDocumentAsync("productVariants", Guid.NewGuid().ToString("D"), new VariantRecord { Name = name.Trim(), Size = size, Color = color, CapType = cap, Material = material, ImagePath = image, IsActive = true, CreatedAt = now, UpdatedAt = now }, new Dictionary<string, string> { ["product"] = product.SyncId });
        await LoadSectionAsync();
    }

    private async Task EditVariantAsync(FirestoreDataDocument<VariantRecord> variant)
    {
        var name = await DisplayPromptAsync("Edit variant", "Variant name", initialValue: variant.Data.Name); if (string.IsNullOrWhiteSpace(name)) return;
        var size = await DisplayPromptAsync("Edit variant", "Size", initialValue: variant.Data.Size ?? string.Empty);
        var color = await DisplayPromptAsync("Edit variant", "Color", initialValue: variant.Data.Color ?? string.Empty);
        var cap = await DisplayPromptAsync("Edit variant", "Cap type", initialValue: variant.Data.CapType ?? string.Empty);
        var material = await DisplayPromptAsync("Edit variant", "Material", initialValue: variant.Data.Material ?? string.Empty);
        var status = await DisplayActionSheet("Variant status", "Cancel", null, "Active", "Inactive"); if (status == "Cancel" || string.IsNullOrEmpty(status)) return;
        variant.Data.Name = name.Trim(); variant.Data.Size = size; variant.Data.Color = color; variant.Data.CapType = cap; variant.Data.Material = material; variant.Data.IsActive = status == "Active"; variant.Data.UpdatedAt = DateTime.UtcNow;
        await _firestore.WriteDocumentAsync("productVariants", variant.SyncId, variant.Data, variant.References);
        await LoadSectionAsync();
    }

    private async Task DeleteVariantAsync(FirestoreDataDocument<ProductRecord> product, FirestoreDataDocument<VariantRecord> variant)
    {
        if (!await DisplayAlert("Delete variant", $"Delete {variant.Data.Name}?", "Delete", "Cancel")) return;
        var transactions = await _firestore.GetCollectionAsync<TransactionRecord>("stockTransactions");
        var orders = await _firestore.GetCollectionAsync<OrderItemRecord>("orderRequestItems");
        if (transactions.Any(x => x.References.GetValueOrDefault("productVariant") == variant.SyncId) || orders.Any(x => x.References.GetValueOrDefault("productVariant") == variant.SyncId))
        { await DisplayAlert("Variant in use", "This variant has stock history or reservations. Deactivate it instead.", "OK"); return; }
        await _firestore.WriteTombstoneAsync("ProductVariant", $"{Normalize(product.Data.Name)}|{Normalize(variant.Data.Name)}");
        await LoadSectionAsync();
    }

    // Warehouse, transaction history and stock reports
    private async Task LoadWarehouseAsync()
    {
        if (!Has("Stock.View")) { _message.Text = "Your account cannot view warehouse stock."; return; }
        var variants = await _firestore.GetCollectionAsync<VariantRecord>("productVariants");
        var transactions = await _firestore.GetCollectionAsync<TransactionRecord>("stockTransactions");
        var products = await _firestore.GetCollectionAsync<ProductRecord>("products");
        var transactionsLookup = transactions.ToLookup(x => x.References.GetValueOrDefault("productVariant"));

        AddSectionHeader("Current balances", "Select a variant to record stock in or stock out.");
        foreach (var variant in variants.Where(x => x.Data.IsActive).OrderBy(x => x.Data.Name))
        {
            var related = transactionsLookup[variant.SyncId].ToList();
            var incoming = related.Where(x => x.Data.Type == 1).Sum(x => x.Data.Quantity);
            var outgoing = related.Where(x => x.Data.Type == 2).Sum(x => x.Data.Quantity);
            var product = products.FirstOrDefault(x => x.SyncId == variant.References.GetValueOrDefault("product"));
            var body = new VerticalStackLayout { Spacing = 6 };
            body.Children.Add(new Label { Text = $"{product?.Data.Name ?? "Product"} · {variant.Data.Name}", FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Ink });
            body.Children.Add(new Label { Text = $"In {incoming:N0}   ·   Out {outgoing:N0}   ·   Available {incoming - outgoing - variant.Data.ReservedQuantity:N0}   ·   Reserved {variant.Data.ReservedQuantity:N0}", FontSize = 13, TextColor = Muted });
            var actions = new HorizontalStackLayout { Spacing = 8 };
            if (Has("Stock.In")) actions.Children.Add(SmallButton("Stock in", async () => await CreateTransactionAsync(variant, 1, incoming - outgoing)));
            if (Has("Stock.Out")) actions.Children.Add(SmallButton("Stock out", async () => await CreateTransactionAsync(variant, 2, incoming - outgoing - variant.Data.ReservedQuantity)));
            if (Has("Stock.Edit")) actions.Children.Add(SmallButton("Adjust", async () => await QuickAdjustAsync(variant)));
            body.Children.Add(actions);
            _content.Children.Add(Panel(body));
        }
        AddSectionHeader("Recent transactions", "Newest first");
        var historySearch = new SearchBar { Placeholder = "Filter transaction history", BackgroundColor = Colors.White };
        var history = new VerticalStackLayout { Spacing = 8 };
        _content.Children.Add(historySearch); _content.Children.Add(history);
        void RenderHistory()
        {
            history.Children.Clear();
            var query = historySearch.Text?.Trim() ?? string.Empty;
            foreach (var tx in transactions.OrderByDescending(x => x.Data.CreatedAt).Where(x =>
                         string.IsNullOrWhiteSpace(query) || x.Data.Notes?.Contains(query, StringComparison.CurrentCultureIgnoreCase) == true ||
                         x.References.GetValueOrDefault("productVariant") == variants.FirstOrDefault(v => v.Data.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase))?.SyncId).Take(100))
                history.Children.Add(Card(tx.Data.Type == 1 ? "Stock in" : "Stock out", $"{variants.FirstOrDefault(x => x.SyncId == tx.References.GetValueOrDefault("productVariant"))?.Data.Name} · {tx.Data.Quantity:N0}", $"{tx.Data.CreatedAt.ToLocalTime():g} · {tx.Data.Notes}"));
            if (history.Children.Count == 0) history.Children.Add(EmptyState("No transactions match this filter."));
        }
        historySearch.TextChanged += (_, _) => RenderHistory(); RenderHistory();
    }

    private async Task CreateTransactionAsync(FirestoreDataDocument<VariantRecord> variant, int type, decimal available)
    {
        var raw = await DisplayPromptAsync(type == 1 ? "Stock in" : "Stock out", $"Quantity{(type == 2 ? $" (available: {available:N0})" : string.Empty)}", keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(raw)) return;

        if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.CurrentCulture, out var quantity) || quantity <= 0)
        {
            await DisplayAlert("Invalid quantity", "Please enter a positive number.", "OK");
            return;
        }

        if (type == 2 && quantity > available) { await DisplayAlert("Insufficient stock", $"Only {available:N0} units are available after reservations.", "OK"); return; }
        var notes = await DisplayPromptAsync("Transaction", "Notes (optional)");
        var now = DateTime.UtcNow;

        try
        {
            await _firestore.WriteDocumentAsync("stockTransactions", Guid.NewGuid().ToString("D"), new TransactionRecord { Type = type, Quantity = quantity, Notes = notes, CreatedAt = now, UpdatedAt = now }, new Dictionary<string, string> { ["productVariant"] = variant.SyncId, ["user"] = await FindUserSyncIdAsync() });
            await LoadSectionAsync();
        }
        catch (Exception ex) { await HandleException(ex); }
    }

    private async Task QuickAdjustAsync(FirestoreDataDocument<VariantRecord> variant)
    {
        var type = await DisplayActionSheet("Adjustment type", "Cancel", null, "Stock In", "Stock Out");
        if (string.IsNullOrEmpty(type) || type == "Cancel") return;
        var txType = type == "Stock In" ? 1 : 2;
        await CreateTransactionAsync(variant, txType, 0); // Available check handled inside CreateTransactionAsync for Stock Out
    }

    private async Task LoadReportsAsync()
    {
        if (!Has("Reports.View")) { _message.Text = "Your account cannot view reports."; return; }
        var variants = await _firestore.GetCollectionAsync<VariantRecord>("productVariants");
        var transactions = await _firestore.GetCollectionAsync<TransactionRecord>("stockTransactions");
        var products = await _firestore.GetCollectionAsync<ProductRecord>("products");
        var transactionsLookup = transactions.ToLookup(t => t.References.GetValueOrDefault("productVariant"));
        var query = new SearchBar { Placeholder = "Search stock report", BackgroundColor = Colors.White };
        var summary = new Label { FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Ink };
        var list = new VerticalStackLayout { Spacing = 8 };
        _content.Children.Add(summary); _content.Children.Add(query); _content.Children.Add(list);
        var report = variants.Select(v =>
        {
            var rows = transactionsLookup[v.SyncId].ToList();
            return new { Variant = v, Product = products.FirstOrDefault(p => p.SyncId == v.References.GetValueOrDefault("product"))?.Data.Name ?? "", In = rows.Where(t => t.Data.Type == 1).Sum(t => t.Data.Quantity), Out = rows.Where(t => t.Data.Type == 2).Sum(t => t.Data.Quantity) };
        }).ToList();
        var totalIn = report.Sum(x => x.In); var totalOut = report.Sum(x => x.Out);
        summary.Text = $"Inbound  {totalIn:N0}       Outbound  {totalOut:N0}       Current  {totalIn - totalOut:N0}";
        var export = PrimaryButton("Export / share CSV", async () =>
        {
            var rows = report.Select(x => string.Join(",", Csv(x.Product), Csv(x.Variant.Data.Name), x.In.ToString(CultureInfo.InvariantCulture), x.Out.ToString(CultureInfo.InvariantCulture), (x.In - x.Out).ToString(CultureInfo.InvariantCulture)));
            var csv = "Product,Variant,Inbound,Outbound,Current\n" + string.Join("\n", rows);
            await Share.Default.RequestAsync(new ShareTextRequest { Title = "Stock report", Text = csv });
        });
        _content.Children.Add(export);
        void Render()
        {
            list.Children.Clear(); var search = query.Text?.Trim() ?? "";
            foreach (var x in report.Where(x => string.IsNullOrWhiteSpace(search) || x.Product.Contains(search, StringComparison.CurrentCultureIgnoreCase) || x.Variant.Data.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase)))
                list.Children.Add(Card($"{x.Product} · {x.Variant.Data.Name}", $"Inbound {x.In:N0} · Outbound {x.Out:N0}", $"Current {x.In - x.Out:N0}"));
        }
        query.TextChanged += (_, _) => Render(); Render();
    }

    // Order reservation workflows
    private async Task LoadOrdersAsync()
    {
        if (!CanManageOrders) { _message.Text = "Order requests are available to Admin and Secretary accounts."; return; }
        var orders = await _firestore.GetCollectionAsync<OrderRecord>("orderRequests");
        var items = await _firestore.GetCollectionAsync<OrderItemRecord>("orderRequestItems");
        var variants = await _firestore.GetCollectionAsync<VariantRecord>("productVariants");
        AddSectionHeader("Reservations", "Pending orders reserve variant quantities.");
        AddAction("＋ New order request", CreateOrderAsync);
        foreach (var order in orders.OrderByDescending(x => x.Data.RequestedAt))
        {
            var orderItems = items.Where(x => x.References.GetValueOrDefault("orderRequest") == order.SyncId).ToList();
            var lines = orderItems.Select(x => $"{variants.FirstOrDefault(v => v.SyncId == x.References.GetValueOrDefault("productVariant"))?.Data.Name ?? "Variant"} × {x.Data.Quantity:N0}");
            var panel = new VerticalStackLayout { Spacing = 6 };
            panel.Children.Add(new Label { Text = order.Data.CustomerName, FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Ink });
            panel.Children.Add(new Label { Text = $"{order.Data.CustomerPhone} · {order.Data.RequestedAt.ToLocalTime():g}", FontSize = 13, TextColor = Muted });
            panel.Children.Add(new Label { Text = $"{string.Join("\n", lines)}\nStatus: {OrderStatusName(order.Data.Status)}", TextColor = Muted });
            var buttons = new HorizontalStackLayout { Spacing = 7 };
            if (_user.Role == "Admin" && order.Data.Status != 5)
                buttons.Children.Add(SmallButton("Change status", async () => await ChangeOrderStatusAsync(order, orderItems, variants)));
            if (order.Data.Status == 1)
                buttons.Children.Add(SmallButton("Cancel request", async () => await CancelOrderAsync(order, orderItems, variants)));
            if (buttons.Children.Count > 0) panel.Children.Add(buttons);
            _content.Children.Add(Panel(panel));
        }
        if (orders.Count == 0) _content.Children.Add(EmptyState("No order requests yet."));
    }

    private async Task CreateOrderAsync()
    {
        var customer = await DisplayPromptAsync("New order request", "Customer name"); if (string.IsNullOrWhiteSpace(customer)) return;
        var phone = await DisplayPromptAsync("New order request", "Customer phone (optional)");
        var variants = (await _firestore.GetCollectionAsync<VariantRecord>("productVariants")).Where(x => x.Data.IsActive).ToList();
        if (variants.Count == 0) { await DisplayAlert("No variants", "Create an active product variant first.", "OK"); return; }
        var selected = new List<(FirestoreDataDocument<VariantRecord> Variant, decimal Quantity)>();
        while (true)
        {
            var labels = variants.Select(v => v.Data.Name).ToArray();
            var choice = await DisplayActionSheet("Choose a variant", "Done", null, labels);
            if (choice == "Done" || string.IsNullOrEmpty(choice)) break;
            var variant = variants.FirstOrDefault(v => v.Data.Name == choice); if (variant is null) continue;
            var raw = await DisplayPromptAsync("Quantity", $"Quantity for {variant.Data.Name}", keyboard: Keyboard.Numeric);
            if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.CurrentCulture, out var qty) || qty <= 0) continue;
            selected.Add((variant, qty));
            var addMore = await DisplayAlert("Order items", $"Added {variant.Data.Name} × {qty:N0}. Add another item?", "Add item", "Finish");
            if (!addMore) break;
        }
        if (selected.Count == 0) return;
        var now = DateTime.UtcNow; var orderId = Guid.NewGuid().ToString("D");
        await _firestore.WriteDocumentAsync("orderRequests", orderId, new OrderRecord { CustomerName = customer.Trim(), CustomerPhone = phone, RequestedAt = now, UpdatedAt = now, Status = 1 }, new Dictionary<string, string> { ["user"] = await FindUserSyncIdAsync() });
        foreach (var item in selected)
        {
            item.Variant.Data.ReservedQuantity += item.Quantity; item.Variant.Data.UpdatedAt = now;
            await _firestore.WriteDocumentAsync("productVariants", item.Variant.SyncId, item.Variant.Data, item.Variant.References);
            await _firestore.WriteDocumentAsync("orderRequestItems", Guid.NewGuid().ToString("D"), new OrderItemRecord { Quantity = item.Quantity, UpdatedAt = now }, new Dictionary<string, string> { ["orderRequest"] = orderId, ["productVariant"] = item.Variant.SyncId });
        }
        await LoadSectionAsync();
    }

    private async Task CancelOrderAsync(FirestoreDataDocument<OrderRecord> order, IReadOnlyList<FirestoreDataDocument<OrderItemRecord>> items, IReadOnlyList<FirestoreDataDocument<VariantRecord>> variants)
    {
        if (!await DisplayAlert("Cancel order", $"Cancel reservation for {order.Data.CustomerName}?", "Cancel order", "Keep")) return;
        foreach (var item in items)
        {
            var variant = variants.FirstOrDefault(x => x.SyncId == item.References.GetValueOrDefault("productVariant")); if (variant is null) continue;
            variant.Data.ReservedQuantity = Math.Max(0, variant.Data.ReservedQuantity - item.Data.Quantity); variant.Data.UpdatedAt = DateTime.UtcNow;
            await _firestore.WriteDocumentAsync("productVariants", variant.SyncId, variant.Data, variant.References);
        }
        order.Data.Status = 5; order.Data.UpdatedAt = DateTime.UtcNow;
        await _firestore.WriteDocumentAsync("orderRequests", order.SyncId, order.Data, order.References);
        await LoadSectionAsync();
    }

    private async Task ChangeOrderStatusAsync(FirestoreDataDocument<OrderRecord> order, IReadOnlyList<FirestoreDataDocument<OrderItemRecord>> items, IReadOnlyList<FirestoreDataDocument<VariantRecord>> variants)
    {
        var status = await DisplayActionSheet("Update order status", "Cancel", null, "Approved", "Rejected", "Completed", "Cancelled");
        if (status == "Cancel" || string.IsNullOrEmpty(status)) return;
        var next = status switch { "Approved" => 2, "Rejected" => 3, "Completed" => 4, _ => 5 };
        if (next == 5 && order.Data.Status == 1)
        {
            foreach (var item in items)
            {
                var variant = variants.FirstOrDefault(x => x.SyncId == item.References.GetValueOrDefault("productVariant")); if (variant is null) continue;
                variant.Data.ReservedQuantity = Math.Max(0, variant.Data.ReservedQuantity - item.Data.Quantity); variant.Data.UpdatedAt = DateTime.UtcNow;
                await _firestore.WriteDocumentAsync("productVariants", variant.SyncId, variant.Data, variant.References);
            }
        }
        order.Data.Status = next; order.Data.UpdatedAt = DateTime.UtcNow;
        await _firestore.WriteDocumentAsync("orderRequests", order.SyncId, order.Data, order.References);
        await LoadSectionAsync();
    }

    // Users, roles, access assignments and password management
    private async Task LoadUsersAsync()
    {
        if (!Has("Users.View")) { _message.Text = "Your account cannot view users."; return; }
        var users = await _firestore.GetCollectionAsync<UserRecord>("users");
        var search = new SearchBar { Placeholder = "Search name, username or role", BackgroundColor = Colors.White };
        var list = new VerticalStackLayout { Spacing = 10 };
        _content.Add(search); _content.Add(list);
        if (Has("Users.Create")) _content.Children.Insert(0, PrimaryButton("＋ Add user", async () => await CreateUserAsync()));
        void Render()
        {
            list.Children.Clear(); var query = search.Text?.Trim() ?? "";
            foreach (var user in users.Where(x => string.IsNullOrWhiteSpace(query) || x.Data.FullName.Contains(query, StringComparison.CurrentCultureIgnoreCase) || x.Data.Username.Contains(query, StringComparison.CurrentCultureIgnoreCase) || RoleName(x.Data.Role).Contains(query, StringComparison.CurrentCultureIgnoreCase)))
            {
                var panel = new VerticalStackLayout { Spacing = 6 };
                panel.Children.Add(new Label { Text = user.Data.FullName, FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Ink });
                panel.Children.Add(new Label { Text = $"@{user.Data.Username} · {RoleName(user.Data.Role)} · {(user.Data.IsActive ? "Active" : "Inactive")}", FontSize = 13, TextColor = Muted });
                var buttons = new HorizontalStackLayout { Spacing = 7 };
                if (Has("Users.Edit"))
                {
                    buttons.Children.Add(SmallButton("Edit profile", async () => await EditUserAsync(user)));
                    buttons.Children.Add(SmallButton("Access", async () => await EditUserPermissionsAsync(user)));
                    buttons.Children.Add(SmallButton("Change password", async () => await ChangePasswordAsync(user)));
                }
                if (Has("Users.Delete")) buttons.Children.Add(SmallButton("Delete", async () => await DeleteUserAsync(user)));
                panel.Children.Add(buttons); list.Children.Add(Panel(panel));
            }
            if (list.Children.Count == 0) list.Children.Add(EmptyState("No matching users."));
        }
        search.TextChanged += (_, _) => Render(); Render();
    }

    private async Task CreateUserAsync()
    {
        var username = await DisplayPromptAsync("New user", "Username"); if (string.IsNullOrWhiteSpace(username)) return;
        if ((await _firestore.GetCollectionAsync<UserRecord>("users")).Any(x => x.Data.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase)))
        { await DisplayAlert("Duplicate username", "That username already exists.", "OK"); return; }
        var fullName = await DisplayPromptAsync("New user", "Full name"); if (string.IsNullOrWhiteSpace(fullName)) return;
        var password = await PromptPasswordAsync("New user", "Initial password (minimum 6 characters)");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6) { await DisplayAlert("Invalid password", "Use at least 6 characters.", "OK"); return; }
        var role = await ChooseRoleAsync(2); if (role is null) return;
        var permissions = await _firestore.GetCollectionAsync<PermissionRecord>("permissions");
        var id = Guid.NewGuid().ToString("D"); var now = DateTime.UtcNow;
        // Credential hashes are written to the server-only collection; raw passwords never enter Firestore.
        await _firestore.WriteUserCredentialAsync(id, HashIdentityPassword(password));
        await _firestore.WriteDocumentAsync("users", id, new UserRecord { Username = username.Trim(), FullName = fullName.Trim(), Role = role.Value, IsActive = true, CreatedAt = now, UpdatedAt = now });
        foreach (var permission in DefaultPermissions(role.Value, permissions))
            await _firestore.WriteDocumentAsync("userPermissions", Guid.NewGuid().ToString("D"), new UserPermissionRecord { UpdatedAt = now }, new Dictionary<string, string> { ["user"] = id, ["permission"] = permission.SyncId });
        await LoadSectionAsync();
    }

    private async Task EditUserAsync(FirestoreDataDocument<UserRecord> user)
    {
        var name = await DisplayPromptAsync("Edit user", "Full name", initialValue: user.Data.FullName); if (string.IsNullOrWhiteSpace(name)) return;
        var role = await ChooseRoleAsync(user.Data.Role); if (role is null) return;
        var active = await DisplayActionSheet("Account status", "Cancel", null, "Active", "Inactive"); if (active == "Cancel" || string.IsNullOrEmpty(active)) return;
        user.Data.FullName = name.Trim(); user.Data.Role = role.Value; user.Data.IsActive = active == "Active"; user.Data.UpdatedAt = DateTime.UtcNow;
        await _firestore.WriteDocumentAsync("users", user.SyncId, user.Data, user.References);
        await LoadSectionAsync();
    }

    private async Task EditUserPermissionsAsync(FirestoreDataDocument<UserRecord> user)
    {
        var all = await _firestore.GetCollectionAsync<PermissionRecord>("permissions");
        var assignments = await _firestore.GetCollectionAsync<UserPermissionRecord>("userPermissions");
        var current = assignments.Where(x => x.References.GetValueOrDefault("user") == user.SyncId).ToList();
        var grantedIds = current.Select(x => x.References.GetValueOrDefault("permission")).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var permission in all.OrderBy(x => x.Data.Name))
        {
            var isGranted = grantedIds.Contains(permission.SyncId);
            var choice = await DisplayActionSheet($"{permission.Data.Name} · {(isGranted ? "Granted" : "Not granted")}", "Finish", null, isGranted ? "Revoke" : "Grant");
            if (choice == "Finish" || string.IsNullOrEmpty(choice)) break;
            if (!isGranted && choice == "Grant")
                await _firestore.WriteDocumentAsync("userPermissions", Guid.NewGuid().ToString("D"), new UserPermissionRecord { UpdatedAt = DateTime.UtcNow }, new Dictionary<string, string> { ["user"] = user.SyncId, ["permission"] = permission.SyncId });
            else if (isGranted && choice == "Revoke")
            {
                foreach (var assignment in current.Where(x => x.References.GetValueOrDefault("permission") == permission.SyncId))
                    await _firestore.DeleteDocumentAsync("userPermissions", assignment.SyncId);
                grantedIds.Remove(permission.SyncId);
            }
        }
        await LoadSectionAsync();
    }

    private async Task ChangePasswordAsync(FirestoreDataDocument<UserRecord> user)
    {
        var password = await PromptPasswordAsync("Change password", $"New password for {user.Data.Username}");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6) { await DisplayAlert("Invalid password", "Use at least 6 characters.", "OK"); return; }
        await _firestore.WriteUserCredentialAsync(user.SyncId, HashIdentityPassword(password));
        await DisplayAlert("Password updated", "The new password hash has been saved securely.", "OK");
    }

    private async Task DeleteUserAsync(FirestoreDataDocument<UserRecord> user)
    {
        var transactions = await _firestore.GetCollectionAsync<TransactionRecord>("stockTransactions");
        if (transactions.Any(x => x.References.GetValueOrDefault("user") == user.SyncId))
        { await DisplayAlert("User has activity", "This account has stock transactions. Deactivate it instead.", "OK"); return; }
        if (!await DisplayAlert("Delete user", $"Delete {user.Data.FullName}?", "Delete", "Cancel")) return;
        await _firestore.WriteTombstoneAsync("User", Normalize(user.Data.Username));
        await LoadSectionAsync();
    }

    private async Task LoadPermissionsAsync()
    {
        if (!Has("Permissions.Manage")) { _message.Text = "Your account cannot view permission definitions."; return; }
        var permissions = await _firestore.GetCollectionAsync<PermissionRecord>("permissions");
        var users = await _firestore.GetCollectionAsync<UserPermissionRecord>("userPermissions");
        var search = new SearchBar { Placeholder = "Filter permissions", BackgroundColor = Colors.White };
        var list = new VerticalStackLayout { Spacing = 8 };
        _content.Add(Card("System access", $"{permissions.Count(x => x.Data.IsActive)} active permissions", $"Assigned {users.Count} times"));
        _content.Add(search); _content.Add(list);
        void Render()
        {
            list.Children.Clear(); var query = search.Text?.Trim() ?? "";
            foreach (var permission in permissions.Where(x => string.IsNullOrWhiteSpace(query) || x.Data.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) || (x.Data.Description?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false)))
                list.Children.Add(Card(permission.Data.Name, permission.Data.Description ?? "", permission.Data.IsActive ? "Active" : "Inactive"));
        }
        search.TextChanged += (_, _) => Render(); Render();
    }

    // Account settings and locally applied appearance options
    private async Task LoadSettingsAsync()
    {
        AddSectionHeader("Account", "Signed in as");
        _content.Add(Card(_user.FullName, $"@{_user.Username}", _user.Role));
        _content.Add(SectionTitle("Permissions in this session"));
        foreach (var permission in _user.Permissions.OrderBy(x => x)) _content.Add(Badge(permission, "#E8F6F3"));
        _content.Add(SectionTitle("Appearance"));
        _content.Add(SmallButton("Use light theme", async () => { Application.Current!.UserAppTheme = AppTheme.Light; Preferences.Set("appearance", "light"); await DisplayAlert("Appearance saved", "Light theme enabled.", "OK"); }));
        _content.Add(SmallButton("Use dark theme", async () => { Application.Current!.UserAppTheme = AppTheme.Dark; Preferences.Set("appearance", "dark"); await DisplayAlert("Appearance saved", "Dark theme enabled.", "OK"); }));
        _content.Add(SmallButton("Use device theme", async () => { Application.Current!.UserAppTheme = AppTheme.Unspecified; Preferences.Remove("appearance"); await DisplayAlert("Appearance saved", "Device theme enabled.", "OK"); }));
        _content.Add(PrimaryButton("Change my password", async () => {
            try {
                var userDoc = await CurrentUserDocumentAsync();
                await ChangePasswordAsync(userDoc);
            } catch (Exception ex) {
                await HandleException(ex);
            }
        }));
        _content.Add(SmallButton("Sign out", () => { _logout(); return Task.CompletedTask; }));
    }

    private async Task<FirestoreDataDocument<UserRecord>> CurrentUserDocumentAsync()
    {
        var userId = _firestore.AuthenticatedUserId;
        if (string.IsNullOrWhiteSpace(userId)) throw new InvalidOperationException("The Firebase session has expired.");
        return await _firestore.GetDocumentAsync<UserRecord>("users", userId);
    }

    private async Task<int?> ChooseRoleAsync(int current)
    {
        var choices = new[] { "Admin", "Warehouse", "Accountant", "Secretary" };
        var chosen = await DisplayActionSheet($"Choose role (current: {RoleName(current)})", "Cancel", null, choices);
        if (chosen == "Cancel" || string.IsNullOrWhiteSpace(chosen)) return null;
        return chosen switch { "Admin" => 1, "Warehouse" => 2, "Accountant" => 3, _ => 4 };
    }

    private static IEnumerable<FirestoreDataDocument<PermissionRecord>> DefaultPermissions(int role, IReadOnlyList<FirestoreDataDocument<PermissionRecord>> permissions)
    {
        var names = role switch
        {
            1 => permissions.Select(x => x.Data.Name).ToHashSet(StringComparer.OrdinalIgnoreCase),
            2 => new[] { "Products.View", "Stock.View", "Stock.In", "Stock.Out" }.ToHashSet(StringComparer.OrdinalIgnoreCase),
            3 => new[] { "Products.View", "Stock.View", "Reports.View" }.ToHashSet(StringComparer.OrdinalIgnoreCase),
            _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        };
        return permissions.Where(x => x.Data.IsActive && names.Contains(x.Data.Name));
    }

    private Task<string> FindUserSyncIdAsync()
    {
        return Task.FromResult(_firestore.AuthenticatedUserId
               ?? throw new InvalidOperationException("The signed-in user record is missing from Firestore."));
    }

    private bool CanManageOrders => _user.Role is "Admin" or "Secretary";
    private static string RoleName(int role) => role switch { 1 => "Admin", 2 => "Warehouse", 3 => "Accountant", 4 => "Secretary", _ => "Unknown" };
    private static string OrderStatusName(int status) => status switch { 1 => "Pending", 2 => "Approved", 3 => "Rejected", 4 => "Completed", 5 => "Cancelled", _ => "Unknown" };
    private static string Normalize(string value) => value.Trim().ToLowerInvariant();
    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"")}\"";

    private static string HashIdentityPassword(string password)
    {
        const int iterations = 100_000;
        var salt = RandomNumberGenerator.GetBytes(16);
        var subkey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA512, 32);
        var payload = new byte[13 + salt.Length + subkey.Length];
        payload[0] = 0x01;
        WriteNetworkInt(payload, 1, 2); // Identity V3 PRF: HMAC-SHA512
        WriteNetworkInt(payload, 5, iterations);
        WriteNetworkInt(payload, 9, salt.Length);
        Buffer.BlockCopy(salt, 0, payload, 13, salt.Length);
        Buffer.BlockCopy(subkey, 0, payload, 13 + salt.Length, subkey.Length);
        return Convert.ToBase64String(payload);
    }

    private static void WriteNetworkInt(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)(value >> 24); buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8); buffer[offset + 3] = (byte)value;
    }

    private async Task<UserRecord> ReadCurrentUserRecordAsync() => (await CurrentUserDocumentAsync()).Data;

    private static Color Ink => Color.FromArgb("#1E293B");
    private static Color Muted => Color.FromArgb("#64748B");
    private static Border Panel(View content) => new() { Content = content, Padding = 15, Stroke = Color.FromArgb("#E2E8F0"), StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 }, BackgroundColor = Colors.White };
    private static Border Card(string title, string detail, string badge)
    {
        var stack = new VerticalStackLayout { Spacing = 5 };
        stack.Children.Add(new Label { Text = title, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Ink });
        if (!string.IsNullOrWhiteSpace(detail)) stack.Children.Add(new Label { Text = detail, FontSize = 13, TextColor = Muted });
        if (!string.IsNullOrWhiteSpace(badge)) stack.Children.Add(Badge(badge, "#E8F6F3"));
        return Panel(stack);
    }
    private static Label Badge(string text, string background) => new() { Text = text, FontSize = 12, TextColor = Color.FromArgb("#0F766E"), BackgroundColor = Color.FromArgb(background), Padding = new Thickness(9, 4), HorizontalOptions = LayoutOptions.Start };
    private static Label SectionTitle(string text) => new() { Text = text, FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Ink, Margin = new Thickness(2, 8, 2, 0) };
    private void AddSectionHeader(string title, string subtitle) { _content.Add(SectionTitle(title)); _content.Add(new Label { Text = subtitle, FontSize = 13, TextColor = Muted, Margin = new Thickness(2, -6, 2, 2) }); }
    private static Label EmptyState(string text) => new() { Text = text, HorizontalTextAlignment = TextAlignment.Center, TextColor = Muted, Margin = new Thickness(12, 28) };
    private static Button SmallButton(string text, Func<Task> action)
    {
        var button = new Button { Text = text, CornerRadius = 9, Padding = new Thickness(12, 7), BackgroundColor = Color.FromArgb("#E8F6F3"), TextColor = Color.FromArgb("#0F766E"), FontSize = 13 };
        button.Clicked += async (_, _) => { try { await action(); } catch (Exception ex) { await Application.Current!.Windows[0].Page!.DisplayAlert("Action failed", ex.Message, "OK"); } };
        return button;
    }
    private static Button PrimaryButton(string text, Func<Task> action) { var button = SmallButton(text, action); button.BackgroundColor = Color.FromArgb("#0D9488"); button.TextColor = Colors.White; return button; }
    private void AddAction(string text, Func<Task> action) => _content.Children.Insert(0, PrimaryButton(text, action));

    private async Task<string?> PromptPasswordAsync(string title, string subtitle)
    {
        var completion = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var passwordEntry = new Entry { IsPassword = true, Placeholder = "At least 6 characters", MaxLength = 64, BackgroundColor = Colors.White };
        var page = new ContentPage { Title = title, BackgroundColor = Color.FromArgb("#F1F5F9") };
        var save = PrimaryButton("Save", async () => { completion.TrySetResult(passwordEntry.Text); await Navigation.PopModalAsync(); });
        var cancel = SmallButton("Cancel", async () => { completion.TrySetResult(null); await Navigation.PopModalAsync(); });
        page.Content = new VerticalStackLayout { Padding = 24, Spacing = 16, VerticalOptions = LayoutOptions.Center, Children = { SectionTitle(title), new Label { Text = subtitle, TextColor = Muted }, passwordEntry, new HorizontalStackLayout { Spacing = 10, Children = { save, cancel } } } };
        await Navigation.PushModalAsync(new NavigationPage(page));
        return await completion.Task;
    }

    public sealed class ProductRecord { public string Name { get; set; } = ""; public string? Description { get; set; } public string? ImagePath { get; set; } public bool IsActive { get; set; } = true; public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } }
    public sealed class VariantRecord { public string Name { get; set; } = ""; public string? Size { get; set; } public string? Color { get; set; } public string? CapType { get; set; } public string? Material { get; set; } public string? ImagePath { get; set; } public bool IsActive { get; set; } = true; public decimal ReservedQuantity { get; set; } public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } }
    public sealed class TransactionRecord { public int Type { get; set; } public decimal Quantity { get; set; } public string? Notes { get; set; } public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } }
    public sealed class OrderRecord { public string CustomerName { get; set; } = ""; public string? CustomerPhone { get; set; } public DateTime RequestedAt { get; set; } public DateTime UpdatedAt { get; set; } public int Status { get; set; } }
    public sealed class OrderItemRecord { public decimal Quantity { get; set; } public DateTime UpdatedAt { get; set; } }
    public sealed class UserRecord { public string Username { get; set; } = ""; public string FullName { get; set; } = ""; public int Role { get; set; } public bool IsActive { get; set; } = true; public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } }
    public sealed class PermissionRecord { public string Name { get; set; } = ""; public string? Description { get; set; } public bool IsActive { get; set; } = true; }
    public sealed class UserPermissionRecord { public DateTime UpdatedAt { get; set; } }
}
