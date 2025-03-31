using HarmonyLib;
using SFS.World;

namespace VanillaUpgrades.World.Patches
{
    [HarmonyPatch(typeof(ThrottleDrawer))]
    public class ManageHoverMode
    {
        [HarmonyPatch("ToggleThrottle")]
        [HarmonyPostfix]
        public static void ToggleThrottle_Postfix(ref Throttle_Local ___throttle)
        {
            if (!___throttle.Value.throttleOn)
                // Do this only if throttle has been turned off (keep hover mode on if this was set before throttle is turned on)
                HoverHandler.EnableHoverMode(false, false);
        }

        [HarmonyPatch("AdjustThrottleRaw")]
        [HarmonyPostfix]
        public static void AdjustThrottleRaw_Postfix()
        {
            HoverHandler.EnableHoverMode(false);
        }

        [HarmonyPatch("SetThrottleRaw")]
        [HarmonyPostfix]
        public static void SetThrottleRaw_Postfix()
        {
            HoverHandler.EnableHoverMode(false);
        }
    }
}