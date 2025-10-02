using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace UGIS.de.OfficeComponents.UniCacheLib
{

    /// <summary>
    /// Dictionary of UniCacheElement-instances which is indexed by logical name
    /// </summary>
    internal class UniCacheElements : IEnumerable<UniCacheElement>
    {

        /// <summary>
        /// Determines whether a given content-xml contains UniCacheElement definitions.
        /// </summary>
        /// <param name="contentXml">The content-xml.</param>
        /// <returns><c>true</c> if UniCaches content-xml contains UniCacheElement definitions; otherwise, <c>false</c>.</returns>
        public static Boolean HasUniCacheElements(XmlDocument contentXml)
        {
            Boolean result = false;
            foreach (UniCacheElement uce in GetUniCacheElementNodes(contentXml, null))
            {
                result = true;
                break;
            }
            return result;
        }

        /// <summary>
        /// Enumerates and creates <see cref="UniCacheElement"/>-instances of a given content-xml and migrates cached files to new filenames if needed
        /// </summary>
        /// <param name="contentXml">The content-xml.</param>
        /// <param name="uniCache">The UniCache.</param>
        /// <returns>IEnumerable of UniCacheElement-instances</returns>
        private static IEnumerable<UniCacheElement> GetUniCacheElementNodes(XmlDocument contentXml, UniCache uniCache)
        {
            Logger logger = (uniCache != null ? uniCache.UniCacheLogger : Logger.DummyLogger);
            foreach (XmlNode node in contentXml.DocumentElement.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Comment)
                {
                    UniCacheElement uce = null;
                    try
                    {
                        uce = new UniCacheElement(node, uniCache, logger); // Migrates to new filenames if needed
                    }
                    catch (Exception ex)
                    {
                        logger.WriteException("creating UniCacheElement from content xml node: {0}", ex, node.OuterXml);
                    }
                    if (uce != null)
                    {
                        yield return uce;
                    }
                }
            }
        }



        private Dictionary<String, UniCacheElement> m_UniCacheElements;
        private UniCache m_UniCache;
        private Boolean m_UniCacheSeemsCorrupt;
        internal Boolean UniCacheSeemsCorrupt
        {
            get { return m_UniCacheSeemsCorrupt; }
        }


        /// <summary>
        /// Returns an enumerator that iterates through the collection of <see cref="UniCacheElement"/>-instances of field <c>m_UniCacheElements</c>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection of <see cref="UniCacheElement"/>-instances of field <c>m_UniCacheElements</c>.
        /// </returns>
        public IEnumerator<UniCacheElement> GetEnumerator()
        {
            return m_UniCacheElements.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection of <see cref="UniCacheElement"/>-instances of field <c>m_UniCacheElements</c>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection of <see cref="UniCacheElement"/>-instances of field <c>m_UniCacheElements</c>.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_UniCacheElements.Values.GetEnumerator();
        }

        /// <summary>
        /// Loads a given content-xml for a given <see cref="UniCache"/> and adds <see cref="UniCacheElement"/>-instances.
        /// </summary>
        /// <remarks>If will migrate to new filenames if needed</remarks>
        public UniCacheElements(UniCache uniCache, XmlDocument contentXml, String xmlSource)
        {
            m_UniCacheSeemsCorrupt = false;
            // Get Server Content
            uniCache.UniCacheLogger.EnterSection("Creating UniCacheElements for {0} ", xmlSource);
            try
            {
                m_UniCache = uniCache;
                m_UniCacheElements = new Dictionary<String, UniCacheElement>(20);

                if (contentXml != null)
                {
                    Boolean allElementsExist = true;
                    Boolean noElementExists = true;
                    foreach (UniCacheElement uce in GetUniCacheElementNodes(contentXml, uniCache))
                    {
                        try
                        {
                            noElementExists  = false;
                            allElementsExist = allElementsExist && uce.ExistsInCache;
                            AddCacheElement(uce);
                        }
                        catch (Exception ex)
                        {
                            uniCache.UniCacheLogger.WriteException("Exception appending UniCacheElement for node '{0}'!", ex, uce.LogicalFilename);
                        }
                    }
                    m_UniCacheSeemsCorrupt = noElementExists || !allElementsExist;
                }
            }
            catch (Exception ex)
            {
                uniCache.UniCacheLogger.WriteException("Exception in UniCacheElements-constructor!", ex);
                throw;
            }
            finally
            {
                uniCache.UniCacheLogger.ExitSection();
            }
        }

        /// <summary>
        /// Tries to get the <see cref="UniCacheElement"/> with the given logical filename.
        /// </summary>
        /// <param name="logicalFilename">The logical filename.</param>
        /// <param name="uniCacheElement">When this method returns, <paramref name="uniCacheElement"/> will contain the <see cref="UniCacheElement"/> with 
        /// the logical filename <paramref name="logicalFilename"/>, if the logical filename is found; otherwise, <c>null</c>.</param>
        /// <returns>
        /// <c>true</c> if a <see cref="UniCacheElement"/> with logical filename <paramref name="logicalFilename"/> is found; otherwise, <c>false</c>.</returns>
        internal Boolean TryGetValue(String logicalFilename, out UniCacheElement uniCacheElement)
        {
            return m_UniCacheElements.TryGetValue(logicalFilename, out uniCacheElement);
        }

        /// <summary>
        /// Tries to get the <see cref="UniCacheElement" /> with the given filename (of the cached file).
        /// </summary>
        /// <param name="filename">The filename of the cached file.</param>
        /// <param name="uniCacheElement">When this method returns, <paramref name="uniCacheElement" /> will contain the <see cref="UniCacheElement" /> with
        /// the logical filename <paramref name="filename" />, if the logical filename is found; otherwise, <c>null</c>.</param>
        /// <returns>
        ///   <c>true</c> if a <see cref="UniCacheElement" /> with logical filename <paramref name="filename" /> is found; otherwise, <c>false</c>.
        /// </returns>
        internal Boolean TryGetValue(Uri filename, out UniCacheElement uniCacheElement)
        {
            Boolean result = false;
            uniCacheElement = null;
            foreach (UniCacheElement uce in m_UniCacheElements.Values)
            {
                if (new Uri(uce.AbsoluteCachedFilename).Equals(filename))
                {
                    uniCacheElement = uce;
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Adds the passed <see cref="UniCacheElement"/>-instance to the dictionary <see cref="m_UniCacheElements"/> (if not already contained).
        /// </summary>
        /// <param name="uce">The <see cref="UniCacheElement"/>-instance to add.</param>
        private void AddCacheElement(UniCacheElement uce)
        {
            if (!m_UniCacheElements.ContainsKey(uce.LogicalFilename))
            {
                m_UniCacheElements.Add(uce.LogicalFilename, uce);
                m_UniCache.UniCacheLogger.WriteLine("Add UniCacheElement '{0}'", uce);
            }
        }

    }

}
