using System;
using System.Collections.Generic;

namespace NDistribUnit.Common.HashChecks
{
	/// <summary>
	/// 
	/// </summary>
	public class HashValidationException: Exception
	{
		private readonly IEnumerable<string> messages;

		/// <summary>
		/// Initializes a new instance of the <see cref="HashValidationException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public HashValidationException(string message): this(new[]{message})
		{}

		/// <summary>
		/// Initializes a new instance of the <see cref="HashValidationException"/> class.
		/// </summary>
		/// <param name="messages">The messages.</param>
		public HashValidationException(IEnumerable<string> messages)
		{
			this.messages = messages;
		}
		
		/// <summary>
		/// Gets the message.
		/// </summary>
		public override string Message { get { return string.Join("\n\r", messages); } }
	}
}