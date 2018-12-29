using System;
using Wide.Interfaces;
using Wide.Interfaces.Services;
using System.Threading;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Xml;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.IO;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Rendering;

namespace Turtle_IDE.Core
{
    /// <summary>
    /// MDView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PYView : IContentView
    {
        private IStatusbarService _statusbar;
        private Thread t;
        public static ICSharpCode.AvalonEdit.TextEditor editor;
        public static PythonConsoleControl.IronPythonConsoleControl console;
        private bool isModuleLoaded = false;

        public PYView(IStatusbarService statusbar)
        {
            this._statusbar = statusbar;
            InitializeComponent();
            textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            editor = textEditor;
            console = pythonConsole;
            try
            {
                IHighlightingDefinition pythonHighlighting;
                using (Stream s = typeof(PYView).Assembly.GetManifestResourceStream("Turtle_IDE.Core.Resource.Python.xshd"))
                {
                    if (s == null)
                        throw new InvalidOperationException("Could not find embedded resource");
                    using (XmlReader reader = new XmlTextReader(s))
                    {
                        pythonHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                            HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }

                HighlightingManager.Instance.RegisterHighlighting("Python Highlighting", new string[] { ".cool" }, pythonHighlighting);
                textEditor.SyntaxHighlighting = pythonHighlighting;
                IList<IVisualLineTransformer> transformers = textEditor.TextArea.TextView.LineTransformers;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void textEditor_TextChanged(object sender, EventArgs e)
        {
            var model = this.DataContext as IDEModel;
            if (model != null)
            {
                // Add autocomplete 
            }
        }

        private void Update()
        {
            _statusbar.LineNumber = textEditor.Document.GetLineByOffset(textEditor.CaretOffset).LineNumber;
            _statusbar.ColPosition = textEditor.TextArea.Caret.VisualColumn + 1;
            _statusbar.CharPosition = textEditor.CaretOffset;
            _statusbar.InsertMode = false;
            if (t == null || !t.IsAlive)
                Run();
        }

        private void Run()
        {
            t = new Thread(SimpleRun);
            t.Start();
        }

        private void SimpleRun(object obj)
        {
            uint i = 0;
            while (i < 1000)
            {
                _statusbar.Progress(true, i, 1000);
                Thread.Sleep(10);
                i++;
            }
            _statusbar.Progress(false, i, 1000);
        }

        private void PythonConsole_GotFocus(object sender, RoutedEventArgs e)
        {
            if (isModuleLoaded == false)
            {
                string script = "import sys\n" +
                "sys.path.append(\"External\\IronPython.2.7.9\\Lib\")\n" +
                "sys.path.append(\"External\\IronPython.2.7.9\\Lib\\site-packages\")";
                pythonConsole.Console.RunStatements(script);
                isModuleLoaded = true;
            }
        }
    }
}
