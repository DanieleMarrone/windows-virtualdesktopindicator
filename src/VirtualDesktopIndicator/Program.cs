using System;
using System.Reflection;
using System.Windows.Forms;

namespace VirtualDesktopIndicator
{
    class Program
    {
        private static string AppName =>
            Assembly.GetExecutingAssembly().GetName().Name;

        private static int VirtualDesktopsCount =>
            VirtualDesktopApi.Desktop.Count;


        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int nDesktops;
            if (args.Length != 1 || !Int32.TryParse(args[0], out nDesktops))
                nDesktops = 4;

            using (ThemeMonitor themeMonitor = new ThemeMonitor())
            using (TrayIndicator ti = new TrayIndicator(nDesktops, themeMonitor.CurrentTheme))
            {
                themeMonitor.ThemeChanged += (s, e) => ti.CreateIcons(themeMonitor.CurrentTheme);

                ti.Switch += SwitchToDesktop;

                ti.Display();
                themeMonitor.Start();

                Application.Run();

                ti.Switch -= SwitchToDesktop;
            }
        }

        static void SwitchToDesktop(object sender, int index)
        {
            // create desktop if not exists
            for (int i = VirtualDesktopsCount; i <= index; i++)
                VirtualDesktopApi.Desktop.Create();

            VirtualDesktopApi.Desktop.FromIndex(index).MakeVisible();
        }


    }
}
