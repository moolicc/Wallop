using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageGen.ChangeTracking
{
    public class ChangeSet : IEnumerable<Change>
    {
        public Change[] Changes => _changes.ToArray();

        private List<Change> _changes;

        public ChangeSet()
        {
            _changes = new List<Change>();
        }

        public Change this[int index]
        {
            get => _changes[index];
            set => _changes[index] = value;
        }

        public void AddChange(Change change)
        {
            _changes.Add(change);
        }

        public void PrintChangeSet(string filter = "")
        {
            const ConsoleColor COLOR_ADD = ConsoleColor.DarkGreen;
            const ConsoleColor COLOR_UPDATE = ConsoleColor.DarkCyan;
            const ConsoleColor COLOR_REMOVE = ConsoleColor.Red;


            var originalForeColor = Console.ForegroundColor;

            for(int i = 0; i < _changes.Count; i++)
            {
                var item = _changes[i];

                Console.ForegroundColor = originalForeColor;
                Console.Write("{0}: ", i);
                if(item.ChangeType.HasFlag(ChangeTypes.Revert))
                {
                    Console.Write("(R) ");
                }

                if (item.ChangeType.HasFlag(ChangeTypes.Create))
                {
                    Console.ForegroundColor = COLOR_ADD;
                    Console.WriteLine($"(+) Creating {item.TargetField} '{item.NewValue}'");
                }
                else if (item.ChangeType.HasFlag(ChangeTypes.Update))
                {
                    Console.ForegroundColor = COLOR_UPDATE;
                    Console.WriteLine($"(*) Updating {item.TargetField} '{item.CurrentValue}'->'{item.NewValue}'");
                }
                else
                {
                    Console.ForegroundColor = COLOR_REMOVE;
                    Console.WriteLine($"(-) Removing {item.TargetField} '{item.CurrentValue}'");
                }
            }

            Console.ForegroundColor = originalForeColor;
        }

        public IEnumerator<Change> GetEnumerator()
        {
            return _changes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _changes.GetEnumerator();
        }
    }
}
