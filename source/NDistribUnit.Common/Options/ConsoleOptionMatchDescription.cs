using System;

namespace NDistribUnit.Common.Options
{
    /// <summary>
    /// Describes an action, which should be performed on a particular match
    /// </summary>
    internal class OptionMatchDescription
    {
        /// <summary>
        /// Specifies, that the described console option is a simple flag
        /// </summary>
        public bool IsFlag { get; private set; }

        /// <summary>
        /// The name of the console option
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The action which should be performed on match
        /// </summary>
        public Delegate Action { get; private set; }

        /// <summary>
        /// The supposed type of option value
        /// </summary>
        public Type OptionValueType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the class
        /// </summary>
        /// <param name="name">The name of the console option</param>
        /// <param name="action">The action which should be performed on match</param>
        /// <param name="optionValueType">The supposed type of option value</param>
        /// <param name="isFlag">Specifies, that the described console option is a simple flag</param>
        public OptionMatchDescription(string name, Delegate action, Type optionValueType, bool isFlag)
        {
            IsFlag = isFlag;
            Name = name;
            Action = action;
            OptionValueType = optionValueType;
        }
    }
}