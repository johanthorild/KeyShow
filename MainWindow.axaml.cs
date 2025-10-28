using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KeyShow;

public partial class MainWindow : Window
{
        private readonly Border _border;
        private readonly TextBlock _text;
        private readonly Settings _settings = new();
        private readonly IKeyboardListener _keyboardListener;

        public MainWindow()
        {
                InitializeComponent();

                _border = this.FindControl<Border>("KeyBorder")!;
                _text = this.FindControl<TextBlock>("KeyText")!;

#if WINDOWS
        _keyboardListener = new WindowsKeyboardListener();
#else
                _keyboardListener = new DummyKeyboardListener();
#endif
                _keyboardListener.OnKeyPressed += ShowKey;
                _keyboardListener.Start();

                ApplyPosition();
        }

        private void InitializeComponent()
        {
                Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }

        private string NormalizeModifiers(KeyModifiers mods)
        {
                var parts = new List<string>();
                if (mods.HasFlag(KeyModifiers.Control)) parts.Add("CTRL");
                if (mods.HasFlag(KeyModifiers.Alt)) parts.Add("ALT");
                if (mods.HasFlag(KeyModifiers.Shift)) parts.Add("SHIFT");
                return string.Join(" + ", parts);
        }

        private async void ShowKey(KeyInfo keyInfo)
        {
                string modifiers = NormalizeModifiers(keyInfo.Modifiers);
                string displayText = !string.IsNullOrEmpty(modifiers)
                    ? $"{modifiers} + {keyInfo.KeyName}"
                    : keyInfo.KeyName;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                        _text.Text = displayText;
                        _border.Opacity = 1;
                });

                await Task.Delay(_settings.DisplayTimeMs);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                        _text.Text = string.Empty;
                        _border.Opacity = 0;
                });
        }

        private async void OpenSettings_Click(object? sender, RoutedEventArgs e)
        {
                var win = new SettingsWindow(_settings);
                await win.ShowDialog(this);

                // Flytta fönstret efter att inställningarna ändrats
                ApplyPosition();
        }

        private void ApplyPosition()
        {
                var screen = Screens.Primary;
                var workArea = screen.WorkingArea;

                double windowWidth = Width;
                double windowHeight = Height;

                int x = 0, y = 0;

                switch (_settings.Position)
                {
                        case "TopLeft":
                                x = workArea.X;
                                y = workArea.Y;
                                break;
                        case "TopRight":
                                x = workArea.X + (int)(workArea.Width - windowWidth);
                                y = workArea.Y;
                                break;
                        case "BottomLeft":
                                x = workArea.X;
                                y = workArea.Y + (int)(workArea.Height - windowHeight);
                                break;
                        case "BottomRight":
                        default:
                                x = workArea.X + (int)(workArea.Width - windowWidth);
                                y = workArea.Y + (int)(workArea.Height - windowHeight);
                                break;
                }

                Position = new PixelPoint(x, y);
        }
}
