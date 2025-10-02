using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace UGIS.de.OfficeComponents.UniCacheLib
{

    /// <summary>
    /// Element of UniCache representing a single content-file
    /// </summary>
    /// <seealso cref="UGIS.de.OfficeComponents.UniCacheLib.IUniCacheElement" />
    [ComVisible(true)]
    [Guid("4AC0EEE9-8638-4946-9612-99399B80E94D")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("UniCacheLib.UniCacheElement")]
    public class UniCacheElement : IUniCacheElement
    {

        /// <summary>
        /// Name of XMLElement representing an UniCacheElement (see content-xml)
        /// </summary>
        internal static String XmlElement_UniCacheFile = "UniCacheFile";

        /// <summary>
        /// Name of XMLAttribute containing the cached filename (see content-xml)
        /// </summary>
        /// <remarks>
        /// Only used in old content-xmls. Needed for migration only (which should be not needed any more)
        /// </remarks>
        internal const String XmlAttrib_CachedFilename = "CachedFilename";
        
        /// <summary>
        /// Name of XMLAttribute containing Timeout time (see content-xml)
        /// </summary>
        private const String XmlAttrib_Timeout = "Timeout";

        /// <summary>
        /// Name of XMLAttribute containing the logical filename (see content-xml)
        /// </summary>
        private const String XmlAttrib_LogicalFilename = "LogicalFilename";
        
        /// <summary>
        /// Name of XMLAttribute containing the server relative filename
        /// </summary>
        private const String XmlAttrib_ServerRelativeFilename = "ServerRelativeFilename";
        
        /// <summary>
        /// Name of XMLAttribute containing the flag for auto increment of the usage statistic
        /// </summary>
        private const String XmlAttrib_AutoIncStatistic = "AutoIncStatistic";
        
        /// <summary>
        /// Name of XMLAttribute containing the file version 
        /// </summary>
        private const String XmlAttrib_Version = "Version";


        /// <summary>
        /// Meta-Data-Dictionary containing metadata-values of this UniCacheElement
        /// </summary>
        private Dictionary<String, String> m_MetadataDict;


        /// <summary>
        /// The timeout for downloading the file from the server.
        /// </summary>
        private Int32 m_Timeout;
        
        /// <summary>
        /// Gets or sets the timeout for downloading the file from the server.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public Int32 Timeout
        {
            get { return m_Timeout; }
            set { m_Timeout = value; }
        }

        /// <summary>
        /// The version of the file.
        /// </summary>
        private String m_Version;
        
        /// <summary>
        /// Gets the file version.
        /// </summary>
        /// <value>
        /// The file version.
        /// </value>
        public String Version
        {
            get { return m_Version; }
            internal set { m_Version = value; }
        }


        /// <summary>
        /// Set to <c>true</c> to increment usage statistic for this file when the file is used (e.g. <see cref="M:GetCacheFilenameCopy"/> is called)
        /// </summary>
        private Boolean m_AutoIncStatistic;

        /// <summary>
        /// The logical filename
        /// </summary>
        /// <remarks>The logical filename is used like a key to access the cached file.</remarks>
        private String m_LogicalFilename;
        
        /// <summary>
        /// Gets the logical filename.
        /// </summary>
        /// <value>
        /// The logical filename.
        /// </value>
        public String LogicalFilename
        {
            get { return m_LogicalFilename; }
            internal set { m_LogicalFilename = value; }
        }

        /// <summary>
        /// The server relative filename of the cache file 
        /// </summary>
        /// <remarks>The server relative filename is relative to the application cache folder on the server. 
        /// It might be a filename or a filename with a relative path (e.g. <c>subfolder/filename</c>)</remarks>
        private String m_ServerRelativeFilename;
        
        /// <summary>
        /// Gets the server relative filename.
        /// </summary>
        /// <value>
        /// The server relative filename.
        /// </value>
        public String ServerRelativeFilename
        {
            get { return m_ServerRelativeFilename; }
            internal set { m_ServerRelativeFilename = value; }
        }

        /// <summary>
        /// Gets the full URL of the cache file on the server.
        /// </summary>
        /// <value>
        /// The full URL of the cache file on the server.
        /// </value>
        public String ServerFilename
        {
            get { return m_UniCache.ApplicationsCacheUrl + m_ServerRelativeFilename; }
        }

        /// <summary>
        /// Touches the cached file.
        /// </summary>
        /// <remarks>Prevents the cached file to be deleted when cleaning the cache (<see cref="M:UGIS.de.OfficeComponents.UniCacheLib.UniCache.CleanCache"/>)</remarks>
        internal void TouchCachedFile(Logger logger, DateTime today)
        {
            try
            {
                if (ExistsInCache)
                {
                    using (logger.CreateSection("Touching '{0}'", true, AbsoluteCachedFilename))
                    {
                        FileInfo fi = new FileInfo(AbsoluteCachedFilename);
                        if (fi.Exists && !IsSameDay(fi.LastAccessTime, today))
                        {
                            fi.LastAccessTime = today;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.WriteException("Failed to set last access time of file '{0}'", ex, AbsoluteCachedFilename);
            }
        }

        private Boolean IsSameDay(DateTime d1, DateTime d2)
        {
            return (d1.Year == d2.Year) && (d1.Month == d2.Month) && (d1.Day == d2.Day);
        }

        /// <summary>
        /// Gets the filename (without path) of the cached file.
        /// </summary>
        /// <value>
        /// The filename (without path) of the cached file.
        /// </value>
        internal String CachedFilename
        {
            get
            {
                Int32 pos = ServerRelativeFilename.LastIndexOf('.');
                String extension = pos == -1 ? String.Empty : ServerRelativeFilename.Substring(pos);
                return LogicalFilename + "_" + Version + extension;
            }
        }

        /// <summary>
        /// Gets the (full) filename (with path) of the cached (uncopied) file.
        /// </summary>
        /// <returns>
        /// Filename (with path) of the cached file
        /// </returns>
        [ComVisible(false)]
        public String GetCacheFilename()
        {
            return GetCacheFilename(false, false);
        }

        /// <summary>
        /// Gets the (full) filename (with path) of the cached (uncopied) file.
        /// </summary>
        /// <param name="forceDownload">if set to <c>true</c> the file will be downloaded even if already cached.</param>
        /// <returns>
        /// Filename with path of the cached file
        /// </returns>
        public String GetCacheFilename(Boolean forceDownload)
        {
            return GetCacheFilename(forceDownload, false);
        }

        /// <summary>
        /// Gets the (full) filename (with path) of the cached file.
        /// </summary>
        /// <param name="forceDownload">if set to <c>true</c> the file will be downloaded even if already cached.</param>
        /// <param name="createCopy">if set to <c>true</c> the cached file will be copied to a temporary folder (may used to avoid access conflicts).</param>
        /// <returns>
        /// Filename with path of the cached file
        /// </returns>
        [ComVisible(false)]
        public String GetCacheFilename(Boolean forceDownload, Boolean createCopy)
        {
            String result = null;
            if (m_UniCache != null) result = m_UniCache.GetCacheFilename(LogicalFilename, forceDownload, createCopy);
            return result;
        }

        /// <summary>
        /// Gets the (full) filename (with path) of a copy of the cached file.
        /// </summary>
        /// <param name="forceDownload">if set to <c>true</c> the file will be downloaded even if already cached.</param>
        /// <param name="destinationFilename">The destination filename of the cached file copy.</param>
        /// <returns>If successful the destination filename of the cached file copy will be returned; <c>null</c> otherwise</returns>
        public String GetCacheFilenameCopy(Boolean forceDownload, String destinationFilename)
        {
            String result = null;
            if (m_UniCache != null) result = m_UniCache.GetCacheFilenameCopy(LogicalFilename, forceDownload, destinationFilename);
            return result;
        }

        /// <summary>
        /// Gets the absolute filename of the cached file.
        /// </summary>
        /// <value>
        /// The absolute filename of the cached file.
        /// </value>
        internal String AbsoluteCachedFilename
        {
            get { return m_UniCache.ApplicationsCacheTempFolder + CachedFilename; }
        }

        /// <summary>
        /// The UniCache-instance this UniCacheElement is part of
        /// </summary>
        private UniCache m_UniCache;

        /// <summary>
        /// Creates an UniCacheElement for UniCache-instance <c>uc</c> (and migrates to new filenames)
        /// </summary>
        /// <param name="node">XmlNode containing data of the UniCacheElement</param>
        /// <param name="uc">UniCache-instance to create an element for</param>
        /// <param name="logger">The logger.</param>
        internal UniCacheElement(XmlNode node, UniCache uc, Logger logger)
        {
            try
            {
                m_MetadataDict = new Dictionary<String, String>(StringComparer.Create(CultureInfo.InvariantCulture, true));
                m_UniCache = uc;

                ReadMandatoryAttribute(node, XmlAttrib_LogicalFilename, ref m_LogicalFilename, logger);
                ReadMandatoryAttribute(node, XmlAttrib_Version, ref m_Version, logger);
                ReadMandatoryAttribute(node, XmlAttrib_ServerRelativeFilename, ref m_ServerRelativeFilename, logger);

                Int32 defaultTimeout = uc != null ? uc.DefaultTimeout : SharePointHelper.StandardTimeout;
                m_Timeout = Convert.ToInt32(Common.GetAttribValue(node, XmlAttrib_Timeout, defaultTimeout.ToString("D")));
                m_AutoIncStatistic = Common.GetAttribValue(node, XmlAttrib_AutoIncStatistic, "false").ToLowerInvariant() == "true";

                String cachedFilename = Common.GetAttribValue(node, XmlAttrib_CachedFilename, String.Empty);
                if (!String.IsNullOrEmpty(cachedFilename))
                {
                    String cachedFullname = uc.ApplicationsCacheTempFolder + cachedFilename;
                    if (!File.Exists(AbsoluteCachedFilename) && File.Exists(cachedFullname))
                    {
                        // Migrate to new filename schema
                        File.Copy(cachedFullname, AbsoluteCachedFilename);
                    }
                }

                foreach (XmlAttribute attrib in node.Attributes)
                {
                    String dataName = attrib.Name ?? String.Empty;

                    if (!IsReservedAttribName(dataName) && !m_MetadataDict.ContainsKey(dataName))
                    {
                        String value = attrib.Value ?? String.Empty;
                        m_MetadataDict.Add(dataName, value);
                        logger.WriteLine("Added MetaData {0}='{1}'", dataName, value);
                    }
                }
                
                logger.WriteLine("UniCacheElement '{0}' created!", this.ToString());
            }
            catch (Exception ex)
            {
                logger.WriteException("Exception in UniCacheElement-constructor!", ex);
            }
        }

        /// <summary>
        /// Checks whether an attrib-name is reserved
        /// </summary>
        /// <param name="name">attribute-name to check</param>
        /// <returns>Returns true if name is reserved; otherwise false</returns>
        private Boolean IsReservedAttribName(String name)
        {
            Boolean isReserved = String.IsNullOrEmpty(name);    //Handle this case like isreserved == true
            isReserved = isReserved || XmlAttrib_AutoIncStatistic.Equals(name, StringComparison.InvariantCultureIgnoreCase);
            isReserved = isReserved || XmlAttrib_CachedFilename.Equals(name, StringComparison.InvariantCultureIgnoreCase);
            isReserved = isReserved || XmlAttrib_LogicalFilename.Equals(name, StringComparison.InvariantCultureIgnoreCase);
            isReserved = isReserved || XmlAttrib_ServerRelativeFilename.Equals(name, StringComparison.InvariantCultureIgnoreCase);
            isReserved = isReserved || XmlAttrib_Timeout.Equals(name, StringComparison.InvariantCultureIgnoreCase);
            isReserved = isReserved || XmlAttrib_Version.Equals(name, StringComparison.InvariantCultureIgnoreCase);
            return isReserved;
        }


        /// <summary>
        /// Reads a mandatory xml-attribute.
        /// </summary>
        /// <param name="node">The xml-node.</param>
        /// <param name="attribName">Name of the xml-attribute.</param>
        /// <param name="attribValue">The xml-attributes value.</param>
        /// <param name="logger">The logger.</param>
        private void ReadMandatoryAttribute(XmlNode node, String attribName, ref String attribValue, Logger logger)
        {
            attribValue = Common.GetAttribValue(node, attribName, null);
            if (String.IsNullOrEmpty(attribValue))
            {
                logger.WriteLine("!? Warning: XmlAttribute '{0}' should not be undefined or empty!", attribName);
            }
        }


        /// <summary>
        /// Appends a xml-element representing this UniCacheElement to the parent node
        /// </summary>
        /// <param name="parentnode">parent node the element is appended to</param>
        internal void AppendToXml(XmlNode parentnode)
        {
            try
            {
                if (m_UniCache != null) m_UniCache.UniCacheLogger.WriteLine("Append UniCacheElement '{0}' to xml!", this.ToString());
                XmlNode uniCacheElemNode = parentnode.OwnerDocument.CreateElement(XmlElement_UniCacheFile);
                Common.SetAttribValue(uniCacheElemNode, XmlAttrib_LogicalFilename, m_LogicalFilename);
                Common.SetAttribValue(uniCacheElemNode, XmlAttrib_Version, m_Version);
                Common.SetAttribValue(uniCacheElemNode, XmlAttrib_ServerRelativeFilename, m_ServerRelativeFilename);
                Common.SetAttribValue(uniCacheElemNode, XmlAttrib_Timeout, m_Timeout.ToString("D"));
                Common.SetAttribValue(uniCacheElemNode, XmlAttrib_AutoIncStatistic, m_AutoIncStatistic ? "true" : "false");

                foreach (KeyValuePair<String, String> kvp in m_MetadataDict)
                {
                    Common.SetAttribValue(uniCacheElemNode, kvp.Key, kvp.Value ?? String.Empty);
                }

                parentnode.AppendChild(uniCacheElemNode);
            }
            catch (Exception ex)
            {
                if (m_UniCache != null) m_UniCache.UniCacheLogger.WriteException("Exception appending UniCacheElement{0}!", ex, this.ToString());
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            String result = m_LogicalFilename;
            try
            {
                result += String.Format(" ({0}, Cached={1}, AutoIncStatistic={2}, Timeout={3}ms;)", m_Version, ExistsInCache, m_AutoIncStatistic, m_Timeout);
            }
            catch { }
            return result;
        }

        /// <summary>
        /// Gets an information about the file in the cache.
        /// </summary>
        /// <value>
        /// The information about the file in the cache.
        /// </value>
        public String CacheFileInfo
        {
            get { return ToString(); }
        }

        /// <summary>
        /// Returns the time of the last refresh/download (or <c>DateTime.MinValue</c> if not refreshed)
        /// </summary>
        /// <value>
        /// The last refresh time.
        /// </value>
        public DateTime LastRefresh
        {
            get 
            { 
                DateTime lastRefresh = DateTime.MinValue;
                FileInfo fi = new FileInfo(AbsoluteCachedFilename);
                if (fi != null && fi.Exists)
                {
                    lastRefresh = fi.LastWriteTime;
                }
                return lastRefresh; 
            }
        }

        /// <summary>
        /// Downloads the file corresponding to this UniCacheElement if it's not in the cache or a download is forced by parameter
        /// </summary>
        /// <param name="forceDownload">if set to <c>true</c> the file will be downloaded and overwritten even if it is already cached.</param>
        /// <param name="raiseEvent">if set to <c>true</c> an OnCacheElementLoaded-event will be fired.</param>
        /// <returns>
        /// Returns <c>true</c>, if file is available (already existing or has been successfully downloaded), otherwise <c>false</c>.
        /// </returns>
        internal Boolean LoadIfFileIsMissing(Boolean forceDownload, Boolean raiseEvent)
        {
            Logger logger = m_UniCache != null ? m_UniCache.UniCacheLogger : Logger.DummyLogger;
            Boolean result = false;
            try
            {
                if (m_UniCache.IsOffline)
                {
                    result = ExistsInCache;
                    logger.WriteLine("Offline mode: File '{0}' ({1} in cache) will not be downloaded!", this.ToString(), result ? "exists" : "doesn't exist");
                }
                else
                {
                    if (forceDownload || !ExistsInCache)
                    {
                        logger.WriteLine("Download file '{0}'", ServerFilename);
                        Byte[] fileContent = m_UniCache.SharePointHelper.GetFileTimeout(ServerFilename, Timeout);
                        if (fileContent != null)
                        {
                            logger.WriteLine("Got file content! Writing file '{0}' ...", ServerFilename);
                            File.WriteAllBytes(AbsoluteCachedFilename, fileContent);
                            result = true;
                            logger.WriteLine("UniCacheElement '{0}' loaded!", this.ToString());
                        }
                        else
                        {
                            logger.WriteLine("Didn't get content of file '{0}'!", this.ToString());
                        }
                    }
                    else
                    {
                        logger.WriteLine("File '{0}' already exists in cache (and download not forced)!", this.ToString());
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.WriteException("Exception in LoadIfNotUptodate with forceDownload={0}!", ex, forceDownload);
            }
            if (raiseEvent && m_UniCache != null) m_UniCache.RaiseOnCacheElementLoadedEvent(this, result);
            return result;
        }


        /// <summary>
        /// Determines whether the file corresponding to this UniCacheElement exists the in cache (which is independent of being up-to-date).
        /// </summary>
        /// <value>
        /// <c>true</c> if the file corresponding to this UniCacheElement exists the in cache; otherwise, <c>false</c>.
        /// </value>
        public Boolean ExistsInCache
        {
            get
            {
                String absoluteCacheFilename = AbsoluteCachedFilename;   // thread safety
                return !absoluteCacheFilename.EndsWith(@"\")
                       && File.Exists(absoluteCacheFilename);
            }
        }

        /// <summary>
        /// Increments the usage statistic of this file, if statistic auto increment for this file is set to <c>true</c> (see content-xml)
        /// </summary>
        internal void IncStatistic()
        {
            IncStatistic(false);
        }

        /// <summary>
        /// Increments the usage statistic of this file, if statistic auto increment for this file is set to <c>true</c> (see content-xml) 
        /// or if it is forced by parameter
        /// </summary>
        /// <param name="forceIncStatistic">if set to <c>true</c> usage statistic of this file will be incremented even if auto increment is set to <c>false</c>.</param>
        public void IncStatistic(Boolean forceIncStatistic)
        {
            String logicalFileName = "?";
            try
            {
                if (forceIncStatistic || m_AutoIncStatistic)
                {
                    logicalFileName = this.LogicalFilename;
                    if (!String.IsNullOrEmpty(logicalFileName))
                    {
                        String application = m_UniCache.UniCacheApplicationName;
                        Boolean ok = m_UniCache.SharePointHelper.IncUniCacheStatistic(application , logicalFileName);
                        m_UniCache.UniCacheLogger.WriteAppendTime("{0} incrementing statistic for file '{1}' in {2}", ok ? "Success" : "Problem", logicalFileName, application);
                    }
                }
            }
            catch (Exception ex)
            {
                m_UniCache.UniCacheLogger.WriteException("Error incrementing statistic count '" + logicalFileName + "'", ex);
            }
        }

        /// <summary>
        /// Returns the array of metadata names available for this UniCacheElement
        /// </summary>
        /// <returns></returns>
        public String[] GetMetadataNames()
        {
            String[] result = new String[m_MetadataDict.Keys.Count];
            m_MetadataDict.Keys.CopyTo(result, 0);
            return result;
        }

        /// <summary>
        /// Gets metadata by name
        /// </summary>
        /// <param name="dataname">name of metadata to get</param>
        /// <returns>Returns Metadata-value or null if metadata for dataname is not found.</returns>
        public String GetMetadata(String dataname)
        {
            String value = null;
            m_MetadataDict.TryGetValue(dataname, out value);
            return value;
        }

    }


}
