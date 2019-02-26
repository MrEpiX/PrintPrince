using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;

namespace PrintPrince.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Instance of <see cref="ApplicationViewModel"></see>.
        /// </summary>
        public ApplicationViewModel Application => ServiceLocator.Current.GetInstance<ApplicationViewModel>();

        /// <summary>
        /// Instance of <see cref="MainMenuViewModel"></see>.
        /// </summary>
        public MainMenuViewModel MainMenu => ServiceLocator.Current.GetInstance<MainMenuViewModel>();

        /// <summary>
        /// Instance of <see cref="PrinterListViewModel"></see>.
        /// </summary>
        public PrinterListViewModel PrinterList => ServiceLocator.Current.GetInstance<PrinterListViewModel>();

        /// <summary>
        /// Instance of <see cref="CreatePrinterViewModel"></see>.
        /// </summary>
        public CreatePrinterViewModel CreatePrinter => ServiceLocator.Current.GetInstance<CreatePrinterViewModel>();

        /// <summary>
        /// Instance of <see cref="LoginDialogViewModel"></see>.
        /// </summary>
        public LoginDialogViewModel LoginDialog => ServiceLocator.Current.GetInstance<LoginDialogViewModel>();

        /// <summary>
        /// Instance of <see cref="ConfirmationBoxViewModel"></see>.
        /// </summary>
        public ConfirmationBoxViewModel ConfirmationBox => ServiceLocator.Current.GetInstance<ConfirmationBoxViewModel>();

        /// <summary>
        /// Instance of <see cref="PrinterDetailsViewModel"></see>.
        /// </summary>
        public PrinterDetailsViewModel PrinterDetails => ServiceLocator.Current.GetInstance<PrinterDetailsViewModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelLocator"></see> class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<ApplicationViewModel>();
            SimpleIoc.Default.Register<MainMenuViewModel>();
            SimpleIoc.Default.Register<PrinterListViewModel>();
            SimpleIoc.Default.Register<CreatePrinterViewModel>();
            SimpleIoc.Default.Register<LoginDialogViewModel>();
            SimpleIoc.Default.Register<ConfirmationBoxViewModel>();
            SimpleIoc.Default.Register<PrinterDetailsViewModel>();
        }
    }
}