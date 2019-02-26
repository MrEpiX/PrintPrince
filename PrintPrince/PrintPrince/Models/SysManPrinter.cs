using System.Text;

namespace PrintPrince.Models
{
    /// <summary>
    /// Model of a printer in SysMan.
    /// </summary>
    /// <remarks>
    /// Used as a model when deserializing the JSON response from the SysMan API.
    /// </remarks>
    public class SysManPrinter
    {
        /// <summary>
        /// Name of the printer.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// SysMan ID of the printer.
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Description of the printer.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Location of the printer.
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// Server of the printer.
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// Tag of the printer.
        /// </summary>
        public string Tag { get; set; }
        /// <summary>
        /// Whether or not the printer can be set as default printer.
        /// </summary>
        public bool CanBeDefault { get; set; }
        /// <summary>
        /// If the printer can be removed.
        /// </summary>
        public bool CanBeRemoved { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="SysManPrinter"/> class.
        /// </summary>
        public SysManPrinter() { }

        /// <summary>
        /// Returns a string that represents the <see cref="SysManPrinter"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Printer Name: {Name}");
            sb.AppendLine($"ID: {ID}");
            sb.AppendLine($"Description: {Description}");
            sb.AppendLine($"Location: {Location}");
            sb.AppendLine($"Server: {Server}");
            sb.AppendLine($"Tag: {Tag}");
            sb.AppendLine($"Can be default: {CanBeDefault}");
            sb.AppendLine($"Can be removed: {CanBeRemoved}");

            return sb.ToString();
        }
    }
}
