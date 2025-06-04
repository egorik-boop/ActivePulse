using ActivePulse.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для Favourites.xaml
    /// </summary>
    public partial class FavouritesWindow : Window
    {
        private ObservableCollection<Cart> _cart = new ObservableCollection<Cart>();
        private ObservableCollection<Favourite> _favourites;
        internal FavouritesWindow(ObservableCollection<Favourite> favourites, ObservableCollection<Cart> cart)
        {
            InitializeComponent();
            _favourites = favourites;
            _cart = cart; // Сохраняем ссылку на корзину из MainWindow
            FavouritesItemsControl.ItemsSource = favourites;
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            dynamic product = button.DataContext;

            var existingItem = _cart.FirstOrDefault(item => item.ProductId == product.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                _cart.Add(new Cart
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    ImagePath = product.ImagePath,
                    Price = product.Price,
                    Quantity = 1
                });
            }

            CustomMessageBox.Show("Товар в корзине!", $"\"{product.ProductName}\" добавлен в корзину!");
        }

        private void RemoveFromFavourites_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var product = (Favourite)button.DataContext;

            var itemToRemove = _favourites.FirstOrDefault(f => f.ProductId == product.ProductId);
            if (itemToRemove != null)
            {
                _favourites.Remove(itemToRemove);
                CustomMessageBox.Show("Удалено", $"\"{product.ProductName}\" удален из избранного");
            }
        }
    }
}

