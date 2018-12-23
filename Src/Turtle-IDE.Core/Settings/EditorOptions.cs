using System.ComponentModel;
using System.Configuration;
using System.Windows.Media;
using Wide.Interfaces.Settings;

namespace Turtle_IDE.Core.Settings
{
    internal class EditorOptions : AbstractSettings
    {
        private static EditorOptions settings = new EditorOptions();

        private EditorOptions()
        {
        }

        [Browsable(false)]
        public static EditorOptions Default
        {
            get { return settings; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("White")]
        [Category("Colors")]
        [DisplayName("Background Color")]
        [Description("The background color of the text editor")]
        public SolidColorBrush BackgroundColor
        {
            get { return (SolidColorBrush)this["BackgroundColor"]; }
            set { this["BackgroundColor"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("Black")]
        [Category("Colors")]
        [DisplayName("Foreground Color")]
        [Description("The foreground color of the text editor")]
        public SolidColorBrush ForegroundColor
        {
            get { return (SolidColorBrush)this["ForegroundColor"]; }
            set { this["ForegroundColor"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("true")]
        [Category("Format")]
        [DisplayName("Display line numbers?")]
        [Description("Set to Yes to show line numbers on the text editor")]
        public bool ShowLineNumbers
        {
            get { return (bool)this["ShowLineNumbers"]; }
            set { this["ShowLineNumbers"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("false")]
        [Category("Format")]
        [DisplayName("Word wrap?")]
        [Description("Set to Yes to wrap words in a line on the text editor")]
        public bool WordWrap
        {
            get { return (bool)this["WordWrap"]; }
            set { this["WordWrap"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("Consolas")]
        [Category("Format")]
        [DisplayName("Font")]
        [Description("Select the font to use in the text editor")]
        public FontFamily FontFamily
        {
            get { return (FontFamily)this["FontFamily"]; }
            set { this["FontFamily"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("14")]
        [Category("Format")]
        [DisplayName("Size")]
        [Description("Select the size to use for the font in the text editor")]
        public int FontSize
        {
            get { return (int)this["FontSize"]; }
            set { this["FontSize"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("true")]
        [Browsable(false)]
        public bool LivePreview
        {
            get { return (bool)this["LivePreview"]; }
            set { this["LivePreview"] = value; }
        }
    }
}
