using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HospitalQuality.Services;

namespace HospitalQuality
{
    public partial class MainWindow : Window
    {
        private readonly SettingsService _settingsService;
        private readonly ThemeService _themeService;
        private readonly ToastService _toastService;

        private AppSettings _currentSettings;

        private bool _isInitializing;
        private bool _isThemeAnimating;

        public MainWindow()
        {
            _settingsService = new SettingsService();
            _themeService = new ThemeService();

            _currentSettings = _settingsService.Load();

            InitializeComponent();

            _toastService = new ToastService(ToastHost);

            _isInitializing = true;

            ThemeToggle.IsChecked = _currentSettings.IsDarkTheme;

            if (_currentSettings.IsSettingsSectionExpanded)
            {
                SettingsSectionToggle.IsChecked = true;
                SettingsSectionContent.Visibility = Visibility.Visible;
                SettingsSectionContent.Opacity = 1;
                SettingsChevronRotation.Angle = 180;
            }

            _isInitializing = false;

            _themeService.SetDarkTheme(_currentSettings.IsDarkTheme);

            ThemeModeText.Text = _currentSettings.IsDarkTheme ? "حالت تاریک" : "حالت روشن";

            if (_currentSettings.IsDarkTheme)
            {
                AnimateToggleThumb(31, immediate: true);
            }

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _toastService.ShowToast("پوسته گرافیکی با موفقیت بارگذاری شد.", ToastType.Success);
        }

        private void ThemeToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            ThemeModeText.Text = "حالت تاریک";
            AnimateToggleThumb(31, immediate: false);
            ApplyThemeWithCircularReveal(true);
        }

