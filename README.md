# **Print Prince**
Print Prince aims to centralize administration of printers in environments using both [Cirrato One](https://www.lrsoutputmanagement.com/products/cirrato) and the tool [SysMan (in Swedish)](https://www.inera.se/tjanster/eklient/eklient/), part of the swedish [eKlient coordinated by Inera (in Swedish)](https://www.inera.se/tjanster/eklient/eklient/), as their printer management.

By using the APIs of Cirrato and SysMan, creation of new printers in each system can be done from a single tool. Print Prince optimizes printer management workflow with a GUI simple and quick enough for even the first line support to use.

* **Flexible** - Using Print Prince enables solutions such as login scripts to install printers through the Cirrato Client's API instead.
* **Modern** - Print Prince replaces the dated Microsoft Management Console, implementing Material Design with a modern look and responsive design focused on user experience.
* **Minimize Mistakes** - When working with two systems and needing to create printers in both of them it's easy to forget one, Print Prince prevents that by creating the printer in both by default.
* **Maximize Value** - No more double licenses for the same printer. Print Prince validates creation of printers against both name and IP address to make sure you don't pay double for what you have.
* **Lightweight** - Because Print Prince stores no data on its own, Print Prince is quick. All communication is in real-time with Cirrato and SysMan in your own environment asynchronously to keep the UI responsive.
* **User Authentication** - User rights are assigned in each system as always, Print Prince simply authenticates the user through the APIs to make sure no one unauthorized get into your systems.

# Requirements
The machine running Print Prince requires at least .NET Framework 4.6.1 and the Cirrato Print Management Client (PMC) installed, configured to the Cirrato system in your environment.

If Print Prince is used from a remote server, ability to communicate with Cirrato and SysMan servers needs to be assured according to configuration. The default ports are TCP 80 and 443 for HTTP/HTTPS.

## **Setup**
Install Print Prince through the setup file among the releases, download the zipped tool for a portable version or build the project on your own.

### **Configuration**
Print Prince comes with a .config file where the Cirrato Print PMC installation path and SysMan URL need to be set.

### **Rights**
* Cirrato API rights are assigned through a separate AD group for the API set in the Cirrato Configuration Manager where direct membership of the user is required (no nested groups because of how Cirrato works). When using Print Prince the user needs to log into the Cirrato PMC.
* SysMan API rights are shared with the normal rights assignment in the SysMan settings, Print Prince uses the credentials of the currently logged in user.

## **Using Print Prince**

### **Logging into Print Prince**
If the user is not already logged into the PMC from a separate source such as the CMD, Print Prince will handle the login by prompting the user when started.

![](login.gif)

### **Creating a Printer**
When creating a printer, Print Prince checks for printers in Cirrato containing three underscores, matching the naming convention SITE_BUILDING_FLOOR_xx where xx is a series of numbers starting at 01. If this naming convention is followed, Print Prince suggests the first available number in the series, but the name can be changed freely afterwards if desired.
Print Prince requires each field to be filled, and verifies that the name and IP address is not already used by a printer in Cirrato.

![](create.gif)

### **Listing Printers**
Print Prince lets you easily list all printers in Cirrato with their associated information. If they don't exist with the same name in SysMan they are marked red to make troubleshooting easier. The list implements quick and responsive filtering by name and sorting on each column, and clicking a printer will show what computers or users have the printer installed in SysMan to centralize printer information gathering.

![](list.gif)

## Building Print Prince
The full solution includes two extra projects for documentation and installation files. To build the full project, the [SHFB](http://ewsoftware.github.io/SHFB) and Visual Studio extension [Microsoft Visual Studio Installer Projects](https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2017InstallerProjects) are required on the machine.

This enables creation of automatic documentation based on the XML comments in the code, and building installation files such as .msi.

# Credit
Print Prince implements and uses the following libraries and frameworks:
* [Material Design In XAML Toolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)
* [MVVM Light](http://www.mvvmlight.net/)
* [MVVM Dialogs](https://github.com/FantasticFiasco/mvvm-dialogs)
* [Json.NET](https://www.newtonsoft.com/json)

------

**[GridViewSort](http://www.thomaslevesque.com/2009/08/04/wpf-automatically-sort-a-gridview-continued/)** is written by Thomas Levesque.

[Licensing](https://www.thomaslevesque.com/about/#comment-105)

------

**[ValidatableModel](http://burnaftercoding.com/post/asynchronous-validation-with-wpf-4-5/")** written by Anthyme Caillard is implemented as part of the *ValidatableViewModelBase* class.

[Licensing](https://twitter.com/anthyme/status/1072923162600529923)

------

**[ExceptionExtensions](https://stackoverflow.com/a/35084416)** written by StackOverflow user ThomazMoura inspired the implementation of *ExceptionExtender*.

------

**[DomainManager](https://stackoverflow.com/a/23390899)** is written by StackOverflow user Nate B.

# Disclaimers
While Print Prince is used to assist with printer management in environments using commercial systems, neither Print Prince nor the developer has any affiliation with the owners of these systems, Print Prince simply uses the APIs as they looked during the development of Print Prince.