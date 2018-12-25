using Wide.Interfaces.Settings;

namespace Turtle_IDE.Core.Settings
{
    public class IDESettingsItem : AbstractSettingsItem
    {
        public IDESettingsItem(string title, int priority, AbstractSettings settings) : base(title, settings)
        {
            this.Priority = priority;
        }
    }
}
