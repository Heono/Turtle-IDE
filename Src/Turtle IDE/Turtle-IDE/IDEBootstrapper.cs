using System.Collections.Generic;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Wide.Interfaces;
using Wide.Shell;
using Wide.Splash;

namespace Turtle_IDE
{
    internal class IDEBootstrapper : WideBootstrapper
    {
        public IDEBootstrapper(bool isMetro = true)
            : base(isMetro)
        {
        }

        protected override void InitializeModules()
        {
            //Register your splash view or else the default splash will load
            Container.RegisterType<ISplashView, AppSplash>(); 

            //Register your workspace here - if you have any
            Container.RegisterType<AbstractWorkspace, IDEWorkspace>(new ContainerControlledLifetimeManager()); //다름

            // You can also override the logger service. Currently, NLog is used.
            // Since the config file is there in the output location, text files should be available in the Logs folder.

            //Initialize the original bootstrapper which will load modules from the probing path. Check app.config for probing path.
            base.InitializeModules();
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            var catalog = new MultipleDirectoryModuleCatalog(new List<string>() { @".", @".\External", @".\Internal" });
            return catalog;
        }
    }
}
