using ActivePulse.Data;
using ActivePulse.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ActivePulse.Forms.Pages
{
    public partial class EmployeesPage : Page
    {
        private AppDbContext _context;
        private Employee _selectedEmployee;
        private bool _isEditMode = false;

        public EmployeesPage()
        {
            InitializeComponent();
            _context = new AppDbContext();
            LoadEmployees();
            LoadGenders();
        }

        private void LoadEmployees()
        {
            try
            {
                var employees = _context.Employees
                    .Include(e => e.GenderNavigation)
                    .ToList();
                EmployeesGrid.ItemsSource = employees;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке сотрудников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadGenders()
        {
            try
            {
                GenderComboBox.ItemsSource = _context.Genders.ToList();
                GenderComboBox.DisplayMemberPath = "Gender1";
                GenderComboBox.SelectedValuePath = "GenderId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка полов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _isEditMode = false;
            EditForm.Visibility = Visibility.Visible;
            ClearForm();
            EmployeesGrid.IsEnabled = false;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEmployee == null)
            {
                MessageBox.Show("Пожалуйста, выберите сотрудника для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isEditMode = true;
            EditForm.Visibility = Visibility.Visible;
            FillForm(_selectedEmployee);
            EmployeesGrid.IsEnabled = false;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEmployee == null)
            {
                MessageBox.Show("Пожалуйста, выберите сотрудника для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить сотрудника {_selectedEmployee.Lastname} {_selectedEmployee.Firstname}?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Employees.Remove(_selectedEmployee);
                    _context.SaveChanges();
                    LoadEmployees();
                    ClearSelection();
                    MessageBox.Show("Сотрудник успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении сотрудника: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                if (_isEditMode)
                {
                    UpdateEmployee();
                }
                else
                {
                    AddEmployee();
                }

                LoadEmployees();
                CancelButton_Click(null, null);
                MessageBox.Show($"Сотрудник успешно {(_isEditMode ? "обновлен" : "добавлен")}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении сотрудника: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            EditForm.Visibility = Visibility.Collapsed;
            EmployeesGrid.IsEnabled = true;
            ClearForm();
            ClearSelection();
        }

        private void EmployeesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedEmployee = EmployeesGrid.SelectedItem as Employee;
            EditButton.IsEnabled = _selectedEmployee != null;
            DeleteButton.IsEnabled = _selectedEmployee != null;
        }

        private void AddEmployee()
        {
            var employee = new Employee
            {
                Firstname = FirstNameTextBox.Text,
                Lastname = LastNameTextBox.Text,
                Patronymic = PatronymicTextBox.Text,
                BirthDate = DateOnly.FromDateTime(BirthDatePicker.SelectedDate ?? DateTime.Now),
                Phone = PhoneTextBox.Text,
                Gender = (GenderComboBox.SelectedItem as Gender)?.GenderId
            };

            _context.Employees.Add(employee);
            _context.SaveChanges();
        }

        private void UpdateEmployee()
        {
            _selectedEmployee.Firstname = FirstNameTextBox.Text;
            _selectedEmployee.Lastname = LastNameTextBox.Text;
            _selectedEmployee.Patronymic = PatronymicTextBox.Text;
            _selectedEmployee.BirthDate = DateOnly.FromDateTime(BirthDatePicker.SelectedDate ?? DateTime.Now);
            _selectedEmployee.Phone = PhoneTextBox.Text;
            _selectedEmployee.Gender = (GenderComboBox.SelectedItem as Gender)?.GenderId;

            _context.Employees.Update(_selectedEmployee);
            _context.SaveChanges();
        }

        private void FillForm(Employee employee)
        {
            FirstNameTextBox.Text = employee.Firstname;
            LastNameTextBox.Text = employee.Lastname;
            PatronymicTextBox.Text = employee.Patronymic;
            BirthDatePicker.SelectedDate = employee.BirthDate.ToDateTime(TimeOnly.MinValue);
            PhoneTextBox.Text = employee.Phone;
            GenderComboBox.SelectedValue = employee.Gender;
        }

        private void ClearForm()
        {
            FirstNameTextBox.Text = string.Empty;
            LastNameTextBox.Text = string.Empty;
            PatronymicTextBox.Text = string.Empty;
            BirthDatePicker.SelectedDate = null;
            PhoneTextBox.Text = string.Empty;
            GenderComboBox.SelectedItem = null;
        }

        private void ClearSelection()
        {
            _selectedEmployee = null;
            EmployeesGrid.SelectedItem = null;
            EditButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(PatronymicTextBox.Text) ||
                BirthDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля (Имя, Фамилия, Отчество, Дата рождения)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }
    }
}