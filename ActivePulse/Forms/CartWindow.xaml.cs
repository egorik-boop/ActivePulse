
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
using System.Collections.ObjectModel;
namespace ActivePulse.Forms
{
    /// <summary>
    /// Логика взаимодействия для Cart.xaml
    /// </summary>
    public partial class CartWindow : Window
    {
        private readonly ObservableCollection<Cart> cart;

        public CartWindow(ObservableCollection<Cart> cartItems)
        {
            InitializeComponent();
            cart = cartItems ?? throw new ArgumentNullException(nameof(cartItems));
            DataContext = cart;
        }

        public CartWindow() : this(new ObservableCollection<Cart>()) { }

        public decimal GrandTotal => cart?.Sum(item => item.Total) ?? 0;

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            if (cart.Count > 0)
            {
                OrderCreateWindow orderCreateWindow = new OrderCreateWindow(cart);
                orderCreateWindow.ShowDialog();
            }
            else
            {
                CustomMessageBox.Show("А что заказываете?", "Корзина пуста, заказывать нечего :(");
            }
        }

        private void DeleteItemContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is Cart itemToRemove)
            {
                cart.Remove(itemToRemove);
                UpdateTotal();
            }
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Cart item)
            {
                item.Quantity++;
                UpdateTotal();
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Cart item)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
                else
                {
                    cart.Remove(item);
                }
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            var binding = TotalTextBlock.GetBindingExpression(TextBlock.TextProperty);
            binding?.UpdateTarget();
            CartListView.Items.Refresh();
        }


    }
}
