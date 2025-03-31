using SFS.UI;
using SFS.UI.ModGUI;
using TMPro;
using UITools;
using UnityEngine;
using UnityEngine.UI;
using VanillaUpgrades.Utility;
using UIExtensions = VanillaUpgrades.Utility.UIExtensions;

namespace VanillaUpgrades
{
    partial class AdvancedInfo
    {
        private const string PositionKey = "VU.AdvancedInfoWindow";
        // --------------------------------------------------------------------
        // SEPARATE WINDOW UI CREATION
        // --------------------------------------------------------------------
        private static void CreateWindowUI()
        {
            // Create the separate window using your existing factory
            advancedInfoWindow = Builder.CreateWindow(
                parent:        windowHolder.transform,
                ID:            1200,
                width:         220,   // default vertical size
                height:        350,
                posX:          0,
                posY:          0,
                draggable:     true,
                savePosition:  true,
                opacity:       1f,
                titleText:     "Advanced Info"
            );
            
            advancedInfoWindow.RegisterOnDropListener(advancedInfoWindow.ClampWindow);
            advancedInfoWindow.RegisterPermanentSaving(PositionKey);
            
            // The window typically has a "Title" child; if you want to forcibly set its height:
            if (advancedInfoWindow.rectTransform.Find("Title") is RectTransform titleRect)
            {
                titleRect.sizeDelta = new Vector2(titleRect.sizeDelta.x, 30);
            }

            // Create a main vertical layout group on the window so its contents
            // are placed below the title. We add some top padding to avoid overlap.
            advancedInfoWindow.CreateLayoutGroup(
                type: Type.Vertical,
                childAlignment:  TextAnchor.UpperLeft,
                spacing:    0,
                padding:    new RectOffset(0, 0, 0, 0) // top=30 for title bar
            );

            // Now create the sub-containers for vertical/horizontal modes
            verticalContainer = Builder.CreateContainer(advancedInfoWindow);
            verticalContainer.CreateLayoutGroup(
                type: Type.Vertical,
                childAlignment:  TextAnchor.UpperLeft,
                spacing:    0,
                padding:    new RectOffset(0, 0, 0, 0)
            );

            horizontalContainer = Builder.CreateContainer(advancedInfoWindow);
            horizontalContainer.CreateLayoutGroup(
                type: Type.Vertical,
                childAlignment:  TextAnchor.UpperLeft,
                spacing:    10,
                padding:    new RectOffset(0, 0, 0, 0)
            );

            BuildVerticalUI(verticalContainer);
            BuildHorizontalUI(horizontalContainer);

            advancedInfoWindow.ScaleWindow();
            advancedInfoWindow.ClampWindow();

            // Finally, pick horizontal or vertical layout based on user config
            CheckHorizontalToggle();
        }

        private static void BuildVerticalUI(Container parent)
        {
            // If you want a small visual line at top, keep it minimal:
            Builder.CreateSeparator(parent, 210);
            Builder.CreateSpace(parent, 1, 10);

            // Field blocks for each advanced info
            UIExtensions.AlignedLabel(parent, 140, 30, "Apoapsis:");
            Label apo = UIExtensions.AlignedLabel(parent, 175, 30);
            DisplayMap["Apoapsis"].vertical = apo;
            Builder.CreateSpace(parent, 1, 10);


            UIExtensions.AlignedLabel(parent, 140, 30, "Periapsis:");
            Label peri = UIExtensions.AlignedLabel(parent, 175, 30);
            DisplayMap["Periapsis"].vertical = peri;
            Builder.CreateSpace(parent, 1, 10);


            UIExtensions.AlignedLabel(parent, 140, 30, "Eccentricity:");
            Label ecc = UIExtensions.AlignedLabel(parent, 175, 30);
            DisplayMap["Eccentricity"].vertical = ecc;
            Builder.CreateSpace(parent, 1, 10);


            var angleTitleLabel = UIExtensions.AlignedLabel(parent, 140, 30, "Angle:");
            // Value label
            var angleValueLabel = UIExtensions.AlignedLabel(parent, 175, 30);

            // Hook them up so the refactoring code can update each
            DisplayMap["AngleTitle"].vertical = angleTitleLabel;
            DisplayMap["Angle"].vertical      = angleValueLabel;
        }

