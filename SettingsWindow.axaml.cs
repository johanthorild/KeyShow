using Avalonia.Controls;
using Avalonia.Interactivity;

namespace KeyShow;

public partial class SettingsWindow : Window
{
    private readonly Settings _settings;
    private TextBox? _displayTimeTextBox;
    private ComboBox? _positionComboBox;

    public SettingsWindow() : this(Settings.Load()) { }

    public SettingsWindow(Settings settings)
    {
        InitializeComponent();
        _settings = settings;
        _displayTimeTextBox = this.FindControl<TextBox>("DisplayTimeTextBox");
        _positionComboBox = this.FindControl<ComboBox>("PositionComboBox");

        if (_displayTimeTextBox != null)
            _displayTimeTextBox.Text = _settings.DisplayTimeMs.ToString();

        if (_positionComboBox?.Items != null)
        {
            foreach (var raw in _positionComboBox.Items)
            {
                if (raw is ComboBoxItem item && item.Content?.ToString() == _settings.Position)
                {
                    _positionComboBox.SelectedItem = item;
                    break;
                }
            }
        }
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        if (int.TryParse(_displayTimeTextBox?.Text ?? string.Empty, out int ms))
            _settings.DisplayTimeMs = ms;

        if (_positionComboBox?.SelectedItem is ComboBoxItem selectedItem)
            _settings.Position = selectedItem.Content?.ToString() ?? "TopLeft";

        try { _settings.Save(); } catch { }

        Close();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        if (int.TryParse(_displayTimeTextBox?.Text ?? string.Empty, out int ms))
            _settings.DisplayTimeMs = ms;

        if (_positionComboBox?.SelectedItem is ComboBoxItem selectedItem)
            _settings.Position = selectedItem.Content?.ToString() ?? "TopLeft";

        try { _settings.Save(); } catch { }

        Close();
    }
}
