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
using static csProj.Artifacts.EvilSystem.SinSystem;

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
		public static uint totalVoidCoins = 0; // base parameter to indicate corruption level (server side)
		public static int totalVoidItems = 0; // contributes to the rate of void coins earned.
		public static float lastTime = 0; // last time trigger that gave a void coin
		public static float interval = 10; // current time interval to get a void coin + some other updates

		// interactable intervals and times
		public static float lastChestTime = 0;
		public static float intervalChest = 3600; // interval to spawn a void chest, decreases with more void coins.
		public static float lastBarrelTime = 0;
		public static float intervalBarrel = 3600; // interval to spawn a void barrel, decreases with more void coins.
		public static float lastTripleTime = 0;
		public static float intervalTriple = 3600; // interval to spawn a void triple choice, decreases with more void coins.

		public static int currentStageClearCount = 0;

		public static Random r = new Random();

		public static bool deadlock = false;
		public static bool loadingStage = false;

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

			On.RoR2.Run.AdvanceStage += (On.RoR2.Run.orig_AdvanceStage orig, global::RoR2.Run self, global::RoR2.SceneDef nextScene) =>
			{
				orig.Invoke(self, nextScene);
				if (ArtifactEnabled)
				{
					loadingStage = true;
				}
			};

			On.RoR2.InteractableSpawnCard.Spawn += (On.RoR2.InteractableSpawnCard.orig_Spawn orig, global::RoR2.InteractableSpawnCard self, Vector3 position, Quaternion rotation, global::RoR2.DirectorSpawnRequest directorSpawnRequest, ref global::RoR2.SpawnCard.SpawnResult result) =>
			{
				orig.Invoke(self, position, rotation, directorSpawnRequest, ref result);
				var resources = Resources.LoadAll("spawncards/");
				for (int i = 0; i < resources.Length; i++)
				{
					Debug.Log(resources[i].name);
				}
			};

			// change stats / name of Umbra character I.E shadow clone on spawn
			On.RoR2.CharacterMaster.OnBodyStart += (orig_OnBodyStart orig, global::RoR2.CharacterMaster self, global::RoR2.CharacterBody body) =>
			{
				orig.Invoke(self, body);
				if (ArtifactEnabled && NetworkServer.active)
				{
					if (body.isBoss)
					{
						VoidBuffer.BuffBoss(body);
					}

					if (body.isPlayerControlled && loadingStage)
					{
						SinSystem.progressStage();
						loadingStage = false;
					}
				}

				if (ArtifactEnabled && body.isPlayerControlled)
				{
					body.baseMaxHealth *= SinSystem.healthMult;
					body.levelMaxHealth *= SinSystem.healthMult;
				}
			};

			On.RoR2.CharacterBody.OnModelChanged += (On.RoR2.CharacterBody.orig_OnModelChanged orig, global::RoR2.CharacterBody body, Transform modelTransform) =>
			{
				orig.Invoke(body, modelTransform);
				if (ArtifactEnabled && NetworkServer.active)
				{
					Debug.Log("monster -> " + body.name + " body.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>().isDoppelganger == " + body.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>().isDoppelganger);
					if (body.teamComponent.teamIndex == TeamIndex.Monster && body.name.Contains("Commando"))
					{
						Debug.Log("IsDoppleGanger - Init");
						body.baseNameToken = PackSpawner.nextPackName;
						body.master.name = PackSpawner.nextPackName;
						body.subtitleNameToken = PackSpawner.nextPackName;
					}
				}
			};

			On.EntityStates.Duplicator.Duplicating.OnEnter += (On.EntityStates.Duplicator.Duplicating.orig_OnEnter orig, global::EntityStates.Duplicator.Duplicating self) =>
			{
				orig.Invoke(self);
				if (ArtifactEnabled && NetworkServer.active)
				{
					SinSystem.lustProc(true);
				}
			};

			// pride sin hook
			On.RoR2.ShrineBossBehavior.AddShrineStack += (On.RoR2.ShrineBossBehavior.orig_AddShrineStack orig, ShrineBossBehavior self, Interactor activator) =>
			{
				orig.Invoke(self, activator);
				if (ArtifactEnabled && NetworkServer.active && activator != null)
				{
					SinSystem.prideProc(true);
				}
			};

			// stat recalulation based on sin accumulation
			On.RoR2.CharacterBody.RecalculateStats += (On.RoR2.CharacterBody.orig_RecalculateStats orig, global::RoR2.CharacterBody self) =>
			{
				orig.Invoke(self);
				if (ArtifactEnabled)
				{
					self.attackSpeed *= SinSystem.attackSpeedMult;
					self.moveSpeed *= SinSystem.speedMult;
					self.damage *= SinSystem.damageMult;
				}
			};

			On.RoR2.CharacterBody.OnTakeDamageServer += (On.RoR2.CharacterBody.orig_OnTakeDamageServer orig, global::RoR2.CharacterBody self, global::RoR2.DamageReport damageReport) =>
			{
				orig.Invoke(self, damageReport);
				// envy code
				if (ArtifactEnabled && NetworkServer.active && damageReport.victimBody.isPlayerControlled && !damageReport.attackerBody.isPlayerControlled)
				{
					// add hit data
					dmgHitTake hit = new dmgHitTake();
					hit.dmgPercent = damageReport.damageDealt / self.healthComponent.fullCombinedHealth;
					hit.hitTime = RoR2.Run.instance.fixedTime;
					hit.userID = self.netId.Value;
					SinSystem.hitHistory.Add(hit);

					Debug.Log("Hit DmgPercent : " + hit.dmgPercent);
					Debug.Log("Hit hitTime : " + RoR2.Run.instance.fixedTime);
					Debug.Log("Hit userID : " + self.netId.Value);

					// go through hit data and see if it exceeds the percentage needed
					// delete any out of date times
					float dmgTotalPercent = 0.0f;
					List<dmgHitTake> takeList = new List<dmgHitTake>();
					for (int i = 0; i < SinSystem.hitHistory.Count; i++)
					{
						dmgHitTake currentHit = SinSystem.hitHistory[i];
						if ((RoR2.Run.instance.fixedTime - currentHit.hitTime) <= 2)
						{
							if (currentHit.userID == self.netId.Value)
							{
								Debug.Log("Hit match percent : " + currentHit.dmgPercent);
								dmgTotalPercent += currentHit.dmgPercent;
								takeList.Add(currentHit);
							}
							else
							{
								Debug.Log("Hit match reject");
							}
						}
						else
						{
							Debug.Log("Hit match DESTROY (hitTime > 2)");
							SinSystem.hitHistory.Remove(currentHit);
						}

						if (dmgTotalPercent >= 0.7)
						{
							SinSystem.envyProc(true);
							for (int x = 0; x < takeList.Count; x++)
							{
								if (SinSystem.hitHistory.Contains(takeList[x]))
								{
									SinSystem.hitHistory.Remove(takeList[x]);
								}
							}
							break;
						}
					}

					// make monster take damage from hit
					if (SinSystem.reflectMult > 0)
					{
						Debug.Log("Reflecting: " + (SinSystem.reflectMult * 100) + "% of damage to " + damageReport.attackerBody);
						DamageInfo dmg = new DamageInfo();
						dmg.damage = SinSystem.reflectMult * damageReport.damageDealt;
						dmg.attacker = self.gameObject;
						dmg.damageType = DamageType.BypassArmor;
						dmg.procCoefficient = 3f;
						damageReport.attackerBody.healthComponent.TakeDamage(dmg);
					}
				}
			};

			On.RoR2.CharacterBody.OnDeathStart += (On.RoR2.CharacterBody.orig_OnDeathStart orig, global::RoR2.CharacterBody self) =>
			{
				orig.Invoke(self);
				// wrath code
				if (ArtifactEnabled && NetworkServer.active && self.teamComponent.teamIndex == TeamIndex.Monster)
				{
					// add kill data
					SinSystem.killHistory.Add(RoR2.Run.instance.fixedTime);
					Debug.Log("killtime : " + RoR2.Run.instance.fixedTime);

					List<float> killList = new List<float>();
					int kills = 0;
					for (int i = 0; i < SinSystem.killHistory.Count; i++)
					{
						float currentKill = SinSystem.killHistory[i];
						if ((RoR2.Run.instance.fixedTime - currentKill) <= 1)
						{
							kills++;
							killList.Add(currentKill);
						}
						else
						{
							Debug.Log("kill match DESTROY (hitTime > 1)");
							SinSystem.killHistory.Remove(currentKill);
						}
					}

					Debug.Log("Kills = " + kills);
					if (kills >= 5)
					{
						SinSystem.wrathProc(true, (uint)kills);
						for (int x = 0; x < killList.Count; x++)
						{
							if (SinSystem.killHistory.Contains(killList[x]))
							{
								SinSystem.killHistory.Remove(killList[x]);
							}
						}
					}
				}
			};

			/*
			On.RoR2.CharacterBody.OnClientBuffsChanged += (On.RoR2.CharacterBody.orig_OnClientBuffsChanged orig, global::RoR2.CharacterBody self) =>
            {
				orig.Invoke(self);
				RoR2.DLC1Content.Buffs.Fracture.
				self.activeBuffsList[0] = RoR2.DLC1Content.Buffs.Fracture.
            }
		*/

			On.RoR2.Run.Update += (On.RoR2.Run.orig_Update orig, global::RoR2.Run self) =>
			{
				orig.Invoke(self);

				// test hook
				if (ArtifactEnabled && Input.GetKeyDown(KeyCode.Alpha0))
				{
					totalVoidCoins = ((uint)(totalVoidCoins * 1.1f)) + 10;
				}

				if (ArtifactEnabled && Input.GetKeyDown(KeyCode.Alpha9))
				{
					totalVoidCoins += 10;
				}

				if (ArtifactEnabled && NetworkServer.active && stageActive())
				{
					intervalChest = (3600 - Math.Min(totalVoidCoins, 500)) / (1f + 0.2f * totalVoidCoins);
					intervalBarrel = (3600 - Math.Min(totalVoidCoins, 500)) / (1f + 0.24f * totalVoidCoins);
					intervalTriple = (3600 - Math.Min(totalVoidCoins, 500)) / (1f + 0.32f * totalVoidCoins);

					SinSystem.slothProc(true);

					// void barrel logic
					if (ArtifactEnabled && NetworkServer.active && RoR2.Run.instance.fixedTime > (lastChestTime + intervalChest))
					{
						spawnVoidChest();
						Debug.Log("lastChestTime = " + lastChestTime);
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
						VCUPDATE();
					}
				}
			};

		}

		public static void VCUPDATE()
		{
			// get void item count for all players
			setTotalVoidItems();

			interval = (float)(10 / Math.Pow(0.4 * totalVoidItems + 1, 0.7));

			Debug.Log("Current VC interval:: " + interval);

			lastTime = RoR2.Run.instance.fixedTime; // update prev time
													//interval /= 1.1f; // for fun only 
													//PackSpawner.spawnPack();
			double outcome = new Random().NextDouble();
			double chanceToSpawnPack = Math.Min(Math.Min(Math.Pow((totalVoidCoins * 0.004), 2), 3.0) * 0.002, 0.5) * interval;
			Debug.Log("Mob Group spawn chance:: " + chanceToSpawnPack);
			if (outcome < chanceToSpawnPack)
			{
				PackSpawner.randomPack();
			}

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
				totalVoidCoins += 1;
				u.master.voidCoins = totalVoidCoins;
			}
		}

		public static void setTotalVoidItems()
		{
			totalVoidItems = 0;
			foreach (NetworkUser u in NetworkUser.readOnlyInstancesList)
			{
				for (int i = 0; i < u.master.inventory.itemAcquisitionOrder.Count; i++)
				{
					ItemIndex item = u.master.inventory.itemAcquisitionOrder[i];
					if (VoidItemMatcher.isVoidItem(item))
					{
						totalVoidItems += u.master.inventory.itemStacks[i];
					}
				}
			}
			Debug.Log("Current Void Item Count: " + totalVoidItems);
		}

		public static void spawnVoidSeed()
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
				interval = 10; // current time interval to get a void coin
				lastChestTime = 0;
				lastBarrelTime = 0;
				lastTripleTime = 0;
				intervalBarrel = 3600;
				intervalChest = 3600;
				intervalTriple = 3600;
				currentStageClearCount = 0;
				loadingStage = true;
				r = new Random();
				SinSystem.selectSin();

				// sin system vars
				SinSystem.slothStartTime = 0;
				SinSystem.healthMult = 1;
				SinSystem.damageMult = 1;
				SinSystem.speedMult = 1;
				SinSystem.attackSpeedMult = 1;
				SinSystem.hitHistory.Clear();
				SinSystem.reflectMult = 0;
			}

		}
	}

}

