using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Windows;

using Marketplace.Helpers;
using Marketplace.Model;

using Microsoft.Win32;

namespace Marketplace
{
    /// <summary>
    /// Логика взаимодействия для AddEditProductWindow.xaml
    /// </summary>
    public partial class AddEditProductWindow : Window
    {
        private MarketplaceDBEntities _db = new MarketplaceDBEntities();
        private bool _isEditing;
        private Product _product;
        private MessageHelper _mh = new MessageHelper();
        public AddEditProductWindow(int? Id)
        {
            InitializeComponent();

            if (Id == null)
            {
                _isEditing = false;
            } else
            {
                _isEditing = true;
                _product = _db.Product.Find(Id);
            }
            LoadData();

        }

        private void LoadData()
        {
            var units = _db.Unit.ToList();
            var categories = _db.Category.ToList();
            var providers = _db.Provider.ToList();
            var producers = _db.Producer.ToList();

            ProductUnit.ItemsSource = units;
            ProductUnit.DisplayMemberPath = "Name";
            ProductUnit.SelectedValuePath = "Id";
            ProductUnit.SelectedIndex = 0;

            ProductProducer.ItemsSource = producers;
            ProductProducer.DisplayMemberPath = "Name";
            ProductProducer.SelectedValuePath = "Id";
            ProductProducer.SelectedIndex = 0;

            ProductProvider.ItemsSource = providers;
            ProductProvider.DisplayMemberPath = "Name";
            ProductProvider.SelectedValuePath = "Id";
            ProductProvider.SelectedIndex = 0;

            ProductCategory.ItemsSource = categories;
            ProductCategory.DisplayMemberPath = "Name";
            ProductCategory.SelectedValuePath = "Id";
            ProductCategory.SelectedIndex = 0;

            if (_isEditing)
            {
                FillData();
            }
        }

        private void FillData()
        {
            ProductArticle.Text = _product.Articul;
            ProductTitle.Text = _product.Name;
            ProductPrice.Text = _product.Price.ToString();
            Discount.Text = _product.Discount.ToString();
            ProductAmountInStock.Text = _product.AmountIntStock.ToString();
            ProductDescription.Text = _product.Description;
            ProductPhoto.Text = _product.ImagePath;

            ProductUnit.SelectedValue = _product.UnitId;
            ProductProducer.SelectedValue = _product.ProducerId;
            ProductProvider.SelectedValue = _product.ProviderId;
            ProductCategory.SelectedValue = _product.CategoryId;

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            if (_isEditing)
                UpdateProduct();
            else
                CreateProduct();

            
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            new ProductWindow().Show();
            Close();
        }

        private void UpdateProduct()
        {
            string article = ProductArticle.Text;
            string title = ProductTitle.Text;
            decimal price = Convert.ToDecimal(ProductPrice.Text);
            int discount = Convert.ToInt32(Discount.Text);
            int amount = Convert.ToInt32(ProductAmountInStock.Text);
            string description = ProductDescription.Text;
            string imagePath = ProductPhoto.Text;

            _product.Articul = article;
            _product.Name = title;
            _product.Price = price;
            _product.Discount = discount;
            _product.AmountIntStock = amount;
            _product.Description = description;
            _product.ImagePath = imagePath;

            _product.UnitId = (int) ProductUnit.SelectedValue;
            _product.ProducerId = (int) ProductProducer.SelectedValue;
            _product.ProviderId = (int) ProductProvider.SelectedValue;
            _product.CategoryId = (int) ProductCategory.SelectedValue;

            try
            {
                _db.Product.AddOrUpdate(_product);
                _db.SaveChanges();
                _mh.ShowInfo("Продукт успешно изменен!");
                new ProductWindow().Show();
                Close();
            } catch (Exception ex)
            {
                _mh.ShowError(ex.Message);
            }
        }

        private void CreateProduct()
        {
            Product product = new Product();

            string article = ProductArticle.Text;
            string title = ProductTitle.Text;
            decimal price = Convert.ToDecimal(ProductPrice.Text);
            int discount = Convert.ToInt32(Discount.Text);
            int amount = Convert.ToInt32(ProductAmountInStock.Text);
            string description = ProductDescription.Text;
            string imagePath = ProductPhoto.Text;

            product.Articul = article;
            product.Name = title;
            product.Price = price;
            product.Discount = discount;
            product.AmountIntStock = amount;
            product.Description = description;
            product.ImagePath = imagePath;

            product.UnitId = (int)ProductUnit.SelectedValue;
            product.ProducerId = (int)ProductProducer.SelectedValue;
            product.ProviderId = (int)ProductProvider.SelectedValue;
            product.CategoryId = (int)ProductCategory.SelectedValue;

            try
            {
                _db.Product.AddOrUpdate(product);
                _db.SaveChanges();
                _mh.ShowInfo("Продукт успешно создан!");
                new ProductWindow().Show();
                Close();
            }
            catch (Exception ex)
            {
                _mh.ShowError(ex.Message);
            }
        }

        private bool ValidateInput()
        {
            StringBuilder errors = new StringBuilder();

            string article = ProductArticle.Text;
            string title = ProductTitle.Text;
            string price = ProductPrice.Text;
            string discount = Discount.Text;
            string amount = ProductAmountInStock.Text;
            string description = ProductDescription.Text;

            if (string.IsNullOrWhiteSpace(article))
                errors.AppendLine("Поле артикула не заполнено!");

            if (string.IsNullOrWhiteSpace(title))
                errors.AppendLine("Поле наименования не заполнено!");

            if (string.IsNullOrWhiteSpace(price) || !decimal.TryParse(price, out decimal priceDecimal))
                errors.AppendLine("Поле цены не заполнено!");

            if (string.IsNullOrWhiteSpace(discount) || !decimal.TryParse(discount, out decimal discountInt)
                || discountInt > 100 || discountInt < 0)
                errors.AppendLine("Поле скидки не заполнено!");

            if (string.IsNullOrWhiteSpace(amount) || !int.TryParse(amount, out int amountInt) || amountInt < 0)
                errors.AppendLine("Поле количества на складке не заполнено!");

            if (string.IsNullOrWhiteSpace(description))
                errors.AppendLine("Поле описания не заполнено!");

            if (errors.Length > 0)
            {
                _mh.ShowError(errors.ToString());
                return false;
            }

            return true;

        }

        private void ChooseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp";
            bool? result = openFile.ShowDialog();
            if (result == true)
            {
                var imagePath = openFile.FileName;
                ProductPhoto.Text = imagePath;
            }
        }
    }
}
