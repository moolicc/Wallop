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
        const ConsoleColor COLOR_ADD = ConsoleColor.DarkGreen;
        const ConsoleColor COLOR_UPDATE = ConsoleColor.DarkCyan;
        const ConsoleColor COLOR_REMOVE = ConsoleColor.Red;
        const ConsoleColor COLOR_REVERT = ConsoleColor.DarkYellow;
        public Change[] Changes => _changes.ToArray();

        private List<Change> _changes;
        private int _lastId;

        public ChangeSet()
        {
            _changes = new List<Change>();
            _lastId = -1;
        }

        public Change this[int index]
        {
            get => _changes[index];
            set => _changes[index] = value;
        }

        public int AddChange(Change change)
        {
            _lastId++;
            _changes.Add(change);
            change.ID = _lastId;

            return _lastId;
        }


        public void PrintChangeSet(string filter = "")
        {

            if(_changes.Count == 0)
            {
                Console.WriteLine();
                Console.WriteLine("Changeset is empty.");
                return;
            }

            var originalForeColor = Console.ForegroundColor;

            for(int i = 0; i < _changes.Count; i++)
            {
                PrintChange(_changes[i]);
                Console.WriteLine();
            }

            Console.ForegroundColor = originalForeColor;
        }
        public void PrintChange(int changeId)
        {
            var item = _changes.FirstOrDefault(c => c.ID == changeId);

            if (item == null)
            {
                Console.WriteLine("Change with ID #{0} does not exist.", changeId);
                return;
            }

            PrintChange(item);
        }

        public Change? FindLastChange(string field)
            => _changes.LastOrDefault(c => c.TargetField == field);

        public Change? FindLastChange(string field, params ChangeTypes[] typeFilter)
            => _changes.LastOrDefault(c => c.TargetField == field && typeFilter.Contains(c.ChangeType));

        public IEnumerator<Change> GetEnumerator()
        {
            return _changes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _changes.GetEnumerator();
        }

        public void RemoveByIndex(int i)
        {
            _changes.RemoveAt(i);
        }

        private void PrintChange(Change change)
        {
            var originalForeColor = Console.ForegroundColor;

            Console.ForegroundColor = originalForeColor;
            Console.Write("#{0}: ", change.ID);
            if (change.ChangeType.HasFlag(ChangeTypes.Revert))
            {
                Console.ForegroundColor = COLOR_REVERT;
                Console.Write($"(REVERTS #{change.RevertsID}) ");
            }

            if (change.ChangeType.HasFlag(ChangeTypes.Create))
            {
                Console.ForegroundColor = COLOR_ADD;
                Console.Write($"(+) Creating {change.TargetField} '{change.NewValue}'");
            }
            else if (change.ChangeType.HasFlag(ChangeTypes.Update))
            {
                Console.ForegroundColor = COLOR_UPDATE;
                Console.Write($"(*) Updating {change.TargetField} '{change.CurrentValue}'->'{change.NewValue}'");
            }
            else
            {
                Console.ForegroundColor = COLOR_REMOVE;
                Console.Write($"(-) Removing {change.TargetField} '{change.CurrentValue}'");
            }

            Console.ForegroundColor = originalForeColor;
        }
    }
}
