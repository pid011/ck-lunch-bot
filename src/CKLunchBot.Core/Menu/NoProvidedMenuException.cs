// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Serialization;

namespace CKLunchBot.Core.Menu
{

    [Serializable]
    public class NoProvidedMenuException : Exception
    {
        public NoProvidedMenuException() { }
        public NoProvidedMenuException(string message) : base(message) { }
        public NoProvidedMenuException(string message, Exception inner) : base(message, inner) { }
        protected NoProvidedMenuException(
          SerializationInfo info,
          StreamingContext context) : base(info, context) { }
    }
}
