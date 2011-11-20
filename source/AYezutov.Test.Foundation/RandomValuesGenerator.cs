using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AYezutov.Test.Foundation
{


    /// <summary>
    /// 
    /// </summary>
    public class ExcludeDescriptor
    {
        protected ExcludeDescriptor()
        {
        }

        public ExcludeDescriptor(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; protected set; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Exclude<T> : ExcludeDescriptor
    {
        public Exclude(Expression<Func<T, object>> property)
        {
            Expression propertyAccessor = property.Body;
            if (propertyAccessor.NodeType == ExpressionType.Convert)
                propertyAccessor = ((UnaryExpression)propertyAccessor).Operand;
            PropertyName = ((MemberExpression)propertyAccessor).Member.Name;
        }
    }

    /// <summary>
    /// Generates random values of different type
    /// </summary>
    public class RandomValuesGenerator
    {
        private static readonly Random rand = new Random();

        /// <summary>
        /// Generates the random entity.
        /// </summary>
        /// <returns></returns>
        public static T GenerateRandomEntity<T>() where T : new()
        {
            var newEntity = new T();

            FillPropertiesWithRandomValues(newEntity, true);

            return newEntity;
        }

        public static void FillPropertiesWithRandomValues<T>(T newEntity, bool fkRandom, params Exclude<T>[] excludes)
        {
            FillPropertiesWithRandomValues((object)newEntity, fkRandom, excludes);
        }

        public static void FillPropertiesWithRandomValues(object newEntity, bool fkRandom, params ExcludeDescriptor[] excludes)
        {
            foreach (PropertyInfo propInfo in newEntity.GetType().GetProperties())
            {
                try
                {
                    ExcludeDescriptor excludeForProperty = (from exclude in excludes where exclude.PropertyName.Equals(propInfo.Name) select exclude)
                        .FirstOrDefault();

                    if (excludeForProperty != null)
                        continue;

                    if ((propInfo.Name.EndsWith("FK") || propInfo.Name.EndsWith("Fk")) && !fkRandom)
                    {
                        propInfo.SetValue(newEntity, 1, null);
                    }
                    else
                    {
                        Type propertyType = propInfo.PropertyType;

                        object resultValue = GetRandomValue(propertyType);

                        if (resultValue != null)
                        {
                            propInfo.SetValue(newEntity, resultValue, null);
                        }
                    }
                }
                catch (Exception)
                {
                    // ignore property if could not set it;
                }
            }
        }
        public static T GetRandomValue<T>()
        {
            return (T)GetRandomValue(typeof(T));
        }

        /// <summary>
        /// Gets the random value.
        /// </summary>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        public static object GetRandomValue(Type propertyType)
        {
            int value = rand.Next();
            if (propertyType == typeof (String))
                return value.ToString();
            
            if (propertyType == typeof (Double))
                return rand.NextDouble();

            if (propertyType == typeof (Int32))
                return value;

            if (propertyType == typeof (Int16))
                return Convert.ToInt16(value%Int16.MaxValue);
            
            if (propertyType == typeof (Decimal))
                return Convert.ToDecimal(Math.Floor(Math.Sqrt(value)));
            
            if (propertyType == typeof (Boolean))
                return (value > (Int32.MaxValue/2));

            if (propertyType == typeof (DateTime) ||
                propertyType == typeof (DateTime?))
                return DateTime.Now.AddSeconds(-value);

            if (propertyType == typeof (Guid))
                return Guid.NewGuid();

            if (propertyType == typeof (Byte[]))
            {
                var buffer = new byte[100];
                rand.NextBytes(buffer);
                return buffer;
            }

            if (propertyType == typeof (MemoryStream))
            {
                return new MemoryStream(GetRandomValue<byte[]>());
            }

            return null;
        }
    }
}