using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NDistribUnit.Common.Options
{
    public class ConsoleParametersParser : IEnumerable
    {
        readonly IList<OptionMatchDescription> options = new List<OptionMatchDescription>();
        public const string UnnamedOptionName = "/unnamed/";

        public ConsoleParametersParser()
        {
            ParameterPrefixes = new []{"-", "/"};
            ParameterDelimiters = new[] {":"};
        }

        public string[] ParameterPrefixes { get; set; }
        public string[] ParameterDelimiters { get; set; }

        public void Add<T>(string optionName, Action<T> action, bool isFlag)
        {
            options.Add(new OptionMatchDescription(optionName, action, typeof(T), isFlag));
        }

        public IList<Option> Parse(IEnumerable<string> commandLine)
        {
            var availableOptions = GetAllOptions(commandLine);
            
            foreach (var option in options)
            {
                IEnumerable<Option> matchingOptions;
                if ((matchingOptions = availableOptions.Where(o =>
                                                        {
                                                            if (!o.Name.Equals(option.Name))
                                                                return false;
                                                            try
                                                            {
                                                                Convert.ChangeType(o.Value, option.OptionType);
                                                            }
                                                            catch (Exception)
                                                            {
                                                                return false;
                                                            }
                                                            return true;
                                                        }).ToArray()).Count() > 0)
                {
                    foreach (var matchingOption in matchingOptions)
                    {
                        option.Action.DynamicInvoke(new[] { Convert.ChangeType(matchingOption.Value, option.OptionType) });
                        availableOptions.Remove(matchingOption);
                    }
                }
            }

            return availableOptions;
        }

        private IList<Option> GetAllOptions(IEnumerable<string> commandLine)
        {
            var result = new List<Option>();
            string currentArgumentName = null;

            foreach (var argument in commandLine)
            {
                if (!string.IsNullOrEmpty(currentArgumentName))
                {
                    result.Add(new Option(currentArgumentName, argument));
                    currentArgumentName = null;
                    continue;
                }

                string prefix = ParameterPrefixes.FirstOrDefault(argument.StartsWith);
                if (!string.IsNullOrEmpty(prefix))
                {
                    string argumentWithoutPrefix = argument.Substring(prefix.Length);
                    var delimiter = ParameterDelimiters.FirstOrDefault(argumentWithoutPrefix.Contains);
                    if (!string.IsNullOrEmpty(delimiter))
                    {
                        string[] parts = argumentWithoutPrefix.Split(ParameterDelimiters, 2, StringSplitOptions.RemoveEmptyEntries);
                        result.Add(new Option(parts[0], parts[1]));
                    }
                    else
                    {
                        if (options.FirstOrDefault(o => o.Name.Equals(argumentWithoutPrefix) && o.IsFlag) != null)
                        {
                            result.Add(new Option(argumentWithoutPrefix, true.ToString()));
                            continue;
                        }

                        currentArgumentName = argumentWithoutPrefix;
                    }
                }
                else
                {
                    result.Add(new Option(UnnamedOptionName, argument));
                }
            }

            if (!string.IsNullOrEmpty(currentArgumentName))
            {
                result.Add(new Option(currentArgumentName, true.ToString()));
            }

            return result;
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class Option
    {
        public Option(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class OptionMatchDescription
    {
        public bool IsFlag { get; set; }
        public string Name { get; set; }
        public Delegate Action { get; set; }
        public Type OptionType { get; set; }

        public OptionMatchDescription(string name, Delegate action, Type optionType, bool isFlag)
        {
            IsFlag = isFlag;
            Name = name;
            Action = action;
            OptionType = optionType;
        }
    }
}