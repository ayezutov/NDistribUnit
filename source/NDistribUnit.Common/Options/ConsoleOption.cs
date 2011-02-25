namespace NDistribUnit.Common.ConsoleProcessing.Options
{
    /// <summary>
    /// Describes a single parsed console option
    /// </summary>
    public class ConsoleOption
    {
        /// <summary>
        /// Initializes a new instance of an unnamed option
        /// </summary>
        /// <param name="value">The value for an unnamed option</param>
        public ConsoleOption(string value): this(UnnamedOptionName, value)
        {}

        /// <summary>
        /// Initializes a new instance of an option with name and value
        /// </summary>
        /// <param name="name">The name of the option</param>
        /// <param name="value">The value of the option</param>
        public ConsoleOption(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// The name of the console option:
        /// "test" for "/test:testValue"
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The name of the console option:
        /// "testValue" for "/test:testValue"
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// The default name for an option without name.
        /// As far as it contains quotes and whitespaces it should not match
        /// any real-life name
        /// </summary>
        public const string UnnamedOptionName = @"""not named""";
    }
}