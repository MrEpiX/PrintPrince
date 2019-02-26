using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PrintPrince.ViewModels
{
    /// <summary>
    /// The ViewModel of the main menu.
    /// </summary>
    public class MainMenuViewModel : ValidatableViewModelBase
    {
        /// <summary>
        /// The command for the Create button to bind to in the view.
        /// </summary>
        public ICommand CreateCommand { get; private set; }

        /// <summary>
        /// The command for the List button to bind to in the view.
        /// </summary>
        public ICommand ListCommand { get; private set; }

        /// <summary>
        /// Initializes an instance of the <see cref="MainMenuViewModel"/> class.
        /// </summary>
        public MainMenuViewModel(){}

        /// <summary>
        /// Sets the commands for the buttons in the view.
        /// </summary>
        /// <param name="createCommand">The command to run to navigate to the Create Printer view.</param>
        /// <param name="listCommand">The command to run to navigate to the List Printers view.</param>
        public void SetCommands(ICommand createCommand, ICommand listCommand)
        {
            CreateCommand = createCommand;
            ListCommand = listCommand;
        }
    }
}
