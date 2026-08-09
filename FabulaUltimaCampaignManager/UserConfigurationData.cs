using Godot;

namespace FabulaUltimaGMTool
{
    public partial class UserConfigurationData : Resource
    {
        [Export]
        public string CurrentCampaignID { get; set; }

        [Export]
        public bool BackgroundMusicEnabled { get; set; }

        [Export]
        public float UiScale { get; set; } = 1f;

        [Export]
        public string[] InstanceNames { get; set; }
    }
}
