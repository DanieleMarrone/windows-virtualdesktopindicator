using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualDesktopIndicator.Helpers;

namespace VirtualDesktopIndicator
{
    public class ThemeMonitor : IDisposable
    {
        private const string RegistryThemeDataPath =
            @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        public Theme CurrentTheme { get; private set; }

        private RegistryMonitor registryMonitor;
        public event EventHandler<ThemeEventArgs> ThemeChanged;


        public ThemeMonitor()
        {
            InitRegistryMonitor();

            CurrentTheme = GetSystemTheme();
        }

        public void Start()
        {
            registryMonitor.Start();
        }

        public void Dispose()
        {
            StopRegistryMonitor();
        }

        private void InitRegistryMonitor()
        {
            registryMonitor = new RegistryMonitor(RegistryThemeDataPath);            

            registryMonitor.RegChanged += new EventHandler(OnRegistryChanged);
            registryMonitor.Error += new ErrorEventHandler(OnRegistryError);
        }

        private void StopRegistryMonitor()
        {
            if (registryMonitor == null)
                return;

            registryMonitor.Stop();
            registryMonitor.RegChanged -= new EventHandler(OnRegistryChanged);
            registryMonitor.Error -= new ErrorEventHandler(OnRegistryError);
            registryMonitor = null;
        }

        private Theme GetSystemTheme()
        {
            return (int)Registry.GetValue(RegistryThemeDataPath, "SystemUsesLightTheme", 0) == 1 ?
                        Theme.Light :
                        Theme.Dark;
        }

        private void OnRegistryChanged(object sender, EventArgs e)
        {
            Theme systemTheme = GetSystemTheme();
            if (systemTheme == CurrentTheme)
                return;

            CurrentTheme = systemTheme;
            OnThemeChanged(new ThemeEventArgs(CurrentTheme));
        }

        protected virtual void OnThemeChanged(ThemeEventArgs e)
        {
            ThemeChanged?.Invoke(this, e);
        }

        private void OnRegistryError(object sender, ErrorEventArgs e) => StopRegistryMonitor();

    }


    public enum Theme { Light, Dark }


    public class ThemeEventArgs : EventArgs
    {
        public ThemeEventArgs(Theme theme)
        {
            Theme = theme;
        }

        public Theme Theme { get; }
    }

}
