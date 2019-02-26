using System;

namespace PrintPrince.Services
{
    static class ExceptionExtender
    {
        /// <summary>
        /// Gets all messages of nested InnerExceptions.
        /// </summary>
        /// <param name="ex">Exception to loop through.</param>
        /// <remarks>
        /// Implemented with inspiration from the solution of StackOverflow user ThomazMoura at https://stackoverflow.com/a/35084416.
        /// </remarks>
        /// <returns>String with full error message.</returns>
        public static string GetFullMessage(this Exception ex)
        {
            // If there is no nested exception, return message
            if (ex.InnerException == null)
            {
                return ex.Message;
            } // else return message of innerexception recursively
            else
            {
                return ex.Message + "\n" + ex.InnerException.GetFullMessage();
            }
        }
    }
}
