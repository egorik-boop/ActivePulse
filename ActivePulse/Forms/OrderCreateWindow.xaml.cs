using ActivePulse.Data;
using ActivePulse.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ActivePulse.Forms
{
    public partial class OrderCreateWindow : Window
    {
        private readonly ObservableCollection<Cart> _cartItems;
        private readonly decimal _deliveryCost = 300m;
        private List<Store> _stores;
        private List<DeliveryCategory> _deliveryCategories;
        private int _currentUserId;
        private Dictionary<int, Dictionary<int, int>> _productAvailability;

        public OrderCreateWindow(ObservableCollection<Cart> cartItems)
        {
            InitializeComponent();
            _cartItems = cartItems ?? throw new ArgumentNullException(nameof(cartItems));
            _currentUserId = AuthorizationWindow.currentUser?.UserId ?? 0;
            _productAvailability = new Dictionary<int, Dictionary<int, int>>();

            Loaded += async (sender, e) =>
            {
                await InitializeDataAsync();
                
                UpdateTotals();
            };
        }

        private async Task InitializeDataAsync()
        {
            using (var context = new AppDbContext())
            {
                // Загружаем магазины
                _stores = await context.Stores.ToListAsync();
                StoreComboBox.ItemsSource = _stores;

                // Загружаем категории доставки
                _deliveryCategories = await context.DeliveryCategories.ToListAsync();

                // Загружаем доступность товаров для всех магазинов
                await LoadProductAvailability();
            }

            // Устанавливаем значения по умолчанию
            PickupRadio.IsChecked = true;
            CashRadio.IsChecked = true;

            // Выбираем первый магазин по умолчанию
            if (_stores?.Count > 0)
            {
                StoreComboBox.SelectedIndex = 0;
            }
        }

        private async Task LoadProductAvailability()
        {
            using (var context = new AppDbContext())
            {
                foreach (var store in _stores)
                {
                    var availability = new Dictionary<int, int>();

                    foreach (var item in _cartItems)
                    {
                        using (var cmd = context.Database.GetDbConnection().CreateCommand())
                        {
                            cmd.CommandText = @"
                            SELECT current_stock 
                            FROM public.get_product_availability_by_store(@p_product_id) 
                            WHERE store_id = @p_store_id";

                            cmd.Parameters.Add(new NpgsqlParameter("@p_product_id", item.ProductId));
                            cmd.Parameters.Add(new NpgsqlParameter("@p_store_id", store.StoreId));

                            if (cmd.Connection.State != System.Data.ConnectionState.Open)
                                await cmd.Connection.OpenAsync();

                            var result = await cmd.ExecuteScalarAsync();
                            int currentStock = result != null ? Convert.ToInt32(result) : 0;

                            availability[item.ProductId] = currentStock;
                        }
                    }

                    _productAvailability[store.StoreId] = availability;
                }
            }
        }

        

        private void UpdateSubmitButtonState()
        {
            if (PickupRadio.IsChecked == true && StoreComboBox.SelectedItem is Store selectedStore)
            {
                var storeAvailability = _productAvailability[selectedStore.StoreId];
                bool allAvailable = _cartItems.All(item =>
                    storeAvailability.TryGetValue(item.ProductId, out var quantity) &&
                    quantity >= item.Quantity);

                SubmitButton.IsEnabled = allAvailable;

                if (!allAvailable)
                {
                    var unavailableItems = _cartItems
                        .Where(item => !storeAvailability.TryGetValue(item.ProductId, out var quantity) ||
                                      quantity < item.Quantity)
                        .Select(item => $"{item.ProductName} (не хватает {item.Quantity - (storeAvailability.TryGetValue(item.ProductId, out var q) ? q : 0)} шт.)");

                    TotalTextBlock.Text = $"Товары: {_cartItems.Sum(item => item.Total)} руб.";
                    DeliveryCostTextBlock.Text = "Самовывоз (не все товары доступны в выбранном магазине)";
                    FinalTotalTextBlock.Text = $"Недоступно: {string.Join(", ", unavailableItems)}";
                }
            }
            else
            {
                SubmitButton.IsEnabled = true;
            }
        }

        private void StoreComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSubmitButtonState();
            UpdateTotals();
            UpdateAvailabilityWarning();
        }

        private void DeliveryType_Checked(object sender, RoutedEventArgs e)
        {
            if (PickupRadio.IsChecked == true)
            {
                PickupPanel.Visibility = Visibility.Visible;
                DeliveryPanel.Visibility = Visibility.Collapsed;
                UpdateSubmitButtonState();
                UpdateAvailabilityWarning(); // Добавьте этот вызов
            }
            else if (DeliveryRadio.IsChecked == true)
            {
                PickupPanel.Visibility = Visibility.Collapsed;
                DeliveryPanel.Visibility = Visibility.Visible;
                SubmitButton.IsEnabled = true;
                AvailabilityWarningTextBlock.Visibility = Visibility.Collapsed; // Скрываем предупреждение при доставке
            }

            UpdateTotals();
        }
        private void UpdateTotals()
        {
            decimal subtotal = _cartItems.Sum(item => item.Total);
            decimal deliveryCost = DeliveryRadio.IsChecked == true ? _deliveryCost : 0;
            decimal total = subtotal + deliveryCost;

            TotalTextBlock.Text = $"Товары: {subtotal} руб.";
            DeliveryCostTextBlock.Text = DeliveryRadio.IsChecked == true ? $"Доставка: {deliveryCost} руб." : "Самовывоз (без стоимости доставки)";
            FinalTotalTextBlock.Text = $"Итого к оплате: {total} руб.";
        }

        

        private void PaymentType_Checked(object sender, RoutedEventArgs e)
        {
            CardPanel.Visibility = CardRadio.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput()) return;

                using (var context = new AppDbContext())
                {
                    using (var transaction = await context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            // Получаем выбранный магазин
                            int? storeId = PickupRadio.IsChecked == true
                                ? ((Store)StoreComboBox.SelectedItem)?.StoreId
                                : null;

                            // Проверка наличия товаров (только для самовывоза)
                            if (PickupRadio.IsChecked == true && storeId.HasValue)
                            {
                                foreach (var item in _cartItems)
                                {
                                    using (var cmd = context.Database.GetDbConnection().CreateCommand())
                                    {
                                        cmd.CommandText = @"
                                SELECT current_stock 
                                FROM public.get_product_availability_by_store(@p_product_id) 
                                WHERE store_id = @p_store_id";

                                        cmd.Parameters.Add(new NpgsqlParameter("@p_product_id", item.ProductId));
                                        cmd.Parameters.Add(new NpgsqlParameter("@p_store_id", storeId.Value));

                                        if (cmd.Connection.State != System.Data.ConnectionState.Open)
                                            await cmd.Connection.OpenAsync();

                                        var result = await cmd.ExecuteScalarAsync();
                                        int currentStock = result != null ? Convert.ToInt32(result) : 0;

                                        if (currentStock < item.Quantity)
                                        {
                                            CustomMessageBox.Show("Товар закончился :(", $"Товара '{item.ProductName}' не хватает в выбранном магазине. Доступно: {currentStock} шт.");
                                            return;
                                        }
                                    }
                                }
                            }

                            // Создание заказа
                            var order = new Order
                            {
                                CustomerId = _currentUserId,
                                EmployeeId = 1, // Здесь можно указать конкретного сотрудника или получить из системы
                                StoreId = storeId ?? 1, // Если доставка, используем магазин по умолчанию
                                Price = _cartItems.Sum(item => item.Total),
                                PaymentMethod = CashRadio.IsChecked == true ? 1 : 2, // 1 - наличные, 2 - карта
                                OrderStatus = 1, // 1 - новый заказ
                                OrderDate = DateOnly.FromDateTime(DateTime.Now)
                            };

                            // Добавляем заказ в контекст
                            context.Orders.Add(order);
                            await context.SaveChangesAsync(); // Сохраняем, чтобы получить OrderId

                            // Добавляем товары в заказ
                            foreach (var item in _cartItems)
                            {
                                var productInOrder = new ProductsInOrder
                                {
                                    OrderId = order.OrderId,
                                    ProductId = item.ProductId,
                                    Count = item.Quantity,
                                };
                                context.ProductsInOrders.Add(productInOrder);
                            }

                            // Если это доставка, добавляем запись о доставке
                            if (DeliveryRadio.IsChecked == true)
                            {
                                var delivery = new Delivery
                                {
                                    DeliveryDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)), // Доставка на следующий день
                                    OrderId = order.OrderId,
                                    DeliveryCategoryId = 1, // Категория доставки по умолчанию
                                    Address = AddressTextBox.Text
                                };
                                context.Deliveries.Add(delivery);
                            }

                            // Если оплата картой, сохраняем данные карты
                            if (CardRadio.IsChecked == true)
                            {
                                var cardData = new CardData
                                {
                                    OrderId = order.OrderId,
                                    CardNumber = CardNumberTextBox.Text,
                                    CardOwner = CardHolderTextBox.Text,
                                    CardPeriod = DateOnly.FromDateTime(CardExpiryDatePicker.SelectedDate.Value),
                                    Cvv = CardCvvTextBox.Text
                                };
                                context.CardData.Add(cardData);
                            }

                            // Сохраняем все изменения
                            await context.SaveChangesAsync();
                            await transaction.CommitAsync();

                            CustomMessageBox.Show("Спасибо за заказ!", "Заказ успешно оформлен!");
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            CustomMessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}",
                               "Ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (PickupRadio.IsChecked == true)
            {
                if (StoreComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите магазин для самовывоза", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            if (DeliveryRadio.IsChecked == true && string.IsNullOrWhiteSpace(AddressTextBox.Text))
            {
                MessageBox.Show("Введите адрес доставки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CardRadio.IsChecked == true && !ValidateCardData())
            {
                return false;
            }

            return true;
        }

        private bool ValidateCardData()
        {
            if (string.IsNullOrWhiteSpace(CardNumberTextBox.Text) ||
                string.IsNullOrWhiteSpace(CardHolderTextBox.Text) ||
                CardExpiryDatePicker.SelectedDate == null ||
                string.IsNullOrWhiteSpace(CardCvvTextBox.Text))
            {
                MessageBox.Show("Заполните все данные карты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CardNumberTextBox.Text.Length != 16 || !CardNumberTextBox.Text.All(char.IsDigit))
            {
                MessageBox.Show("Номер карты должен содержать 16 цифр", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CardCvvTextBox.Text.Length != 3 || !CardCvvTextBox.Text.All(char.IsDigit))
            {
                MessageBox.Show("CVV код должен содержать 3 цифры", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CardExpiryDatePicker.SelectedDate < DateTime.Today)
            {
                MessageBox.Show("Срок действия карты истек", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
        private void UpdateAvailabilityWarning()
        {
            if (PickupRadio.IsChecked == true && StoreComboBox.SelectedItem is Store selectedStore)
            {
                var storeAvailability = _productAvailability[selectedStore.StoreId];
                var unavailableItems = _cartItems
                    .Where(item => !storeAvailability.TryGetValue(item.ProductId, out var quantity) ||
                                  quantity < item.Quantity)
                    .Select(item => $"{item.ProductName} (размер: {item.Size}), не хватает {item.Quantity - (storeAvailability.TryGetValue(item.ProductId, out var q) ? q : 0)} шт.")
                    .ToList();

                if (unavailableItems.Any())
                {
                    AvailabilityWarningTextBlock.Text = $"Внимание: в магазине '{selectedStore.StoreName}' отсутствуют:\n{string.Join("\n", unavailableItems)}";
                    AvailabilityWarningTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    AvailabilityWarningTextBlock.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                AvailabilityWarningTextBlock.Visibility = Visibility.Collapsed;
            }
        }
    }
}