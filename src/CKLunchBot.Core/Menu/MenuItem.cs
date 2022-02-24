using System.Collections.Generic;

namespace CKLunchBot.Core.Menu
{
    public class MenuItem
    {
        public IReadOnlyList<string>? Menus { get; init; }
        public IReadOnlyList<string>? SelfBar { get; init; }
    }
}
