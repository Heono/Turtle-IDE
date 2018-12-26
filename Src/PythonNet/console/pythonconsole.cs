using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Python.Runtime;

namespace Python.Runtime
{
    /// <summary>
    /// Example of Embedding Python inside of a .NET program.
    /// </summary>
    /// <remarks>
    /// It has similar functionality to doing `import clr` from within Python, but this does it
    /// the other way around; That is, it loads Python inside of .NET program.
    /// See https://github.com/pythonnet/pythonnet/issues/358 for more info.
    /// </remarks>
    public sealed class PythonConsole
    {
        private PythonConsole()
        {
        }

        [STAThread]
        public static int Main(string[] args)
        {
            string[] cmd = Environment.GetCommandLineArgs();
            PythonEngine.Initialize();

            int i = Runtime.Py_Main(cmd.Length, cmd);
            PythonEngine.Shutdown();

            return i;
        }
    }
}
