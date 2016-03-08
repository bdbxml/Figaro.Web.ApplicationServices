/*************************************************************************************************
* 
* THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
* KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
* PARTICULAR PURPOSE.
* 
*************************************************************************************************/
using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Web.Hosting;
using System.Web.Security;

namespace Figaro.Web
{
  internal sealed class ProviderUtility
  {
      /// <summary>
      /// Look for the provider in the list of registered providers. If the provider isn't there, return the first entry in the list.
      /// </summary>
      /// <param name="providerName">The provider to look for.</param>
      /// <returns>Returns <paramref name="providerName"/> if in the <see cref="Membership.Providers"/> list; otherwise, the first one is returned.</returns>
      public static string GetProviderName(string providerName)
      {
          var first = String.Empty;
          var i = 0;
          foreach (var provider in Membership.Providers)
          {
              var name = ((ProviderBase) provider).Name;
              if (i == 0)
                  first = name;
              if (name.Equals(providerName)) return providerName;
              i++;
          }
          return first;
      }

    /// <summary>
    /// QueryGet a default application name
    /// </summary>
    /// <returns></returns>
    public  static string GetDefaultAppName()
    {
      string defPath;
      try
      {
        string vPath = HostingEnvironment.ApplicationVirtualPath;
        if (string.IsNullOrEmpty(vPath))
        {
            var mod = Process.GetCurrentProcess().MainModule;
           vPath = mod.ModuleName;
            if (!string.IsNullOrEmpty(vPath))
            {
                var num1 = vPath.IndexOf('.');
                if (num1 > -1)
                {
                    vPath = vPath.Remove(num1);
                }
            }
        }
        if (string.IsNullOrEmpty(vPath))
        {
          return "/";
        }
        defPath = vPath;
      }
      catch
      {
        defPath = "/";
      }
      return defPath;
    }

    /// <summary>
    /// return a value from a collection of a default value if not in that collection
    /// </summary>
    /// <param name="config">the collection</param>
    /// <param name="valueName">the value to look up</param>
    /// <param name="defaultValue">the default value</param>
    /// <returns>a value from the collection or the default value</returns>
    internal static bool GetBooleanValue(NameValueCollection config, string valueName, bool defaultValue)
    {
      bool result;
      string valueToParse = config[valueName];
      if (valueToParse == null)
      {
        return defaultValue;
      }
      if (bool.TryParse(valueToParse, out result))
      {
        return result;
      }
      throw new Exception("Value must be boolean");
    }

    /// <summary>
    /// return a value from a collection of a default value if not in that collection
    /// </summary>
    /// <param name="config">a collection</param>
    /// <param name="valueName">the name of the value in the collection</param>
    /// <param name="defaultValue">a default value</param>
    /// <param name="zeroAllowed">is zero allowed</param>
    /// <param name="maxValueAllowed">what is the largest number that will be accepted</param>
    /// <returns>a value</returns>
    internal static int GetIntValue(NameValueCollection config, string valueName, int defaultValue, bool zeroAllowed, int maxValueAllowed)
    {
      int result;
      string valueToParse = config[valueName];
      if (valueToParse == null)
      {
        return defaultValue;
      }
      if (!int.TryParse(valueToParse, out result))
      {
        if (zeroAllowed)
        {
          throw new Exception("Value must be non negative integer");
        }
        throw new Exception("Value must be positive integer");
      }
      if (zeroAllowed && (result < 0))
      {
        throw new Exception("Value must be non negative integer");
      }
      if (!zeroAllowed && (result <= 0))
      {
        throw new Exception("Value must be positive integer");
      }
      if ((maxValueAllowed > 0) && (result > maxValueAllowed))
      {
        throw new Exception("Value too big");
      }
      return result;
    }

      public static MembershipPasswordFormat GetPasswordFormat(NameValueCollection config, MembershipPasswordFormat defaultValue)
      {
          if (null == config["passwordFormat"]) return defaultValue;

          var fmt = config["passwordFormat"];

          //check to see if selected value is in list, otherwise return default
          if (fmt.ToLower().Equals(MembershipPasswordFormat.Clear.ToString().ToLower()) ||
              fmt.ToLower().Equals(MembershipPasswordFormat.Encrypted.ToString().ToLower()) ||
              fmt.ToLower().Equals(MembershipPasswordFormat.Hashed.ToString().ToLower()))
          {
              return (MembershipPasswordFormat)Enum.Parse(typeof(MembershipPasswordFormat), fmt);
          }

          Trace.WriteLine("unexpected MembershipPasswordFormat: " + fmt, "FigaroMembershipProvider");
          return defaultValue;
      }
  }
}
