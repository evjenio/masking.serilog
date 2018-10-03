// Licensed under the Apache License, Version 2.0 (the "License");
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
using Masking.Serilog.ByMasking;
using Serilog;
using Serilog.Configuration;

namespace Masking.Serilog
{
    /// <summary>
    /// Adds the Destructure.ByMaskingProperties() extension to <see cref="LoggerDestructuringConfiguration"/>.
    /// </summary>
    public static class LoggerConfigurationIgnoreExtensions
    {
        /// <summary>
        /// Destructure.ByMaskingProperties takes one or more property names, e.g. "Hash", and uses them to determine which
        /// properties are masked when an object of is destructured by serilog.
        /// </summary>
        /// <param name="configuration">The logger configuration to apply configuration to.</param>
        /// <param name="maskedPropertyName">The property name to hide.</param>
        /// <returns>An object allowing configuration to continue.</returns>
        public static LoggerConfiguration ByMaskingProperties(this LoggerDestructuringConfiguration configuration, params string[] maskedPropertyName)
        {
            return configuration.With(new DestructureByMaskingPolicy(maskedPropertyName));
        }

        /// <summary>
        /// Destructure.ByMaskingProperties takes one or more property names, e.g. "Hash", and uses them to determine which
        /// properties are masked when an object of is destructured by serilog.
        /// </summary>
        /// <param name="configuration">The logger configuration to apply configuration to.</param>
        /// <param name="opts"><see cref="Action"/> to configure <see cref="MaskingOptions"/>.</param>
        /// <returns>An object allowing configuration to continue.</returns>
        public static LoggerConfiguration ByMaskingProperties(this LoggerDestructuringConfiguration configuration, Action<MaskingOptions> opts)
        {
            var maskingOptions = new MaskingOptions();
            opts(maskingOptions);
            return configuration.With(new DestructureByMaskingPolicy(maskingOptions));
        }
    }
}
