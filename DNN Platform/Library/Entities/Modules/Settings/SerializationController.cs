﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Settings
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Services.Exceptions;

    /// <inheritdoc/>
    public partial class SerializationController : ISerializationManager
    {
        /// <inheritdoc/>
        string ISerializationManager.SerializeValue<T>(T value) =>
            this.SerializeValue(value, null, typeof(T));

        /// <inheritdoc/>
        string ISerializationManager.SerializeValue<T>(T value, string serializer) =>
            this.SerializeValue(value, serializer, typeof(T));

        /// <inheritdoc/>
        string ISerializationManager.SerializeProperty<T>(T myObject, PropertyInfo property) =>
            ((ISerializationManager)this).SerializeProperty(myObject, property, null);

        /// <inheritdoc/>
        string ISerializationManager.SerializeProperty<T>(T myObject, PropertyInfo property, string serializer)
        {
            var settingValue = property.GetValue(myObject, null) ?? string.Empty;
            return this.SerializeValue(settingValue, serializer, property.PropertyType);
        }

        /// <inheritdoc/>
        void ISerializationManager.DeserializeProperty<T>(T myObject, PropertyInfo property, string propertyValue) =>
            ((ISerializationManager)this).DeserializeProperty(myObject, property, propertyValue, null);

        /// <inheritdoc/>
        void ISerializationManager.DeserializeProperty<T>(T myObject, PropertyInfo property, string propertyValue, string serializer)
        {
            try
            {
                var valueType = propertyValue.GetType();
                var propertyType = property.PropertyType;
                if (propertyType.GetGenericArguments().Any() && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // Nullable type
                    propertyType = propertyType.GetGenericArguments()[0];
                    if (string.IsNullOrEmpty(propertyValue))
                    {
                        property.SetValue(myObject, null, null);
                        return;
                    }
                }

                if (propertyType == valueType)
                {
                    // The property and settingsValue have the same type - no conversion needed - just update!
                    property.SetValue(myObject, propertyValue, null);
                    return;
                }

                if (!string.IsNullOrEmpty(serializer))
                {
                    var deserializedValue = this.CallSerializerMethod(serializer, property.PropertyType, propertyValue, nameof(ISettingsSerializer<T>.Deserialize));
                    property.SetValue(myObject, deserializedValue, null);
                    return;
                }

                if (propertyType.BaseType == typeof(Enum))
                {
                    // The property is an enum. Determine if the enum value is persisted as string or numeric.
                    if (Regex.IsMatch(propertyValue, "^\\d+$"))
                    {
                        // The enum value is a number
                        property.SetValue(myObject, Enum.ToObject(propertyType, Convert.ToInt32(propertyValue, CultureInfo.InvariantCulture)), null);
                    }
                    else
                    {
                        try
                        {
                            property.SetValue(myObject, Enum.Parse(propertyType, propertyValue, true), null);
                        }
                        catch (ArgumentException exception)
                        {
                            // Just log the exception. Use the default.
                            Exceptions.LogException(exception);
                        }
                    }

                    return;
                }

                TimeSpan timeSpanValue;
                if (propertyType.IsAssignableFrom(typeof(TimeSpan)) && TimeSpan.TryParse(propertyValue, CultureInfo.InvariantCulture, out timeSpanValue))
                {
                    property.SetValue(myObject, timeSpanValue);
                    return;
                }

                DateTime dateTimeValue;
                if (propertyType.IsAssignableFrom(typeof(DateTime)) && DateTime.TryParse(propertyValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTimeValue))
                {
                    property.SetValue(myObject, dateTimeValue);
                    return;
                }

                if (propertyType.GetInterface(typeof(IConvertible).FullName) != null)
                {
                    propertyValue = this.ChangeFormatForBooleansIfNeeded(propertyType, propertyValue);
                    property.SetValue(myObject, Convert.ChangeType(propertyValue, propertyType, CultureInfo.InvariantCulture), null);
                    return;
                }

                var converter = TypeDescriptor.GetConverter(propertyType);
                if (converter.IsValid(propertyValue))
                {
                    converter.ConvertFromInvariantString(propertyValue);
                }
            }
            catch (Exception exception)
            {
                // TODO: Localize exception
                var message = string.Format(
                    CultureInfo.CurrentUICulture,
                    "Could not cast {0} to property {1} of type {2}",
                    propertyValue,
                    property.Name,
                    property.PropertyType);
                throw new InvalidCastException(message, exception);
            }
        }

        private string SerializeValue<T>(T value, string serializer, Type valueType)
        {
            string settingValueAsString = null;
            if (!string.IsNullOrEmpty(serializer))
            {
                settingValueAsString = (string)this.CallSerializerMethod(serializer, valueType, value, nameof(ISettingsSerializer<T>.Serialize));
            }

            if (settingValueAsString == null)
            {
                settingValueAsString = this.GetSettingValueAsString(value);
            }

            return settingValueAsString;
        }

        private string GetSettingValueAsString<T>(T settingValue)
        {
            var dateTimeValue = settingValue as DateTime?;
            if (dateTimeValue != null)
            {
                return dateTimeValue.Value.ToString("o", CultureInfo.InvariantCulture);
            }

            var timeSpanValue = settingValue as TimeSpan?;
            if (timeSpanValue != null)
            {
                return timeSpanValue.Value.ToString("c", CultureInfo.InvariantCulture);
            }

            return Convert.ToString(settingValue, CultureInfo.InvariantCulture);
        }

        private string ChangeFormatForBooleansIfNeeded(Type propertyType, string propertyValue)
        {
            if (!propertyType.Name.Equals("Boolean"))
            {
                return propertyValue;
            }

            bool boolValue;
            if (bool.TryParse(propertyValue, out boolValue))
            {
                return propertyValue;
            }

            if (propertyValue.Equals("1"))
            {
                return bool.TrueString;
            }

            if (propertyValue.Equals("0"))
            {
                return bool.FalseString;
            }

            return propertyValue;
        }

        private object CallSerializerMethod(string serializerTypeName, Type typeArgument, object value, string methodName)
        {
            var serializerType = Framework.Reflection.CreateType(serializerTypeName, true);
            if (serializerType == null)
            {
                return null;
            }

            var serializer = Framework.Reflection.CreateInstance(serializerType);
            if (serializer == null)
            {
                return null;
            }

            var serializerInterfaceType = typeof(ISettingsSerializer<>).MakeGenericType(typeArgument);
            var method = serializerInterfaceType.GetMethod(methodName);
            return method.Invoke(serializer, new[] { value, });
        }
    }
}