        private void ThemeToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            ThemeModeText.Text = "حالت روشن";
            AnimateToggleThumb(3, immediate: false);
            ApplyThemeWithCircularReveal(false);
        }

        private void AnimateToggleThumb(double targetCanvasLeft, bool immediate)
        {
            if (ThemeToggle == null || ThemeToggle.Template == null)
            {
                return;
            }

            var thumb = ThemeToggle.Template.FindName("ToggleThumb", ThemeToggle) as Border;

            if (thumb == null)
            {
                return;
            }

            thumb.BeginAnimation(Canvas.LeftProperty, null);

            if (immediate)
            {
                Canvas.SetLeft(thumb, targetCanvasLeft);
                return;
            }

            var animation = new DoubleAnimation
            {
                To = targetCanvasLeft,
                Duration = TimeSpan.FromMilliseconds(260),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            thumb.BeginAnimation(Canvas.LeftProperty, animation);
        }

        private void ApplyThemeWithCircularReveal(bool isDarkTheme)
        {
            if (_isThemeAnimating)
            {
                return;
            }

            SaveThemeSetting(isDarkTheme);

            if (ThemeRevealCanvas == null || ThemeRevealEllipse == null || ThemeRevealScale == null ||
                ThemeToggle == null || RootGrid == null)
            {
                _themeService.SetDarkTheme(isDarkTheme);
                return;
            }

            _isThemeAnimating = true;
            ThemeToggle.IsEnabled = false;

            ThemeRevealScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            ThemeRevealScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            ThemeRevealEllipse.BeginAnimation(UIElement.OpacityProperty, null);

            var targetColor = GetTargetBackgroundColor(isDarkTheme);

            var center = ThemeToggle.TranslatePoint(
                new Point(ThemeToggle.ActualWidth / 2, ThemeToggle.ActualHeight / 2),
                RootGrid
            );

            var baseSize = 100.0;

            ThemeRevealEllipse.Width = baseSize;
            ThemeRevealEllipse.Height = baseSize;

            Canvas.SetLeft(ThemeRevealEllipse, center.X - baseSize / 2);
            Canvas.SetTop(ThemeRevealEllipse, center.Y - baseSize / 2);

            ThemeRevealEllipse.Fill = new SolidColorBrush(targetColor);
            ThemeRevealEllipse.Visibility = Visibility.Visible;
            ThemeRevealEllipse.Opacity = 1;

            ThemeRevealScale.ScaleX = 0;
            ThemeRevealScale.ScaleY = 0;

            var maxX = Math.Max(center.X, RootGrid.ActualWidth - center.X);
            var maxY = Math.Max(center.Y, RootGrid.ActualHeight - center.Y);
            var maxDistance = Math.Sqrt(maxX * maxX + maxY * maxY);
            var targetScale = maxDistance / (baseSize / 2) + 0.2;

            var storyboard = new Storyboard();

            var scaleX = new DoubleAnimation(0, targetScale, TimeSpan.FromMilliseconds(520));
            scaleX.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };

            var scaleY = new DoubleAnimation(0, targetScale, TimeSpan.FromMilliseconds(520));
            scaleY.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };

            Storyboard.SetTarget(scaleX, ThemeRevealEllipse);
            Storyboard.SetTargetProperty(scaleX, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));

            Storyboard.SetTarget(scaleY, ThemeRevealEllipse);
            Storyboard.SetTargetProperty(scaleY, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

            storyboard.Completed += (sender, args) =>
            {
                _themeService.SetDarkTheme(isDarkTheme);

                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(240));
                fadeOut.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };

                fadeOut.Completed += (fadeOutSender, fadeOutArgs) =>
                {
                    ThemeRevealEllipse.Visibility = Visibility.Collapsed;
                    _isThemeAnimating = false;
                    ThemeToggle.IsEnabled = true;
                };

                ThemeRevealEllipse.BeginAnimation(OpacityProperty, fadeOut);
            };

            storyboard.Children.Add(scaleX);
            storyboard.Children.Add(scaleY);

            storyboard.Begin();
        }

        private Color GetTargetBackgroundColor(bool isDarkTheme)
        {
            if (isDarkTheme)
            {
                return Color.FromRgb(2, 6, 23);
            }

            return Color.FromRgb(241, 245, 249);
        }

        private void SaveThemeSetting(bool isDarkTheme)
        {
            _currentSettings.IsDarkTheme = isDarkTheme;

            Task.Run(() =>
            {
                _settingsService.Save(_currentSettings);
            });
        }

        private void SaveSettingsSectionState(bool isExpanded)
        {
            _currentSettings.IsSettingsSectionExpanded = isExpanded;

            Task.Run(() =>
            {
                _settingsService.Save(_currentSettings);
            });
        }

        private void SettingsSectionToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            SaveSettingsSectionState(true);
            AnimateSettingsSection(expand: true);
        }

        private void SettingsSectionToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            SaveSettingsSectionState(false);
            AnimateSettingsSection(expand: false);
        }

        private void AnimateSettingsSection(bool expand)
        {
            if (SettingsSectionContent == null || SettingsChevronRotation == null)
            {
                return;
            }

            var chevronAnimation = new DoubleAnimation
            {
                To = expand ? 180 : 0,
                Duration = TimeSpan.FromMilliseconds(260),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            SettingsChevronRotation.BeginAnimation(RotateTransform.AngleProperty, chevronAnimation);

            if (expand)
            {
                SettingsSectionContent.Visibility = Visibility.Visible;

                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(260),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                SettingsSectionContent.BeginAnimation(OpacityProperty, fadeIn);
            }
            else
            {
                var fadeOut = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(220),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };

                fadeOut.Completed += (sender, args) =>
                {
                    SettingsSectionContent.Visibility = Visibility.Collapsed;
                };

                SettingsSectionContent.BeginAnimation(OpacityProperty, fadeOut);
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (MaximizeIcon == null || RestoreIcon == null)
            {
                return;
            }

            if (WindowState == WindowState.Maximized)
            {
                MaximizeIcon.Visibility = Visibility.Collapsed;
                RestoreIcon.Visibility = Visibility.Visible;
            }
            else
            {
                MaximizeIcon.Visibility = Visibility.Visible;
                RestoreIcon.Visibility = Visibility.Collapsed;
            }
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            SetPage("داشبورد", "نمای کلی پوسته گرافیکی و وضعیت ظاهری برنامه");
            _toastService.ShowToast("داشبورد انتخاب شد.", ToastType.Success);
        }

        private void HospitalInfoButton_Click(object sender, RoutedEventArgs e)
        {
            SetPage("اطلاعات بیمارستان", "این بخش در مراحل بعدی فعال خواهد شد");
            _toastService.ShowToast("بخش اطلاعات بیمارستان در مراحل بعدی فعال می‌شود.", ToastType.Info);
        }

        private void PersonnelButton_Click(object sender, RoutedEventArgs e)
        {
            SetPage("مدیریت پرسنل", "این بخش در مراحل بعدی فعال خواهد شد");
            _toastService.ShowToast("بخش مدیریت پرسنل در مراحل بعدی فعال می‌شود.", ToastType.Info);
        }

        private void UnitsButton_Click(object sender, RoutedEventArgs e)
        {
            SetPage("واحدها / بخش‌ها", "این بخش در مراحل بعدی فعال خواهد شد");
            _toastService.ShowToast("بخش واحدها / بخش‌ها در مراحل بعدی فعال می‌شود.", ToastType.Info);
        }

        private void SetPage(string title, string subtitle)
        {
            HeaderTitle.Text = title;
            HeaderSubtitle.Text = subtitle;
        }
    }
}