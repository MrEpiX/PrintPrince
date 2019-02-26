using System;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Text;

namespace PrintPrince.Services
{
    /// <summary>
    /// Manages and gets information about the current domain.
    /// </summary>
    /// <remarks>
    /// DomainManager is largely written by StackOverflow user Nate B at https://stackoverflow.com/a/23390899.
    /// </remarks>
    public static class DomainManager
    {
        /// <summary>
        /// Initializes the DomainManager.
        /// </summary>
        static DomainManager()
        {
            UserName = Environment.UserName;
            AccessGroup = ConfigurationManager.AppSettings["AccessADGroup"];

            // If group is empty all users can use the tool, otherwise they are restricted unless member of AccessADGroup
            if (AccessGroup != "")
            {
                // See if user is member of group, or nested group
                var filter = $"(&(sAMAccountName={UserName})(memberOf:1.2.840.113556.1.4.1941:={AccessGroup}))";

                var searcher = new DirectorySearcher(filter);

                var result = searcher.FindOne();

                if (result == null)
                {
                    ReadOnlyAccess = true;
                }
                else
                {
                    ReadOnlyAccess = false;
                }
            }
            else
            {
                ReadOnlyAccess = false;
            }

            Domain domain = null;
            DomainController domainController = null;
            try
            {
                domain = Domain.GetCurrentDomain();
                DomainName = domain.Name.Split('.')[0];
                domainController = domain.PdcRoleOwner;
                DomainControllerName = domainController.Name.Split('.')[0];
                ComputerName = Environment.MachineName;
            }
            finally
            {
                if (domain != null)
                {
                    domain.Dispose();
                }
                if (domainController != null)
                {
                    domainController.Dispose();
                }
            }
        }

        /// <summary>
        /// Name of current user.
        /// </summary>
        public static string UserName { get; private set; }

        /// <summary>
        /// DistinguishedName of AD group to use for members that need more than read access, empty if all users of tool should have full access.
        /// </summary>
        public static string AccessGroup { get; set; }

        /// <summary>
        /// If user has read-only access because they lack membership in <see cref="AccessGroup"/>.
        /// </summary>
        public static bool ReadOnlyAccess { get; set; }

        /// <summary>
        /// Name of the primary domain controller.
        /// </summary>
        public static string DomainControllerName { get; private set; }

        /// <summary>
        /// The name of the computer this application is being run from.
        /// </summary>
        public static string ComputerName { get; private set; }

        /// <summary>
        /// The name of the current domain.
        /// </summary>
        public static string DomainName { get; private set; }

        /// <summary>
        /// Path of the domain.
        /// </summary>
        public static string DomainPath
        {
            get
            {
                bool bFirst = true;
                StringBuilder sbReturn = new StringBuilder(200);
                string[] strlstDc = DomainName.Split('.');
                foreach (string strDc in strlstDc)
                {
                    if (bFirst)
                    {
                        sbReturn.Append("DC=");
                        bFirst = false;
                    }
                    else
                    {
                        sbReturn.Append(",DC=");
                    }

                    sbReturn.Append(strDc);
                }
                return sbReturn.ToString();
            }
        }

        /// <summary>
        /// Gets the LDAP connection string for directory searches.
        /// </summary>
        public static string RootPath
        {
            get
            {
                return string.Format("LDAP://{0}/{1}", DomainName, DomainPath);
            }
        }
    }
}
