using System;
using System.Windows;

namespace HospitalQuality.Services
{
    public class ThemeService
    {
        private bool _isDarkTheme;

        public bool IsDarkTheme
        {
            get { return _isDarkTheme; }
        }

        public void SetDarkTheme(bool isDarkTheme)
        {
            if (_isDarkTheme == isDarkTheme)
            {
                return;
            }

            _isDarkTheme = isDarkTheme;
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            var resources = Application.Current.Resources.MergedDictionaries;

            if (_isDarkTheme)
            {
                resources[0] = new ResourceDictionary
                {
                    Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
                };
            }
            else
            {
                resources[0] = new ResourceDictionary
                {
                    Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative)
                };
            }
        }
    }
}