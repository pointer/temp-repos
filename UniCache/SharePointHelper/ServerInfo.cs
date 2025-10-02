using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.Web;

namespace UGIS.de.OfficeComponents.UniCacheLib
{

    /// <summary>
    /// Class providing server-related data (data needed for file uploads and downloads)
    /// </summary>
    /// <seealso cref="UGIS.de.OfficeComponents.UniCacheLib.IServerInfo" />
    [ComVisible(true)]
    [GuidAttribute("1EDCB332-5F83-4ba8-9880-01DE9046E681")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("UniCacheLib.ServerInfo")]
    public class ServerInfo : IServerInfo
    {

        /// <summary>
        /// The server name used as key/id.
        /// </summary>
        private String m_Servername;
        /// <summary>
        /// Gets the server name (used as key/id)
        /// </summary>
        /// <value>
        /// The server name (used as key/id)
        /// </value>
        public String Servername
        {
            get { return m_Servername; }
        }

        /// <summary>
        /// The server's long name (for logging purposes)
        /// </summary>
        private String m_ServerLongname;
        /// <summary>
        /// Gets the server's long name (for logging purposes).
        /// </summary>
        /// <value>
        /// The server's long name (for logging purposes).
        /// </value>
        public String ServerLongname
        {
            get { return m_ServerLongname; }
        }

        /// <summary>
        /// The server's SerVo-site-root URL
        /// </summary>
        private String m_ServoRootUrl;
        /// <summary>
        /// Gets the server's SerVo-site-root URL
        /// </summary>
        /// <value>
        /// The server's SerVo-site-root URL
        /// </value>
        public String ServoRootUrl
        {
            get { return m_ServoRootUrl; }
        }

        /// <summary>
        /// The admin credentials (currently default credentials are used)
        /// </summary>
        private ICredentials m_AdminCredentials;
        /// <summary>
        /// Gets the admin credentials  (currently default credentials are used).
        /// </summary>
        /// <value>
        /// The admin credentials (currently default credentials are used).
        /// </value>
        internal ICredentials AdminCredentials
        {
            get { return m_AdminCredentials; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerInfo" /> class.
        /// </summary>
        /// <param name="servername">The server's name (used as id/key).</param>
        /// <param name="serverlongname">The server's long name.</param>
        /// <param name="servoRootUrl">The server's SerVo-site-root URL.</param>
        /// <param name="adminCredentials">The admin credentials (currently default credentials are used).</param>
        internal ServerInfo(String servername, String serverlongname, String servoRootUrl, ICredentials adminCredentials)
        {
            m_Servername = servername;
            m_ServerLongname = serverlongname;
            m_ServoRootUrl = servoRootUrl;
            m_AdminCredentials = adminCredentials;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return m_Servername;
        }

    }

}
