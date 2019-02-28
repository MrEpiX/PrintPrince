using PrintPrince.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PrintPrince.Services
{
    /// <summary>
    /// PrinterRepository handles all communication with the Cirrato environment through the Print Management Client API and stores printer data.
    /// </summary>
    /// <remarks>
    /// This class can log into the PMC with provided credentials and get all printers and drivers currently in Cirrato.
    /// PrinterRepository starts pmc.exe from the CirratoPath key specified in the App.config file of this project.
    /// If the PMC output formatting changes, this class has to be modified to work with the new output.
    /// </remarks>
    public static class PrinterRepository
    {
        /// <summary>
        /// List of printers in Cirrato.
        /// </summary>
        public static List<Printer> PrinterList { get; private set; }

        /// <summary>
        /// List of printers in SysMan.
        /// </summary>
        public static List<SysManPrinter> SysManPrinterList { get; private set; }

        /// <summary>
        /// List of visible drivers in Cirrato.
        /// </summary>
        public static List<Driver> DriverList { get; private set; }

        /// <summary>
        /// List of regions in Cirrato.
        /// </summary>
        public static List<Region> RegionList { get; private set; }
        
        /// <summary>
        /// List of printers in SysMan.
        /// </summary>
        /// <remarks>
        /// Used to set <see cref="Printer.ExistsInSysMan"/> of each <see cref="Printer"/> in <see cref="PrinterList"/>.
        /// </remarks>
        private static List<string> SysManPrinters { get; set; }

        /// <summary>
        /// Gets the path of the directory where the PMC is installed, set in the App.config file.
        /// </summary>
        public static string CirratoPath { get; private set; }
        
        /// <summary>
        /// Initializes the <see cref="PrinterRepository"/> and validates PMC path.
        /// </summary>
        public static void Initialize()
        {
            DriverList = new List<Driver>();
            RegionList = new List<Region>();
            PrinterList = new List<Printer>();

            SysManPrinterList = new List<SysManPrinter>();

            // Set path from app.config file
            CirratoPath = ConfigurationManager.AppSettings["CirratoPath"];

            if (!File.Exists(CirratoPath))
            {
                throw new FileNotFoundException($"Could not find {CirratoPath}, please verify path in .config file!");
            }
        }

        /// <summary>
        /// Loads data from the Cirrato PMC.
        /// </summary>
        public static async Task LoadPMCData()
        {
            try
            {
                // This order needs to be followed since the methods populate lists between each other
                await GetCirratoDriversAsync();
                await GetCirratoConfigurationsAsync();
                await GetCirratoDeploymentsAsync();
                await GetCirratoRegionsAsync();
                await GetCirratoPrintersAsync();
                await GetCirratoQueuesAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Loads data from the SysMan API.
        /// </summary>
        public static async Task LoadSysManData()
        {
            await GetSysManPrintersAsync();
        }

        /// <summary>
        /// Checks if the user is already logged into the PMC.
        /// </summary>
        /// <returns>
        /// Returns the first line of the output of the PMC.
        /// </returns>
        public static async Task<string> GetLoginStatusAsync()
        {
            return await Task.Run(() =>
            {
                Process cirratoProcess = new Process
                {
                    // use a command simply to see if we get the "not logged in"-error or not
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = CirratoPath,
                        Arguments = "model list",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                cirratoProcess.Start();

                string setting = cirratoProcess.StandardOutput.ReadLine();

                return setting;
            });
        }
        
        /// <summary>
        /// Log into Cirrato PMC asynchronously.
        /// </summary>
        /// <param name="domain">The FQDN of the domain Cirrato is installed in.</param>
        /// <param name="username">The username of the user logging into Cirrato PMC.</param>
        /// <param name="password">The password of the user logging into Cirrato PMC.</param>
        /// <remarks>
        /// The user logging in has to be part of the Cirrato API AD group specified in the Cirrato Configuration Manager.
        /// </remarks>
        /// <returns>
        /// Returns the output of the pmc.exe with the arguments "login -d <paramref name="domain"/> -u <paramref name="username"/> -p <paramref name="password"/>."
        /// </returns>
        public static async Task<string> LoginAsync(string domain, string username, string password)
        {
            return await Task.Run(() =>
            {
                Process cirratoProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = CirratoPath,
                        Arguments = $"login -d {domain} -u {username} -p {password}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                cirratoProcess.Start();

                return cirratoProcess.StandardOutput.ReadLine();
            });
        }

        /// <summary>
        /// Gets all visible drivers from Cirrato asynchronously and saves them to <see cref="DriverList"/>.
        /// </summary>
        /// <remarks>
        /// Starts pmc.exe with the argument "model list --verbose", filters out drivers set to invisible and saves each <see cref="Driver"/> to a list.
        /// Encodes the PMC output with OEM Code Page 850 (Multilingual Latin 1) to enable Swedish letters.
        /// </remarks>
        private static async Task GetCirratoDriversAsync()
        {
            await Task.Run(() =>
            {
                // example output from PMC
                /*
                    [
                        {
                            "modelId": "aabbccdd-eeee-ffff-1111-2233445566",
                            "modelBrandId": "aabbccdd-eeee-ffff-1111-2233445566",
                            "modelName": "SHARP MX-M6070 PCL6",
                            "modelComment": "",
                            "modelVisible": 1,
                            "statusOctetSwapped": 0,
                            "CheckTonerOids": 0
                        },
                        {
                            "modelId": "aabbccdd-eeee-ffff-1111-2233445566",
                            "modelBrandId": "aabbccdd-eeee-ffff-1111-2233445566",
                            "modelName": "SHARP MX-M905 PCL6",
                            "modelComment": "",
                            "modelVisible": 1,
                            "statusOctetSwapped": 0,
                            "CheckTonerOids": 0
                        }
                    ]
                */

                // Start Cirrato PMC process to get all models / drivers
                Process cirratoProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = CirratoPath,
                        Arguments = "model list --verbose",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.GetEncoding(850)
                    }
                };
                cirratoProcess.Start();

                Driver currentPrintDriver = new Driver();
                bool visible = false;

                // Save drivers to list
                while (!cirratoProcess.StandardOutput.EndOfStream)
                {
                    string line = cirratoProcess.StandardOutput.ReadLine();

                    if (line.StartsWith("[ERROR]"))
                    {
                        Logger.Log($"Error loading data from Cirrato. Error message:\n{line}", System.Diagnostics.EventLogEntryType.Error);
                        MessageBox.Show($"Error loading data from Cirrato. Error message:\n{line}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.MainWindow.Close();
                        return;
                    }

                    // remove the whitespace before the string
                    line = line.Trim();
                    
                    // save name of driver
                    if (line.Contains("modelName"))
                    {
                        // Example of current string: "modelName": "SHARP MX-M905 PCL6",
                        line = line.Substring(line.IndexOf(':') + 3);

                        // Example of current string: SHARP MX-M905 PCL6",
                        line = line.Substring(0, line.Length - 2);

                        // Example of current string: SHARP MX-M905 PCL6
                        currentPrintDriver.Name = line;
                    } // save ID of driver
                    else if (line.Contains("modelId"))
                    {
                        // Example of current string: "modelId": "aabbccdd-eeee-ffff-1111-2233445566",
                        line = line.Substring(line.IndexOf(':') + 3);

                        // Example of current string: aabbccdd-eeee-ffff-1111-2233445566",
                        line = line.Substring(0, line.Length - 2);

                        // Example of current string: "aabbccdd-eeee-ffff-1111-2233445566
                        currentPrintDriver.CirratoID = line;
                    } // add model only if it is set to visible in cirrato
                    else if (line == "\"modelVisible\": 1,")
                    {
                        visible = true;
                    } // if all info about the current driver has been presented, reset current driver object
                    else if (line == "},")
                    {
                        if (visible)
                        {
                            DriverList.Add(currentPrintDriver);
                        }

                        visible = false;
                        currentPrintDriver = new Driver();
                    }
                }

                // the last '}' in the output does not have a comma behind it, so we need to check the last driver after the loop
                if (visible)
                {
                    DriverList.Add(currentPrintDriver);
                }
            });
        }

        /// <summary>
        /// Gets all regions from Cirrato asynchronously and saves each <see cref="Region"/> to <see cref="RegionList"/>.
        /// </summary>
        /// <remarks>
        /// Starts pmc.exe with the argument "region list -p / --verbose", filters the output and saves the regions to a list.
        /// Encodes the PMC output with OEM Code Page 850 (Multilingual Latin 1) to enable Swedish letters.
        /// </remarks>
        private static async Task GetCirratoRegionsAsync()
        {
            await Task.Run(() =>
            {
                // Example of PMC output
                /*
                [
                    {
                        "regionId": 1,
                        "regionParentId": 0,
                        "regionName": "Region1",
                        "regionActive": 1,
                        "regionAlarmProgramId": "",
                        "regionSortOrder": 100,
                        "regionMapFile": null,
                        "regionParentMapShape": "poly",
                        "regionParentMapCoords": null,
                        "regionFullPath": "Region1",
                        "lastUpdated": null,
                        "regionMapPosX": null,
                        "regionMapPosY": null,
                        "regionPosUpdated": null
                    },
                    {
                        "regionId": 2,
                        "regionParentId": 0,
                        "regionName": "Region2",
                        "regionActive": 1,
                        "regionAlarmProgramId": "",
                        "regionSortOrder": 100,
                        "regionMapFile": null,
                        "regionParentMapShape": "poly",
                        "regionParentMapCoords": null,
                        "regionFullPath": "Region1/Region2",
                        "lastUpdated": null,
                        "regionMapPosX": null,
                        "regionMapPosY": null,
                        "regionPosUpdated": null
                    }
                ]
                 */

                // Start Cirrato PMC process
                Process cirratoProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = CirratoPath,
                        Arguments = "region list -p / --verbose",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.GetEncoding(850)
                    }
                };
                cirratoProcess.Start();

                Region currentRegion = new Region();

                // Save printers to list
                while (!cirratoProcess.StandardOutput.EndOfStream)
                {
                    string line = cirratoProcess.StandardOutput.ReadLine();

                    if (line.StartsWith("[ERROR]"))
                    {
                        Logger.Log($"Error loading data from Cirrato. Error message:\n{line}", System.Diagnostics.EventLogEntryType.Error);
                        MessageBox.Show($"Error loading data from Cirrato. Error message:\n{line}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.MainWindow.Close();
                        return;
                    }

                    // remove the whitespace before the string
                    line = line.Trim();

                    // save name of printer
                    if (line.Contains("regionFullPath"))
                    {
                        // Example of current string: "regionFullPath": "Region1",
                        line = line.Substring(line.IndexOf(':') + 3);

                        // Example of current string: Region1",
                        line = line.Substring(0, line.Length - 2);

                        // Example of current string: Region1
                        currentRegion.Name = line;
                    } // save ID of region
                    else if (line.Contains("regionId"))
                    {
                        // Example of current string: "regionId": 1,
                        line = line.Substring(line.IndexOf(':') + 2);

                        // Example of current string: 1",
                        line = line.Substring(0, line.Length - 1);

                        // Example of current string: 1
                        currentRegion.CirratoID = int.Parse(line);
                    } // if all info about the current driver has been presented, reset current region object
                    else if (line == "},")
                    {
                        RegionList.Add(currentRegion);

                        currentRegion = new Region();
                    }
                }

                // the last '}' in the output does not have a comma behind it, so we need to add the last region after the loop
                RegionList.Add(currentRegion);
            });
        }

        /// <summary>
        /// Gets all queues from Cirrato asynchronously and binds configuration ID to the <see cref="Printer"/> of the queue in <see cref="PrinterList"/>.
        /// </summary>
        /// <remarks>
        /// Starts pmc.exe with the argument "queue list --path * --verbose", filters the output and saves the configurations to a list.
        /// Encodes the PMC output with OEM Code Page 850 (Multilingual Latin 1) to enable Swedish letters.
        /// </remarks>
        private static async Task GetCirratoQueuesAsync()
        {
            await Task.Run(() =>
            {
                // Example of PMC output
                /*
                ]
                    {
                        "queueId": "aabbccdd-eeee-ffff-1111-2233445566",
                        "queueName": "Testprinter01",
                        "queueModelId": "aabbccdd-eeee-ffff-1111-2233445566",
                        "queueLocation": "Outside management",
                        "queueComment": "Printer Example Model 1",
                        "queueRegion": 7,
                        "queueType": 1,
                        "queueStatus": 0,
                        "queueTTL": 60,
                        "queueAdminComment": "",
                        "queueNewStatus": null,
                        "queueHidden": 0,
                        "offlineReason": null,
                        "pagesPerMinuteBWSingle": null,
                        "pagesPerMinuteColorSingle": null,
                        "pagesPerMinuteBWDuplex": null,
                        "pagesPerMinuteColorDuplex": null,
                        "Configurations": [
                            {
                                "Os": "W7",
                                "Locale": "",
                                "ConfigurationFileId": "aabbccdd-eeee-ffff-1111-2233445566"
                            },
                            {
                                "Os": "W1064",
                                "Locale": "",
                                "ConfigurationFileId": "aabbccdd-eeee-ffff-1111-2233445566"
                            }
                        ]
                    },
                    {
                        "queueId": "aabbccdd-eeee-ffff-1111-2233445566",
                        "queueName": "Testprinter02",
                        "queueModelId": "aabbccdd-eeee-ffff-1111-2233445566",
                        "queueLocation": "At the helpdesk",
                        "queueComment": "Example model 2",
                        "queueRegion": 7,
                        "queueType": 1,
                        "queueStatus": 0,
                        "queueTTL": 60,
                        "queueAdminComment": "",
                        "queueNewStatus": null,
                        "queueHidden": 0,
                        "offlineReason": null,
                        "pagesPerMinuteBWSingle": null,
                        "pagesPerMinuteColorSingle": null,
                        "pagesPerMinuteBWDuplex": null,
                        "pagesPerMinuteColorDuplex": null,
                        "Configurations": []
                    }
                ]
                 */

                // Start Cirrato PMC process
                Process cirratoProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = CirratoPath,
                        Arguments = "queue list --path * --verbose",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.GetEncoding(850)
                    }
                };
                cirratoProcess.Start();

                string queueName = "";
                string configurationID = "";

                // Save printers to list
                while (!cirratoProcess.StandardOutput.EndOfStream)
                {
                    string line = cirratoProcess.StandardOutput.ReadLine();

                    if (line.StartsWith("[ERROR]"))
                    {
                        Logger.Log($"Error loading data from Cirrato. Error message:\n{line}", System.Diagnostics.EventLogEntryType.Error);
                        MessageBox.Show($"Error loading data from Cirrato. Error message:\n{line}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.MainWindow.Close();
                        return;
                    }

                    // remove the whitespace before the string
                    line = line.Trim();

                    // save name of queue
                    if (line.Contains("queueName"))
                    {
                        // Example of current string: "queueName": "Testprinter02",
                        line = line.Substring(line.IndexOf(':') + 3);

                        // Example of current string: Testprinter02",
                        line = line.Substring(0, line.Length - 2);

                        // Example of current string: Testprinter02
                        queueName = line;
                    } // save comment/name of config
                    else if (line.Contains("Configurations"))
                    {
                        // inner loop until end of configurations section
                        while (!line.Contains("]"))
                        {
                            // Skip until we find get the ID of the configuration
                            line = cirratoProcess.StandardOutput.ReadLine();
                            
                            if (line.Contains("ConfigurationFileId"))
                            {
                                // Example of current string: "ConfigurationFileId": "aabbccdd-eeee-ffff-1111-2233445566"
                                line = line.Substring(line.IndexOf(':') + 3);

                                // Example of current string: aabbccdd-eeee-ffff-1111-2233445566"
                                line = line.Substring(0, line.Length - 1);

                                // Example of current string: aabbccdd-eeee-ffff-1111-2233445566
                                configurationID = line;
                            }
                        }
                    } // if all info about the current driver has been presented, reset current queue
                    else if (line == "},")
                    {
                        // Find printer in list and set the configuration to the name of the configuration in the driver's list of configurations matched by ID
                        int index = PrinterList.FindIndex(p => p.Name == queueName);
                        if (!string.IsNullOrWhiteSpace(configurationID) && index >= 0 && index < PrinterList.Count)
                        {
                            PrinterList[index].Configuration = DriverList.Where(d => d.ConfigurationList.Any(c => c.CirratoID == configurationID)).FirstOrDefault().ConfigurationList.Where(c => c.CirratoID == configurationID).FirstOrDefault().Name;
                        }
                        queueName = "";
                        configurationID = "";
                    }
                }

                // the last '}' in the output does not have a comma behind it, so we need to add the last config after the loop
                int lastIndex = PrinterList.FindIndex(p => p.Name == queueName);
                if (!string.IsNullOrWhiteSpace(configurationID) && lastIndex >= 0 && lastIndex < PrinterList.Count)
                {
                    PrinterList[lastIndex].Configuration = DriverList.Where(d => d.ConfigurationList.Any(c => c.CirratoID == configurationID)).FirstOrDefault().ConfigurationList.Where(c => c.CirratoID == configurationID).FirstOrDefault().Name;
                }
            });
        }

        /// <summary>
        /// Gets all configurations from Cirrato asynchronously and saves each <see cref="Configuration"/> to the <see cref="Printer"/> in <see cref="PrinterList"/>.
        /// </summary>
        /// <remarks>
        /// Starts pmc.exe with the argument "configuration list --verbose", filters the output and saves the configurations to a list.
        /// Encodes the PMC output with OEM Code Page 850 (Multilingual Latin 1) to enable Swedish letters.
        /// </remarks>
        private static async Task GetCirratoConfigurationsAsync()
        {
            await Task.Run(() =>
            {
                // Example of PMC output
                /*
                [
                    {
                        "configfile_id": "aabbccdd-eeee-ffff-1111-2233445566",
                        "configfile_model": "RICOH PCL6 UniversalDriver V4.18",
                        "configfile_date": "2019-02-27T12:42:16.953",
                        "configfile_creator": "username",
                        "configfile_domain": "domain",
                        "configfile_arch": 0,
                        "configfile_majorOs": 10,
                        "configfile_minorOs": 0,
                        "configfile_Locale": "",
                        "configfile_majorversion": 3,
                        "configfile_minorversion": 0,
                        "configfile_comment": "Ricoh B/W Config",
                        "configfile_queueName": "Ricoh Template Printer",
                        "configfile_queueId": "",
                        "configfile_driverOS": 0
                    },
                    {
                        "configfile_id": "aabbccdd-eeee-ffff-1111-2233445566",
                        "configfile_model": "PCL6 Driver for Universal Print",
                        "configfile_date": "2017-11-20T10:22:24.363",
                        "configfile_creator": "username",
                        "configfile_domain": "domain",
                        "configfile_arch": 0,
                        "configfile_majorOs": 6,
                        "configfile_minorOs": 3,
                        "configfile_Locale": "",
                        "configfile_majorversion": 1,
                        "configfile_minorversion": 2,
                        "configfile_comment": "Ricoh BW Duplex",
                        "configfile_queueName": "Ricoh Template Printer",
                        "configfile_queueId": "",
                        "configfile_driverOS": 0
                    }
                ]
                 */

                // Start Cirrato PMC process
                Process cirratoProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = CirratoPath,
                        Arguments = "configuration list --verbose",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.GetEncoding(850)
                    }
                };
                cirratoProcess.Start();

                Models.Configuration currentConfig = new Models.Configuration();

                // Save printers to list
                while (!cirratoProcess.StandardOutput.EndOfStream)
                {
                    string line = cirratoProcess.StandardOutput.ReadLine();

                    if (line.StartsWith("[ERROR]"))
                    {
                        Logger.Log($"Error loading data from Cirrato. Error message:\n{line}", System.Diagnostics.EventLogEntryType.Error);
                        MessageBox.Show($"Error loading data from Cirrato. Error message:\n{line}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.MainWindow.Close();
                        return;
                    }

                    // remove the whitespace before the string
                    line = line.Trim();

                    // save comment/name of config
                    if (line.Contains("configfile_comment"))
                    {
                        // Example of current string: "configfile_comment": "Ricoh BW Duplex",
                        line = line.Substring(line.IndexOf(':') + 3);

                        // Example of current string: Ricoh BW Duplex",
                        line = line.Substring(0, line.Length - 2);

                        // Example of current string: Ricoh BW Duplex
                        currentConfig.Name = line;
                    } // save ID of config
                    else if (line.Contains("configfile_id"))
                    {
                        // Example of current string: "configfile_id": "aabbccdd-eeee-ffff-1111-2233445566",
                        line = line.Substring(line.IndexOf(':') + 3);

                        // Example of current string: aabbccdd-eeee-ffff-1111-2233445566",
                        line = line.Substring(0, line.Length - 2);

                        // Example of current string: aabbccdd-eeee-ffff-1111-2233445566
                        currentConfig.CirratoID = line;
                    } // save ID of config
                    else if (line.Contains("configfile_model"))
                    {
                        // Example of current string: "configfile_model": "PCL6 Driver for Universal Print",
                        line = line.Substring(line.IndexOf(':') + 3);

                        // Example of current string: PCL6 Driver for Universal Print",
                        line = line.Substring(0, line.Length - 2);

                        // Example of current string: PCL6 Driver for Universal Print
                        currentConfig.Driver = line;
                    } // if all info about the current driver has been presented, reset current config object
                    else if (line == "},")
                    {
                        int index = DriverList.FindIndex(d => d.Name == currentConfig.Driver);
                        // Find driver in driverlist and add the configuration to it
                        if (index >= 0 && index < DriverList.Count)
                        {
                            DriverList[index].ConfigurationList.Add(currentConfig);
                        }

                        currentConfig = new Models.Configuration();
                    }
                }

                // the last '}' in the output does not have a comma behind it, so we need to add the last config after the loop
                int lastIndex = DriverList.FindIndex(d => d.Name == currentConfig.Driver);
                // Find driver in driverlist and add the configuration to it
                if (lastIndex >= 0 && lastIndex < DriverList.Count)
                {
                    DriverList[lastIndex].ConfigurationList.Add(currentConfig);
                }
            });
        }

        /// <summary>
        /// Gets all driver deployments from Cirrato asynchronously and saves each to the <see cref="Driver"/> in <see cref="DriverList"/>.
        /// </summary>
        /// <remarks>
        /// Starts pmc.exe with the argument "deployment list --verbose", filters the output and saves the configurations to a list.
        /// </remarks>
        private static async Task GetCirratoDeploymentsAsync()
        {
            await Task.Run(() =>
            {
                // Example of PMC output
                /*
                [
                    {
                        "mapId": "aabbccdd-eeee-ffff-1111-2233445566",
                        "mapDriverFileId": "aabbccdd-eeee-ffff-1111-2233445566",
                        "mapModelId": "aabbccdd-eeee-ffff-1111-2233445566",
                        "mapDeployed": 1,
                        "mapInstall": 1,
                        "mapOs": 0,
                        "mapOsId": "W1064",
                        "mapLocale": "",
                        "mapSpecial": 0,
                        "mapIm": 2,
                        "mapQ": 0,
                        "regionId": null,
                        "rolloutId": null,
                        "useOldConfig": false,
                        "queueId": null
                    },
                    {
                        "mapId": "aabbccdd-eeee-ffff-1111-2233445566",
                        "mapDriverFileId": "aabbccdd-eeee-ffff-1111-2233445566",
                        "mapModelId": "aabbccdd-eeee-ffff-1111-2233445566",
                        "mapDeployed": 1,
                        "mapInstall": 1,
                        "mapOs": 0,
                        "mapOsId": "W1032",
                        "mapLocale": "",
                        "mapSpecial": 0,
                        "mapIm": 0,
                        "mapQ": 0,
                        "regionId": null,
                        "rolloutId": null,
                        "useOldConfig": false,
                        "queueId": null
                    }
                ]
                 */

                // Start Cirrato PMC process
                Process cirratoProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = CirratoPath,
                        Arguments = "deployment list --verbose",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                cirratoProcess.Start();

                // Save the operating system of the deployment and the ID of the driver that it's connected to
                string driverID = "";
                string os = "";

                // Save printers to list
                while (!cirratoProcess.StandardOutput.EndOfStream)
                {
                    string line = cirratoProcess.StandardOutput.ReadLine();

                    if (line.StartsWith("[ERROR]"))
                    {
                        Logger.Log($"Error loading data from Cirrato. Error message:\n{line}", System.Diagnostics.EventLogEntryType.Error);
                        MessageBox.Show($"Error loading data from Cirrato. Error message:\n{line}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.MainWindow.Close();
                        return;
                    }

                    // remove the whitespace before the string
                    line = line.Trim();

                    // save operating system ID for configuration mapping
                    if (line.Contains("mapOsId"))
                    {
                        // Example of current string: "mapOsId": "W1064",
                        line = line.Substring(line.IndexOf(':') + 3);

                        // Example of current string: W1064",
                        line = line.Substring(0, line.Length - 2);

                        // Example of current string: W1064
                        os = line;
                    } // save ID of model
                    else if (line.Contains("mapModelId"))
                    {
                        // Example of current string: "mapModelId": "aabbccdd-eeee-ffff-1111-2233445566",
                        line = line.Substring(line.IndexOf(':') + 3);

                        // Example of current string: aabbccdd-eeee-ffff-1111-2233445566",
                        line = line.Substring(0, line.Length - 2);

                        // Example of current string: aabbccdd-eeee-ffff-1111-2233445566
                        driverID = line;
                    } // if all info about the current driver has been presented, reset current deployment
                    else if (line == "},")
                    {
                        // Find driver in driverlist and add the deployed operating system to it
                        int index = DriverList.FindIndex(d => d.CirratoID == driverID);
                        if (index >= 0 && index < DriverList.Count)
                        {
                            if (!DriverList[index].DeployedOperatingSystems.Contains(os))
                            {
                                DriverList[index].DeployedOperatingSystems.Add(os);
                            }
                        }

                        os = "";
                        driverID = "";
                    }
                }

                // the last '}' in the output does not have a comma behind it, so we need to add the last config after the loop
                int lastIndex = DriverList.FindIndex(d => d.CirratoID == driverID);
                if (lastIndex >= 0 && lastIndex < DriverList.Count)
                {
                    if (!DriverList[lastIndex].DeployedOperatingSystems.Contains(os))
                    {
                        DriverList[lastIndex].DeployedOperatingSystems.Add(os);
                    }
                }
            });
        }

        /// <summary>
        /// Gets all printers from Cirrato asynchronously and saves each <see cref="Printer"/> to <see cref="PrinterList"/>.
        /// </summary>
        /// <remarks>
        /// Needs the <see cref="DriverList"/> to have been populated with drivers.
        /// Needs the <see cref="RegionList"/> to have been populated with regions.
        /// Starts pmc.exe with the argument "printer list -p * --verbose", filters the output and saves the printers to a list.
        /// Encodes the PMC output with OEM Code Page 850 (Multilingual Latin 1) to enable Swedish letters.
        /// </remarks>
        private static async Task GetCirratoPrintersAsync()
        {
            await Task.Run(() =>
            {
                // Example output from the PMC
                /*
                [
                    {
                        "printerId": "aabbccdd-eeee-ffff-1111-2233445566",
                        "printerName": "TestPrinter",
                        "printerLocation": "",
                        "printerComment": "",
                        "printerModel": "aabbccdd-eeee-ffff-1111-2233445566",
                        "printerIp": "1.1.1.1",
                        "printerPort": "",
                        "printerNetPort": 9100,
                        "modelId": "aabbccdd-eeee-ffff-1111-2233445566",
                        "region": 1,
                        "physStatus": 0,
                        "printStatus": 0,
                        "deviceStatus": 0,
                        "adminStatus": 0,
                        "printerAdminComment": "",
                        "printerTTL": 0,
                        "snmpCheck": "0",
                        "printerCommunity": "public",
                        "printerMailMessage": "",
                        "printerSnmpVersion": 1,
                        "printerProgramId": "",
                        "printerCounterProgramId": "",
                        "printerHidden": 0,
                        "statusOctetSwapped": 0,
                        "CheckTonerOids": 0,
                        "printerExternalAccounting": "0",
                        "http": "",
                        "printerUserName": null,
                        "printerPassword": null
                    },
                    {
                        "printerId": "aabbccdd-eeee-ffff-1111-2233445566",
                        "printerName": "TestPrinter Color",
                        "printerLocation": "Example Location",
                        "printerComment": "",
                        "printerModel": "aabbccdd-eeee-ffff-1111-2233445566",
                        "printerIp": "2.2.2.2",
                        "printerPort": "",
                        "printerNetPort": 9100,
                        "modelId": "aabbccdd-eeee-ffff-1111-2233445566",
                        "region": 2,
                        "physStatus": 0,
                        "printStatus": 0,
                        "deviceStatus": 0,
                        "adminStatus": 0,
                        "printerAdminComment": "",
                        "printerTTL": 0,
                        "snmpCheck": "0",
                        "printerCommunity": "public",
                        "printerMailMessage": "",
                        "printerSnmpVersion": 1,
                        "printerProgramId": "",
                        "printerCounterProgramId": "",
                        "printerHidden": 0,
                        "statusOctetSwapped": 0,
                        "CheckTonerOids": 0,
                        "printerExternalAccounting": "0",
                        "http": "",
                        "printerUserName": null,
                        "printerPassword": null
                    }
                ]
                 */

                // Start Cirrato PMC process
                Process cirratoProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = CirratoPath,
                        Arguments = "printer list -p * --verbose",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.GetEncoding(850)
                    }
                };
                cirratoProcess.Start();

                Printer currentPrinter = new Printer();

                // Save printers to list
                while (!cirratoProcess.StandardOutput.EndOfStream)
                {
                    string line = cirratoProcess.StandardOutput.ReadLine();

                    if (line.StartsWith("[ERROR]"))
                    {
                        Logger.Log($"Error loading data from Cirrato. Error message:\n{line}", System.Diagnostics.EventLogEntryType.Error);
                        MessageBox.Show($"Error loading data from Cirrato. Error message:\n{line}", "Cirrato PMC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.MainWindow.Close();
                        return;
                    }

                    // remove the whitespace before the string
                    line = line.Trim();

                    // save name of printer
                    if (line.Contains("printerName"))
                    {
                        // Example of current string: "printerName": "TestPrinter",
                        line = line.Substring(line.IndexOf(':') + 3);

                        // Example of current string: TestPrinter",
                        line = line.Substring(0, line.Length - 2);

                        // Example of current string: TestPrinter
                        currentPrinter.Name = line;
                    } // save ID of printer
                    else if (line.Contains("printerId"))
                    {
                        // Example of current string: "printerId": "aabbccdd-eeee-ffff-1111-2233445566",
                        line = line.Substring(line.IndexOf(':') + 3);

                        // Example of current string: aabbccdd-eeee-ffff-1111-2233445566",
                        line = line.Substring(0, line.Length - 2);

                        // Example of current string: aabbccdd-eeee-ffff-1111-2233445566
                        currentPrinter.CirratoID = line;
                    } // save location of printer
                    else if (line.Contains("printerLocation"))
                    {
                        line = line.Substring(line.IndexOf(':') + 3);
                        line = line.Substring(0, line.Length - 2);

                        currentPrinter.Location = line;
                    } // save IP of printer
                    else if (line.Contains("printerIp"))
                    {
                        line = line.Substring(line.IndexOf(':') + 3);
                        line = line.Substring(0, line.Length - 2);

                        currentPrinter.IP = line;
                    } // save region of printer
                    else if (line.Contains("region"))
                    {
                        line = line.Substring(line.IndexOf(':') + 2);
                        line = line.Substring(0, line.Length - 1);

                        // find region that matches current ID
                        currentPrinter.Region = RegionList.Find(r => r.CirratoID == int.Parse(line));
                    } // save description of printer
                    else if (line.Contains("printerComment"))
                    {
                        line = line.Substring(line.IndexOf(':') + 3);
                        line = line.Substring(0, line.Length - 2);

                        currentPrinter.Description = line;
                    } // save ID of driver from property printerModel (modelId is another property that does not represent the right driver)
                    else if (line.Contains("printerModel"))
                    {
                        line = line.Substring(line.IndexOf(':') + 3);
                        line = line.Substring(0, line.Length - 2);

                        // find driver that matches current ID
                        currentPrinter.Driver = DriverList.Find(d => d.CirratoID == line);
                    } // if all info about the current driver has been presented, reset current printer object
                    else if (line == "},")
                    {
                        PrinterList.Add(currentPrinter);
                        
                        currentPrinter = new Printer();
                    }
                }

                // the last '}' in the output does not have a comma behind it, so we need to add the last printer after the loop
                PrinterList.Add(currentPrinter);
            });
        }

        /// <summary>
        /// Gets all info from SysMan and sets properties of printers in <see cref="PrinterList"/>.
        /// </summary>
        private static async Task GetSysManPrintersAsync()
        {
            // Get all active printers from SysMan
            SysManPrinterList = await SysManManager.GetAllPrinters();

            // Match all printers in SysMan to Cirrato ones
            for (int i = 0; i < PrinterList.Count; i++)
            {
                // Get printer in SysMan
                SysManPrinter sysmanPrinter = SysManPrinterList.Where(p => p.Name == PrinterList[i].Name).FirstOrDefault();

                // if the printer exists in SysMan, set properties of current printer
                if (sysmanPrinter != null)
                {
                    PrinterList[i].ExistsInSysMan = true;
                    PrinterList[i].SysManID = sysmanPrinter.ID.ToString();
                }
                else // else set default values
                {
                    PrinterList[i].ExistsInSysMan = false;
                    PrinterList[i].SysManID = "";
                }
            }
        }

        /// <summary>
        /// Delete a printer in Cirrato asynchronously.
        /// </summary>
        /// <param name="id">The ID of the printer to delete.</param>
        /// <returns>Returns the first line of the output of the command from the PMC.</returns>
        public static async Task<string> DeletePrinterAsync(string id)
        {
            return await Task.Run(() =>
            {
                string argumentString = $"printer delete --id \"{id}\"";

                // Start process to create printer in Cirrato
                Process cirratoProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = CirratoPath,
                        Arguments = argumentString,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                cirratoProcess.Start();

                string output = cirratoProcess.StandardOutput.ReadLine();

                return output;
            });
        }

        /// <summary>
        /// Modify a printer in Cirrato asynchronously.
        /// </summary>
        /// <param name="printer">A printer object with modified information.</param>
        /// <returns>Returns the first line of the output of the command from the PMC.</returns>
        public static async Task<string> ModifyPrinterAsync(Printer printer)
        {
            return await Task.Run(() =>
            {
                string argumentString = $"printer modify --id \"{printer.CirratoID}\" --name \"{printer.Name}\" --path \"{printer.Region.Name}\" --ip \"{printer.IP}\" --model \"{printer.Driver.Name}\" --comment \"{printer.Description}\" --location \"{printer.Location}\"";
                
                // Start process to create printer in Cirrato
                Process cirratoProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = CirratoPath,
                        Arguments = argumentString,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                cirratoProcess.Start();

                string output = cirratoProcess.StandardOutput.ReadLine();

                return output;
            });
        }

        /// <summary>
        /// Modify a queue in Cirrato asynchronously.
        /// </summary>
        /// <param name="printer">A printer object with modified information.</param>
        /// <returns>Returns the first line of the output of the command from the PMC.</returns>
        public static async Task<string> ModifyQueueAsync(Printer printer)
        {
            return await Task.Run(() =>
            {
                string argumentString = $"queue modify --target \"{printer.Region.Name}\\{printer.Name}\" --model \"{printer.Driver.Name}\" --comment \"{printer.Description}\" --location \"{printer.Location}\"";

                // Get configuration of existing printer to see if it was changed
                string existingPrinterConfiguration = PrinterList.Where(p => p.Name == printer.Name).FirstOrDefault().Configuration;
                // Only set new configuration if the configuration is not empty
                if (!string.IsNullOrWhiteSpace(printer.Configuration))
                {
                    // If the configuration has changed, update to new configuration
                    if (existingPrinterConfiguration != printer.Configuration)
                    {
                        // Get ID of configuration in Cirrato
                        string configID = printer.Driver.ConfigurationList.Where(c => c.Name == printer.Configuration).Select(c => c.CirratoID).FirstOrDefault();

                        // Make a list of strings for PMC input with operating system ID and configuration ID separated by colon
                        List<string> configStrings = new List<string>();
                        foreach (string os in printer.Driver.DeployedOperatingSystems)
                        {
                            configStrings.Add($"{os}:{configID}");
                        }

                        // Join list to make one string of all deployments to set configuration for
                        string configString = string.Join(",", configStrings);

                        // Add the configuration settings to the pmc argument
                        argumentString = $"{argumentString} --ac \"{configString}\"";
                    }
                } // If the configuration is empty, remove configurations
                else
                {
                    string osString = string.Join(",", printer.Driver.DeployedOperatingSystems);

                    argumentString = $"{argumentString} --rc \"{osString}\"";
                }

                // Start process to create printer in Cirrato
                Process cirratoProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = CirratoPath,
                        Arguments = argumentString,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                cirratoProcess.Start();

                string output = cirratoProcess.StandardOutput.ReadLine();

                return output;
            });
        }

        /// <summary>
        /// Add a configuration to a queue in Cirrato asynchronously.
        /// </summary>
        /// <param name="queue">The full path to the queue to modify.</param>
        /// <param name="configStrings">A list containing strings with the operating system ID and the configuration ID separated by a colon.</param>
        /// <returns>Returns the first line of the output of the command from the PMC.</returns>
        public static async Task<string> AddQueueConfigurationAsync(string queue, List<string> configStrings)
        {
            return await Task.Run(() =>
            {
                // Join all elements in comma separated list
                string osString = string.Join(",", configStrings);

                // Start process to create printer in Cirrato
                Process cirratoProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = CirratoPath,
                        Arguments = $"queue modify --target \"{queue}\" -ac \"{osString}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                cirratoProcess.Start();

                string output = cirratoProcess.StandardOutput.ReadLine();

                return output;
            });
        }

        /// <summary>
        /// Removes a configuration from a queue in Cirrato asynchronously.
        /// </summary>
        /// <param name="queue">The full path to the queue to modify.</param>
        /// <param name="operatingSystems">The operating systems that the configuration should be removed for.</param>
        /// <returns>Returns the first line of the output of the command from the PMC.</returns>
        public static async Task<string> RemoveQueueConfigurationAsync(string queue, List<string> operatingSystems)
        {
            return await Task.Run(() =>
            {
                // Join all elements in comma separated list
                string osString = string.Join(",", operatingSystems);

                // Start process to create printer in Cirrato
                Process cirratoProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = CirratoPath,
                        Arguments = $"queue modify --target \"{queue}\" -rc \"{osString}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                cirratoProcess.Start();

                string output = cirratoProcess.StandardOutput.ReadLine();

                return output;
            });
        }

        /// <summary>
        /// Create printer in Cirrato asynchronously.
        /// </summary>
        /// <param name="printerPath">Full path and name of the printer to create in Cirrato.</param>
        /// <param name="ip">The IP address of the printer to create.</param>
        /// <param name="driver">The print driver to use for the printer.</param>
        /// <param name="comment">The comment to add to the printer.</param>
        /// <param name="location">The location of the printer.</param>
        /// <returns>
        /// Returns the first line of the output of the command from the PMC. 
        /// </returns>
        public static async Task<string> CreatePrinterAsync(string printerPath, string ip, string driver, string comment, string location)
        {
            return await Task.Run(() =>
            {
                string argumentString = $"printer add --enforceuniquename y --enforceuniqueip y --target \"{printerPath}\" --ip \"{ip}\" --model \"{driver}\" --location \"{location}\" --comment \"{comment}\"";

                // Start process to create printer in Cirrato
                Process cirratoProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = CirratoPath,
                        Arguments = argumentString,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                cirratoProcess.Start();

                string output = cirratoProcess.StandardOutput.ReadLine();

                return output;
            });
        }

        /// <summary>
        /// Adds a printer to the <see cref="PrinterList"/>.
        /// </summary>
        public static void AddPrinter(Printer printer)
        {
            PrinterList.Add(printer);
        }

        /// <summary>
        /// Adds a printer to the <see cref="SysManPrinterList"/>.
        /// </summary>
        public static void AddSysManPrinter(SysManPrinter printer)
        {
            SysManPrinterList.Add(printer);
        }
    }
}
