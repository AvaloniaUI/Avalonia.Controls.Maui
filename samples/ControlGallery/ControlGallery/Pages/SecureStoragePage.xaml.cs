using Microsoft.Maui.Storage;

namespace ControlGallery.Pages;

public partial class SecureStoragePage : ContentPage
{
    const string TimestampKey = "demo_timestamp";

    public SecureStoragePage()
    {
        InitializeComponent();
        LoadTimestamp();
    }

    async void LoadTimestamp()
    {
        try
        {
            var timestamp = await SecureStorage.Default.GetAsync(TimestampKey);
            if (timestamp is not null)
                PersistenceResultLabel.Text = $"Saved: {timestamp}";
        }
        catch
        {
            // ignore load errors on startup
        }
    }

    async void OnSaveClicked(object? sender, EventArgs e)
    {
        var key = KeyEntry.Text;
        var value = ValueEntry.Text ?? "";

        if (string.IsNullOrWhiteSpace(key))
        {
            ResultLabel.Text = "Enter a key first";
            return;
        }

        try
        {
            await SecureStorage.Default.SetAsync(key, value);
            ResultLabel.Text = $"Value: {MaskValue(value)} (saved, {value.Length} chars)";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Error: {ex.Message}";
        }
    }

    async void OnLoadClicked(object? sender, EventArgs e)
    {
        var key = KeyEntry.Text;

        if (string.IsNullOrWhiteSpace(key))
        {
            ResultLabel.Text = "Enter a key first";
            return;
        }

        try
        {
            var value = await SecureStorage.Default.GetAsync(key);
            if (value is not null)
            {
                ValueEntry.Text = value;
                ResultLabel.Text = $"Value: {MaskValue(value)} ({value.Length} chars)";
            }
            else
            {
                ResultLabel.Text = $"Key '{key}' not found";
            }
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Error: {ex.Message}";
        }
    }

    void OnRemoveClicked(object? sender, EventArgs e)
    {
        var key = KeyEntry.Text;

        if (string.IsNullOrWhiteSpace(key))
        {
            ResultLabel.Text = "Enter a key first";
            return;
        }

        var removed = SecureStorage.Default.Remove(key);
        ResultLabel.Text = removed ? "Value: (removed)" : $"Key '{key}' not found";
        ValueEntry.Text = "";
    }

    async void OnSaveTimestampClicked(object? sender, EventArgs e)
    {
        var timestamp = DateTime.Now.ToString("G");
        try
        {
            await SecureStorage.Default.SetAsync(TimestampKey, timestamp);
            PersistenceResultLabel.Text = $"Saved: {timestamp}";
        }
        catch (Exception ex)
        {
            PersistenceResultLabel.Text = $"Error: {ex.Message}";
        }
    }

    async void OnLoadTimestampClicked(object? sender, EventArgs e)
    {
        try
        {
            var value = await SecureStorage.Default.GetAsync(TimestampKey);
            PersistenceResultLabel.Text = value is not null
                ? $"Saved: {value}"
                : "Saved: (none)";
        }
        catch (Exception ex)
        {
            PersistenceResultLabel.Text = $"Error: {ex.Message}";
        }
    }

    static string MaskValue(string value)
    {
        if (value.Length <= 4)
            return new string('*', value.Length);

        return string.Concat(value.AsSpan(0, 4), new string('*', Math.Min(value.Length - 4, 8)));
    }
}
