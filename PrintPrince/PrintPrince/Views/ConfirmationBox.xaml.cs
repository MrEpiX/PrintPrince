using System.Windows;

namespace PrintPrince.Views
{
    /// <summary>
    /// Interaction logic for ConfirmationBox.xaml
    /// </summary>
    public partial class ConfirmationBox : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmationBox"/> class.
        /// </summary>
        public ConfirmationBox()
        {
            InitializeComponent();

            Owner = Application.Current.MainWindow;

            // Adding functionality to the title bar
            titleBar.MouseLeftButtonDown += (o, e) => DragMove();
        }
    }
}
