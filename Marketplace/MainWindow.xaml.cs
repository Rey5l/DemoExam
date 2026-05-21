using System.Linq;
using System.Windows;
using System.Windows.Input;

using Marketplace.Helpers;
using Marketplace.Model;
using Marketplace.Statics;

namespace Marketplace
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MarketplaceDBEntities _db = new MarketplaceDBEntities();
        private MessageHelper _mh = new MessageHelper();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordTextBox.Password;

            var user = _db.Users.Where(u => u.Login == login && u.Password == password).FirstOrDefault();
            if (user != null)
            {
                CurrentSession.CurrentUser = user;
                new ProductWindow().Show();
                Close();
            } else 
            {
                _mh.ShowError("Введен неправильный логин или пароль");
                return;
            }

        }


        private void AsGuest_MouseDown(object sender, MouseButtonEventArgs e)
        {
            new ProductWindow().Show();
            Close();
        }
    }
}
