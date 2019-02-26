using Newtonsoft.Json.Linq;
using PrintPrince.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PrintPrince.Services
{
    /// <summary>
    /// Manager class for interaction with the SysMan API.
    /// </summary>
    public static class SysManManager
    {
        /// <summary>
        /// Gets the URL of SysMan for communication with the API, set in the App.config file.
        /// </summary>
        public static string SysManURL { get; private set; }

        /// <summary>
        /// Client to manage communication with SysMan API.
        /// </summary>
        private static HttpClient _client;
        
        /// <summary>
        /// Initializes the <see cref="SysManManager"/> and validates URL.
        /// </summary>
        public static void Initialize()
        {
            // Set up new HTTPClient that uses the credentials of the user and does not dispose the handler
            _client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true }, false);

            SysManURL = ConfigurationManager.AppSettings["SysManURL"];

            try
            {
                HttpResponseMessage response = _client.GetAsync(SysManURL).Result;

                if (response == null || response.StatusCode != HttpStatusCode.OK)
                {
                    throw new HttpRequestException($"Failed to connect to SysMan URL {SysManURL}, verify the URL in the .config file!");
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Creates a printer in SysMan asynchronously.
        /// </summary>
        /// <param name="name">Name of the printer.</param>
        /// <param name="description">Description of the printer.</param>
        /// <param name="location">Location of the printer.</param>
        /// <returns>
        /// Returns the first line of the HTTP response.
        /// </returns>
        public static async Task<string> CreatePrinterAsync(string name, string description, string location)
        {
            // Create JSON string to POST with the servername "Cirrato" to differentiate the printer from a normal print queue
            string json = $"{{'Name':'{name}','Server':'Cirrato','Description':'{description}','Location':'{location}','canBeDefault':'true','isActive':'true'}}";

            string responseBody;

            // Send API request and save response
            HttpResponseMessage response = await _client.PostAsync(SysManURL + "/api/Printer", new StringContent(json, Encoding.UTF8, "application/json"));

            // Format response to string
            responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }

        /// <summary>
        /// Modify a printer in SysMan asynchronously.
        /// </summary>
        /// <param name="printer">Printer object to update.</param>
        /// <returns>
        /// Returns the first line of the HTTP response.
        /// </returns>
        public static async Task<string> ModifyPrinterAsync(SysManPrinter printer)
        {
            // Create JSON string to PUT the updated printer info
            string json = $"{{'Id':'{printer.ID}','Name':'{printer.Name}','Server':'Cirrato','Description':'{printer.Description}','Location':'{printer.Location}','canBeDefault':'true','isActive':'true'}}";

            string responseBody;

            // Send API request and save response
            HttpResponseMessage response = await _client.PutAsync(SysManURL + "/api/Printer", new StringContent(json, Encoding.UTF8, "application/json"));

            // Format response to string
            responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }

        /// <summary>
        /// Delete a printer in SysMan asynchronously.
        /// </summary>
        /// <param name="id">ID of printer to delete.</param>
        public static async Task<string> DeletePrinterAsync(int id)
        {
            // Create JSON string to PUT the updated printer info
            string json = $"{{'Id':{id}}}";
            
            // Send API request and save response
            HttpRequestMessage request = new HttpRequestMessage{
                Method = HttpMethod.Delete,
                RequestUri = new Uri(SysManURL + "/api/Printer"),
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            HttpResponseMessage response = await _client.SendAsync(request);

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Get all computers or users that have a printer installed through SysMan.
        /// </summary>
        /// <param name="name">Name of the printer to find installations for.</param>
        /// <returns>Returns a list of computers or users that the printer is installed on through SysMan.</returns>
        public async static Task<List<string>> GetPrinterInstallationTargets(string name)
        {
            string responseBody;

            // Send API request and save response
            HttpResponseMessage response = await _client.GetAsync(SysManURL + $"/api/Printer/GetTargetsWithPrinterInstalled?printerName={name}");

            // Format response to string
            responseBody = await response.Content.ReadAsStringAsync();

            // get response as JObject
            var jresult = JArray.Parse(responseBody);

            // Create a list of computers from API response using value "DisplayName" with no empty entries and no duplicates
            List<string> computers = jresult.Select(c => (string)c["displayName"]).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            return computers;
        }

        /// <summary>
        /// Gets all active printers in SysMan.
        /// </summary>
        /// <returns>
        /// Returns a list of all printers that exist and are active in SysMan.
        /// </returns>
        public static async Task<List<SysManPrinter>> GetAllPrinters()
        {
            string responseBody;

            // Send API request and save response
            HttpResponseMessage response = await _client.GetAsync(SysManURL + "/api/Printer/Active?name=%&take=10000&skip=0");
            
            // Format response to string
            responseBody = await response.Content.ReadAsStringAsync();

            // get response as JObject
            var jresult = JObject.Parse(responseBody);
            // get the only property of the response: "result"
            var jprinters = jresult.Properties().FirstOrDefault();

            // Format value of result to a list of SysManPrinter
            List<SysManPrinter> printers = jprinters.Value.ToObject<List<SysManPrinter>>();
            
            return printers;
        }
    }
}
