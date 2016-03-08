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
using System.IO;
using System.Text;
using System.Web.Security;
using System.Xml;
using Figaro.Web.ApplicationServices.Security;
using System.Security.Cryptography;

namespace Figaro.Web.ApplicationServices.Data
{
    /// <summary>
    /// Provides the data access layer to the Figaro container holding the membership data.
    /// </summary>
    /// <remarks>
    /// Because every entry has a unique name, and we require a unique user name identity requirement, we're going to use the member's 
    /// the user name will also serve as the record key in the container.
    /// <para>
    /// This version of the membership class prepares all queries at initialization in order to 
    /// enhance performance.
    /// </para>
    /// </remarks>
    internal class FigaroMemberQuery: FigaroBase
    {
        protected const int Zero = 0;
        protected const int Ten = 10;
        private FigaroMemberQuery()
        {}

        public FigaroMemberQuery(string containerPath): base("FigaroMembershipData",
            containerPath,typeof(FigaroMembershipUser),MemberResource.xmlns,MemberResource.ContainerAlias)
        {
        }

        public FigaroMemberQuery(string containerPath, string mgrName): 
            base("FigaroMembershipData",containerPath, mgrName,typeof(FigaroMembershipUser),MemberResource.xmlns,
            MemberResource.ContainerAlias)
        {
        }

        #region GetNumberOfUsers

        /// <summary>
        /// Get the number of users in the container.
        /// </summary>
        /// <returns>The number of records in the container.</returns>
        public ulong GetNumberOfUsers()
        {
            StartTimer();
            try
            {
                return RecordCount();
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("GetNumberOfUsers");
            }
            return 0;
        }

        #endregion

        #region UpdateUserOnline

        protected void PrepareUpdateUserOnline()
        {
                var dc = new QueryUnit {Context = mgr.CreateQueryContext(EvaluationType.Eager),
                Query = MemberResource.QueryUpdateUserOnline};
                dc.Context.SetNamespace(string.Empty, MemberResource.xmlns);
                dc.Context.SetVariableValue("user", "test");
                dc.Context.SetVariableValue("doc", "dbxml:/membership/$user");
                dc.Context.SetVariableValue("date", DateTime.Now.ToString("o"));
                dc.Expression = mgr.Prepare(dc.Query,dc.Context);
                Trace("UpdateUserOnline query: {0}", dc.Query);
                catalog.Add("UpdateUserOnline", dc);
        }

        public void UpdateUserOnline(string userName)
        {
            if (RecordCount() < 1) throw new ProviderException("FigaroMembershipProvider contains no users.");
            StartTimer();
            try
            {
                if (!catalog.ContainsKey("UpdateUserOnline"))
                    PrepareUpdateUserOnline();

                catalog["UpdateUserOnline"].Context.SetVariableValue("doc", "dbxml:/membership/" + userName);
                catalog["UpdateUserOnline"].Context.SetVariableValue("user", userName);
                catalog["UpdateUserOnline"].Expression.Execute(catalog["UpdateUserOnline"].Context);

            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("UpdateUserOnline");
            }
        }

        #endregion

        #region UpdateUser

        /// <summary>
        /// Update the existing <see cref="FigaroMembershipUser"/> with the information provided by the 
        /// <see cref="MembershipUser"/> properties.
        /// </summary>
        /// <param name="user"></param>
        public void UpdateUser(MembershipUser user)
        {
            if (RecordCount() < 1) throw new ProviderException("FigaroMembershipProvider contains no users.");
            StartTimer();
            try
            {
                var fu = GetUserByUserName(user.UserName);
                fu.Comment = user.Comment;
                fu.Email = user.Email;
                fu.IsApproved = user.IsApproved;
                fu.LastActivityDate = user.LastActivityDate;
                fu.LastLoginDate = user.LastLoginDate;
                UpdateUser(fu);
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("UpdateUser");
            }
        }

        public void UpdateUser(FigaroMembershipUser user)
        {
            if (RecordCount() < 1) throw new ProviderException("FigaroMembershipProvider contains no users.");
            StartTimer();
            try
            {
                user.Password = hashPassword(user.Password);
                using (var stream = new MemoryStream())
                {
                    serializer.Serialize(stream, user);
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(stream))
                    {
                        using (var doc = container.GetDocument(user.UserName))
                        {
                            doc.SetContent(reader.ReadToEnd());
                            container.UpdateDocument(doc, mgr.CreateUpdateContext());
                        }
                        reader.Close();
                    }
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("UpdateUser");
                container.Sync();
            }
        }        
        #endregion

