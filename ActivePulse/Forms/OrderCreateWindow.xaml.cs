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
        private readonly decimal _deliveryCost = 300m; // Фиксированная стоимость доставки
        private List<Store> _stores;
        private List<DeliveryCategory> _deliveryCategories;
        private int _currentUserId;

        public OrderCreateWindow(ObservableCollection<Cart> cartItems)
        {
            InitializeComponent();
            _cartItems = cartItems ?? throw new ArgumentNullException(nameof(cartItems));
            _currentUserId = AuthorizationWindow.currentUser?.UserId ?? 0;

            InitializeData();
            UpdateTotals();
        }

        private void InitializeData()
        {
            using (var context = new AppDbContext())
            {
                // Загружаем магазины
                _stores = context.Stores.ToList();
                StoreComboBox.ItemsSource = _stores;

                // Загружаем категории доставки
                _deliveryCategories = context.DeliveryCategories.ToList();
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

        private void UpdateTotals()
        {
            decimal subtotal = _cartItems.Sum(item => item.Total);
            decimal deliveryCost = DeliveryRadio.IsChecked == true ? _deliveryCost : 0;
            decimal total = subtotal + deliveryCost;

            TotalTextBlock.Text = $"Товары: {subtotal} руб.";
            DeliveryCostTextBlock.Text = DeliveryRadio.IsChecked == true ? $"Доставка: {deliveryCost} руб." : "Самовывоз (без стоимости доставки)";
            FinalTotalTextBlock.Text = $"Итого к оплате: {total} руб.";
        }

        private void DeliveryType_Checked(object sender, RoutedEventArgs e)
        {
            if (PickupRadio.IsChecked == true)
            {
                PickupPanel.Visibility = Visibility.Visible;
                DeliveryPanel.Visibility = Visibility.Collapsed;
            }
            else if (DeliveryRadio.IsChecked == true)
            {
                PickupPanel.Visibility = Visibility.Collapsed;
                DeliveryPanel.Visibility = Visibility.Visible;
            }

            UpdateTotals();
        }

        private void PaymentType_Checked(object sender, RoutedEventArgs e)
        {
            CardPanel.Visibility = CardRadio.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void StoreComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Можно добавить логику проверки наличия товаров в выбранном магазине
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
                                    // Создаем и настраиваем команду
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

                            // Создание заказа (остальной код без изменений)
                            var order = new Order
                            {
                                    CustomerId = _currentUserId,
                                    EmployeeId = 1,
                                    StoreId = storeId ?? 1,
                                    Price = _cartItems.Sum(item => item.Total),
                                    PaymentMethod = CashRadio.IsChecked == true ? 1 : 2,
                                    OrderStatus = 1,
                                    OrderDate = DateOnly.FromDateTime(DateTime.Now)
                            };

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
    }
}