using System.Windows;

namespace PrintPrince.Views
{
    /// <summary>
    /// Interaction logic for LoginDialog.xaml
    /// </summary>
    public partial class LoginDialog : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginDialog"/> class.
        /// </summary>
        public LoginDialog()
        {
            InitializeComponent();

            Owner = Application.Current.MainWindow;

            // Adding functionality to the title bar
            titleBar.MouseLeftButtonDown += (o, e) => DragMove();
        }
    }
}
