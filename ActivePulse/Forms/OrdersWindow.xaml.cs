using ActivePulse.Data;
using ActivePulse.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ActivePulse.Forms
{
    public partial class OrdersWindow : Window
    {
        private readonly AppDbContext _db;
        private readonly int _userId;

        public OrdersWindow(int userId)
        {
            InitializeComponent();
            _db = new AppDbContext();
            _userId = userId;
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                var orders = _db.Orders
                    .Include(o => o.OrderStatusNavigation)
                    .Where(o => o.CustomerId == _userId)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();

                var orderDisplays = orders.Select(o => new
                {
                    o.OrderId,
                    o.OrderDate,
                    TotalPrice = o.Price,
                    Status = o.OrderStatusNavigation?.OrderStatusDescription ?? "Неизвестно",
                    Items = _db.ProductsInOrders
                        .Include(pio => pio.Product)
                        .Where(pio => pio.OrderId == o.OrderId)
                        .Select(pio => new
                        {
                            pio.Product,
                            pio.Count,
                            Price = _db.ProductInSupplies
                                .Where(ps => ps.ProductId == pio.ProductId)
                                .OrderBy(ps => ps.Price)
                                .FirstOrDefault().Price
                        })
                        .ToList()
                })
                .ToList();

                OrdersItemsControl.ItemsSource = orderDisplays;
                NoOrdersText.Visibility = orderDisplays.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить заказы: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class ItemTotalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            dynamic item = value;
            return $"{item.Price * item.Count} руб.";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsCompletedOrderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            string status = value.ToString();
            bool isCompleted = status.Contains("Завершен") || status.Contains("Доставлен");

            if (parameter?.ToString() == "Foreground")
            {
                return isCompleted ? Brushes.LightGray : Brushes.White;
            }

            if (isCompleted)
            {
                return new Style(typeof(Expander))
                {
                    Setters =
                {
                    new Setter(Expander.BackgroundProperty, new SolidColorBrush(Color.FromRgb(58, 58, 58))),
                    new Setter(Expander.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(90, 90, 90))),
                    new Setter(Control.ForegroundProperty, Brushes.LightGray)
                }
                };
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}