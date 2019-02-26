using GalaSoft.MvvmLight.CommandWpf;
using MvvmDialogs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

namespace PrintPrince.ViewModels
{
    /// <summary>
    /// ViewModel used for the dialog gathering unencrypted credentials from the user to log into the PMC, implementing <see cref="IModalDialogViewModel"/> from <see cref="MvvmDialogs"/>.
    /// </summary>
    public class LoginDialogViewModel : ValidatableViewModelBase, IModalDialogViewModel
    {
        private string _username;
        private string _password;
        private bool? _dialogResult;

        /// <summary>
        /// Username to use for PMC login.
        /// </summary>
        /// <remarks>
        /// Cannot be left empty, null or whitespace.
        /// </remarks>
        [CustomValidation(typeof(LoginDialogViewModel), "ValidateUsername")]
        public string Username
        {
            get => _username;
            set
            {
                Set(nameof(Username), ref _username, value);

                ValidateAsync();
            }
        }

        /// <summary>
        /// Password to use for PMC login.
        /// </summary>
        /// <remarks>
        /// Cannot be left empty, null or whitespace.
        /// The password is hidden when written but unencrypted in memory bound to with the help of <see cref="PasswordHelper"/>.
        /// </remarks>
        [CustomValidation(typeof(LoginDialogViewModel), "ValidatePassword")]
        public string Password
        {
            get => _password;
            set
            {
                Set(nameof(Password), ref _password, value);

                ValidateAsync();
            }
        }

        /// <summary>
        /// The result of the dialog depending on what button is clicked.
        /// </summary>
        public bool? DialogResult
        {
            get => _dialogResult;
            private set => Set(nameof(DialogResult), ref _dialogResult, value);
        }

        /// <summary>
        /// The command for the OK button to bind to in the dialog.
        /// </summary>
        public ICommand OkCommand { get; }
        
        /// <summary>
        /// The command for the Cancel button to bind to in the dialog.
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// Validates <see cref="Username"/> and <see cref="Password"/> once to make sure they show as required.
        /// </remarks>
        public LoginDialogViewModel()
        {
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);
            ValidateAsync();
        }

        /// <summary>
        /// Validates that <see cref="Password"/> is not empty, null or whitespace.
        /// </summary>
        /// <param name="obj">Object of the validation.</param>
        /// <param name="context">Context of the validation.</param>
        /// <returns>Returns validation result.</returns>
        public static ValidationResult ValidatePassword(object obj, ValidationContext context)
        {
            string password = ((LoginDialogViewModel)context.ObjectInstance).Password;

            if (string.IsNullOrWhiteSpace(password))
            {
                return new ValidationResult("Password is required.", new List<string> { "Password" });
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that <see cref="Username"/> is not empty, null or whitespace.
        /// </summary>
        /// <param name="obj">Object of the validation.</param>
        /// <param name="context">Context of the validation.</param>
        /// <returns>Returns validation result.</returns>
        public static ValidationResult ValidateUsername(object obj, ValidationContext context)
        {
            string username = ((LoginDialogViewModel)context.ObjectInstance).Username;

            if (string.IsNullOrWhiteSpace(username))
            {
                return new ValidationResult("Username is required.", new List<string> { "Username" });
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Report <see cref="DialogResult"/> as <c>false</c> when the Cancel button is clicked.
        /// </summary>
        private void Cancel()
        {
            DialogResult = false;
        }

        /// <summary>
        /// Reports <see cref="DialogResult"/> as <c>true</c> when the OK button is clicked and credentials are filled in.
        /// </summary>
        private void Ok()
        {
            if (!HasErrors)
            {
                DialogResult = true;
            }
        }
    }
}
