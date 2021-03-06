﻿using System;

namespace NDistribUnit.Common.DataContracts
{
    /// <summary>
    /// The result of a ping operation
    /// </summary>
    public class PingResult
    {
        /// <summary>
        /// Gets or sets the name of the endpoint.
        /// </summary>
        /// <value>
        /// The name of the endpoint.
        /// </value>
        public string Name { get; set; }

        /// <summary>
		/// Gets or sets the version.
		/// </summary>
		/// <value>
		/// The version.
		/// </value>
    	public Version Version { get; set; }
    }
}