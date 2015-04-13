using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace PathOfFilters
{
    internal class CompletionLists
    {
        private static readonly string[] _armour = File.ReadAllLines(Directory.GetCurrentDirectory() + @"\resources\armour-data.txt");
        private static readonly string[] _weapon = File.ReadAllLines(Directory.GetCurrentDirectory() + @"\resources\weapon-data.txt");
        public string[] Items = _armour.Union(_weapon).ToArray();

        public string[] Conditions =
        {
            "ItemLevel", "DropLevel", "Quality", "Rarity", "Class", "BaseType", "Sockets", "LinkedSockets", "SocketGroup",
            "PlayAlertSound", "SetBackgroundColor", "SetTextColor", "SetBorderColor"
        };

        public ObservableCollection<string> ObservableClass
        {
            get
            {
                var collectionString = new[]
                {
                    "ItemLevel", "DropLevel", "Quality", "Rarity", "Class",
                    "BaseType", "Sockets", "LinkedSockets", "SocketGroup",
                    "SetBorderColor", "SetTextColor", "SetBackgroundColor",
                    "PlayAlertSound"
                };

                return new ObservableCollection<string>(collectionString);
            }
        }
    }
}
