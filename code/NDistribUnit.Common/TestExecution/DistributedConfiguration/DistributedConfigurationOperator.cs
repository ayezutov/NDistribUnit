using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using System.Xml.XPath;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.TestExecution.Storage;
using NUnit.Util;

namespace NDistribUnit.Common.TestExecution.DistributedConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    public class DistributedConfigurationOperator : IDistributedConfigurationOperator
    {
        static readonly Regex TypeIdentifier = new Regex(@"^\s*(?:NDistribUnit\.)(?<selector>\w+)\s*(?<object>{.+})\s*$", RegexOptions.Compiled);
        static readonly Regex Variable = new Regex(@"(?<!\$)(?:\${)(?<varName>\w+?)(?:})", RegexOptions.Compiled);
        static readonly Regex Escaping = new Regex(@"\$(?=\${\w+?})", RegexOptions.Compiled);

        /// <summary>
        /// Reads the configuration setup.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="nUnitParameters">The n unit parameters.</param>
        /// <returns></returns>
        public DistributedConfigurationSetup ReadConfigurationSetup(TestProject project, NUnitParameters nUnitParameters)
        {
            var result = new DistributedConfigurationSetup();

            var distributedConfigurationFileNames = GetDistributedConfigurationFileNames(project, nUnitParameters);

            foreach (var distributedConfigurationFileName in distributedConfigurationFileNames)
            {
                if (string.IsNullOrEmpty(distributedConfigurationFileName))
                    continue;
                FillSetupInstance(distributedConfigurationFileName, result);
            }
            return result;
        }

        /// <summary>
        /// Gets the substituted configuration file.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="nUnitParameters">The n unit parameters.</param>
        /// <param name="configurationSubstitutions">The configuration substitutions.</param>
        /// <returns></returns>
        public string GetSubstitutedConfigurationFile(TestProject project, NUnitParameters nUnitParameters, DistributedConfigurationSubstitutions configurationSubstitutions)
        {
            var distributedConfigurationFileNames = GetDistributedConfigurationFileNames(project, nUnitParameters);

            if (configurationSubstitutions == null || configurationSubstitutions.Variables.Count == 0)
                return distributedConfigurationFileNames.FirstOrDefault();

            string finalConfigurationFile = null;
            int hashCode = configurationSubstitutions.GetHashCode();
            foreach (var configurationFileName in distributedConfigurationFileNames)
            {
                var configurationDocument = XDocument.Load(configurationFileName);
                SubstitutePlaceHolders(configurationDocument, configurationSubstitutions);
                var substitutedFileName = Path.ChangeExtension(configurationFileName,
                                     string.Format(".{0}.config", hashCode));
                configurationDocument.Save(substitutedFileName);

                if (finalConfigurationFile == null)
                    finalConfigurationFile = substitutedFileName;
            }

            return finalConfigurationFile;
        }

        internal void SubstitutePlaceHolders(XDocument doc, DistributedConfigurationSubstitutions configurationSubstitutions)
        {
            var descendantComments = doc.DescendantNodes().OfType<XComment>();

            var comments = (from comment in descendantComments
                            let match = TypeIdentifier.Match(comment.Value)
                            where match.Success
                            select new { XComment = comment, Match = match }).ToList();

            foreach (var comment in comments)
            {
                if (!comment.Match.Groups["selector"].Value.Equals("Replace"))
                    continue;

                var parameters = new JavaScriptSerializer().DeserializeObject(comment.Match.Groups["object"].Value) as Dictionary<string, object>;
                var xPath = parameters["XPath"] as string;
                var value = parameters["Value"] as string;

                var node = (comment.XComment.XPathEvaluate(xPath) as IEnumerable<object>).FirstOrDefault();
                if (node is XElement)
                    ((XElement)node).SetValue(value);
                else if (node is XAttribute)
                    ((XAttribute)node).SetValue(value);
                comment.XComment.Remove();
            }

            var xmlAsString = doc.ToString(); // not efficient but clear and ok for usually small configs
            xmlAsString = Variable.Replace(xmlAsString, match =>
                                              {
                                                  var variableName = match.Groups["varName"].Value;
                                                  var variable = configurationSubstitutions.Variables
                                                      .FirstOrDefault(v => v.Name.Equals(variableName));

                                                  if (variable == null)
                                                     return match.Value;

                                                  return variable.Value;
                                              });
            xmlAsString = Escaping.Replace(xmlAsString, string.Empty);
            var doc2 = XDocument.Parse(xmlAsString);
            doc.ReplaceNodes(doc2.Nodes());
        }

        private void FillSetupInstance(string distributedConfigName, DistributedConfigurationSetup result)
        {
            var doc = XDocument.Load(distributedConfigName);
            FillSetupInstance(doc, result);
        }

        internal void FillSetupInstance(XDocument doc, DistributedConfigurationSetup result)
        {
            
            var nDistribUnitComments = doc.Nodes().OfType<XComment>();

            foreach (var comment in nDistribUnitComments)
            {
                var match = TypeIdentifier.Match(comment.Value);
                if (!match.Success)
                    continue;

                try
                {
                    var selector = match.Groups["selector"].Value;
                    var jsonObject = new JavaScriptSerializer().DeserializeObject(match.Groups["object"].Value) as Dictionary<string, object>;

                    if (jsonObject == null)
                        continue;

                    if ("Variable".Equals(selector))
                    {
                        var name = jsonObject["Name"] as string;
                        var properties = jsonObject["TypeArguments"] as Dictionary<string, object>;

                        DistributedConfigurationVariable variable = null;
                        if ("Sequence".Equals(jsonObject["Type"]))
                        {
                            variable = new SequenceDistributedConfigurationVariable(name);
                        }

                        if (variable != null)
                        {
                            InitializePropertiesFromJson(variable, properties);
                            result.Variables.Add(variable);
                        }
                    }
                }
                catch
                {
                    // Just ignore any errors while configuration reading
                    continue;
                }
            }
        }

        private void InitializePropertiesFromJson(object variable, Dictionary<string, object> properties)
        {
            if (properties == null || properties.Count == 0 || variable == null)
                return;

            var variableType = variable.GetType();
            foreach (var property in properties)
            {
                var variableProperty = variableType.GetProperty(property.Key);
                if (variableProperty == null)
                    continue;

                object convertedValue;
                try
                {
                    convertedValue = Convert.ChangeType(property.Value, variableProperty.PropertyType);
                }
                catch
                {
                    // There are too many specific exceptions to catch them by one
                    continue;
                }
                variableProperty.SetValue(variable, convertedValue, null);
            }
        }

        private IEnumerable<string> GetDistributedConfigurationFileNames(TestProject project, NUnitParameters nUnitParameters)
        {
            if (nUnitParameters.AssembliesToTest.Count == 1)
            {
                var localName = Path.Combine(project.Path, Path.GetFileName(nUnitParameters.AssembliesToTest[0]));
                if (NUnitProject.IsNUnitProjectFile(localName) && 
                    File.Exists(localName))
                {
                    var nUnitProject = new NUnitProject(localName);
                    nUnitProject.Load();
                    var matchingConfig = nUnitProject.Configs.Cast<ProjectConfig>().Where(c => c.Name.Equals(nUnitParameters.Configuration)).FirstOrDefault();

                    if (matchingConfig == null)
                    {
                        var configFileNamedAsProject = Path.ChangeExtension(localName, ".config");
                        if (File.Exists(configFileNamedAsProject))
                            yield return configFileNamedAsProject;
                        yield break;
                    }

                    yield return matchingConfig.ConfigurationFilePath;
                    yield break;
                }
            }

            foreach (var assembly in nUnitParameters.AssembliesToTest)
            {
                var localName = Path.Combine(project.Path, Path.GetFileName(assembly));
                string possibleConfigName = null;
                if (File.Exists(possibleConfigName = localName + ".config"))
                    yield return possibleConfigName;
                else if (File.Exists(possibleConfigName = Path.ChangeExtension(localName, ".config")))
                    yield return possibleConfigName;
            }
        }
    }
}