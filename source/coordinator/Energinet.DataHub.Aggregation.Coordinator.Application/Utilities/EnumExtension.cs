// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.ComponentModel;

namespace Energinet.DataHub.Aggregation.Coordinator.Application.Utilities
{
    public static class EnumExtension
    {
        /// <summary>
        /// Gets the description attribute from an enum
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns>string</returns>
        public static string GetDescription(this Enum enumValue)
        {
            var attribute = enumValue.GetAttributeOfType<DescriptionAttribute>();
            return attribute == null ? string.Empty : attribute.Description;
        }

        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example><![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]></example>
        public static T GetAttributeOfType<T>(this Enum enumVal)
            where T : System.Attribute
        {
            if (enumVal == null)
            {
                throw new ArgumentNullException(nameof(enumVal));
            }

            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return attributes.Length > 0 ? (T)attributes[0] : null!;
        }
    }
}
