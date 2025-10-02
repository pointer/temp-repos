using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;
using System.Xml;
using System.Web;
using System.Diagnostics;

namespace UGIS.de.OfficeComponents.UniCacheLib
{
    /// <summary>
    /// Helper class providing access to SharePoint files (download and upload of files from/to SerVo site collection)
    /// </summary>
    /// <seealso cref="UGIS.de.OfficeComponents.UniCacheLib.ISharePointHelper" />
    [ComVisible(true)]
    [GuidAttribute("17FCA0CC-3111-4888-A7C2-0F20F057B3BC")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("UniCacheLib.SharePointHelper")]
    public class SharePointHelper : ISharePointHelper
    {

        /// <summary>
        /// The standard timeout (in milliseconds) which will used as start value for <see cref="DefaultTimeout"/>
        /// </summary>
        public const Int32 StandardTimeout = 10000;

        /// <summary>
        /// The server name used as key/identifier for SharePoint server in production environment (see <see cref="ServerInfos.ServerPROD"/>)
        /// </summary>
        /// <value>
        /// The server name used as key/identifier for SharePoint server in production environment
        /// </value>
        [Obsolete("ServerNameMOSS is deprecated, use ServerNamePROD instead.")]
        public String ServerNameMOSS { get { return ServerInfos.ServerPROD; } }

        /// <summary>
        /// The server name used as key/identifier for SharePoint server in production environment (see <see cref="ServerInfos.ServerPROD"/>)
        /// </summary>
        /// <value>
        /// The server name used as key/identifier for SharePoint server in production environment
        /// </value>
        public String ServerNamePROD { get { return ServerInfos.ServerPROD; } }


        /// <summary>
        /// The server name used as key/identifier for SharePoint server in test environment (see <see cref="ServerInfos.ServerQA"/>)
        /// </summary>
        /// <value>
        /// The server name used as key/identifier for SharePoint server in test environment
        /// </value>
        [Obsolete("ServerNameMOSSQC is deprecated, use ServerNameQA instead.")]
        public String ServerNameMOSSQC { get { return ServerInfos.ServerQA; } }

        /// <summary>
        /// The server name used as key/identifier for SharePoint server in test environment (see <see cref="ServerInfos.ServerQA"/>)
        /// </summary>
        /// <value>
        /// The server name used as key/identifier for SharePoint server in test environment
        /// </value>
        public String ServerNameQA { get { return ServerInfos.ServerQA; } }

        /// <summary>
        /// The server name used as key/identifier for SharePoint server in dev environment (see <see cref="ServerInfos.ServerDEV"/>)
        /// </summary>
        /// <value>
        /// The server name used as key/identifier for SharePoint server in dev environment
        /// </value>
        public String ServerNameDEV { get { return ServerInfos.ServerDEV; } }

        /// <summary>
        /// The default timeout for server requests when downloading files (will be used if no timeout is specified). 
        /// </summary>
        private Int32 m_DefaultTimeout = StandardTimeout;
        
        /// <summary>
        /// The default timeout for server requests when downloading files (will be used if no timeout is specified). 
        /// </summary>
        /// <value>
        /// The default timeout for server requests when downloading files (will be used if no timeout is specified). 
        /// </value>
        public Int32 DefaultTimeout
        {
            get { return m_DefaultTimeout; }
            set { m_DefaultTimeout = value; }
        }

        /// <summary>
        /// Value determining whether default credentials have to be used when downlaoding/uploading files.
        /// </summary>
        private Boolean m_UseDefaultCredentials = true;
        
        /// <summary>
        /// Gets or sets a value indicating whether default credentials have to be used when downlaoding/uploading files.
        /// </summary>
        /// <value>
        /// <c>true</c> if default credentials have to be used; otherwise, <c>false</c>.
        /// </value>
        public Boolean UseDefaultCredentials
        {
            get { return m_UseDefaultCredentials; }
            set { m_UseDefaultCredentials = value; }
        }

        private StringBuilder m_WebExceptions = new StringBuilder();
        /// <summary>
        /// Returns string list of web exceptions since last query of property WebExceptions
        /// </summary>
        public String WebExceptions
        {
            get {
                String webExceptions = m_WebExceptions.ToString();
                m_WebExceptions = new StringBuilder();
                return webExceptions; 
            }
        }

