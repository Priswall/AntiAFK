using Exiled.API.Interfaces;
using System.ComponentModel;

namespace AntiAFK
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        [Description("Sets the time (in seconds) that players are allowed to be AFK before getting booted")]
        public int AfkTime { get; set; } = 300;

        [Description("Toggles whether or not a kicked player should be replaced by a spectating one")]
        public bool ShouldBeReplaced = true;

        [Description("Sets the message displayed on a users screen when they replace an AFK player")]
        public string AfkReplaceBroadcast { get; set; } = "A player was AFK, so you took their place";

        [Description("Sets the number of warnings given. Set to 0 if no warnings should be given")]
        public ushort NumWarnings { get; set; } = 2;

        [Description("Sets the amount of time before the player is kicked that a warning should appear. Each warning happens this amount of time before the next\n  # I.E Having 2 warnings with 60 seconds gives a warning 60 seconds and 120 seconds before the play would be kicked")]
        public ushort TimeAfterWarning { get; set; } = 60;

        [Description("Sets how long the warning broadcast should be displayed on the user's screen")]
        public ushort WarningBroadcastDuration = 5;

        [Description("Sets the warning message to be displayed on the player's screen.\n  # %afk% will be replaced with the amount of time (in seconds) the player has been AFK. %seconds% will be replaced with how long the player has before geting kicked in seconds.\n  # %minutes% will be replaced with how long the player has before getting kicked in minutes")]
        public string WarningMessage { get; set; } = "You will be kicked in %minutes% minute(s) if you do not move";

        [Description("Sets the length of the kick countdown. Set to 0 if there should be no countdown")]
        public ushort CountdownLength { get; set; } = 0;

        [Description("Sets the countdown message. %time% will be replaced with how long the player has before getting kicked in seconds")]
        public string CountdownMessage { get; set; } = "<color='red'>Move now or you will be kicked in %time% second(s)</color>";
    }
}