        #region GetUsersByName
        protected void PrepareGetUsersByNameCount()
        {
            //QueryGetUsersByNameCount
            var dc = new QueryUnit
            {
                Context = mgr.CreateQueryContext(EvaluationType.Eager),
                Query = MemberResource.QueryGetUsersByNameCount
            };

            dc.Context.SetNamespace(string.Empty, MemberResource.xmlns);
            dc.Context.SetVariableValue("user", "test");
            dc.Expression = mgr.Prepare(dc.Query, dc.Context);
            Trace("GetUsersByNameCount query: {0}", dc.Query);
            catalog.Add("GetUsersByNameCount", dc);
        }

        protected void PrepareGetUsersByName()
        {
            var dc = new QueryUnit
            {
                Context = mgr.CreateQueryContext(EvaluationType.Eager),
                Query = MemberResource.QueryGetUsersByNamePage
            };

            dc.Context.SetNamespace(string.Empty, MemberResource.xmlns);
            dc.Context.SetVariableValue("user", "test");
            dc.Context.SetVariableValue("i", new XmlValue(Zero));
            dc.Context.SetVariableValue("j", new XmlValue(Ten));
            dc.Expression = mgr.Prepare(dc.Query, dc.Context);
            Trace("GetUsersByName query: {0}", dc.Query);
            catalog.Add("GetUsersByName", dc);

        }

        public MembershipUserCollection GetUsersByName(string userNameToMatch, int pageIndex, int pageSize, 
            out int totalRecords)
        {
            if (RecordCount() < 1)
            {
                totalRecords = 0;
                return new MembershipUserCollection();
            }
            var users = new MembershipUserCollection();
            StartTimer();
            try
            {
                var i = (pageIndex == 0 || pageIndex == 1) ? 1 : (pageSize*pageIndex) - pageSize + 1;
                var j = (pageIndex == 0 || pageIndex == 1) ? pageSize : pageIndex*pageSize;
                if (!catalog.ContainsKey("GetUsersByName"))
                {
                    PrepareGetUsersByName();
                    PrepareGetUsersByNameCount();
                }
                catalog["GetUsersByName"].Context.SetVariableValue("user", userNameToMatch);
                catalog["GetUsersByNameCount"].Context.SetVariableValue("user", userNameToMatch);
                catalog["GetUsersByName"].Context.SetVariableValue("i", new XmlValue(i));
                catalog["GetUsersByName"].Context.SetVariableValue("j", new XmlValue(j));
                var results = catalog["GetUsersByName"].Expression.Execute(catalog["GetUsersByName"].Context);
                users = getUsers(results);
                var total = catalog["GetUsersByNameCount"].Expression.Execute(catalog["GetUsersByNameCount"].Context);
                totalRecords = 0;
                if (total.IsNull() || total.Count < 1) return users;
                
                totalRecords = (int)total.NextValue().AsNumber;
                return users;
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("GetUsersByName");
            }
            totalRecords = 0;
            return users;
        }

        #endregion

        #region GetUserByUserName
        /// <summary>
        /// Query and return the user according to their user name.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public FigaroMembershipUser GetUserByUserName(string userName)
        {
            if (RecordCount() < 1) return null;
            StartTimer();
            try
            {
                var doc = container.GetDocument(userName);
                return doc == null ? null : GetUser(doc.ToString());
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("GetUserByUserName");
            }

            return null;
        }
        #endregion

        #region GetNumberOfOnlineUsers
        protected void PrepareGetNumberOfOnlineUsers()
        {
            //for $x in collection('membership') where xs:dateTime($x/FigaroMembershipUser/LastActivityDate) >= $y return $x
            var dc = new QueryUnit
            {
                Context = mgr.CreateQueryContext(EvaluationType.Eager),
                Query =  string.Format(MemberResource.QueryGet,
                                    string.Format("xs:dateTime({0}{1}) >= {2}", MemberResource.VariableX,
                                                  MemberResource.PathLastActivity, MemberResource.VariableY),
                                    MemberResource.VariableX)
            };


            dc.Context.SetNamespace(string.Empty, MemberResource.xmlns);
            dc.Context.SetVariableValue("y", new XmlValue(XmlValueType.DateTime, DateTime.Now.ToString("o")));
            
            Trace("GetNumberOfOnlineUsers query: {0}", dc.Query);
            dc.Expression = mgr.Prepare(dc.Query, dc.Context);
            catalog.Add("GetNumberOfOnlineUsers", dc);
            //catalog.Add("GetNumberOfOnlineUsers", mgr.Prepare(GetNumberOfOnlineUsersQuery(), qc));
        }
        
