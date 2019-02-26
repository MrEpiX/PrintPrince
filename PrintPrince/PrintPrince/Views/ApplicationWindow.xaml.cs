using System.Windows;

namespace PrintPrince.Views
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class ApplicationWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationWindow"/> class.
        /// </summary>
        public ApplicationWindow()
        {
            InitializeComponent();

            // Adding functionality to the title bar
            titleBar.MouseLeftButtonDown += (o, e) => DragMove();
            closeButton.Click += (o, e) => Close();
            minimizeButton.Click += (o, e) => WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Re-centers the window when it changes size.
        /// </summary>
        /// <param name="sizeInfo"></param>
        // Resize function implemented with inspiration from answer from StackOverflow user Imran Rashid at https://stackoverflow.com/a/29822717
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.HeightChanged)
            {
                Top += (sizeInfo.PreviousSize.Height - sizeInfo.NewSize.Height) / 2;
            }
            if (sizeInfo.WidthChanged)
            {
                Left += (sizeInfo.PreviousSize.Width - sizeInfo.NewSize.Width) / 2;
            }
        }
    }
}
