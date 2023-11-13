using ArtifactGroup;
using ArtifactsOfDoom;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace csProj.Artifacts.EvilSystem
{
    // This holds the list of ALL monsters that a player may select and morph into. It also
    // Retrieves and produces components related to the monsters, for the UI.

    class ShrineSpawner
    {
        public static String nameToken = "VOIDALTAR";

        // spawn 30 altars in a random locations on the map
        static public void spawnAltar()
        { 
            // initiate shrine object and give it interactable properties
            GameObject ShrineOBJ = Main.GOAssets.LoadAsset<GameObject>("Assets/Void Altar.prefab");
            GameObject obj = GameObject.Instantiate(ShrineOBJ);


            obj.AddComponent<NetworkIdentity>(); 

            // interaction section
            var interact = obj.AddComponent<PurchaseInteraction>();
            interact.displayNameToken = $"INTERACTABLE_{nameToken}_NAME";
            interact.contextToken = $"INTERACTABLE_{nameToken}_CONTEXT";
            interact.costType = CostTypeIndex.VoidCoin;
            interact.automaticallyScaleCostWithDifficulty = false;
            interact.isShrine = true;
            interact.isGoldShrine = false;
            interact.cost = 0;
            interact.available = true;
            interact.setUnavailableOnTeleporterActivated = false;

            // ping info provider, not sure if I care yet lol
            var pingProv = obj.AddComponent<PingInfoProvider>();
            //pingProv.pingIconOverride = MainBundle.LoadAsset<Sprite>("Assets/RoR2/Base/Common/MiscIcons/texShrineIconOutlined.png");

            // provides a name
            var genericNameDisplay = obj.AddComponent<GenericDisplayNameProvider>();
            genericNameDisplay.displayToken = $"INTERACTABLE_{nameToken}_NAME";

            // provides an interaction with object
            var shrineManager = obj.AddComponent<voidShrineManager>();
            shrineManager.PurchaseInteraction = interact;
            shrineManager.ScalingModifier = 1;
            shrineManager.UseDefaultScaling = false;

            // provides collision with object
            var entityLocator = obj.GetComponentInChildren<MeshCollider>().gameObject.AddComponent<EntityLocator>();
            entityLocator.entity = obj;

            // not sure what is going on I assume it just works
            var modelLocator = obj.AddComponent<ModelLocator>();
            modelLocator.modelTransform = obj.transform;
            try
            {
                modelLocator.modelBaseTransform = obj.transform.Find("Base");
                Debug.Log("Base Transform found:");
            }
            catch (Exception e)
            {
                Debug.Log("Base Transform not found: " + e.ToString());
                modelLocator.modelBaseTransform = obj.transform;
            }
            modelLocator.dontDetatchFromParent = true;
            modelLocator.autoUpdateModelTransform = true;

            // outline highlighter
            var highlightController = obj.GetComponent<Highlight>();
            highlightController.targetRenderer = obj.GetComponentsInChildren<MeshRenderer>().Where(x => x.gameObject.name.Contains("Crystal")).First();
            highlightController.strength = 1;
            highlightController.highlightColor = Highlight.HighlightColor.interactive;

            PrefabAPI.RegisterNetworkPrefab(obj);

            SpawnCard card = Resources.Load<SpawnCard>("spawncards/interactablespawncard/iscShrineBlood");
            card.directorCreditCost = 0;
            card.prefab = obj;
            DirectorCore.spawnedObjects.Capacity = 99999;
            RoR2.SceneDirector.cardSelector.Capacity = 99999;

            DirectorPlacementRule rule = new DirectorPlacementRule();
            rule.placementMode = DirectorPlacementRule.PlacementMode.Random;

            DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(card, rule, new Xoroshiro128Plus(1));
            directorSpawnRequest.ignoreTeamMemberLimit = true;
            GameObject result = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
        }


        public class voidShrineManager : NetworkBehaviour
        {
            public PurchaseInteraction PurchaseInteraction;
            public float ScalingModifier;
            public bool UseDefaultScaling;

            [SyncVar]
            public int BaseCostDetermination;

            public int uses;

            public void Start()
            {
                if (NetworkServer.active && Run.instance)
                {
                    PurchaseInteraction.SetAvailable(true);
                }

                PurchaseInteraction.onPurchase.AddListener(activateAltar);
            }
        }

        [Server]
        public static void activateAltar(Interactor interactor)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'ShrineOfRepair.Interactables.ShrineOfRepair::RepairPurchaseAttempt(RoR2.Interactor)' called on client");
                return;
            }

            if (!interactor) { return; }
            var body = interactor.GetComponent<CharacterBody>();
            if (body && body.master)
            {
                Debug.Log("Interaction Detected from player: " + interactor.name);
            }
        }
    }
}
