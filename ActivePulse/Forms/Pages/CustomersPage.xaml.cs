using ActivePulse.Data;
using ActivePulse.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ActivePulse.Forms.Pages
{
    public partial class CustomersPage : Page
    {
        private AppDbContext _context;
        private List<Customer> _customers;
        private List<Gender> _genders;

        public CustomersPage()
        {
            InitializeComponent();
            _context = new AppDbContext();
            LoadData();
            InitializeGenderComboBox();
        }

        private void LoadData()
        {
            try
            {
                _customers = _context.Customers
                    .Include(c => c.GenderNavigation)
                    .ToList();
                CustomersDataGrid.ItemsSource = _customers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeGenderComboBox()
        {
            try
            {
                _genders = _context.Genders.ToList();
                GenderComboBox.ItemsSource = _genders;
                GenderComboBox.DisplayMemberPath = "Gender1"; // Отображаемое свойство
                GenderComboBox.SelectedValuePath = "GenderId"; // Значение для сохранения
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка полов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var customer = new Customer
                {
                    Firstname = FirstNameTextBox.Text,
                    Lastname = LastNameTextBox.Text,
                    Patronymic = PatronymicTextBox.Text,
                    BirthDate = DateOnly.FromDateTime(BirthDatePicker.SelectedDate ?? DateTime.Now),
                    Phone = PhoneTextBox.Text,
                    Gender = (GenderComboBox.SelectedItem as Gender)?.GenderId
                };

                _context.Customers.Add(customer);
                _context.SaveChanges();
                LoadData();
                ClearFields();
                MessageBox.Show("Клиент успешно добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomersDataGrid.SelectedItem is Customer selectedCustomer)
            {
                try
                {
                    selectedCustomer.Firstname = FirstNameTextBox.Text;
                    selectedCustomer.Lastname = LastNameTextBox.Text;
                    selectedCustomer.Patronymic = PatronymicTextBox.Text;
                    selectedCustomer.BirthDate = DateOnly.FromDateTime(BirthDatePicker.SelectedDate ?? DateTime.Now);
                    selectedCustomer.Phone = PhoneTextBox.Text;
                    selectedCustomer.Gender = (GenderComboBox.SelectedItem as Gender)?.GenderId;

                    _context.Customers.Update(selectedCustomer);
                    _context.SaveChanges();
                    LoadData();
                    ClearFields();
                    MessageBox.Show("Клиент успешно обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите клиента для обновления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomersDataGrid.SelectedItem is Customer selectedCustomer)
            {
                try
                {
                    var result = MessageBox.Show("Вы уверены, что хотите удалить этого клиента?", "Подтверждение удаления",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        _context.Customers.Remove(selectedCustomer);
                        _context.SaveChanges();
                        LoadData();
                        ClearFields();
                        MessageBox.Show("Клиент успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите клиента для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CustomersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomersDataGrid.SelectedItem is Customer selectedCustomer)
            {
                FirstNameTextBox.Text = selectedCustomer.Firstname;
                LastNameTextBox.Text = selectedCustomer.Lastname;
                PatronymicTextBox.Text = selectedCustomer.Patronymic;
                BirthDatePicker.SelectedDate = selectedCustomer.BirthDate?.ToDateTime(TimeOnly.MinValue);
                PhoneTextBox.Text = selectedCustomer.Phone;
                GenderComboBox.SelectedValue = selectedCustomer.Gender;
            }
        }

        // В методах AddButton_Click и UpdateButton_Click исправьте работу с BirthDate:
        

        private void ClearFields()
        {
            FirstNameTextBox.Text = string.Empty;
            LastNameTextBox.Text = string.Empty;
            PatronymicTextBox.Text = string.Empty;
            BirthDatePicker.SelectedDate = null;
            PhoneTextBox.Text = string.Empty;
            GenderComboBox.SelectedItem = null;
            CustomersDataGrid.SelectedItem = null;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }
    }
}