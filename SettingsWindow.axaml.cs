using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace KeyShow;

public partial class SettingsWindow : Window
{
    private readonly Settings _settings;
    private TextBox? _displayTimeTextBox;
    private ComboBox? _positionComboBox;

    public SettingsWindow(Settings settings)
    {
        InitializeComponent();
        _settings = settings;
        _displayTimeTextBox = this.FindControl<TextBox>("DisplayTimeTextBox");
        _positionComboBox = this.FindControl<ComboBox>("PositionComboBox");

        _displayTimeTextBox.Text = _settings.DisplayTimeMs.ToString();

        foreach (ComboBoxItem item in _positionComboBox.Items)
        {
            if (item.Content?.ToString() == _settings.Position)
            {
                _positionComboBox.SelectedItem = _settings.Position;
                break;
            }
        }
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        if (int.TryParse(_displayTimeTextBox.Text, out int ms))
            _settings.DisplayTimeMs = ms;

        if (_positionComboBox.SelectedItem is ComboBoxItem selectedItem)
            _settings.Position = selectedItem.Content?.ToString() ?? "BottomRight";

        Close();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
