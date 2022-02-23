// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace CKLunchBot.Core.Utils
{
    public static class KST
    {
        public static DateTime Now => DateTime.UtcNow.AddHours(9);
    }
}
