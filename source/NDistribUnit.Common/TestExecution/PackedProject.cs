using System;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class PackedProject
    {
        private readonly string fileName;

        /// <summary>
        /// Gets or sets the packed project.
        /// </summary>
        /// <value>
        /// The packed project.
        /// </value>
        public byte[] Data { get; private set; }
      

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        private string FileName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestExecution.PackedProject"/> class.
        /// </summary>
        /// <param name="data">The packed project.</param>
        public PackedProject(byte[] data): this(null, data)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="TestExecution.PackedProject"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="data">The data.</param>
        public PackedProject(string fileName, byte[] data)
        {
            this.fileName = fileName;
            Data = data;
        }
    }
}