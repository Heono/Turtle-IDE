using System.Windows;
using Microsoft.Practices.Unity;
using Wide.Interfaces;

namespace Turtle_IDE
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        private IDEBootstrapper b;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            b = new IDEBootstrapper();
            b.Run();
            var shell = b.Container.Resolve<IShell>();
            (shell as Window).Loaded += App_Loaded;
            (shell as Window).Unloaded += App_Unloaded;
        }

        void App_Unloaded(object sender, System.EventArgs e)
        {
            var shell = b.Container.Resolve<IShell>();
            shell.SaveLayout();
        }

        void App_Loaded(object sender, RoutedEventArgs e)
        {
            var shell = b.Container.Resolve<IShell>();
            shell.LoadLayout();
        }
    }
}
