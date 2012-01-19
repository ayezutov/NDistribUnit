using System;
using System.Diagnostics;

namespace NDistribUnit.Common.Extensions
{
	/// <summary>
	/// 
	/// </summary>
	public static class EventExtensions
	{
		/// <summary>
		/// Invokes the event safely.
		/// </summary>
        [DebuggerStepThrough]
        public static void SafeInvoke<T1, T2>(this Action<T1, T2> @event, T1 arg1, T2 arg2)
		{
			var local = @event;

			if (local != null)
				local(arg1, arg2);
		}

		/// <summary>
		/// Invokes the event safely.
		/// </summary>
		[DebuggerStepThrough]
		public static void SafeInvoke<TEventArgs>(this EventHandler<TEventArgs> @event, object sender, TEventArgs args) where TEventArgs : EventArgs
		{
			var local = @event;

			if (local != null)
				local(sender, args);
		}

		/// <summary>
		/// Invokes the event safely.
		/// </summary>
		/// <typeparam name="TData">The type of the data.</typeparam>
		/// <param name="event">The @event.</param>
		/// <param name="sender">The sender.</param>
		/// <param name="data">The data.</param>
        [DebuggerStepThrough]
        public static void SafeInvoke<TData>(this EventHandler<EventArgs<TData>> @event, object sender, TData data)
		{
			var local = @event;

			if (local != null)
				local(sender, new EventArgs<TData>(data));
		}

		/// <summary>
		/// Invokes the event safely.
		/// </summary>
		/// <typeparam name="TData">The type of the data.</typeparam>
		/// <param name="event">The @event.</param>
		/// <param name="sender">The sender.</param>
		/// <param name="data">The data.</param>
        [DebuggerStepThrough]
        public static void SafeInvokeAsync<TData>(this EventHandler<EventArgs<TData>> @event, object sender, TData data)
		{
			var local = @event;

			if (local != null)
				local.BeginInvoke(sender, new EventArgs<TData>(data), null, null);
		}

		/// <summary>
		/// Invokes the event safely.
		/// </summary>
		/// <param name="event">The @event.</param>
		/// <param name="sender">The sender.</param>
        [DebuggerStepThrough]
        public static void SafeInvoke(this EventHandler @event, object sender)
		{
			var local = @event;

			if (local != null)
				local(sender, EventArgs.Empty);
		}
	}
}