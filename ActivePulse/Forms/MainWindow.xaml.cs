using ActivePulse.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ActivePulse.Forms.Pages;
using ActivePulse.Entities;

namespace ActivePulse.Forms
{
    public partial class MainWindow : Window
    {
        private readonly AppDbContext db;
        private ObservableCollection<Cart> _cart = new ObservableCollection<Cart>();
        private ObservableCollection<Favourite> _favourites = new ObservableCollection<Favourite>();
        private List<Product> products;
        private List<Product> filteredProducts;
        User currentUser = new User();

        public MainWindow(int currentUser_id)
        {
            db = new AppDbContext();
            currentUser = db.Users.FirstOrDefault(u => u.UserId == currentUser_id);
            InitializeComponent();
            LoadProducts();
        }


        private void AddToFavourites_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            dynamic product = button.DataContext;

            var existingItem = _favourites.FirstOrDefault(f => f.ProductId == product.ProductId);
            if (existingItem == null)
            {
                _favourites.Add(new Favourite
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    ImagePath = product.ImagePath,
                    Price = product.Price
                });

                CustomMessageBox.Show("Добавлено в избранное", $"{product.ProductName} добавлен в избранное!");
            }
            else
            {
                CustomMessageBox.Show("Уже в избранном", $"{product.ProductName} уже есть в избранном!");
            }
        }

        private void LoadProducts()
        {
            try
            {
                products = db.Products
                    .Include(p => p.ProductImages)
                    .Include(p => p.Manufacturer)  // Добавляем загрузку производителя
                    .Include(p => p.CategoryNameNavigation)  // Добавляем загрузку категории
                    .ToList();

                // Создаем анонимный тип для отображения
                var displayProducts = products.Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    ManufacturerName = p.Manufacturer?.ManufacturerName ?? "Не указан",
                    CategoryName = p.CategoryNameNavigation?.CategoryName ?? "Не указана",
                    ImagePath = p.ProductImages.FirstOrDefault() != null ?
                              "pack://application:,,," + p.ProductImages.First().Description :
                              "pack://application:,,,/Images/image.png",
                    Price = db.ProductInSupplies
                            .Where(ps => ps.ProductId == p.ProductId)
                            .OrderBy(ps => ps.Price)
                            .FirstOrDefault()?.Price ?? 0
                }).ToList();

                ProductsItemsControl.ItemsSource = displayProducts;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Ошибка", $"Ошибка загрузки товаров: {ex.InnerException?.Message ?? ex.Message}");
            }
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

        private void ViewCart_Click(object sender, RoutedEventArgs e)
        {
            var cartWindow = new CartWindow(_cart);
            cartWindow.ShowDialog();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Возвращаем все продукты
                var displayProducts = products.Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    ManufacturerName = p.Manufacturer?.ManufacturerName ?? "Не указан",
                    CategoryName = p.CategoryNameNavigation?.CategoryName ?? "Не указана",
                    ImagePath = p.ProductImages.FirstOrDefault() != null ?
                              "pack://application:,,," + p.ProductImages.First().Description :
                              "/Images/image.png",
                    Price = db.ProductInSupplies
                            .Where(ps => ps.ProductId == p.ProductId)
                            .OrderBy(ps => ps.Price)
                            .FirstOrDefault()?.Price ?? 0
                }).ToList();

                ProductsItemsControl.ItemsSource = displayProducts;
                return;
            }

            // Фильтруем продукты
            filteredProducts = products.Where(p =>
                p.ProductName.ToLower().Contains(searchText) ||
                (p.Manufacturer != null && p.Manufacturer.ManufacturerName.ToLower().Contains(searchText)) ||
                (p.CategoryNameNavigation != null && p.CategoryNameNavigation.CategoryName.ToLower().Contains(searchText))
            ).ToList();

            // Создаем анонимный тип для отображения отфильтрованных продуктов
            var filteredDisplayProducts = filteredProducts.Select(p => new
            {
                p.ProductId,
                p.ProductName,
                ManufacturerName = p.Manufacturer?.ManufacturerName ?? "Не указан",
                CategoryName = p.CategoryNameNavigation?.CategoryName ?? "Не указана",
                ImagePath = p.ProductImages.FirstOrDefault() != null ?
                          "pack://application:,,," + p.ProductImages.First().Description :
                          "/Images/image.png",
                Price = db.ProductInSupplies
                        .Where(ps => ps.ProductId == p.ProductId)
                        .OrderBy(ps => ps.Price)
                        .FirstOrDefault()?.Price ?? 0
            }).ToList();

            ProductsItemsControl.ItemsSource = filteredDisplayProducts;
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            EditProfileWindow editProfileWindow = new EditProfileWindow(currentUser.UserId);
            editProfileWindow.Show();
        }

        private void StackPanel_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            UsersControlWindow usersControlWindow = new UsersControlWindow();
            usersControlWindow.Show();
        }

        private void ProductsButton_Click(object sender, RoutedEventArgs e)
        {
            ProductsControlWindow productsControlWindow = new ProductsControlWindow();
            productsControlWindow.Show();
        }

        private void FavouritesButton_Click(object sender, RoutedEventArgs e)
        {
            var favouritesWindow = new FavouritesWindow(_favourites, _cart)
            {
                Owner = this
            };
            favouritesWindow.ShowDialog();
        }

        private void ProductInformation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                try
                {
                    // Получаем ProductId из DataContext
                    dynamic productDisplay = menuItem.DataContext;
                    int productId = productDisplay.ProductId;

                    // Загружаем полную информацию о товаре
                    var product = db.Products
                        .Include(p => p.Manufacturer)
                        .Include(p => p.ProductImages)
                        .FirstOrDefault(p => p.ProductId == productId);

                    if (product != null)
                    {
                        // Получаем цену
                        decimal price = db.ProductInSupplies
                            .Where(ps => ps.ProductId == productId)
                            .OrderBy(ps => ps.Price)
                            .FirstOrDefault()?.Price ?? 0;

                        // Получаем спецификации
                        var specs = db.ProductSpecifications
                            .FirstOrDefault(ps => ps.ProductId == productId);

                        // Открываем окно
                        var window = new ProductInformationWindow(product, price, _cart, _favourites, specs)
                        {
                            Owner = this
                        };
                        window.ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show("Ошибка", ex.Message);
                }
            }
        }
    }
}