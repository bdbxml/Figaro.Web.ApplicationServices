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
using System.Linq;
using System.Web.Configuration;
using System.Web.Security;
using Figaro.Web.ApplicationServices.Data;

namespace Figaro.Web.ApplicationServices.Security
{
    /// <summary>
    /// Manages storage of membership information for an ASP.NET application in a
    /// Figaro XML database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To use the <see cref="FigaroMembershipProvider"/>, you must specify a path and
    /// file name in the <c>container</c> setting of the provider configuration. 
    /// </para>
    /// The following Figaro-specific fields are described as:
    /// <list type="table">
    /// <item>
    ///     <term>container (required)</term>
    ///     <description>
    ///     The path and file name of the XML database to open. If no <c>manager</c> is specified, this should be either an absolute path or 
    ///     a web application path. In the case of the web application path, the library uses the <c>AppDomain.CurrentDomain.BaseDirectory</c> 
    ///     property to resolve the absolute path for the container. If a <c>manager</c> name is specified and is configured to use a configured 
    ///     <c>Environment</c> instance from configuration, no path is required for the <c>container</c> value.
    ///     </description>
    /// </item>
    /// <item>
    ///     <term>manager (optional)</term>    
    ///     <description>
    ///     The name of the configured <c>XmlManager</c> instance. 
    ///     </description>
    /// </item>
    /// </list>
    /// <para>
    /// If the specified container does not exist, it will automatically be created for
    /// you.
    /// </para>
    /// <note type="caution">
    /// This version of the Membership provider is built for Figaro DS and CDS. Transaction support is not enabled.
    /// </note>
    /// <para>
    /// Membership user passwords are stored as Base 64 encoded strings. MD5 encryption is used, but
    /// hashing is not supported in this version of the provider.
    /// </para>
    /// <para>
    ///     Members are stored as XML-serialized objects and indexed according to user
    ///     name. The schema definition for a member entry is shown below:
    /// </para>
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
    /// </list>
    /// </remarks>
    public sealed class FigaroMembershipProvider: MembershipProvider
    {
        /// <summary>
        /// Creates a new instance of the <see cref="FigaroMembershipProvider"/> object.
        /// </summary>
        /// <remarks>
        /// This constructor opens the <c>web.config</c> file at the website root and attempts to read the 
        /// <c>system.web/membership</c> section, and initialize the membership provider. If this section does not 
        /// exist, a <see cref="ProviderException"/> is thrown. 
        /// </remarks>
        public FigaroMembershipProvider()
        {
            System.Configuration.Configuration cfg;
            try
            {                
                cfg = WebConfigurationManager.OpenWebConfiguration("~/web.config");
            }
            catch(Exception ex)
            {
                ExceptionHandler.HandleException(ex, "FigaroMembershipProvider ctor");
                return;
            }

            var membership = cfg.GetSection("system.web/membership") as MembershipSection;// : 
                                         //WebConfigurationManager.GetSection("system.web/membership") as MembershipSection;

            if (membership == null) throw new ProviderException("No membership section found in configuration file.");
            var settings = membership.Providers["FigaroMembershipProvider"];
            if (settings == null) throw new ProviderException("No FigaroMembershipProvider configuration found in membership section.");
            Initialize(settings.Name,settings.Parameters);
            init = true;
        }

        private readonly bool init;
        private bool enablePasswordReset;
        private bool enablePasswordRetrieval;
        private bool requiresQuestionAndAnswer;
        private bool requiresUniqueEmail;
        private MembershipPasswordFormat passwordFormat;
        private int maxInvalidPasswordAttempts;
        private int minRequiredPasswordLength;
        private int minRequiredNonalphanumericCharacters;
        private int passwordAttemptWindow;
        private string passwordStrengthRegularExpression;
        private string mgrName;
        //private FigaroMemberQuery MemberQuery;
        private Stopwatch watch;

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.
        ///                 </param><param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.
        ///                 </param><exception cref="T:System.ArgumentNullException">The name of the provider is <see langword="null"/>.
        ///                 </exception><exception cref="T:System.ArgumentException">The name of the provider has a length of zero.
        ///                 </exception><exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.
        ///                 </exception>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            if (init) return;
            StartTimer();
            base.Initialize(name, config);
            if (null == config["container"])
                throw new ArgumentException("container is a required setting for FigaroMembershipProvider.");
            
