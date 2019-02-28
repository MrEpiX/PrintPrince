using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using MvvmDialogs;
using PrintPrince.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PrintPrince.ViewModels
{
    /// <summary>
    /// The view model of the application to manage content of window.
    /// </summary>
    public class ApplicationViewModel : ValidatableViewModelBase
    {
        /// <summary>
        /// Service that handles the dialogs as MVVM through <see cref="MvvmDialogs"/>.
        /// </summary>
        private readonly IDialogService _dialogService;

        private bool _loading;
        /// <summary>
        /// If there are currently background operations in the <see cref="LoadingHandler"/> that should be made aware to the user.
        /// </summary>
        /// <remarks>
        /// When loading printers, loading drivers or logging into Cirrato through the PMC the <see cref="LoadingHandler"/> has operations that are visible to the user.
        /// </remarks>
        public bool Loading
        {
            get => _loading;
            set => Set(nameof(Loading), ref _loading, value);
        }

        private string _loadingText;
        /// <summary>
        /// The status message of the most recent loading operation in <see cref="LoadingHandler"/> when <see cref="Loading"/> is <c>true</c>.
        /// </summary>
        public string LoadingText
        {
            get => _loadingText;
            set => Set(nameof(LoadingText), ref _loadingText, value);
        }

        /// <summary>
        /// Whether or not the user is logged into the Cirrato PMC.
        /// </summary>
        private bool _loggedIn;

        private ValidatableViewModelBase _currentPage;
        /// <summary>
        /// The current view of the window.
        /// </summary>
        public ValidatableViewModelBase CurrentPage
        {
            get => _currentPage;
            set => Set(nameof(CurrentPage), ref _currentPage, value);
        }

        private List<ValidatableViewModelBase> _pageList;
        /// <summary>
        /// A list of all views to navigate to.
        /// </summary>
        public List<ValidatableViewModelBase> PageList
        {
            get => _pageList;
            set => Set(nameof(PageList), ref _pageList, value);
        }

        /// <summary>
        /// The command to exit the application.
        /// </summary>
        public ICommand ExitCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainMenuViewModel"/> class.
        /// </summary>
        public ApplicationViewModel(IDialogService ds)
        {
            InitializeServices();
            SetupContent();

            _loggedIn = false;

            _dialogService = ds;

            ExitCommand = new RelayCommand(() => Application.Current.MainWindow.Close());

            // Finish the loading operation started in InitializeServices()
            RemoveLoadingOperation("ConstructorLoading");
        }

        /// <summary>
        /// Initializes the <see cref="LoadingHandler"/>, <see cref="SysManManager"/>, <see cref="PrinterRepository"/> and makes sure that the user is logged into the PMC.
        /// </summary>
        private void InitializeServices()
        {
            // Initialize the LoadingHandler
            try
            {
                // Add loading operation to show progress to user
                AddLoadingOperation("ConstructorLoading", "Initializing...");
            }
            catch (Exception ex)
            {
                Logger.Log($"Could not create LoadingHandler. Error message:\n{ex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                MessageBox.Show($"Could not create LoadingHandler. Error message:\n{ex.GetFullMessage()}", "LoadingHandler Creation Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.MainWindow.Close();
                return;
            }

            // Initialize SysManManager
            try
            {
                SysManManager.Initialize();
            }
            catch (HttpRequestException hrex)
            {
                Logger.Log(hrex.GetFullMessage(), System.Diagnostics.EventLogEntryType.Error);
                MessageBox.Show(hrex.GetFullMessage(), "SysMan Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.MainWindow.Close();
                return;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error connecting to SysMan URL! Error message:\n{ex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                MessageBox.Show($"Error connecting to SysMan URL! Error message:\n{ex.GetFullMessage()}", "SysMan Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.MainWindow.Close();
                return;
            }

            // Initialize PrinterRepository
            try
            {
                PrinterRepository.Initialize();
            }
            catch (FileNotFoundException fnfex)
            {
                Logger.Log($"Error connecting to Cirrato PMC! Error message:\n{fnfex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                MessageBox.Show($"Error connecting to Cirrato PMC! Error message:\n{fnfex.GetFullMessage()}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.MainWindow.Close();
                return;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error connecting to Cirrato PMC! Error message:\n{ex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                MessageBox.Show($"Error connecting to Cirrato PMC! Error message:\n{ex.GetFullMessage()}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.MainWindow.Close();
                return;
            }

            try
            {
                // Check whether the user is logged in, and log in if they aren't
                Action checkLogin = async () => await CheckLoginAsync();
                checkLogin.Invoke();
            }
            catch (InvalidOperationException iopex)
            {
                Logger.Log($"Path to Cirrato PMC not set correctly, check .config file! Error message:\n{iopex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                MessageBox.Show($"Path to Cirrato PMC not set correctly, check .config file! Error message:\n{iopex.GetFullMessage()}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.MainWindow.Close();
                return;
            }
            catch (Win32Exception w32ex)
            {
                Logger.Log($"Error opening file \"{PrinterRepository.CirratoPath}\". Error message:\n{w32ex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                MessageBox.Show($"Error opening file \"{PrinterRepository.CirratoPath}\". Error message:\n{w32ex.GetFullMessage()}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.MainWindow.Close();
                return;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error loading data from Cirrato. Error message:\n{ex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                MessageBox.Show($"Error loading data from Cirrato. Error message:\n{ex.GetFullMessage()}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.MainWindow.Close();
                return;
            }
        }

        /// <summary>
        /// Loads all printer data from external systems.
        /// </summary>
        private async Task LoadPrinterData()
        {
            AddLoadingOperation("LoadPMCData", "Loading Cirrato data...");

            try
            {
                await PrinterRepository.LoadPMCData();
            }
            catch
            {
                throw;
            }

            AddLoadingOperation("LoadSysManData", "Loading SysMan data...");

            RemoveLoadingOperation("LoadPMCData");

            try
            {
                await PrinterRepository.LoadSysManData();
            }
            catch
            {
                throw;
            }

            RemoveLoadingOperation("LoadSysManData");
        }

        /// <summary>
        /// Sets up pages that the user can navigate to.
        /// </summary>
        private void SetupContent()
        {
            PageList = new List<ValidatableViewModelBase>();

            var mainMenu = SimpleIoc.Default.GetInstance<MainMenuViewModel>();
            var createPrinter = SimpleIoc.Default.GetInstance<CreatePrinterViewModel>();
            var printerList = SimpleIoc.Default.GetInstance<PrinterListViewModel>();

            ICommand createPrinterNavigateCommand = new RelayCommand(
                () => { NavigateToPage(1); },
                () => CanNavigate(1));

            ICommand printerListNavigateCommand = new RelayCommand(
                () => { NavigateToPage(2); },
                () => CanNavigate(2));

            ICommand returnToMenuCommand = new RelayCommand(
                () => { NavigateToPage(0); },
                () => CanNavigate(0));

            // Set commands to run when the buttons in the views are clicked
            mainMenu.SetCommands(createPrinterNavigateCommand, printerListNavigateCommand);
            createPrinter.SetCommands(returnToMenuCommand);
            printerList.SetCommands(returnToMenuCommand);

            CurrentPage = mainMenu;

            PageList.Add(mainMenu);
            PageList.Add(createPrinter);
            PageList.Add(printerList);
        }

        /// <summary>
        /// Makes sure the user is logged in and that there are no ongoing loading operations in the background.
        /// </summary>
        /// <returns>
        /// Returns whether or not the user is allowed to navigate from the current page.
        /// </returns>
        /// <remarks>
        /// This method is set as CanExecute for the commands calling <see cref="NavigateToPage(int)"/>.
        /// </remarks>
        private bool CanNavigate(int pageIndex)
        {
            // User cannot create printers if they only have read access
            if (pageIndex == 1 && DomainManager.ReadOnlyAccess)
            {
                return false;
            }

            if (_loggedIn && !Loading)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sets the content of the window to a chosen view.
        /// </summary>
        /// <param name="pageIndex">The index of <see cref="PageList"/> to set as current page.</param>
        private void NavigateToPage(int pageIndex)
        {
            // 0 = MainMenu
            // 1 = CreatePrinter
            // 2 = PrinterList
            CurrentPage = PageList[pageIndex];

            // Initialize pages
            if (pageIndex == 1)
            {
                SimpleIoc.Default.GetInstance<CreatePrinterViewModel>().Initialize();
            }
            else if (pageIndex == 2)
            {
                SimpleIoc.Default.GetInstance<PrinterListViewModel>().Initialize();
            }
        }

        /// <summary>
        /// Asynchronously determines whether or not the user is logged into the Cirrato PMC and initiates a login if not.
        /// </summary>
        private async Task CheckLoginAsync()
        {
            AddLoadingOperation("LoginCheck", "Checking login status...");

            string status = await Task.Run(() => {
                var output = PrinterRepository.GetLoginStatusAsync();

                return output;
            });

            RemoveLoadingOperation("LoginCheck");

            if (status.StartsWith("[ERROR]"))
            {
                if (status == "[ERROR] Please log in before executing any scripts.")
                {
                    await LoginAsync();
                }
                else
                {
                    Logger.Log($"Could not log into Cirrato! Error message:\n{status}", System.Diagnostics.EventLogEntryType.Error);
                    MessageBox.Show($"Could not log into Cirrato! Error message:\n{status}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.MainWindow.Close();
                    return;
                }
            }

            _loggedIn = true;

            // Load data from the PMC
            try
            {
                await LoadPrinterData();
            }
            catch (Exception ex)
            {
                Logger.Log($"Communication with Cirrato failed! Error message:\n{ex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                MessageBox.Show($"Communication with Cirrato failed! Error message:\n{ex.GetFullMessage()}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.MainWindow.Close();
                return;
            }
        }

        /// <summary>
        /// Asks for credentials until successfully logged into Cirrato PMC.
        /// </summary>
        private async Task LoginAsync()
        {
            AddLoadingOperation("Login", "Waiting for login...");

            // prompt for login until successful
            bool loginSuccess = false;

            while (loginSuccess != true)
            {
                // create new viewmodel each login attempt, otherwise the OK button stopped working after one click
                var loginViewModel = new LoginDialogViewModel();
                
                // check whether the user clicked the OK button with values entered
                bool? success = _dialogService.ShowDialog(this, loginViewModel);

                if (success == true)
                {
                    // Log in to PMC
                    string output = "";
                    try
                    {
                        // get output from the login command to see if it was successful or not
                        output = await PrinterRepository.LoginAsync(DomainManager.DomainName, loginViewModel.Username, loginViewModel.Password);
                    }
                    catch (InvalidOperationException iopex)
                    {
                        Logger.Log($"Path to Cirrato PMC not set correctly, check .config file! Error message:\n{iopex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                        MessageBox.Show($"Path to Cirrato PMC not set correctly, check .config file! Error message:\n{iopex.GetFullMessage()}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.MainWindow.Close();
                        return;
                    }
                    catch (Win32Exception w32ex)
                    {
                        Logger.Log($"Error opening file \"{PrinterRepository.CirratoPath}\". Error message:\n{w32ex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                        MessageBox.Show($"Error opening file \"{PrinterRepository.CirratoPath}\". Error message:\n{w32ex.GetFullMessage()}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.MainWindow.Close();
                        return;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error starting Cirrato PMC process. Error message:\n{ex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                        MessageBox.Show($"Error starting Cirrato PMC process. Error message:\n{ex.GetFullMessage()}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.MainWindow.Close();
                    }

                    if (output == "[OK] Login successful.")
                    {
                        loginSuccess = true;
                    }
                    else if (output.Contains("[ERROR]"))
                    {
                        string line = output;

                        // Format PMC output and show error to user
                        line = line.Substring(line.IndexOf(']') + 1);
                        line = line.Trim();
                        if (line == "Login unsuccessful. Error code : Unauthorized")
                        {
                            MessageBox.Show("Login unsuccessful, likely incorrect credentials!", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else if (line == "Login unsuccessful. User has insufficient rights.")
                        {
                            MessageBox.Show($"{line} Verify membership in AD group for API access!", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            MessageBox.Show(line, "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        // if this happens, the PMC has been updated and the output is no longer either "[ERROR] ..." or "[OK] Login successful."
                        // change the above if statements to match the current PMC
                        MessageBox.Show($"Could not categorize output from PMC. Message: {output}", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    Logger.Log($"You need to log in to Cirrato PMC to create a printer!", System.Diagnostics.EventLogEntryType.Error);
                    MessageBox.Show("You need to log in to Cirrato PMC to create a printer!", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.MainWindow.Close();
                    return;
                }
            }

            RemoveLoadingOperation("Login");
        }

        /// <summary>
        /// Adds a new loading operation to the <see cref="LoadingHandler"/> and updates <see cref="Loading"/> and <see cref="LoadingText"/>.
        /// </summary>
        private void AddLoadingOperation(string action, string message)
        {
            LoadingHandler.AddLoadingOperation(action, message);
            RefreshLoading();
        }

        /// <summary>
        /// Removes a loading operation from the <see cref="LoadingHandler"/> and updates <see cref="Loading"/> and <see cref="LoadingText"/>.
        /// </summary>
        private bool RemoveLoadingOperation(string action)
        {
            bool result = LoadingHandler.RemoveLoadingOperation(action);
            RefreshLoading();

            return result;
        }

        /// <summary>
        /// Update <see cref="Loading"/> and <see cref="LoadingText"/> on <see cref="ApplicationViewModel"/> and <see cref="CreatePrinterViewModel"/>.
        /// </summary>
        private void RefreshLoading()
        {
            Loading = LoadingHandler.Loading;
            LoadingText = LoadingHandler.CurrentStatus;

            // CreatePrinterViewModel has its own loading property
            SimpleIoc.Default.GetInstance<CreatePrinterViewModel>().RefreshLoading();
        }
    }
}
