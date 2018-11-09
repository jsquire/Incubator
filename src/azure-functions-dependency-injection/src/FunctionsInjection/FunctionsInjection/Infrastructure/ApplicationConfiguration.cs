using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace FunctionsInjection.Infrastructure
{
    /// <summary>
    ///   The set of configuration values known by this application.
    /// </summary>
    public class ApplicationConfiguration
    {
        /// <summary>The connection string to the ratings data store.</summary>
        public readonly string RatingsStoreConnection;

        /// <summary>The name of the ratings database.</summary>
        public readonly string RatingsDatabaseName;

        /// <summary>The name of the ratings collection within the data store.</summary>
        public readonly string RatingsCollectionName;

        /// <summary>The url to the API endpoint used to get product information.</summary>
        public readonly string GetProductUrlFormatMask;

        /// <summary>The url to the API endpoint used to get user information.</summary>
        public readonly string GetUserUrlFormatMask;

        /// <summary>The amount of time, in seconds, to wait for completion of an external request.</summary>
        public readonly float ExternalRequestTimeoutSeconds;

        /// <summary>
        ///   Initializes a new instance of the <see cref="ApplicationConfiguration" /> class.
        /// </summary>
        /// 
        /// <param name="config">The root configuration data to use as a source for populating the instance.</param>
        /// 
        public ApplicationConfiguration(IConfigurationRoot config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var configItems = this.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Select(field => (field: field, configValue: config[field.Name]))
                .Where(item => item.configValue != null);

            foreach (var item in configItems)
            {
                this.TrySetValue(this, item.field, item.configValue);
            }
        }

        /// <summary>
        ///   Attempts to set a field value, ensuring the proper destination
        ///   type conversion.
        /// </summary>
        /// 
        /// <param name="target">The target instance to set the value on.</param>
        /// <param name="member">The reflective field information to use for setting the value.</param>
        /// <param name="value">The value to set, in string form.</param>
        /// 
        /// <returns><c>true</c> if the set was successful; otherwise, <c>false</c></returns>
        /// 
        private bool TrySetValue(object    target, 
                                 FieldInfo member, 
                                 string    value)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            object convertedValue;

            var targetType = member.FieldType;

            if ((targetType == typeof(object)) || (targetType == typeof(string)))
            {
                convertedValue = value;
            }
            else if ((targetType == typeof(bool)) || (targetType == typeof(bool?)))
            {
                if (bool.TryParse(value, out bool typedValue))
                {
                    convertedValue = typedValue;
                }
                else
                {
                    return false;
                }
            }
            else if ((targetType == typeof(short)) || (targetType == typeof(short?)))
            {
                if (short.TryParse(value, out short typedValue))
                {
                    convertedValue = typedValue;
                }
                else
                {
                    return false;
                }
            }
            else if ((targetType == typeof(ushort)) || (targetType == typeof(ushort?)))
            {
                if (ushort.TryParse(value, out ushort typedValue))
                {
                    convertedValue = typedValue;
                }
                else
                {
                    return false;
                }
            }
            else if ((targetType == typeof(int)) || (targetType == typeof(int?)))
            {
                if (int.TryParse(value, out int typedValue))
                {
                    convertedValue = typedValue;
                }
                else
                {
                    return false;
                }
            }
            else if ((targetType == typeof(uint)) || (targetType == typeof(uint?)))
            {
                if (uint.TryParse(value, out uint typedValue))
                {
                    convertedValue = typedValue;
                }
                else
                {
                    return false;
                }
            }
            else if ((targetType == typeof(double)) || (targetType == typeof(double?)))
            {
                if (double.TryParse(value, out double typedValue))
                {
                    convertedValue = typedValue;
                }
                else
                {
                    return false;
                }
            }
            else if ((targetType == typeof(float)) || (targetType == typeof(float?)))
            {
                if (float.TryParse(value, out float typedValue))
                {
                    convertedValue = typedValue;
                }
                else
                {
                    return false;
                }
            }
            else if ((targetType.IsEnum) && (Enum.IsDefined(targetType, value)))
            {
                convertedValue = Enum.Parse(targetType, value, true);
            }
            else
            {
                return false;
            }

            member.SetValue(target, convertedValue);
            return true;
        } 
    }
}