        /// <summary>
        /// Logger used to log usage of this SharePointHelper-instance
        /// </summary>
        private Logger m_UniCacheLogger;
        /// <summary>
        /// Gets or sets the logger to be used.
        /// </summary>
        /// <value>
        /// The logger to be used.
        /// </value>
        internal Logger UniCacheLogger
        {
            get 
            {
                if (m_UniCacheLogger == null) m_UniCacheLogger = new Logger("SPH");
                return m_UniCacheLogger; 
            }
            set { m_UniCacheLogger = value; }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("SharePointHelper '{0}' ({1})", m_ServerInfo.Servername);
        }

        /// <summary>
        /// <see cref="UGIS.de.OfficeComponents.UniCacheLib.ServerInfo"/>-instance providing server-related data defining which server will be used for 
        /// download/upload of files.
        /// </summary>
        /// <remarks>
        /// ServerInfo is set by the SharePointHelper-constructor, which will be the productive SharePoint server in case of the parameterless constructor.
        /// </remarks>
        private ServerInfo m_ServerInfo;
        /// <summary>
        /// <see cref="UGIS.de.OfficeComponents.UniCacheLib.ServerInfo"/>-instance providing server-related data defining which server will be used for 
        /// download/upload of files.
        /// </summary>
        /// <value>
        /// <see cref="UGIS.de.OfficeComponents.UniCacheLib.ServerInfo"/>-instance providing server-related data defining which server will be used for 
        /// download/upload of files.
        /// </value>
        public ServerInfo ServerInfo
        {
            get { return m_ServerInfo; }
        }

