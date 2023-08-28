using ArtifactGroup;
using ArtifactsOfDoom;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace csProj.Artifacts.MorphSystem
{
    // This holds the list of ALL monsters that a player may select and morph into. It also
    // Retrieves and produces components related to the monsters, for the UI.

    class MorphController
    {
        
        static public String[] fullSet = {"CommandoBody", "HuntressBody", "Bandit2Body", "EngiBody", "ToolbotBody", "MageBody", "MercBody", "TreebotBody", "LoaderBody",
            "CrocoBody", "CaptainBody", "HereticBody", "RailgunnerBody", "VoidSurvivorBody", "WispSoulBody", "BeetleBody", "BeetleGuardBody", "ClayBruiserBody",
                "BeetleQueen2Body", "BellBody", "BisonBody", "BrotherBody", "BrotherHurtBody", "BrotherHauntBody", "ClayBossBody",
                 "ElectricWormBody", "EngiWalkerTurretBody"
            , "GolemBody", "GrandParentBody", "GreaterWispBody", "HermitCrabBody", "ImpBody",  "JellyfishBody",
            "LemurianBody", "LunarExploderBody", "LunarGolemBody", "LunarWispBody", "MagmaWormBody", "MiniMushroomBody",
            "ParentBody", "RoboBallBossBody", "RoboBallMiniBody", "SuperRoboBallBossBody", "ScavBody", "ScavLunar1Body", "TitanBody", "TitanGoldBody",
            "VagrantBody", "VultureBody", "WispBody", "Assassin2Body", "ClayGrenadierBody", "FlyingVerminBody",
            "AssassinBody", "BomberBody", "AcidLarvaBody", "VoidInfestorBody", "VerminBody", "MinorConstructBody", "LemurianBruiserBody", "GupBody",
            "NullifierBody", "GravekeeperBody", "ImpBossBody", "MegaConstructBody", "VoidMegaCrabBody", "VoidRaidCrabBody", 
            "BackupDroneBody", "Drone1Body", "Drone2Body", "DroneCommanderBody", "EmergencyDroneBody", "FlameDroneBody", // turrets and drones
            "MegaDroneBody", "MissileDroneBody", "EngiWalkerTurretBody"}; // turrets and drones
                                                                   // to do: add "VoidRaidCrabBody"

        // some entities can't be elite... they are listed here
        static public String[] illegalElites =
        {
            "BackupDroneBody", "Drone1Body", "Drone2Body", "DroneCommanderBody", "EmergencyDroneBody", "FlameDroneBody", // turrets and drones
            "MegaDroneBody", "MissileDroneBody", "EngiWalkerTurretBody"
        };

        // get a prefab to load monster portrait and other contextual info
        static public void injectButtons()
        {
            for (int i = 0; i < fullSet.Length; i++)
            {
                //Debug.Log("Injecting button...");
                GameObject selectOBJ = Main.MorphAssets.LoadAsset<GameObject>("Assets/GameObjects/Button.prefab");
                //Debug.Log("Injecting button P2...");
                GameObject obj = GameObject.Instantiate(selectOBJ);
                //Debug.Log("Injecting button P3...");
                UnityEngine.UI.Button button = obj.GetComponent<UnityEngine.UI.Button>();
                //Debug.Log("IMG RETRIEVE => " + fullSet[i]);
                //Debug.Log("Assets/GameObjects/" + fullSet[i] + ".png");
                Sprite spr = Main.MorphAssets.LoadAsset<Sprite>("Assets/GameObjects/" + fullSet[i] + ".png");
                if (spr != null)
                {
                    //Debug.Log("Found Sprite!");
                    button.image.sprite = spr;

                    // Inject the button into the dropdown
                    GameObject inst = ArtifactOfMorph.selectOBJInstance;
                    //Debug.Log("00: " + inst.ToString());
                    GameObject search1 = inst.transform.GetChild(0).gameObject;
                    //Debug.Log("01: " + search1.ToString());
                    GameObject search2 = search1.transform.GetChild(1).gameObject;
                    //Debug.Log("02: " + search2.ToString());
                    GameObject search3 = search2.transform.GetChild(0).gameObject;
                    //Debug.Log("03: " + search3.ToString());
                    GameObject search4 = search3.transform.GetChild(0).gameObject;
                    //Debug.Log("04: " + search4.ToString());
                    hookButton(button, fullSet[i]);
                    button.transform.SetParent(search4.transform, false);
                    //button.transform.parent = search4.transform;
                    //button.transform.parent = ArtifactOfMorph.selectOBJInstance.transform
                }
                else
                {
                    //Debug.Log("Null...");
                }
            }
            // unlock mouse
            //Debug.Log("Cursor6 " + RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount);
            RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount++;
        }


        // add hook to transform and remove the UI on click
        public static void hookButton(Button b, String target)
        {
            //Debug.Log("Hooking: " + target);
            b.onClick.AddListener(() => bTrigger(target));
        }

        public static void bTrigger(String s)
        {
            Debug.Log("TRANSFORM: " + s);
            NetworkUser user = LocalUserManager.GetFirstLocalUser().currentNetworkUser;

            if (NetworkServer.active)
            { // lock mouse
                ArtifactOfMorph.selectOBJInstance.SetActive(false);
                ArtifactOfMorph.selectOBJInstance.active = false;
                //Debug.Log("Cursor7 " + RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount);
                RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount--;
                Debug.Log("SERVER MORPH LOCAL");
                CharacterBody body = user.GetCurrentBody();
                transformBody(user, body, s);
            }
            else
            { // lock mouse
                ArtifactOfMorph.selectOBJInstance.SetActive(false);
                ArtifactOfMorph.selectOBJInstance.active = false;
                RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount--;
                //Debug.Log("Cursor8 " + RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount);
                Debug.Log("SERVER MORPH REQUEST SEND");
                new NetworkBehavior.TransformRequest(user.GetCurrentBody().netId.Value, s).Send(NetworkDestination.Server);
            }
        }

        // transform character body into a specified monster from string s
        public static void transformBody(NetworkUser user, CharacterBody body, String s)
        {
            try
            {
                user.SetBodyPreference(BodyCatalog.FindBodyIndex(s));
                user.master.Respawn(user.GetCurrentBody().footPosition, user.transform.rotation);
            }
            catch (Exception e) // this will handle the cursorOpener bug itself
            {
                Debug.LogError(e.ToString());
            }
        }
    }
}
