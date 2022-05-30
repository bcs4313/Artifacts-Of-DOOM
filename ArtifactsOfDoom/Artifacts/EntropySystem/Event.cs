using ArtifactsOfDoom;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ArtifactGroup
{
    // holds all the data associated with an event.
    // this includes probability chances, attached hooks, and an effect call
    class Event
    {
        public int eventID;

        // each hook that could trigger something
        public String[] availableHooks = { "Teleporter", "DamageTake", "DamageDeal", "Land", "Kill", "EquipmentActivate", "PickupSpawn",
                                           "ShrineChance", "ShrineBoss", "ShrineBlood", "ShrineCleanse", "ShrineCombat", "ShrineHealing", "PrimarySkill", "SecondarySkill", "UtilitySkill", "SpecialSkill", "W",
                                           "A", "S", "D", "Ctrl", "CoinBarrel"};

        // multipliers that help "balance" out each random hook's chances (0.5 = normal chance, 1.5 = double)
        public double[] implicitWeights = { 40,            0.4,          0.2,          0.4,    0.3,    2.3,                 3.2,
                                            2.5,            3.5,          3,               500,              2.5,            3.5,             0.2,               1.9,            2.1,           2.3,           0.1,
                                           0.1, 0.1, 0.1, 0.25,     2.5,           500};

        public List<String> hooksAttached = new List<String>();
        public List<double> hookChances = new List<double>();
        bool forceEvent = false;
        int forceValue = 78;

        System.Random rnd = ArtifactOfEntropy.rnd;

        // Create an event with an ID. This ID is attached to the execution of a random effect
        public Event(int eventID)
        {
            this.eventID = eventID;

            double hookChance = 1.8;
            //Debug.Log("base hook chance: " + hookChance);
            // formuala: hookChance *= 1 / ((availableHooks.Length - 6) / (EntropyHost.eventTotal / 18));
            //Debug.Log("initial hook chance: " + hookChance);
            // apply RiskOfOptions setting 
            try
            {
                hookChance *= float.Parse(OptionsLink.AOE_EventHookMultiplier.Value);
            }
            catch
            {
                Messenger.MessageHandler.globalMessage("error parsing EventHookMultiplier setting! Check to see if the setting is a decimal value!");
            }
            //Debug.Log("Caclulated hook chance with setting: " + hookChance);

            // run randomly through the list of hooks, on average picking 
            // x hooks, divided by the known event count / 10
            for (int i = 0; i < availableHooks.Length; i++)
            {
                double roll = rnd.NextDouble() * 100;
                if (roll < hookChance)
                {
                    hooksAttached.Add(availableHooks[i]);

                    // roll the chance of this event to occur on the hook. 
                    // made to be fairly unstable (for fun)
                    double r1 = rnd.NextDouble();
                    double r2 = rnd.NextDouble();
                    double r3 = rnd.NextDouble();
                    double bigMult = rnd.NextDouble() + implicitWeights[i];

                    // include the implicit weight for the chance!
                    try
                    {
                        float optionMult = float.Parse(OptionsLink.AOE_EventChanceMultiplier.Value);
                        float optionAdd = OptionsLink.AOE_EventChanceOffset.Value;
                        hookChances.Add(Math.Min(Math.Min(100.0 * r1 * r2 * r3 * bigMult, 100) * optionMult, 100) + optionAdd);
                    }
                    catch
                    {
                        // include the implicit weight for the chance!
                        hookChances.Add(100.0 * r1 * r2 * r3 * bigMult);
                    }
                }
            }
            
            if(forceEvent == true && this.eventID == forceValue)
            {
                int selectedHook = (int)Math.Round(rnd.NextDouble() * availableHooks.Length) - 1;
                hooksAttached.Add(availableHooks[selectedHook]);

                // roll the chance of this event to occur on the hook. 
                // made to be fairly unstable (for fun)
                double r1 = rnd.NextDouble();
                double r2 = rnd.NextDouble();
                double r3 = rnd.NextDouble();
                double bigMult = rnd.NextDouble() + implicitWeights[selectedHook];

                try
                {
                    float optionMult = float.Parse(OptionsLink.AOE_EventChanceMultiplier.Value);
                    float optionAdd = OptionsLink.AOE_EventChanceOffset.Value;
                    hookChances.Add(Math.Min(Math.Min(100.0 * r1 * r2 * r3 * bigMult, 100) * optionMult, 100) + optionAdd);
                }
                catch
                {
                    // include the implicit weight for the chance!
                    hookChances.Add(100.0 * r1 * r2 * r3 * bigMult);
                }
            }
            
            
        }

        // run a hook with a corresponding outcome chance, to see if this event triggers
        public bool runOutcome(String hook)
        {
            // find String index
            int loc = -1;
            for (int i = 0; i < hooksAttached.Count; i++)
            {
                if (hooksAttached[i].Equals(hook))
                {
                    loc = i;
                    break;
                }
            }

            if (loc == -1)
            {
                return false;
            }

            double chanceToRun = hookChances[loc];

            return (rnd.NextDouble() * 100) < chanceToRun;
        }

        public void printData()
        {
            Debug.Log("Data for ID: " + eventID);
            Debug.Log("Hooks:");
            for (int i = 0; i < hooksAttached.Count; i++)
            {
                Debug.Log("-> " + hooksAttached[i] + "  -- Chance: " + hookChances[i]);
            }
        }
    }
}
