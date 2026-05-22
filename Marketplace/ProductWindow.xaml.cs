using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Marketplace.Helpers;
using Marketplace.Model;
using Marketplace.Statics;
using Marketplace.ViewModels;

namespace Marketplace
{
    /// <summary>
    /// Логика взаимодействия для ProductWindow.xaml
    /// </summary>
    public partial class ProductWindow : Window
    {
        private MarketplaceDBEntities _db = new MarketplaceDBEntities();
        private MessageHelper _mh = new MessageHelper();
        private List<ProductViewModel> _allProducts = new List<ProductViewModel>();
        private List<ProductViewModel> _filteredProducts = new List<ProductViewModel>();
        private int? _selectedProductId;
        private Border _selectedBorder;

        private string[] _sortingTypes = new string[]
        {
            "По умолчанию",
            "По возрастанию",
            "По убыванию"
        };

        private List<string> _filteringTypes = new List<string>()
        {
            "Все поставщики"
        };
        
        public ProductWindow()
        {
            InitializeComponent();
            LoadData();
            LoadProducts();
            LoadUI();
        }

        private void LoadUI()
        {
            var user = CurrentSession.CurrentUser;
            if (user == null || user.RoleId == 3)
            {
                AdminPanel.Visibility = Visibility.Collapsed;

            } else if (user.RoleId == 1)
            {
                EditButton.Visibility = Visibility.Visible;
                CreateButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;
            }
        }

        private void LoadData()
        {
            var currentUser = CurrentSession.CurrentUser;

            if (currentUser != null)
            {
                FullUserName.Text = currentUser.Surname + " " + currentUser.Name + " " + currentUser.Patronmic;
            }
            else
            {
                FullUserName.Text = "Гость";
            }

            SortingComboBox.ItemsSource = _sortingTypes;
            SortingComboBox.SelectedIndex = 0;

            var provider = _db.Provider.ToList();
            foreach (var pr in provider)
            {
                _filteringTypes.Add(pr.Name);
            }

            FilterComboBox.ItemsSource = _filteringTypes;
            FilterComboBox.SelectedIndex = 0;
        }

        private void LoadProducts()
        {
            var products = _db.Product.ToList();
            _allProducts = products.Select(p => new ProductViewModel(p)).ToList();
            ClearSelection();
            ProductList.ItemsSource = _allProducts;
        }

        private void ApplyFilters()
        {
            var result = _allProducts;
            string searchText = SearchTextBox?.Text?.ToLower() ?? "";
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                result = result
                .Where(p =>
                    p.Category.Name.ToLower().Contains(searchText) ||
                    p.Name.ToLower().Contains(searchText) ||
                    p.Description.ToLower().Contains(searchText) ||
                    p.Provider.Name.ToLower().Contains(searchText) ||
                    p.Producer.Name.ToLower().Contains(searchText) ||
                    p.Unit.Name.ToLower().Contains(searchText)
                )
                .ToList();
            }

            string filterText = FilterComboBox?.SelectedValue?.ToString();
            if (filterText != null && filterText != "Все поставщики")
            {
                result = result.Where(p => p.Provider.Name == filterText).ToList();
            }

            int sortingTypes = SortingComboBox?.SelectedIndex ?? 0;
            if (sortingTypes == 1)
            {
                result = result.OrderByDescending(p => p.AmountIntStock).ToList();
            } else if (sortingTypes == 2)
            {
                result = result.OrderBy(p => p.AmountIntStock).ToList();
            }

            _filteredProducts = result;
            ClearSelection();
            ProductList.ItemsSource = _filteredProducts;
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentSession.CurrentUser = null;
            new MainWindow().Show();
            Close();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            new AddEditProductWindow(null).Show();
            Close();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var user = CurrentSession.CurrentUser;
            if (user == null || user.RoleId != 1) return;

            var border = sender as Border;
            SelectProduct(border);
        }

        private void SelectProduct(Border border)
        {
            if (_selectedBorder != null)
            {
                _selectedBorder.BorderBrush = Brushes.Transparent;
                _selectedBorder.BorderThickness = new Thickness(0);
            }

            _selectedBorder = border;
            _selectedProductId = (int)border.Tag;
            border.BorderBrush = Brushes.DarkBlue;
            border.BorderThickness = new Thickness(2);
        }

        private void ClearSelection()
        {
            if (_selectedBorder != null)
            {
                _selectedBorder.BorderBrush = Brushes.Transparent;
                _selectedBorder.BorderThickness = new Thickness(0);
            }

            _selectedBorder = null;
            _selectedProductId = null;
        }


        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_selectedProductId.HasValue)
            {
                _mh.ShowWarning("Выберите продукт, нажав на его карточку");
                return;
            }

            int id = _selectedProductId.Value;
            var product = _db.Product.Find(id);
            if (product == null) return;

            var confirm = MessageBox.Show($"Удалить продукт {product.Name}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                var orderItems = _db.ProductInOrder.Where(p => p.ProductId == product.Id).ToList();
                _db.ProductInOrder.RemoveRange(orderItems);
                _db.Product.Remove(product);
                _db.SaveChanges();

                ClearSelection();
                _mh.ShowInfo("Продукт успешно удален!");
                LoadProducts();
                ApplyFilters();
            } catch (Exception ex)
            {
                _mh.ShowError(ex.Message);
            }


        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_selectedProductId.HasValue)
            {
                _mh.ShowInfo("Выберите продукт, нажав на его карточку");
                return;
            }

            new AddEditProductWindow(_selectedProductId.Value).Show();
            Close();
        }
    }
}
