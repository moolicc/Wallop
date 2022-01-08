using System.Text;

namespace Cog
{
    public abstract class Settings
    {
        public virtual void SettingChanged(string setting, object oldValue) { }

        public virtual string GetHierarchyName() => GetType().Name;

        public override string ToString()
        {
            var properties = GetType().GetProperties();
            var fields = GetType().GetFields();
            var builder = new StringBuilder();
            builder.AppendLine(GetType().Name);
            foreach (var property in properties)
            {
                builder.AppendFormat("  {0}: {1}", property.Name, property.GetValue(this)).AppendLine();
            }
            foreach (var field in fields)
            {
                builder.AppendFormat("  {0}: {1}", field.Name, field.GetValue(this)).AppendLine();
            }
            return builder.ToString();
        }
    }
}