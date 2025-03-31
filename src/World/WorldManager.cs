using System;
using HarmonyLib;
using SFS.World;
using static VanillaUpgrades.HoverHandler;

namespace VanillaUpgrades
{
    // Hook for things that need to run every frame in-game.
    [HarmonyPatch(typeof(GameManager))]
    public class UpdateInGame
    {
        public static Action execute = () => {};

        [HarmonyPatch("Update")]
        public static void Postfix()
        {
            execute.Invoke();
        }
    }
    
    internal static partial class WorldManager
    {
        public static Rocket currentRocket;

        private static void UpdatePlayer()
        {
            currentRocket = PlayerController.main.player.Value != null
                ? PlayerController.main.player.Value as Rocket
                : null;
            ToggleTorque.Set(false);
            EnableHoverMode(false, false);
        }

        public static void Setup()
        {
            UpdatePlayer();
            HideTopLeftButtonText();
            
            PlayerController.main.player.OnChange += UpdatePlayer;
            Config.settings.allowTimeSlowdown.OnChange += TimeManipulation.ToggleChange;
            Config.settings.hideTopLeftButtonText.OnChange += HideTopLeftButtonText;
            
            AdvancedInfo.Setup();
            WorldClockDisplay.Setup();
            
            UpdateInGame.execute += () =>
            {
                if (hoverMode) TwrTo1();
            };
            
            MissionLogButton.Create();
        }
    }
}