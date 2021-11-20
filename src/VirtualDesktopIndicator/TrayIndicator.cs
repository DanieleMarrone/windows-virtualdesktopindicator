using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace VirtualDesktopIndicator
{
    class TrayIndicator : IDisposable
    {
        private int NDesktops { get; }

        private NotifyIcon[] trayIcons;
        private IconSet[] icons;

        private Timer timer;

        public event EventHandler<int> Switch;

        #region Virtual Desktops

        private static int CurrentVirtualDesktop =>
            VirtualDesktopApi.Desktop.FromDesktop(VirtualDesktopApi.Desktop.Current) + 1;

        private int previousVirtualDesktop;

        private static int VirtualDesktopsCount =>
            VirtualDesktopApi.Desktop.Count;

        #endregion


        #region Colors

        private static readonly Dictionary<Theme, Palette> ThemesColors = new Dictionary<Theme, Palette>()
        {
            { Theme.Dark, new Palette(Color.White, Color.Black) },
            { Theme.Light, new Palette(Color.Black, Color.White) },
        };

        #endregion

        public TrayIndicator(int nDesktops, Theme theme)
        {
            NDesktops = nDesktops;

            timer = new Timer { Interval = 250, Enabled = false };
            timer.Tick += TimerTick;

            icons = new IconSet[NDesktops];

            trayIcons = new NotifyIcon[NDesktops];
            for (int i = 0; i < NDesktops; i++)
            {
                trayIcons[i] = CreateNotifyIcon(i);
                trayIcons[i].Click += TrayIconClick;
            }

            CreateIcons(theme);
        }


        public void CreateIcons(Theme currentTheme)
        {
            BuildIcons(ThemesColors[currentTheme]);
            RefreshIcons();
        }


        private NotifyIcon CreateNotifyIcon(int i)
        {
            return new NotifyIcon() { Tag = i, Text = $"Desktop {i + 1}" };
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

            Switch?.Invoke(this, i);

            RefreshIcons();
        }

        #endregion

        public void Display()
        {
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

        private void BuildIcons(Palette palette)
        {
            IconMaker iconMaker = new IconMaker(palette.ForegroundColor, palette.BackgroundColor);
            
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
}
