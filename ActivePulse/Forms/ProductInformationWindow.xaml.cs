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
        private ObservableCollection<Cart> _cart = new ObservableCollection<Cart>();
        private ObservableCollection<Favourite> _favourites = new ObservableCollection<Favourite>();
        public ProductInformationWindow(Product product, decimal price, ObservableCollection<Cart> cart, ObservableCollection<Favourite> favourites, ProductSpecification? specs = null)
        {
            InitializeComponent();

            _product = product;
            _price = price;
            _specs = specs;
            _cart = cart;
            _favourites = favourites;

            // Устанавливаем контекст данных
            DataContext = this;

            // Заполняем данные
            LoadProductData();
        }

        private void LoadProductData()
        {
            // Основная информация
            ProductNameTextBlock.Text = _product.ProductName;
            ManufacturerTextBlock.Text = _product.Manufacturer?.ManufacturerName ?? "Не указан";
            WarrantyTextBlock.Text = $"{_product.WarrantyPeriodInMonth} месяцев";
            PriceTextBlock.Text = $"{_price} руб.";

            // Характеристики
            if (_specs != null)
            {
                ColorTextBlock.Text = $"Цвет: {_specs.Color ?? "не указан"}";
                MaterialTextBlock.Text = $"Материал: {_specs.Material ?? "не указан"}";
                WeightTextBlock.Text = _specs.Weight.HasValue ? $"Вес: {_specs.Weight.Value} кг" : "Вес: не указан";
                SizeTextBlock.Text = $"Размер: {_specs.Size ?? "не указан"}";
                GenderTextBlock.Text = $"Пол: {_specs.Gender ?? "не указан"}";
            }

            // Изображение
            if (_product.ProductImages?.FirstOrDefault() != null)
            {
                ProductImage.Source = new BitmapImage(
                    new Uri("pack://application:,,," + _product.ProductImages.First().Description));
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var existingItem = _cart.FirstOrDefault(item => item.ProductId == _product.ProductId);
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
                    Quantity = 1
                });
            }

            CustomMessageBox.Show("Товар в корзине!", $"\"{_product.ProductName}\" добавлен в корзину!");
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
    }
}