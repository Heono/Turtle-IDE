using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Windows.Controls;
using Wide.Interfaces;
using System.Windows.Forms.Integration;

namespace Turtle_IDE.Tools.ConsoleView
{
    /// <summary>
    /// ConsoleView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ConsoleView : System.Windows.Controls.UserControl, IContentView, INotifyPropertyChanged
    {
        public static WindowsFormsHost winfrmHost;
        protected Process ConEmu;
        protected GuiMacro guiMacro;
        private System.Windows.Forms.Panel termPanel;
        private System.Windows.Forms.Timer timer1;

        private bool argRunAs = false;
        private bool argDebug = false;
        private bool argLog = false;

        private string argConEmuExe;
        private string argDirectory;
        private string argXmlFile;
        private string argCmdLine;
        private string promptBox;
        private string pyPath;

        public ConsoleView()
        {
            InitializeComponent();
            winfrmHost = wfh;

            pyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) 
                     + @"\External\WPy3710\python-3.7.1\python.exe";

            string lsOurDir;

            termPanel = new System.Windows.Forms.Panel();
            ConsoleView.winfrmHost.Child = termPanel;
            termPanel.Enabled = true;

            argConEmuExe = GetConEmu();
            argDirectory = Directory.GetCurrentDirectory();
            lsOurDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            argXmlFile = Path.Combine(lsOurDir, @"External\ConEmu\ConEmu.xml");
            argCmdLine = pyPath; //@"{cmd}"; // Use ConEmu's default {cmd} task
            termPanel.Resize += new System.EventHandler(this.termPanel_Resize);

            timer1 = new System.Windows.Forms.Timer();
            timer1.Interval = 100;
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Start();

            startConsole();
        }

        private void ConEmu_Exited(object sender, EventArgs e)
        {
            //Console 종료 이벤트
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
                    //lsConEmuExe = ConEmu.Modules[0].FileName;
                    lsConEmuExe = ConEmu.StartInfo.FileName;
                    bExeLoaded = true;
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    Thread.Sleep(50);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
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
                //throw new GuiMacroException("ConEmuCD must not be null");
                return;
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

        public void macroStart()
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
                btn_closeConsole.IsEnabled = false;
            }
        }

        public void startConsole()
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
                ConEmu.Exited += new EventHandler(ConEmu_Exited);
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

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void newConsole_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if ((ConEmu == null))
            {
                string lsOurDir;
                argConEmuExe = GetConEmu();
                argDirectory = Directory.GetCurrentDirectory();
                lsOurDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                argXmlFile = Path.Combine(lsOurDir, @"External\ConEmu\ConEmu.xml");
                argCmdLine = @"{cmd}"; // Use ConEmu's default {cmd} task

                startConsole();
                btn_closeConsole.IsEnabled = true;
            }
            else if ((ConEmu != null) && !ConEmu.HasExited)
            {
                ExecuteGuiMacro("Close(2,1)");

                string lsOurDir;
                argConEmuExe = GetConEmu();
                argDirectory = Directory.GetCurrentDirectory();
                lsOurDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                argXmlFile = Path.Combine(lsOurDir, @"External\ConEmu\ConEmu.xml");
                argCmdLine = @"{cmd}"; // Use ConEmu's default {cmd} task

                startConsole();
                btn_closeConsole.IsEnabled = true;
            }
        }

        public void closeConsoleProcess()
        {
            ExecuteGuiMacro("Close(2,1)");
        }

        private void closeConsole_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ExecuteGuiMacro("Close(2,1)");
        }

        private void newPyShell_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if ((ConEmu == null))
            {
                string lsOurDir;
                argConEmuExe = GetConEmu();
                argDirectory = Directory.GetCurrentDirectory();
                lsOurDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                argXmlFile = Path.Combine(lsOurDir, @"External\ConEmu\ConEmu.xml");
                argCmdLine = pyPath; // python 파일 위치

                startConsole();
                btn_closeConsole.IsEnabled = true;
            }
            else if ((ConEmu != null) && !ConEmu.HasExited)
            {
                ExecuteGuiMacro("Close(2,1)");

                string lsOurDir;
                argConEmuExe = GetConEmu();
                argDirectory = Directory.GetCurrentDirectory();
                lsOurDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                argXmlFile = Path.Combine(lsOurDir, @"External\ConEmu\ConEmu.xml");
                argCmdLine = pyPath; // python 파일 위치

                startConsole();
                btn_closeConsole.IsEnabled = true;
            }
        }
    }
}
