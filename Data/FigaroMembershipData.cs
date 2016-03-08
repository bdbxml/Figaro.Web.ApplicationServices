/*************************************************************************************************
* 
* THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
* KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
* PARTICULAR PURPOSE.
* 
*************************************************************************************************/
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.Security;
using System.Xml;
using Figaro.Web.ApplicationServices.Security;
namespace Figaro.Web.ApplicationServices.Data
{
    /// <summary>
    /// Provides the data access layer to the Figaro container holding the membership data.
    /// </summary>
    /// <summary>
    /// Because every entry has a unique name, and we typically have a unique username identity requirement, we're going to use the member's 
    /// user name in our container.
    /// </summary>
    internal class FigaroMembershipData : FigaroBase
    {
        public ulong GetNumberOfUsers()
        {
            StartTimer();
            try
            {
               return container.GetNumDocuments();
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
        public FigaroMembershipData(string containerPath): base("FigaroMembershipData",containerPath,typeof(FigaroMembershipUser),MemberResource.xmlns,MemberResource.ContainerAlias){}

        public FigaroMembershipUser GetUserByUserName(string userName)
        {
            StartTimer();
            try
            {
                using (var qc = mgr.CreateQueryContext())
                {
                    var qry = string.Format(MemberResource.QueryGet,
                                               MemberResource.VariableX + 
                                               MemberResource.PathUserName + " = " 
                                               + MemberResource.VariableUser,
                                               MemberResource.VariableX);
                    qc.SetNamespace(string.Empty, MemberResource.xmlns);
                    qc.SetVariableValue("user", userName);
                    Trace("GetUserByUserName query: {0}", qry);
                    return GetUser(qry, qc);
                }
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
        
        private FigaroMembershipUser GetUser(string query, QueryContext queryContext)
        {
            StartTimer();
            try
            {
                using (var results = mgr.Query(query,queryContext,QueryOptions.None))
                {
                    if (results.IsNull()) return null;
                    Trace("QueryGetUser returned {0} results.", results.GetSize());
                    var user = serializer.Deserialize(results.NextReader()) as FigaroMembershipUser;
                    if (null != user) user.Password = fromBase64(user.Password);
                    return user;
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("QueryGetUser");
            }
            return null;
        }

        public int GetNumberOfOnlineUsers(DateTime timestamp)
        {
            StartTimer();
            try
            {
                //for $x in collection('membership') where xs:dateTime($x/FigaroMembershipUser/LastActivityDate) >= $y return $x
                using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, MemberResource.xmlns);
                    qc.SetVariableValue("y", new XmlValue(XmlValueType.DateTime, timestamp.ToString("o")));

                    var qry = string.Format(MemberResource.QueryGet,
                                            string.Format("xs:dateTime({0}{1}) >= {2}", MemberResource.VariableX,
                                                          MemberResource.PathLastActivity, MemberResource.VariableY),
                                            MemberResource.VariableX);
                    Trace("GetNumberOfOnlineUsers query: {0}", qry);
                    using (var results = mgr.Query(qry,qc,QueryOptions.None))
                    {
                        return (int)results.GetSize();
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
                StopTimer("GetNumberOfOnlineUsers");
            }
            return 0;
        }
        
        public void UpdateUser(FigaroMembershipUser user)
        {
            StartTimer();
            try
            {
                user.Password = toBase64(user.Password);
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
            }
        }        
        
        public MembershipUserCollection GetAllUsers()
        {
            StartTimer();
            try
            {
                using (var results = container.GetAllDocuments())
                {
                    if (!results.IsNull())
                    {
                        var users = new MembershipUserCollection();
                        do
                        {
                            var u =
                                ((FigaroMembershipUser) serializer.Deserialize(results.NextReader())).
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

        public MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            StartTimer();
            var users = new MembershipUserCollection();
            try
            {
                var where = (pageIndex == 0 || pageIndex == 1)
                                   ? string.Format("1 to {0}", pageSize)
                                   : string.Format("{0} to {1}", (pageSize*pageIndex) - pageSize+1, pageIndex*pageSize);
                var qry = string.Format(MemberResource.QueryGetUsersPage, where);
                using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
                {
                    using (var results = mgr.Query(qry,qc,QueryOptions.None))
                    {
                        Trace("GetAllUsers returned {0} results.", results.GetSize());
                        if (results.GetSize() > 0)
                        {
                            do
                            {
                                users.Add(((FigaroMembershipUser) serializer.Deserialize(results.NextReader())).AsCurrentMembershipUser());
                            } while (results.HasNext());
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
                StopTimer("GetAllUsers");
            }
            totalRecords = (int)container.GetNumDocuments();
            return users;
        }
        
        public MembershipUserCollection GetUsersByName(string userNameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            var users = new MembershipUserCollection();
            int total = 0;
            StartTimer();
            try
            {
                var where = (pageIndex == 0 || pageIndex == 1)
                                   ? string.Format("1 to {0}", pageSize)
                                   : string.Format("{0} to {1}", (pageSize * pageIndex) - pageSize + 1, pageIndex * pageSize);
                var qry = string.Format(MemberResource.QueryGetUsersByNamePage, where);
                using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, MemberResource.xmlns);
                    qc.SetVariableValue("user", userNameToMatch);

                    using (var results = mgr.Query(qry, qc, QueryOptions.None))
                    {
                        Trace("GetUsersByEmail returned {0} results.", results.GetSize());
                        if (results.GetSize() > 0)
                        {
                            do
                            {
                                users.Add(((FigaroMembershipUser)serializer.Deserialize(results.NextReader())).AsCurrentMembershipUser());
                            } while (results.HasNext());
                        }
                    }
                    using (var countResults = mgr.Query(MemberResource.QueryCountUsersByUserName, qc, QueryOptions.None))
                    {
                        total = (int)countResults.GetSize();
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
                StopTimer("GetUsersByName");
            }
            totalRecords = total;
            return users;
        }
        
        public MembershipUserCollection GetUsersByEmail(string emailToMatch,int 
            pageIndex,int pageSize,out int totalRecords)
        {
            var users = new MembershipUserCollection();
            int total = 0;
            StartTimer();
            try
            {
                var where = (pageIndex == 0 || pageIndex == 1)
                                   ? string.Format("1 to {0}", pageSize)
                                   : string.Format("{0} to {1}", (pageSize * pageIndex) - pageSize + 1, pageIndex * pageSize);
                var qry = string.Format(MemberResource.QueryGetUsersByEmailPage, where);
                using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, MemberResource.xmlns);
                    qc.SetVariableValue("email", emailToMatch);

                    using (var results = mgr.Query(qry, qc, QueryOptions.None))
                    {
                        Trace("GetUsersByEmail returned {0} results.", results.GetSize());
                        if (results.GetSize() > 0)
                        {
                            do
                            {
                                users.Add(((FigaroMembershipUser)serializer.Deserialize(results.NextReader())).AsCurrentMembershipUser());
                            } while (results.HasNext());
                        }
                    }
                    using (var countResults = mgr.Query(MemberResource.QueryCountUsersByEmail,qc,QueryOptions.None))
                    {
                        total = (int)countResults.GetSize();
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
                StopTimer("GetUsersByEmail");
            }
            totalRecords = total;
            return users;
        }

        /// <summary>
        /// Retrieve each member entry's file name from the metadata. This will come in handy if we 
        /// change our file name naming convention to something other than the current 'user name 
        /// as file name' policy.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string GetFileNameFromUserName(string userName)
        {
            StartTimer();
            try
            {
                using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, MemberResource.xmlns);
                    qc.SetNamespace("db", MemberResource.FilenameMetaDataNamespace);
                    qc.SetVariableValue("user", userName);
                    var qry = string.Format(MemberResource.QueryGetFileName,
                                            MemberResource.PathUserName);
                    using (var results = mgr.Query(qry, qc, QueryOptions.None))
                    {
                        if (!results.IsNull() && results.GetSize() > 0)
                        {
                            Trace("GetfileNameFromUserName returns {0} results.", results.GetSize());
                            var ret = results.NextValue();
                            return ret.AsString;
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
                StopTimer("GetFileNameFromUserName");
            }
            return null;

        }
        
        /// <summary>
        /// Retrieve the membership user from the container with the specified <see cref="FigaroMembershipUser.ProviderUserKey"/>.
        /// </summary>
        /// <param name="key">The <see cref="FigaroMembershipUser.ProviderUserKey"/> to find a user with.</param>
        /// <returns>A <see cref="FigaroMembershipUser"/> if the member exists; otherwise, a null value is returned. </returns>
        public FigaroMembershipUser GetUserByObjectKey(object key)
        {
            StartTimer();
            try
            {
                using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, MemberResource.xmlns);
                    qc.SetVariableValue("key", KeyAsString(key));
                    var query = string.Format(MemberResource.QueryGet,MemberResource.VariableX + MemberResource.PathUserKey + " = " + MemberResource.VariableKey,MemberResource.VariableX);
                    Trace("GetUserByObjectKey query: {0}", query);
                    return GetUser(query, qc);
                }
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

        public bool UnlockUser(string userName)
        {
            StartTimer();
            try
            {
                var update = string.Format(MemberResource.QueryReplace, userName, MemberResource.PathIsLockedOut, "false");
                return UpdateBoolean(update);
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

        /// <summary>
        /// Returns the password of the membership user.
        /// </summary>
        /// <param name="userName">The user name to look up.</param>
        /// <returns>The password of the membership user.</returns>
        public string GetPassword(string userName)
        {
            StartTimer();
            try
            {
                using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, MemberResource.xmlns);
                    qc.SetVariableValue("user", userName);
                    var query = string.Format(MemberResource.QueryGet,
                                              string.Format("{0}{1} = {2}", MemberResource.VariableX, MemberResource.PathUserName,
                                                            MemberResource.VariableUser),
                                              string.Format("xs:string({0}{1}", MemberResource.VariableX, MemberResource.PathPassword));
                    return fromBase64(GetStringValue(query, qc));
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("QueryGetPassword");
            }
            return null;
        }

        private string GetStringValue(string query, QueryContext qc)
        {
            StartTimer();
            try
            {
                using (var results= mgr.Query(query,qc,QueryOptions.None))
                {
                    Trace("Query returned {0} results.", results.GetSize());
                    if (!results.HasNext()) return null;
                    var r = results.NextValue();
                    return r.IsNode ? r.NodeValue : r.AsString;
                }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e;
            }
            finally
            {
                StopTimer("Query");
            }
            return null;
        }

        public void UpdateUserOnline(string userName)
        {
            StartTimer();
            try
            {
                var update = string.Format(MemberResource.QueryReplace, userName, MemberResource.PathLastActivity,
                                              DateTime.Now.ToString("o"));
                Update(update);
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

        public string GetUserNameByEmail(string email)
        {
            StartTimer();
            try
            {
                using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, MemberResource.xmlns);
                    qc.SetVariableValue("y", email);
                    var query = string.Format(MemberResource.QueryGet,
                                              string.Format("{0} = {1}", MemberResource.PathEmail, MemberResource.VariableY),
                                              string.Format("xs:string({0})", MemberResource.PathUserName));
                    return GetStringValue(query, qc);
                }
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

        private static string fromBase64(string val)
        {
           return Encoding.Unicode.GetString(Convert.FromBase64String(val));
        }
        private static string toBase64(string val)
        {
            return Convert.ToBase64String(Encoding.Unicode.GetBytes(val));            
        }
        
        public bool SetNewPassword(string username, string newPassword)
        {
            StartTimer();
            try
            {
                var qry = string.Format(MemberResource.QueryReplace, username, MemberResource.PathPassword, toBase64(newPassword) );
                return UpdateBoolean(qry);
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

        public bool ValidateUser(string username, string password)
        {
            StartTimer();
            try
            {
                using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, MemberResource.xmlns);
                    qc.SetVariableValue("user", username);
                    qc.SetVariableValue("pwd", toBase64(password));
                    var where = string.Format("{0} and {1}", string.Format("$x{0} = $user", MemberResource.PathUserName), string.Format("$x{0} = $pwd", MemberResource.PathPassword));
                    var qry = string.Format(MemberResource.QueryGetUser, where);
                    Trace("ValidateUser query: {0}", qry);
                    using (var results = mgr.Query(qry,qc,QueryOptions.None))
                    {
                        if (!results.IsNull()) return results.GetSize() > 0;
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
                StopTimer("ValidateUser");
            }
            return false;
        }
        
        public string GetUserName(string userName)
        {
            StartTimer();
            try
            {
                using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty,MemberResource.xmlns);
                    qc.SetVariableValue("user", userName);
                    var qry = string.Format(MemberResource.QueryGet, 
                        string.Format("{0}{1} = {2}",MemberResource.VariableX,MemberResource.PathUserName,MemberResource.VariableUser),
                        string.Format("xs:string({0}{1})",MemberResource.VariableX, MemberResource.PathUserName));
                    Trace("user name query: " + qry);
                    return GetStringValue(qry, qc);
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
        
        /// <summary>
        /// Gets the password value from the membership container (not in Base 64 format).
        /// </summary>
        /// <param name="userName">The user to extract the password for.</param>
        /// <returns>Returns the string value of the password if the user and password exist; otherwise, null is returned.</returns>
        public string GetUserPassword(string userName)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                using (var qc = mgr.CreateQueryContext())
                {
                    qc.SetNamespace(string.Empty,MemberResource.xmlns);
                    qc.SetVariableValue("user", userName);
                    var qry = string.Format(MemberResource.QueryGet,
                                               string.Format("{0}{1} = {2}",MemberResource.VariableX,MemberResource.PathUserName,MemberResource.VariableUser),
                                               string.Format("xs:string({0}{1})",
                                               MemberResource.VariableX,MemberResource.PathPassword));
                    Trace("GetUserPassword query: " + qry);
                    return fromBase64(GetStringValue(qry, qc));
                }
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

        public bool ChangePasswordQuestionAndAnswer(string userName, string newPasswordQuestion, string newAnswer)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                var qry = string.Format(MemberResource.QueryReplace, userName, MemberResource.PasswordQuestion, newPasswordQuestion);
                if (!UpdateBoolean(qry)) return false;
                qry = string.Format(MemberResource.QueryReplace, userName, MemberResource.PasswordAnswer, newAnswer);
                if (!UpdateBoolean(qry)) return false;
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
                Trace("ChangePasswordQuestionAndAnswer completed in {0} seconds.", sw.Elapsed.TotalSeconds);
            }
            return false;
        }
        
        private bool UpdateBoolean(string query)
        {
            StartTimer();
            try
            {
                using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, MemberResource.xmlns);
                    using (var results = mgr.Query(query, qc, QueryOptions.None))
                    {
                        if (!results.IsNull() && results.GetSize() > 0)
                        {
                            Trace("Update Results: {0}", results.NextValue().AsString);
                        }
                        return true;
                    }
                }
           }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e; else throw;
            }
            finally
            {
                container.Sync();
                StopTimer("Update");
            }
        }

        private void Update(string query)
        {
            StartTimer();
            try
            {
               using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
               {
                   qc.SetNamespace(string.Empty, MemberResource.xmlns);
                   using (var results = mgr.Query(query, qc, QueryOptions.None))
                   {
                       if (!results.IsNull() && results.GetSize() > 0)
                       {
                           Trace("Update Results: {0}", results.NextValue().AsString);
                       }
                   }
               }
            }
            catch (Exception ex)
            {
                var e = ExceptionHandler.HandleException(ex, source);
                if (null != e) throw e; else throw;
            }
            finally
            {
                container.Sync();
                StopTimer("Update");
            }
        }

        protected static string KeyAsString(object key)
        {
            if (key is Guid)
                return ((Guid) key).ToString();
            if (key is string)
                return key as string;
            
            return Convert.ToString(key);
        }

        public MembershipCreateStatus CreateUser(FigaroMembershipUser user)
        {
            var status = MembershipCreateStatus.Success;
            StartTimer();
            try
            {
                user.Password = toBase64(user.Password);
                var ms = new MemoryStream();
                serializer.Serialize(ms, user);
                ms.Seek(0, SeekOrigin.Begin);
                var reader = XmlReader.Create(ms);
                var doc = mgr.CreateDocument(reader);
                doc.Name = user.UserName;
                container.PutDocument(doc,mgr.CreateUpdateContext(),PutDocumentOptions.None);
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
    
        public bool DeleteUser(string username, bool deleteRelatedData)
        {
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

        public bool ResetPassword(string userName, string newPassword)
        {
            StartTimer();
            try
            {
                var qry = string.Format(MemberResource.QueryReplace,MemberResource.PathUserName, MemberResource.PathPassword, toBase64(newPassword));
                Trace("ResetPassword query: " + qry);
                return UpdateBoolean(qry);
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

        public string GetPasswordAnswer(string userName)
        {
            StartTimer();
            try
            {
                using (var qc = mgr.CreateQueryContext(EvaluationType.Eager))
                {
                    qc.SetNamespace(string.Empty, MemberResource.xmlns);
                    qc.SetVariableValue("user", userName);
                    var qry = string.Format(MemberResource.QueryGet,
                                            MemberResource.VariableX + MemberResource.PathUserName + " = " + MemberResource.VariableUser,
                                            MemberResource.VariableX + MemberResource.PasswordAnswer);
                    Trace("GetPasswordAnswer query: " + qry);
                    return GetStringValue(qry, qc);
                }
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
    }
}
