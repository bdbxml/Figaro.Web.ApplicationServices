/*************************************************************************************************
* 
* THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
* KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
* PARTICULAR PURPOSE.
* 
*************************************************************************************************/
using System;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Web.Security;
using Figaro.Web.ApplicationServices.Data;

namespace Figaro.Web.ApplicationServices.Security
{
    /// <summary>
    /// Manages storage of role information for an ASP.NET application in a Figaro XML database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To use the <see cref="FigaroRoleProvider"/>, you must specify a path and file name in the <c>container</c> setting of the provider configuration. 
    /// </para>
    /// <example>
    /// The following example shows the <c>web.config</c> file for an ASP.NET application used to configure <see cref="FigaroRoleProvider"/>.
    /// <code lang="xml">
        ///&lt;roleManager enabled="true" defaultProvider="FigaroRoleProvider"&gt;
        ///    &lt;providers&gt;
        ///        &lt;clear/&gt;
        ///        &lt;add applicationName="/" 
        ///             container="D:\data\db\Providers\Role.dbxml" 
        ///             name="FigaroRoleProvider" 
        ///             type="Figaro.Web.Security.FigaroRoleProvider, Figaro.Web, Version=2.4.16.1, Culture=neutral, PublicKeyToken=ff51709ab7ef68cf"/&gt;
        ///    &lt;/providers&gt;
        ///&lt;/roleManager&gt;
    /// </code>
    /// </example>
    /// <para>
    /// If the specified container does not exist, a node-storage container will automatically be created for you.
    /// </para>
    /// <para>
    ///  If this provider is used to support multiple applications, the roles created will overlap each other for the applications. For example, if App1 has an 
    /// <b>Administrators</b> role, and the role container is shared with <b>App2</b> also containing an <b>Administrators</b> role, users added to the 
    /// <b>Administrators</b> role will become administrators for both applications.
    /// </para>
    /// <para>
    ///     Roles are stored as XML messages and indexed according to role name. The schema definition for a role entry is shown below:
    /// </para>
    /// <code lang="xml">
    /// &lt;?xml version="1.0" encoding="utf-8"?&gt;
    ///&lt;xs:schema id="FigaroRole" targetNamespace="http://schemas.bdbxml.net/web/role/2009/05/" xmlns:mstns="http://schemas.bdbxml.net/web/role/2009/05/" xmlns="http://schemas.bdbxml.net/web/role/2009/05/" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata" attributeFormDefault="qualified" elementFormDefault="qualified"&gt;
    ///  &lt;xs:element name="FigaroRole" msdata:IsDataSet="true" msdata:UseCurrentLocale="true"&gt;
    ///    &lt;xs:complexType&gt;
    ///      &lt;xs:sequence minOccurs="0" maxOccurs="unbounded"&gt;
    ///        &lt;xs:element name="Apps"&gt;
    ///          &lt;xs:complexType&gt;
    ///            &lt;xs:sequence&gt;
    ///              &lt;xs:element name="App" type="xs:string" minOccurs="0" maxOccurs="unbounded"/&gt;
    ///            &lt;/xs:sequence&gt;
    ///          &lt;/xs:complexType&gt;
    ///        &lt;/xs:element&gt;
    ///        &lt;xs:element name="Users"&gt;
    ///          &lt;xs:complexType&gt;
    ///            &lt;xs:sequence&gt;
    ///              &lt;xs:element name="User" type="xs:string" minOccurs="0" maxOccurs="unbounded"/&gt;
    ///            &lt;/xs:sequence&gt;
    ///          &lt;/xs:complexType&gt;
    ///        &lt;/xs:element&gt;
    ///      &lt;/xs:sequence&gt;
    ///    &lt;/xs:complexType&gt;
    ///  &lt;/xs:element&gt;
    ///&lt;/xs:schema&gt;
    /// </code>
    /// <para>
    ///     The following XML metadata is stored for each role entry:
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Metadata name</term>
    /// <description>Metadata URI</description>
    /// </listheader>
    /// <item>
    ///     <term>
    ///         name
    ///     </term>
    ///     <description>
    ///     http://www.sleepycat.com/2002/dbxml
    ///     </description>
    /// </item>
    /// <item>
    ///     <term>
    ///     DateCreated
    ///     </term>
    ///     <description>
    ///     http://schemas.bdbxml.net/web/role/2009/05/
    ///     </description>
    /// </item>
    /// </list>
    /// </remarks>
    public class FigaroRoleProvider : RoleProvider
    {
        private string container;
        private FigaroRoleData data;
        private static Stopwatch watch;

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.
        ///                 </param><param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.
        ///                 </param><exception cref="T:System.ArgumentNullException">The name of the provider is null.
        ///                 </exception><exception cref="T:System.ArgumentException">The name of the provider has a length of zero.
        ///                 </exception><exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.
        ///                 </exception>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {                
            if (null == config[RoleResource.ConfigContainer])
                throw new ArgumentException("container is a required setting for FigaroRoleProvider.");

