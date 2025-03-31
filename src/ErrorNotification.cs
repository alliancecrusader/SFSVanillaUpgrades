using System;
using System.Text;
using SFS.UI;
using UnityEngine;

namespace VanillaUpgrades
{
    public class ErrorNotification : MonoBehaviour
    {
        private static readonly StringBuilder Errors = new();

        private void Start()
        {
            if (Errors.Length == 0) return;
            Errors.Insert(0,
                "An error occured while loading VanillaUpgrades." + Environment.NewLine + Environment.NewLine);
            Menu.read.ShowReport(Errors, () => Errors.Clear());
        }

        public static void Error(string error)
        {
            Errors.AppendLine($"- {error}");
        }
    }
}