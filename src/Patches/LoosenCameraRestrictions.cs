﻿using HarmonyLib;
using SFS.World;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

namespace VanillaUpgrades.Patches
{
    [HarmonyPatch(typeof(PlayerController))]
    internal class LoosenCameraRestrictions
    {
        [HarmonyPatch(typeof(PlayerController), "ClampTrackingOffset")]
        [HarmonyPrefix]
        private static bool MoreCameraMove(ref Vector2 __result, Vector2 newValue)
        {
            if (!Config.settings.moreCameraMove) return true;
            if (PlayerController.main.player.Value == null) return true;
            PlayerController.main.player.Value.ClampTrackingOffset(ref newValue, -30);
            __result = newValue;
            return false;
        }

        [HarmonyPatch("ClampCameraDistance")]
        [HarmonyPrefix]
        private static bool MoreCameraZoom(ref float __result, float newValue)
        {
            if (!Config.settings.moreCameraZoom) return true;
            if (PlayerController.main.player.Value == null) return true;
            __result = Mathf.Clamp(newValue, 0.05f, 2.5E+12f);
            return false;
        }
    }
}