            Container                               = config["container"];
            ApplicationName                         = config["applicationName"] ?? ProviderUtility.GetDefaultAppName();
            mgrName                                 = config["manager"] ?? string.Empty;
            passwordStrengthRegularExpression       = config["passwordStrengthRegularExpression"] ?? string.Empty;
            enablePasswordReset                     = ProviderUtility.GetBooleanValue(config, "enablePasswordReset", false);
            enablePasswordRetrieval                 = ProviderUtility.GetBooleanValue(config, "enablePasswordRetrieval", true);
            requiresQuestionAndAnswer               = ProviderUtility.GetBooleanValue(config, "requiresQuestionAndAnswer", false);
            requiresUniqueEmail                     = ProviderUtility.GetBooleanValue(config, "requiresUniqueEmail", false);
            passwordFormat                          = ProviderUtility.GetPasswordFormat(config, MembershipPasswordFormat.Clear);
            maxInvalidPasswordAttempts              = ProviderUtility.GetIntValue(config, "maxInvalidPasswordAttempts", 5, false,int.MaxValue);
            minRequiredPasswordLength               = ProviderUtility.GetIntValue(config, "minRequiredPasswordLength", 1, false,int.MaxValue);
            minRequiredNonalphanumericCharacters    = ProviderUtility.GetIntValue(config,"minRequiredNonalphanumericCharacters", 0,true, int.MaxValue);
            passwordAttemptWindow                   = ProviderUtility.GetIntValue(config, "passwordAttemptWindow", int.MaxValue, true, int.MaxValue);
            //MemberQuery = new FigaroMembershipData(Container);
            //data = new FigaroMemberQuery(Container,mgrName);
            MemberQuery = new FigaroMemberQuery(Container,mgrName);
            StopTimer("Initialize");
        }

