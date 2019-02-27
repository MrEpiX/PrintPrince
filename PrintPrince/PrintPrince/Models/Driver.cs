using System.Collections.Generic;

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
        /// The configurations of the driver.
        /// </summary>
        public List<Configuration> ConfigurationList { get; set; }
        /// <summary>
        /// The operating systems that the driver is deployed to.
        /// </summary>
        public List<string> DeployedOperatingSystems { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="Driver"/> class.
        /// </summary>
        public Driver()
        {
            ConfigurationList = new List<Configuration>();
            DeployedOperatingSystems = new List<string>();
        }
    }
}
