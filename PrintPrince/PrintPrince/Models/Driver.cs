namespace PrintPrince.Models
{
    /// <summary>
    /// The model for a driver.
    /// </summary>
    /// <remarks>
    /// Cirrato calls drivers "models".
    /// </remarks>
    public class Driver
    {
        /// <summary>
        /// The name of the driver.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The ID of the driver in Cirrato.
        /// </summary>
        public string CirratoID { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="Driver"/> class.
        /// </summary>
        public Driver(){}
    }
}