        public int GetNumberOfOnlineUsers(DateTime timestamp)
        {
            if (RecordCount() < 1) return 0;
            //for $x in collection('membership') where xs:dateTime($x/FigaroMembershipUser/LastActivityDate) >= $y return $x
            StartTimer();
            try
            {
                if (!catalog.ContainsKey("GetNumberOfOnlineUsers"))
                    PrepareGetNumberOfOnlineUsers();

                catalog["GetNumberOfOnlineUsers"].Context.SetVariableValue("y", new XmlValue(XmlValueType.DateTime, timestamp.ToString("o")));
                var results = catalog["GetNumberOfOnlineUsers"].Expression.Execute(catalog["GetNumberOfOnlineUsers"].Context);

                return (int)results.GetSize();
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("GetNumberOfOnlineUsers");
            }
            return 0;
        }

        #endregion

        #region protected get users
        protected MembershipUserCollection getUsers(XmlResults results)
        {
            var users = new MembershipUserCollection();
            
            if (results.Count <1) return users;

            do
            {
                users.Add(GetUser(results.NextReader()).AsCurrentMembershipUser());
            } while (results.HasNext());

            return users;
        }

        /// <summary>
        /// Run through the necessary steps to extract the <see cref="FigaroMembershipUser"/>.
        /// </summary>
        /// <param name="results">The results to get the <see cref="FigaroMembershipUser"/> from.</param>
        /// <returns></returns>
        protected FigaroMembershipUser GetUser(XmlResults results)
        {
            if (results.IsNull() || results.Count < 1) return null;
            return GetUser(results.NextReader());
        }

        protected FigaroMembershipUser GetUser(string userData)
        {
            var stm = new MemoryStream(Encoding.Default.GetBytes(userData));
            stm.Seek(0, SeekOrigin.Begin);
            stm.ToArray();
            stm.Seek(0, SeekOrigin.Begin);
            var user = serializer.Deserialize(stm) as FigaroMembershipUser;
            if (null != user) user.Password = fromBase64(user.Password);
            return user;
        }

        protected FigaroMembershipUser GetUser(XmlReader result)
        {
            var user = serializer.Deserialize(result) as FigaroMembershipUser;
            if (null != user) user.Password = fromBase64(user.Password);
            return user;
        }
        #endregion

        #region base 64 encoding/decoding
        /// <summary>
        /// decode our 'encrypted' information.
        /// </summary>
        /// <param name="val">The value to decode.</param>
        /// <returns>A decoded string.</returns>
        protected static string fromBase64(string val)
        {
            return Encoding.Unicode.GetString(Convert.FromBase64String(val));
        }
        /// <summary>
        /// Base-64 encode a string.
        /// </summary>
        /// <param name="val">The value to encode.</param>
        /// <returns>The encoded string value.</returns>
        protected static string hashPassword(string val)
        {
            var clearBytes = new UnicodeEncoding().GetBytes(val);
            var hashedBytes = ((HashAlgorithm) CryptoConfig.CreateFromName("MD5")).ComputeHash(clearBytes);
            return BitConverter.ToString(hashedBytes);
            //return Convert.ToBase64String(Encoding.Unicode.GetBytes(val));
        }
        #endregion

