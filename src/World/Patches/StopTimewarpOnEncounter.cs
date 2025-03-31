using System.Linq;
using HarmonyLib;
using SFS.World;
using SFS.World.Maps;
using SFS.WorldBase;

namespace VanillaUpgrades.World.Patches
{
    [HarmonyPatch]
    public class StopTimewarpOnEncounter
    {
        private static TimewarpTo timewarpTo;

        [HarmonyPatch(typeof(TimewarpTo), "Start")]
        [HarmonyPrefix]
        private static void GetTimewarpTo(TimewarpTo __instance)
        {
            timewarpTo = __instance;
        }

        /*
         Extends SFS's built-in "stop warp at altitude" system to also handle crossing from the orbit planet's SOI
         into a child's SOI, if enabled in Config.
        */
        [HarmonyPatch(typeof(Orbit), nameof(Orbit.GetStopTimewarpTime))]
        public static class Patch_Orbit_GetStopTimewarpTime
        {
            private static void Postfix(
                Orbit __instance,
                double timeOld,
                double timeNew,
                ref double __result
            )
            {
                // 1) Check whether config setting is off, and if user is using Timewarp To. Don't run code if either.
                if (!Config.settings.stopTimewarpOnEncounter || timewarpTo.warp != null)
                    return;

                // 2) The base game might have found an altitude crossing at __result (< timeNew),
                //    or no crossing at all (>= timeNew). We'll only check up to 'earliestStop.'
                var earliestStop = __result < timeNew ? __result : timeNew;

                // 3) If we detect a SoI crossing earlier than 'earliestStop',
                //    we override __result so the game does a mid-frame stop at that time.
                var soiCrossTime = CheckParentToChildSoI(__instance, timeOld, earliestStop);
                if (soiCrossTime < earliestStop) __result = soiCrossTime;
            }

            /*
            Checks if we cross from 'orbit.Planet' to any of its satellites' SoI in the interval (timeOld, timeLimit).
            Returns crossing time or PositiveInfinity.
            */
            private static double CheckParentToChildSoI(Orbit orbit, double timeOld, double timeLimit)
            {
                Planet parent = orbit.Planet;
                if (parent == null || parent.satellites == null || parent.satellites.Length == 0)
                    return double.PositiveInfinity;

                // Evaluate each child planet (satellite)
                return parent.satellites.Select(child => CheckSoICrossing(orbit, child, timeOld, timeLimit))
                    .Prepend(double.PositiveInfinity).Min();
            }

            /*
            Detects crossing of the child's SoI boundary in [timeOld, timeLimit].
            If found, returns that crossing time via binary search; else PositiveInfinity.
            */
            private static double CheckSoICrossing(Orbit orbit, Planet child, double timeOld, double timeLimit)
            {
                var wasInside = IsInsideSoI(orbit, child, timeOld);
                var isInside = IsInsideSoI(orbit, child, timeLimit);

                // No crossing if inside status didn't change
                if (wasInside == isInside)
                    return double.PositiveInfinity;

                // Binary search
                var low = timeOld;
                var high = timeLimit;

                for (var i = 0; i < 30; i++)
                {
                    var mid = (low + high) * 0.5;
                    var midInside = IsInsideSoI(orbit, child, mid);

                    if (midInside == wasInside)
                        low = mid;
                    else
                        high = mid;

                    if (high - low < 1e-5)
                        break;
                }

                return high;
            }


            /*
            True if the rocket is inside 'childPlanet' SoI at a given time, using orbit.GetLocation(time) to
            get rocket's local position, then offsetting by the orbit planet's global position.
            */

            private static bool IsInsideSoI(Orbit orbit, Planet childPlanet, double time)
            {
                // Acquire rocket's location at 'time'
                Location loc = orbit.GetLocation(time);

                // orbit.GetLocation(time).position is the rocket position *relative* to orbit.Planet
                Planet orbitPlanet = orbit.Planet;
                if (orbitPlanet == null)
                    return false;

                // Rocket's global position = planet's global pos + local offset
                Double2 planetGlobalPos = orbitPlanet.GetSolarSystemPosition(time);
                Double2 rocketGlobalPos = planetGlobalPos + loc.position;

                // Child planet's global position
                Double2 childGlobalPos = childPlanet.GetSolarSystemPosition(time);

                var dist = (rocketGlobalPos - childGlobalPos).magnitude;
                return dist < childPlanet.SOI;
            }
        }
    }
}