using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace PrintPrince.Services
{
    /// <summary>
    /// Manages loading state and status messages.
    /// </summary>
    public static class LoadingHandler
    {
        /// <summary>
        /// If the <see cref="LoadingHandler"/> has any loading operations.
        /// </summary>
        /// <remarks>
        /// <see cref="Loading"/> is set to <c>true</c> when the Statuses list has members, and <c>false</c> when it doesn't.
        /// </remarks>
        public static bool Loading { get; private set; }

        /// <summary>
        /// The most recent status message that was added to the <see cref="LoadingHandler"/>.
        /// </summary>
        public static string CurrentStatus { get; private set; }

        /// <summary>
        /// The list of status messages.
        /// </summary>
        private static Dictionary<string, string> Statuses { get; set; }

        /// <summary>
        /// Initializes an instance of the <see cref="LoadingHandler"/> class.
        /// </summary>
        static LoadingHandler()
        {
            Statuses = new Dictionary<string, string>();
        }

        /// <summary>
        /// Helper method to set current status message.
        /// </summary>
        /// <param name="status">The status message to be displayed through <see cref="CurrentStatus"/>.</param>
        private static void SetCurrentStatus(string status)
        {
            CurrentStatus = status;
        }

        /// <summary>
        /// Helper method to set the current loading state.
        /// </summary>
        /// <param name="loadingState">Sets <see cref="Loading"/>.</param>
        /// <remarks>
        /// This method also forces the GUI to refresh to enable or disable through <see cref="CommandManager.InvalidateRequerySuggested()"/> after status change.
        /// </remarks>
        private static void SetLoading(bool loadingState)
        {
            if (Loading != loadingState)
            {
                Loading = loadingState;
                // force GUI refresh after status is set
                // otherwise controls dependent on this through the RelayCommand.CanExecute (the create printer button) will not show correct enabled state until GUI is clicked
                // https://stackoverflow.com/questions/2331622/weird-problem-where-button-does-not-get-re-enabled-unless-the-mouse-is-clicked
                CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Adds a loading operation, updates <see cref="CurrentStatus"/> and sets <see cref="Loading"/> to <c>true</c>.
        /// </summary>
        /// <param name="action">The key to refer to the added loading operation.</param>
        /// <param name="message">The message to be displayed through <see cref="CurrentStatus"/> for the added loading operation.</param>
        public static void AddLoadingOperation(string action, string message)
        {
            Statuses.Add(action, message);

            SetLoading(true);
            SetCurrentStatus(Statuses.Values.ToList().FirstOrDefault());
        }

        /// <summary>
        /// Removes a specified loading operation, updates <see cref="CurrentStatus"/> and sets <see cref="Loading"/> to <c>false</c> if no more operations.
        /// </summary>
        /// <param name="action">The key referring to the loading operation to be removed.</param>
        /// <returns>
        /// Returns whether or not the removal of the operation was successful.
        /// </returns>
        public static bool RemoveLoadingOperation(string action)
        {
            if (action == null)
            {
                return false;
            }

            bool removed = Statuses.Remove(action);

            if (Statuses.Count == 0)
            {
                SetLoading(false);
                SetCurrentStatus("");
            }
            else
            {
                SetCurrentStatus(Statuses.Values.ToList().FirstOrDefault());
            }

            return removed;
        }

        /// <summary>
        /// Gets the loading operations in the form of a list of their status messages.
        /// </summary>
        /// <returns>
        /// Returns a <c>List&lt;string&gt;</c> of all loading operation messages. 
        /// </returns>
        public static List<string> GetLoadingOperations()
        {
            return Statuses.Values.ToList();
        }

        /// <summary>
        /// Clear all loading operations and set <see cref="Loading"/> to <c>false</c>.
        /// </summary>
        public static void Clear()
        {
            Statuses.Clear();
            SetLoading(false);
            SetCurrentStatus("");
        }
    }
}
