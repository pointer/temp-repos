
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml;

namespace UGIS.de.OfficeComponents.UniCacheLib
{

    /// <summary>
    /// Loads and saves the clients cached content-xml. Using <see cref="UniCachedContentXml.OpenExclusive"/> grants exclusive access 
    /// to the clients content-xml which is used to prevent conflicts while initializing multiple UniCache-instances (in one or more processes).
    /// </summary>
    internal class UniCachedContentXml
    {

        const String EmptyContentXML = "<?xml version='1.0' encoding='utf-8'?><UniCache/>";

        private String m_ApplicationsCachedContentXmlName;
        /// <summary>
        /// Gets the (full) name of the applications cached content XML.
        /// </summary>
        /// <value>
        /// The (full) name of the applications cached content XML.
        /// </value>
        public String ApplicationsCachedContentXmlName
        {
            get { return m_ApplicationsCachedContentXmlName; }
        }

        private Boolean m_IsOffline;
        private UniCache m_UniCache;


        private FileStream m_XmlFileStream;

        private String m_InitialXmlDocOuterXml;
        private Boolean m_InitialXmlDocIsEmpty;
        private Boolean m_ExceptionLoadingXml;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniCachedContentXml"/> class by loading a cached content-xml.
        /// </summary>
        /// <param name="uniCache">The <see cref="UniCache"/>-instance using this <see cref="UniCachedContentXml"/>.</param>
        /// <param name="isOffline">if set to <c>true</c> the offline-content-xml will be used; otherwise, the online-content-xml.</param>
        internal UniCachedContentXml(UniCache uniCache, Boolean isOffline)
        {
            m_ExceptionLoadingXml = false;
            m_IsOffline = isOffline;
            m_UniCache = uniCache;
            String onlineContentXmlName = GetApplicationsCachedContentXmlName(false /*online*/);
            String offlineContentXmlName = GetApplicationsCachedContentXmlName(true /*offline*/);

            m_ApplicationsCachedContentXmlName = isOffline ? offlineContentXmlName : onlineContentXmlName;
            m_InitialXmlDocOuterXml = null;
            m_InitialXmlDocIsEmpty = true;

            if (isOffline && !File.Exists(offlineContentXmlName))
            {
                if (File.Exists(onlineContentXmlName))
                {
                    // Copy Content-Xml to offline-content.xml
                    File.Copy(onlineContentXmlName, offlineContentXmlName);
                }
                else
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(EmptyContentXML);
                    xmlDoc.Save(offlineContentXmlName);
                }
            }
        }

        private String GetApplicationsCachedContentXmlName(Boolean isOffline)
        {
            return Config.GetApplicationsCachedContentXmlName(m_UniCache.ApplicationsCacheTempFolder, isOffline);
        }

        /// <summary>
        /// Gets the last update time of the cached content-xml.
        /// </summary>
        /// <value>
        /// The last update time of the cached content-xml.
        /// </value>
        public DateTime LastUpdateTime
        {
            get
            {
                DateTime result = DateTime.MinValue;
                try
                {
                    FileInfo fileInfo = new FileInfo(m_ApplicationsCachedContentXmlName);
                    if (fileInfo.Exists) result = fileInfo.LastWriteTime;
                }
                catch (Exception ex)
                {
                    m_UniCache.UniCacheLogger.WriteException("Getting LastUpdateTime of content xml ('{0}')", ex, m_ApplicationsCachedContentXmlName);
                }
                return result;
            }
        }

        /// <summary>
        /// Checks if the cached content-xml exists.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the cached content-xml exists; otherwise, <c>false</c>.
        /// </value>
        public Boolean Exists
        {
            get
            {
                Boolean result = false;
                try
                {
                    result = File.Exists(m_ApplicationsCachedContentXmlName);
                }
                catch (Exception ex)
                {
                    m_UniCache.UniCacheLogger.WriteException("Getting Exists-Property of content xml ('{0}')", ex, m_ApplicationsCachedContentXmlName);
                }
                return result;
            }
        }

        /// <summary>
        /// Checks if the cached content-xml has been updated the same day.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is updated the same day; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsUpdatedToday
        {
            get
            {
                return LastUpdateTime.Date == DateTime.Now.Date;
            }
        }

