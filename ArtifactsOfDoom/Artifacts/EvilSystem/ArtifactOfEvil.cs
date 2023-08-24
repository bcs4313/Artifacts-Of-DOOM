using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using ArtifactsOfDoom;
using Random = System.Random;
using static On.RoR2.GlobalEventManager;
using static On.RoR2.CharacterMaster;
using Messenger;
using orig_Start = On.RoR2.Run.orig_Start;
using On.RoR2.UI.LogBook;
using RoR2.ExpansionManagement;
using RoR2.EntitlementManagement;
using RoR2.UI;
using csProj.Artifacts.MorphSystem;
using R2API.Networking;
using R2API.Networking.Interfaces;
using csProj.Artifacts.Utilities;
using static RoR2.SpawnCard;
using csProj.Artifacts.EvilSystem;

namespace ArtifactGroup
{
	public class ArtifactOfEvil : ArtifactBase
	{

		public static ConfigEntry<int> TimesToPrintMessageOnStart;
		public override string ArtifactName => "Artifact of Ancient Evil";
		public override string ArtifactLangTokenName => "ARTIFACT_OF_EVIL";
		public override string ArtifactDescription => "Play the game as any monster with any base stats / traits.\nPress the UI shortcut (Default: F2) to open/close the monster selection UI.";
		public override Sprite ArtifactEnabledIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/ArtifactOfMetamorphosis.png");
		public override Sprite ArtifactDisabledIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/ArtifactOfMetamorphosisDisabled.png");

		// base parameters
		public uint totalVoidCoins = 0; // base parameter to indicate corruption level (server side)
		public int totalVoidItems = 0; // contributes to the rate of void coins earned.
        public float lastTime = 0; // last time trigger that gave a void coin
		public float interval = 3; // current time interval to get a void coin + some other updates

		// interactable intervals and times
		public float lastChestTime = 0; 
		public float intervalChest = 3600; // interval to spawn a void chest, decreases with more void coins.
		public float lastBarrelTime = 0; 
		public float intervalBarrel = 3600; // interval to spawn a void barrel, decreases with more void coins.
		public float lastTripleTime = 0;
		public float intervalTriple = 3600; // interval to spawn a void triple choice, decreases with more void coins.
		
		public int currentStageClearCount = 0;



        public override void Init()
		{
			//CreateConfig(config);
			CreateLang();
			CreateArtifact();
			Hooks();
			Debug.Log("EVIL INVOKED");
		}

		
		public bool stageActive()
        {
			return LocalUserManager.GetFirstLocalUser().currentNetworkUser.GetCurrentBody() != null;
		}