            if (string.IsNullOrEmpty(name))
                name = RoleResource.Source;
            
            container = config[RoleResource.ConfigContainer];
            
            if (string.IsNullOrEmpty(config[RoleResource.ConfigAppName]))
                ApplicationName = ProviderUtility.GetDefaultAppName();
            data = new FigaroRoleData(container);
            base.Initialize(name, config);

        }        
        
        #region methods

        /// <summary>
        /// Adds the specified user names to the specified roles in the role container.
        /// </summary>
        /// <param name="usernames">A string array of user names to be added to the specified roles. 
        ///                 </param><param name="roleNames">A string array of the role names to add the specified user names to.
        ///                 </param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            if (usernames == null)
                throw new NullReferenceException("usernames cannot be null.");
            if (roleNames == null)
                throw new NullReferenceException("roleNames cannot be null.");
            try
            {
                foreach (string roleName in roleNames)
                {
                        if (string.IsNullOrEmpty(roleName))
                            throw new NullReferenceException("A null role name was provided to FigaroRoleProvider.");
                        if (!data.RoleExists(roleName)) throw new NullReferenceException("Role '" + roleName + "' does not exist in the container, and must be added first.");
                    foreach (string user in usernames)
                    {
                        if (string.IsNullOrEmpty(user))
                            throw new NullReferenceException("A null user name was provided to FigaroRoleProvider.");

                        data.AddUserToRole(user, roleName, ApplicationName);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "add users to roles"), ex);
            }
        }

        /// <summary>
        /// Adds a new role to the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to create.
        ///                 </param>
        public override void CreateRole(string roleName)
        {
            StartTimer();
            if (data.RoleExists(roleName)) throw new ProviderException("Role '" + roleName + "' already exists.");
            try
            {
                data.CreateRole(roleName, ApplicationName);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "create role"), ex);
            }
            finally
            {
                StopTimer("CreateRole");
            }

        }

        /// <summary>
        /// Removes a role from the data source for the configured applicationName.
        /// </summary>
        /// <returns>
        /// true if the role was successfully deleted; otherwise, false.
        /// </returns>
        /// <param name="roleName">The name of the role to delete.
        ///                 </param><param name="throwOnPopulatedRole">If true, throw an exception if <paramref name="roleName"/> has one or more members and do not delete <paramref name="roleName"/>.
        ///                 </param>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            var rn = "role '" + roleName + "' ";
            var dne = rn + "does not exist in the role container.";
            if (string.IsNullOrEmpty(roleName))
                throw new NullReferenceException(dne);

            if (!data.RoleExists(roleName))
                throw new ArgumentException(dne);

            if (throwOnPopulatedRole)
            {
                if (data.RoleHasMembers(roleName)) throw new ProviderException(rn + "cannot be deleted because it is populated with users.");
            }
            StartTimer();
            try
            {
                return data.DeleteRole(roleName);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "delete role '" + roleName + "'"), ex);
            }
            finally
            {
                StopTimer("DeleteRole");
            }
        }

        /// <summary>
        /// Gets an array of user names in a role where the user name contains the specified user name to match.
        /// </summary>
        /// <remarks>
        /// The XQuery used to retrieve the user names uses the XQuery <c>matches($query as xs:string?, $pattern as xs:string?)</c> function. The <paramref name="usernameToMatch"/> 
        /// parameter goes into the <c>$pattern</c> variable, which also accepts wildcards and regular expressions.
        /// </remarks>
        /// <returns>
        /// A string array containing the names of all the users where the user name matches <paramref name="usernameToMatch"/> and the user is a member of the specified role.
        /// </returns>
        /// <param name="roleName">The role to search in.
        ///                 </param><param name="usernameToMatch">The user name to search for.
        ///                 </param>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(roleName);
            if (string.IsNullOrEmpty(usernameToMatch)) throw new ArgumentNullException(usernameToMatch);
            if (!data.RoleExists(roleName)) throw new ArgumentException("Role name '" + roleName + "' does not exist.");

            StartTimer();

            try
            {
                return data.FindUsersInRole(roleName, usernameToMatch);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "find users in role '" + roleName + "'"), ex);
            }
            finally
            {
                StopTimer("FindUsersInRole");
            }
        }

        /// <summary>
        /// Gets a list of all the roles for the configured applicationName.
        /// </summary>
        /// <returns>
        /// A string array containing the names of all the roles stored in the data source for the configured applicationName.
        /// </returns>
        public override string[] GetAllRoles()
        {
            StartTimer();
            try
            {
                return data.GetAllRoles();
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "get all roles"), ex);
            }
            finally
            {
                StopTimer("GetAllRoles");
            }

        }

        /// <summary>
        /// Gets a list of the roles that a specified user is in for the configured applicationName.
        /// </summary>
        /// <returns>
        /// A string array containing the names of all the roles that the specified user is in for the configured applicationName.
        /// </returns>
        /// <param name="username">The user to return a list of roles for.
        ///                 </param>
        public override string[] GetRolesForUser(string username)
        {
            StartTimer();
            try
            {
                return data.GetRolesForUser(username);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "get roles for user '" + username + "'"), ex);
            }
            finally
            {
                StopTimer("GetRolesForUser");
            }
        }

        /// <summary>
        /// Gets a list of users in the specified role for the configured applicationName.
        /// </summary>
        /// <returns>
        /// A string array containing the names of all the users who are members of the specified role for the configured applicationName.
        /// </returns>
        /// <param name="roleName">The name of the role to get the list of users for.
        ///                 </param>
        public override string[] GetUsersInRole(string roleName)
        {
            StartTimer();
            try
            {
                return data.FindUsersInRole(roleName);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "get users in role '" + roleName + "'"), ex);
            }
            finally
            {
                StopTimer("GetUsersInRole");
            }

        }

        /// <summary>
        /// Gets a value indicating whether the specified user is in the specified role for the configured applicationName.
        /// </summary>
        /// <returns>
        /// true if the specified user is in the specified role for the configured applicationName; otherwise, false.
        /// </returns>
        /// <param name="username">The user name to search for.
        ///                 </param><param name="roleName">The role to search in.
        ///                 </param>
        public override bool IsUserInRole(string username, string roleName)
        {
            StartTimer();
            try
            {
                var roles = data.GetRolesForUser(username);
                foreach (string role in roles)
                {
                    if (role.Equals(roleName)) return true;
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, RoleResource.Source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("IsUserInRole");
            }
            return false;
        }

        /// <summary>
        /// Removes the specified user names from the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be removed from the specified roles. 
        ///                 </param><param name="roleNames">A string array of role names to remove the specified user names from.
        ///                 </param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            if (usernames == null) throw new ArgumentNullException("usernames");
            if (roleNames == null) throw new ArgumentNullException("roleNames");

            StartTimer();
            try
            {
                data.RemoveUsersFromRoles(usernames, roleNames);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "remove users from roles"), ex);
            }
            finally
            {
                StopTimer("RemoveUsersFromRoles");
            }
        }

        /// <summary>
        /// Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
        /// </summary>
        /// <returns>
        /// true if the role name already exists in the data source for the configured applicationName; otherwise, false.
        /// </returns>
        /// <param name="roleName">The name of the role to search for in the data source.
        ///                 </param>
        public override bool RoleExists(string roleName)
        {
            StartTimer();
            try
            {
                return data.RoleExists(roleName);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "determine if role exists"), ex);
            }
            finally
            {
                StopTimer("RoleExists");
            }

        }
        
        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the name of the application to store and retrieve role information for.
        /// </summary>
        /// <returns>
        /// The name of the application to store and retrieve role information for.
        /// </returns>
        public override string ApplicationName {get;set;}

        /// <summary>
        /// Gets the friendly name used to refer to the provider during configuration.
        /// </summary>
        /// <returns>
        /// The friendly name used to refer to the provider during configuration.
        /// </returns>
        public override string Name
        {
            get
            {
                return "FigaroRoleProvider";
            }
        }

        /// <summary>
        /// Gets a brief, friendly description suitable for display in administrative tools or other user interfaces (UIs).
        /// </summary>
        /// <returns>
        /// A brief, friendly description suitable for display in administrative tools or other UIs.
        /// </returns>
        public override string Description
        {
            get
            {
                return "Gets roles stored in a Figaro XML database container.";
            }
        }
        #endregion

        #region helpers
        private static void StartTimer()
        {
            if (watch == null) watch = new Stopwatch();
            watch.Start();
        }
        
        private static void StopTimer(string source)
        {
            watch.Stop();
            Trace("{0} completed in {1} seconds.", source, watch.Elapsed.TotalSeconds);
        }

        private static void Trace(string message, params object[] args)
        {
            TraceHelper.Write(RoleResource.Source,message,args);
        }
        #endregion
    }
}
