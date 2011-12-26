using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NDistribUnit.Common.Common.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Determines whether the value is one of the provided.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="values">The provided values.</param>
        /// <returns>
        ///   <c>true</c> if the value is one of the provided; otherwise, <c>false</c>.
        /// </returns>
         public static bool IsOneOf<TEnum>(this TEnum value, params TEnum[] values)
        {
            return values.Any(e => e.Equals(value));
        }
    }
}