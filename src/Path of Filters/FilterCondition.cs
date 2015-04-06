using System.Collections.ObjectModel;

namespace PathOfFilters
{
    public class FilterCondition
    {
        public static string[] Conditions =
        {
            "ItemLevel", "DropLevel", "Quality", "Rarity", "Class",
            "BaseType", "Sockets", "LinkedSockets", "SocketGroup",
            "SetBorderColor", "SetTextColor", "SetBackgroundColor",
            "PlayAlertSound"
        };
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
