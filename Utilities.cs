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

namespace Figaro.Web.ApplicationServices
{
    /// <summary>
    /// Global exception handling policy.
    /// </summary>
    internal static class ExceptionHandler
    {
        public static Exception HandleException(Exception ex, string source)
        {

            //for now, our policy is to simply trace the exception and return null
            //anything further requirement can be handled at local level
            TraceHelper.Write(source,"[{0}] {1}: {2}\r\nStack Trace:\r\n{3}", source, ex.GetType(), ex.Message,
                                        ex.StackTrace);
            return ex;
        }
    }

    internal static class TraceHelper
    {
        public static void Write(string source, string message, params object[] args)
        {
            Trace.WriteLine(args == null ? message : string.Format(message, args), source);
        }
    }
}
