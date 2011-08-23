using System;

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
		/// <param name="version">The version.</param>
		/// <returns></returns>
		byte[] GetZippedVersionFolder(Version version);
	}
}