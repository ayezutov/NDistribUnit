using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NDistribUnit.Common.Common.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ObjectExtensions
    {
        static readonly BinaryFormatter formatter = new BinaryFormatter();
        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public static T DeepClone<T>(this T result)
        {
            var ms = new MemoryStream();
            formatter.Serialize(ms, result);
            ms.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(ms);
        }
    }
}