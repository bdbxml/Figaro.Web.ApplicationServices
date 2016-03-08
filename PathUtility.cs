/*************************************************************************************************
* 
* THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
* KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
* PARTICULAR PURPOSE.
* 
*************************************************************************************************/
#if DEBUG
using System.Diagnostics;
#endif
using System.Web;
namespace Figaro.Web
{
    /// <summary>
    /// Used to resolve the file paths containing server paths.
    /// </summary>
    internal class PathUtility
    {

        /// <summary>
        /// Resolve paths possibly containing environment variables.
        /// </summary>
        /// <param name="path">The directory path to resolve.</param>
        /// <returns>A resolved path.</returns>
        public static string ResolvePath(string path)
        {
#if DEBUG
            Debug.WriteLine(string.Format("resolving '{0}' to '{1}'... ",path,VirtualPathUtility.GetDirectory(path)),"Configuration");
#endif
            return VirtualPathUtility.GetDirectory(path);
        }
    }
}
