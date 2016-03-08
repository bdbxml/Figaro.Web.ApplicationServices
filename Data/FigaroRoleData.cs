/*************************************************************************************************
* 
* THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
* KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
* PARTICULAR PURPOSE.
* 
*************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Figaro.Web.ApplicationServices.Data
{
    /// <summary>
    /// The data access layer for the Figaro ASP.NET Role Provider.
    /// </summary>
    class FigaroRoleData : FigaroBase
    {        
        /// <summary>
        /// Instantiates a new instance of the <see cref="FigaroRoleData"/> data access layer.
        /// </summary>
        /// <param name="containerPath">The full path to the container used for role data.</param>
        public FigaroRoleData(string containerPath) : base(RoleResource.Source, containerPath, null, RoleResource.xmlns, "role") { }
        
        /// <summary>
        /// Create a new Role entry in the container.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        /// <param name="appName">The application requesting creation of the role.</param>
        /// <remarks>
        /// This method will check for existence of an entry with the same role name in the container. If the role exists, the application name will 
        /// be added to the <c>&lt;Apps&gt;</c> list. If the role does not exist, the role will be created and the calling <paramref name="appName"/> 
        /// will be added as an entry in the <c>&lt;Apps&gt;</c> list.
        /// </remarks>
        public void CreateRole(string roleName, string appName)
        {
            StartTimer();
            try
            {
                var role = mgr.CreateDocument();
                role.SetMetadata(RoleResource.xmlns, RoleResource.MetadataDateCreated, new XmlValue(XmlValueType.DateTime, DateTime.Now.ToString("o")));
                role.SetContent(RoleResource.RoleRecord.Replace("<Apps></Apps>", "<Apps><App>" + appName + "</App></Apps>"));
                role.Name = roleName;
                container.PutDocument(role, mgr.CreateUpdateContext());
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
                throw;
            }
            finally
            {
                container.Sync();
                StopTimer("CreateRole");
            }
        }

        /// <summary>
        /// Adds a user to the specified role.
        /// </summary>
        /// <param name="userName">The user to add.</param>
        /// <param name="roleName">The role to add the user to.</param>
        /// <param name="appName">The name of the calling application.</param>
        public void AddUserToRole(string userName, string roleName, string appName)
        {
            StartTimer();
            try
            {
                UpsertAppName(appName, roleName);
                var sb = new StringBuilder("<User>");
                sb.Append(userName);
                sb.Append("</User>");
                using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, RoleResource.xmlns);
                    var query = string.Format(RoleResource.QueryInsertUser, sb, roleName);
                    Trace("AddUserToRole query: " + query);
                    using (mgr.Query(query, qc, QueryOptions.None))
                    { }
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
                throw;
            }
            finally
            {
                container.Sync();
                StopTimer("AddUserToRole");
            }
        }
        
        /// <summary>
        /// Checks for the existence of a role in the role container.
        /// </summary>
        /// <param name="roleName">The name of the role to look for.</param>
        /// <returns><see langword="true"/> if the role exists; <see langword="false"/> if the role does not exist.</returns>
        public bool RoleExists(string roleName)
        {
            var w = new Stopwatch();
            w.Start();
            try
            {
                using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, RoleResource.xmlns);
                    qc.SetNamespace("db", RoleResource.MetadataFileNameNS);
                    qc.SetVariableValue("fileName", roleName);
                    var query = string.Format(RoleResource.QueryRoleExists, "db", "name", "fileName");
                    using (var results = mgr.Query(query, qc, QueryOptions.None))
                    {
                        return results.GetSize() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
                throw;
            }
            finally
            {
                StopTimer("RoleExists");
            }
        }

        /// <summary>
        /// Deletes the role entry from the container.
        /// </summary>
        /// <param name="roleName">The name of the role to remove.</param>
        /// <returns><see langword="true"/> if the role was successfully removed; <see langword="false"/> if there was a problem removing the role from the container.</returns>
        public bool DeleteRole(string roleName)
        {
            StartTimer();
            try
            {
                container.DeleteDocument(roleName, mgr.CreateUpdateContext());
                return true;
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                container.Sync();
                StopTimer("DeleteRole");
            }
            return false;
        }
        /// <summary>
        /// Runs a check to see how many users are stored in a specified role.
        /// </summary>
        /// <param name="roleName">The name of the role to evaluate.</param>
        /// <returns><see langword="true"/> if the number of users in a role is greater than zero; <see langword="false"/> if there are no users.</returns>
        public bool RoleHasMembers(string roleName)
        {
            StartTimer();
            try
            {
                using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, RoleResource.xmlns);
                    var query = string.Format(RoleResource.QueryUsers, roleName);
                    using (var results = mgr.Query(query, qc, QueryOptions.None))
                    {
                        return results.GetSize() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("RoleHasMembers");
            }
            return false;
        }
        /// <summary>
        /// Searches for users in a specified role.
        /// </summary>
        /// <param name="roleName">The role to look for users in.</param>
        /// <param name="userToMatch">The user name to look for.</param>
        /// <returns>An array of users matching the user requested, if found.</returns>
        /// <remarks>
        /// This method does a search for users matching the criteria specified in <paramref name="userToMatch"/>, which can be a wildcard or regular expression 
        /// according to the XQuery documentation for the <c>matches(string$,string$)</c> function used to perform the search.
        /// </remarks>
        public string[] FindUsersInRole(string roleName, string userToMatch)
        {
            StartTimer();
            try
            {
                using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, RoleResource.xmlns);
                    qc.SetNamespace("db", RoleResource.MetadataFileNameNS);
                    var query = string.Format(RoleResource.QueryUserMatchInRole, roleName, userToMatch);
                    Trace("FindUsersInRole query: {0}", query);
                    using (var results = mgr.Query(query, qc, QueryOptions.None))
                    {
                        if (results.GetSize() > 0)
                        {
                            var users = new List<string>();
                            while (results.HasNext())
                            {
                                users.Add(results.NextValue().AsString);
                            }
                            return users.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("FindUsersInRole");
            }
            return null;
        }
        /// <summary>
        /// Get a list of users associated with a particular role.
        /// </summary>
        /// <param name="roleName">The role to retrieve all users for.</param>
        /// <returns>An array of users found in the role.</returns>
        public string[] FindUsersInRole(string roleName)
        {
            StartTimer();
            try
            {
                using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, RoleResource.xmlns);
                    qc.SetNamespace("db", RoleResource.MetadataFileNameNS);
                    var query = string.Format(RoleResource.QueryUsersInRole, roleName);
                    using (var results = mgr.Query(query, qc, QueryOptions.None))
                    {
                        var users = new List<string>();
                        while (results.HasNext())
                        {
                            users.Add(results.NextValue().AsString);
                        }
                        return users.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("FindUsersInRole");
            }
            return new string[0];
        }
        /// <summary>
        /// Get a list of all roles found in the role container.
        /// </summary>
        /// <returns>
        /// An array of roles found in the container.
        /// </returns>
        public string[] GetAllRoles()
        {
            StartTimer();
            try
            {
                using (var results = container.GetAllDocuments())
                {
                    var roles = new List<string>();
                    while (results.HasNext())
                    {
                        roles.Add(results.NextDocument().Name);
                    }
                    return roles.ToArray();
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
                throw;
            }
            finally
            {
                StopTimer("GetAllRoles");
            }
        }
        /// <summary>
        /// Gets a list of roles containing the specified user.
        /// </summary>
        /// <param name="userName">The user name to search for.</param>
        /// <returns>An array of role names the user is associated with.</returns>
        public string[] GetRolesForUser(string userName)
        {
            StartTimer();
            try
            {
                using (var qc = mgr.CreateQueryContext())
                {
                    qc.SetNamespace(string.Empty, RoleResource.xmlns);
                    qc.SetVariableValue("user", userName);
                    using (var results = mgr.Query(RoleResource.QueryRolesForUser, qc, QueryOptions.None))
                    {
                        var roles = new List<string>();
                        while (results.HasNext())
                        {
                            roles.Add(results.NextDocument().Name);
                        }
                        return roles.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("GetRolesForUser");
            }
            return new string[0];
        }
        
        /// <summary>
        /// Remove the specified users from the specified roles.
        /// </summary>
        /// <param name="users">The users to remove.</param>
        /// <param name="roles">The roles to remove the users from.</param>
        public void RemoveUsersFromRoles(string[] users, string[] roles)
        {
            StartTimer();
            try
            {
                foreach (string role in roles)
                {
                    using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
                    {
                        qc.SetNamespace(string.Empty, RoleResource.xmlns);
                        foreach (string user in users)
                        {
                            qc.SetVariableValue("user",user);
                            var query = string.Format(RoleResource.QueryDeleteUserFromRole, role);
                            using (mgr.Query(query, qc, QueryOptions.None)){}                            
                        } 
                    }                
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
                throw;
            }
            finally
            {
                container.Sync();
                StopTimer("RemoveUsersFromRoles");
            }
        }
        
        /// <summary>
        /// Perform an insert/update operation on an application name in a role.
        /// </summary>
        /// <param name="appName">The application name.</param>
        /// <param name="roleName">The role name.</param>
        private void UpsertAppName(string appName, string roleName)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                using (var qc = mgr.CreateQueryContext())
                {
                    qc.SetNamespace(string.Empty, RoleResource.xmlns);
                    qc.SetVariableValue("name", appName);
                    var query = string.Format(RoleResource.QueryRoleAppNameUpsert, roleName);
                    using (mgr.Query(query, qc, QueryOptions.None)) { }
                }

            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                container.Sync();
                sw.Stop();
                Trace("UpsertAppName completed in {0} seconds.", sw.Elapsed.TotalSeconds);
            }
        }

    }
}
