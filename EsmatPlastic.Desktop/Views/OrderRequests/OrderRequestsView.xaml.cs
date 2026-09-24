using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EsmatPlastic.Desktop.Models;
using EsmatPlastic.Shared.Models.ProductVariants; // Fixed missing namespace
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EsmatPlastic.Desktop.Views.OrderRequests
{
    public partial class OrderRequestsView : UserControl
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private ObservableCollection<OrderItemDto> _currentItems = new ObservableCollection<OrderItemDto>();

        public OrderRequestsView()
        {
            InitializeComponent();
            RequestedItemsList.ItemsSource = _currentItems;
            _ = LoadRequestsAsync();
            _ = LoadProductVariantsAsync();
        }

        private async Task LoadProductVariantsAsync()
        {
            // API call to fetch variants for the ComboBox
            await Task.CompletedTask;
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (ProductVariantCombo.SelectedItem == null) return;

            if (ProductVariantCombo.SelectedItem is ProductVariantResponseDto variant)
            {
                if (decimal.TryParse(QuantityInput.Text, out decimal qty))
                {
                    _currentItems.Add(new OrderItemDto {
                        ProductVariantId = variant.Id,
                        Quantity = qty,
                        VariantName = variant.Name
                    });
                }
            }
        }

        private async void SubmitRequest_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CustomerNameInput.Text) || _currentItems.Count == 0)
            {
                MessageBox.Show("يرجى إدخال اسم العميل واختيار منتج واحد على الأقل.");
                return;
            }

            var request = new {
                CustomerName = CustomerNameInput.Text,
                CustomerPhone = CustomerPhoneInput.Text,
                Items = _currentItems.Select(i => new { i.ProductVariantId, i.Quantity }).ToList()
            };

            await SubmitRequestAsync(request);
            _currentItems.Clear();
            await LoadRequestsAsync();
        }

        private async Task SubmitRequestAsync(object request)
        {
            try
            {
                // API POST /api/OrderRequests implementation
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private async void LoadRequests()
        {
            await LoadRequestsAsync();
        }

        private async Task LoadRequestsAsync()
        {
            try
            {
                // API GET /api/OrderRequests implementation
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                // Log error
            }
        }

        private async void CancelRequest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                try
                {
                    // API DELETE /api/OrderRequests/{id}
                    await Task.CompletedTask;
                    await LoadRequestsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }
    }

    public class OrderItemDto
    {
        public int ProductVariantId { get; set; }
        public decimal Quantity { get; set; }
        public string VariantName { get; set; } = string.Empty;
    }
}
