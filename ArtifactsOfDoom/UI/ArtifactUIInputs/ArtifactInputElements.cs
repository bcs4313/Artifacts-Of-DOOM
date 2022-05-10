using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.ComponentModel;
using static On.RoR2.UI.CurrentRunArtifactDisplayDataDriver;

namespace ArtifactsOfDoom
{
    public class textBox
    {
        public void Start()
        {
            Debug.Log("Textbox object initiated");
            // constructor hook for artifact display panel. We will attach our textbox from here
            On.RoR2.UI.CurrentRunArtifactDisplayDataDriver.OnEnable += (orig_OnEnable orig, global::RoR2.UI.CurrentRunArtifactDisplayDataDriver self) =>
            {
                orig.Invoke(self);
                Debug.Log("Textbox injection attempted");
                // get display panel controller

                var obj = (RoR2.UI.ArtifactDisplayPanelController)RoR2.UI.ArtifactDisplayPanelController.FindObjectOfType<RoR2.UI.ArtifactDisplayPanelController>();
                var boxAllocator = obj.panelObject.AddComponent<SettingsTextBox>();
                boxAllocator.nameToken = "Artifact Instability Multiplier (Default x1)";
            };

        }

        private void SubmitName(string arg0)
        {
            Debug.Log(arg0);
        }
    }
}
