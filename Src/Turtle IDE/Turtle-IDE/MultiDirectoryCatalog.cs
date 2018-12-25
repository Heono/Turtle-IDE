using System.Collections.Generic;
using System.IO;
using Microsoft.Practices.Prism.Modularity;

namespace Turtle_IDE
{
    public class MultipleDirectoryModuleCatalog : DirectoryModuleCatalog
    {
        private readonly IList<string> _pathsToProbe;

        /// <summary>
        /// Initializes a new instance of the MultipleDirectoryModuleCatalog class.
        /// </summary>
        /// <param name="pathsToProbe">An IList of paths to probe for modules.</param>
        public MultipleDirectoryModuleCatalog(IList<string> pathsToProbe)
        {
            _pathsToProbe = pathsToProbe;
        }

        /// <summary>
        /// Provides multiple-path loading of modules over the default <see cref="DirectoryModuleCatalog.InnerLoad"/> method.
        /// </summary>
        protected override void InnerLoad()
        {
            foreach (string path in _pathsToProbe)
            {
                if (Directory.Exists(path))
                {
                    this.ModulePath = path;
                    base.InnerLoad();
                }
            }
        }
    }
}