        ///<summary>
        /// Gets the name of the file containing the member entry in the <see cref="FigaroMembershipProvider"/> container.
        ///</summary>
        ///<param name="userName">The user name to look up.</param>
        ///<returns>The membership user's file name.</returns>
        ///<exception cref="ProviderException">Thrown if an error occurs while accessing the container.</exception>
        public string GetFileName(string userName)
        {
            try
            {
                return MemberQuery.GetFileNameFromUserName(userName);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "get file name"), ex);
            }
        }

        internal FigaroMemberQuery MemberQuery { get; private set; }

        ///<summary>
        /// Generates a random password that is at least 10 characters long.
        ///</summary>
        ///<returns>A random password that is at least 10 characters long.</returns>
        public string GeneratePassword()
        {
            return Membership.GeneratePassword(MinRequiredPasswordLength < 10 ? 10 : MinRequiredPasswordLength,
                                               MinRequiredNonAlphanumericCharacters);
            
        }

        /// <summary>
        /// Processes a request to update the password for a membership user.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the password was updated successfully; otherwise, <see langword="false"/>.
        /// </returns>
        /// <param name="username">The user to update the password for. 
        ///                 </param><param name="oldPassword">The current password for the specified user. 
        ///                 </param><param name="newPassword">The new password for the specified user. 
        ///                 </param>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
                throw new NullReferenceException("Cannot pass null values to ChangePassword.");
            if (oldPassword.Equals(newPassword))
                throw new ArgumentException("oldPassword and newPassword cannot be equal.");
            if (newPassword.Length < minRequiredPasswordLength)
                throw new ArgumentException("Password does not meet minimum length requirements.");

            var num = newPassword.Count(s => !char.IsLetterOrDigit(s));
            if (num < MinRequiredNonAlphanumericCharacters)
                throw new ArgumentException("Password does not meet minimum non-alphanumeric character requirements.");

            if (!MemberQuery.GetUserPassword(username).Equals(oldPassword))
                throw new ArgumentException("Old Password does not match stored password.");

            try
            {

                return MemberQuery.ResetPassword(username, newPassword);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "change password"), ex);
            }

        }

        /// <summary>
        /// Processes a request to update the password question and answer for a membership user.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the password question and answer are updated successfully; otherwise, <see langword="false"/>.
        /// </returns>
        /// <param name="username">The user to change the password question and answer for. 
        ///                 </param><param name="password">The password for the specified user. 
        ///                 </param><param name="newPasswordQuestion">The new password question for the specified user. 
        ///                 </param><param name="newPasswordAnswer">The new password answer for the specified user. 
        ///                 </param>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            if (string.IsNullOrEmpty(username))
                throw new NullReferenceException("username cannot be null.");
            if (string.IsNullOrEmpty(password))
                throw new NullReferenceException("password cannot be null.");

            if (username.Length > 256)
                throw new ArgumentException(MemberResource.FigaroMembershipProvider_InvalidUsername + username + @"'", "username");
            if ((RequiresQuestionAndAnswer) && string.IsNullOrEmpty(newPasswordQuestion) || string.IsNullOrEmpty(newPasswordAnswer))
                throw new ArgumentException("Password question and answer are required fields.");
            Trace("retrieved user name: " + MemberQuery.GetUserName(username));
            if (string.IsNullOrEmpty(MemberQuery.GetUserName(username)))
                throw new ProviderException(string.Format("User name '{0}' does not exist in Figaro container.",username));

            Trace("old password: {0}. new password: {1}", password, MemberQuery.GetUserPassword(username));

            if (!password.Equals(MemberQuery.GetUserPassword(username)))
                throw new MembershipPasswordException("member's password does not match.");
            try
            {
                return MemberQuery.ChangePasswordQuestionAndAnswer(username, newPasswordQuestion, newPasswordAnswer);
            }
            catch(Exception ex)
            {
                throw new ProviderException(
                    "An exception occurred changing password question and answer. See inner exception for details.", ex);
            }
        }


        /// <summary>
        /// Adds a new membership user to the data source.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the information for the newly created user.
        /// </returns>
        /// <param name="username">The user name for the new user. 
        ///                 </param><param name="password">The password for the new user. 
        ///                 </param><param name="email">The e-mail address for the new user.
        ///                 </param><param name="passwordQuestion">The password question for the new user.
        ///                 </param><param name="passwordAnswer">The password answer for the new user
        ///                 </param><param name="isApproved">Whether or not the new user is approved to be validated.
        ///                 </param><param name="providerUserKey">The unique identifier from the membership data source for the user.
        ///                 </param><param name="status">A <see cref="T:System.Web.Security.MembershipCreateStatus"/> enumeration value indicating whether the user was created successfully.
        ///                 </param>
        public override MembershipUser CreateUser(string username, string password, string email, 
            string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, 
            out MembershipCreateStatus status)
        {
            try
            {
                var fu = new FigaroMembershipUser
                {
                    UserName = username,
                    Password = password,
                    Email = email,
                    PasswordQuestion = passwordQuestion,
                    PasswordAnswer = passwordAnswer,
                    IsApproved = isApproved,
                    ProviderUserKey = providerUserKey,
                    CreationDate = DateTime.Now,
                    IsLockedOut = false,
                    LastActivityDate = DateTime.Now,
                    LastLockoutDate = DateTime.Now,
                    LastLoginDate = DateTime.Now,
                    LastPasswordChangedDate = DateTime.Now,
                };
                status = MemberQuery.CreateUser(fu);
                return status.Equals(MembershipCreateStatus.Success) ? fu.AsCurrentMembershipUser() : null;
            }
            catch (Exception ex)
            {
                throw new ProviderException(
                    string.Format("Failed to {0}. See inner exception for details.", "create user"), ex);
            }
        }

        /// <summary>
        /// Deletes the user from the container.
        /// </summary>
        /// <param name="username">The user name to remove.</param>
        /// <param name="deleteAllRelatedData"></param>
        /// <returns><see langword="true"/> if the operation is successful; if an exception occurs, <see langword="false"/> is returned.</returns>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            try
            {
                return MemberQuery.DeleteUser(username, deleteAllRelatedData);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "delete user"), ex);
            }
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// <returns>
        /// The new password for the specified user.
        /// </returns>
        /// <param name="username">The user to reset the password for. 
        ///                 </param><param name="answer">The password answer for the specified user. 
        ///                 </param>
        public override string ResetPassword(string username, string answer)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(answer))
                throw new NullReferenceException("User name or password cannot be null. ResetPassword failed.");
            try
            {
                if (string.IsNullOrEmpty(username))
                    throw new ArgumentException(MemberResource.FigaroMembership_UserName_NullOrEmpty, username);

                if (!validPasswordAnswer(username, answer)) return null;

                var rsg = new RandomStringGenerator(69);
                var pwd = rsg.NextString(minRequiredPasswordLength);
                return MemberQuery.ResetPassword(username, pwd) ? pwd : null;
            }
            catch(Exception ex)
            {
                throw new ProviderException("Failed to reset password. See inner exception for details.", ex);
            }

        }
        
        private bool validPasswordAnswer(string userName, string submittedAnswer)
        {
            var storedAnswer = MemberQuery.GetPasswordAnswer(userName);

            //if they're both null/empty then it's good - otherwise it's not
            if (string.IsNullOrEmpty(storedAnswer) && string.IsNullOrEmpty(submittedAnswer)) return true;
            if (string.IsNullOrEmpty(storedAnswer) && !string.IsNullOrEmpty(submittedAnswer)) return false;
            if (!string.IsNullOrEmpty(storedAnswer) && string.IsNullOrEmpty(submittedAnswer)) return false;

            var lowerS = string.IsNullOrEmpty(submittedAnswer) ? string.Empty : submittedAnswer.ToLower().Trim();
            var lowerT = string.IsNullOrEmpty(storedAnswer) ? string.Empty : storedAnswer.ToLower().Trim();

            return lowerS.Equals(lowerT);
        }

        /// <summary>
        /// Clears a lock so that the membership user can be validated.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the membership user was successfully unlocked; otherwise, <see langword="false"/>.
        /// </returns>
        /// <param name="userName">The membership user whose lock status you want to clear.
        ///                 </param>
        public override bool UnlockUser(string userName)
        {
            try
            {
                return MemberQuery.UnlockUser(userName);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "unlock user"), ex);
            }
        }

        /// <summary>
        /// Updates information about a user in the MemberQuery source.
        /// </summary>
        /// <param name="user">A <see cref="T:System.Web.Security.MembershipUser"/> object that represents the user to update and the updated information for the user. 
        ///                 </param>
        public override void UpdateUser(MembershipUser user)
        {
            try
            {
                MemberQuery.UpdateUser(user);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "update user"), ex);
            }
        }

        /// <summary>
        /// Verifies that the specified user name and password exist in the MemberQuery source.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the specified username and password are valid; otherwise, <see langword="false"/>.
        /// </returns>
        /// <param name="username">The name of the user to validate. 
        ///                 </param><param name="password">The password for the specified user. 
        ///                 </param>
        public override bool ValidateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                throw new NullReferenceException("User name and/or password cannot be empty.");
            try
            {
                return MemberQuery.ValidateUser(username,  password);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "validate user"), ex);
            }
        }

        /// <summary>
        /// Gets a collection of membership users where the e-mail address contains the specified e-mail address to match.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        /// <param name="emailToMatch">The e-mail address to search for.
        ///                 </param><param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.
        ///                 </param><param name="pageSize">The size of the page of results to return.
        ///                 </param><param name="totalRecords">The total number of matched users.
        ///                 </param>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            try
            {
                return MemberQuery.GetUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "find users by email"), ex);
            }

        }

        /// <summary>
        /// Gets a collection of membership users where the user name contains the specified user name to match.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        /// <param name="usernameToMatch">The user name to search for.
        ///                 </param><param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.
        ///                 </param><param name="pageSize">The size of the page of results to return.
        ///                 </param><param name="totalRecords">The total number of matched users.
        ///                 </param>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            try
            {
                return MemberQuery.GetUsersByName(usernameToMatch, pageIndex, pageSize, out totalRecords);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "find users by name"), ex);
            }
        }

        /// <summary>
        /// Gets a collection of all the users in the MemberQuery source in pages of MemberQuery.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        /// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.
        ///                 </param><param name="pageSize">The size of the page of results to return.
        ///                 </param><param name="totalRecords">The total number of matched users.
        ///                 </param>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            try
            {
                return MemberQuery.GetAllUsers(pageIndex, pageSize, out totalRecords);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "get all users"), ex);
            }
        }

        ///<summary>
        /// Get the number of users stored in the <see cref="FigaroMembershipProvider"/>'s container.
        ///</summary>
        ///<returns></returns>
        ///<exception cref="ProviderException"></exception>
        public ulong GetNumberOfUsers()
        {
            try
            {
                return MemberQuery.GetNumberOfUsers();
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "get number of users"), ex);
            }
        }

        /// <summary>
        /// Gets the number of users currently accessing the application.
        /// </summary>
        /// <returns>
        /// The number of users currently accessing the application.
        /// </returns>
        public override int GetNumberOfUsersOnline()
        {
            try
            {
                var span = new TimeSpan(0, Membership.UserIsOnlineTimeWindow, 0);
                var time = DateTime.UtcNow.Subtract(span);
                return MemberQuery.GetNumberOfOnlineUsers(time);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "get number of users online"), ex);
            }
        }

        /// <summary>
        /// Gets the password for the specified user name from the data source.
        /// </summary>
        /// <returns>
        /// The password for the specified user name.
        /// </returns>
        /// <param name="username">The user to retrieve the password for. 
        ///                 </param><param name="answer">The password answer for the user. 
        ///                 </param>
        public override string GetPassword(string username, string answer)
        {
            if (string.IsNullOrEmpty(username))
                throw new NullReferenceException("Requires a user name.");
            if (!string.IsNullOrEmpty(answer))
                if (!validPasswordAnswer(username,answer)) throw new ArgumentException("Invalid password answer.");
            try
            {
                return MemberQuery.GetPassword(username);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "get password"), ex);
            }
        }

        /// <summary>
        /// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the specified user's information from the data source.
        /// </returns>
        /// <param name="username">The name of the user to get information for. 
        ///                 </param><param name="userIsOnline"><see langword="true"/> to update the last-activity date/time stamp for the user; <see langword="false"/> to return user information without updating the last-activity date/time stamp for the user. 
        ///                 </param>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            try
            {
                var user = MemberQuery.GetUserByUserName(username);
                if (null != user && userIsOnline)
                {
                    MemberQuery.UpdateUserOnline(username);
                }
                return null != user ? user.AsCurrentMembershipUser() : null;
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "get user"), ex);
            }
        }

        /// <summary>
        /// Gets user information from the data source based on the unique identifier for the membership user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the specified user's information from the data source.
        /// </returns>
        /// <param name="providerUserKey">The unique identifier for the membership user to get information for.
        ///                 </param><param name="userIsOnline"><see langword="true"/> to update the last-activity date/time stamp for the user; <see langword="false"/> to return user information without updating the last-activity date/time stamp for the user.
        ///                 </param>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            if (providerUserKey == null) throw new ArgumentNullException("providerUserKey");
            try
            {
                var user = MemberQuery.GetUserByObjectKey(providerUserKey);
                if (user != null && userIsOnline) MemberQuery.UpdateUserOnline(user.UserName);

                return null != user ? user.AsCurrentMembershipUser() : null;
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "get user"), ex);
            }
        }

        /// <summary>
        /// Gets the user name associated with the specified e-mail address.
        /// </summary>
        /// <returns>
        /// The user name associated with the specified e-mail address. If no match is found, return <see langword="null"/>.
        /// </returns>
        /// <param name="email">The e-mail address to search for. 
        ///                 </param>
        public override string GetUserNameByEmail(string email)
        {
            try
            {
                return MemberQuery.GetUserNameByEmail(email);
            }
            catch (Exception ex)
            {
                throw new ProviderException(string.Format("Failed to {0}. See inner exception for details.", "get user name by email"), ex);
            }

        }

        #region properties

        /// <summary>
        /// The path and file name of the container holding the Membership data.
        /// </summary>
        /// <remarks>
        /// If the container does not exist, a node-storage container will be created for you.
        /// </remarks>
        public string Container { get; private set; }

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
                return "Membership provider implementation using the Figaro .NET XML Database";
            }
        }

        /// <summary>
        /// The name of the application using the custom membership provider.
        /// </summary>
        /// <returns>
        /// The name of the application using the custom membership provider.
        /// </returns>
        public override string ApplicationName { get; set; }

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
                return "FigaroMembershipProvider";
            }
        }
        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to reset their passwords.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the membership provider supports password reset; otherwise, <see langword="false"/>. <para>The default is <see langword="true"/>.</para>
        /// </returns>
        public override bool EnablePasswordReset
        {
            get { return enablePasswordReset; }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to retrieve their passwords.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the membership provider is configured to support password retrieval; otherwise, <see langword="false"/>. <para>The default is <see langword="false"/>.</para>
        /// </returns>
        public override bool EnablePasswordRetrieval
        {
            get { return enablePasswordRetrieval; }
        }

        /// <summary>
        /// Gets the number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </summary>
        /// <returns>
        /// The number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </returns>
        public override int MaxInvalidPasswordAttempts
        {
            get { return maxInvalidPasswordAttempts; }
        }

        /// <summary>
        /// Gets the minimum number of special characters that must be present in a valid password.
        /// </summary>
        /// <returns>
        /// The minimum number of special characters that must be present in a valid password.
        /// </returns>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return minRequiredNonalphanumericCharacters; }
        }

        /// <summary>
        /// Gets the minimum length required for a password.
        /// </summary>
        /// <returns>
        /// The minimum length required for a password. 
        /// </returns>
        public override int MinRequiredPasswordLength
        {
            get { return minRequiredPasswordLength; }
        }

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        /// <returns>
        /// The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </returns>
        public override int PasswordAttemptWindow
        {
            get { return passwordAttemptWindow; }
        }

        /// <summary>
        /// Gets a value indicating the format for storing passwords in the membership data store.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Web.Security.MembershipPasswordFormat"/> values indicating the format for storing passwords in the data store.
        /// </returns>
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return passwordFormat; }
        }

        /// <summary>
        /// Gets the regular expression used to evaluate a password.
        /// </summary>
        /// <returns>
        /// A regular expression used to evaluate a password.
        /// </returns>
        public override string PasswordStrengthRegularExpression
        {
            get { return passwordStrengthRegularExpression; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if a password answer is required for password reset and retrieval; otherwise, <see langword="false"/>. <para>The default is <see langword="true"/>.</para>
        /// </returns>
        public override bool RequiresQuestionAndAnswer
        {
            get { return requiresQuestionAndAnswer; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require a unique e-mail address for each user name.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the membership provider requires a unique e-mail address; otherwise, <see langword="false"/>. <para>The default is <see langword="true"/>.</para>
        /// </returns>
        public override bool RequiresUniqueEmail
        {
            get { return requiresUniqueEmail; }
        }

        #endregion
        /// <summary>
        /// Start a timer and measure performance.
        /// </summary>
        private void StartTimer()
        {
            if (null == watch) watch = new Stopwatch();
            watch.Start();
        }
        /// <summary>
        /// Stops the timer and displays the results.
        /// </summary>
        /// <param name="timedAction">The activity that was timed.</param>
        private void StopTimer(string timedAction)
        {
            watch.Stop();
            Trace("{0} completed in {1} seconds ({2} ms).", timedAction, watch.Elapsed.TotalSeconds, watch.Elapsed.TotalMilliseconds);
            watch.Reset();
        }
        /// <summary>
        /// Trace diagnostic and performance output.
        /// </summary>
        /// <param name="message">The format string output.</param>
        /// <param name="args">Arguments for the format string.</param>
        private static void Trace(string message, params object[] args)
        {
            TraceHelper.Write(MemberResource.Source, "[FigaroMembershipProvider]\t" + message, args);
        }

        /// <summary>
        /// Allows <see cref="FigaroMembershipProvider"/> to attempt to free
        /// resources and perform other cleanup operations before the 
        /// <see cref="FigaroMembershipProvider"/> is reclaimed by garbage
        /// collection.
        /// </summary>
        ~FigaroMembershipProvider()
        {
            MemberQuery = null;
        }
    }
}
