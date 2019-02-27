using System.Text;

namespace PrintPrince.Models
{
    /// <summary>
    /// Model of a printer to create.
    /// </summary>
    /// <remarks>
    /// All fields are populated with data from the Cirrato PMC except <see cref="ExistsInSysMan"/>.
    /// </remarks>
    public class Printer
    {
        /// <summary>
        /// Name of the printer.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Cirrato ID of the printer.
        /// </summary>
        public string CirratoID { get; set; }
        /// <summary>
        /// SysMan ID of the printer.
        /// </summary>
        public string SysManID { get; set; }
        /// <summary>
        /// Driver used in Cirrato for the printer.
        /// </summary>
        public Driver Driver { get; set; }
        /// <summary>
        /// Description of the printer.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Location of the printer.
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// IP Address of the printer.
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// Cirrato region that the printer is in.
        /// </summary>
        public Region Region { get; set; }
        /// <summary>
        /// If the printer exists in SysMan or not.
        /// </summary>
        public bool ExistsInSysMan { get; set; }
        /// <summary>
        /// The configuration set for the queue associated with the printer.
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="Printer"/> class.
        /// </summary>
        public Printer(){}

        /// <summary>
        /// Returns a string that represents the <see cref="Printer"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Printer Name: {Name}");
            sb.AppendLine($"Cirrato ID: {CirratoID}");
            sb.AppendLine($"Cirrato Printer Region: {Region.Name}");
            sb.AppendLine($"Exists in SysMan: {ExistsInSysMan}");
            sb.AppendLine($"SysMan ID: {SysManID}");
            sb.AppendLine($"IP Address: {IP}");
            sb.AppendLine($"Print Driver: {Driver.Name}");
            sb.AppendLine($"Configuration: {Configuration}");
            sb.AppendLine($"Description: {Description}");
            sb.AppendLine($"Location: {Location}");

            return sb.ToString();
        }
    }
}
