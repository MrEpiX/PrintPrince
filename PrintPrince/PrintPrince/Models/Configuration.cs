namespace PrintPrince.Models
{
    /// <summary>
    /// The model for a configuration.
    /// </summary>
    /// <remarks>
    /// Configurations in Cirrato are settings bound to drivers that can be applied to queues.
    /// </remarks>
    public class Configuration
    {
        /// <summary>
        /// The name of the configuration.
        /// </summary>
        /// <remarks>
        /// This value is the comment property of the configuration in Cirrato.
        /// </remarks>
        public string Name { get; set; }
        /// <summary>
        /// The ID of the configuration in Cirrato.
        /// </summary>
        public string CirratoID { get; set; }
        /// <summary>
        /// The name of the driver that the configuration is associated with in Cirrato.
        /// </summary>
        public string Driver { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        public Configuration(){}
    }
}
