using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Provides static utility methods to display messages.
    /// </summary>
    public static class Messages
    {
        /// <summary>
        /// Displays a dialog box with Yes / No buttons.
        /// </summary>
        /// <param name="message">The message to display in the dialog box.</param>
        /// <returns>true if Yes if selected; otherwise, false.</returns>
        public static bool ShowYesNo(string message)
        {
            MessageBoxResult result = MessageBox.Show(message, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return (result == MessageBoxResult.Yes);
        }

        /// <summary>
        /// Display a dialog box with an error icon.
        /// </summary>
        /// <param name="message">The message to display in the dialog box.</param>
        public static void ShowError(string message)
        {
            MessageBox.Show(message, "Operation Not Available", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Display a dialog box with an information icon.
        /// </summary>
        /// <param name="message">The message to display in the dialog box.</param>
        public static void Show(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}