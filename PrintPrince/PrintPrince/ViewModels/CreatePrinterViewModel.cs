using GalaSoft.MvvmLight.CommandWpf;
using MvvmDialogs;
using PrintPrince.Models;
using PrintPrince.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PrintPrince.ViewModels
{
    /// <summary>
    /// ViewModel used to create a printer.
    /// </summary>
    public class CreatePrinterViewModel : ValidatableViewModelBase
    {
        #region Private Fields

        /// <summary>
        /// Service that handles the dialogs as MVVM through <see cref="MvvmDialogs"/>.
        /// </summary>
        private readonly IDialogService _dialogService;

        /// <summary>
        /// Whether or not the current status message should stay regardless of loading changes.
        /// </summary>
        private bool _persistentMessage;

        #endregion

        #region Public Properties

        private List<string> _cirratoDrivers;
        /// <summary>
        /// List of drivers in the Cirrato environment.
        /// </summary>
        /// <remarks>
        /// This list is populated asynchronously from Cirrato through the PMC using <see cref="PrinterRepository"/>.
        /// </remarks>
        public List<string> CirratoDrivers
        {
            get => _cirratoDrivers;
            set => Set(nameof(CirratoDrivers), ref _cirratoDrivers, value);
        }

        private List<string> _configurationList;
        /// <summary>
        /// List of configurations in the Cirrato environment for the currently selected driver.
        /// </summary>
        /// <remarks>
        /// This list is populated asynchronously from Cirrato through the PMC using <see cref="PrinterRepository"/>.
        /// </remarks>
        public List<string> ConfigurationList
        {
            get => _configurationList;
            set
            {
                Set(nameof(ConfigurationList), ref _configurationList, value);

                if (value != null)
                {
                    ConfigurationsExist = value.Count > 0;
                }
                else
                {
                    ConfigurationsExist = false;
                }
            }
        }

        private string _selectedConfiguration;
        /// <summary>
        /// The currently selected configuration.
        /// </summary>
        public string SelectedConfiguration
        {
            get => _selectedConfiguration;
            set => Set(nameof(SelectedConfiguration), ref _selectedConfiguration, value);
        }

        private bool _configurationsExist;
        /// <summary>
        /// If there are any configurations for the currently selected driver.
        /// </summary>
        public bool ConfigurationsExist
        {
            get => _configurationsExist;
            set => Set(nameof(ConfigurationsExist), ref _configurationsExist, value);
        }

        private List<string> _cirratoPrinters;
        /// <summary>
        /// List of printers in the Cirrato environment.
        /// </summary>
        /// <remarks>
        /// This list is populated asynchronously from Cirrato through the PMC using <see cref="PrinterRepository"/>.
        /// </remarks>
        public List<string> CirratoPrinters
        {
            get => _cirratoPrinters;
            set => Set(nameof(CirratoPrinters), ref _cirratoPrinters, value);
        }

        private List<string> _cirratoRegions;
        /// <summary>
        /// List of regions in the Cirrato environment.
        /// </summary>
        /// <remarks>
        /// This list is populated asynchronously from Cirrato through the PMC using <see cref="PrinterRepository"/>.
        /// </remarks>
        public List<string> CirratoRegions
        {
            get => _cirratoRegions;
            set => Set(nameof(CirratoRegions), ref _cirratoRegions, value);
        }

        #region Printer Name

        private string _selectedSite;
        /// <summary>
        /// Currently selected site for the printer name.
        /// </summary>
        /// <remarks>
        /// Printer names are formatted as "Site_Building_Floor_FirstFreeNumber".
        /// </remarks>
        public string SelectedSite
        {
            get { return _selectedSite; }
            set
            {
                Set(nameof(SelectedSite), ref _selectedSite, value);

                if (value != null)
                {
                    // reset building and floor comboboxes if a new site is chosen
                    if (Buildings != null)
                    {
                        Buildings.Clear();
                        SelectedBuilding = null;
                    }
                    if (Floors != null)
                    {
                        Floors.Clear();
                        SelectedFloor = null;
                    }

                    // populate buildings combobox, which in turn tries to populate floors through property change
                    SetBuildings();
                    // set hint text
                    SetNames();
                }
            }
        }

        private string _selectedBuilding;
        /// <summary>
        /// Currently selected building for the printer name.
        /// </summary>
        /// <remarks>
        /// Printer names are formatted as "Site_Building_Floor_FirstFreeNumber".
        /// </remarks>
        public string SelectedBuilding
        {
            get { return _selectedBuilding; }
            set
            {
                Set(nameof(SelectedBuilding), ref _selectedBuilding, value);

                if (value != null)
                {
                    // reset floor combobox if a new building is chosen
                    if (Floors != null)
                    {
                        Floors.Clear();
                        SelectedFloor = null;
                    }

                    // populate floors combobox
                    SetFloors();
                    // set hint text and final name suggestion based on site, building and floor
                    SetNames();
                }
            }
        }

        private string _selectedFloor;
        /// <summary>
        /// Currently selected floor for the printer name.
        /// </summary>
        /// <remarks>
        /// Printer names are formatted as "Site_Building_Floor_FirstFreeNumber".
        /// </remarks>
        public string SelectedFloor
        {
            get { return _selectedFloor; }
            set
            {
                Set(nameof(SelectedFloor), ref _selectedFloor, value);

                if (value != null)
                {
                    SetNames();
                }
            }
        }

        private string _suggestedName = "";
        /// <summary>
        /// The name format used as a hint for the final printer name.
        /// </summary>
        public string SuggestedName
        {
            get => _suggestedName;
            set => Set(nameof(SuggestedName), ref _suggestedName, value);
        }

        private string _printerName = "";
        /// <summary>
        /// Final printer name set after all parts are selected, using the first number available.
        /// </summary>
        /// <remarks>
        /// When Site, Building and Floor have been selected this is set based on the first available number matching the format among the printers in <see cref="CirratoPrinters"/>, starting from 01.
        /// This property is validated to not be empty, null or whitespace.
        /// </remarks>
        [CustomValidation(typeof(CreatePrinterViewModel), "ValidatePrinterName")]
        public string PrinterName
        {
            get { return _printerName; }
            set
            {
                Set(nameof(PrinterName), ref _printerName, value);

                if (value != null)
                {
                    ValidateAsync();
                }
            }
        }

        private List<string> _sites;
        /// <summary>
        /// A list holding all the sites found among printers in Cirrato.
        /// </summary>
        /// <remarks>
        /// This list is populated after importing all printers from Cirrato and splitting their names on the underscore symbol, using only the first part.
        /// </remarks>
        public List<string> Sites
        {
            get => _sites;
            set => Set(nameof(Sites), ref _sites, value);
        }

        private List<string> _buildings;
        /// <summary>
        /// A list holding all the buildings found among printers in Cirrato matching the <see cref="SelectedSite"/>.
        /// </summary>
        /// <remarks>
        /// This list is cleared and populated when <see cref="SelectedSite"/> is changed, based on the printers in <see cref="CirratoPrinters"/> that match the new site. Those are split on the underscore symbol, using only the second part.
        /// </remarks>
        public List<string> Buildings
        {
            get => _buildings;
            set => Set(nameof(Buildings), ref _buildings, value);
        }

        private List<string> _floors;
        /// <summary>
        /// A list holding all the floors found among printers in Cirrato matching the <see cref="SelectedSite"/> and <see cref="SelectedBuilding"/>.
        /// </summary>
        /// <remarks>
        /// This list is cleared and populated when <see cref="SelectedSite"/> is changed, based on the printers in <see cref="CirratoPrinters"/> that match the new site. Those are split on the underscore symbol, using only the second part.
        /// </remarks>
        public List<string> Floors
        {
            get => _floors;
            set => Set(nameof(Floors), ref _floors, value);
        }

        #endregion

        #region Printer Info

        private string _selectedRegion;
        /// <summary>
        /// The chosen region in Cirrato for the printer being created.
        /// </summary>
        public string SelectedRegion
        {
            get { return _selectedRegion; }
            set
            {
                Set(nameof(SelectedRegion), ref _selectedRegion, value);
            }
        }

        private string _ipAddress = "";
        /// <summary>
        /// The IP address of the printer to create.
        /// </summary>
        /// <remarks>
        /// This is validated to match the format of an IP address.
        /// </remarks>
        [CustomValidation(typeof(CreatePrinterViewModel), "ValidateIPAddress")]
        public string IPAddress
        {
            get { return _ipAddress; }
            set
            {
                Set(nameof(IPAddress), ref _ipAddress, value);

                if (value != null)
                {
                    ValidateAsync();
                }
            }
        }

        private string _selectedDriver;
        /// <summary>
        /// The chosen driver of the printer being created.
        /// </summary>
        public string SelectedDriver
        {
            get { return _selectedDriver; }
            set
            {
                Set(nameof(SelectedDriver), ref _selectedDriver, value);

                // Get configurations for driver and add an empty option as the first element as option for default configuration
                ConfigurationList = PrinterRepository.DriverList.Where(d => d.Name == value).Select(d => d.ConfigurationList).FirstOrDefault().Select(c => c.Name).ToList();
                // Make sure user can select no configuration if they accidentally choose one and want to revert
                ConfigurationList.Insert(0, "[None]");
                SelectedConfiguration = "[None]";
            }
        }

        private string _printerComment;
        /// <summary>
        /// The comment to add to the printer being created, suggested information is printer model.
        /// </summary>
        /// <remarks>
        /// This property is validated to not be empty, null or whitespace.
        /// </remarks>
        [CustomValidation(typeof(CreatePrinterViewModel), "ValidatePrinterComment")]
        public string PrinterComment
        {
            get { return _printerComment; }
            set
            {
                Set(nameof(PrinterComment), ref _printerComment, value);

                if (value != null)
                {
                    ValidateAsync();
                }
            }
        }

        private string _printerLocation;
        /// <summary>
        /// The location of the printer being created, suggested information is building, floor and room.
        /// </summary>
        /// <remarks>
        /// This property is validated to not be empty, null or whitespace.
        /// </remarks>
        [CustomValidation(typeof(CreatePrinterViewModel), "ValidatePrinterLocation")]
        public string PrinterLocation
        {
            get { return _printerLocation; }
            set
            {
                Set(nameof(PrinterLocation), ref _printerLocation, value);

                if (value != null)
                {
                    ValidateAsync();
                }
            }
        }

        #endregion

        private bool _createInSysMan = true;
        /// <summary>
        /// Whether or not the printer should be created in SysMan.
        /// </summary>
        /// <remarks>
        /// The SysMan URL is set in App.config.
        /// </remarks>
        public bool CreateInSysMan
        {
            get => _createInSysMan;
            set
            {
                Set(nameof(CreateInSysMan), ref _createInSysMan, value);

                ValidateAsync();
            }
        }
        
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
            set
            {
                Set(nameof(Loading), ref _loading, value);
            }
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
        /// The command run when the create printer button is clicked.
        /// </summary>
        public ICommand CreatePrinterCommand { get; private set; }

        /// <summary>
        /// The command run to quit the application.
        /// </summary>
        public ICommand ExitCommand { get; private set; }

        /// <summary>
        /// The command run to return to the menu.
        /// </summary>
        public ICommand MenuCommand { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatePrinterViewModel"/> class.
        /// </summary>
        /// <param name="ds">The implementation of <see cref="IDialogService"/> provided to handle dialogs in MVVM.</param>
        public CreatePrinterViewModel(IDialogService ds)
        {
            _persistentMessage = false;

            _dialogService = ds;
        }

        /// <summary>
        /// Validates that the IP address matches the correct format.
        /// </summary>
        /// <param name="obj">Object of the validation.</param>
        /// <param name="context">Context of the validation.</param>
        /// <returns>Returns validation result.</returns>
        public static ValidationResult ValidateIPAddress(object obj, ValidationContext context)
        {
            string ipAddress = ((CreatePrinterViewModel)context.ObjectInstance).IPAddress;

            if (ipAddress == null)
            {
                return null;
            }

            Match match = Regex.Match(ipAddress, "^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
            if (!match.Success)
            {
                return new ValidationResult("Enter IP address in correct format.", new List<string> { "IPAddress" });
            }

            // Check if IP is in use by another printer
            Printer printer = PrinterRepository.PrinterList.Where(p => p.IP == ipAddress).FirstOrDefault();
            if (printer != null)
            {
                return new ValidationResult($"IP already in use by printer {printer.Name}.", new List<string> { "IPAddress" });
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that the printer name is not empty, null or whitespace.
        /// </summary>
        /// <param name="obj">Object of the validation.</param>
        /// <param name="context">Context of the validation.</param>
        /// <returns>Returns validation result.</returns>
        public static ValidationResult ValidatePrinterName(object obj, ValidationContext context)
        {
            string printerName = ((CreatePrinterViewModel)context.ObjectInstance).PrinterName;
            if (string.IsNullOrWhiteSpace(printerName))
            {
                return new ValidationResult("Enter a valid printer name.", new List<string> { "PrinterName" });
            }

            // If the printer already exists in Cirrato
            if (PrinterRepository.PrinterList.Any(p => p.Name == printerName))
            {
                return new ValidationResult($"Printer {printerName} already exists in Cirrato.", new List<string> { "PrinterName" });
            }

            // If the printer already exists in SysMan
            if (((CreatePrinterViewModel)context.ObjectInstance).CreateInSysMan)
            {
                if (PrinterRepository.SysManPrinterList.Any(p => p.Name == printerName))
                {
                    return new ValidationResult($"Printer {printerName} already exists in SysMan.", new List<string> { "PrinterName" });
                }
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that the printer comment is not empty, null or whitespace.
        /// </summary>
        /// <param name="obj">Object of the validation.</param>
        /// <param name="context">Context of the validation.</param>
        /// <returns>Returns validation result.</returns>
        public static ValidationResult ValidatePrinterComment(object obj, ValidationContext context)
        {
            string printerComment = ((CreatePrinterViewModel)context.ObjectInstance).PrinterComment;
            if (string.IsNullOrWhiteSpace(printerComment))
            {
                return new ValidationResult("Printer comment is required.", new List<string> { "PrinterComment" });
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that the printer location is not empty, null or whitespace.
        /// </summary>
        /// <param name="obj">Object of the validation.</param>
        /// <param name="context">Context of the validation.</param>
        /// <returns>Returns validation result.</returns>
        public static ValidationResult ValidatePrinterLocation(object obj, ValidationContext context)
        {
            string printerLocation = ((CreatePrinterViewModel)context.ObjectInstance).PrinterLocation;
            if (string.IsNullOrWhiteSpace(printerLocation))
            {
                return new ValidationResult("Printer location is required.", new List<string> { "PrinterLocation" });
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Gets data from <see cref="PrinterRepository"/>.
        /// </summary>
        public void Initialize()
        {
            CreatePrinterCommand = new RelayCommand(async () => await CreatePrinterAsync(), () => CanCreatePrinter());
            ExitCommand = new RelayCommand(() => Application.Current.MainWindow.Close());
            
            RefreshLoading();
            LoadPrinterData();
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
        /// Creates the printer asynchronously.
        /// </summary>
        /// <remarks>
        /// Adds the printer to the lists in <see cref="PrinterRepository"/> if successfully created. 
        /// </remarks>
        private async Task CreatePrinterAsync()
        {
            var csb = new StringBuilder();
            csb.AppendLine($"Printer Name: {PrinterName}");
            csb.AppendLine($"Cirrato Region: {SelectedRegion}");
            csb.AppendLine($"IP Address: {IPAddress}");
            csb.AppendLine($"Driver: {SelectedDriver}");
            csb.AppendLine($"Configuration: {SelectedConfiguration}");
            csb.AppendLine($"Comment: {PrinterComment}");
            csb.AppendLine($"Location: {PrinterLocation}");
            csb.AppendLine($"Create in SysMan: {CreateInSysMan.ToString()}");

            var confirmationBox = new ConfirmationBoxViewModel("Do you want to create the following printer?", csb.ToString());
            
            // let user confirm info of printer to create
            bool? confirmation = _dialogService.ShowDialog(this, confirmationBox);
            if (confirmation == true)
            {
                AddLoadingOperation("CreatePrinter", "Creating printer...");

                bool cirratoSuccess = await CreateCirratoPrinter();
                bool sysmanSuccess = false;
                if (CreateInSysMan)
                {
                    sysmanSuccess = await CreateSysManPrinter();
                }

                StringBuilder sb = new StringBuilder();

                // If at least one printer creation succeeded
                if (cirratoSuccess || sysmanSuccess)
                {
                    sb.Append("Created printer in ");

                    if (cirratoSuccess)
                    {
                        sb.Append("Cirrato ");

                        if (sysmanSuccess)
                        {
                            sb.Append("and SysMan");
                        }
                    }
                    else if (sysmanSuccess)
                    {
                        sb.Append("SysMan ");
                    }

                    // If the printer creation only succeeded in one of the systems
                    if (!(cirratoSuccess && sysmanSuccess) && (sysmanSuccess || cirratoSuccess))
                    {
                        sb.Append("only");
                    }

                    // If printer is successfully created in one of the systems, clear fields and update list.
                    Refresh();
                }
                else
                {
                    sb.Append("Failed to create printer");
                }

                sb.Append("!");

                RemoveLoadingOperation("CreatePrinter");

                await SetPersistentMessage(sb.ToString(), 2000);
            }
        }

        /// <summary>
        /// Clears all properties used for printer creation and update list with new printer.
        /// </summary>
        public void Refresh()
        {
            Keyboard.ClearFocus();
            IPAddress = "";
            PrinterComment = "";
            PrinterLocation = "";
            CirratoPrinters = PrinterRepository.PrinterList.OrderBy(p => p.Name).Select(p => p.Name).ToList();
            SetNames();
        }

        /// <summary>
        /// Create printer in Cirrato.
        /// </summary>
        /// <returns>
        /// Whether or not the printer was created successfully.
        /// </returns>
        private async Task<bool> CreateCirratoPrinter()
        {
            AddLoadingOperation("CirratoPrinter", "Creating printer in Cirrato...");

            // Assume the printer is created successfully.
            bool success = true;

            try
            {
                string result = await PrinterRepository.CreatePrinterAsync($"{SelectedRegion}\\{PrinterName}", IPAddress, SelectedDriver, PrinterComment, PrinterLocation);

                // If the PMC output is not a success, throw error message
                if (!result.StartsWith("[OK]"))
                {
                    Logger.Log($"Failed to create printer in Cirrato! Error message:\n{result}", System.Diagnostics.EventLogEntryType.Error);
                    MessageBox.Show($"Failed to create printer in Cirrato! Error message:\n{result}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    success = false;
                }
                else
                {
                    // Get ID of printer to save, from output [OK] "aabbccdd-eeee-ffff-1111-2233445566"
                    string id = result.Substring(result.IndexOf('"'));
                    id = id.Trim('"');

                    // Get currently selected driver and region with full info
                    Driver driver = PrinterRepository.DriverList.Where(d => d.Name == SelectedDriver).FirstOrDefault();
                    Region region = PrinterRepository.RegionList.Where(r => r.Name == SelectedRegion).FirstOrDefault();

                    Printer newPrinter = new Printer
                    {
                        Name = PrinterName,
                        ExistsInSysMan = false,
                        Description = PrinterComment,
                        Location = PrinterLocation,
                        Driver = driver,
                        IP = IPAddress,
                        Region = region,
                        CirratoID = id,
                        SysManID = "",
                        Configuration = SelectedConfiguration == "[None]" ? "" : SelectedConfiguration // Set configuration to empty if none
                    };
                    // Add new Cirrato printer to list
                    PrinterRepository.AddPrinter(newPrinter);
                    
                    Logger.Log($"Created printer in Cirrato with information:\n{newPrinter.ToString()}", System.Diagnostics.EventLogEntryType.Information);

                    // Add configuration to queue of the newly created printer if user selected a config
                    if (!string.IsNullOrWhiteSpace(SelectedConfiguration == "[None]" ? "" : SelectedConfiguration))
                    {
                        string configID = driver.ConfigurationList.Where(c => c.Name == SelectedConfiguration).Select(c => c.CirratoID).FirstOrDefault();

                        List<string> configStrings = new List<string>();
                        // Make a list of strings for PMC input with operating system ID and configuration ID separated by colon
                        foreach (string os in driver.DeployedOperatingSystems)
                        {
                            configStrings.Add($"{os}:{configID}");
                        }

                        string queueResult = await PrinterRepository.AddQueueConfigurationAsync($"{region.Name}\\{PrinterName}", configStrings);

                        if (!queueResult.StartsWith("[OK]"))
                        {
                            Logger.Log($"Failed to add configuration {SelectedConfiguration} for operating systems {string.Join(", ", driver.DeployedOperatingSystems)} to printer queue {PrinterName} in Cirrato! Error message:\n{queueResult}", System.Diagnostics.EventLogEntryType.Error);
                            MessageBox.Show($"Failed to add configuration {SelectedConfiguration} for operating systems {string.Join(", ", driver.DeployedOperatingSystems)} to printer queue {PrinterName} in Cirrato! Error message:\n{queueResult}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            success = false;
                        }
                    }
                }
            }
            catch (InvalidOperationException iopex)
            {
                Logger.Log($"Path to Cirrato PMC not set correctly, check .config file! Error message:\n{iopex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                MessageBox.Show($"Path to Cirrato PMC not set correctly, check .config file! Error message:\n{iopex.GetFullMessage()}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.MainWindow.Close();
                return false;
            }
            catch (Win32Exception w32ex)
            {
                Logger.Log($"Error opening file \"{PrinterRepository.CirratoPath}\". Error message:\n{w32ex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                MessageBox.Show($"Error opening file \"{PrinterRepository.CirratoPath}\". Error message:\n{w32ex.GetFullMessage()}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.MainWindow.Close();
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error starting Cirrato PMC process. Error message:\n{ex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                MessageBox.Show($"Error starting Cirrato PMC process. Error message:\n{ex.GetFullMessage()}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.MainWindow.Close();
                return false;
            }

            RemoveLoadingOperation("CirratoPrinter");

            return success;
        }

        /// <summary>
        /// Create printer in SysMan.
        /// </summary>
        /// <returns>
        /// Returns whether or not the printer was created.
        /// </returns>
        private async Task<bool> CreateSysManPrinter()
        {
            AddLoadingOperation("SysManPrinter", "Creating printer in SysMan...");

            // Assume the printer is created successfully
            bool success = true;

            // Create the printer in SysMan
            string result = await SysManManager.CreatePrinterAsync(PrinterName, PrinterComment, PrinterLocation);

            // The printer creation failed if the return value is not a number, the ID of the printer created.
            if (!int.TryParse(result, out int id))
            {
                success = false;
                Logger.Log($"Failed to create printer in SysMan! SysMan response:\n{result}", System.Diagnostics.EventLogEntryType.Error);
                MessageBox.Show($"Failed to create printer in SysMan! SysMan response:\n{result}", "SysMan Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                SysManPrinter newSysManPrinter = new SysManPrinter
                {
                    Name = PrinterName,
                    CanBeDefault = true,
                    CanBeRemoved = false,
                    Description = PrinterComment,
                    Server = "Cirrato",
                    Location = PrinterLocation,
                    ID = id,
                    Tag = ""
                };

                PrinterRepository.AddSysManPrinter(newSysManPrinter);

                var existingPrinterIndex = PrinterRepository.PrinterList.FindIndex(p => p.Name == newSysManPrinter.Name);
                if (existingPrinterIndex >= 0 && existingPrinterIndex < PrinterRepository.PrinterList.Count)
                {
                    PrinterRepository.PrinterList[existingPrinterIndex].SysManID = id.ToString();
                    PrinterRepository.PrinterList[existingPrinterIndex].ExistsInSysMan = true;
                }

                Logger.Log($"Created printer in SysMan with information:\n{newSysManPrinter.ToString()}", System.Diagnostics.EventLogEntryType.Information);
            }

            RemoveLoadingOperation("SysManPrinter");

            return success;
        }
        
        /// <summary>
        /// Checks that the printer can be created with the current information filled in and the current state of the application.
        /// </summary>
        /// <returns>Returns if the printer can be created.</returns>
        /// <remarks>
        /// This method is set as CanExecute for the <see cref="RelayCommand"/> <see cref="CreatePrinterCommand"/>.
        /// The printer cannot be created as long as there are background operations, or if any information is missing or invalid.
        /// </remarks>
        private bool CanCreatePrinter()
        {
            if (HasErrors || Loading || DomainManager.ReadOnlyAccess)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Display a status message regardless of ongoing loading operations.
        /// </summary>
        /// <param name="message">The message to be displayed as status message.</param>
        /// <param name="duration">The amount of time to display the message in milliseconds.</param>
        private async Task SetPersistentMessage(string message, int duration)
        {
            _persistentMessage = true;
            LoadingText = message;

            await Task.Delay(duration).ContinueWith(t =>
            {
                _persistentMessage = false;
                RefreshLoading();
            });
        }
        
        /// <summary>
        /// Sets property values with data from Cirrato.
        /// </summary>
        private void LoadPrinterData()
        {
            AddLoadingOperation("LoadPMCData", "Loading data from Cirrato...");

            CirratoDrivers = PrinterRepository.DriverList.Select(d => d.Name).ToList();
            CirratoRegions = PrinterRepository.RegionList.Select(r => r.Name).ToList();
            CirratoPrinters = PrinterRepository.PrinterList.Select(p => p.Name).ToList();

            SetSites();

            RemoveLoadingOperation("LoadPMCData");
        }
        
        /// <summary>
        /// Updates <see cref="Loading"/> and <see cref="LoadingText"/>.
        /// </summary>
        public void RefreshLoading()
        {
            Loading = LoadingHandler.Loading;
            if (!_persistentMessage)
            {
                LoadingText = LoadingHandler.CurrentStatus;
            }
        }

        /// <summary>
        /// Sets the external commands from the parent View Model.
        /// </summary>
        /// <param name="command">The command to set for the return arrow button.</param>
        public void SetCommands(ICommand command)
        {
            MenuCommand = command;
        }

        /// <summary>
        /// Populates <see cref="Sites"/> with all sites found among printers in Cirrato.
        /// </summary>
        private void SetSites()
        {
            List<string> sites = new List<string>();

            // Select only site part of printer name, from printers containing 3 underscores since they most likely follow the printer name standard
            foreach (string s in CirratoPrinters.Where(p => (p.Count(c => c == '_')) == 3))
            {
                string site = s.Split('_')[0];
                if (!sites.Contains(site))
                {
                    sites.Add(site);
                }
            }

            Sites = sites;
        }

        /// <summary>
        /// Populates <see cref="Buildings"/> with all buildings found among printers in Cirrato that match <see cref="SelectedSite"/>.
        /// </summary>
        private void SetBuildings()
        {
            List<string> buildings = new List<string>();
            
            // Select only building part of printer name, filter on those that match selected site
            foreach (string b in CirratoPrinters.Where(prt => prt.Split('_')[0] == SelectedSite))
            {
                string building = b.Split('_')[1];
                if (!buildings.Contains(building))
                {
                    buildings.Add(building);
                }
            }

            Buildings = buildings;
        }

        /// <summary>
        /// Populates <see cref="Floors"/> with all floors found among printers in Cirrato that match <see cref="SelectedSite"/> and <see cref="SelectedBuilding"/>.
        /// </summary>
        private void SetFloors()
        {
            List<string> floors = new List<string>();

            // Select only floor part of printer name, filter on those that match selected site and building
            foreach (string f in CirratoPrinters.Where(prt => prt.Split('_')[0] == SelectedSite &&
            prt.Split('_')[1] == SelectedBuilding))
            {
                string floor = f.Split('_')[2];
                if (!floors.Contains(floor))
                {
                    floors.Add(floor);
                }
            }

            Floors = floors;
        }

        /// <summary>
        /// Updates <see cref="SuggestedName"/> based on selection and sets <see cref="PrinterName"/> based on first free number if all parts are selected.
        /// </summary>
        private void SetNames()
        {
            StringBuilder sb = new StringBuilder();

            // name for "hint text" if all fields are not selected
            string nameSuggestion = "";

            // name for text if all fields are selected
            string printerName = "";

            // if something is selected in the site combobox
            if (SelectedSite != null)
            {
                // set the name to "SITE_"
                sb.Append(SelectedSite);
                sb.Append('_');

                // if something is selected in the building combobox
                if (SelectedBuilding != null)
                {
                    // set the name to "SITE_BUILDING_"
                    sb.Append(SelectedBuilding);
                    sb.Append('_');

                    // if something is selected in the floor combobox
                    if (SelectedFloor != null)
                    {
                        // set the name to "SITE_BUILDING_FLOOR_"
                        sb.Append(SelectedFloor);
                        sb.Append('_');

                        // get list of printers that match the current "SITE_BUILDING_FLOOR_XX" format, sort them and get the XX part as a list
                        List<string> takenNumbers = CirratoPrinters.Where(prt => prt.Split('_')[0] == SelectedSite &&
                        prt.Split('_')[1] == SelectedBuilding &&
                        prt.Split('_')[2] == SelectedFloor).OrderBy(prt => prt.Length).Select(prt => prt.Split('_')[3]).ToList();

                        // see what the first free number is, starting from 01
                        int count = 0;
                        if (takenNumbers.Count > 0)
                        {
                            while(count < takenNumbers.Count)
                            {
                                if (takenNumbers[count] != (count + 1).ToString("00"))
                                {
                                    break;
                                }
                                count++;
                            }
                        }

                        // set the name to "SITE_BUILDING_FLOOR_XX" where XX is the number found
                        sb.Append((count + 1).ToString("00"));

                        // since the whole name is found, we set the printername used for text and not just the hint property
                        printerName = sb.ToString();
                    }
                    else
                    {
                        sb.Append("XX_XX");
                    }
                }
                else
                {
                    sb.Append("XX_XX_XX");
                }
            }
            else
            {
                sb.Append("XX_XX_XX_XX");
            }

            nameSuggestion = sb.ToString();

            // printerName is either empty or the whole printer name with the first free number matching SITE_BUILDING_FLOOR_XX
            PrinterName = printerName;

            // SuggestedName is set after each combobox selection, differing depending on how many parts of the name are selected
            SuggestedName = nameSuggestion;
        }        
    }
}
