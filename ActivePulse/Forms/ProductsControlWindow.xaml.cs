using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ActivePulse.Data;
using ActivePulse.Entities;
using Microsoft.EntityFrameworkCore;

namespace ActivePulse.Forms
{
    public partial class ProductsControlWindow : Window
    {
        private AppDbContext _context;
        private Product _currentProduct;
        private bool _isAddingNew;

        public ProductsControlWindow()
        {
            InitializeComponent();
            _context = new AppDbContext();
            LoadData();
            LoadComboBoxes();
            SetFormEnabled(false);
        }

        private void LoadData()
        {
            try
            {
                _context.Products
                    .Include(p => p.Manufacturer)
                    .Include(p => p.CategoryNameNavigation)
                    .Load();

                ProductsDataGrid.ItemsSource = _context.Products.Local.ToObservableCollection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadComboBoxes()
        {
            try
            {
                _context.Manufacturers.Load();
                ManufacturerComboBox.ItemsSource = _context.Manufacturers.Local.ToObservableCollection();

                _context.ProductCategories.Load();
                CategoryComboBox.ItemsSource = _context.ProductCategories.Local.ToObservableCollection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки справочников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetFormEnabled(bool enabled)
        {
            ProductNameTextBox.IsEnabled = enabled;
            ManufacturerComboBox.IsEnabled = enabled;
            CategoryComboBox.IsEnabled = enabled;
            WarrantyTextBox.IsEnabled = enabled;
            ColorTextBox.IsEnabled = enabled;
            MaterialTextBox.IsEnabled = enabled;
            WeightTextBox.IsEnabled = enabled;
            SizeTextBox.IsEnabled = enabled;
            GenderComboBox.IsEnabled = enabled;

            SaveButton.IsEnabled = enabled;
            CancelButton.IsEnabled = enabled;

            AddButton.IsEnabled = !enabled;
            EditButton.IsEnabled = !enabled && ProductsDataGrid.SelectedItem != null;
            DeleteButton.IsEnabled = !enabled && ProductsDataGrid.SelectedItem != null;
        }

        private void ClearForm()
        {
            _currentProduct = null;

            ProductNameTextBox.Text = string.Empty;
            ManufacturerComboBox.SelectedItem = null;
            CategoryComboBox.SelectedItem = null;
            WarrantyTextBox.Text = string.Empty;

            ColorTextBox.Text = string.Empty;
            MaterialTextBox.Text = string.Empty;
            WeightTextBox.Text = string.Empty;
            SizeTextBox.Text = string.Empty;
            GenderComboBox.SelectedItem = null;
        }

        private void LoadProductData(Product product)
        {
            if (product == null)
            {
                ClearForm();
                return;
            }

            _currentProduct = product;

            ProductNameTextBox.Text = product.ProductName;
            ManufacturerComboBox.SelectedItem = product.Manufacturer;
            CategoryComboBox.SelectedItem = product.CategoryNameNavigation;
            WarrantyTextBox.Text = product.WarrantyPeriodInMonth.ToString();

            // Загрузка спецификаций
            var spec = _context.ProductSpecifications.FirstOrDefault(ps => ps.ProductId == product.ProductId);
            if (spec != null)
            {
                ColorTextBox.Text = spec.Color;
                MaterialTextBox.Text = spec.Material;
                WeightTextBox.Text = spec.Weight?.ToString();
                SizeTextBox.Text = spec.Size;

                // Установка значения пола
                foreach (ComboBoxItem item in GenderComboBox.Items)
                {
                    if (item.Content.ToString() == spec.Gender)
                    {
                        GenderComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                ColorTextBox.Text = string.Empty;
                MaterialTextBox.Text = string.Empty;
                WeightTextBox.Text = string.Empty;
                SizeTextBox.Text = string.Empty;
                GenderComboBox.SelectedItem = null;
            }
        }

        private void SaveProduct()
        {
            try
            {
                // Основные данные товара
                if (_isAddingNew)
                {
                    _currentProduct = new Product();
                    _context.Products.Add(_currentProduct);
                }

                _currentProduct.ProductName = ProductNameTextBox.Text;
                _currentProduct.Manufacturer = (Manufacturer)ManufacturerComboBox.SelectedItem;
                _currentProduct.ManufacturerId = _currentProduct.Manufacturer?.ManufacturerId ?? 0;
                _currentProduct.CategoryNameNavigation = (ProductCategory)CategoryComboBox.SelectedItem;
                _currentProduct.CategoryName = _currentProduct.CategoryNameNavigation?.CategoryId ?? 0;

                if (int.TryParse(WarrantyTextBox.Text, out int warranty))
                {
                    _currentProduct.WarrantyPeriodInMonth = warranty;
                }

                // Сначала сохраняем продукт, чтобы получить его ID
                _context.SaveChanges();

                // Теперь работаем со спецификациями
                var spec = _context.ProductSpecifications
                    .FirstOrDefault(ps => ps.ProductId == _currentProduct.ProductId);

                if (spec == null)
                {
                    spec = new ProductSpecification
                    {
                        ProductId = _currentProduct.ProductId,
                        Color = ColorTextBox.Text,
                        Material = MaterialTextBox.Text,
                        Size = SizeTextBox.Text,
                        Gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString()
                    };

                    if (decimal.TryParse(WeightTextBox.Text, out decimal weight))
                    {
                        spec.Weight = weight;
                    }

                    _context.ProductSpecifications.Add(spec);
                }
                else
                {
                    spec.Color = ColorTextBox.Text;
                    spec.Material = MaterialTextBox.Text;
                    spec.Size = SizeTextBox.Text;
                    spec.Gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

                    if (decimal.TryParse(WeightTextBox.Text, out decimal weight))
                    {
                        spec.Weight = weight;
                    }
                    else
                    {
                        spec.Weight = null;
                    }
                }

                // Сохраняем изменения спецификаций
                _context.SaveChanges();

                MessageBox.Show("Данные сохранены успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                _isAddingNew = false;
                ClearForm();
                SetFormEnabled(false);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _isAddingNew = true;
            _currentProduct = new Product();
            ClearForm();
            SetFormEnabled(true);
            ProductsDataGrid.SelectedItem = null;
            ProductNameTextBox.Focus();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is Product selectedProduct)
            {
                _isAddingNew = false;
                LoadProductData(selectedProduct);
                SetFormEnabled(true);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is Product selectedProduct)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить товар '{selectedProduct.ProductName}'?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Удаляем спецификации
                        var spec = _context.ProductSpecifications.FirstOrDefault(ps => ps.ProductId == selectedProduct.ProductId);
                        if (spec != null)
                        {
                            _context.ProductSpecifications.Remove(spec);
                        }

                        // Удаляем изображения
                        var images = _context.ProductImages.Where(pi => pi.ProductId == selectedProduct.ProductId).ToList();
                        foreach (var image in images)
                        {
                            _context.ProductImages.Remove(image);
                        }

                        // Удаляем сам товар
                        _context.Products.Remove(selectedProduct);
                        _context.SaveChanges();

                        MessageBox.Show("Товар удален успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                        ClearForm();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления товара: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
            LoadComboBoxes();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveProduct();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _isAddingNew = false;
            SetFormEnabled(false);
            LoadProductData(ProductsDataGrid.SelectedItem as Product);
        }

        private void ProductsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isAddingNew)
            {
                LoadProductData(ProductsDataGrid.SelectedItem as Product);
                SetFormEnabled(false);
            }

            EditButton.IsEnabled = ProductsDataGrid.SelectedItem != null;
            DeleteButton.IsEnabled = ProductsDataGrid.SelectedItem != null;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context.Dispose();
        }
    }
}