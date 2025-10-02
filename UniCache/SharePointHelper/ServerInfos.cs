using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.Web;
using Microsoft.Win32;

namespace UGIS.de.OfficeComponents.UniCacheLib
{

    /// <summary>
    /// Provides <see cref="ServerInfo" />-instances for the SharePoint servers in production, test and dev environment
    /// </summary>
    /// <seealso cref="UGIS.de.OfficeComponents.UniCacheLib.IServerInfos" />
    [ComVisible(true)]
    [GuidAttribute("1C993DA0-5F71-42a5-BAE8-7B5EBB7516E7")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("UniCacheLib.ServerInfos")]
    public class ServerInfos : IServerInfos
    {

        /// <summary>
        /// Key/Identifier of the SharePoint server in production environment
        /// </summary>
        [Obsolete("ServerMOSS is deprecated, use ServerPROD instead.")]
        public const String ServerMOSS = "MOSS";

        /// <summary>
        /// Key/Identifier of the SharePoint server in production environment
        /// </summary>
        public const String ServerPROD = "PROD";

        /// <summary>
        /// Key/Identifier of the SharePoint server in test environment
        /// </summary>
        [Obsolete("ServerMOSSQC is deprecated, use ServerQA instead.")]
        public const String ServerMOSSQC = "QC";

        /// <summary>
        /// Key/Identifier of the SharePoint server in test environment
        /// </summary>
        public const String ServerQA = "QA";

        /// <summary>
        /// Key/Identifier of the SharePoint server in dev environment
        /// </summary>
        public const String ServerDEV = "DEV";

        /// <summary>
        /// The long name of the SharePoint server in production environment (for logging purposes)
        /// </summary>
        private const String LongnamePROD = "SP2013";


        /// <summary>
        /// The long name of the SharePoint server in test environment (for logging purposes)
        /// </summary>
        private const String LongnameQA = "SP2013QA";

        /// <summary>
        /// The long name of the SharePoint server in dev environment (for logging purposes)
        /// </summary>
        private const String LongnameDEV = "SP2013DEV";

        /// <summary>
        /// Key/Identifier of the the default SharePoint server which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment
        /// </summary>
        /// <value>
        /// Key/Identifier of the the default SharePoint server which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment
        /// </value>
        [Obsolete("ServerMOSSDefault is deprecated, use ServerDefault instead.")]
        public static String ServerMOSSDefault
        {
            get
            {
                return ServerDefault;
            }
        }

        /// <summary>
        /// Key/Identifier of the the default SharePoint server which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment
        /// </summary>
        /// <value>
        /// Key/Identifier of the the default SharePoint server which is <see cref="ServerPROD" /> in production and <see cref="ServerQA" /> in test environment
        /// </value>
        public static String ServerDefault
        {
            get
            {
                String serverName = ServerPROD;
                try
                {
                    if (Config.GlobalConfig.IsTestEnvironment) serverName = ServerQA;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception in 'ServerDefault':" + ex.Message);
                }
                return serverName;
            }
        }


        /// <summary>
        /// Gets the key/identifier of the SharePoint server in production environment (= <see cref="ServerPROD" /> as COM property)
        /// </summary>
        /// <value>
        ///   <see cref="ServerPROD" />
        /// </value>
        [Obsolete("ServerNameMOSS is deprecated, use ServerNamePROD instead.")]
        public String ServerNameMOSS { get { return ServerNamePROD; } }

        /// <summary>
        /// Gets the key/identifier of the SharePoint server in production environment (= <see cref="ServerPROD" /> as COM property)
        /// </summary>
        /// <value>
        ///   <see cref="ServerPROD" />
        /// </value>
        public String ServerNamePROD { get { return ServerPROD; } }


        /// <summary>
        /// Gets the key/identifier of the SharePoint server in production environment (= <see cref="ServerDefault" /> as COM property)
        /// </summary>
        /// <value>
        ///   <see cref="ServerDefault" />
        /// </value>
        [Obsolete("ServerNameMOSSDefault is deprecated, use ServerDefault instead.")]
        public String ServerNameMOSSDefault { get { return ServerDefault; } }