        /// <summary>
        /// Provides the loaded content-xml as XmlDocument.
        /// </summary>
        /// <value>
        /// The loaded content-xml as XmlDocument.
        /// </value>
        internal XmlDocument ContentXml
        {
            get
            {
                XmlDocument resultDoc = null;

                m_UniCache.UniCacheLogger.EnterSection("Get client content-xml");
                try
                {
                    if (m_IsOffline)
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        if (File.Exists(m_ApplicationsCachedContentXmlName))
                        {
                            xmlDoc.Load(m_ApplicationsCachedContentXmlName);
                            resultDoc = xmlDoc;
                        }
                    }
                    else
                    {
                        resultDoc = GetOnlineContentXml();
                        if (resultDoc != null)
                        {
                            m_InitialXmlDocOuterXml = resultDoc.OuterXml;
                            m_InitialXmlDocIsEmpty = !UniCacheElements.HasUniCacheElements(resultDoc);
                        }
                        else
                        {
                            m_InitialXmlDocIsEmpty = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    m_ExceptionLoadingXml = true;
                    m_UniCache.UniCacheLogger.WriteException("Exception in Property 'ContentXml' (get).", ex, m_ApplicationsCachedContentXmlName);
                }
                finally
                {
                    if (resultDoc == null)
                    {
                        resultDoc = MakeContentXml();
                    }
                }
                m_UniCache.UniCacheLogger.ExitSection();

                return resultDoc;
            }
        }

        /// <summary>
        /// Gets the online-content-xml from the server.
        /// </summary>
        /// <returns>The loaded server-content-xml as XmlDocument.</returns>
        private XmlDocument GetOnlineContentXml()
        {
            XmlDocument resultDoc = null;
            if (IsLocked)
            {
                XmlDocument xmlDoc = new XmlDocument();
                m_UniCache.UniCacheLogger.WriteLine("Get client content-xml '{0}'.", m_ApplicationsCachedContentXmlName);
                FileInfo fileInfo = new FileInfo(m_ApplicationsCachedContentXmlName);
                if (fileInfo.Exists && fileInfo.Length > 0)
                {
                    // ContentXml is locked 
                    // -> XmlDoc.load(filename) doesn't work (throws exception)
                    // -> Read file using a FileStream
                    Int64 contentLength = fileInfo.Length;
                    Byte[] buffer = new Byte[contentLength];
                    Int32 offset = 0;
                    Int32 remaining = (Int32)contentLength;
                    while (remaining > 0)
                    {
                        Int32 readBytes = m_XmlFileStream.Read(buffer, offset, remaining);
                        if (readBytes <= 0) throw new EndOfStreamException(String.Format("End of stream reached with {0} bytes left to read", remaining));
                        remaining -= readBytes;
                        offset += readBytes;
                    }

                    // Remove BOM (if needed)
                    String xmlContent;
                    if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                    {
                        xmlContent = Encoding.UTF8.GetString(buffer, 3, (Int32)contentLength - 3);
                    }
                    else
                    {
                        xmlContent = Encoding.UTF8.GetString(buffer);
                    }

                    // Parse ContentXml
                    m_UniCache.UniCacheLogger.WriteLine("Parsing ContentXml ...");
                    xmlDoc.LoadXml(xmlContent);
                    resultDoc = xmlDoc;

                    m_UniCache.UniCacheLogger.WriteLine("ContentXml parsed.");
                }
            }
            return resultDoc;
        }

        /// <summary>
        /// Creates a new content-xml (XmlDocument) containing all files of the current <see cref="UniCache"/>-instance (see <c>m_UniCache</c>).
        /// </summary>
        /// <returns>The content-xml containing all files of the current <see cref="UniCache"/>-instance.</returns>
        internal XmlDocument MakeContentXml()
        {
            XmlDocument result = null;
            m_UniCache.UniCacheLogger.EnterSection("Make content.xml from CacheElements");
            try
            {
                m_UniCache.UniCacheLogger.WriteLine("MakeContentXml");
                XmlDocument contentXml = new XmlDocument();
                contentXml.LoadXml(EmptyContentXML);
                XmlNode rootNode = contentXml.DocumentElement;
                if (m_UniCache.UniCacheElements != null)
                {
                    foreach (UniCacheElement uce in m_UniCache.UniCacheElements)
                    {
                        uce.AppendToXml(rootNode);
                    }
                }
                result = contentXml;
            }
            catch (Exception ex)
            {
                m_UniCache.UniCacheLogger.WriteException("Exception in 'MakeContentXml'!", ex);
            }
            m_UniCache.UniCacheLogger.ExitSection();
            return result;
        }


        /// <summary>
        /// Locks the content-xml by opening the content-xml-file with option 'FileShare.None'. If that fails it will be 
        /// retried until success or timeout (<paramref name="maxWaitTimeMilliSeconds"/>) 
        /// </summary>
        /// <param name="maxWaitTimeMilliSeconds">Milliseconds locking will be retried</param>
        /// <returns><c>true</c>, if locking was successful; otherwise, <c>false</c></returns>
        internal Boolean OpenExclusive(Int32 maxWaitTimeMilliSeconds)
        {
            if (m_IsOffline) throw new ApplicationException("Do not call OpenExclusive() for offline-content.xml!");

            Boolean result = false;
            try
            {
                m_UniCache.UniCacheLogger.WriteLine("Opening content-xml exclusively ...");
                FileStream fs = null;
                if (m_XmlFileStream == null)
                {
                    try
                    {
                        Int32 sleepInterval = Math.Min(maxWaitTimeMilliSeconds, 800);
                        Int64 startTimeTicks = DateTime.Now.Ticks;
                        while (fs == null
                               && TimeSpan.FromTicks(DateTime.Now.Ticks - startTimeTicks).TotalMilliseconds < maxWaitTimeMilliSeconds)
                        {
                            try
                            {
                                fs = new FileStream(m_ApplicationsCachedContentXmlName,
                                                    FileMode.OpenOrCreate, FileAccess.ReadWrite,
                                                    FileShare.None);
                                m_UniCache.UniCacheLogger.WriteLineAppendTime("Content-xml opend!");
                            }
                            catch (Exception ex) 
                            {
                                System.Diagnostics.Debug.WriteLine("Exception in 'OpenExclusive':" + ex.Message);
                                m_UniCache.UniCacheLogger.WriteLineAppendTime("Could not open content-xml.");
                            };
                            if (fs == null) Thread.Sleep(sleepInterval);
                        }
                    }
                    catch (Exception ex)
                    {
                        m_UniCache.UniCacheLogger.WriteException("Exception in 'OpenExclusive' (inner).", ex);
                    }
                    m_XmlFileStream = fs;
                }
                result = m_XmlFileStream != null;
            }
            catch (Exception ex)
            {
                m_UniCache.UniCacheLogger.WriteException("Exception in 'OpenExclusive'.", ex);
            }
            return result;
        }

        /// <summary>
        /// Writes the current xml (if UniCache is in online mode and the xml has been changed) and then unlocks (closes) the content-xml. 
        /// </summary>
        internal void CloseAndWrite(Boolean writeUnchangedXml)
        {
            Close(m_IsOffline || m_UniCache.IsOffline, writeUnchangedXml);
        }

        /// <summary>
        /// Unlocks (closes) the content-xml without writing changes. 
        /// </summary>
        internal void CloseAndDiscardChanges()
        {
            Close(true, false);
        }

        private void Close(Boolean discardChanges, Boolean writeUnchangedXml)
        {
            m_UniCache.UniCacheLogger.EnterSection("Close content xml (discardChanges={0}, writeUnchangedXml={1})", discardChanges, writeUnchangedXml);
            try
            {
                if (!m_IsOffline && m_XmlFileStream != null)
                {
                    try
                    {
                        if (m_ExceptionLoadingXml)
                        {
                            m_UniCache.UniCacheLogger.WriteLineAppendTime("Content.xml is not written because there has been an ecxeption loading it!");
                        }
                        else
                        {
                            XmlDocument newXmlDoc = MakeContentXml();
                            Int64 currentFileLength = m_XmlFileStream.Length;
                            Boolean definedXml = (newXmlDoc != null);
                            Boolean isChanged = m_InitialXmlDocOuterXml != newXmlDoc.OuterXml;
                            Boolean newXmlIsDefinedAndShouldBeWritten = definedXml && (writeUnchangedXml || isChanged);
                            XmlDocument beautifiedContentXml = BeautifyXmlDoc(newXmlDoc);
                            if ((!discardChanges || m_InitialXmlDocIsEmpty) && newXmlIsDefinedAndShouldBeWritten)
                            {
                                m_UniCache.UniCacheLogger.WriteLineAppendTime("Writing content.xml ... (Discard changes={0}, Empty file={1}, Undefined xml={2}, Write unchanged xml={3}, Is changed={4})",
                                                                               discardChanges, m_InitialXmlDocIsEmpty, !definedXml, writeUnchangedXml, isChanged);

                                Byte[] buffer = Encoding.UTF8.GetBytes(beautifiedContentXml.OuterXml);
                                m_XmlFileStream.Position = 0;
                                m_XmlFileStream.Write(buffer, 0, buffer.Length);

                                OverwriteRestOfOldXml(currentFileLength);

                                m_UniCache.UniCacheLogger.WriteLineAppendTime("Wrote content.xml!");

                                SaveOfflineContentXml(beautifiedContentXml, true);
                            }
                            else
                            {
                                m_UniCache.UniCacheLogger.WriteLineAppendTime("No need to write content.xml (Discard changes={0}, Empty file={1}, Undefined xml={2}, Write unchanged xml={3}, Is changed={4})",
                                                                               discardChanges, m_InitialXmlDocIsEmpty, !definedXml, writeUnchangedXml, isChanged);
                                SaveOfflineContentXml(beautifiedContentXml, false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        m_UniCache.UniCacheLogger.WriteException("Exception in 'Close' writing content-xml.", ex);
                    }
                    finally
                    {
                        m_XmlFileStream.Close();
                    }
                }

                m_XmlFileStream = null;
            }
            catch (Exception ex)
            {
                m_UniCache.UniCacheLogger.WriteException("Exception in 'Close'.", ex);
            }
            m_UniCache.UniCacheLogger.ExitSection();
        }

        private void OverwriteRestOfOldXml(Int64 currentFileLength)
        {
            //Old file has been longer than current xml -> overwrite with blanks
            while (m_XmlFileStream.Position < currentFileLength)
            {
                m_XmlFileStream.WriteByte((Byte)0x20);
            }
        }

        private void SaveOfflineContentXml(XmlDocument contentXml, Boolean force)
        {
            try {
                String offlineCachedContentXmlName = GetApplicationsCachedContentXmlName(true);
                Boolean safeOfflineContentXml = true;
                if (!force)
                {
                    try
                    {
                        XmlDocument offlineXmlDoc = new XmlDocument();
                        offlineXmlDoc.Load(offlineCachedContentXmlName);
                        safeOfflineContentXml = offlineXmlDoc.OuterXml != contentXml.OuterXml;
                    }
                    catch (Exception ex)
                    {
                        m_UniCache.UniCacheLogger.WriteException("Error loading/parsing offline-content.xml -> set offline-content.xml!", ex);
                    }
                }

                if (safeOfflineContentXml)
                {
                    contentXml.Save(offlineCachedContentXmlName);
                }
            }
            catch (Exception ex)
            {
                m_UniCache.UniCacheLogger.WriteException("Error saving offline-content.xml!", ex);
            }
        }


        /// <summary>
        /// Gets a value indicating whether this instance is locked.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is locked; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsLocked
        {
            get { return m_XmlFileStream != null; }
        }

        /// <summary>
        /// Returns a XmlDocument with same content but inserted tabs and line breaks 
        /// </summary>
        /// <param name="doc">The XmlDocument to be beautified.</param>
        /// <returns>The beautified XmlDocument.</returns>
        private XmlDocument BeautifyXmlDoc(XmlDocument doc)
        {
            XmlDocument resultDoc = doc;
            try
            {
                XmlDocument tmpXmlDoc = new XmlDocument();
                tmpXmlDoc.LoadXml(doc.OuterXml);
                InsertWhiteSpaceNodeAndTraverse(tmpXmlDoc.DocumentElement, 0);
                tmpXmlDoc.InsertAfter(tmpXmlDoc.CreateWhitespace(Environment.NewLine), tmpXmlDoc.FirstChild);
                resultDoc = tmpXmlDoc;
            }
            catch (Exception ex)
            {
                m_UniCache.UniCacheLogger.WriteException("Error beautifying xml -> unbeautified xml will be written", ex);
            }
            return resultDoc;
        }

        private static void InsertWhiteSpaceNodeAndTraverse (XmlNode node, Int32 level) 
        {
            if (node != null)
            {
                XmlNode prevSibling = node.PreviousSibling;
                XmlNode nextSibling = node.NextSibling;
                Boolean nodeIsElement = (node is XmlElement);
                Boolean prevNodeIsElement = prevSibling == null || (prevSibling is XmlElement);

                String whiteSpaceChar = String.Empty;
                whiteSpaceChar = Environment.NewLine + whiteSpaceChar.PadLeft(level, '\t');

                if (nodeIsElement && prevNodeIsElement)
                {
                    node.ParentNode.InsertBefore(node.OwnerDocument.CreateWhitespace(whiteSpaceChar), node);
                }

                if (node.HasChildNodes)
                {
                    InsertWhiteSpaceNodeAndTraverse(node.FirstChild, level + 1);
                    if (node.LastChild is XmlElement)
                    {
                        node.InsertAfter(node.OwnerDocument.CreateWhitespace(whiteSpaceChar), node.LastChild);
                    }
                }

                if (nextSibling != null)
                {
                    InsertWhiteSpaceNodeAndTraverse(nextSibling, level);
                }
            }
        }

    }
}
