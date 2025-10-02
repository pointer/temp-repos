using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace UGIS.de.OfficeComponents.UniCacheLib
{

    /// <summary>
    ///  <see cref="IUniCacheTestData" /> provides <see cref="UniCache"/> configuration data for test purposes
    /// </summary>
    /// <remarks>
    /// Avoids to have public <see cref="UniCache" />-properties which should be internal
    /// </remarks>
    public interface IUniCacheTestData
    {


        /// <summary>
        /// Gets the applications (offline- or online-) content-xml fullname on the client.
        /// </summary>
        /// <value>
        /// the applications (offline- or online-) content-xml fullname on the client.
        /// </value>
        String ApplicationsCachedContentXmlName { get; }


        /// <summary>
        /// Gets a value indicating whether this instance is updated today.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is updated today; otherwise, <c>false</c>.
        /// </value>
        Boolean IsUpdatedToday { get; }

        /// <summary>
        /// Returns string list of web exceptions since last query of property WebExceptions
        /// </summary>
        String WebExceptions { get; }


    }

}