                /// <summary>
        /// Gets the key/identifier of the SharePoint server in production environment (= <see cref="ServerDefault" /> as COM property)
        /// </summary>
        /// <value>
        ///   <see cref="ServerDefault" />
        /// </value>
        public String ServerNameDefault { get { return ServerDefault; } }
        
        
        /// <summary>
        /// Gets the key/identifier of the SharePoint server in test environment (= <see cref="ServerQA" /> as COM property)
        /// </summary>
        /// <value>
        ///   <see cref="ServerQA" />
        /// </value>
        [Obsolete("ServerNameMOSSQC is deprecated, use ServerNameQA instead.")]
        public String ServerNameMOSSQC { get { return ServerNameQA; } }

        /// <summary>
        /// Gets the key/identifier of the SharePoint server in test environment (= <see cref="ServerQA" /> as COM property)
        /// </summary>
        /// <value>
        ///   <see cref="ServerQA" />
        /// </value>
        public String ServerNameQA { get { return ServerQA; } }

        /// <summary>
        /// Gets the key/identifier of the SharePoint server in dev environment (= <see cref="ServerDEV" /> as COM property)
        /// </summary>
        /// <value>
        ///   <see cref="ServerDEV" />
        /// </value>
        public String ServerNameDEV { get { return ServerDEV; } }


        /// <summary>
        /// Dictionary holding <see cref="ServerInfo" />-instances for the supported SharePoint servers
        /// </summary>
        private static Dictionary<String, ServerInfo> m_ServerInfos;

        /// <summary>
        /// Returns an enumerator that iterates <see cref="ServerInfo" />-instances for the supported SharePoint servers.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through <see cref="ServerInfo" />-instances for the supported SharePoint servers.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetServerInfos().Values.GetEnumerator();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerInfos" /> class.
        /// </summary>
        public ServerInfos()
        {
            GetServerInfos();
        }

        /// <summary>
        /// Gets the <see cref="ServerInfo" />-instances for the supported SharePoint servers.
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<String, ServerInfo> GetServerInfos()
        {
            if (m_ServerInfos == null)
            {
                m_ServerInfos = new Dictionary<String, ServerInfo>(3);

                ServerInfo si = new ServerInfo(ServerPROD, LongnamePROD, Config.GlobalConfig.URL_ServerPROD, CredentialCache.DefaultCredentials);
                m_ServerInfos.Add(si.Servername, si);

                si = new ServerInfo(ServerQA, LongnameQA, Config.GlobalConfig.URL_ServerQA, CredentialCache.DefaultCredentials);
                m_ServerInfos.Add(si.Servername, si);

                si = new ServerInfo(ServerDEV, LongnameDEV, Config.GlobalConfig.URL_ServerDEV, CredentialCache.DefaultCredentials);
                m_ServerInfos.Add(si.Servername, si);

                si = new ServerInfo(ServerMOSS, ServerMOSS + " (Mig)", Config.GlobalConfig.URL_ServerPROD, CredentialCache.DefaultCredentials);
                m_ServerInfos.Add(si.Servername, si);  //Allow to use the productive SP with deprecated key.

                si = new ServerInfo(ServerMOSSQC, ServerMOSSQC + " (Mig)", Config.GlobalConfig.URL_ServerQA, CredentialCache.DefaultCredentials);
                m_ServerInfos.Add(si.Servername, si); //Allow to use the SP in test environment with deprecated key.

            }
            return m_ServerInfos;
        }

        /// <summary>
        /// Gets the <see cref="ServerInfo" />-instance for the SharePoint server with the specified id/key
        /// </summary>
        /// <param name="serverId">The id/key of the server</param>
        /// <returns></returns>
        public static ServerInfo GetServerInfo(String serverId)
        {
            Dictionary<String, ServerInfo> Dic = GetServerInfos();
            return Dic[serverId];
        }

        /// <summary>
        /// Gets the <see cref="ServerInfo" /> with the specified id/key.
        /// </summary>
        /// <value>
        /// The <see cref="ServerInfo" />.
        /// </value>
        /// <param name="serverId">The id/key of the server</param>
        /// <returns>
        /// <see cref="ServerInfo" /> with the specified id/key.
        /// </returns>
        public ServerInfo this[String serverId]
        {
            get
            {
                Dictionary<String, ServerInfo> Dic = GetServerInfos();
                return Dic[serverId];
            }
        }

    }

}
