namespace PrintPrince.Models
{
    /// <summary>
    /// The model for a deployment of a driver.
    /// </summary>
    public class Deployment
    {
        /// <summary>
        /// The operating system that the deployment is deployed to.
        /// </summary>
        public string OS { get; set; }
        /// <summary>
        /// The ID of the driver that the deployment is associated with.
        /// </summary>
        public string DriverID { get; set; }
        /// <summary>
        /// Creates a new instance of the <see cref="Deployment"/> class.
        /// </summary>
        public Deployment(){}
    }
}
