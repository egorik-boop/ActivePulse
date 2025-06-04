using ActivePulse.Data;
using ActivePulse.Forms.Pages;
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

namespace ActivePulse.Forms
{
    /// <summary>
    /// Логика взаимодействия для UsersControlWindow.xaml
    /// </summary>
    public partial class UsersControlWindow : Window
    {
        public UsersControlWindow()
        {
            InitializeComponent();
            CustomerFrame.Navigate(new CustomersPage());
            EmployeeFrame.Navigate(new EmployeesPage());
            LoadPages();
        }
        private void LoadPages()
        {
            // Загрузка страницы пользователей
            CustomerFrame.Navigate(new CustomersPage());

            // Здесь вы можете аналогично загрузить другие страницы
            // EmployeeFrame.Navigate(new EmployeesPage());
            // CustomerFrame.Navigate(new CustomersPage());
        }

        //public MainWindow()
        //{
        //    InitializeComponent();
        //    dishesFrame.Navigate(new DishesPage());
        //}

        //private void Grid_Loaded(object sender, RoutedEventArgs e)
        //{
        //    FrameClass.dishesFrame = dishesFrame;
        //    FrameClass.dishesFrame.Navigate(new Pages.DishesPage());

        //    lbUsername.Content = AuthorizationWindow.currentUser.LastName + " " + AuthorizationWindow.currentUser.FirstName;
        //}
    }
}
