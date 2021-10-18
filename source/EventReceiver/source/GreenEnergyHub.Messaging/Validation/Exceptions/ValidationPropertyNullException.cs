﻿// Copyright 2020 Energinet DataHub A/S
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

namespace GreenEnergyHub.Messaging.Validation.Exceptions
{
    /// <summary>
    /// ValidationPropertyNullException
    /// </summary>
    // Violates rule ImplementStandardExceptionConstructors.
#pragma warning disable CA1032 // Implement standard exception constructors
    public class ValidationPropertyNullException : Exception
    {
        public ValidationPropertyNullException(string propertyName, NullReferenceException nullReferenceException)
            : base($"{propertyName} is null", nullReferenceException)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        /// PropertyName
        /// </summary>
        public string PropertyName { get; }
    }
#pragma warning restore CA1032 // Implement standard exception constructors

}
