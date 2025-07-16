using ActivePulse.Data;
using ActivePulse.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ActivePulse.Forms
{
    /// <summary>
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        //AppDbContext db;
        AuthorizationWindow authorizationWindow;
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            authorizationWindow = new AuthorizationWindow();
            authorizationWindow.Show();
            this.Close();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка заполненности полей
                if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                    string.IsNullOrWhiteSpace(LoginTextBox.Text) ||
                    PasswordTextBox.Password.Length == 0 ||
                    ConfirmPasswordTextBox.Password.Length == 0)
                {
                    CustomMessageBox.Show("Пожалуйста, заполните все поля", "Ошибка");
                    return;
                }

                if (PasswordTextBox.Password != ConfirmPasswordTextBox.Password)
                {
                    CustomMessageBox.Show("Пароли не совпадают", "Ошибка");
                    return;
                }

                
                using (var db = new AppDbContext())
                {
                    if (await db.Users.AnyAsync(u => u.Login == LoginTextBox.Text))
                    {
                        CustomMessageBox.Show("Пользователь с таким логином уже существует", "Ошибка");
                        return;
                    }

                    if (await db.Users.AnyAsync(u => u.Email == EmailTextBox.Text))
                    {
                        CustomMessageBox.Show("Пользователь с таким email уже существует", "Ошибка");
                        return;
                    }

                    
                    await db.Database.ExecuteSqlRawAsync(
                        "CALL public.register_customer_and_user({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})",
                        FirstNameTextBox.Text,
                        LastNameTextBox.Text,
                        null, 
                        null, 
                        null, 
                        null, 
                        EmailTextBox.Text,
                        LoginTextBox.Text,
                        Verification.GetSHA512Hash(PasswordTextBox.Password)
                    );

                    SendMessage(EmailTextBox.Text, LoginTextBox.Text, PasswordTextBox.Password);

                    CustomMessageBox.Show("Успех", "Регистрация успешно завершена! Данные для входа отправлены на вашу почту.");

                    authorizationWindow = new AuthorizationWindow(LoginTextBox.Text, PasswordTextBox.Password);
                    authorizationWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка");
            }
        }

        private static void SendMessage(string email, string login, string password)
        {
            MailAddress from = new MailAddress("petrichenkoegor2006@yandex.ru", "ActivePulse");
            // кому отправляем
            MailAddress to = new MailAddress(email);
            // создаем объект сообщения
            MailMessage m = new MailMessage(from, to);
            // тема письма
            m.Subject = "Логин и пароль от аккаунта";
            // текст письма
            m.Body = $"<h2>Ваш логин: {login} \nВаш пароль: {password}</h2>";
            // письмо представляет код html
            m.IsBodyHtml = true;
            // адрес smtp-сервера и порт, с которого будем отправлять письмо
            SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 587);
            // логин и пароль
            smtp.Credentials = new NetworkCredential("petrichenkoegor2006@yandex.ru", "kulbgmdrqowifzkn");
            smtp.EnableSsl = true;
            smtp.Send(m);
        }
    }
}
