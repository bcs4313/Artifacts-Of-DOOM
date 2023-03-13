using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using ArtifactsOfDoom;
using static On.RoR2.GlobalEventManager;
using static On.RoR2.Run;
using Messenger;
using static On.RoR2.CharacterMaster;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using ArtifactsOfDoom;
using Random = System.Random;
using static On.RoR2.GenericPickupController;
using UnityEngine.Networking;
using R2API.Networking.Interfaces;
using R2API.Networking;
using RoR2.Artifacts;
using System;
using static On.RoR2.CharacterBody;
using static On.RoR2.TeleporterInteraction;
using orig_Update = On.RoR2.Run.orig_Update;
using orig_Start = On.RoR2.Run.orig_Start;
using static On.RoR2.EquipmentSlot;
using static On.RoR2.PickupDropletController;
using UnityEngine.UI;
using RoR2.UI;
using System.Collections.Generic;

namespace ArtifactGroup
{
    class EntropySubsetGen
    {
        int seed = 0;
        static public String[] enemySubset = new string[0];
        static public String[] transformSubset = new string[0];
        static public ItemIndex[] itemSubset = new ItemIndex[0];
        static public int[] itemColorSubset = new int[0];

        public static void generateMonsterSubset()
        {
            String[] fullSet = {
                "cscBeetle", "cscBeetleGuard", "cscBeetleQueen", "cscBell", "cscBrother", "cscClayBoss", "cscClayBruiser",
            "cscElectricWorm", "cscGolem", "cscGravekeeper", "cscGreaterWisp", "cscHermitCrab", "cscImp", "cscImpBoss", "cscJellyfish"
            , "cscLemurian", "cscLemurianBruiser", "cscLesserWisp", "cscLunarExploder", "cscLunarGolem", "cscLunarWisp", "cscMagmaWorm",
            "cscMiniMushroom", "cscNullifier", "cscParent", "cscParentPod", "cscRoboBallBoss", "cscRoboBallGreenBuddy", "cscRoboBallRedBuddy",
            "cscScav", "cscScavLunar", "cscSquidTurret", "cscSuperRoboBallBoss", "cscTitanGold", "cscVagrant", "cscVulture",
            "cscGrandparent", "cscBackupDrone", "cscArchWisp"};


            // calculate how many enemies are in the subset
            int subsetTotal = 1;
            double subsetIncreaseChance = 0.5; // coin flip chance to add to subset each time
            double result = 0;
            while (result < subsetIncreaseChance)
            {
                subsetTotal++;
                result = ArtifactOfEntropy.rnd.NextDouble();
            }

            // now create the array and generate the set
            String[] subset = new String[subsetTotal];
            for (int i = 0; i < subset.Length; i++)
            {
                // select a random enemy in the fullset
                int randomEnemy = ArtifactOfEntropy.rnd.Next(0, fullSet.Length);
                subset[i] = fullSet[randomEnemy];
            }

            // now update the static variable
            enemySubset = subset;
        }

        public static void generateTransformSubset()
        {
            String[] fullSet = { "WispSoulBody", "Bandit2Body", "BeetleBody", "BeetleGuardBody", "ClayBruiserBody", "CommandoBody", "CrocoBody",
                "BeetleQueen2Body", "BellBody", "BisonBody", "BrotherBody", "BrotherHurtBody", "BrotherHauntBody", "CaptainBody", "ClayBossBody",
                "Drone1Body", "Drone2Body", "FlameDroneBody", "MegaDroneBody", "MissileDroneBody", "ElectricWormBody", "EngiBody", "EngiWalkerTurretBody"
            , "GolemBody", "GrandParentBody", "GreaterWispBody", "HermitCrabBody", "HuntressBody", "ImpBody",  "JellyfishBody",
            "LemurianBody", "LoaderBody", "LunarExploderBody", "LunarGolemBody", "LunarWispBody", "MageBody", "MagmaWormBody", "MercBody", "MiniMushroomBody",
            "ParentBody", "RoboBallBossBody", "RoboBallMiniBody", "SuperRoboBallBossBody", "ScavBody", "ScavLunar1Body", "TitanBody", "TitanGoldBody",
            "ToolbotBody", "VagrantBody", "VultureBody", "WispBody", "Assassin2Body", "ClayGrenadierBody", "FlyingVerminBody", "RailgunnerBody", "VoidSurvivorBody"
            , "AssassinBody", "BomberBody"};

            // calculate how many enemies are in the subset
            int subsetTotal = 1;
            double subsetIncreaseChance = 0.5; // coin flip chance to add to subset each time
            double result = 0;
            while (result < subsetIncreaseChance)
            {
                subsetTotal++;
                result = ArtifactOfEntropy.rnd.NextDouble();
            }

            // now create the array and generate the set
            String[] subset = new String[subsetTotal];
            for (int i = 0; i < subset.Length; i++)
            {
                // select a random enemy in the fullset
                int randomEnemy = ArtifactOfEntropy.rnd.Next(0, fullSet.Length);
                subset[i] = fullSet[randomEnemy];
            }

            // now update the static variable
            transformSubset = subset;
        }

        public static void generateItemSubset()
        {
            // calculate how many items are in the subset
            int subsetTotal = 1;
            double subsetIncreaseChance = 0.7; // 70% chance to add to subset each time
            double result = 0;
            while (result < subsetIncreaseChance)
            {
                subsetTotal++;
                result = ArtifactOfEntropy.rnd.NextDouble();
            }

            // now create the array and generate the set
            ItemIndex[] subset = new ItemIndex[subsetTotal];
            int[] itemColorSubset = new int[subsetTotal];
            for (int i = 0; i < subset.Length; i++)
            {
                // item index lists
                List<ItemIndex> commons = ItemCatalog.tier1ItemList;
                List<ItemIndex> rares = ItemCatalog.tier2ItemList;
                List<ItemIndex> legendaries = ItemCatalog.tier3ItemList;

                // rarity drop chances
                double rareDropChance = 69.0;
                double legendaryDropChance = 95.0;

                // determine what rarity will drop
                Random rnd = new Random();
                double gen = rnd.Next(1, 100);

                ItemIndex dex;
                if (gen >= legendaryDropChance)
                {
                    //Debug.Log("Spawning Legendary...");
                    int len = legendaries.Count;
                    int index = rnd.Next(0, len);
                    dex = legendaries[index];
                    itemColorSubset[i] = 3;
                }
                else if (gen >= rareDropChance)
                {
                    //Debug.Log("Spawning Rare...");
                    int len = rares.Count;
                    int index = rnd.Next(0, len);
                    dex = rares[index];
                    itemColorSubset[i] = 2;
                }
                else
                {
                    //Debug.Log("Spawning Common...");
                    int len = commons.Count;
                    int index = rnd.Next(0, len);
                    dex = commons[index];
                    itemColorSubset[i] = 1;

                }
                subset[i] = dex;
            }
            itemSubset = subset;
        }
    }
}