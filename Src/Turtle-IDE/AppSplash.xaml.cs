using System.Windows;
using Wide.Splash;

namespace Turtle_IDE
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AppSplash : Window, ISplashView
    {
        public AppSplash(SplashViewModel model)
        {
            InitializeComponent();
            DataContext = model;
        }
    }
}
