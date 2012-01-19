using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace NDistribUnit.MSBuild.Extensions
{
    // This file was copied from MsBuildExtensionsPack
    public class AssemblyInfoWrapper
    {
        private readonly Regex attributeBooleanValuePattern = new Regex("\\((?<attributeValue>([tT]rue|[fF]alse))\\)",
                                                                        RegexOptions.Compiled);

        private readonly Dictionary<string, int> attributeIndex = new Dictionary<string, int>();

        private readonly Regex attributeNamePattern = new Regex("[aA]ssembly:?\\s*(?<attributeName>\\w+)\\s*\\(",
                                                                RegexOptions.Compiled);

        private readonly Regex attributeStringValuePattern = new Regex("\"(?<attributeValue>.*?)\"",
                                                                       RegexOptions.Compiled);

        private readonly Regex multilineCSharpCommentEndPattern = new Regex(".*?\\*/", RegexOptions.Compiled);
        private readonly Regex multilineCSharpCommentStartPattern = new Regex("\\s*/\\*^\\*", RegexOptions.Compiled);
        private readonly List<string> rawFileLines = new List<string>();
        private readonly Regex singleLineCSharpCommentPattern = new Regex("\\s*//", RegexOptions.Compiled);
        private readonly Regex singleLineVbCommentPattern = new Regex("\\s*'", RegexOptions.Compiled);

        public AssemblyInfoWrapper(string fileName)
        {
            using (StreamReader streamReader = File.OpenText(fileName))
            {
                int num = 0;
                bool flag = false;
                string text;
                while ((text = streamReader.ReadLine()) != null)
                {
                    rawFileLines.Add(text);
                    if (singleLineCSharpCommentPattern.IsMatch(text) ||
                        singleLineVbCommentPattern.IsMatch(text))
                    {
                        num++;
                    }
                    else
                    {
                        if (multilineCSharpCommentStartPattern.IsMatch(text))
                        {
                            num++;
                            flag = true;
                        }
                        else
                        {
                            if (multilineCSharpCommentEndPattern.IsMatch(text) && flag)
                            {
                                num++;
                                flag = false;
                            }
                            else
                            {
                                if (flag)
                                {
                                    num++;
                                }
                                else
                                {
                                    MatchCollection matchCollection = attributeNamePattern.Matches(text);
                                    if (matchCollection.Count > 0 &&
                                        !attributeIndex.ContainsKey(
                                            matchCollection[0].Groups["attributeName"].Value))
                                    {
                                        attributeIndex.Add(matchCollection[0].Groups["attributeName"].Value, num);
                                    }
                                    num++;
                                }
                            }
                        }
                    }
                }
            }
        }

        public string this[string attribute]
        {
            get
            {
                if (!attributeIndex.ContainsKey(attribute))
                {
                    return null;
                }
                MatchCollection matchCollection =
                    attributeStringValuePattern.Matches(rawFileLines[attributeIndex[attribute]]);
                if (matchCollection.Count > 0)
                {
                    return matchCollection[0].Groups["attributeValue"].Value;
                }
                matchCollection =
                    attributeBooleanValuePattern.Matches(rawFileLines[attributeIndex[attribute]]);
                if (matchCollection.Count > 0)
                {
                    return matchCollection[0].Groups["attributeValue"].Value;
                }
                return null;
            }
            set
            {
                if (!attributeIndex.ContainsKey(attribute))
                {
                    throw new ArgumentOutOfRangeException("attribute",
                                                          string.Format(
                                                              CultureInfo.CurrentUICulture,
                                                              "{0} is not an attribute in the specified AssemblyInfo.cs file",
                                                              new object[]
                                                                  {
                                                                      attribute
                                                                  }));
                }
                MatchCollection matchCollection =
                    attributeStringValuePattern.Matches(rawFileLines[attributeIndex[attribute]]);
                if (matchCollection.Count > 0)
                {
                    rawFileLines[attributeIndex[attribute]] =
                        attributeStringValuePattern.Replace(rawFileLines[attributeIndex[attribute]],
                                                            "\"" + value + "\"");
                    return;
                }
                matchCollection =
                    attributeBooleanValuePattern.Matches(rawFileLines[attributeIndex[attribute]]);
                if (matchCollection.Count > 0)
                {
                    rawFileLines[attributeIndex[attribute]] =
                        attributeBooleanValuePattern.Replace(rawFileLines[attributeIndex[attribute]],
                                                             "(" + value + ")");
                }
            }
        }

        public void Write(TextWriter streamWriter)
        {
            foreach (string current in rawFileLines)
            {
                streamWriter.WriteLine(current);
            }
        }
    }
}