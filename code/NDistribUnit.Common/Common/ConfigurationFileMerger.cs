using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using System.Linq;

namespace NDistribUnit.Common.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class ConfigurationFileMerger
    {
        /// <summary>
        /// Merges the files.
        /// </summary>
        /// <param name="base">The @base.</param>
        /// <param name="part">The part.</param>
        /// <returns></returns>
        public string MergeFiles(string @base, string part)
        {
            if (@base == null)
                return null;

            if (part == null)
                return @base;

            var resultingDocument = MergeFilesToString(@base, part);

            var mergedFileName = InAnotherDomainConfigurationMerger.GetMergedFileName(@base);

            if (File.Exists(mergedFileName))
                File.Delete(mergedFileName);

            using (var file = new FileStream(mergedFileName, FileMode.Create))
            {
                using(var fileStream = new StreamWriter(file))
                {
                    fileStream.Write(resultingDocument);
                }
            }

            return mergedFileName;
        }

        internal string MergeFilesToString(string @base, string part)
        {
            var basePath = Path.GetDirectoryName(@base);

            var domain = AppDomain.CreateDomain("Configuration.File.Merger",
                                                AppDomain.CurrentDomain.Evidence,
                                                new AppDomainSetup
                                                    {
                                                        ApplicationBase = basePath,
                                                        PrivateBinPath = Path.GetDirectoryName(part),
                                                        ConfigurationFile = @base,
                                                    });
            try
            {
                var inAnotherDomainMerger =
                    (InAnotherDomainConfigurationMerger)
                    domain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location,
                                                       typeof (InAnotherDomainConfigurationMerger).FullName,
                                                       false, BindingFlags.Default, null, null,
                                                       Thread.CurrentThread.CurrentCulture, null);

                var resultingDocument = inAnotherDomainMerger.Merge(@base, part);
                return resultingDocument;
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class InAnotherDomainConfigurationMerger: MarshalByRefObject
    {
        /// <summary>
        /// Gets the name of the merged file.
        /// </summary>
        /// <param name="baseName">Name of the base.</param>
        /// <returns></returns>
        public static string GetMergedFileName(string baseName)
        {
            return Path.ChangeExtension(baseName, ".merged.config");
        }

        /// <summary>
        /// Merges the specified @base.
        /// </summary>
        /// <param name="base">The @base.</param>
        /// <param name="part">The part.</param>
        /// <returns></returns>
        public string Merge(string @base, string part)
        {
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
                                                                                    {
                                                                                        ExeConfigFilename = @base
                                                                                    }, ConfigurationUserLevel.None);
            var docPart = XDocument.Load(part);
            var docBase = XDocument.Load(@base);

            var docResult = Merge(docBase, docPart, configuration);
            return docResult.ToString();
        }

        private XDocument Merge(XDocument docBase, XDocument docPart, Configuration configuration)
        {
            if (docBase.Root.Name.LocalName != docPart.Root.Name.LocalName)
                return docBase;

            var merged = new XDocument(docBase);

            foreach (var partElement in docPart.Root.Nodes().OfType<XElement>())
            {
                var mergedElement = merged.Root.Element(partElement.Name);
                if (mergedElement == null)
                {
                    merged.Root.Add(partElement);
                    continue;
                }
                ConfigurationSection configSection = null;
                try
                {
                    configSection = configuration.GetSection(partElement.Name.LocalName);
                }
                catch (Exception)
                {
                    // do nothing, just merge node as XML
                }

                if (configSection != null)
                    MergeBasedOnConfig(mergedElement, partElement, configSection);
                else
                    MergeAsRawXml(mergedElement, partElement);
            }

            return merged;
        }

        private void MergeBasedOnConfig(XElement mergedElement, XElement partElement, ConfigurationElement configElement)
        {
            var defaultCollectionProperty = configElement.GetType()
                .GetProperties()
                .FirstOrDefault(p =>
                                    {
                                        var attr =
                                            (ConfigurationPropertyAttribute)
                                            Attribute.GetCustomAttribute(p, typeof (ConfigurationPropertyAttribute));
                                        return attr!=null && attr.IsDefaultCollection;
                                    });
            if (defaultCollectionProperty != null)
            {
                var configCollectionAttribute = (ConfigurationCollectionAttribute)Attribute.GetCustomAttribute(defaultCollectionProperty.PropertyType,
                                                                                                            typeof (ConfigurationCollectionAttribute));
                if (configCollectionAttribute != null)
                {
                    var keyProperty = configCollectionAttribute.ItemType.GetProperties()
                        .Select(p => new
                                         {
                                             Property = p,
                                             Attribute = (ConfigurationPropertyAttribute)
                                                         Attribute.GetCustomAttribute(p,
                                                                                      typeof (
                                                                                          ConfigurationPropertyAttribute
                                                                                          ))
                                         })
                        .FirstOrDefault(p => p.Attribute != null && p.Attribute.IsKey);

                    if (keyProperty != null)
                    {
                        MergeAttributes(mergedElement, partElement);

                        var mergedElements = mergedElement.Nodes().OfType<XElement>().ToList();

                        foreach (var partChildElement in partElement.Nodes().OfType<XElement>())
                        {
                            var key = partChildElement.Attributes()
                                .FirstOrDefault(
                                    a =>
                                    a.Name.LocalName.Equals(keyProperty.Attribute.Name,
                                                            StringComparison.OrdinalIgnoreCase));
                            if (key == null)
                                continue;

                            var mergedChildElement = mergedElements
                                .FirstOrDefault(e =>
                                                    {
                                                        var keyAttribute = e.Attribute(key.Name);
                                                        return keyAttribute != null && key.Value.Equals(keyAttribute.Value);
                                                    });
                            
                            if (mergedChildElement != null)
                                mergedElements.Remove(mergedChildElement);
                            mergedElements.Add(partChildElement);
                        }

                        mergedElement.ReplaceNodes(mergedElements);
                        return;
                    }
                }
            }

            // TODO: actually, we should dig into each ConfigurationElement, but we'll skip it for now
            MergeAsRawXml(mergedElement, partElement);
        }

        private void MergeAsRawXml(XElement mergedElement, XElement partElement)
        {
            if (partElement.HasAttributes)
                MergeAttributes(mergedElement, partElement);

            var elements = mergedElement.Nodes().ToList();
            var partNodes = partElement.Nodes();

            foreach (var partNode in partNodes)
            {
                if (partNode is XComment)
                {
                    elements.Add(partNode);
                    continue;
                }
                if (partNode is XText)
                {
                    elements.Clear();
                    elements.Add(partNode);
                    return;
                }

                if (partNode is XElement)
                {
                    var partChildElement= (XElement)partNode;
                    var correspondingMergedElements =
                        elements.Where(e => e is XElement && ((XElement) e).Name.Equals(partChildElement.Name)).ToList();

                    if (correspondingMergedElements.Count != 1)
                        elements.Add(partChildElement);
                    else
                        MergeAsRawXml((XElement) correspondingMergedElements[0], partChildElement);
                }
            }

            mergedElement.ReplaceNodes(elements);
        }

        private void MergeAttributes(XElement mergedElement, XElement partElement)
        {
            var mergedAttributes = mergedElement.Attributes().ToList();
            foreach (var partAttribute in partElement.Attributes())
            {
                XAttribute mergedAttribute = mergedAttributes.FirstOrDefault(a => a.Name.Equals(partAttribute.Name));
                if (mergedAttribute != null)
                    mergedAttributes.Remove(mergedAttribute);
                mergedAttributes.Add(partAttribute);
            }
            mergedElement.RemoveAttributes();
            mergedElement.Add(mergedAttributes);
        }
    }
}