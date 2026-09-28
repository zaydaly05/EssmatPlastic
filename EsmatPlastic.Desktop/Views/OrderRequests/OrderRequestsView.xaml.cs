using System.Windows;
using System.Windows.Controls;
using EsmatPlastic.Desktop.Models.OrderRequests;
using EsmatPlastic.Desktop.Services;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.OrderRequests;
using EsmatPlastic.Desktop.Services.Stock;
using EsmatPlastic.Desktop.ViewModels.OrderRequests;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views.OrderRequests;

public partial class OrderRequestsView : UserControl
{
    private readonly OrderRequestsViewModel _viewModel;
    private readonly LocalizationService _loc;
    private readonly AppSession _appSession;

    public OrderRequestsView()
    {
        InitializeComponent();

        var sp = App.ServiceProvider;
        _loc = sp.GetRequiredService<LocalizationService>();
        _appSession = sp.GetRequiredService<AppSession>();

        _viewModel = new OrderRequestsViewModel(
            sp.GetRequiredService<OrderRequestService>(),
            sp.GetRequiredService<StockService>(),
            _appSession,
            _loc);

        DataContext = _viewModel;
        ApplyLocalization();

        Loaded += OrderRequestsView_Loaded;
    }

    private void ApplyLocalization()
    {
        PageTitleText.Text = _loc["طلبات الحجز"];
        PageSubtitleText.Text = _loc["إدارة طلبات حجز المنتجات ومتابعة حالتها للعملاء"];
        RefreshButton.Content = _loc["⟳  تحديث"];

        TotalStatLabel.Text = _loc["إجمالي الطلبات"];
        PendingStatLabel.Text = _loc["قيد الانتظار"];
        ApprovedStatLabel.Text = _loc["مقبولة / قيد المعالجة"];
        CompletedStatLabel.Text = _loc["مكتملة"];
        CancelledStatLabel.Text = _loc["ملغاة"];

        NewOrderTitleText.Text = _loc["إنشاء طلب حجز جديد"];
        CustomerNameLabel.Text = _loc["اسم العميل *"];
        CustomerPhoneLabel.Text = _loc["رقم الهاتف"];
        AddProductsLabel.Text = _loc["إضافة منتجات للطلب"];
        ProductVariantLabel.Text = _loc["الصنف / المنتج *"];
        QuantityLabel.Text = _loc["الكمية *"];
        SelectedItemsLabel.Text = _loc["الأصناف المحددة:"];
        SubmitOrderButton.Content = _loc["إرسال طلب الحجز"];

        ColCustomer.Header = _loc["العميل"];
        ColPhone.Header = _loc["الهاتف"];
        ColDate.Header = _loc["التاريخ"];
        ColItems.Header = _loc["الأصناف"];
        ColStatus.Header = _loc["الحالة"];
        ColActions.Header = _loc["الإجراءات"];
    }

    private async void OrderRequestsView_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OrderRequestsView_Loaded;
        await _viewModel.LoadAsync();
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshButton.IsEnabled = false;
        try
        {
            await _viewModel.LoadAsync();
        }
        finally
        {
            RefreshButton.IsEnabled = true;
        }
    }

    private void AddItem_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.AddCurrentItem();
    }

    private void RemoveItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is OrderItemDto item)
        {
            _viewModel.RemoveItem(item);
        }
    }

    private async void SubmitOrder_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_viewModel.CustomerName))
        {
            MessageBox.Show(
                _loc["يرجى إدخال اسم العميل."],
                _loc["تنبيه"],
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (_viewModel.CurrentItems.Count == 0)
        {
            MessageBox.Show(
                _loc["يرجى إضافة صنف واحد على الأقل."],
                _loc["تنبيه"],
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        bool success = await _viewModel.SubmitRequestAsync();
        if (success)
        {
            MessageBox.Show(
                _loc["تم إرسال طلب الحجز بنجاح."],
                _loc["تمت العملية"],
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    private async void CancelOrder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is OrderRequestResponseDto request)
        {
            var result = MessageBox.Show(
                string.Format(_loc["هل تريد إلغاء طلب الحجز رقم {0} للعميل {1}؟"], request.Id, request.CustomerName),
                _loc["تأكيد الإلغاء"],
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _viewModel.CancelRequestAsync(request.Id);
            }
        }
    }

    private void ChangeStatus_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is OrderRequestResponseDto request)
        {
            if (!_appSession.IsAdmin())
            {
                MessageBox.Show(
                    _loc["عفواً، تغيير حالة طلب الحجز مقتصر على مدير النظام فقط."],
                    _loc["تنبيه الصلاحيات"],
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Create context menu for status selection
            var menu = new ContextMenu();

            var statuses = new (string Key, string Label)[]
            {
                ("Pending", _loc["قيد الانتظار"]),
                ("Approved", _loc["تم القبول"]),
                ("Processing", _loc["قيد المعالجة"]),
                ("Completed", _loc["مكتمل"]),
                ("Cancelled", _loc["ملغي"])
            };

            foreach (var (key, label) in statuses)
            {
                var menuItem = new MenuItem
                {
                    Header = label,
                    IsChecked = request.Status == key
                };

                string targetKey = key;
                menuItem.Click += async (_, _) =>
                {
                    await _viewModel.UpdateStatusAsync(request.Id, targetKey);
                };

                menu.Items.Add(menuItem);
            }

            menu.PlacementTarget = btn;
            menu.IsOpen = true;
        }
    }
}
