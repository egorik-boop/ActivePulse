using ActivePulse.Data;
using ActivePulse.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace ActivePulse.Forms
{
    public partial class EditProfileWindow : Window
    {
        private readonly AppDbContext db;
        private readonly User currentUser;

        public EditProfileWindow(int user_id)
        {
            InitializeComponent();

            db = new AppDbContext();

            //Загружаем пользователя со всеми связанными данными
            currentUser = db.Users
                .Include(u => u.Customer)
                .Include(u => u.Employee)
                .FirstOrDefault(u => u.UserId == user_id);

            if (currentUser == null)
            {
                MessageBox.Show("Пользователь не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            LoadUserData();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация обязательных полей
                if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(LoginTextBox.Text) ||
                    string.IsNullOrWhiteSpace(EmailTextBox.Text))
                {
                    MessageBox.Show("Заполните все обязательные поля (*)", "Ошибка",
                                 MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка выбора пола
                if (GenderComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите пол", "Ошибка",
                                 MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Обновление данных пользователя
                currentUser.Login = LoginTextBox.Text;
                currentUser.Email = EmailTextBox.Text;

                if (!string.IsNullOrEmpty(PasswordBox.Password))
                {
                    if (PasswordBox.Password != ConfirmPasswordBox.Password)
                    {
                        MessageBox.Show("Пароли не совпадают", "Ошибка",
                                     MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (PasswordBox.Password.Length < 6)
                    {
                        MessageBox.Show("Пароль должен содержать не менее 6 символов", "Ошибка",
                                     MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    currentUser.Password = Verification.GetSHA512Hash(PasswordBox.Password);
                }

                // Явно отмечаем объект как измененный
                db.Entry(currentUser).State = EntityState.Modified;

                // Обновление данных в зависимости от роли
                if (currentUser.RoleId == 'А' || currentUser.RoleId == 'П')
                {
                    UpdateEmployeeData();
                }
                else if (currentUser.RoleId == 'К')
                {
                    UpdateCustomerData();
                }

                // Сохраняем изменения с проверкой
                int changes = db.SaveChanges();

                if (changes > 0)
                {
                    CustomMessageBox.Show("Успех", "Ваши данные успешно обновлены");
                    Close();
                }
                else
                {
                    CustomMessageBox.Show("Информация", "Не было изменений для сохранения");
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

        private void UpdateEmployeeData()
        {
            var employee = db.Employees.FirstOrDefault(e => e.EmployeeId == currentUser.EmployeeId);
            if (employee == null)
            {
                MessageBox.Show("Сотрудник не найден", "Ошибка",
                             MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            employee.Firstname = FirstNameTextBox.Text;
            employee.Lastname = LastNameTextBox.Text;
            employee.Patronymic = PatronymicTextBox.Text;
            employee.Phone = PhoneTextBox.Text;

            if (BirthDatePicker.SelectedDate.HasValue)
            {
                employee.BirthDate = new DateOnly(
                    BirthDatePicker.SelectedDate.Value.Year,
                    BirthDatePicker.SelectedDate.Value.Month,
                    BirthDatePicker.SelectedDate.Value.Day);
            }

            // Исправленное преобразование пола
            employee.Gender = GenderComboBox.SelectedItem.ToString() == "Мужской" ? 'М' : 'Ж';

            db.Entry(employee).State = EntityState.Modified;
        }

        private void UpdateCustomerData()
        {
            var customer = db.Customers
                .FirstOrDefault(c => c.CustomerId == currentUser.CustomerId);

            if (customer == null)
            {
                MessageBox.Show("Покупатель не найден", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            customer.Firstname = FirstNameTextBox.Text;
            customer.Lastname = LastNameTextBox.Text;
            customer.Patronymic = PatronymicTextBox.Text;
            customer.Phone = PhoneTextBox.Text ?? string.Empty; // Гарантируем, что телефон не будет null

            if (BirthDatePicker.SelectedDate.HasValue)
            {
                customer.BirthDate = new DateOnly(
                    BirthDatePicker.SelectedDate.Value.Year,
                    BirthDatePicker.SelectedDate.Value.Month,
                    BirthDatePicker.SelectedDate.Value.Day);
            }

            // Исправленное преобразование пола
            customer.Gender = GenderComboBox.SelectedItem.ToString() == "Мужской" ? 'М' : 'Ж';

            db.Entry(customer).State = EntityState.Modified;
        }

        private void LoadUserData()
        {
            LoginTextBox.Text = currentUser.Login;
            EmailTextBox.Text = currentUser.Email;

            if (currentUser.RoleId == 'А' || currentUser.RoleId == 'П')
            {
                LoadEmployeeData();
            }
            else if (currentUser.RoleId == 'К')
            {
                LoadCustomerData();
            }
        }

        private void LoadEmployeeData()
        {
            var employee = db.Employees.FirstOrDefault(e => e.EmployeeId == currentUser.EmployeeId);
            if (employee == null) return;

            FirstNameTextBox.Text = employee.Firstname;
            LastNameTextBox.Text = employee.Lastname;
            PatronymicTextBox.Text = employee.Patronymic;
            PhoneTextBox.Text = employee.Phone;

            if (employee.BirthDate != default)
            {
                BirthDatePicker.SelectedDate = new DateTime(
                    employee.BirthDate.Year,
                    employee.BirthDate.Month,
                    employee.BirthDate.Day);
            }

            // Исправленное отображение пола
            GenderComboBox.SelectedItem = employee.Gender == 'М' ? "Мужской" : "Женский";
        }

        private void LoadCustomerData()
        {
            var customer = db.Customers.FirstOrDefault(c => c.CustomerId == currentUser.CustomerId);
            if (customer == null) return;

            FirstNameTextBox.Text = customer.Firstname;
            LastNameTextBox.Text = customer.Lastname;
            PatronymicTextBox.Text = customer.Patronymic;
            PhoneTextBox.Text = customer.Phone;

            if (customer.BirthDate.HasValue)
            {
                BirthDatePicker.SelectedDate = new DateTime(
                    customer.BirthDate.Value.Year,
                    customer.BirthDate.Value.Month,
                    customer.BirthDate.Value.Day);
            }

            // Исправленное отображение пола
            GenderComboBox.SelectedItem = customer.Gender == 'М' ? "Мужской" : "Женский";
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            db?.Dispose();
        }
    }
}