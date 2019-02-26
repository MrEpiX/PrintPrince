using System.Windows;

namespace PrintPrince.Views
{
    /// <summary>
    /// Interaction logic for PrinterDetails.xaml
    /// </summary>
    public partial class PrinterDetails : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrinterDetails"/> class.
        /// </summary>
        public PrinterDetails()
        {
            InitializeComponent();

            Owner = Application.Current.MainWindow;

            // Adding functionality to the title bar
            titleBar.MouseLeftButtonDown += (o, e) => DragMove();
        }
    }
}
