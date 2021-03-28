using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;


namespace VirtualDesktopIndicator
{
    public struct Palette
    {
        public Palette(Color foregroundColor, Color backgroundColor)
        {
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        public Color ForegroundColor { get; }
        public Color BackgroundColor { get; }

    }

    public struct IconSet
    {
        public IconSet(Icon @default, Icon active)
        {
            Default = @default;
            Active = active;
        }

        public Icon Default { get; }
        public Icon Active { get; }
    }


    class TrayIndicator : IDisposable
    {
        private static string AppName =>
            Assembly.GetExecutingAssembly().GetName().Name;

        private int NDesktops { get; }

        private NotifyIcon[] trayIcons;
        private IconSet[] icons;

        private ThemeMonitor themeMonitor;
        private Timer timer;

        #region Virtual Desktops

        private static int CurrentVirtualDesktop =>
            VirtualDesktopApi.Desktop.FromDesktop(VirtualDesktopApi.Desktop.Current) + 1;

        private int previousVirtualDesktop;

        private static int VirtualDesktopsCount =>
            VirtualDesktopApi.Desktop.Count;

        #endregion


        #region Theme

        private static readonly Dictionary<Theme, Palette> ThemesColors = new Dictionary<Theme, Palette>()
        {
            { Theme.Dark, new Palette(Color.White, Color.Black) },
            { Theme.Light, new Palette(Color.Black, Color.White) },
        };

        private Palette CurrentThemeColors => ThemesColors[themeMonitor.CurrentTheme];

        #endregion

        public TrayIndicator(int nDesktops)
        {
            NDesktops = nDesktops;

            themeMonitor = new ThemeMonitor();
            themeMonitor.ThemeChanged += (s, e) => { CreateIcons(); RefreshIcons(); };

            timer = new Timer { Interval = 250, Enabled = false };
            timer.Tick += TimerTick;

            icons = new IconSet[NDesktops];

            trayIcons = new NotifyIcon[NDesktops];
            for (int i = 0; i < NDesktops; i++)
            {
                trayIcons[i] = new NotifyIcon() { Tag = i, Text = $"Desktop {i+1}" };
                trayIcons[i].Click += TrayIconClick;
            }

            CreateIcons();
            RefreshIcons();
        }


        #region Events

        private void TimerTick(object sender, EventArgs e)
        {
            try
            {
                RefreshTooltips();

                if (CurrentVirtualDesktop != previousVirtualDesktop)
                {
                    previousVirtualDesktop = CurrentVirtualDesktop;
                    RefreshIcons();
                }
            }
            catch
            {
                ShowError("An unhandled error occured!");
                Application.Exit();
            }
        }

        private void ShowError(string error)
        {
            MessageBox.Show(
                error,
                "VirtualDesktopIndicator",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }


        private void RefreshTooltips()
        {
            int i;
            for (i = 0; i < VirtualDesktopsCount && i < NDesktops; i++)
                trayIcons[i].Text = VirtualDesktopApi.Desktop.DesktopNameFromIndex(i);
            for (; i < NDesktops; i++)
                trayIcons[i].Text = $"Desktop {i + 1}";
        }

        private void TrayIconClick(object sender, EventArgs e)
        {
            int i = (int)(sender as NotifyIcon).Tag;

            if (i == CurrentVirtualDesktop - 1)
                return;

            SwitchToDesktop(i);
        }

        private void SwitchToDesktop(int index)
        {
            // create desktop if not exists
            for (int i = VirtualDesktopsCount; i <= index; i++)
                VirtualDesktopApi.Desktop.Create();

            VirtualDesktopApi.Desktop.FromIndex(index).MakeVisible();

            RefreshIcons();
        }

        #endregion

        public void Display()
        {
            themeMonitor.Start();

            timer.Enabled = true;

            for (int i = 0; i < NDesktops; i++)
                trayIcons[i].Visible = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {

            for (int i = 0; i < NDesktops; i++)
                trayIcons[i].Visible = false;

            for (int i = 0; i < NDesktops; i++)
                trayIcons[i].Dispose();

            timer.Dispose();
        }

        ~TrayIndicator()
        {
            Dispose(false);
        }

        private void CreateIcons()
        {
            IconMaker iconMaker = new IconMaker(CurrentThemeColors.ForegroundColor, CurrentThemeColors.BackgroundColor);
            
            for (int i = 0; i < NDesktops; i++)
            {
                Icon defaultIcon = iconMaker.GenerateIcon(i + 1);
                Icon activeIcon = iconMaker.GenerateIcon(i + 1, true);
                icons[i] = new IconSet(defaultIcon, activeIcon);
            }
        }

        private void RefreshIcons()
        {
            for (int i = 0; i < NDesktops; i++)
                trayIcons[i].Icon = CurrentVirtualDesktop == i + 1 ? icons[i].Active : icons[i].Default;
        }

    }
}
