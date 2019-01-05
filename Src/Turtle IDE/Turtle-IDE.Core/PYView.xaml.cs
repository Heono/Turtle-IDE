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
        //private bool isModuleLoaded = false;

        protected Process ConEmu;
        protected GuiMacro guiMacro;
        private Panel termPanel;
        private System.Windows.Forms.Timer timer1;

        private bool argRunAs = false;
        private bool argDebug = false;
        private bool argLog = false;

        private string argConEmuExe;
        private string argDirectory;
        private string argXmlFile;
        private string argCmdLine;
        private string promptBox;


        public PYView(IStatusbarService statusbar)
        {
            this._statusbar = statusbar;
            InitializeComponent();
            textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            editor = textEditor;
            
            try
            {
                byte[] r = Encoding.Default.GetBytes("40");
                byte[] g = Encoding.Default.GetBytes("3C");
                byte[] b = Encoding.Default.GetBytes("3B");
                textEditor.Background = new SolidColorBrush(Color.FromRgb(r[0], g[0], b[0]));
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
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            string lsOurDir;
            termPanel = new Panel();
            WFH.Child = termPanel;
            termPanel.Enabled = true;
            argConEmuExe = GetConEmu();
            argDirectory = Directory.GetCurrentDirectory();
            lsOurDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            argXmlFile = Path.Combine(lsOurDir, @"External\ConEmu\ConEmu.xml");
            argCmdLine = @"{cmd}"; // Use ConEmu's default {cmd} task
            termPanel.Resize += new System.EventHandler(this.termPanel_Resize);
            timer1 = new System.Windows.Forms.Timer();
            timer1.Interval = 100;
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Start();
            startConsole();
        }

        private string GetConEmu()
        {
            string sOurDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string[] sSearchIn = {
              Directory.GetCurrentDirectory(),
              sOurDir,
              Path.Combine(sOurDir, ".."),
              Path.Combine(sOurDir, @"External\ConEmu"),
              "%PATH%", "%REG%"
              };

            string[] sNames;
            sNames = new string[] { "ConEmu.exe", "ConEmu64.exe" };

            foreach (string sd in sSearchIn)
            {
                foreach (string sn in sNames)
                {
                    string spath;
                    if (sd == "%PATH%" || sd == "%REG%")
                    {
                        spath = sn; //TODO
                    }
                    else
                    {
                        spath = Path.Combine(sd, sn);
                    }
                    if (File.Exists(spath))
                        return spath;
                }
            }

            // Default
            return "ConEmu.exe";
        }

        private string GetConEmuExe()
        {
            bool bExeLoaded = false;
            string lsConEmuExe = null;

            while (!bExeLoaded && (ConEmu != null) && !ConEmu.HasExited)
            {
                try
                {
                    lsConEmuExe = ConEmu.Modules[0].FileName;
                    bExeLoaded = true;
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    Thread.Sleep(50);
                }
            }

            return lsConEmuExe;
        }

        // Returns Path to "ConEmuCD[64].dll" (to GuiMacro execution)
        private string GetConEmuCD()
        {
            // Query real (full) path of started executable
            string lsConEmuExe = GetConEmuExe();
            if (lsConEmuExe == null)
                return null;

            // Determine bitness of **our** process
            string lsDll = (IntPtr.Size == 8) ? "ConEmuCD64.dll" : "ConEmuCD.dll";

            // Ready to find the library
            String lsExeDir, ConEmuCD;
            lsExeDir = Path.GetDirectoryName(lsConEmuExe);
            ConEmuCD = Path.Combine(lsExeDir, "ConEmu\\" + lsDll);
            if (!File.Exists(ConEmuCD))
            {
                ConEmuCD = Path.Combine(lsExeDir, lsDll);
                if (!File.Exists(ConEmuCD))
                {
                    ConEmuCD = lsDll; // Must not get here actually
                }
            }
            return ConEmuCD;
        }

        private void ExecuteGuiMacro(string asMacro)
        {
            // conemuc.exe -silent -guimacro:1234 print("\e","git"," --version","\n")
            string ConEmuCD = GetConEmuCD();
            if (ConEmuCD == null)
            {
                throw new GuiMacroException("ConEmuCD must not be null");
            }

            if (guiMacro != null && guiMacro.LibraryPath != ConEmuCD)
            {
                guiMacro = null;
            }

            try
            {
                if (guiMacro == null)
                    guiMacro = new GuiMacro(ConEmuCD);
                guiMacro.Execute(ConEmu.Id.ToString(), asMacro,
                    (GuiMacro.GuiMacroResult code, string data) => {
                        Debugger.Log(0, "GuiMacroResult", "code=" + code.ToString() + "; data=" + data + "\n");
                    });
            }
            catch (GuiMacroException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message, "GuiMacroException", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void macroStart()
        {
            if (promptBox == "")
                return;
            ExecuteGuiMacro(promptBox);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if ((ConEmu != null) && ConEmu.HasExited)
            {
                timer1.Stop();
                ConEmu = null;
            }
        }

        private void startConsole()
        {
            string sRunAs, sRunArgs;

            // Show terminal panel, hide start options
            //RefreshControls(true);

            sRunAs = argRunAs ? " -cur_console:a" : "";

            sRunArgs =
                (argDebug ? " -debugw" : "") +
                " -NoKeyHooks" +
                " -InsideWnd 0x" + termPanel.Handle.ToString("X") +
                " -LoadCfgFile \"" + argXmlFile + "\"" +
                " -Dir \"" + argDirectory + "\"" +
                (argLog ? " -Log" : "") +
                " -detached"
                //" -cmd " + // This one MUST be the last switch
                //argCmdLine.Text + sRunAs // And the shell command line itself
                ;

            promptBox = "Shell(\"new_console\", \"\", \"" + (argCmdLine + sRunAs).Replace("\"", "\\\"") + "\")";

            try
            {
                // Start ConEmu
                ConEmu = Process.Start(argConEmuExe, sRunArgs);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" +
                    "Command:\r\n" + argConEmuExe + "\r\n\r\n" +
                    "Arguments:\r\n" + sRunArgs,
                    ex.GetType().FullName + " (" + ex.NativeErrorCode.ToString() + ")",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Start monitoring
            timer1.Start();
            // Execute "startup" macro
            macroStart();
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr FindWindowEx(IntPtr hParent, IntPtr hChild, string szClass, string szWindow);

        private void termPanel_Resize(object sender, EventArgs e)
        {
            if (ConEmu != null)
            {
                IntPtr hConEmu = FindWindowEx(termPanel.Handle, (IntPtr)0, null, null);
                if (hConEmu != (IntPtr)0)
                {
                    //MoveWindow(hConEmu, 0, 0, termPanel.Width, termPanel.Height, true);
                }
            }
        }

        private void closeBtn_Click(object sender, EventArgs e)
        {
            ExecuteGuiMacro("Close(2,1)");
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
    }
}