        #region GetAllUsers
        public MembershipUserCollection GetAllUsers()
        {
            StartTimer();
            try
            {
                using (var results = container.GetAllDocuments())
                {
                    if (results.Count > 0)
                    {
                        var users = new MembershipUserCollection();
                        do
                        {
                            var u =
                                ((FigaroMembershipUser)serializer.Deserialize(results.NextReader())).
                                    AsCurrentMembershipUser();
                            users.Add(u);
                        } while (results.HasNext());
                        return users;
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
                StopTimer("GetAllUsers");
            }
            return null;
        }
        #endregion

        #region GetAllUsers (paged)
        protected void PrepareGetAllUsers()
        {
            var dc = new QueryUnit
            {
                Context = mgr.CreateQueryContext(EvaluationType.Eager),
                Query = MemberResource.QueryGetAllUsersPage
            };
            dc.Context.SetNamespace(string.Empty, MemberResource.xmlns);
            dc.Context.SetVariableValue("i", new XmlValue(XmlValueType.UntypedAtomic,Zero.ToString()));
            dc.Context.SetVariableValue("j", new XmlValue(XmlValueType.UntypedAtomic,Ten.ToString()));
            dc.Expression = mgr.Prepare(dc.Query, dc.Context);
            catalog.Add("GetAllUsers", dc);
        }
        
        public MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, 
            out int totalRecords)
        {
            if (RecordCount() < 1)
            {
                totalRecords = 0;
                return new MembershipUserCollection();
            }
            
            if (RecordCount() < (ulong)(pageSize * pageIndex))
            {
                pageIndex = ((int)RecordCount() / pageSize);
            }

            StartTimer();
            var users = new MembershipUserCollection();
            if (!catalog.ContainsKey("GetAllUsers"))
                PrepareGetAllUsers();

            var i = (pageIndex == 0 || pageIndex == 1)? 1: (pageSize*pageIndex) - pageSize + 1;
            var j = (pageIndex == 0 || pageIndex == 1) ? pageSize : pageSize*pageIndex;
            var ix = new XmlValue(XmlValueType.UntypedAtomic, i.ToString());
            var jx = new XmlValue(XmlValueType.UntypedAtomic, j.ToString());
            totalRecords = (int)container.GetNumDocuments();
            catalog["GetAllUsers"].Context.SetVariableValue("i", ix);
            catalog["GetAllUsers"].Context.SetVariableValue("j", jx);

            using (var results = catalog["GetAllUsers"].Expression.Execute(catalog["GetAllUsers"].Context))
            {
                if (results.IsNull())
                {
                    totalRecords = 0;
                    return users;
                }
                users = getUsers(results);
                return users;
            }
        }

        protected string GetAllUsersQuery()
        {
            return MemberResource.QueryGetAllUsersPage;
        }

        #endregion

        #region GetUsersByEmail
        protected void PrepareGetUsersByEmailCount()
        {
            var dc = new QueryUnit
            {
                Query = MemberResource.QueryGetUsersByEmailCount1,
                Context = mgr.CreateQueryContext(EvaluationType.Eager)
            };
            dc.Context.SetNamespace(string.Empty, MemberResource.xmlns);
            dc.Context.SetVariableValue("email", "test");
            dc.Expression = mgr.Prepare(dc.Query, dc.Context);
            catalog.Add("GetUsersByEmailCount", dc);
        }

        protected void PrepareGetUsersByEmail()
        {
            var dc = new QueryUnit
            {
                Query = MemberResource.QueryGetUsersByEmailPage,
                Context = mgr.CreateQueryContext(EvaluationType.Eager)
            };

            dc.Context.SetVariableValue("i", new XmlValue(Zero));
            dc.Context.SetVariableValue("j", new XmlValue(Ten));
            dc.Context.SetNamespace(string.Empty, MemberResource.xmlns);
            dc.Context.SetVariableValue("email", "test");
            dc.Expression = mgr.Prepare(dc.Query, dc.Context);
            catalog.Add("GetUsersByEmail", dc);
        }
        
        public MembershipUserCollection GetUsersByEmail(string emailToMatch, int
            pageIndex, int pageSize, out int totalRecords)
        {
            if (RecordCount() < 1)
            {
                totalRecords = 0;
                return new MembershipUserCollection();
            }
            var users = new MembershipUserCollection();
            StartTimer();
            try
            {
                var i = (pageIndex == 0 || pageIndex == 1)? 1: (pageSize*pageIndex) - pageSize + 1;
                var j = (pageIndex == 0 || pageIndex == 1) ? pageSize : pageSize*pageIndex;
                if (!catalog.ContainsKey("GetUsersByEmail"))
                {
                    PrepareGetUsersByEmail();
                    PrepareGetUsersByEmailCount();
                }
                catalog["GetUsersByEmail"].Context.SetVariableValue("i", new XmlValue(i));
                catalog["GetUsersByEmail"].Context.SetVariableValue("j", new XmlValue(j));
                catalog["GetUsersByEmail"].Context.SetVariableValue("email", emailToMatch);
                catalog["GetUsersByEmailCount"].Context.SetVariableValue("email", emailToMatch);

                var results = catalog["GetUsersByEmail"].Expression.Execute(catalog["GetUsersByEmail"].Context);
                var total = catalog["GetUsersByEmailCount"].Expression.Execute(catalog["GetUsersByEmailCount"].Context);

                totalRecords = 0;
                if (!results.IsNull() && results.Count > 0)
                    users = getUsers(results);
                if (total.IsNull() || results.Count < 1) return users;
                
                totalRecords = (int)total.NextValue().AsNumber;
                return users;
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("GetUsersByEmail");
            }
            totalRecords = 0;
            return users;
        }

        protected string GetUsersByEmailQuery()
        {
            return MemberResource.QueryGetUsersByEmailPage;
        }

        #endregion

        #region GetfileNameFromUserName
        /// <summary>
        /// Retrieve each member entry's file name from the metadata. This will come in handy if we 
        /// change our file name naming convention to something other than the current 'user name 
        /// as file name' policy.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string GetFileNameFromUserName(string userName)
        {
            if (RecordCount() < 1) return null;
            StartTimer();
            try
            {
                var qc = mgr.CreateQueryContext(EvaluationType.Eager);
                qc.SetNamespace(string.Empty, MemberResource.xmlns);
                qc.SetNamespace("dbxml", MemberResource.FilenameMetaDataNamespace);
                qc.SetVariableValue("user", userName);
                var qry = string.Format(MemberResource.QueryGetFileName,
                                        MemberResource.WhereUserName);
                using (var results = mgr.Query(qry, qc, QueryOptions.None))
                {
                    if (!results.IsNull() && results.GetSize() > 0)
                    {
                        Trace("GetFileNameFromUserName returns {0} results.", results.GetSize());
                        var ret = results.NextValue();
                        return ret.AsString;
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
                StopTimer("GetFileNameFromUserName");
            }
            return null;

        }
        #endregion

        #region GetUserByObjectKey
        protected void PrepareGetUserByObjectKey()
        {
            var dc = new QueryUnit
            {
                Query = string.Format(MemberResource.QueryGet, 
                    MemberResource.VariableX + MemberResource.PathUserKey + " = " + 
                    MemberResource.VariableKey, MemberResource.VariableX),
                Context = mgr.CreateQueryContext(EvaluationType.Eager)
            };
            Trace("GetUserByObjectKey query: {0}", dc.Query);
            dc.Context.SetNamespace(string.Empty, MemberResource.xmlns);
            dc.Context.SetVariableValue("key", KeyAsString(Guid.NewGuid()));
            dc.Expression = mgr.Prepare(dc.Query, dc.Context);
            catalog.Add("GetUserByObjectKey", dc);
        }

        /// <summary>
        /// Retrieve the membership user from the container with the specified <see cref="FigaroMembershipUser.ProviderUserKey"/>.
        /// </summary>
        /// <param name="key">The <see cref="FigaroMembershipUser.ProviderUserKey"/> to find a user with.</param>
        /// <returns>A <see cref="FigaroMembershipUser"/> if the member exists; otherwise, a <see langword="null"/> value is returned. </returns>
        public FigaroMembershipUser GetUserByObjectKey(object key)
        {
            if (RecordCount() < 1) return null;
            StartTimer();
            try
            {
                if (!catalog.ContainsKey("GetUserByObjectKey")) PrepareGetUserByObjectKey();
                catalog["GetUserByObjectKey"].Context.SetVariableValue("key", KeyAsString(key));
                return GetUser(catalog["GetUserByObjectKey"].Expression.Execute(catalog["GetUserByObjectKey"].Context));
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("GetUserByObjectKey");
            }
            return null;
        }

        

        #endregion

        #region KeyAsString
        protected static string KeyAsString(object key)
        {
            if (key is Guid)
                return ((Guid)key).ToString();
            if (key is string)
                return key as string;

            return Convert.ToString(key);
        }
        #endregion

        #region CreateUser
        public MembershipCreateStatus CreateUser(FigaroMembershipUser user)
        {
            var status = MembershipCreateStatus.Success;
            StartTimer();
            try
            {
                if (user.ProviderUserKey == null)
                    user.ProviderUserKey = Guid.NewGuid();

                user.Password = hashPassword(user.Password);
                var ms = new MemoryStream();
                serializer.Serialize(ms, user);
                ms.Seek(0, SeekOrigin.Begin);
                var reader = XmlReader.Create(ms);
                var doc = mgr.CreateDocument(reader);
                doc.Name = user.UserName;
                container.PutDocument(doc, mgr.CreateUpdateContext());
            }
            catch (Exception ex)
            {
                status = MembershipCreateStatus.ProviderError;
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                //you must sync or your data will disappear when you close the container!
                container.Sync();
                StopTimer("CreateUser " + user.UserName);
            }
            return status;
        }
        #endregion

        #region DeleteUser
        /// <summary>
        /// Delete the user document from the container.
        /// </summary>
        /// <param name="username">the user name of the user to remove.</param>
        /// <param name="deleteRelatedData">not used in this implementation.</param>
        /// <returns></returns>
        public bool DeleteUser(string username, bool deleteRelatedData)
        {
            if (RecordCount() < 1) return true;
            StartTimer();
            try
            {
                container.DeleteDocument(username, mgr.CreateUpdateContext());
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
                StopTimer("DeleteUser");
            }
            return false;
        }
        #endregion

        #region UnlockUser
        protected void PrepareUnlockUser()
        {
            if (catalog.ContainsKey("UnlockUser")) return;
            
            var dc = new QueryUnit
            {
                Context = mgr.CreateQueryContext(EvaluationType.Eager),
                Query = MemberResource.QueryUnlockUser
            };

            dc.Context.SetNamespace(string.Empty, MemberResource.xmlns);
            dc.Context.SetVariableValue("user", "test");
            dc.Context.SetVariableValue("doc", "dbxml:/membership/$user");
            dc.Context.SetVariableValue("locked", "false");

            catalog.Add("UnlockUser",dc);
        }
        public bool UnlockUser(string userName)
        {
            if (RecordCount() < 1) return false;
            StartTimer();
            try
            {
                if (!catalog.ContainsKey("UnlockUser")) PrepareUnlockUser();
                catalog["UnlockUser"].Context.SetVariableValue("user", userName);
                catalog["UnlockUser"].Expression.Execute(catalog["UnlockUser"].Context);
                return true;
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("UnlockUser");
            }
            return false;
        }
        #endregion
            
        #region ResetPassword
        protected void PrepareResetPassword()
        {
            if (catalog.ContainsKey("ResetPassword")) return;
            var dc = new QueryUnit
            {
                Context = mgr.CreateQueryContext(EvaluationType.Eager),
                Query = MemberResource.QueryReplacePassword
            };

            dc.Context.SetNamespace(string.Empty, MemberResource.xmlns);
            dc.Context.SetVariableValue("user", "test");
            dc.Context.SetVariableValue("doc", "dbxml:/membership/$user");
            dc.Context.SetVariableValue("pass", "test");
            catalog.Add("ResetPassword", dc);
            catalog["ResetPassword"].Expression = mgr.Prepare(MemberResource.QueryReplacePassword, dc.Context);
        }

        public bool ResetPassword(string userName, string newPassword)
        {
            if (RecordCount() < 1) return false;

            StartTimer();
            try
            {
                if (!catalog.ContainsKey("ResetPassword")) PrepareResetPassword();
                catalog["ResetPassword"].Context.SetVariableValue("user", userName);
                catalog["ResetPassword"].Context.SetVariableValue("doc", "dbxml:/membership/" + userName);
                catalog["ResetPassword"].Context.SetVariableValue("pass", hashPassword(newPassword));
                catalog["ResetPassword"].Expression.Execute(catalog["ResetPassword"].Context);
                return true;
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("ResetPassword");
            }
            return false;
        }
        #endregion

        #region GetPassword
        
        protected void PrepareGetPassword()
        {
            if (catalog.ContainsKey("GetPassword")) return;
            var dc = new QueryUnit
            {
                Query = MemberResource.QueryGetPassword,
                Context = mgr.CreateQueryContext(EvaluationType.Eager)
            };
            dc.Context.SetNamespace(string.Empty, MemberResource.xmlns);
            dc.Context.SetVariableValue("doc", "dbxml:/membership/test");
            dc.Expression = mgr.Prepare(dc.Query, dc.Context);
            catalog.Add("GetPassword", dc);
        }

        /// <summary>
        /// Returns the password of the membership user.
        /// </summary>
        /// <param name="userName">The user name to look up.</param>
        /// <returns>The password of the membership user.</returns>
        public string GetPassword(string userName)
        {
            if (RecordCount() < 1) throw new ProviderException("FigaroMembershipProvider contains no users.");

            StartTimer();
            try
            {
                if (!catalog.ContainsKey("GetPassword")) PrepareGetPassword();
                catalog["GetPassword"].Context.SetVariableValue("doc", "dbxml:/membership/" + userName);
                var results = catalog["GetPassword"].Expression.Execute(catalog["GetPassword"].Context);
                if (results.IsNull() || results.Count < 1)
                {
                    throw new MembershipPasswordException("Failed to retrieve password for user " + userName);
                }
                return fromBase64(results.NextValue().AsString);
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                throw e ?? ex;
            }
            finally
            {
                StopTimer("QueryGetPassword");
            }
        }
        #endregion

        #region prepareAndAdd
        private void prepareAndAdd(string name, QueryContext ctx, string query)
        {
            if (catalog.ContainsKey(name)) return;
            var dc = new QueryUnit
            {
                Query = query,
                Context = ctx
            };
            dc.Expression = mgr.Prepare(dc.Query, dc.Context);
            catalog.Add(name, dc);
        }
        #endregion

        #region SetNewPassword
        protected void PrepareSetNewPassword()
        {
            if (catalog.ContainsKey("SetNewPassword"))return;

            var qc = mgr.CreateQueryContext(EvaluationType.Eager);
            qc.SetVariableValue("user", "test");
            qc.SetVariableValue("doc", "dbxml:/membership/$user");
            qc.SetVariableValue("val", "test");
            prepareAndAdd("SetNewPassword", qc, MemberResource.QuerySetNewPassword);
            
        }

        public bool SetNewPassword(string username, string newPassword)
        {
            if (RecordCount() < 1) return false;

            StartTimer();
            try
            {
                if (!catalog.ContainsKey("SetNewPassword")) PrepareSetNewPassword();
                catalog["SetNewPassword"].Context.SetVariableValue("user", username);
                catalog["SetNewPassword"].Context.SetVariableValue("val", newPassword);
                catalog["SetNewPassword"].Expression.Execute(catalog["SetNewPassword"].Context);
                return true;
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("SetNewPassword");
            }
            return false;
        }
        #endregion

        #region ValidateUser
        protected void PrepareValidateUser()
        {
            var qc = mgr.CreateQueryContext(EvaluationType.Eager);
            qc.SetNamespace(string.Empty, MemberResource.xmlns);
            qc.SetVariableValue("user", "test");
            qc.SetVariableValue("pwd", "test");
            prepareAndAdd("ValidateUser", qc, MemberResource.QueryValidateUser);
            Trace("ValidateUser query: {0}", MemberResource.QueryValidateUser);
        }
        public bool ValidateUser(string username, string password)
        {
            if (RecordCount() < 1) return false;
            StartTimer();
            try
            {
                if (!catalog.ContainsKey("ValidateUser")) 
                    PrepareValidateUser();

                catalog["ValidateUser"].Context.SetVariableValue("user", username);
                catalog["ValidateUser"].Context.SetVariableValue("pwd", hashPassword(password));
                var results = catalog["ValidateUser"].Expression.Execute(catalog["ValidateUser"].Context);
                return results.Count > 0;
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("ValidateUser");
            }
            return false;
        }

        protected static string ValidateUserQuery()
        {
            var where = string.Format("{0} and {1}",
                string.Format("$x{0} = $user", MemberResource.PathUserName),
                string.Format("$x{0} = $pwd", MemberResource.PathPassword));
            return string.Format(MemberResource.QueryGetUser, where);
        }

        #endregion

        #region GetUserName
        protected void PrepareGetUserName()
        {
            var qc = mgr.CreateQueryContext(EvaluationType.Eager);
            qc.SetVariableValue("doc", "dbxml:/membership/test");
            qc.SetNamespace(string.Empty, MemberResource.xmlns);
            prepareAndAdd("GetUserName",qc,"xs:string(doc($doc)" + MemberResource.PathUserName + ")");
        }
        public string GetUserName(string userName)
        {
            if (RecordCount() < 1) return null;
            StartTimer();
            try
            {
                if (!catalog.ContainsKey("GetUserName")) PrepareGetUserName();
                Trace("GetUserName query: {0}", catalog["GetUserName"].Query);
                catalog["GetUserName"].Context.SetVariableValue("doc", "dbxml:/membership/" + userName);
                using (var results = catalog["GetUserName"].Expression.Execute(catalog["GetUserName"].Context))
                {
                    if (results.IsNull() || results.Count < 1) return null;
                    return results.NextValue().AsString;
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("GetUserName");
            }
            return null;
        }
        protected static string GetUserNameQuery()
        {
            return "doc($doc)" + MemberResource.PathUserName;
        }

        #endregion

        #region GetUserPassword
        protected void PrepareGetUserPassword()
        {
            var qc = mgr.CreateQueryContext(EvaluationType.Eager);
            qc.SetNamespace(string.Empty, MemberResource.xmlns);
            qc.SetVariableValue("user", "test");
            var qry = string.Format(MemberResource.QueryGet,
                                       string.Format("{0}{1} = {2}", MemberResource.VariableX, MemberResource.PathUserName, MemberResource.VariableUser),
                                       string.Format("xs:string({0}{1})",
                                       MemberResource.VariableX, MemberResource.PathPassword));
            Trace("GetUserPassword query: " + qry);
            prepareAndAdd("GetUserPassword", qc, qry);
        }
        /// <summary>
        /// Gets the unencrypted password value for the membership user.
        /// </summary>
        /// <param name="userName">The user to extract the password for.</param>
        /// <returns>Returns the string value of the password if the user and password exist; otherwise, <see langword="null"/> is returned.</returns>
        public string GetUserPassword(string userName)
        {
            if (RecordCount() < 1) return null;
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                if (!catalog.ContainsKey("GetUserPassword")) PrepareGetUserPassword();
                catalog["GetUserPassword"].Context.SetVariableValue("user", userName);
                var results = catalog["GetUserPassword"].Expression.Execute(catalog["GetUserPassword"].Context);
                return results.IsNull() || results.Count < 1 ? null : fromBase64(results.NextValue().AsString);
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                sw.Stop();
                Trace("GetUserPassword completed in {0} seconds ({1} ms).", sw.Elapsed.TotalSeconds,sw.Elapsed.TotalMilliseconds);
            }
            return null;
        }

        #endregion

        #region ChangePasswordQuestionAndAnswer
        protected void PrepareChangePasswordQuestionAndAnswer()
        {
            var qc = mgr.CreateQueryContext(EvaluationType.Eager);
            qc.SetNamespace(string.Empty, MemberResource.xmlns);
            //qc.SetVariableValue("user", "test");
            qc.SetVariableValue("doc", "dbxml:/membership/test");
            qc.SetVariableValue("question", string.Empty);
            qc.SetVariableValue("answer", string.Empty);
            prepareAndAdd("ChangePasswordQuestion", qc, MemberResource.QueryChangePasswordQuestion);
            prepareAndAdd("ChangePasswordAnswer", qc, MemberResource.QueryChangePasswordAnswer);
            Trace("ChangePasswordQuestion query: {0}", MemberResource.QueryChangePasswordQuestion);
            Trace("ChangePasswordAnswer query: {0}", MemberResource.QueryChangePasswordAnswer);
        }

        public bool ChangePasswordQuestionAndAnswer(string userName, string newPasswordQuestion, 
            string newAnswer)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                if (!catalog.ContainsKey("ChangePasswordQuestionAndAnswer")) PrepareChangePasswordQuestionAndAnswer();
                //password question
                //catalog["ChangePasswordQuestion"].Context.SetVariableValue("user", userName);
                catalog["ChangePasswordQuestion"].Context.SetVariableValue("doc", "dbxml:/membership/" + userName);
                catalog["ChangePasswordQuestion"].Context.SetVariableValue("question", newPasswordQuestion);
                catalog["ChangePasswordQuestion"].Context.SetVariableValue("answer", newAnswer);
                Trace("$doc: {0}",catalog["ChangePasswordQuestion"].Context.GetVariableXmlValue("doc"));
                catalog["ChangePasswordQuestion"].Expression.Execute(catalog["ChangePasswordQuestion"].Context);

                //password answer
                //catalog["ChangePasswordAnswer"].Context.SetVariableValue("user", userName);
                catalog["ChangePasswordAnswer"].Context.SetVariableValue("doc", "dbxml:/membership/" + userName);
                catalog["ChangePasswordAnswer"].Context.SetVariableValue("question", newPasswordQuestion);
                catalog["ChangePasswordAnswer"].Context.SetVariableValue("answer", newAnswer);
                Trace("$doc: {0}", catalog["ChangePasswordAnswer"].Context.GetVariableXmlValue("doc"));
                catalog["ChangePasswordAnswer"].Expression.Execute(catalog["ChangePasswordQuestion"].Context);
                return true;
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                sw.Stop();
                Trace("ChangePasswordQuestionAndAnswer completed in {0} seconds ({1} ms).", sw.Elapsed.TotalSeconds,sw.Elapsed.TotalMilliseconds);
            }
            return false;
        }
        #endregion

        #region GetPasswordAnswer
        
        protected void PrepareGetPasswordAnswer()
        {
            var qc = mgr.CreateQueryContext(EvaluationType.Eager);
            qc.SetNamespace(string.Empty, MemberResource.xmlns);
            qc.SetVariableValue("user", "test");
            qc.SetVariableValue("doc", "dbxml:/membership/" + "test");
            Trace("GetPasswordAnswer $doc: {0}", qc.GetVariableXmlValue("doc"));
            Trace("GetPasswordAnswer: {0}", MemberResource.QueryGetPasswordAnswer);
            prepareAndAdd("GetPasswordAnswer", qc, MemberResource.QueryGetPasswordAnswer);
        }

        public string GetPasswordAnswer(string userName)
        {
            if (RecordCount() < 1) throw new ProviderException("FigaroMembershipProvider contains no users.");
            StartTimer();
            try
            {
                if (!catalog.ContainsKey("GetPasswordAnswer")) PrepareGetPasswordAnswer();
                catalog["GetPasswordAnswer"].Context.SetVariableValue("user", userName);
                catalog["GetPasswordAnswer"].Context.SetVariableValue("doc", "dbxml:/membership/" + userName);
                var result = catalog["GetPasswordAnswer"].Expression.Execute(catalog["GetPasswordAnswer"].Context);
                if (result.IsNull() || result.Count < 1) return null;
                return result.NextValue().AsString;
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("GetPasswordAnswer");
            }
            return null;
        }
        #endregion

        #region GetUserNameByEmail
        protected void PrepareGetUserNameByEmail()
        {
            var qc = mgr.CreateQueryContext(EvaluationType.Eager);
            qc.SetNamespace(string.Empty, MemberResource.xmlns);
            qc.SetVariableValue("email", "test");
            prepareAndAdd("GetUserNameByEmail", qc, MemberResource.QueryGetUserNameByEmail);
        }
        public string GetUserNameByEmail(string email)
        {
            if (RecordCount() < 1) return null;
            StartTimer();
            try
            {
                if (!catalog.ContainsKey("GetUserNameByEmail")) 
                    PrepareGetUserNameByEmail();

                catalog["GetUserNameByEmail"].Context.SetVariableValue("email", email);
                var results = catalog["GetUserNameByEmail"].Expression.Execute(catalog["GetUserNameByEmail"].Context);
                return results.IsNull() ? null : results.NextValue().AsString;
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("GetUserNameByEmail");
            }
            return null;
        }
        #endregion

    }
}
