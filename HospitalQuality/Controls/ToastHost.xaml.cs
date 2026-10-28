using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using HospitalQuality.Services;

namespace HospitalQuality.Controls
{
    public partial class ToastHost : UserControl
    {
        private const int MaxVisibleToasts = 4;
        private const double SwipeDistance = -420;

        public ToastHost()
        {
            InitializeComponent();
        }

        public void Show(string message, ToastType type)
        {
            TrimExcessToasts();

            var brush = GetToastBrush(type);
            var transform = new TranslateTransform();

            var border = new Border
            {
                Background = brush,
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 8, 0, 0),
                MinWidth = 330,
                MaxWidth = 440,
                Opacity = 0,
                RenderTransform = transform,
                Effect = new DropShadowEffect
                {
                    BlurRadius = 24,
                    ShadowDepth = 5,
                    Direction = 270,
                    Opacity = 0.25,
                    Color = Colors.Black
                }
            };

            var content = CreateToastContent(message, type, border);
            border.Child = content;

            ToastItems.Items.Add(border);

            PlayShowAnimation(border, transform);
            StartAutoClose(border);
        }

        private Grid CreateToastContent(string message, ToastType type, Border owner)
        {
            var grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var iconBadge = new Border
            {
                Width = 30,
                Height = 30,
                CornerRadius = new CornerRadius(10),
                Background = new SolidColorBrush(Color.FromArgb(51, 255, 255, 255)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0)
            };

            var icon = new TextBlock
            {
                Text = GetIcon(type),
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            iconBadge.Child = icon;

            var messageText = new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = Application.Current.TryFindResource("BodyFontFamily") as FontFamily
            };

            var close = new TextBlock
            {
                Text = "✕",
                Foreground = new SolidColorBrush(Color.FromArgb(225, 255, 255, 255)),
                Cursor = Cursors.Hand,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0)
            };

            close.MouseLeftButtonUp += (sender, args) => Hide(owner);

            Grid.SetColumn(iconBadge, 0);
            Grid.SetColumn(messageText, 1);
            Grid.SetColumn(close, 2);

            grid.Children.Add(iconBadge);
            grid.Children.Add(messageText);
            grid.Children.Add(close);

            return grid;
        }

        private string GetIcon(ToastType type)
        {
            switch (type)
            {
                case ToastType.Success:
                    return "✓";

                case ToastType.Warning:
                    return "!";

                case ToastType.Error:
                    return "✕";

                default:
                    return "i";
            }
        }

        private Brush GetToastBrush(ToastType type)
        {
            string key;

            switch (type)
            {
                case ToastType.Success:
                    key = "SuccessBrush";
                    break;

                case ToastType.Warning:
                    key = "WarningBrush";
                    break;

                case ToastType.Error:
                    key = "ErrorBrush";
                    break;

                default:
                    key = "InfoBrush";
                    break;
            }

            return Application.Current.TryFindResource(key) as Brush ?? Brushes.DimGray;
        }

        private void PlayShowAnimation(Border border, TranslateTransform transform)
        {
            var storyboard = new Storyboard();

            var slide = new DoubleAnimation
            {
                From = SwipeDistance,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(slide, transform);
            Storyboard.SetTargetProperty(slide, new PropertyPath(TranslateTransform.XProperty));

            var fade = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(240)
            };

            Storyboard.SetTarget(fade, border);
            Storyboard.SetTargetProperty(fade, new PropertyPath(UIElement.OpacityProperty));

            storyboard.Children.Add(slide);
            storyboard.Children.Add(fade);

            storyboard.Begin();
        }

        private void StartAutoClose(Border border)
        {
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };

            timer.Tick += (sender, args) =>
            {
                timer.Stop();
                Hide(border);
            };

            border.Tag = timer;
            timer.Start();
        }

        private void Hide(Border border)
        {
            if (border.Tag as string == "Closing")
            {
                return;
            }

            if (border.Tag is DispatcherTimer timer)
            {
                timer.Stop();
            }

            border.Tag = "Closing";

            if (!(border.RenderTransform is TranslateTransform transform))
            {
                ToastItems.Items.Remove(border);
                return;
            }

            var storyboard = new Storyboard();

            var slide = new DoubleAnimation
            {
                To = SwipeDistance,
                Duration = TimeSpan.FromMilliseconds(260),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            Storyboard.SetTarget(slide, transform);
            Storyboard.SetTargetProperty(slide, new PropertyPath(TranslateTransform.XProperty));

            var fade = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(220)
            };

            Storyboard.SetTarget(fade, border);
            Storyboard.SetTargetProperty(fade, new PropertyPath(UIElement.OpacityProperty));

            storyboard.Completed += (sender, args) => ToastItems.Items.Remove(border);

            storyboard.Children.Add(slide);
            storyboard.Children.Add(fade);

            storyboard.Begin();
        }

        private void TrimExcessToasts()
        {
            while (ToastItems.Items.Count >= MaxVisibleToasts)
            {
                var oldest = ToastItems.Items[0] as Border;

                if (oldest == null)
                {
                    ToastItems.Items.RemoveAt(0);
                    continue;
                }

                if (oldest.Tag is DispatcherTimer timer)
                {
                    timer.Stop();
                }

                ToastItems.Items.RemoveAt(0);
            }
        }
    }
}