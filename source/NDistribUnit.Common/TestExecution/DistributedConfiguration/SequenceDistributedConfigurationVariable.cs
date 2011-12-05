namespace NDistribUnit.Common.TestExecution.DistributedConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    public class SequenceDistributedConfigurationVariable : DistributedConfigurationVariable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceDistributedConfigurationVariable"/> class.
        /// </summary>
        /// <param name="name"></param>
        public SequenceDistributedConfigurationVariable(string name): base(name)
        {
            Start = 1;
            Step = 1;
            Maximum = int.MaxValue;
        }

        /// <summary>
        /// Gets or sets the start.
        /// </summary>
        /// <value>
        /// The start.
        /// </value>
        public int Start { get; set; }

        /// <summary>
        /// Gets or sets the step.
        /// </summary>
        /// <value>
        /// The step.
        /// </value>
        public int Step { get; set; }

        /// <summary>
        /// Gets or sets the end.
        /// </summary>
        /// <value>
        /// The end.
        /// </value>
        public int Maximum { get; set; }

        private int? value;

        /// <summary>
        /// Gets the next value.
        /// </summary>
        /// <returns></returns>
        public override string GetNextValue()
        {
            lock (this)
            {
                if (!value.HasValue)
                {
                    value = Start;
                    return value.ToString();
                }
                return (value = (value + Step)).ToString();
            }
        }
    }
}