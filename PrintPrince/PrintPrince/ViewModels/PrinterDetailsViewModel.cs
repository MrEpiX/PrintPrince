using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using MvvmDialogs;
using PrintPrince.Models;
using PrintPrince.Services;
using System;
using System.Collections.Generic;
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
    /// ViewModel used for the dialog to let the user confirm printer creation, implementing <see cref="IModalDialogViewModel"/> from <see cref="MvvmDialogs"/>.
    /// </summary>
    public class PrinterDetailsViewModel : ValidatableViewModelBase, IModalDialogViewModel
    {
        /// <summary>
        /// Service that handles the dialogs as MVVM through <see cref="MvvmDialogs"/>.
        /// </summary>
        private readonly IDialogService _dialogService;

        private Printer _currentCirratoPrinter;
        /// <summary>
        /// Current printer being modified.
        /// </summary>
        public Printer CurrentCirratoPrinter
        {
            get => _currentCirratoPrinter;
            set => Set(nameof(CurrentCirratoPrinter), ref _currentCirratoPrinter, value);
        }

        private SysManPrinter _currentSysManPrinter;
        /// <summary>
        /// Current printer being modified.
        /// </summary>
        public SysManPrinter CurrentSysManPrinter
        {
            get => _currentSysManPrinter;
            set => Set(nameof(CurrentSysManPrinter), ref _currentSysManPrinter, value);
        }

        private bool _sysmanPrinterExists;
        /// <summary>
        /// If the printer exists in Sysman.
        /// </summary>
        /// <remarks>
        /// This is used for visibility for the list of SysMan target installations of the printer.
        /// </remarks>
        public bool SysManPrinterExists
        {
            get => _sysmanPrinterExists;
            set => Set(nameof(SysManPrinterExists), ref _sysmanPrinterExists, value);
        }

        private bool _sysmanPrinterNotExists;
        /// <summary>
        /// If the printer does not exist in Sysman.
        /// </summary>
        /// <remarks>
        /// This is used for visibility for the button to create the printer in SysMan.
        /// </remarks>
        public bool SysManPrinterNotExists
        {
            get => _sysmanPrinterNotExists;
            set => Set(nameof(SysManPrinterNotExists), ref _sysmanPrinterNotExists, value);
        }

        private string _sysmanIDText;
        /// <summary>
        /// Text to show ID of printer in SysMan.
        /// </summary>
        public string SysManIDText
        {
            get => _sysmanIDText;
            set => Set(nameof(SysManIDText), ref _sysmanIDText, value);
        }
        private string _sysmanDescription;
        /// <summary>
        /// Description of printer in Sysman.
        /// </summary>
        [CustomValidation(typeof(PrinterDetailsViewModel), "ValidateSysManDescription")]
        public string SysManDescription
        {
            get => _sysmanDescription;
            set
            {
                Set(nameof(SysManDescription), ref _sysmanDescription, value);

                if (value != null)
                {
                    ValidateAsync();
                }
            }
        }

        private string _sysmanLocation;
        /// <summary>
        /// Location of printer in Sysman.
        /// </summary>
        [CustomValidation(typeof(PrinterDetailsViewModel), "ValidateSysManLocation")]
        public string SysManLocation
        {
            get => _sysmanLocation;
            set
            {
                Set(nameof(SysManLocation), ref _sysmanLocation, value);

                if (value != null)
                {
                    ValidateAsync();
                }
            }
        }

        private List<string> _installationTargets;
        /// <summary>
        /// Target installations of printer in Sysman.
        /// </summary>
        public List<string> InstallationTargets
        {
            get => _installationTargets;
            set => Set(nameof(InstallationTargets), ref _installationTargets, value);
        }

        private string _cirratoIDText;
        /// <summary>
        /// Text to show ID of printer in Cirrato.
        /// </summary>
        public string CirratoIDText
        {
            get => _cirratoIDText;
            set => Set(nameof(CirratoIDText), ref _cirratoIDText, value);
        }

        private string _printerName;
        /// <summary>
        /// Name of printer in Cirrato.
        /// </summary>
        [CustomValidation(typeof(PrinterDetailsViewModel), "ValidatePrinterName")]
        public string PrinterName
        {
            get => _printerName;
            set
            {
                Set(nameof(PrinterName), ref _printerName, value);

                if (value != null)
                {
                    ValidateAsync();
                }
            }
        }

        private string _ip;
        /// <summary>
        /// IP of printer in Cirrato.
        /// </summary>
        [CustomValidation(typeof(PrinterDetailsViewModel), "ValidateIPAddress")]
        public string IP
        {
            get => _ip;
            set
            {
                Set(nameof(IP), ref _ip, value);

                if (value != null)
                {
                    ValidateAsync();
                }
            }
        }

        private string _description;
        /// <summary>
        /// Description of printer in Cirrato.
        /// </summary>
        [CustomValidation(typeof(PrinterDetailsViewModel), "ValidatePrinterDescription")]
        public string Description
        {
            get => _description;
            set
            {
                Set(nameof(Description), ref _description, value);

                if (value != null)
                {
                    ValidateAsync();
                }
            }
        }

        private string _location;
        /// <summary>
        /// Location of printer in Cirrato.
        /// </summary>
        [CustomValidation(typeof(PrinterDetailsViewModel), "ValidatePrinterLocation")]
        public string Location
        {
            get => _location;
            set
            {
                Set(nameof(Location), ref _location, value);

                if (value != null)
                {
                    ValidateAsync();
                }
            }
        }

        private List<string> _regionList;
        /// <summary>
        /// List of all regions in Cirrato.
        /// </summary>
        public List<string> RegionList
        {
            get => _regionList;
            set => Set(nameof(RegionList), ref _regionList, value);
        }

        private List<string> _driverList;
        /// <summary>
        /// List of all drivers in Cirrato.
        /// </summary>
        public List<string> DriverList
        {
            get => _driverList;
            set => Set(nameof(DriverList), ref _driverList, value);
        }

        private string _selectedRegion;
        /// <summary>
        /// Currently selected region of printer.
        /// </summary>
        public string SelectedRegion
        {
            get => _selectedRegion;
            set => Set(nameof(SelectedRegion), ref _selectedRegion, value);
        }

        private string _selectedDriver;
        /// <summary>
        /// Currently selected driver of printer.
        /// </summary>
        public string SelectedDriver
        {
            get => _selectedDriver;
            set => Set(nameof(SelectedDriver), ref _selectedDriver, value);
        }

        private bool _loading;
        /// <summary>
        /// Whether or not there are any background operations going on.
        /// </summary>
        public bool Loading
        {
            get => _loading;
            set
            {
                Set(nameof(Loading), ref _loading, value);

                // force GUI refresh after status is set
                // otherwise controls dependent on this through the RelayCommand.CanExecute (the create printer button) will not show correct enabled state until GUI is clicked
                // https://stackoverflow.com/questions/2331622/weird-problem-where-button-does-not-get-re-enabled-unless-the-mouse-is-clicked
                CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// The command for the Save button to bind to in the dialog.
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// The command for the Cancel button to bind to in the dialog.
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// The command for the Delete button to bind to in the dialog.
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// The command for the Create in SysMan button to bind to in the dialog.
        /// </summary>
        public ICommand CreateCommand { get; }

        private bool? _dialogResult;
        /// <summary>
        /// The result of the dialog depending on what button is clicked.
        /// </summary>
        public bool? DialogResult
        {
            get => _dialogResult;
            private set => Set(nameof(DialogResult), ref _dialogResult, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrinterDetailsViewModel"/> class.
        /// </summary>
        [PreferredConstructor] // there needs to be a preferred, parameterless constructor for the MVVMLight IoC
        public PrinterDetailsViewModel() {}
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PrinterDetailsViewModel"/> class.
        /// </summary>
        /// <param name="dialogService">Service to handle dialog window.</param>
        /// <param name="printer">Cirrato Printer to edit.</param>
        /// <param name="sysmanPrinter">SysMan Printer to edit.</param>
        /// <param name="driverList">List of all drivers.</param>
        /// <param name="regionList">List of all regions.</param>
        public PrinterDetailsViewModel(IDialogService dialogService, Printer printer, SysManPrinter sysmanPrinter, List<Driver> driverList, List<Region> regionList)
        {
            Loading = false;
            _dialogService = dialogService;

            CurrentCirratoPrinter = printer;
            CurrentSysManPrinter = sysmanPrinter;

            PrinterName = printer.Name;
            IP = printer.IP;
            Description = printer.Description;
            Location = printer.Location;
            CirratoIDText = "ID: " + printer.CirratoID;

            if (sysmanPrinter != null)
            {
                SysManPrinterExists = true;
                
                SysManDescription = sysmanPrinter.Description;
                SysManLocation = sysmanPrinter.Location;
                SysManIDText = "ID: " + sysmanPrinter.ID;

                try
                {
                    // get list of targets and sort
                    Action getPrinterTargets = async () => await SetPrinterTargets();
                    getPrinterTargets.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to get or show printer installation targets from SysMan!Message: { ex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                    MessageBox.Show($"Failed to get or show printer installation targets from SysMan! Message: {ex.GetFullMessage()}", "SysMan Info Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.MainWindow.Close();
                    return;
                }
            }
            else
            {
                InstallationTargets = new List<string>();
                SysManPrinterExists = false;
            }

            SysManPrinterNotExists = !SysManPrinterExists;

            DriverList = driverList.Select(d => d.Name).ToList();
            RegionList = regionList.Select(r => r.Name).ToList();

            SelectedDriver = DriverList.Where(d => d == CurrentCirratoPrinter.Driver.Name).FirstOrDefault();
            SelectedRegion = RegionList.Where(r => r == CurrentCirratoPrinter.Region.Name).FirstOrDefault();

            // Can only save or delete if access allowes
            SaveCommand = new RelayCommand(async () => await Save(),
                () => { return !DomainManager.ReadOnlyAccess && IsPrinterModified() && !HasErrors && !Loading; });
            DeleteCommand = new RelayCommand(async () => await Delete(), () => { return !DomainManager.ReadOnlyAccess && !Loading; });
            CreateCommand = new RelayCommand(async () => await CreateInSysMan(), () => { return !DomainManager.ReadOnlyAccess && !Loading; });
            CancelCommand = new RelayCommand(() => { DialogResult = false && !Loading; });
        }

        /// <summary>
        /// Validates that the IP address matches the correct format.
        /// </summary>
        /// <param name="obj">Object of the validation.</param>
        /// <param name="context">Context of the validation.</param>
        /// <returns>Returns validation result.</returns>
        public static ValidationResult ValidateIPAddress(object obj, ValidationContext context)
        {
            string currentIPAddress = ((PrinterDetailsViewModel)context.ObjectInstance).IP;
            string newIPAddress = ((PrinterDetailsViewModel)context.ObjectInstance).IP;
            string currentPrinterName = ((PrinterDetailsViewModel)context.ObjectInstance).CurrentCirratoPrinter.Name;

            if (newIPAddress == null)
            {
                return null;
            }

            Match match = Regex.Match(newIPAddress, "^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
            if (!match.Success)
            {
                return new ValidationResult("Enter IP address in correct format.", new List<string> { "IP" });
            }

            // Check if IP is in use by another printer
            Printer printer = PrinterRepository.PrinterList.Where(p => p.IP == newIPAddress).FirstOrDefault();
            if (printer != null && printer.Name != currentPrinterName)
            {
                return new ValidationResult($"IP already in use by printer {printer.Name}.", new List<string> { "IP" });
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
            string currentPrinterName = ((PrinterDetailsViewModel)context.ObjectInstance).CurrentCirratoPrinter.Name;
            string newPrinterName = ((PrinterDetailsViewModel)context.ObjectInstance).PrinterName;
            if (string.IsNullOrWhiteSpace(newPrinterName))
            {
                return new ValidationResult("Enter a valid printer name.", new List<string> { "PrinterName" });
            }

            // If the printer already exists in Cirrato
            if (PrinterRepository.PrinterList.Any(p => p.Name == newPrinterName) && newPrinterName != currentPrinterName)
            {
                return new ValidationResult($"Printer {newPrinterName} already exists in Cirrato.", new List<string> { "PrinterName" });
            }

            // If the printer already exists in SysMan
            if (PrinterRepository.SysManPrinterList.Any(p => p.Name == newPrinterName) && newPrinterName != currentPrinterName)
            {
                return new ValidationResult($"Printer {newPrinterName} already exists in SysMan.", new List<string> { "PrinterName" });
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that the printer description is not empty, null or whitespace.
        /// </summary>
        /// <param name="obj">Object of the validation.</param>
        /// <param name="context">Context of the validation.</param>
        /// <returns>Returns validation result.</returns>
        public static ValidationResult ValidatePrinterDescription(object obj, ValidationContext context)
        {
            string printerDescription = ((PrinterDetailsViewModel)context.ObjectInstance).Description;
            if (string.IsNullOrWhiteSpace(printerDescription))
            {
                return new ValidationResult("Printer description is required.", new List<string> { "Description" });
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
            string printerLocation = ((PrinterDetailsViewModel)context.ObjectInstance).Location;
            if (string.IsNullOrWhiteSpace(printerLocation))
            {
                return new ValidationResult("Printer location is required.", new List<string> { "Location" });
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that the SysMan description is not empty, null or whitespace.
        /// </summary>
        /// <param name="obj">Object of the validation.</param>
        /// <param name="context">Context of the validation.</param>
        /// <returns>Returns validation result.</returns>
        public static ValidationResult ValidateSysManDescription(object obj, ValidationContext context)
        {
            // Only validate if the SysMan printer exists
            if (((PrinterDetailsViewModel)context.ObjectInstance).CurrentSysManPrinter != null)
            {
                string sysmanDescription = ((PrinterDetailsViewModel)context.ObjectInstance).SysManDescription;
                if (string.IsNullOrWhiteSpace(sysmanDescription))
                {
                    return new ValidationResult("Printer description is required.", new List<string> { "SysManDescription" });
                }
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that the SysMan location is not empty, null or whitespace.
        /// </summary>
        /// <param name="obj">Object of the validation.</param>
        /// <param name="context">Context of the validation.</param>
        /// <returns>Returns validation result.</returns>
        public static ValidationResult ValidateSysManLocation(object obj, ValidationContext context)
        {
            // Only validate if the SysMan printer exists
            if (((PrinterDetailsViewModel)context.ObjectInstance).CurrentSysManPrinter != null)
            {
                string sysmanLocation = ((PrinterDetailsViewModel)context.ObjectInstance).SysManLocation;
                if (string.IsNullOrWhiteSpace(sysmanLocation))
                {
                    return new ValidationResult("Printer location is required.", new List<string> { "SysManLocation" });
                }
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Sets <see cref="InstallationTargets"/> asynchronously.
        /// </summary>
        private async Task SetPrinterTargets()
        {
            try
            {
                var targets = await SysManManager.GetPrinterInstallationTargets(CurrentSysManPrinter.Name);
                targets.Sort();

                InstallationTargets = targets;
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to get or show printer installation targets from SysMan! Message:\n{ex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                MessageBox.Show($"Failed to get or show printer installation targets from SysMan! Message: {ex.GetFullMessage()}", "SysMan Info Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.MainWindow.Close();
                return;
            }
        }

        /// <summary>
        /// Saves modifications to printer in Cirrato and SysMan.
        /// </summary>
        private async Task Save()
        {
            Loading = true;

            // One stringbuilder for the confirmation box, one for the logging for each of the systems
            StringBuilder sb = new StringBuilder();
            StringBuilder sysb = new StringBuilder();

            // Create body text of confirmation window and log changed information
            if (PrinterName != CurrentCirratoPrinter.Name) { sb.AppendLine($"Name (Cirrato): {CurrentCirratoPrinter.Name} > {PrinterName}"); }
            if (IP != CurrentCirratoPrinter.IP) { sb.AppendLine($"IP Address: {CurrentCirratoPrinter.IP} > {IP}"); }
            if (Description != CurrentCirratoPrinter.Description) { sb.AppendLine($"Description (Cirrato): {CurrentCirratoPrinter.Description} > {Description}"); }
            if (Location != CurrentCirratoPrinter.Location) { sb.AppendLine($"Location (Cirrato): {CurrentCirratoPrinter.Location} > {Location}"); }
            if (SelectedDriver != CurrentCirratoPrinter.Driver.Name) { sb.AppendLine($"Driver: {CurrentCirratoPrinter.Driver.Name} > {SelectedDriver}"); }
            if (SelectedRegion != CurrentCirratoPrinter.Region.Name) { sb.AppendLine($"Region: {CurrentCirratoPrinter.Region.Name} > {SelectedRegion}"); }

            // set the contents of the Cirrato logging text
            string cirratoLogText = sb.ToString();

            if (CurrentSysManPrinter != null)
            {
                if (PrinterName != CurrentSysManPrinter.Name) { sb.AppendLine($"Name (SysMan): {CurrentSysManPrinter.Name} > {PrinterName}"); }
                if (SysManDescription != CurrentSysManPrinter.Description) { sb.AppendLine($"Description (SysMan): {CurrentSysManPrinter.Description} > {SysManDescription}"); }
                if (SysManLocation != CurrentSysManPrinter.Location) { sb.AppendLine($"Location (SysMan): {CurrentSysManPrinter.Location} > {SysManLocation}"); }
            }

            var confirmationBox = new ConfirmationBoxViewModel($"Do you want to update the printer {CurrentCirratoPrinter.Name} with the following information?", sb.ToString());

            bool? confirmation = _dialogService.ShowDialog(this, confirmationBox);

            Printer printerToChange = new Printer {
                CirratoID = CurrentCirratoPrinter.CirratoID,
                SysManID = CurrentCirratoPrinter.SysManID,
                Name = PrinterName,
                IP = IP,
                Description = Description,
                Location = Location,
                Driver = PrinterRepository.DriverList.Where(d => d.Name == SelectedDriver).FirstOrDefault(),
                Region = PrinterRepository.RegionList.Where(r => r.Name == SelectedRegion).FirstOrDefault(),
                ExistsInSysMan = CurrentCirratoPrinter.ExistsInSysMan
            };

            if (confirmation == true)
            {
                if (IsCirratoInformationModified())
                {
                    try
                    {
                        Logger.Log($"Attempting to update printer {CurrentCirratoPrinter.Name} in Cirrato:\n{cirratoLogText}", System.Diagnostics.EventLogEntryType.Information);

                        string printerResult = await PrinterRepository.ModifyPrinterAsync(printerToChange);

                        // If printer was modified successfully
                        if (printerResult.StartsWith("[OK]"))
                        {
                            Logger.Log($"Successfully updated printer {CurrentCirratoPrinter.Name}!", System.Diagnostics.EventLogEntryType.Information);
                            Logger.Log($"Attempting to modify queue {CurrentCirratoPrinter.Name} with the same information as the printer.", System.Diagnostics.EventLogEntryType.Information);

                            // Modify queue with same name too
                            string queueResult = await PrinterRepository.ModifyQueueAsync(printerToChange);
                            
                            if (queueResult.StartsWith("[OK]"))
                            {
                                Logger.Log($"Updated queue {CurrentCirratoPrinter.Name} with the same information as the printer!", System.Diagnostics.EventLogEntryType.Information);

                                // Update printer in list
                                int printerIndex = PrinterRepository.PrinterList.FindIndex(p => p.Name == CurrentCirratoPrinter.Name);
                                PrinterRepository.PrinterList[printerIndex] = printerToChange;
                                CurrentCirratoPrinter = printerToChange;
                            }
                            else
                            {
                                Logger.Log($"Modified printer, but could not modify queue in Cirrato! Error message:\n{queueResult}", System.Diagnostics.EventLogEntryType.Error);
                                MessageBox.Show($"Modified printer, but could not modify queue in Cirrato! Error message:\n{queueResult}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            Logger.Log($"Failed to modify printer in Cirrato! Error message:\n{printerResult}", System.Diagnostics.EventLogEntryType.Error);
                            MessageBox.Show($"Failed to modify printer in Cirrato! Error message:\n{printerResult}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Failed to modify printer in Cirrato! Error message:\n{ex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                        MessageBox.Show($"Failed to modify printer in Cirrato! Error message:\n{ex.GetFullMessage()}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.MainWindow.Close();
                        return;
                    }
                }

                if (IsSysManInformationModified())
                {
                    try
                    {
                        SysManPrinter newSysManPrinter = new SysManPrinter
                        {
                            Name = PrinterName,
                            ID = CurrentSysManPrinter.ID,
                            CanBeDefault = true,
                            CanBeRemoved = false,
                            Description = SysManDescription,
                            Location = SysManLocation,
                            Server = "Cirrato",
                            Tag = ""
                        };
                        
                        if (PrinterName != CurrentSysManPrinter.Name) { sysb.AppendLine($"Name (SysMan): {CurrentSysManPrinter.Name} > {PrinterName}"); }
                        if (SysManDescription != CurrentSysManPrinter.Description) { sysb.AppendLine($"Description (SysMan): {CurrentSysManPrinter.Description} > {SysManDescription}"); }
                        if (SysManLocation != CurrentSysManPrinter.Location) { sysb.AppendLine($"Location (SysMan): {CurrentSysManPrinter.Location} > {SysManLocation}"); }

                        Logger.Log($"Attempting to update printer {CurrentSysManPrinter.Name} in SysMan:\n{sysb.ToString()}", System.Diagnostics.EventLogEntryType.Information);

                        string sysmanResult = await SysManManager.ModifyPrinterAsync(newSysManPrinter);

                        Logger.Log($"Succesfully updated printer {CurrentSysManPrinter.Name}!", System.Diagnostics.EventLogEntryType.Information);

                        CurrentSysManPrinter.Name = PrinterName;
                        CurrentSysManPrinter.Description = SysManDescription;
                        CurrentSysManPrinter.Location = SysManLocation;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Failed to modify printer in SysMan! Error message:\n{ex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                        MessageBox.Show($"Failed to modify printer in SysMan! Error message:\n{ex.GetFullMessage()}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.MainWindow.Close();
                        return;
                    }
                }
            }

            Loading = false;
        }

        /// <summary>
        /// Deletes printer in Cirrato and SysMan.
        /// </summary>
        private async Task Delete()
        {
            Loading = true;

            var confirmationBox = new ConfirmationBoxViewModel($"Do you want to delete the printer {CurrentCirratoPrinter.Name}?", CurrentCirratoPrinter.ToString());

            bool? confirmation = _dialogService.ShowDialog(this, confirmationBox);

            if (confirmation == true)
            {
                try
                {
                    string result = await PrinterRepository.DeletePrinterAsync(CurrentCirratoPrinter.CirratoID);

                    // If printer was deleted successfully
                    if (result.StartsWith("[OK]"))
                    {
                        Logger.Log($"Successfully deleted printer {CurrentCirratoPrinter.Name} in Cirrato!", System.Diagnostics.EventLogEntryType.Information);
                        PrinterRepository.PrinterList.Remove(PrinterRepository.PrinterList.Where(p => p.Name == CurrentCirratoPrinter.Name).FirstOrDefault());
                    }
                    else
                    {
                        Logger.Log($"Failed to delete printer in Cirrato! Error message:\n{result}", System.Diagnostics.EventLogEntryType.Error);
                        MessageBox.Show($"Failed to delete printer in Cirrato! Error message:\n{result}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to delete printer in Cirrato! Error message:\n{ex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                    MessageBox.Show($"Failed to delete printer in Cirrato! Error message:\n{ex.GetFullMessage()}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.MainWindow.Close();
                    return;
                }

                try
                {
                    if (CurrentSysManPrinter != null)
                    {
                        // Successfully deleting printer in SysMan returns status code NoContent
                        var result = await SysManManager.DeletePrinterAsync(CurrentSysManPrinter.ID);
                        if (result == "")
                        {
                            Logger.Log($"Deleted printer {CurrentSysManPrinter.Name} in SysMan!", System.Diagnostics.EventLogEntryType.Information);
                            PrinterRepository.SysManPrinterList.Remove(PrinterRepository.SysManPrinterList.Where(p => p.Name == CurrentSysManPrinter.Name).FirstOrDefault());
                        }
                        else
                        {
                            Logger.Log($"Failed to delete printer in SysMan! Error message:\n{result}", System.Diagnostics.EventLogEntryType.Error);
                            MessageBox.Show($"Failed to delete printer in SysMan! Error message:\n{result}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to delete printer in SysMan! Error message:\n{ex.GetFullMessage()}", System.Diagnostics.EventLogEntryType.Error);
                    MessageBox.Show($"Failed to delete printer in SysMan! Error message:\n{ex.GetFullMessage()}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.MainWindow.Close();
                    return;
                }

                Loading = false;

                DialogResult = true;
            }

            Loading = false;
        }

        /// <summary>
        /// Creates printer SysMan.
        /// </summary>
        private async Task CreateInSysMan()
        {
            Loading = true;

            string name = CurrentCirratoPrinter.Name;
            string desc = CurrentCirratoPrinter.Description;
            string loc = CurrentCirratoPrinter.Location;
            
            string result = await SysManManager.CreatePrinterAsync(CurrentCirratoPrinter.Name, CurrentCirratoPrinter.Description, CurrentCirratoPrinter.Location);

            // The printer creation failed if the return value is not a number, the ID of the printer created.
            if (!int.TryParse(result, out int id))
            {
                Logger.Log($"Failed to create printer in SysMan! SysMan response:\n{result}", System.Diagnostics.EventLogEntryType.Error);
                MessageBox.Show($"Failed to create printer in SysMan! SysMan response:\n{result}", "SysMan Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                SysManPrinter newSysManPrinter = new SysManPrinter
                {
                    Name = name,
                    CanBeDefault = true,
                    CanBeRemoved = false,
                    Description = desc,
                    Server = "Cirrato",
                    Location = loc,
                    ID = id,
                    Tag = ""
                };

                PrinterRepository.AddSysManPrinter(newSysManPrinter);

                // update existing printer with SysMan ID
                var existingPrinterIndex = PrinterRepository.PrinterList.FindIndex(p => p.Name == newSysManPrinter.Name);
                if (existingPrinterIndex >= 0 && existingPrinterIndex < PrinterRepository.PrinterList.Count)
                {
                    PrinterRepository.PrinterList[existingPrinterIndex].SysManID = id.ToString();
                    PrinterRepository.PrinterList[existingPrinterIndex].ExistsInSysMan = true;
                }

                // Update properties of current printer
                CurrentSysManPrinter = newSysManPrinter;
                CurrentCirratoPrinter.SysManID = id.ToString();
                SysManDescription = desc;
                SysManLocation = loc;

                SysManPrinterExists = true;
                SysManPrinterNotExists = false;

                Logger.Log($"Created printer in SysMan with information:\n{newSysManPrinter.ToString()}", System.Diagnostics.EventLogEntryType.Information);
            }

            Loading = false;
        }

        /// <summary>
        /// Checks if printer information has been changed.
        /// </summary>
        /// <returns>Returns whether or not the printer information has been modified.</returns>
        /// <remarks>
        /// Is used as part of the CanExecute of the <see cref="SaveCommand"/>.
        /// </remarks>
        private bool IsPrinterModified()
        {
            if (PrinterName != CurrentCirratoPrinter.Name ||
                IP != CurrentCirratoPrinter.IP ||
                Description != CurrentCirratoPrinter.Description ||
                Location != CurrentCirratoPrinter.Location ||
                SelectedDriver != CurrentCirratoPrinter.Driver.Name ||
                SelectedRegion != CurrentCirratoPrinter.Region.Name)
            {
                return true;
            }
            else if (CurrentSysManPrinter != null)
            {
                if (PrinterName != CurrentSysManPrinter.Name ||
                SysManDescription != CurrentSysManPrinter.Description ||
                SysManLocation != CurrentSysManPrinter.Location)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if printer information in Cirrato has been changed.
        /// </summary>
        /// <returns>Returns whether or not the printer information has been modified.</returns>
        private bool IsCirratoInformationModified()
        {
            if (PrinterName != CurrentCirratoPrinter.Name ||
                IP != CurrentCirratoPrinter.IP ||
                Description != CurrentCirratoPrinter.Description ||
                Location != CurrentCirratoPrinter.Location ||
                SelectedDriver != CurrentCirratoPrinter.Driver.Name ||
                SelectedRegion != CurrentCirratoPrinter.Region.Name)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if printer information in SysMan has been changed.
        /// </summary>
        /// <returns>Returns whether or not the printer information has been modified.</returns>
        private bool IsSysManInformationModified()
        {
            if (CurrentSysManPrinter != null)
            {
                if (PrinterName != CurrentSysManPrinter.Name ||
                SysManDescription != CurrentSysManPrinter.Description ||
                SysManLocation != CurrentSysManPrinter.Location)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
