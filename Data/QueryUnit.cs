/*************************************************************************************************
* 
* THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
* KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
* PARTICULAR PURPOSE.
* 
*************************************************************************************************/

namespace Figaro.Web.ApplicationServices.Data
{
    /// <summary>
    /// This class is used to maintain everything needed to prepare and execute a query.
    /// </summary>
    public class QueryUnit
    {
        /// <summary>
        /// Gets or sets the <see cref="QueryContext"/> used for the query.
        /// </summary>
        public QueryContext Context { get; set; }
        /// <summary>
        /// Gets or sets the prepared <see cref="XQueryExpression"/>.
        /// </summary>
        public XQueryExpression Expression { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="Query"/>
        /// </summary>
        public string Query { get; set; }
    }
}
