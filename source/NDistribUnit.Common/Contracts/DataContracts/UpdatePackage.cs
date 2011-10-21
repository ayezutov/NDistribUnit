using System;

namespace NDistribUnit.Common.ServiceContracts
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class UpdatePackage
	{
		/// <summary>
		/// Gets or sets a value indicating whether this instance is available.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is available; otherwise, <c>false</c>.
		/// </value>
		public bool IsAvailable { get; set; }

		/// <summary>
		/// Gets or sets the version.
		/// </summary>
		/// <value>
		/// The version.
		/// </value>
		public Version Version { get; set; }

		/// <summary>
		/// Gets or sets the update zip.
		/// </summary>
		/// <value>
		/// The update zip.
		/// </value>
		public byte[] UpdateZipBytes { get; set; }
	}
}