        private static void BuildHorizontalUI(Container parent)
        {
            // Minimal separator if needed:
            Builder.CreateSeparator(parent, 370);

            // 1) Apoapsis row
            Container apoRow = Builder.CreateContainer(parent);
            apoRow.CreateLayoutGroup(Type.Horizontal, TextAnchor.UpperLeft, spacing: 5);
            UIExtensions.AlignedLabel(apoRow, 150, 30, "Apoapsis:");
            Label apo = UIExtensions.AlignedLabel(apoRow, 175, 30);
            DisplayMap["Apoapsis"].horizontal = apo;

            // 2) Periapsis row
            Container periRow = Builder.CreateContainer(parent);
            periRow.CreateLayoutGroup(Type.Horizontal, TextAnchor.UpperLeft, spacing: 5);
            UIExtensions.AlignedLabel(periRow, 150, 30, "Periapsis:");
            Label peri = UIExtensions.AlignedLabel(periRow, 175, 30);
            DisplayMap["Periapsis"].horizontal = peri;

            // 3) Eccentricity row
            Container eccRow = Builder.CreateContainer(parent);
            eccRow.CreateLayoutGroup(Type.Horizontal, TextAnchor.UpperLeft, spacing: 5);
            UIExtensions.AlignedLabel(eccRow, 150, 30, "Eccentricity:");
            Label ecc = UIExtensions.AlignedLabel(eccRow, 175, 30);
            DisplayMap["Eccentricity"].horizontal = ecc;

            // 4) Angle row
            Container angleRow = Builder.CreateContainer(parent);
            angleRow.CreateLayoutGroup(Type.Horizontal, TextAnchor.UpperLeft, spacing: 5);
            
            // Title label
            var angleTitleLabel = UIExtensions.AlignedLabel(angleRow, 150, 30, "Angle:");
            // Value label
            var angleValueLabel = UIExtensions.AlignedLabel(angleRow, 175, 30);

            // Hook them up so the refactoring code can update each
            DisplayMap["AngleTitle"].horizontal = angleTitleLabel;
            DisplayMap["Angle"].horizontal      = angleValueLabel;
        }

        // --------------------------------------------------------------------
        // VANILLA (INLINE) UI CREATION
        // --------------------------------------------------------------------
        private static void SetupVanillaUI()
        {
            GameObject thrust    = GameObject.Find("Thrust (1)");
            GameObject separator = GameObject.Find("Separator (1)");
            if (thrust == null || separator == null) return;

            GameObject holder = thrust.transform.parent.gameObject;

            string[] fieldOrder = { "Apoapsis", "Periapsis", "Eccentricity", "Angle" };
            foreach (var key in fieldOrder)
            {
                // Insert a line
                GameObject sep = Object.Instantiate(separator, holder.transform, true);
                VanillaInfoObjects.Add(sep);

                // Insert a copy of "thrust"
                GameObject row = Object.Instantiate(thrust, holder.transform, true);
                row.name = key;
                
                // Disable layout controlling sizes of Apoapsis and Periapsis so large values aren't squished
                if (key is "Apoapsis" or "Periapsis")
                {
                    var rect = row.transform.GetChild(0).GetComponent<RectTransform>();
                    row.GetComponent<VerticalLayoutGroup>().childControlWidth = false;
                    rect.sizeDelta = new Vector2(150, rect.sizeDelta.y);
                    row.transform.GetChild(1).GetComponent<TextMeshProUGUI>().autoSizeTextContainer = true;
                }
                
                VanillaInfoObjects.Add(row);

                var titleText  = row.transform.GetChild(0).GetComponent<TextAdapter>();
                var valueText = row.transform.GetChild(1).GetComponent<TextAdapter>();

                // Set the top label to the key
                titleText.Text = key;

                if (key == "Angle")
                {
                    if (DisplayMap["AngleTitle"] != null)
                        DisplayMap["AngleTitle"].vanillaTitle = titleText;  // for "Local Angle" etc.
                    if (DisplayMap["Angle"] != null)
                        DisplayMap["Angle"].vanilla = valueText;
                }
                else
                {
                    DisplayMap[key].vanilla = valueText;
                }
            }
        }
    }
}