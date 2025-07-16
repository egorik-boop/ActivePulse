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
        private List<Sport> sports;
        private List<ProductCategory> categories;
        User currentUser = new User();

        public MainWindow(int currentUser_id)
        {
            db = new AppDbContext();
            currentUser = db.Users.FirstOrDefault(u => u.UserId == currentUser_id);
            InitializeComponent();
            LoadProducts();
            LoadSportsAndCategories(); // Добавлен вызов
            ApplyFilters();
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

        private void LoadSportsAndCategories()
        {
            try
            {
                // Загрузка видов спорта с добавлением "Все"
                sports = db.Sports.ToList();
                sports.Insert(0, new Sport { SportId = -1, SportName = "Все" }); // Добавляем вариант "Все"
                SportComboBox.ItemsSource = sports;
                SportComboBox.SelectedIndex = 0; // Выбираем "Все" по умолчанию

                // Загрузка всех категорий
                categories = db.ProductCategories.Include(pc => pc.Sport).ToList();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Ошибка", $"Ошибка загрузки фильтров: {ex.Message}");
            }
        }

        private void SportComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SportComboBox.SelectedItem is Sport selectedSport)
            {
                if (selectedSport.SportId == -1) // Если выбрано "Все"
                {
                    // Показываем все категории
                    CategoryComboBox.ItemsSource = categories;
                }
                else
                {
                    // Фильтрация категорий по выбранному виду спорта
                    CategoryComboBox.ItemsSource = categories
                        .Where(c => c.SportId == selectedSport.SportId)
                        .ToList();
                }
            }
            else
            {
                CategoryComboBox.ItemsSource = null;
            }
            ApplyFilters();
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            IQueryable<Product> query = db.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Manufacturer)
                .Include(p => p.CategoryNameNavigation)
                .AsQueryable();

            // Фильтр по виду спорта (если выбрано не "Все")
            if (SportComboBox.SelectedItem is Sport selectedSport && selectedSport.SportId != -1)
            {
                query = query.Where(p => p.CategoryNameNavigation.SportId == selectedSport.SportId);
            }

            // Фильтр по категории
            if (CategoryComboBox.SelectedItem is ProductCategory selectedCategory)
            {
                query = query.Where(p => p.CategoryName == selectedCategory.CategoryId);
            }

            // Фильтр по поисковой строке
            string searchText = SearchTextBox.Text.ToLower();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(p =>
                    p.ProductName.ToLower().Contains(searchText) ||
                    (p.Manufacturer != null && p.Manufacturer.ManufacturerName.ToLower().Contains(searchText)) ||
                    (p.CategoryNameNavigation != null && p.CategoryNameNavigation.CategoryName.ToLower().Contains(searchText))
                );
            }

            // Группировка и преобразование
            var displayProducts = query
                .AsEnumerable()
                .GroupBy(p => p.ProductName)
                .Select(g => g.First())
                .Select(p => new
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
                })
                .ToList();

            ProductsItemsControl.ItemsSource = displayProducts;
        }
        private void LoadProducts()
        {
            try
            {
                products = db.Products
                    .Include(p => p.ProductImages)
                    .Include(p => p.Manufacturer)
                    .Include(p => p.CategoryNameNavigation)
                    .ToList();

                // Группируем товары по названию и выбираем первый из каждой группы
                var uniqueProducts = products
                    .GroupBy(p => p.ProductName)
                    .Select(g => g.First())
                    .ToList();

                // Создаем анонимный тип для отображения
                var displayProducts = uniqueProducts.Select(p => new
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

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int productId = (int)button.Tag;
            OpenProductInformationWindow(productId);
        }

        private void OpenProductInformationWindow(int productId)
        {
            try
            {
                var product = db.Products
                    .Include(p => p.Manufacturer)
                    .Include(p => p.ProductImages)
                    .FirstOrDefault(p => p.ProductId == productId);

                if (product != null)
                {
                    decimal price = db.ProductInSupplies
                        .Where(ps => ps.ProductId == productId)
                        .OrderBy(ps => ps.Price)
                        .FirstOrDefault()?.Price ?? 0;

                    var specs = db.ProductSpecifications
                        .FirstOrDefault(ps => ps.ProductId == productId);

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


            filteredProducts = products.Where(p =>
                p.ProductName.ToLower().Contains(searchText) ||
                (p.Manufacturer != null && p.Manufacturer.ManufacturerName.ToLower().Contains(searchText)) ||
                (p.CategoryNameNavigation != null && p.CategoryNameNavigation.CategoryName.ToLower().Contains(searchText))
            ).ToList();


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

                    dynamic productDisplay = menuItem.DataContext;
                    int productId = productDisplay.ProductId;


                    var product = db.Products
                        .Include(p => p.Manufacturer)
                        .Include(p => p.ProductImages)
                        .FirstOrDefault(p => p.ProductId == productId);

                    if (product != null)
                    {
                        decimal price = db.ProductInSupplies
                            .Where(ps => ps.ProductId == productId)
                            .OrderBy(ps => ps.Price)
                            .FirstOrDefault()?.Price ?? 0;

                        var specs = db.ProductSpecifications
                            .FirstOrDefault(ps => ps.ProductId == productId);


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

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            var ordersWindow = new OrdersWindow(currentUser.UserId);
            ordersWindow.Owner = this;
            ordersWindow.ShowDialog();
        }

        private void OrdersEditButton_Click(object sender, RoutedEventArgs e)
        {
            var editOrdersWindow = new EditOrdersWindow();
            editOrdersWindow.Owner = this;
            editOrdersWindow.ShowDialog();
        }
    }
}