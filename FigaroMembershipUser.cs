/*************************************************************************************************
* 
* THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
* KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
* PARTICULAR PURPOSE.
* 
*************************************************************************************************/
using System;
using System.Web.Security;
//using Figaro.Web.Data;

namespace Figaro.Web.ApplicationServices.Security
{
    ///<summary>
    /// The membership user object that <see cref="FigaroMembershipProvider"/> serializes and deserializes in the container.
    ///</summary>
    /// <remarks>
    /// The <see cref="FigaroMembershipProvider"/> uses object serialization to store member entries in the Figaro XML database 
    /// container. This membership user, which inherits from <see cref="MembershipUser"/>, provides the compatibility required for 
    /// integration with the Membership API while giving Figaro an interchangeable, serializable membership user object.
    /// </remarks>
    [Serializable]
    public sealed class FigaroMembershipUser: MembershipUser
    {
        //private FigaroMembershipData data;
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>
        /// This constructor is required for serialization purposes and performs no functionality.
        /// </remarks>
        public FigaroMembershipUser(){}

        /// <summary>
        /// Instantiates a new <see cref="FigaroMembershipUser"/> using an existing <see cref="MembershipUser"/> object.
        /// </summary>
        /// <param name="user">The <see cref="MembershipUser"/> to copy from.</param>
        public FigaroMembershipUser(MembershipUser user)
        {
            
            Comment                     = user.Comment;
            CreationDate                = user.CreationDate;
            Email                       = user.Email;
            IsApproved                  = user.IsApproved;
            IsLockedOut                 = user.IsLockedOut;
            LastActivityDate            = user.LastActivityDate;
            LastLockoutDate             = user.LastLockoutDate;
            LastLoginDate               = user.LastLoginDate;
            LastPasswordChangedDate     = user.LastPasswordChangedDate;
            PasswordQuestion            = user.PasswordQuestion;
            ProviderName                = user.ProviderName;
            ProviderUserKey             = user.ProviderUserKey;
            UserName                    = user.UserName;
            
        }

        /// <summary>
        /// Creates a new membership user object with the specified property values.
        /// </summary>
        /// <param name="providerName">The <see cref="ProviderName"/> string for the membership user.</param>
        /// <param name="name">The <see cref="UserName"/> string for the membership user.</param>
        /// <param name="providerUserKey">The <see cref="ProviderUserKey"/> object for the membership user.</param>
        /// <param name="email">The <see cref="Email"/> string for the membership user.</param>
        /// <param name="passwordQuestion">The <see cref="PasswordQuestion"/>  string for the membership user.</param>
        /// <param name="comment">The <see cref="Comment"/>  string for the membership user.</param>
        /// <param name="isApproved">The <see cref="IsApproved"/>  boolean for the membership user.</param>
        /// <param name="isLockedOut">The <see cref="IsLockedOut"/> boolean for the membership user.</param>
        /// <param name="creationDate">The <see cref="CreationDate"/> for the membership user.</param>
        /// <param name="lastLoginDate">The <see cref="LastLoginDate"/> for the membership user.</param>
        /// <param name="lastActivityDate">The <see cref="LastActivityDate"/> for the membership user.</param>
        /// <param name="lastPasswordChangedDate">The <see cref="LastPasswordChangedDate"/> for the membership user.</param>
        /// <param name="lastLockoutDate">The <see cref="LastLockoutDate"/> for the membership user.</param>
        public FigaroMembershipUser(string providerName, string name, object providerUserKey, string email, string passwordQuestion, 
            string comment, bool isApproved, bool isLockedOut, DateTime creationDate, DateTime lastLoginDate, DateTime lastActivityDate, 
            DateTime lastPasswordChangedDate, DateTime lastLockoutDate)
        {
            ProviderName = providerName;
            UserName = name;
            ProviderUserKey = providerUserKey;
            Email = email;
            PasswordQuestion = passwordQuestion;
            Comment = comment;
            IsApproved = isApproved;
            IsLockedOut = isLockedOut;
            CreationDate = creationDate;
            LastLoginDate = lastLoginDate;
            LastActivityDate = lastActivityDate;
            LastPasswordChangedDate = lastPasswordChangedDate;
            LastLockoutDate = lastLockoutDate;
        }

        ///<summary>
        /// Converts a <see cref="FigaroMembershipUser"/> to a <see cref="MembershipUser"/> object.
        ///</summary>
        ///<returns></returns>
        public MembershipUser AsCurrentMembershipUser()
        {
            return new MembershipUser(ProviderUtility.GetProviderName("FigaroMembershipProvider"),
                UserName,ProviderUserKey,Email,PasswordQuestion,Comment,IsApproved,IsLockedOut,
                CreationDate,LastLoginDate,LastActivityDate,LastPasswordChangedDate,LastLockoutDate);
        }

        #region properties

        /// <summary>
        /// Gets or sets application-specific information for the membership user.
        /// </summary>
        /// <returns>
        /// Application-specific information for the membership user.
        /// </returns>
        public override string Comment { get; set; }

        /// <summary>
        /// Gets or sets the creation date for the membership user.
        /// </summary>
        /// <returns>
        /// The creation date for the membership user.
        /// </returns>
        public new DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the e-mail address for the membership user.
        /// </summary>
        /// <returns>
        /// The e-mail address for the membership user.
        /// </returns>
        public override string Email { get; set; }

        /// <summary>
        /// Gets or sets whether the membership user can be authenticated.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the user can be authenticated; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool IsApproved { get; set; }

        /// <summary>
        /// Gets or sets whether the membership user is locked out of their account.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the user is locked out; otherwise, <see langword="false"/>.
        /// </returns>
        public new bool IsLockedOut { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the membership user was last authenticated or accessed the application.
        /// </summary>
        /// <returns>
        /// The date and time when the membership user was last authenticated or accessed the application.
        /// </returns>
        public override DateTime LastActivityDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time the membership user was last locked out of their account.
        /// </summary>
        /// <returns>
        /// The membership user's last lockout date and time.
        /// </returns>
        public new DateTime LastLockoutDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user was last authenticated.
        /// </summary>
        /// <returns>
        /// The date and time when the user was last authenticated.
        /// </returns>
        public override DateTime LastLoginDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time the membership user's password was last changed.
        /// </summary>
        /// <returns>
        /// The membership user's last password change date and time.
        /// </returns>
        public new DateTime LastPasswordChangedDate { get; set; }

        /// <summary>
        /// Gets or sets the membership user's password.
        /// </summary>
        /// <returns>
        /// The membership user's password.
        /// </returns>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the membership user's password answer.
        /// </summary>
        /// <returns>
        /// The membership user's password answer.
        /// </returns>
        public string PasswordAnswer { get; set; }

        /// <summary>
        /// Gets or sets the membership user's password question.
        /// </summary>
        /// <returns>
        /// The membership user's password question.
        /// </returns>
        public new string PasswordQuestion { get; set; }

        /// <summary>
        /// Gets or sets the membership user's provider name.
        /// </summary>
        /// <returns>
        /// The membership user's provider name.
        /// </returns>
        public new string ProviderName { get; set; }

        /// <summary>
        /// Gets or sets the membership user's provider key.
        /// </summary>
        /// <returns>
        ///The membership user's provider key.
        /// </returns>
        public new object ProviderUserKey { get; set; }

        /// <summary>
        /// Gets or sets the membership user's user name.
        /// </summary>
        /// <returns>
        /// The membership user's user name.
        /// </returns>
        public new string UserName { get; set; }

        #endregion

    }
}
