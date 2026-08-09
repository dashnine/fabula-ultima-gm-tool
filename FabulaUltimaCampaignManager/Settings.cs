using FabulaUltimaGMTool;
using FirstProject;
using Godot;

public partial class Settings : PopupMenu
{
    private UserConfigurationData _userConfiguration;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _userConfiguration = GetNode<UserConfigurationState>("/root/UserConfigurationState").UserConfigurationData;
        SetItemChecked(0, _userConfiguration.BackgroundMusicEnabled);
        CheckCurrentUiScale();
    }

    public void OnOptionPressed(int index)
    {
        var itemText = GetItemText(index);

        switch (itemText)
        {
            case "Play Background Music":
                _userConfiguration.BackgroundMusicEnabled = !_userConfiguration.BackgroundMusicEnabled;
                SetItemChecked(index, _userConfiguration.BackgroundMusicEnabled);
                ResourceExtensions.Save(_userConfiguration);
                break;
            case "UI Scale: 100%":
                SetUiScale(1f);
                break;
            case "UI Scale: 125%":
                SetUiScale(1.25f);
                break;
            case "UI Scale: 150%":
                SetUiScale(1.5f);
                break;
            case "UI Scale: 175%":
                SetUiScale(1.75f);
                break;
            case "UI Scale: 200%":
                SetUiScale(2f);
                break;
        }
    }

    private void SetUiScale(float scale)
    {
        _userConfiguration.UiScale = scale;
        ResourceExtensions.Save(_userConfiguration);
        CheckCurrentUiScale();
        GetNode<WindowScaler>("/root/WindowScaler").RescaleAll();
    }

    private void CheckCurrentUiScale()
    {
        var current = _userConfiguration.UiScale <= 0 ? 1f : _userConfiguration.UiScale;
        for (var index = 0; index < ItemCount; index++)
        {
            var text = GetItemText(index);
            if (!text.StartsWith("UI Scale: ")) continue;
            var percent = int.Parse(text.Replace("UI Scale: ", "").TrimEnd('%'));
            SetItemChecked(index, Mathf.IsEqualApprox(percent / 100f, current));
        }
    }
}
