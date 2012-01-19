using System.IO;

namespace NDistribUnit.Common.Updating
{
	/// <summary>
	/// 
	/// </summary>
	public interface IUpdateSource
	{
	    /// <summary>
	    /// Gets the zipped version folder.
	    /// </summary>
	    /// <returns></returns>
	    Stream GetZippedVersionFolder();
	}
}