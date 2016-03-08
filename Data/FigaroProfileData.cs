/*************************************************************************************************
* 
* THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
* KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
* PARTICULAR PURPOSE.
* 
*************************************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.Profile;
using Figaro.Web.Annotations;

namespace Figaro.Web.ApplicationServices.Data
{
    /// <summary>
    /// Data access layer for the profile container, implemented as a singleton instance.
    /// </summary>
    internal class FigaroProfileData
    {
        private const string Source = "FigaroProfileData";
        private static FigaroProfileData instance;
        private readonly string containerPath;
        private Stopwatch watch;
        
        /// <summary>
        /// Get the <see cref="FigaroProfileData"/> instance.
        /// </summary>
        /// <param name="containerPath">the container that should be opened and used.</param>
        /// <returns> an instance of <see cref="FigaroProfileData"/>.</returns>
        [DebuggerStepThrough]
        public static FigaroProfileData GetFigaroProfileData(string containerPath)
        {
            return instance ?? (instance = new FigaroProfileData(containerPath));
        }

        /// <summary>
        /// Gets the <see cref="XmlManager"/> object used in the data access layer.
        /// </summary>
        public XmlManager Manager{ get; private set;}
        /// <summary>
        /// Gets the <see cref="Container"/> object used in the data access layer.
        /// </summary>
        public Container DbContainer { get; private set; }
        /// <summary>
        /// Gets the directory containing the profile container.
        /// </summary>
        public string ContainerDirectory { [UsedImplicitly] get; private set; }
        /// <summary>
        /// Gets the name of the profile container.
        /// </summary>
        public string Containername { get; private set; }

        /// <summary>
        /// Constructs a new instance of <see cref="FigaroProfileData"/>.
        /// </summary>
        /// <param name="containerPath"></param>
        public FigaroProfileData(string containerPath)
        {
            this.containerPath = containerPath;
            Initialize();
        }

        private void Initialize()
        {
            StartTimer();
            try
            {
                ContainerDirectory = Path.GetDirectoryName(containerPath);
                Containername = Path.GetFileName(containerPath);
                
                if (Manager == null)
                    Manager = new XmlManager();
                
                if (DbContainer == null)
                    DbContainer = Manager.OpenContainer(containerPath);

                DbContainer.AddAlias("profile");
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, Source);
                if (null != e) throw e; else throw;
            }
            finally
            {
                StopTimer("Initialize");
            }
        }
        /// <summary>
        /// Performs an insert/update operation against a profile entry.
        /// </summary>
        /// <param name="profile"></param>
        public void SetProfile(FigaroProfileMessage profile)
        {
            StartTimer();
            var userName = profile.Context["UserName"] as string;
            Trace("[SetProfile] userName: {0}", userName);
            var doc = GetProfileDocument(userName);
            try
            {
                profile.SaveMessage();

                using (var newDoc = Manager.CreateDocument(profile.Reader))
                {
                    foreach (DictionaryEntry context in profile.Context)
                    {
                        if (context.Value != null)
                            newDoc.SetMetadata(ProfileResource.xmlns, context.Key as string, 
                                new XmlValue(Convert.ToString(context.Value)));
                    }
                    newDoc.Name = userName;

                    if (null != doc)
                    {
                        DbContainer.UpdateDocument(newDoc,Manager.CreateUpdateContext());
                    }
                    if (null == doc)
                    {
                        DbContainer.PutDocument(newDoc, Manager.CreateUpdateContext(), PutDocumentOptions.None);
                    }
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, Source);
                if (null != e) throw e; else throw;
            }
            finally
            {
                DbContainer.Sync();
                if (doc != null) doc.Dispose();
                StopTimer("SetProfile");
            }
        }

        /// <summary>
        /// Find and retrieve the user's profile data, if it exists.
        /// </summary>
        /// <param name="userName">The name of the user to retrieve. The user name is also the file name in the DbContainer.</param>
        /// <returns>If exists, returns a <see cref="Figaro.XmlDocument"/> containing the user's profile info; otherwise, null is returned.</returns>
        public XmlDocument GetProfileDocument(string userName)
        {            
            StartTimer();
            try
            {
                //if (DbContainer == null) base.Init("FigaroProfileData", userName["DbContainer"] as string, typeof(FigaroProfileMessage),ProfileResource.xmlns,"profile");
                Trace("FigaroProfileData.GetProfileDocument({0})", userName);

                return userName == null ? null : DbContainer.GetDocument(userName);
            }
            catch (XmlException)
            {
                Trace(
                    "FigaroProfileData.GetProfileDocument threw an XmlException. This is by design, and tells us our user's profile doesn't exist.");
                //if the entry doesn't exist, we'll get a Figaro XmlException here. The next version will have a friendlier way to catch this...
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, Source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("GetProfileDocument");
            }
            return null;
        }

        /// <summary>
        /// Returns all profile entries in the profile container.
        /// </summary>
        /// <returns>Profile entries in the container.</returns>
        public XmlResults GetAllProfiles()
        {
            StartTimer();
            try
            {
                
                
                return DbContainer.GetAllDocuments();
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, Source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("GetAllProfiles");
            }
            return null;
        }
        /// <summary>
        /// QueryGet the specified page and amount of profiles to return.
        /// </summary>
        /// <param name="authenticationOption">The <see cref="ProfileAuthenticationOption"/> value.</param>
        /// <param name="pageIndex">The index of the page of results to return.</param>
        /// <param name="pageSize">How many profiles fill a page.</param>
        /// <param name="totalRecords">How many total profiles there are in the profile DbContainer.</param>
        /// <returns>The selected subset of profiles to be submitted into the <see cref="FigaroProfileReader"/> for conversion.</returns>
        public ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            StartTimer();
            int count = 0;
            ProfileInfoCollection profiles = null;
            try
            {
                var whereAnd = new StringBuilder();
                using (var qc = Manager.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, ProfileResource.xmlns);
                    qc.SetNamespace("meta",ProfileResource.xmlns);
                    whereAnd.Append(WhereAuthenticationOption(authenticationOption, "meta"));
                    count = GetProfileCount(whereAnd.ToString(), qc);
                    whereAnd.Append(" and ");
                    var query = string.Format(ProfileResource.GetProfilesPage, whereAnd, GetPageRange(pageIndex, pageSize));
                    Trace("GetAllProfiles query: {0}", query);
                    profiles = FigaroProfileReader.ResultsToProfiles(Manager.Query(query, qc, QueryOptions.None));
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, Source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("GetAllProfiles");
            }
            totalRecords = count;
            return profiles ?? new ProfileInfoCollection();
        }

        /// <summary>
        /// Count the number of profiles matching the specified criteria.
        /// </summary>
        /// <param name="whereClause">The criteria of the lookup.</param>
        /// <param name="qc">The <see cref="QueryContext"/> to perform the query with.</param>
        /// <returns></returns>
        private int GetProfileCount(string whereClause, QueryContext qc)
        {
            var q = string.Format(ProfileResource.DocCount, whereClause);
            using (var results = Manager.Query(q,qc,QueryOptions.None))
            {
                return (int)results.GetSize();
            }
        }
        
        /// <summary>
        /// Set the authentication status for the specified user.
        /// </summary>
        /// <param name="userName">The user to update.</param>
        /// <param name="auth">The authorization status.</param>
        private void SetIsAuthenticated(string userName, string auth)
        {
            SetMetadata(userName, ProfileResource.IsAuthenticated, auth);
        }
        /// <summary>
        /// Update the metadata of the last update timestamp.
        /// </summary>
        /// <param name="userName">the user profile to update.</param>
        private void SetLastUpdatedDate(string userName)
        {
            SetMetadata(userName, "LastUpdatedTime", DateTime.Now.ToString("o"));
        }
        /// <summary>
        /// Update the user profile to indicate the timestamp of the last activity performed against the user's profile.
        /// </summary>
        /// <param name="userName">The user to update.</param>
        private void SetLastActivityDate(string userName)
        {
            SetMetadata(userName, ProfileResource.LastActivityDate, DateTime.Now.ToString("o"));
        }

        /// <summary>
        /// Perform an XQuery update against metadata in the profile container.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="keyName"></param>
        /// <param name="keyValue"></param>
        private void SetMetadata(string userName, string keyName, string keyValue)
        {
            try
            {
                using (var qc = Manager.CreateQueryContext())
                {
                    qc.SetNamespace(string.Empty, ProfileResource.xmlns);
                    qc.SetNamespace("ts", ProfileResource.xmlns);
                    var query = string.Format(ProfileResource.MetadataUpdate, userName, keyName, keyValue);
                    Trace("SetMetadata query: {0}", query);
                    using (var results = Manager.Query(query,qc,QueryOptions.None))
                    {
                        if (results.GetSize() > 0)
                            Trace("DocSetMetaData returned {0} records.", results.GetSize());
                    }
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, Source);
                if (null != e) throw e; throw;
            }
        }

        #region Profile Provider methods
        
        /// <summary>
        /// Perform a property insert/update for a user profile.
        /// </summary>
        /// <param name="context">The profile settings context.</param>
        /// <param name="collection">The profile property collection.</param>
        public void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            StartTimer();
            try
            {
                var isAuthenticated = (bool)context["IsAuthenticated"];
                var user = context[ProfileResource.ContextUserName] as string;
                var userDoc = GetProfileDocument(user);
                if (null == userDoc)
                {
                    InsertNewProfile(context, collection);
                    return;
                }
                var flag = false;

                foreach (SettingsPropertyValue value in collection)
                {
                    
                    if (value.PropertyValue == null && value.Property.DefaultValue != null)
                    {
                        flag = true;
                        break;
                    }
                    if (value.IsDirty && (isAuthenticated || (bool)value.Property.Attributes["AllowAnonymous"]))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag) return;

                foreach (SettingsPropertyValue value in collection)
                {
                    if ((!isAuthenticated && !((bool)value.Property.Attributes["AllowAnonymous"])))
                    {
                        continue;
                    }

                    UpdateProfileProperty(user, value);
                }
                SetLastActivityDate(user);
                SetLastUpdatedDate(user);
                SetIsAuthenticated(user,(string)context[ProfileResource.IsAuthenticated]);
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, Source);
                if (null != e) throw e;
            }
            finally
            {
                DbContainer.Sync();
                StopTimer("SetPropertyValues");
            }
        }
        /// <summary>
        /// Delete profiles from the container.
        /// </summary>
        /// <param name="authenticationOption">The authentication types to delete.</param>
        /// <param name="userInactiveSinceDate">User profiles with an activity date less than or equal to this date will be removed.</param>
        /// <returns>The number of removed profiles.</returns>
        public int DeleteProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            StartTimer();
            try
            {
                using (var qc = Manager.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, ProfileResource.xmlns);
                    qc.SetNamespace("db",ProfileResource.MetadataFileNameNS);
                    qc.SetNamespace("p", ProfileResource.xmlns);
                    qc.SetVariableValue("time",new XmlValue(XmlValueType.DateTime,userInactiveSinceDate.ToString("o")));

                    var sb = new StringBuilder();
                    sb.Append(WhereUserInactive("p", "time"));
                    sb.Append(" and ");
                    sb.Append(WhereAuthenticationOption(authenticationOption, "p"));

                    var query = string.Format(ProfileResource.Get, sb, ProfileResource.FileName);
                    Trace("DeleteProfiles query: {0}", query);
                    return DeleteProfiles(GetFileNames(query, qc).ToArray());
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, Source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("DeleteProfiles");
            }
            return 0;
        }
        /// <summary>
        /// Delete profiles for the specified users.
        /// </summary>
        /// <param name="userNames">The user profiles to delete.</param>
        /// <returns></returns>
        public int DeleteProfiles(string[] userNames)
        {
            var w = new Stopwatch();
            w.Start();
            var count = 0;
            try
            {
                using (var uc = Manager.CreateUpdateContext())
                {
                    foreach (var s in userNames)
                    {
                        DbContainer.DeleteDocument(s, uc);
                        count++;
                    }
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, Source);
                if (null != e) throw e;
            }
            finally
            {
                DbContainer.Sync();
                w.Stop();
                Trace("[{0}] DeleteProfiles deleted {1} profiles in {2} seconds.", Source, count,
                      w.Elapsed.TotalSeconds);
            }
            return count;
        }
        /// <summary>
        /// Return profile information for the specified user.
        /// </summary>
        /// <param name="userName">The user to look up.</param>
        /// <returns>A collection of profile information retrieved for the specified user.</returns>
        public ProfileInfoCollection GetProfile(string userName)
        {
            StartTimer();
            try
            {
                using (var doc = DbContainer.GetDocument(userName))
                {
                   return new ProfileInfoCollection{ FigaroProfileReader.GetProfile(doc)};
                }

            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, Source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("GetProfile");
            }
            return null;
        }
        #endregion
        /// <summary>
        /// Get the file names (otherwise known as profile user names) found in the container.
        /// </summary>
        /// <param name="query">The query to filter the user list with.</param>
        /// <param name="context">The query context associated with the query.</param>
        /// <returns>A list of user names found in the profile container.</returns>
        private List<string> GetFileNames(string query, QueryContext context)
        {
            var list = new List<string>();
            using (var results = Manager.Query(query,context,QueryOptions.None))
            {
                if (results.GetSize() > 0)
                {
                    while (results.HasNext())
                    {
                        list.Add(results.NextValue().AsString);
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// Get the number of inactive profiles in the profile container.
        /// </summary>
        /// <param name="authenticationOption">The authentication level to filter against.</param>
        /// <param name="userInactiveSinceDate">Users with an activity date less than or equal to this date are returned.</param>
        /// <returns>The number of user profiles matching the specified criteria.</returns>
        public int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            StartTimer();
            try
            {
                using (var qc = Manager.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, ProfileResource.xmlns);
                    qc.SetNamespace("p",ProfileResource.xmlns);
                    var sinceDate = userInactiveSinceDate.ToString("o");
                    Trace("querying for users inactive since {0}", sinceDate);
                    qc.SetVariableValue("time",new XmlValue(XmlValueType.DateTime, userInactiveSinceDate.ToString("o")));
                    var sb = new StringBuilder();
                    sb.Append(WhereUserInactive("p", "time"));
                    sb.Append(" and (");
                    sb.Append(WhereAuthenticationOption(authenticationOption, "p"));
                    sb.Append(")");
                    var query = string.Format(ProfileResource.DocQuery, sb);
                    Trace("GetNumberOfInactiveProfiles query: {0}", query);
                    using (var results = Manager.Query(query,qc,QueryOptions.None))
                    {
                        return (int) results.GetSize();
                    }
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, Source);
                if (null != e) throw e;
                throw;
            }
            finally
            {
                StopTimer("GetNumberOfInactiveProfiles");
            }
        }
        /// <summary>
        /// Insert new profile entries into the profile container.
        /// </summary>
        /// <param name="context">The settings context for the profile.</param>
        /// <param name="collection">The profile property collection.</param>
        public void InsertNewProfile(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            StartTimer();
            var user = context[ProfileResource.ContextUserName] as string;
            if (null == user) throw new NullReferenceException("user name does not exist in profile context");
            try
            {
                //var anon = false;
                //try
                //{
                //    new Guid(user);
                //    anon = true;
                //}
                //catch{}
                var msg = new FigaroProfileMessage(context);
                foreach (SettingsPropertyValue value in collection)
                {
                    msg.WritePropertyValue(value);
                }

                msg.SaveMessage();
                var doc = Manager.CreateDocument(msg.Reader);
                doc.SetMetadata(ProfileResource.xmlns,ProfileResource.IsAuthenticated,new XmlValue((bool)context[ProfileResource.IsAuthenticated]));
                doc.SetMetadata(ProfileResource.xmlns, ProfileResource.Anonymous, new XmlValue(false));
                doc.SetMetadata(ProfileResource.xmlns,ProfileResource.LastActivityDate,new XmlValue(DateTime.Now.ToString("o")));
                doc.SetMetadata(ProfileResource.xmlns, ProfileResource.LastUpdatedTime, new XmlValue(DateTime.Now.ToString("o")));
                doc.Name = user;
                DbContainer.PutDocument(doc, Manager.CreateUpdateContext());
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, Source);
                if (null != e) throw e;
            }
            finally
            {
                DbContainer.Sync();
                StopTimer("InsertNewProfile");
            }
        }
        /// <summary>
        /// Perform an update of an existing user's profile.
        /// </summary>
        /// <param name="userName">The user name to update.</param>
        /// <param name="property">The profile property to update.</param>
        public void UpdateProfileProperty(string userName, SettingsPropertyValue property)
        {
            StartTimer();
            try
            {
                var sb = new StringBuilder();
                sb.Append("<Property name=\"");
                sb.Append(property.Name);
                sb.Append("\" serializeAs=\"");
                sb.Append(property.Property.SerializeAs);
                sb.Append("\" propertyType=\"");
                sb.Append(property.Property.PropertyType.AssemblyQualifiedName);
                sb.Append("\">");
                sb.Append(NodeValue(property));
                sb.Append("</Property>");
                Trace("UpdateProfileProperty Record: {0}", sb.ToString());

                using (var qc = Manager.CreateQueryContext())
                {
                    qc.SetNamespace(string.Empty, ProfileResource.xmlns);
                    qc.SetVariableValue("name",property.Name);
                    var query = string.Format(ProfileResource.PropertyUpsert, userName, sb);
                    Trace("UpdateProfileProperty query: {0}", query);
                    using (var results = Manager.Query(query, qc, QueryOptions.None))
                    {
                        if (results.GetSize() > 0)
                            Trace("UpdateProfileProperty returned {0} results", results.GetSize());
                    }
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, Source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("UpdateProfileProperty");
            }
        }

        /// <summary>
        /// Start the timer so we can see how fast our operations are performing.
        /// </summary>
        protected void StartTimer()
        {
            if (null == watch) watch = new Stopwatch();
            watch.Start();
        }
        /// <summary>
        /// Stop the timer and trace the output.
        /// </summary>
        /// <param name="timedAction">The name of the timed action.</param>
        protected void StopTimer(string timedAction)
        {
            watch.Stop();
            Trace("MembershipData.{0} completed in {1} seconds.", timedAction, watch.Elapsed.TotalSeconds);
            watch.Reset();
        }
        /// <summary>
        /// Write our trace output to the <see cref="TraceHelper"/>.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="args">The arguments used in the <paramref name="message"/> argument, if any.</param>
        protected void Trace(string message, params object[] args)
        {
            TraceHelper.Write("FigaroProfileProvider", "[FigaroProfileData] " + message, args);
        }

        /// <summary>
        /// Provide a paged collection of user profiles from the profile container. 
        /// </summary>
        /// <param name="authenticationOption">The authentication option filter.</param>
        /// <param name="userInactiveSinceDate">Users with an activity date less than or equal to this specified timestamp.</param>
        /// <param name="pageIndex">The page of values to return.</param>
        /// <param name="pageSize">The number of records in a page.</param>
        /// <param name="totalRecords">The total number of records in the container matching the specified criteria.</param>
        /// <returns>A collection of user profile information.</returns>
        public ProfileInfoCollection FindPagedInactiveProfiles(ProfileAuthenticationOption authenticationOption, 
            DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            int recordCount;
            ProfileInfoCollection profiles;
            StartTimer();
            try
            {
                using (var qc = Manager.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, ProfileResource.xmlns);
                    qc.SetNamespace("meta", ProfileResource.xmlns);
                    qc.SetVariableValue("time", new XmlValue(XmlValueType.DateTime, userInactiveSinceDate.ToString("o")));
                    var whereClause = WhereUserInactive("ts", "time") + " and " +
                                      WhereAuthenticationOption(authenticationOption, "meta");

                    var queryCount = string.Format(ProfileResource.DocCount, whereClause);
                    var query = string.Format(ProfileResource.GetProfilesPage,
                                              whereClause,
                                              GetPageRange(pageIndex, pageSize));
                    using (var results = Manager.Query(queryCount, qc, QueryOptions.None))
                    {
                        recordCount = (int)results.GetSize();
                    }
                    using (var results = Manager.Query(query, qc, QueryOptions.None))
                    {
                        profiles = FigaroProfileReader.ResultsToProfiles(results);
                    }
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, Source);
                if (null != e) throw e;
                throw;
            }
            finally
            {
                StopTimer("FindPagedInactiveProfiles");
            }
            totalRecords = recordCount;
            return profiles ?? new ProfileInfoCollection();
        }

        private static string WhereUserInactive(string metaDataPrefix, string dateTimeVariableName)
        {
            return "$x/FigaroUserProfile[xs:dateTime(dbxml:metadata('" + metaDataPrefix + ":" +
                   ProfileResource.LastActivityDate + "')) <= $" + dateTimeVariableName + "] "; 
        }

        private static string GetPageRange(int pageIndex, int pageSize)
        {
            return (pageIndex == 0 || pageIndex == 1)
                                               ? string.Format("1 to {0}", pageSize)
                                               : string.Format("{0} to {1}", (pageSize * pageIndex) - pageSize + 1, pageIndex * pageSize);            
        }
        private static string WhereAuthenticationOption(ProfileAuthenticationOption option, string metaDataPrefix)
        {
            var sb = new StringBuilder();
            switch (option)
            {
                case ProfileAuthenticationOption.Authenticated:
                    sb.AppendFormat(ProfileResource.MetadataBoolean, metaDataPrefix, ProfileResource.IsAuthenticated);
                    break;
                case ProfileAuthenticationOption.Anonymous:
                    sb.AppendFormat(ProfileResource.MetadataBoolean, metaDataPrefix, ProfileResource.Anonymous);
                    break;
                case ProfileAuthenticationOption.All:
                    sb.AppendFormat(ProfileResource.MetadataBoolean, metaDataPrefix, ProfileResource.Anonymous);
                    sb.Append(" or ");
                    sb.AppendFormat(ProfileResource.MetadataBoolean, metaDataPrefix, ProfileResource.IsAuthenticated);
                    break;
            }
            return sb.ToString();
        }
        private static string NodeValue(SettingsPropertyValue value)
        {
            object val = null;   
            if ((value.PropertyValue == null || string.IsNullOrEmpty(value.PropertyValue as string)))
            {
                if (!string.IsNullOrEmpty(value.Property.DefaultValue as string))
                {
                    val = value.Property.DefaultValue;
                }
            }
            else
                val = value.Property.SerializeAs.Equals(SettingsSerializeAs.Binary)
                       ? Convert.ToBase64String((byte[]) value.SerializedValue)
                       : value.SerializedValue as string;
            
            return val as string ?? value.SerializedValue as string;
        }

        /// <summary>
        /// Allows an <see cref="T:System.Object"/> to attempt to free resources and perform other 
        /// cleanup operations before the <see cref="T:System.Object"/> is reclaimed by garbage collection.
        /// </summary>
        ~FigaroProfileData()
        {
            if (null != DbContainer)
            {
                DbContainer.Sync();
                DbContainer.Dispose();
            }
            if (null != Manager)
            {
                Manager.Dispose();
            }
            
        }

    }
}
