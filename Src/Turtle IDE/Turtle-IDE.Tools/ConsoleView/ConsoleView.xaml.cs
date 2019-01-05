using System.ComponentModel;
using System.Windows.Controls;
using Wide.Interfaces;
using System.Windows.Forms.Integration;

namespace Turtle_IDE.Tools.ConsoleView
{
    /// <summary>
    /// ConsoleView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ConsoleView : UserControl, IContentView, INotifyPropertyChanged
    {
        public static WindowsFormsHost winfrmHost;

        public ConsoleView()
        {
            InitializeComponent();
            winfrmHost = wfh;
        }

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
