using ActivePulse.Data;
using ActivePulse.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ActivePulse.Forms
{
    public partial class EditOrdersWindow : Window
    {
        private readonly AppDbContext _context;
        private List<Order> _orders;
        private List<OrderStatus> _statuses;
        private List<PaymentMethod> _paymentMethods;

        public EditOrdersWindow()
        {
            InitializeComponent();
            _context = new AppDbContext();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _orders = _context.Orders
                    .Include(o => o.OrderStatusNavigation)
                    .Include(o => o.PaymentMethodNavigation)
                    .ToList();

                _statuses = _context.OrderStatuses.AsNoTracking().ToList();
                _paymentMethods = _context.PaymentMethods.AsNoTracking().ToList();

                OrdersDataGrid.ItemsSource = _orders;

                if (OrdersDataGrid.Columns.Count > 3)
                {
                    var paymentColumn = OrdersDataGrid.Columns[3] as DataGridComboBoxColumn;
                    paymentColumn.ItemsSource = _paymentMethods;

                    var statusColumn = OrdersDataGrid.Columns[4] as DataGridComboBoxColumn;
                    statusColumn.ItemsSource = _statuses;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Завершаем редактирование
                OrdersDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
                OrdersDataGrid.CommitEdit();

                // Обновляем состояние всех измененных сущностей
                foreach (var order in _orders)
                {
                    var entry = _context.Entry(order);
                    if (entry.State == EntityState.Detached)
                    {
                        _context.Orders.Attach(order);
                        entry.State = EntityState.Modified;
                    }
                }

                int changes = _context.SaveChanges();

                if (changes > 0)
                {
                    MessageBox.Show($"Сохранено {changes} изменений", "Успех",
                                 MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Нет изменений для сохранения", "Информация",
                                 MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"Ошибка базы данных: {ex.InnerException?.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                if (entry.State != EntityState.Unchanged)
                    entry.State = EntityState.Unchanged;
            }
            Close();
        }

        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Order order)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить заказ №{order.OrderId}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        OrdersDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
                        OrdersDataGrid.CommitEdit();

                        // Загружаем заказ со всеми связанными данными
                        var orderToDelete = _context.Orders
                            .Include(o => o.ProductsInOrders)
                            .Include(o => o.Deliveries) // Добавляем загрузку доставок
                            .FirstOrDefault(o => o.OrderId == order.OrderId);

                        if (orderToDelete != null)
                        {
                            // Удаляем связанные продукты в заказе
                            _context.ProductsInOrders.RemoveRange(orderToDelete.ProductsInOrders);

                            // Удаляем связанные доставки
                            _context.Deliveries.RemoveRange(orderToDelete.Deliveries);

                            // Удаляем сам заказ
                            _context.Orders.Remove(orderToDelete);

                            // Сохраняем изменения
                            _context.SaveChanges();

                            // Обновляем интерфейс
                            _orders.Remove(order);
                            OrdersDataGrid.ItemsSource = null;
                            OrdersDataGrid.ItemsSource = _orders;

                            MessageBox.Show("Заказ успешно удален", "Успех",
                                          MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (DbUpdateException dbEx)
                    {
                        MessageBox.Show($"Ошибка базы данных: {dbEx.InnerException?.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void OrdersDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is Order order && _context.Entry(order).State == EntityState.Deleted)
            {
                e.Row.IsEnabled = false;
                e.Row.Background = new SolidColorBrush(Colors.Gray);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _context.Dispose();
            base.OnClosed(e);
        }
    }
}