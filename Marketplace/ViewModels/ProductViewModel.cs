using System.Windows.Media;

using Marketplace.Model;

namespace Marketplace.ViewModels
{
    public class ProductViewModel
    {
        public ProductViewModel(Product product)
        {
            Id = product.Id;
            Articul = product.Articul;
            Name = product.Name;
            Price = product.Price;
            Discount = product.Discount;
            AmountIntStock = product.AmountIntStock;
            Description = product.Description;
            ImagePath = product.ImagePath;
            Category = product.Category;
            Producer = product.Producer;
            Unit = product.Unit;
            Provider = product.Provider;

            GetBackground();
            GetImagePath();
            GetPrice();
        }

        public int Id { get; set; }
        public string Articul { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public int Discount { get; set; }
        public int AmountIntStock { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public Category Category { get; set; }
        public Producer Producer { get; set; }
        public Unit Unit { get; set; }
        public Provider Provider { get; set; }

        public Brush Background { get; set; }

        private void GetBackground()
        {
            if (Discount >= 15)
            {
                Background = (Brush)new BrushConverter().ConvertFromString("#2e8b57");
                return;
            } else if (AmountIntStock == 0)
            {
                Background = Brushes.LightBlue;
                return;
            } else
            {
                Background = (Brush)new BrushConverter().ConvertFromString("#7FFF00");
                return;
            }
        }

        private void GetImagePath()
        {
            if (string.IsNullOrEmpty(ImagePath) || ImagePath == "")
            {
                ImagePath = "Res/picture.png";
                return;
            }
        }

        private void GetPrice()
        {
            if (Discount == 0)
            {
                return;
            }
            OldPrice = Price;
            Price = OldPrice * (1 - (decimal)Discount / 100);
        }

    }
}

