namespace NDistribUnit.Common.TestExecution.Storage
{
    /// <summary>
    /// Represents an unpacked project
    /// </summary>
    public class TestProject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestProject"/> class.
        /// </summary>
        /// <param name="path">The root path.</param>
        public TestProject(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Gets the path.
        /// </summary>
        public string Path { get; private set; }
    }
}