		public override void Hooks()
		{
			Run.onRunStartGlobal += overrides;

			On.RoR2.InteractableSpawnCard.Spawn += (On.RoR2.InteractableSpawnCard.orig_Spawn orig, global::RoR2.InteractableSpawnCard self, Vector3 position, Quaternion rotation, global::RoR2.DirectorSpawnRequest directorSpawnRequest, ref global::RoR2.SpawnCard.SpawnResult result) =>
			{
				var resources = Resources.LoadAll("spawncards/");
				for(int i = 0; i < resources.Length; i++)
                {
					Debug.Log(resources[i].name);
				}
				orig.Invoke(self, position, rotation, directorSpawnRequest, ref result);
			};
			/*
			RoR2.SpawnCard.onSpawnedServerGlobal += (SpawnCard.SpawnResult spawn) =>
			{
				if (ArtifactEnabled && NetworkServer.active && stageActive())
				{
					PackSpawner.spawnPack();
					double outcome = new Random().NextDouble();
					double chanceToSpawnPack = Math.Min(Math.Pow((totalVoidCoins * 0.004), 2), 3.0);
					Debug.Log("Mob Group spawn chance:: " + chanceToSpawnPack);
					if (outcome < chanceToSpawnPack)
					{
						PackSpawner.spawnPack();
					}
				}
			};
			*/
			On.RoR2.Run.Update += (On.RoR2.Run.orig_Update orig, global::RoR2.Run self) =>
			{
				orig.Invoke(self);

				if (ArtifactEnabled && NetworkServer.active && stageActive())
				{
					intervalChest = (3600 - Math.Min(totalVoidCoins, 500)) / (1f + 0.2f * totalVoidCoins);
					intervalBarrel = (3600 - Math.Min(totalVoidCoins, 500)) / (1f + 0.24f * totalVoidCoins);
					intervalTriple = (3600 - Math.Min(totalVoidCoins,  500)) / (1f + 0.32f * totalVoidCoins);

					//Debug.Log("Interval Chest = " + intervalChest + " NEXT ChestTime Now = " + (lastChestTime + intervalChest));

					// void barrel logic
					if (ArtifactEnabled && NetworkServer.active && RoR2.Run.instance.fixedTime > (lastChestTime + intervalChest))
					{
						spawnVoidChest();
						Debug.Log(" lastChestTime = " + lastChestTime);
						lastChestTime = RoR2.Run.instance.fixedTime;
						Debug.Log("Interval Chest = " + intervalChest + " lastChestTime Now = " + lastChestTime);
					}

					// void triple logic
					if (ArtifactEnabled && NetworkServer.active && RoR2.Run.instance.fixedTime > (lastTripleTime + intervalTriple))
					{
						spawnVoidTriple();
						lastTripleTime = RoR2.Run.instance.fixedTime;
					}

					// void barrel logic
					if (ArtifactEnabled && NetworkServer.active && RoR2.Run.instance.fixedTime > (lastBarrelTime + intervalBarrel))
					{
						spawnVoidBarrel();
						lastBarrelTime = RoR2.Run.instance.fixedTime;
					}

					// check if a certain time interval has passed, if so, add a void coin to all players and increase the timer.
					// Time interval will be somewhat random to make the void feel a bit more chaotic.
					if (ArtifactEnabled && NetworkServer.active && RoR2.Run.instance.fixedTime > (lastTime + interval))
					{
						lastTime = RoR2.Run.instance.fixedTime; // update prev time
																//interval /= 1.1f; // for fun only 
																//PackSpawner.spawnPack();
						double outcome = new Random().NextDouble();
						double chanceToSpawnPack = Math.Min(Math.Pow((totalVoidCoins * 0.004), 2), 3.0);
						Debug.Log("Mob Group spawn chance:: " + chanceToSpawnPack);
						if (outcome < chanceToSpawnPack)
						{
							PackSpawner.randomPack();
						}
						//RoR2.Artifacts.DoppelgangerInvasionManager.PerformInvasion(new Xoroshiro128Plus(0));
						// adjusting interactable spawn intervals::
						// max = 1 hour. Gets lower with void coin total (logarithmic)
						//intervalChest = (float)(3600 / (Math.Log(totalVoidCoins, 3f) * Math.Log(totalVoidCoins, 3f) + (0.1f * totalVoidCoins + 1f)));
						//intervalBarrel = (float)(3600 / (Math.Log(totalVoidCoins, 2f) * Math.Log(totalVoidCoins, 1.4f) + (0.1f * totalVoidCoins + 1f)));
						//intervalTriple = (float)(3600 / (Math.Log(totalVoidCoins, 4f) * Math.Log(totalVoidCoins, 4f) + (0.08f * totalVoidCoins + 1f)));

						// void seed spawns
						if (RoR2.Run.instance.stageClearCount > currentStageClearCount)
						{
							Debug.Log("Running void seed  roll");
							currentStageClearCount = RoR2.Run.instance.stageClearCount;
							// chance for void seed each stage = voidCoin total
							int outcome2 = new Random().Next(0, 100);
							Debug.Log("Outcome = " + outcome2 + " total coins = " + totalVoidCoins);
							if (outcome2 < ((int)totalVoidCoins))
							{
								Debug.Log("Spawning void seed");
								spawnVoidSeed();
							}
						}

						// update void coin total
						foreach (NetworkUser u in NetworkUser.readOnlyInstancesList)
						{
							int random = new Random().Next(0, 100);

							if (random > 50)
							{
								totalVoidCoins += 1;
							}
							else if (random > 30)
							{
								totalVoidCoins += 2;
							}
							else if (random > 15)
							{
								totalVoidCoins += 3;
							}
							else if (random > 8)
							{
								totalVoidCoins += 4;
							}
							else
							{
								totalVoidCoins += 5;
							}
							u.master.voidCoins = totalVoidCoins;
						}
					}
				}
			}; 

		}

		public void spawnVoidSeed()
        {
			// retrieve the void card using my Card Retrieval Utility, avoiding needing a path altogether:
			SpawnCard card = CardRetriever.getCard("iscVoidCamp");

			card.directorCreditCost = 0;
			DirectorCore.spawnedObjects.Capacity = 99999;
			RoR2.SceneDirector.cardSelector.Capacity = 99999;

			DirectorPlacementRule rule = new DirectorPlacementRule();
			rule.placementMode = DirectorPlacementRule.PlacementMode.Random;

			DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(card, rule, new Xoroshiro128Plus(1));
			directorSpawnRequest.ignoreTeamMemberLimit = true;
			Debug.Log(directorSpawnRequest.teamIndexOverride.ToString());
			DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
		}

