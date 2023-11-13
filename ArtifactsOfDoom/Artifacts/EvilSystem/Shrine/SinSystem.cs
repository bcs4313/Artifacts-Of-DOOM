using ArtifactGroup;
using Messenger;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace csProj.Artifacts.EvilSystem
{
    // sins work on a proc system, where if the current sin is correct, the proc from a hook will trigger an action.
    class SinSystem
    {
        public static String currentSin = "Pride";

        // sloth variables
        public static float slothStartTime = 0;
        public static float slothBaseInterval = 360;
        public static float slothInterval = 30;

        // envy variables
        public struct dmgHitTake
        {
            public uint userID;
            public float dmgPercent;
            public float hitTime;
        };
        public static List<dmgHitTake> hitHistory = new List<dmgHitTake>();
        public static float reflectMult = 0;

        // wrath variables
        public static List<float> killHistory = new List<float>();



        // variables all sins may influence (stat related)
        public static float healthMult = 1;
        public static float damageMult = 1;
        public static float speedMult = 1;
        public static float attackSpeedMult = 1;


        public static void selectSin()
        {
            int sinIndex = ArtifactOfEvil.r.Next(0, 3);
            currentSin = "Wrath";
            /*
            switch(sinIndex)
            {
                case 0:
                    currentSin = "Pride";
                    break;
                case 1:
                    currentSin = "Sloth";
                    break;
                case 2:
                    currentSin = "Lust";
                    break;
            }
            */
        }

        public static void progressStage()
        {
            selectSin();
            slothStartTime = RoR2.Run.instance.fixedTime;

            // spawn an altar lol
            ShrineSpawner.spawnAltar();

            switch (currentSin)
            {
                case "Pride":
                    MessageHandler.globalMessage("Pride: Activating a mountain shrine spawns 3 mountain shrines. Penalty: 10 VC. Permanent Effect: 50 VC.");
                    prideProc(false);
                break;  
                case "Sloth":
                    MessageHandler.globalMessage("Sloth: Each 30 seconds spent past 6 minutes in a stage: 1.2x health 0.9x speed 0.9x attack speed permanently. Penalty: 5 VC. Permanent Effect: 40 VC.");
                    break;
                case "Lust":
                    MessageHandler.globalMessage("Lust: Printing an item spawns 2 additional printers on the map. Penalty: 10 VC. Permanent Effect: 50 VC.");
                    break;
                case "Envy":
                    MessageHandler.globalMessage("Envy: Taking 70%+ total health damage from monsters in 2 seconds grants +100% additional damage returned to enemies permanently. Proc Coefficient: 3. Penalty: 15 VC. Permanent Effect: 30 VC.");
                    break;
                case "Wrath":
                    MessageHandler.globalMessage("Wrath: Killing 5 enemies in 1 second gives 5% damage/attackspeed for each enemy killed permanently. Penalty: X VC. Permanent Effect: 50 VC.");
                    break;
                case "Gluttony":
                    MessageHandler.globalMessage("Gluttony: Press E with an enemy to eat them. Gain 3% of ALL their total stats permanently. Penalty: 10% remaining hp in VC. Permanent Effect: 50 VC.");
                    break;
                case "Greed":
                    MessageHandler.globalMessage("Greed: Activating a gilded altar grants 30% more chests and gold earned in the run + more interactables on the stage. Penalty: 25 VC. Permanent Effect: 50 VC.");
                    break;
            }
        }

        // plays a sound effect, gives the player a buff, and maybe in the future some other things to notify everyone that a sin has procced!
        public static void notifySinProc()
        {
            foreach (NetworkUser u in NetworkUser.readOnlyInstancesList)
            {
                if(u.GetCurrentBody() != null)
                {
                    CharacterBody body = u.GetCurrentBody();
                    body.AddTimedBuff(DLC1Content.Buffs.EliteVoid, 3, 999);
                }
            }
        }

        public static void wrathProc(bool giveCoins, uint kills)
        {
            if (currentSin.Equals("Wrath"))
            {
                if (giveCoins) { ArtifactOfEvil.totalVoidCoins += kills; }
                Messenger.MessageHandler.globalMessage(":::Wrath Proc - kills: " + kills);
                damageMult += 0.01f * kills;
                attackSpeedMult += 0.01f * kills;
                notifySinProc();
            }
        }

        public static void envyProc(bool giveCoins)
        {
            if (currentSin.Equals("Envy"))
            {
                if (giveCoins) { ArtifactOfEvil.totalVoidCoins += 15; }
                Messenger.MessageHandler.globalMessage(":::Envy Proc");
                reflectMult += 1f;
                notifySinProc();
            }
        }

        public static void prideProc(bool giveCoins)
        {
            if (currentSin.Equals("Pride"))
            {
                Messenger.MessageHandler.globalMessage(":::Pride Proc");
                if (giveCoins) { ArtifactOfEvil.totalVoidCoins += 10; }
                for (int i = 0; i < 3; i++)
                {
                    SpawnCard card = Resources.Load<SpawnCard>("spawncards/interactablespawncard/iscShrineBoss");
                    card.directorCreditCost = 0;
                    DirectorCore.spawnedObjects.Capacity = 99999;
                    RoR2.SceneDirector.cardSelector.Capacity = 99999;

                    DirectorPlacementRule rule = new DirectorPlacementRule();
                    rule.placementMode = DirectorPlacementRule.PlacementMode.Random;

                    DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(card, rule, new Xoroshiro128Plus(1));
                    directorSpawnRequest.ignoreTeamMemberLimit = true;
                    Debug.Log(directorSpawnRequest.teamIndexOverride.ToString());
                    DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                    ArtifactOfEvil.VCUPDATE();
                }
                notifySinProc();
            }
        }

        public static void slothProc (bool giveCoins)
        {
            if (currentSin.Equals("Sloth"))
            {
                Debug.Log("slothStartTime " + slothStartTime);
                Debug.Log("slothBaseInterval " + slothStartTime);
                Debug.Log("slothInterval " + slothInterval);
                if (RoR2.Run.instance.fixedTime > (slothStartTime + slothBaseInterval + slothInterval))
                {
                    Messenger.MessageHandler.globalMessage(":::Sloth Proc");
                    if (giveCoins) { ArtifactOfEvil.totalVoidCoins += 8; }
                    slothStartTime += slothInterval;
                    healthMult *= 1.2f;
                    speedMult *= 0.9f;
                    attackSpeedMult *= 0.9f;

                    foreach (NetworkUser u in NetworkUser.readOnlyInstancesList)
                    {
                        if (u.GetCurrentBody() != null)
                        {
                            u.GetCurrentBody().baseMaxHealth *= 1.2f;
                            u.GetCurrentBody().levelMaxHealth *= 1.2f;
                        }
                    }

                    ArtifactOfEvil.VCUPDATE();
                }
                notifySinProc();
            }
        }

        public static void lustProc(bool giveCoins)
        {
            if (currentSin.Equals("Lust"))
            {
                if (giveCoins) { ArtifactOfEvil.totalVoidCoins += 10; }
                for (int i = 0; i < 2; i++)
                {
                    Messenger.MessageHandler.globalMessage(":::Lust Proc");
                    SpawnCard card = Resources.Load<SpawnCard>("spawncards/interactablespawncard/iscDuplicator");
                    card.directorCreditCost = 0;
                    DirectorCore.spawnedObjects.Capacity = 99999;
                    RoR2.SceneDirector.cardSelector.Capacity = 99999;

                    DirectorPlacementRule rule = new DirectorPlacementRule();
                    rule.placementMode = DirectorPlacementRule.PlacementMode.Random;

                    DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(card, rule, new Xoroshiro128Plus(1));
                    directorSpawnRequest.ignoreTeamMemberLimit = true;
                    Debug.Log(directorSpawnRequest.teamIndexOverride.ToString());
                    DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                    ArtifactOfEvil.VCUPDATE();
                }
                notifySinProc();
            }
        }
    }
}
