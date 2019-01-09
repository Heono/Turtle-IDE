using System;
using Wide.Interfaces;
using Wide.Interfaces.Services;
using System.Threading;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Xml;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Forms;
using System.Windows.Media;
using System.Text;
using System.Windows;

namespace Turtle_IDE.Core
{
    /// <summary>
    /// PYView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PyCraftView : IContentView
    {
        private IStatusbarService _statusbar;
        private Thread t;
        public static ICSharpCode.AvalonEdit.TextEditor editor;
        public static PythonConsoleControl.IronPythonConsoleControl console;
        Panel _panel;
        Process pycraft;
        //private bool isModuleLoaded = false;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_NOACTIVATE = 0x0010;
        private const int GWL_STYLE = -16;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_THICKFRAME = 0x00040000;
        private const int SW_SHOWMAXIMIZED = 3;

        public PyCraftView(IStatusbarService statusbar)
        {
            this._statusbar = statusbar;
            InitializeComponent();
            _panel = new Panel();
            WFH.Child = _panel;
            _panel.Resize += new EventHandler(this._panel_Resize);

            textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            editor = textEditor;
            
            try
            {
                byte[] r = Encoding.Default.GetBytes("30");
                byte[] g = Encoding.Default.GetBytes("40");
                byte[] b = Encoding.Default.GetBytes("34");
                textEditor.Background = new SolidColorBrush(Color.FromRgb(r[0], g[0], b[0]));
                IHighlightingDefinition pythonHighlighting;
                using (Stream s = typeof(PyCraftView).Assembly.GetManifestResourceStream("Turtle_IDE.Core.Resource.Python.xshd"))
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
                System.Windows.Forms.MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            new Thread(() =>
            {
                pycraft = new Process();
                Thread.CurrentThread.IsBackground = true;
                pycraft.StartInfo.FileName = Environment.CurrentDirectory + @"\External\WPy3710\python-3.7.1\Scripts\pycraft.exe";
                pycraft.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                pycraft.Start();               
            }).Start();

            IntPtr hWnd = IntPtr.Zero;
            bool isrun = false;
            while (!isrun)
            {
                foreach (Process pList in Process.GetProcesses())
                {
                    if (pList.MainWindowTitle.Contains("Pyglet"))
                    {
                        hWnd = pList.MainWindowHandle;
                        isrun = true;
                    }
                }
            }

            // remove control box
            int style = GetWindowLong(WinGetHandle("Pyglet"), GWL_STYLE);
            style = style & ~WS_CAPTION & ~WS_THICKFRAME;
            SetWindowLong(WinGetHandle("Pyglet"), GWL_STYLE, style);
            SetParent(hWnd, _panel.Handle);

            // resize embedded application & refresh
            //ResizeEmbeddedApp();
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32")]
        private static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);

        [DllImport("user32")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr FindWindowEx(IntPtr hParent, IntPtr hChild, string szClass, string szWindow);

        private void _panel_Resize(object sender, EventArgs e)
        {
            IntPtr hPycraft = FindWindowEx(_panel.Handle, (IntPtr)0, null, null);
            if (hPycraft != (IntPtr)0)
            {
                MoveWindow(hPycraft, 0, 0, _panel.Width, _panel.Height, true);
            }
        }

        private void ResizeEmbeddedApp()
        {
            if (pycraft == null)
                return;

            SetWindowPos(WinGetHandle("Pyglet"), IntPtr.Zero, 0, 0, (int)_panel.ClientSize.Width, (int)_panel.ClientSize.Height, SWP_NOZORDER | SWP_NOACTIVATE);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size size = base.MeasureOverride(availableSize);
            ResizeEmbeddedApp();
            return size;
        }

        private static IntPtr WinGetHandle(string wName)
        {
            IntPtr hWnd = IntPtr.Zero;
            foreach (Process pList in Process.GetProcesses())
            {
                if (pList.MainWindowTitle.Contains(wName))
                {
                    hWnd = pList.MainWindowHandle;
                }
            }
            return hWnd;
        }

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void textEditor_TextChanged(object sender, EventArgs e)
        {
            var model = this.DataContext as PyCraftModel;
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
            //t = new Thread(SimpleRun);
            //t.Start();
        }

        private void SimpleRun(object obj)
        {
            /*
            uint i = 0;
            while (i < 1000)
            {
                _statusbar.Progress(true, i, 1000);
                Thread.Sleep(10);
                i++;
            }
            _statusbar.Progress(false, i, 1000);
            */
            
        }
    }
}