		public void spawnVoidChest()
        {
			MessageHandler.globalMessage("A Void Chest has spawned on the map!");
			SpawnCard card = Resources.Load<SpawnCard>("spawncards/interactablespawncard/iscVoidChest");
			card.directorCreditCost = 0;
			DirectorCore.spawnedObjects.Capacity = 99999;
			RoR2.SceneDirector.cardSelector.Capacity = 99999;

			DirectorPlacementRule rule = new DirectorPlacementRule();
			rule.placementMode = DirectorPlacementRule.PlacementMode.Random;

			DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(card, rule, new Xoroshiro128Plus(1));
			directorSpawnRequest.ignoreTeamMemberLimit = true;
			Debug.Log(directorSpawnRequest.teamIndexOverride.ToString());
			DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
		}

		public void spawnVoidBarrel()
		{
			MessageHandler.globalMessage("A Void Barrel has spawned on the map!");
			SpawnCard card = Resources.Load<SpawnCard>("spawncards/interactablespawncard/iscVoidCoinBarrel");
			card.directorCreditCost = 0;
			DirectorCore.spawnedObjects.Capacity = 99999;
			RoR2.SceneDirector.cardSelector.Capacity = 99999;

			DirectorPlacementRule rule = new DirectorPlacementRule();
			rule.placementMode = DirectorPlacementRule.PlacementMode.Random;

			DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(card, rule, new Xoroshiro128Plus(1));
			directorSpawnRequest.ignoreTeamMemberLimit = true;
			Debug.Log(directorSpawnRequest.teamIndexOverride.ToString());
			DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
		}

		public void spawnVoidTriple()
		{
			MessageHandler.globalMessage("A Void Potential has spawned on the map!");
			SpawnCard card = Resources.Load<SpawnCard>("spawncards/interactablespawncard/iscVoidTriple");
			card.directorCreditCost = 0;
			DirectorCore.spawnedObjects.Capacity = 99999;
			RoR2.SceneDirector.cardSelector.Capacity = 99999;

			DirectorPlacementRule rule = new DirectorPlacementRule();
			rule.placementMode = DirectorPlacementRule.PlacementMode.Random;

			DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(card, rule, new Xoroshiro128Plus(1));
			directorSpawnRequest.ignoreTeamMemberLimit = true;
			Debug.Log(directorSpawnRequest.teamIndexOverride.ToString());
			DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
		}

		static public GameObject spawnAncientEvil()
		{
			String[] options = { "cscBeetle", "cscBeetleGuard", "cscBeetleQueen", "cscBell", "cscBrother", "cscClayBoss", "cscClayBruiser",
			"cscElectricWorm", "cscGolem", "cscGravekeeper", "cscGreaterWisp", "cscHermitCrab", "cscImp", "cscImpBoss", "cscJellyfish"
			, "cscLemurian", "cscLemurianBruiser", "cscLunarExploder", "cscLunarGolem", "cscLunarWisp", "cscMagmaWorm",
			"cscMiniMushroom", "cscNullifier", "cscParent", "cscParentPod", "cscRoboBallBoss", "cscRoboBallGreenBuddy", "cscRoboBallRedBuddy",
			"cscScav", "cscScavLunar", "cscSquidTurret", "cscSuperRoboBallBoss", "cscTitanGold", "cscVagrant", "cscVulture",
			"cscGrandparent", "cscBackupDrone", "cscArchWisp"};

			String randoMob = options[new Random().Next(0, options.Length)];

			SpawnCard card = Resources.Load<SpawnCard>("spawncards/characterspawncards/" + randoMob);
			card.directorCreditCost = 0;
			DirectorCore.spawnedObjects.Capacity = 99999;
			RoR2.SceneDirector.cardSelector.Capacity = 99999;

			DirectorPlacementRule rule = new DirectorPlacementRule();
			rule.placementMode = DirectorPlacementRule.PlacementMode.Random;

			DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(card, rule, new Xoroshiro128Plus(1));
			directorSpawnRequest.teamIndexOverride = TeamIndex.Monster;
			directorSpawnRequest.ignoreTeamMemberLimit = true;
			Debug.Log(directorSpawnRequest.teamIndexOverride.ToString());
			MessageHandler.globalMessage("An Ancient Evil has Spawned!");
			return DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
		}

		private void overrides(Run run)
		{
			if (NetworkServer.active && ArtifactEnabled)
			{
				MessageHandler.globalMessage("Embrace the void traveler. Give in to your calling...");
				totalVoidCoins = 0; // base parameter to indicate corruption level (server side)
				totalVoidItems = 0; // contributes to the rate of void coins earned.
				lastTime = 0; // last time trigger that gave a void coin
				interval = 3; // current time interval to get a void coin
				lastChestTime = 0;
				lastBarrelTime = 0;
				lastTripleTime = 0;
				intervalBarrel = 3600;
				intervalChest = 3600;
				intervalTriple = 3600;
				currentStageClearCount = 0;
		}

		}
	}

}

