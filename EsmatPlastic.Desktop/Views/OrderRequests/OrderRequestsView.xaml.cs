using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EsmatPlastic.Desktop.Models; // Assuming DTOs are mapped here
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
            LoadRequests();
            LoadProductVariants();
        }

        private async void LoadProductVariants()
        {
            // In a real app, this would call the API.
            // For now, we assume the ViewModel/Service handles the data.
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (ProductVariantCombo.SelectedItem == null) return;

            var variant = (ProductVariantResponseDto)ProductVariantCombo.SelectedItem;
            if (decimal.TryParse(QuantityInput.Text, out decimal qty))
            {
                _currentItems.Add(new OrderItemDto { ProductVariantId = variant.Id, Quantity = qty });
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

            // Call API /api/OrderRequests
            // Handle response and clear form
            _currentItems.Clear();
            LoadRequests();
        }

        private async void LoadRequests()
        {
            // Call API /api/OrderRequests
        }

        private async void CancelRequest_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var id = (int)btn.Tag;
            // Call API DELETE /api/OrderRequests/{id}
            LoadRequests();
        }
    }

    public class OrderItemDto { public int ProductVariantId { get; set; } public decimal Quantity { get; set; } public string VariantName { get; set; } }
}
