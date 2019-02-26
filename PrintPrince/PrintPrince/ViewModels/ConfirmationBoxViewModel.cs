using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using MvvmDialogs;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace PrintPrince.ViewModels
{
    /// <summary>
    /// ViewModel used for the dialog to let the user confirm printer creation, implementing <see cref="IModalDialogViewModel"/> from <see cref="MvvmDialogs"/>.
    /// </summary>
    public class ConfirmationBoxViewModel : ValidatableViewModelBase, IModalDialogViewModel
    {
        private string _headerText;
        /// <summary>
        /// Header text of the confirmation window, such as the question for the user to confirm.
        /// </summary>
        public string HeaderText
        {
            get => _headerText;
            set => Set(nameof(HeaderText), ref _headerText, value);
        }

        private string _bodyText;
        /// <summary>
        /// Body text of the confirmation window, such as a more detailed description.
        /// </summary>
        public string BodyText
        {
            get => _bodyText;
            set => Set(nameof(BodyText), ref _bodyText, value);
        }

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
        /// The command for the Yes button to bind to in the dialog.
        /// </summary>
        public ICommand YesCommand { get; }
        
        /// <summary>
        /// The command for the No button to bind to in the dialog.
        /// </summary>
        public ICommand NoCommand { get; }

        /// <summary>
        /// The command to copy contents to clipboard.
        /// </summary>
        public ICommand CopyCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmationBoxViewModel"/> class.
        /// </summary>
        [PreferredConstructor] // there needs to be a preferred, parameterless constructor for the MVVMLight IoC
        public ConfirmationBoxViewModel() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmationBoxViewModel"/> class.
        /// </summary>
        public ConfirmationBoxViewModel(string header, string body)
        {
            HeaderText = header;
            BodyText = body;

            YesCommand = new RelayCommand(() => { DialogResult = true; });
            NoCommand = new RelayCommand(() => { DialogResult = false; });
            CopyCommand = new RelayCommand(CopyToClipboard);
        }
        
        /// <summary>
        /// Copy the information about the printer being created to clipboard.
        /// </summary>
        private void CopyToClipboard()
        {
            Clipboard.Clear();
            Clipboard.SetText(HeaderText + "\n" + BodyText);
        }
    }
}
