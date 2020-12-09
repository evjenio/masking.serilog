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
using System.Collections.Generic;

namespace Masking.Serilog.ByMasking
{
    /// <summary>
    /// Options for <see cref="DestructureByMaskingPolicy"/>.
    /// </summary>
    public class MaskingOptions
    {
        /// <summary>
        /// Text replacing original value.
        /// </summary>
        public string Mask { get; set; } = "******";

        /// <summary>
        /// List of property names to hide from log.
        /// </summary>
        public List<string> PropertyNames { get; } = new List<string>();

        /// <summary>
        /// A flag to specify whether static properties should be included when Destructuring the object.
        /// </summary>
        public bool ExcludeStaticProperties { get; set; } = false;
        
        /// <summary>
        /// List of name spaces for which masking is ignored.
        /// </summary>
        public List<string> Namespaces { get; } = new List<string>();
    }
}
