namespace PrintPrince.Models
{
    /// <summary>
    /// Model for a Cirrato region.
    /// </summary>
    public class Region
    {
        /// <summary>
        /// Name of the region.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Cirrato ID of the region.
        /// </summary>
        public int CirratoID { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="Region"/> class.
        /// </summary>
        public Region(){}
    }
}
