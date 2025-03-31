using HarmonyLib;
using SFS.World;

namespace VanillaUpgrades.World.Patches
{
    [HarmonyPatch(typeof(EffectManager))]
    public class StopExplosions
    {
        [HarmonyPatch("CreateExplosion")]
        [HarmonyPatch("CreatePartOverheatEffect")]
        [HarmonyPrefix]
        private static bool Prefix()
        {
            return Config.settings.explosions;
        }
    }
}