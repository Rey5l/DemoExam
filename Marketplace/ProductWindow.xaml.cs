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
                CreateButton.Visibility = Visibility.Visible;
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
            if (user == null) return;
            else if (user.RoleId != 1) return;

                int id = (int)(sender as Border).Tag;

            new AddEditProductWindow(id).Show();
            Close();
        }
    }
}
