using System;
using System.IO;
using System.ServiceModel;

namespace NDistribUnit.Common.Contracts.DataContracts
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
    [MessageContract]
	public class UpdatePackage
	{
		/// <summary>
		/// Gets or sets a value indicating whether this instance is available.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is available; otherwise, <c>false</c>.
		/// </value>
		[MessageHeader]
		public bool IsAvailable { get; set; }

		/// <summary>
		/// Gets or sets the version.
		/// </summary>
		/// <value>
		/// The version.
		/// </value>
		[MessageHeader]
        public Version Version { get; set; }

		/// <summary>
		/// Gets or sets the update zip.
		/// </summary>
		/// <value>
		/// The update zip.
		/// </value>
		[MessageBodyMember]
        public Stream UpdateZipStream { get; set; }
	}
}