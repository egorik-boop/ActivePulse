using ActivePulse.Data;
using ActivePulse.Entities;
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
    public partial class ProductInformationWindow : Window
    {
        private readonly Product _product;
        private readonly decimal _price;
        private readonly ProductSpecification _specs;
        private ObservableCollection<Cart> _cart;
        private ObservableCollection<Favourite> _favourites;
        private List<string> _availableSizes;

        public ProductInformationWindow(Product product, decimal price, ObservableCollection<Cart> cart,
                                      ObservableCollection<Favourite> favourites, ProductSpecification? specs = null)
        {
            InitializeComponent();

            _product = product;
            _price = price;
            _specs = specs;
            _cart = cart;
            _favourites = favourites;
            _availableSizes = new List<string>();

            LoadProductData();
            LoadAvailableSizes();
            LoadStoreAvailability();

            SizeComboBox.SelectionChanged += SizeComboBox_SelectionChanged;
        }


        private void LoadProductData()
        {
            ProductNameTextBlock.Text = _product.ProductName;
            ManufacturerTextBlock.Text = _product.Manufacturer?.ManufacturerName ?? "Не указан";
            WarrantyTextBlock.Text = $"{_product.WarrantyPeriodInMonth} месяцев";
            PriceTextBlock.Text = $"{_price} руб.";

            if (_specs != null)
            {
                ColorTextBlock.Text = $"Цвет: {_specs.Color ?? "не указан"}";
                MaterialTextBlock.Text = $"Материал: {_specs.Material ?? "не указан"}";
                WeightTextBlock.Text = _specs.Weight.HasValue ? $"Вес: {_specs.Weight.Value} кг" : "Вес: не указан";
                GenderTextBlock.Text = $"Пол: {_specs.Gender ?? "не указан"}";
            }

            if (_product.ProductImages?.FirstOrDefault() != null)
            {
                ProductImage.Source = new BitmapImage(
                    new Uri("pack://application:,,," + _product.ProductImages.First().Description));
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (!AddToCartButton.IsEnabled) return;
            string selectedSize = SizeComboBox.SelectedItem?.ToString() ?? string.Empty;

            using (var db = new AppDbContext())
            {
                var totalAvailable = db.ProductInSupplies
                    .Where(ps => ps.ProductId == _product.ProductId &&
                                (string.IsNullOrEmpty(selectedSize) || ps.ProductSize == selectedSize))
                    .Sum(ps => ps.Amount);

                if (totalAvailable <= 0)
                {
                    CustomMessageBox.Show("Нет в наличии", $"Товар \"{_product.ProductName}\" отсутствует в магазинах!");
                    return;
                }
            }

            var existingItem = _cart.FirstOrDefault(item =>
                item.ProductId == _product.ProductId && item.Size == selectedSize);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                _cart.Add(new Cart
                {
                    ProductId = _product.ProductId,
                    ProductName = _product.ProductName,
                    ImagePath = _product.ProductImages?.FirstOrDefault()?.Description,
                    Price = _price,
                    Quantity = 1,
                    Size = selectedSize
                });
            }

            CustomMessageBox.Show("Товар в корзине!", $"\"{_product.ProductName}\" добавлен в корзину!");
        }

        

        private void LoadAvailableSizes()
        {
            using (var db = new AppDbContext())
            {
                _availableSizes = db.ProductInSupplies
                    .Where(ps => (ps.Product.ProductName == _product.ProductName ||
                                 ps.ProductId == _product.ProductId) &&
                                !string.IsNullOrEmpty(ps.ProductSize))
                    .Select(ps => ps.ProductSize)
                    .Distinct()
                    .ToList();

                if (_availableSizes.Any())
                {
                    SizeComboBox.ItemsSource = _availableSizes;
                    SizeComboBox.SelectedIndex = 0;
                    SizeComboBox.Visibility = Visibility.Visible;
                }
                else
                {
                    SizeComboBox.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void LoadStoreAvailability()
        {
            string selectedSize = SizeComboBox.SelectedItem?.ToString();

            using (var db = new AppDbContext())
            {
                var query = db.ProductInSupplies
                    .Where(ps => ps.ProductId == _product.ProductId && ps.Amount > 0);

                if (!string.IsNullOrEmpty(selectedSize))
                {
                    query = query.Where(ps => ps.ProductSize == selectedSize);
                }

                var storeItems = query
                    .Join(db.Supplies,
                        ps => ps.SupplyId,
                        s => s.SupplyId,
                        (ps, s) => new
                        {
                            StoreName = s.Store.StoreName,
                            Quantity = ps.Amount,
                            Status = ps.Amount > 0 ? "В наличии" : "Нет в наличии",
                            StatusColor = ps.Amount > 0 ? Brushes.LightGreen : Brushes.Red
                        })
                    .GroupBy(x => x.StoreName)
                    .Select(g => new
                    {
                        StoreName = g.Key,
                        Quantity = g.Sum(x => x.Quantity),
                        Status = g.Sum(x => x.Quantity) > 0 ? "В наличии" : "Нет в наличии",
                        StatusColor = g.Sum(x => x.Quantity) > 0 ? Brushes.LightGreen : Brushes.Red
                    })
                    .OrderBy(x => x.StoreName)
                    .ToList();

                if (storeItems.Count == 0 || storeItems.Sum(x => x.Quantity) <= 0)
                {
                    storeItems = new[]
                    {
                new
                {
                    StoreName = "Товара нет ни в одном магазине",
                    Quantity = 0,
                    Status = "Нет в наличии",
                    StatusColor = Brushes.Red
                }
            }.ToList();

                    AddToCartButton.IsEnabled = false;
                }
                else
                {
                    AddToCartButton.IsEnabled = true;
                }

                StoreAvailabilityListBox.ItemsSource = null;
                StoreAvailabilityListBox.ItemsSource = storeItems;
            }
        }

        private void AddToFavourites_Click(object sender, RoutedEventArgs e)
        {
            var existingItem = _favourites.FirstOrDefault(f => f.ProductId == _product.ProductId);
            if (existingItem == null)
            {
                _favourites.Add(new Favourite
                {
                    ProductId = _product.ProductId,
                    ProductName = _product.ProductName,
                    ImagePath = _product.ProductImages?.FirstOrDefault()?.Description,
                    Price = _price
                });

                CustomMessageBox.Show("Добавлено в избранное", $"{_product.ProductName} добавлен в избранное!");
            }
            else
            {
                CustomMessageBox.Show("Уже в избранном", $"{_product.ProductName} уже есть в избранном!");
            }
        }

        private void SizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadStoreAvailability();
        }
    }
}