        /// <summary>
        /// Sets <see cref="ServerInfo"/>. 
        /// </summary>
        /// <remarks>
        /// <see cref="SetServerInfo"/> is needed for COM clients which can only use the parameterless SharePointHelper-constructor which sets <see cref="ServerInfo"/> to the SharePoint server in production environment.
        /// To use SharePoint servers in test or dev environment call <see cref="SetServerInfo"/> with the server name
        /// </remarks>
        /// <param name="servername">Key/Identifier of the SharePoint server to be used</param>
        public void SetServerInfo(String servername)
        {
            m_ServerInfo = ServerInfos.GetServerInfo(servername);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper"/> class using the production SharePoint server.
        /// </summary>
        public SharePointHelper()
        {
            m_ServerInfo = ServerInfos.GetServerInfo(ServerInfos.ServerPROD);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper"/> class for a given server.
        /// </summary>
        /// <param name="si"><see cref="ServerInfo"/> of the SharePoint server to be used.</param>
        public SharePointHelper(ServerInfo si)
        {
            m_ServerInfo = si;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointHelper"/> class for a given server.
        /// </summary>
        /// <param name="servername">Key/Identifier of the SharePoint server to be used.</param>
        public SharePointHelper(String servername)
        {
            m_ServerInfo = ServerInfos.GetServerInfo(servername);
        }

        /// <summary>
        /// Gets/Loads a XML (using <see cref="DefaultTimeout"/> as maximum request time).
        /// </summary>
        /// <param name="fileUrl">The URL of the xml-file to be loaded.</param>
        /// <returns>The <see cref="XmlDocument"/>-instance of the loaded file. If an error or timeout occurs <c>null</c> will be returned.</returns>
        public XmlDocument GetXml(String fileUrl)
        {
            return GetXmlTimeout(fileUrl, DefaultTimeout);
        }


        /// <summary>
        /// Gets/Loads a XML.
        /// </summary>
        /// <param name="fileUrl">The URL of the xml-file to be loaded.</param>
        /// <param name="timeout">The maximum request time.</param>
        /// <returns>The <see cref="XmlDocument"/>-instance of the loaded file. If an error or timeout occurs <c>null</c> will be returned.</returns>
        public XmlDocument GetXmlTimeout(String fileUrl, Int32 timeout)
        {
            XmlDocument result = null;
            Stream fileStream = null;
            Int64 contentLength;
            UniCacheLogger.EnterSection("GET SERVER xml");
            try
            {
                UniCacheLogger.WriteLineAppendTime("Getting xml with url='{0}'", fileUrl);
                XmlDocument loadedXml = new XmlDocument();
                fileStream = GetFileStream(fileUrl, timeout, out contentLength);
                loadedXml.Load(fileStream);
                UniCacheLogger.WriteLineAppendTime("Success getting xml!");
                result = loadedXml;
            }
            catch (Exception ex)
            {
                result = null;
                UniCacheLogger.WriteException("Error getting xml with url='{0}'", ex, fileUrl);
            }
            finally
            {
                if (fileStream != null) fileStream.Close(); 
                UniCacheLogger.ExitSection();
            }
            return result;
        }

        /// <summary>
        /// Gets/Loads a file (UTF-8 encoded).
        /// </summary>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="timeout">The maximum request time.</param>
        /// <returns>The UTF8-encoded file content. If an error or timeout occurs <c>null</c> will be returned.</returns>
        public String GetFileUTF8(String fileUrl, Int32 timeout)
        {
            return GetFile(fileUrl, Encoding.UTF8, timeout);
        }

        /// <summary>
        /// Gets/Loads a file (Unicode encoded).
        /// </summary>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="timeout">The maximum request time.</param>
        /// <returns>The Unicode-encoded file content. If an error or timeout occurs <c>null</c> will be returned.</returns>
        public String GetFileUnicode(String fileUrl, Int32 timeout)
        {
            return GetFile(fileUrl, Encoding.Unicode, timeout);
        }

        /// <summary>
        /// Gets/Loads a file (ASCII encoded).
        /// </summary>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="timeout">The maximum request time.</param>
        /// <returns>The ASCII-encoded file content. If an error or timeout occurs <c>null</c> will be returned.</returns>
        public String GetFileASCII(String fileUrl, Int32 timeout)
        {
            return GetFile(fileUrl, Encoding.ASCII, timeout);
        }

        /// <summary>
        /// Gets/Loads a file using the encoding passed as parameter.
        /// </summary>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="encoding">The encoding to be used.</param>
        /// <param name="timeout">The maximum request time.</param>
        /// <returns>
        /// The encoded file content. If an error or timeout occurs <c>null</c> will be returned.
        /// </returns>
        [ComVisible(false)]
        public String GetFile(String fileUrl, Encoding encoding, Int32 timeout)
        {
            String result = null;
            try
            {
                Byte[] ba = GetFileTimeout(fileUrl, timeout);
                if (ba != null) result = encoding.GetString(ba);
            }
            catch (Exception ex) 
            {
                UniCacheLogger.WriteException("Error getting file encoded with url='{0}'", ex, fileUrl);
            }
            return result;
        }

        /// <summary>
        /// Gets/Loads a file (using <see cref="DefaultTimeout"/> as maximum request time).
        /// </summary>
        /// <param name="fileUrl">The file URL.</param>
        /// <returns>
        /// The file content as byte-array. If an error or timeout occurs <c>null</c> will be returned.
        /// </returns>
        public Byte[] GetFile(String fileUrl)
        {
            return GetFileTimeout(fileUrl, DefaultTimeout);
        }

        /// <summary>
        /// Gets/Loads a file.
        /// </summary>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="timeout">The maximum request time.</param>
        /// <returns>
        /// The file content as byte-array. If an error or timeout occurs <c>null</c> will be returned.
        /// </returns>
        public Byte[] GetFileTimeout(String fileUrl, Int32 timeout)
        {
            Byte[] result = null;
            UniCacheLogger.EnterSection("GET SERVER file");
            try
            {
                UniCacheLogger.WriteLineAppendTime("Getting file with url='{0}'", fileUrl);
                Int64 contentLength;
                Stream fileStream = GetFileStream(fileUrl, timeout, out contentLength);
                if (fileStream != null)
                {
                    result = new Byte[contentLength];
                    Int32 offset = 0;
                    Int32 remaining = (Int32)contentLength;
                    while (remaining > 0)
                    {
                        Int32 readBytes = fileStream.Read(result, offset, remaining);
                        if (readBytes <= 0) throw new EndOfStreamException(String.Format("End of stream reached with {0} bytes left to read", remaining));
                        remaining -= readBytes;
                        offset += readBytes;
                    }
                    UniCacheLogger.WriteLineAppendTime("Success getting file!");
                }
                else
                {
                    UniCacheLogger.WriteLineAppendTime("Didn't get file!");
                }
            }
            catch (Exception ex) 
            {
                UniCacheLogger.WriteException("getting file with url='{0}'", ex, fileUrl);
            }
            UniCacheLogger.ExitSection();
            return result;
        }


        /// <summary>
        /// Gets a file stream (using <see cref="DefaultTimeout"/> as maximum request time).
        /// </summary>
        /// <param name="fileUrl">The file URL.</param>
        /// <returns>
        /// The file stream. If an error or timeout occurs <c>null</c> will be returned.
        /// </returns>
        public Stream GetFileStream(String fileUrl)
        {
            Int64 l;
            return GetFileStream(fileUrl, DefaultTimeout, out l);
        }

        /// <summary>
        /// Gets a file stream.
        /// </summary>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="timeout">The maximum request time.</param>
        /// <returns>
        /// The file stream. If an error or timeout occurs <c>null</c> will be returned.
        /// </returns>
        public Stream GetFileStreamTimeout(String fileUrl, Int32 timeout)
        {
            Int64 l;
            return GetFileStream(fileUrl, timeout, out l);
        }



        /// <summary>
        /// Gets a file stream.
        /// </summary>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="timeout">The maximum request time.</param>
        /// <param name="contentLength">Length of the content.</param>
        /// <returns>
        /// The file stream. If an error or timeout occurs <c>null</c> will be returned.
        /// </returns>
        private Stream GetFileStream(String fileUrl, Int32 timeout, out Int64 contentLength)
        {
            WebRequest request;
            Stream result = null;
            contentLength = -1;
            String webException = null;
            UniCacheLogger.EnterSection("GET SERVER filestream");
            try
            {
                UniCacheLogger.WriteLine("Getting filestream for url='{0}' with TIMEOUT={1}", fileUrl, timeout);
                request = WebRequest.Create(fileUrl);
                SetCredentialsToUse(request);
                request.Method = "GET";
                request.Timeout = timeout;
                WebResponse response = request.GetResponse();
                contentLength = response.ContentLength;
                result = response.GetResponseStream();
                UniCacheLogger.WriteLineAppendTime("Success getting filestream!");
            }
            catch (WebException wex)
            {
                HttpWebResponse response = (HttpWebResponse)wex.Response;
                if (response != null)
                {
                    webException = String.Format("{0} Requested URL: '{1}'!", wex.Message, fileUrl);
                }
                UniCacheLogger.WriteException("An WebException occured: Error getting filestream for url='{0}' (response-status={1})", (Exception)wex, fileUrl, webException);
            }
            catch (Exception ex)
            {
                UniCacheLogger.WriteException("Error getting filestream for url='{0}'", ex, fileUrl);
            }
            finally
            {
                if (!String.IsNullOrEmpty(webException))
                {
                    m_WebExceptions.AppendLine(webException);
                }
            }
            UniCacheLogger.ExitSection();
            return result;
        }

        /// <summary>
        /// Sets the credentials to use.
        /// </summary>
        /// <param name="request">The request.</param>
        private void SetCredentialsToUse(WebRequest request)
        {
            if (m_UseDefaultCredentials)
            {
                request.UseDefaultCredentials = true;
            }
            else
            {
                request.UseDefaultCredentials = false;
                request.Credentials = m_ServerInfo.AdminCredentials;
            }
        }

        /// <summary>
        /// Uploads a file using WebDAV. If that fails a FrontPage-RPC will be used to upload the file.
        /// </summary>
        /// <param name="webUrl">The web URL.</param>
        /// <param name="documentName">Name of the document.</param>
        /// <param name="content">The content to upload.</param>
        /// <returns><c>true</c> if the upload has been successful; otherwise, <c>false</c>.</returns>
        public Boolean UploadFile(String webUrl, String documentName, Byte[] content)
        {
            Boolean result = false;
            try
            {
                UniCacheLogger.WriteLineAppendTime("Uploading file '{0}' with url='{1}'", documentName, webUrl);
                result = UploadFileWebDAV(webUrl + documentName, content);
                if (!result) result = UploadFileFPRPC(webUrl, documentName, content);
                UniCacheLogger.WriteLineAppendTime("Success uploading file!");
            }
            catch (Exception ex)
            {
                UniCacheLogger.WriteException("Error uploading file '{0}' with url='{1}'", ex, documentName, webUrl);
            }
            return result;
        }

        /// <summary>
        /// Uploads a file using WebDAV.
        /// </summary>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="content">The content to upload.</param>
        /// <returns><c>true</c> if the upload has been successful; otherwise, <c>false</c>.</returns>
        public Boolean UploadFileWebDAV(String fileUrl, Byte[] content)
        {
            Boolean result = false;
            UniCacheLogger.EnterSection("UPLOAD SERVER file (WebDAV)");
            try
            {
                UniCacheLogger.WriteLineAppendTime("Uploading file using WebDAV with url='{0}'", fileUrl);
                WebRequest request = WebRequest.Create(fileUrl);
                SetCredentialsToUse(request);
                request.Method = "PUT";

                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(content, 0, content.Length);
                }

                WebResponse response = null;
                try
                {
                    response = request.GetResponse();
                    result = true;
                    UniCacheLogger.WriteLineAppendTime("Success uploading file using WebDAV!");
                }
                catch (Exception ex)
                {
                    UniCacheLogger.WriteException("Error getting response", ex, fileUrl);
                }
                finally
                {
                    if (response != null) response.Close();
                }
            }
            catch (Exception ex)
            {
                UniCacheLogger.WriteException("Error uploading file using WebDAV for url='{0}'", ex, fileUrl);
            }
            UniCacheLogger.ExitSection();
            return result;
        }

        /// <summary>
        /// Encodes meta information (URL-parameters for FrontPage-RPC).
        /// </summary>
        /// <param name="metaInfo">The dictionary containing meta information to be encoded.</param>
        /// <returns>The encoded URL-parameters for FrontPage-RPC.</returns>
        private static String EncodeMetaInfo(Dictionary<String, Object> metaInfo)
        {
            if (metaInfo == null) return "";
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<String, Object> kvp in metaInfo)
            {
                if (kvp.Value != null)
                {
                    string fieldName = kvp.Key; // note: field names are case sensitive
                    switch (fieldName)
                    {
                        case "Title":
                            fieldName = "vti_title";
                            break;
                    }
                    string data = EscapeVectorChars(kvp.Value.ToString());
                    string dataTypeCode = "S";
                    switch (kvp.Value.GetType().FullName)
                    {
                        case "System.Boolean":
                            dataTypeCode = "B";
                            break;
                        case "System.DateTime":
                            data = ((DateTime)kvp.Value).ToString("s") + "Z";
                            break;
                    }

                    sb.AppendFormat("{0};{1}W|{2};", fieldName, dataTypeCode, data);
                }
            }
            return HttpUtility.UrlEncode(sb.ToString().TrimEnd(';'));
        }

        /// <summary>
        /// Escapes vector chars within a <see cref="System.String" />.
        /// </summary>
        /// <param name="value">The <see cref="System.String" /> to be encoded.</param>
        /// <returns><see cref="System.String" /> with escaped vector chars.</returns>
        private static String EscapeVectorChars(String value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                switch (c)
                {
                    case ';':
                    case '|':
                    case '[':
                    case ']':
                    case '\\':
                        sb.Append("\\");
                        break;
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Uploads a file using FrontPage-RPC.
        /// </summary>
        /// <param name="webUrl">The web URL.</param>
        /// <param name="documentName">Name of the document.</param>
        /// <param name="bytes">The content as byte-array.</param>
        /// <returns><c>true</c> if the upload has been successful; otherwise, <c>false</c>.</returns>
        public Boolean UploadFileFPRPC(String webUrl, String documentName, Byte[] bytes)
        {
            String result;
            return UploadFileFPRPC(webUrl, documentName, bytes, null, out result);
        }

        /// <summary>
        /// Uploads a file using FrontPage-RPC.
        /// </summary>
        /// <param name="webUrl">The web URL.</param>
        /// <param name="documentName">Name of the document.</param>
        /// <param name="bytes">The content as byte-array.</param>
        /// <param name="metaInfo">The meta information.</param>
        /// <param name="resultMessage">The result message received as server response.</param>
        /// <returns><c>true</c> if the upload has been successful; otherwise, <c>false</c>.</returns>
        [ComVisible(false)]
        public Boolean UploadFileFPRPC(String webUrl, String documentName, byte[] bytes, Dictionary<String, Object> metaInfo, out String resultMessage)
        {
            Boolean result = false;
            resultMessage = "error uploading file";
            UniCacheLogger.EnterSection("UPLOAD SERVER file (FPRPC)");
            try
            {
                UniCacheLogger.WriteLineAppendTime("Uploading file using FPRPC with url='{0}'", webUrl);
                String putOption = "overwrite,createdir,migrationsemantics";  // see http://msdn2.microsoft.com/en-us/library/ms455325.aspx
                String comment = null;
                Boolean keepCheckedOut = false;
                String method = "method=put+document%3a12.0.4518.1016&service_name=%2f&document=[document_name={0};meta_info=[{1}]]&put_option={2}&comment={3}&keep_checked_out={4}\n";
                method = String.Format(method, documentName, EncodeMetaInfo(metaInfo), putOption, HttpUtility.UrlEncode(comment), keepCheckedOut.ToString().ToLower());

                List<Byte> Data = new List<Byte>();
                Data.AddRange(Encoding.UTF8.GetBytes(method));
                Data.AddRange(bytes);

                using (WebClient webClient = new WebClient())
                {
                    if (m_UseDefaultCredentials)
                    {
                        webClient.UseDefaultCredentials = true;
                    }
                    else
                    {
                        webClient.UseDefaultCredentials = false;
                        webClient.Credentials = m_ServerInfo.AdminCredentials;
                    }
                    webClient.Headers.Add("Content-Type", "application/x-vermeer-urlencoded");
                    webClient.Headers.Add("X-Vermeer-Content-Type", "application/x-vermeer-urlencoded");

                    resultMessage = Encoding.UTF8.GetString(webClient.UploadData(webUrl + "/_vti_bin/_vti_aut/author.dll", "POST", Data.ToArray()));

                    if (resultMessage.IndexOf("\n<p>message=successfully") >= 0) result = true;
                }
                UniCacheLogger.WriteLineAppendTime("Success uploading file using FPRPC!");
            }

            catch (Exception ex)
            {
                UniCacheLogger.WriteException("Error uploading file '{0}' using FPRPC with url='{1}'", ex, documentName, webUrl);
            }
            UniCacheLogger.ExitSection();
            return result;
        }


        /// <summary>
        /// Increments the UniCache statistic for an application.
        /// </summary>
        /// <param name="application">The application name.</param>
        /// <param name="attribName">Name of the application's xml-attribute to be incremented.</param>
        /// <returns><c>true</c> if the statistic counter has been incremented; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// <para>Increments a statistic counter within the SerVo-statistic-xml of the current day:</para>
		/// <code language="xml" title="Example of the written/incremented statistic tag">
		/// 	<![CDATA[
        /// 	    <Statistic Day="20160420">
	    /// 		    <UniCache>
	    /// 			    <App Name="application" _attrbName="1" />
	    /// 	    	</UniCache>
        /// 	    </Statistic >
        /// 	]]>
        /// </code>
        /// </remarks>
        public Boolean IncUniCacheStatistic(String application, String attribName)
        {
            Boolean result = false;
            if (!String.IsNullOrEmpty(application) && !String.IsNullOrEmpty(attribName))
            {
                String unicacheStatisticPath = String.Format("UniCache/App@{0}/@{1}", application, attribName);
                result = IncStatistic(unicacheStatisticPath);
            }
            return result;
        }

        /// <summary>
        /// Increments the UniCache statistic.
        /// </summary>
        /// <param name="statisticPath">The statistic path of the counter to be incremented.</param>
        /// <returns><c>true</c> if the statistic counter has been incremented; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// <para>Some examples for statisticPath values and their corresponding statistic xml tags:</para>
        /// <code language="xml" title="Example of adding integers with statisticPath=&quot;MyData/@Sum=add(55)&quot;">
        /// 	<![CDATA[
        /// 	    <Statistic Day="20160420">
        /// 	        <MyData StoredCount="0" _Sum="55" />
        /// 	    </Statistic >
        /// 	]]>
        /// </code>
        /// 
        /// <code language="xml" title="Example of incrementing integers with statisticPath=&quot;MyData/@Counts=inc()&quot;">
        /// 	<![CDATA[
        /// 	    <Statistic Day="20160420">
        /// 	        <MyData StoredCount="0" _Counts="1" />
        /// 	    </Statistic >
        /// 	]]>
        /// </code>
        /// 
        ///
        /// <code language="xml" title="Example of concating Strings with statisticPath=&quot;MyData/@Text=concat(ABC)&quot;">
        /// 	<![CDATA[
        /// 	    <Statistic Day="20160420">
        /// 	        <MyData StoredCount="0" _Text="+ABC" />
        /// 	    </Statistic >
        /// 	]]>
        /// </code>
        /// </remarks>
        public Boolean IncStatistic(String statisticPath)
        {
            Boolean result = false;
            Stream fileStream = null;
            try
            {
                if (!String.IsNullOrEmpty(statisticPath))
                {
                    UniCacheLogger.WriteLineAppendTime("Incrementing statistic for path '{0}'", statisticPath);
                    String rootUrl = ServerInfo.ServoRootUrl;
                    String serviceURL = rootUrl + Config.GlobalConfig.StatisticIncRelURL + statisticPath;
                    XmlDocument xmlDoc = GetXmlTimeout(serviceURL, Config.GlobalConfig.DefaultTimeoutServerRequest);
                    XmlAttribute rcAttrib = xmlDoc.SelectSingleNode("//Result/@RC") as XmlAttribute;
                    String rc = rcAttrib != null ? rcAttrib.Value : "?";
                    result = rc == "0";
                    UniCacheLogger.WriteLineAppendTime(result ? "Statistic incremented!" : "Incrementing statistic failed with rc='{0}'!", rc);
                }
            }
            catch (Exception ex)
            {
                UniCacheLogger.WriteException("Error incrementing statistic for path '{0}'", ex, statisticPath);
            }
            finally
            {
                if (fileStream != null) fileStream.Close();
            }
            return result;
        }


        private delegate Boolean IncStatisticDelegate(String statisticPath);

        /// <summary>
        /// Increments the UniCache statistic.
        /// </summary>
        /// <param name="statisticPath">The statistic path of the counter to be incremented.</param>
        /// <returns><c>true</c> if the statistic counter has been incremented; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// <para>Some examples for statisticPath values and their corresponding statistic xml tags:</para>
        /// <code language="xml" title="Example of adding integers with statisticPath=&quot;MyData/@Sum=add(55)&quot;">
        /// 	<![CDATA[
        /// 	    <Statistic Day="20160420">
        /// 	        <MyData StoredCount="0" _Sum="55" />
        /// 	    </Statistic >
        /// 	]]>
        /// </code>
        /// 
        /// <code language="xml" title="Example of incrementing integers with statisticPath=&quot;MyData/@Counts=inc()&quot;">
        /// 	<![CDATA[
        /// 	    <Statistic Day="20160420">
        /// 	        <MyData StoredCount="0" _Counts="1" />
        /// 	    </Statistic >
        /// 	]]>
        /// </code>
        /// 
        ///
        /// <code language="xml" title="Example of concating Strings with statisticPath=&quot;MyData/@Text=concat(ABC)&quot;">
        /// 	<![CDATA[
        /// 	    <Statistic Day="20160420">
        /// 	        <MyData StoredCount="0" _Text="+ABC" />
        /// 	    </Statistic >
        /// 	]]>
        /// </code>
        /// </remarks>
        public void IncStatisticAsync(String statisticPath)
        {
            UniCacheLogger.EnterSection("IncStatisticAsync for path='{0}'", statisticPath);
            try
            {
                UniCacheLogger.WriteLine("IncStatisticAsync ");
                IncStatisticDelegate incStatisticDelegate = new IncStatisticDelegate(this.IncStatistic);
                IAsyncResult iAsynchResult = incStatisticDelegate.BeginInvoke(statisticPath, null, incStatisticDelegate);
            }
            catch (Exception ex)
            {
                UniCacheLogger.WriteException("Exception in 'IncStatisticAsync'", ex);
            }
            UniCacheLogger.ExitSection();
        }

    }

}
