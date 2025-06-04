using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ActivePulse.Forms;
using ActivePulse.Data;
using ActivePulse.Entities;

namespace ActivePulse.Forms
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        int counter = 0;
        AppDbContext dbContext;
        public static User currentUser { get; set; }
        public AuthorizationWindow()
        {
            dbContext = new AppDbContext();
            InitializeComponent();
        }

        public AuthorizationWindow(string login, string password)
        {
            dbContext = new AppDbContext();
            InitializeComponent();
            LoginTextBox.Text = login;
            PasswordTextBox.Password = password;
        }

        private void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow();
            registrationWindow.Show();
            this.Close();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginTextBox.Text == "" || PasswordTextBox.Password == "")
            {
                CustomMessageBox.Show("Ошибка", "Необходимо ввести логин и пароль");
                return;
            }
            try
            {
                var user = dbContext.Users.First(u => u.Login == LoginTextBox.Text);
                if (user != null && Verification.VerifySHA512Hash(PasswordTextBox.Password, user.Password))
                {
                    currentUser = user;
                    MainWindow mainWindow = new MainWindow(currentUser.UserId);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    CustomMessageBox.Show("Ошибка", "Неверный пароль");
                    //можно еще капчу чуть позже добавить 
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Ошибка", "Такой пользователь не найден");
            }
            //        else
            //        {
            //            currentUser = userFromDb.GetUser(textBoxLogin.Text, textBoxPassword.Text);
            //            if (currentUser != null)
            //            {
            //                MainForm mainForm = new MainForm();
            //                mainForm.Show();
            //                this.Hide();
            //            }
            //            else
            //            {
            //                counter++;
            //                if (counter > 1)
            //                {
            //                    CaptchaForm captchaForm = new CaptchaForm();
            //                    DialogResult captcha = captchaForm.ShowDialog();
            //                    return;
            //                }
            //                else
            //                {
            //                    return;
            //                }
            //            }

        }
    }
}
