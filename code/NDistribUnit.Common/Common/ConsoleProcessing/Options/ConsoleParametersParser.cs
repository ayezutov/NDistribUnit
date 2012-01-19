using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NDistribUnit.Common.Console.Options;

namespace NDistribUnit.Common.ConsoleProcessing.Options
{
    /// <summary>
    /// Parser, which parses the command line by invoking registered actions
    /// </summary>
    public class ConsoleParametersParser : IEnumerable
    {
        private readonly IList<OptionMatchDescription> optionActions = new List<OptionMatchDescription>();

        /// <summary>
        /// Initializes a new console parameters parser instance
        /// </summary>
        public ConsoleParametersParser()
        {
            ParameterPrefixes = new[] {"-", "/"};
            ParameterDelimiters = new[] {":"};
        }

        private string[] ParameterPrefixes { get; set; }
        private string[] ParameterDelimiters { get; set; }

        /// <summary>
        /// Adds actions for different option names
        /// </summary>
        /// <typeparam name="T">Supposed option value type</typeparam>
        /// <param name="optionName">Option name</param>
        /// <param name="action">Action, which should be performed on match</param>
        /// <param name="isFlag">Indicates, that the option with such a name is a flag option</param>
        public void Add<T>(string optionName, Action<T> action, bool isFlag = false)
        {
            optionActions.Add(new OptionMatchDescription(optionName, action, typeof (T), isFlag));
        }

        /// <summary>
        /// Parser the command line and invokes all registered action for each option name match
        /// </summary>
        /// <param name="commandLine">The command line</param>
        /// <returns>Returns all the options, which were not registered for action</returns>
        public IList<ConsoleOption> Parse(IEnumerable<string> commandLine)
        {
            var availableOptions = GetAllOptions(commandLine);

            foreach (var optionAction in optionActions)
            {
                IEnumerable<ConsoleOption> matchingOptions;
                if ((matchingOptions = FindAllMatchingOptions(availableOptions, optionAction)).Count() > 0)
                {
                    foreach (var matchingOption in matchingOptions)
                    {
                        optionAction.Action.DynamicInvoke(
                            new[]
                                {
                                    Convert.ChangeType(matchingOption.Value,
                                                       optionAction.OptionValueType)
                                });
                        availableOptions.Remove(matchingOption);
                    }
                }
            }

            return availableOptions;
        }

        /// <summary>
        /// Finds all options, which match by name and at the same time can be converted to registered type
        /// </summary>
        /// <param name="availableOptions">List of all available options to search in</param>
        /// <param name="option">Option description to match with</param>
        /// <returns></returns>
        private static IEnumerable<ConsoleOption> FindAllMatchingOptions(IEnumerable<ConsoleOption> availableOptions,
                                                       OptionMatchDescription option)
        {
            return availableOptions.Where(o =>
                                              {
                                                  if (!o.Name.Equals(option.Name))
                                                      return false;
                                                  try
                                                  {
                                                      Convert.ChangeType(o.Value,
                                                                         option.OptionValueType);
                                                  }
                                                  catch (Exception)
                                                  {
                                                      return false;
                                                  }
                                                  return true;
                                              }).ToArray();
        }

        /// <summary>
        /// Parses the 
        /// </summary>
        /// <param name="commandLine"></param>
        /// <returns></returns>
        private IList<ConsoleOption> GetAllOptions(IEnumerable<string> commandLine)
        {
            var result = new List<ConsoleOption>();
            string previousArgumentName = null;

            foreach (var argument in commandLine)
            {
                string prefix = ParameterPrefixes.FirstOrDefault(argument.StartsWith);

                // if previous argument was a switch and current argument is a switch
                // treat previous as a flag
                if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(previousArgumentName))
                {
                    result.Add(new ConsoleOption(previousArgumentName, true.ToString()));
                    previousArgumentName = null;
                }

                // console argument, which follows its name after a whitespace
                if (!string.IsNullOrEmpty(previousArgumentName))
                {
                    result.Add(new ConsoleOption(previousArgumentName, argument));
                    previousArgumentName = null;
                    continue;
                }

                // If the argument starts with prefix it is a named argument or an argument name
                if (!string.IsNullOrEmpty(prefix))
                {
                    string argumentWithoutPrefix = argument.Substring(prefix.Length);
                    var delimiter = ParameterDelimiters.FirstOrDefault(argumentWithoutPrefix.Contains);

                    // Option with delimiter is a /name:value option
                    if (!string.IsNullOrEmpty(delimiter))
                    {
                        string[] parts = argumentWithoutPrefix.Split(ParameterDelimiters, 2,
                                                                     StringSplitOptions.RemoveEmptyEntries);
                        result.Add(new ConsoleOption(parts[0], parts[1]));
                    }
                    else
                    {
                        // Option without delimiter is either a flag (if registered)
                        if (optionActions.FirstOrDefault(o => o.Name.Equals(argumentWithoutPrefix) && o.IsFlag) != null)
                        {
                            result.Add(new ConsoleOption(argumentWithoutPrefix, true.ToString()));
                            continue;
                        }

                        // or just an option name
                        previousArgumentName = argumentWithoutPrefix;
                    }
                }
                    // If the argument doesn't start with prefix it is an unnamed argument
                else
                {
                    result.Add(new ConsoleOption(argument));
                }
            }

            // If we have something left on exit, let's assume it is a flag
            if (!string.IsNullOrEmpty(previousArgumentName))
            {
                result.Add(new ConsoleOption(previousArgumentName, true.ToString()));
            }

            return result;
        }

        /// <summary>
        ///  Is never used as enumerator, but the interface implementation
        ///  is required to support collection initializer syntax
        /// </summary>
        /// <returns>Always throws an exception, if trying to access</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}