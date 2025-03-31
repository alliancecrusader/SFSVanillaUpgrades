using HarmonyLib;
using SFS.Translations;
using SFS.UI;
using SFS.World;

namespace VanillaUpgrades.World.Patches
{
    [HarmonyPatch(typeof(Rocket), "CanTimewarp")]
    internal class PhysicsTimewarpIfTurning
    {
        private static void Postfix(ref bool __result)
        {
            if (WorldManager.currentRocket.arrowkeys.turnAxis == 0) return;
            WorldTime.ShowCannotTimewarpMsg(Field.Text("Cannot timewarp faster than %speed%x while turning"),
                MsgDrawer.main);
            __result = false;
        }
    }

    [HarmonyPatch(typeof(WorldTime))]
    public class RaiseMaxPhysicsTimewarp
    {
        [HarmonyPatch("GetTimewarpSpeed_Physics")]
        [HarmonyPrefix]
        public static bool AddMoreIndexes(ref double __result, int timewarpIndex_Physics)
        {
            if (!Config.settings.higherPhysicsWarp) return true;
            __result = new[] { 1, 2, 3, 5, 10, 25 }[timewarpIndex_Physics];
            return false;
        }

        [HarmonyPatch("MaxPhysicsIndex", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool AllowUsingIndexes(ref int __result)
        {
            if (!Config.settings.higherPhysicsWarp) return true;
            __result = 5;
            return false;
        }
    }

    [HarmonyPatch(typeof(FlightInfoDrawer), "Update")]
    internal class LimitDecimalsOfTimewarpText
    {
        private static void Postfix(TextAdapter ___timewarpText)
        {
            ___timewarpText.Text = WorldTime.main.timewarpSpeed.Round(2) + "x";
        }
    }
}