using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageGen.ChangeTracking
{
    public class ChangeSet
    {
        public Queue<IChange> Changes { get; private set; }

        public ChangeSet()
        {
            Changes = new Queue<IChange>();
        }

        public void PrintChangeSet()
        {
            const ConsoleColor COLOR_ADD = ConsoleColor.DarkGreen;
            const ConsoleColor COLOR_UPDATE = ConsoleColor.DarkCyan;
            const ConsoleColor COLOR_REMOVE = ConsoleColor.Red;


            var originalForeColor = Console.ForegroundColor;

            foreach (var item in Changes)
            {
                if (item.ChangeType == ChangeTypes.Create)
                {
                    Console.ForegroundColor = COLOR_ADD;
                    Console.WriteLine($"(+) Creating {item.TargetField} '{item.NewValue}'");
                }
                else if (item.ChangeType == ChangeTypes.Update)
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
    }
}
