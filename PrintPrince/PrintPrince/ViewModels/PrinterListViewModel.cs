using GalaSoft.MvvmLight.CommandWpf;
using MvvmDialogs;
using PrintPrince.Models;
using PrintPrince.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PrintPrince.ViewModels
{
    /// <summary>
    /// ViewModel used to list existing printers.
    /// </summary>
    public class PrinterListViewModel : ValidatableViewModelBase
    {
        /// <summary>
        /// Service that handles the dialogs as MVVM through <see cref="MvvmDialogs"/>.
        /// </summary>
        private readonly IDialogService _dialogService;

        private List<Printer> _printerList;
        /// <summary>
        /// List of all printers to show in view.
        /// </summary>
        public List<Printer> PrinterList
        {
            get => _printerList;
            private set => Set(nameof(PrinterList), ref _printerList, value);
        }

        private int _listIndex;
        /// <summary>
        /// Index of currently selected printer in list.
        /// </summary>
        public int ListIndex
        {
            get => _listIndex;
            set
            {
                Set(nameof(ListIndex), ref _listIndex, value);
            }
        }

        private string _filter;
        /// <summary>
        /// User input to filter out printers.
        /// </summary>
        public string Filter
        {
            get => _filter;
            set
            {
                Set(nameof(Filter), ref _filter, value);
                FilterPrinters();
            }
        }

        private string _selectedFilterProperty;
        /// <summary>
        /// Property to filter by.
        /// </summary>
        public string SelectedFilterProperty
        {
            get => _selectedFilterProperty;
            set
            {
                Set(nameof(SelectedFilterProperty), ref _selectedFilterProperty, value);

                // Reset filter
                Filter = "";
            }
        }

        private List<Printer> _filteredPrinters;
        /// <summary>
        /// List of printers that match <see cref="Filter"/>.
        /// </summary>
        public List<Printer> FilteredPrinters
        {
            get => _filteredPrinters;
            private set => Set(nameof(FilteredPrinters), ref _filteredPrinters, value);
        }

        private List<string> _propertyList;
        /// <summary>
        /// List of properties to filter enable filtering by.
        /// </summary>
        public List<string> PropertyList
        {
            get => _propertyList;
            private set => Set(nameof(PropertyList), ref _propertyList, value);
        }

        /// <summary>
        /// The command run to return to the menu.
        /// </summary>
        public ICommand MenuCommand { get; set; }

        /// <summary>
        /// The command run to show details of selected printer.
        /// </summary>
        public ICommand ShowDetailsCommand { get; set; }

        /// <summary>
        /// Creates a new instace of the <see cref="PrinterListViewModel"/> class.
        /// </summary>
        public PrinterListViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            ListIndex = -1;
            Filter = "";
            PrinterList = new List<Printer>();
            FilteredPrinters = new List<Printer>();

            // Disable button if listindex is unselected
            ShowDetailsCommand = new RelayCommand(() => ShowPrinterDetails(), () => { return (ListIndex >= 0 && ListIndex < PrinterList.Count) ? true : false; });
            PropertyList = new List<string> { "Name", "IP", "Driver", "Region", "Description", "Location" } ;
        }

        /// <summary>
        /// Initializes the <see cref="PrinterListViewModel"/>.
        /// </summary>
        public void Initialize()
        {
            Filter = "";
            ListIndex = -1;
            PrinterList = new List<Printer>();
            PrinterList = PrinterRepository.PrinterList;
            FilteredPrinters = new List<Printer>();
            FilteredPrinters = PrinterList;
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
        /// Filters printers based on user input.
        /// </summary>
        /// <remarks>
        /// Clears current selection.
        /// </remarks>
        private void FilterPrinters()
        {
            if (PrinterList != null)
            {
                ListIndex = -1;

                // Get list of printers where the chosen property being filtered matches the filter
                // Gets Property by name based on SelectedFilterProperty, gets value of property, converts to lowercase string
                // Checks if it contains the filter in lowercase and converts it to list
                if (SelectedFilterProperty == "Driver" || SelectedFilterProperty == "Region")
                {
                    FilteredPrinters = PrinterList.Where(p => p.GetType().GetProperty(SelectedFilterProperty).GetValue(p).GetType().GetProperty("Name").GetValue(p.GetType().GetProperty(SelectedFilterProperty).GetValue(p)).ToString().ToLower().Contains(Filter.ToLower())).OrderBy(p => p.Name).ToList();
                }
                else
                {
                    FilteredPrinters = PrinterList.Where(p => p.GetType().GetProperty(SelectedFilterProperty).GetValue(p).ToString().ToLower().Contains(Filter.ToLower())).OrderBy(p => p.Name).ToList();
                }
            }
        }

        /// <summary>
        /// Opens a new window with the details of the selected printer.
        /// </summary>
        private void ShowPrinterDetails()
        {
            // make sure printerlist is filled
            if (PrinterList != null)
            {
                if (PrinterList.Count > 0)
                {
                    var printerDetails = new PrinterDetailsViewModel(_dialogService,
                        FilteredPrinters[ListIndex],
                        PrinterRepository.SysManPrinterList.Where(p => p.Name == FilteredPrinters[ListIndex].Name).FirstOrDefault(),
                        PrinterRepository.DriverList,
                        PrinterRepository.RegionList);

                    bool? result = _dialogService.ShowDialog(this, printerDetails);

                    // refresh list in case user created printer in sysman, updated info or deleted printer
                    PrinterList = new List<Printer>();
                    PrinterList = PrinterRepository.PrinterList;
                    // re-creating list is a reliable way to update GUI
                    FilteredPrinters = new List<Printer>();
                    FilteredPrinters = PrinterList;
                    Filter = "";
                    ListIndex = -1;
                }
            }
        }
    }
}