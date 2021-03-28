using System;
using System.Windows.Forms;

namespace VirtualDesktopIndicator
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int nDesktops;
            if (args.Length != 1 || !Int32.TryParse(args[0], out nDesktops))
                nDesktops = 4;

            using (TrayIndicator ti = new TrayIndicator(nDesktops))
            {
                ti.Display();
                Application.Run();
            }
        }
    }
}
