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

            // Инициализация фреймов
            FrameClass.EmployeeFrame = EmployeeFrame;
            FrameClass.CustomerFrame = CustomerFrame;

            // Загрузка страниц
            EmployeeFrame.Navigate(new EmployeesPage());
            CustomerFrame.Navigate(new CustomersPage());
        }
    }
}
