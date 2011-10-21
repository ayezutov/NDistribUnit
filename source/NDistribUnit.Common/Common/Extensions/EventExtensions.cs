using System;

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
		public static void SafeInvoke<T1, T2>(this Action<T1, T2> @event, T1 arg1, T2 arg2)
		{
			var local = @event;

			if (local != null)
				local(arg1, arg2);
		}

		/// <summary>
		/// Invokes the event safely.
		/// </summary>
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
		public static void SafeInvoke<TData>(this EventHandler<EventArgs<TData>> @event, object sender, TData data)
		{
			var local = @event;

			if (local != null)
				local(sender, new EventArgs<TData>(data));
		}

		/// <summary>
		/// Invokes the event safely.
		/// </summary>
		/// <param name="event">The @event.</param>
		/// <param name="sender">The sender.</param>
		public static void SafeInvoke(this EventHandler @event, object sender)
		{
			var local = @event;

			if (local != null)
				local(sender, EventArgs.Empty);
		}

		/// <summary>
		/// Invokes the event safely.
		/// </summary>
		public static void SafeInvoke(this Delegate @event, params object[] args)
		{
			var local = @event;

			if (local != null)
				local.DynamicInvoke(args);
		}
	}
}