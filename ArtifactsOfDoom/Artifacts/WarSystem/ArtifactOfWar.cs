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

namespace ArtifactGroup
{
	public class ArtifactOfWar : ArtifactBase
	{
		///
		/// List of available item names that will be included in the involve aspect of this artifact.
		///
		public static bool enabled = false;
		bool loadingStage = false;
		bool deadLock = false;
		String currentSpawn = "";

		String[] t1Items = {"BleedOnHit", "ChainLightning", "CritGlasses",
			"FlatHealth", "Hoof", "HealWhileSafe", "NearbyDamageBonus", "SprintBonus",
			"StickyBomb", "StunChanceOnHit"};
		String[] t2Items = { "Syringe", "AttackSpeedOnCrit", "ArmorPlate", "Knurl", "HealOnCrit", 
			"Missile", "Seed", "Pearl", "SprintOutOfCombat", "SprintArmor"};
		String[] t3Items = {"AlienHead", "ArmorReductionOnHit", "BarrierOnOverHeal", "IncreaseHealing"
		, "PersonalShield","BleedOnHitAndExplode", "FireballsOnHit", "FireRing", 
			"HealingPotionConsumed", "PrimarySkillShuriken", "StunChanceOnHit", "MoreMissile",
			"FragileDamageBonus", "CritDamage", "BleedOnHitAndExplode", "FlatHealth", "SiphonOnLowHealth", 
			"ShinyPearl"};
		String[] ultraRares = { "Behemoth", "CaptainDefenseMatrix", "Clover", "SlowOnHit", "BeetleGland", "RoboBallBuddy", "IceRing", "Bear", "ShockNearby", "Medkit"};
		public static String[] lunarItems = { "LunarPrimaryReplacement", "LunarUtilityReplacement", "AutoCastEquipment", "RepeatHeal", 
			"ShieldOnly", "LunarDagger", "GoldOnHit", "RandomDamageZone", "LunarTrinket"};
		// items that are evolve capped to a value
		String[] enemy_limiters = { "Hoof", "SprintOutOfCombat", "SprintBonus" };

		// evolve cap counter
		int[] enemy_limits = { 8, 3, 8 };

		int[] enemy_counters = { 0, 0, 0 };

		// items that are player capped to a value
		String[] player_limiters = { "Hoof", "SprintOutOfCombat", "SprintBonus" };

		// player cap counter
		int[] player_limits = { 7, 7, 0 };
		// at cap hit a logarithm is applied to reduce droprate
		int[] player_counters = { 0, 0, 0 };

		/// <summary>
		/// A temporary list that prevents recursive spawning, resulting in a crash.
		/// </summary>
		List<string> dupelist = new List<string>();

		/// <summary>
		/// Prevents Swarm aspect of Artifact from spawning other utilities (Monsters only desired)
		/// </summary>
		List<String> blacklist = new List<String>() {"PodGroundImpact(Clone)", "Teleporter1", "CategoryChestHealing","Barrel1","Chest1",
			"TripleShop", "EquipmentBarrel","ShrineHealing","Drone2Broken","Turret1Broken"
			,"Teleporter1(Clone)", "CategoryChestHealing(Clone)","Barrel1(Clone)","Chest1(Clone)",
			"TripleShop(Clone)", "EquipmentBarrel(Clone)","ShrineHealing(Clone)","Drone2Broken(Clone)","Turret1Broken(Clone)"
		, "ShrineBoss", "LunarTeleporter Variant(Clone)", "LunarTeleporter Variant", "ShrineChance", "ShrineChance(Clone)",
			"PortalGoldShores(Clone)", "PortalGoldShores(Clone)", "iscPortalGoldShores", "iscGoldshoresPortal", "iscVoidCamp", "iscVoidChest", "iscVoidCoinBarrel", 
			"VoidBarnacleMaster(Clone)", "scVoidCampTallGrassCluster1", "cscVoidBarnacle", "iscVoidChestSacrificeOn",
			"iscShopPortal", "iscCategoryChestUtility", "iscLunarChest", "iscFreeChest", "iscCategoryChestDamage", "iscDuplicatorLarge", 
			"DirectorSpawnProbeHelperPrefab(Clone)", "iscTripleShopEquipment", "iscDuplicator", "iscChest2", "iscShrineBlood", "iscShrineCleanse",
			"iscBrokenDrone1", "iscBrokenEquipmentDrone", "iscBrokenEmergencyDrone", "cscDroneCommander", "iscCategoryChestDamage", "iscScrapper",
		"iscChest2", "iscVoidChest", "iscCategoryChestUtility", "iscShrineBlood", "iscShrineChanceSnowy",
			"iscLockbox", "iscTripleShopLarge", "iscShrineCombat", "iscShrineBossSnowy", "iscShrineBoss", "iscShrineBloodSnowy",
			"iscShrineCombatSnowy", "iscShrineHealingSnowy", "iscDuplicatorWild", "iscShopPortal","iscRadarTower", "iscGoldshoresBeacon"};

		int storedStage = -1; // allows updates of stages upon loading them
		int spike = 1; // spike in swarms in the void fields
		bool spiked = false;

		public static ConfigEntry<int> TimesToPrintMessageOnStart;
		public override string ArtifactName => "Artifact of War";
		public override string ArtifactLangTokenName => "ARTIFACT_OF_WAR";
		public override string ArtifactDescription => "Living is Pain. Life is Chaos.\n" +
			"Time increase = ++Spawn Rates, +++Difficulty\n" +
			"Stage increase = ++Evolution, +++Item Droprate\n" +
			"All increases are exponential.\n" +
			"BEWARE SPEEDRUNNERS! SIDE EFFECTS OF RUSHING ARE DEATH, DEATH, and MORE DEATH!!!\n";
		public override Sprite ArtifactEnabledIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/ArtifactOfWar.png");
		public override Sprite ArtifactDisabledIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/ArtifactOfWarDisabled.png");

		public override void Init()
		{
			//CreateConfig(config);
			CreateLang();
			CreateArtifact();
			Hooks();
			Debug.Log("WAR INVOKED");
		}
		public override void Hooks()
		{
			/// We can Access the Run object on initialization here...
			Run.onRunStartGlobal += overrides;

			RoR2.SpawnCard.onSpawnedServerGlobal += SwarmMultiplier;

			On.RoR2.GlobalEventManager.OnCharacterLevelUp += (orig_OnCharacterLevelUp orig, global::RoR2.CharacterBody characterBody) =>
			{
				if (ArtifactOfUnity.enabled == false && NetworkServer.active && ArtifactEnabled)
				{
					// check if the player leveled up
					if (characterBody.teamComponent.teamIndex == TeamIndex.Player)
					{
						/// Scale the health given each time
						//characterBody.baseMaxHealth = characterBody.baseMaxHealth * (float)(((characterBody.level) * 0.2) + 1);
						characterBody.baseMaxHealth = scalePlayerHealth(characterBody.levelMaxHealth, characterBody.level);
						
						// inform client of new max health
						uint idTarget = characterBody.networkIdentity.netId.Value;
						new networkBehavior.informMaxHealth(idTarget, characterBody.baseMaxHealth);
					}
				}
				else
				{
					orig.Invoke(characterBody);
				}
			};

			On.RoR2.Run.Start += (orig_Start orig, global::RoR2.Run self) =>
			{
				orig.Invoke(self);
				ArtifactOfTitans.desynchronizedMonsters.Clear(); // weird to put it here but C# was really stubborn with this particular hook

				// Define a TeamDef object (from original)
				TeamDef def = RoR2.TeamCatalog.GetTeamDef(TeamIndex.Monster);

				float spawnCap = 250;
				try
				{
					spawnCap = Math.Max(float.Parse(OptionsLink.AOW_EntityCap.Value), 0.001f);
				}
				catch
				{
					MessageHandler.globalMessage("Cannot Interpret Spawn Cap Value in the mod settings! Try changing the syntax!");
				}

				def.softCharacterLimit = (int)spawnCap; // limit adjustment

				// replace in def
				RoR2.TeamCatalog.Register(TeamIndex.Monster, def);

				// test
				Debug.Log("Enemy Spawn Limit Adjusted -> " + RoR2.TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit);
			};
			On.RoR2.CharacterMaster.OnBodyStart += (orig_OnBodyStart orig, global::RoR2.CharacterMaster self, global::RoR2.CharacterBody body) =>
			{
				orig.Invoke(self, body);
				if (NetworkServer.active && ArtifactEnabled)
				{
					if (body.teamComponent.teamIndex == TeamIndex.Monster)
					{
						body.baseMaxHealth = scaleMonsterHealth(body.levelMaxHealth, body.level);
						body.baseDamage *= Math.Max((float)Math.Pow(RoR2.Run.instance.difficultyCoefficient / 16, Math.Max(RoR2.Run.instance.stageClearCount, 4) + 1) + 1, 1);

						// inform client of new max health
						uint idTarget = body.networkIdentity.netId.Value;
						new networkBehavior.informMaxHealth(idTarget, body.baseMaxHealth);
					}

					//Debug.Log("DmgMult: " + ((float)Math.Pow(RoR2.Run.instance.difficultyCoefficient / 32, Math.Max(RoR2.Run.instance.stageClearCount - 1.6, 0))));
				}
			};
			On.RoR2.GlobalEventManager.OnCharacterHitGroundServer += (orig_OnCharacterHitGroundServer orig, global::RoR2.GlobalEventManager self, global::RoR2.CharacterBody characterBody, Vector3 impactVelocity) =>
			{
				orig.Invoke(self, characterBody, impactVelocity);
				if (NetworkServer.active && ArtifactEnabled)
				{
					if (characterBody.teamComponent.teamIndex == TeamIndex.Player)
					{
						characterBody.baseMaxHealth = scalePlayerHealth(characterBody.levelMaxHealth, characterBody.level);
						
						// inform client of new max health
						uint idTarget = characterBody.networkIdentity.netId.Value;
						new networkBehavior.informMaxHealth(idTarget, characterBody.baseMaxHealth);
					}
				}
				if (NetworkServer.active && characterBody.teamComponent.teamIndex == TeamIndex.Player && loadingStage == true && ArtifactEnabled)
				{
					if (deadLock == false)
					{
						loadingStage = false;
						deadLock = true;
					}
				}
			};
			GlobalEventManager.onCharacterDeathGlobal += new Action<DamageReport>(this.SacrificeDrop);
		}

		/// <summary>
		/// Health scaling formula for this artifact
		/// </summary>
		/// <param name="health"></param>
		/// <returns></returns>
		public static float scalePlayerHealth(float levelMaxHealth, float level)
		{
			// there is stage scaling since things can get quite violent
			return (float)((float)Math.Pow((levelMaxHealth * 2) * (((level * 0.1) + 1)) * (Math.Pow(((RoR2.Run.instance.stageClearCount * 0.2 + 1) + 0.8), 2) + 1), 1 + (RoR2.Run.instance.stageClearCount * 0.05)));
		}


		/// <summary>
		/// Health scaling formula for this artifact
		/// </summary>
		/// <param name="health"></param>
		/// <returns></returns>
		/// <returns></returns>
		public static float scaleMonsterHealth(float levelMaxHealth, float level)
		{
			return (float)((float)Math.Pow((levelMaxHealth * 1.3 + 140) * ((((level) * 0.17) + 1)), 0.99 + RoR2.Run.instance.difficultyCoefficient * 0.01));
		}

		private void overrides(Run run)
		{
			if (NetworkServer.active && ArtifactEnabled)
			{
				MessageHandler.globalMessage("The Brutality of War has been enabled.");
				Run.baseGravity = 1;
				storedStage = -1;
				//for(int i = 0; i < ArtifactCatalog.artifactDefs.Length; i++)
				//{
				//	Debug.Log(ArtifactCatalog.artifactDefs[i].cachedName);
				//}
				// force evolution artifact
				RunArtifactManager.instance.SetArtifactEnabled(ArtifactCatalog.FindArtifactDef("MonsterTeamGainsItems"), true);
				enabled = true;
			}

		}
		/// <summary>
		/// Serves as a modified swarms artifact. Multiplies with swarms.
		/// </summary>
		/// <param name="spawn"></param>
		private void SwarmMultiplier(SpawnCard.SpawnResult spawn)
		{
			if (NetworkServer.active && ArtifactEnabled)
			{
				// time in seconds
				float time = RoR2.Run.instance.fixedTime;

				// swarms multiplier for monsters (max 5)
				float divider = 300;
                try
                {
					divider = Math.Max(float.Parse(OptionsLink.AOW_SwarmTime.Value), 0.001f);
                }
				catch
                {
					MessageHandler.globalMessage("Cannot Interpret Swarm Time Value in the mod settings! Try changing the syntax!");
                }

				float swarmCap = 5;
				try
				{
					swarmCap = Math.Max(float.Parse(OptionsLink.AOW_MaxSwarm.Value), 0.001f);
				}
				catch
				{
					MessageHandler.globalMessage("Cannot Interpret Swarm Cap Value in the mod settings! Try changing the syntax!");
				}

				float SwarmsCoeffcient = Math.Min((float)(time / (divider * 60)), swarmCap);
				//Debug.Log("COEFF = " + SwarmsCoeffcient.ToString());
				//Debug.Log("COEFFFFFF = " + Math.Min((float)(time / 300), 5).ToString());

				//Debug.Log("Swarms Coefficient = " + SwarmsCoeffcient.ToString());
				//Debug.Log("Testing: " + spawn.spawnedInstance.name.ToString());
				if ((RoR2.Run.instance.stageClearCount + 1) > storedStage)
				{
					spike = 1;
					spiked = false;
					// we are in a new scene (in most cases)
					//Debug.Log("evolve call");
					MessageHandler.globalMessage("Stage " + (RoR2.Run.instance.stageClearCount + 1).ToString() + " item droprate: " + (OptionsLink.AOW_BaseDropChance.Value * Math.Pow(((Run.instance.stageClearCount) * 1+1), OptionsLink.AOW_DropChanceExpScaling.Value)).ToString());
					storedStage = RoR2.Run.instance.stageClearCount + 1;
					this.evolveEnemies();
					if (loadingStage == false)
					{
						deadLock = false;
					}
					loadingStage = true;
				}

				// handler for the void fields (hell zone)
				if (RoR2.SceneCatalog.currentSceneDef.cachedName.CompareTo("arena") == 0 && spiked == false)
				{
					//Debug.Log("Scene Spiked");
					spike = 60;
					spiked = true;
					loadingStage = true;
				}
				try
				{
					// !blacklist.Contains(spawn.spawnedInstance.name)
					if (containsID(dupelist, spawn.spawnedInstance.name) == false && !blacklist.Contains(spawn.spawnedInstance.name))
					{
						if (containsID(dupelist, spawn.spawnedInstance.name) == false)
						{
							dupelist.Clear();
							for (int i = 0; i < Math.Floor(SwarmsCoeffcient); i++)
							{
								currentSpawn = spawn.spawnedInstance.name;
								dupelist.Clear();
								DirectorCore.spawnedObjects.Capacity = 99999;
								RoR2.SceneDirector.cardSelector.Capacity = 99999;
								dupelist.Add(spawn.spawnedInstance.name);
								DirectorSpawnRequest directorSpawnRequest = spawn.spawnRequest;
								directorSpawnRequest.ignoreTeamMemberLimit = true;
								//Debug.Log(directorSpawnRequest.teamIndexOverride.ToString());
								if (!blacklist.Contains(directorSpawnRequest.spawnCard.name))
								{
									Debug.Log("Spawned extra Instance of " + currentSpawn + " :: " + directorSpawnRequest.spawnCard.name + " COEFF: " + SwarmsCoeffcient);
									DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
								}
								//directorSpawnRequest.spawnCard.DoSpawn(superPosition, new Quaternion(), directorSpawnRequest);
								//DirectorCore.spawnedObjects.Add(directorSpawnRequest.summonerBodyObject);
							}

						}
						else
						{
							//Debug.Log("Blocked");
						}
					}
					else
					{
						dupelist.Clear();
					}
				}
				catch
				{
					Debug.Log("(AOW) ERROR");
				}
			}
		}

		/// <summary>
		/// Triggers on death, serves the same role as artifact 
		/// of Evolution.
		/// </summary>
		private void SacrificeDrop(global::RoR2.DamageReport damageReport)
		{
			if (NetworkServer.active && ArtifactEnabled)
			{
				/// drop chance scales with stages and time
				double baseDropChance = OptionsLink.AOW_BaseDropChance.Value; // 8

				Debug.Log("Base Drop Chance = " + baseDropChance.ToString());

				// evaluate the true drop chance now
				baseDropChance = baseDropChance * Math.Pow(((Run.instance.stageClearCount) * 1)+1, OptionsLink.AOW_DropChanceExpScaling.Value);

				Random rnd = new Random();
				int gen = rnd.Next(1, 100);

				Debug.Log("Calculated drop chance: " + baseDropChance.ToString());

				if (gen <= baseDropChance)
				{
					// we drop an item at the death location
					Vector3 locale = damageReport.victim.body.corePosition;
					Debug.Log("Spawning Item...");
					giveItem(locale, damageReport.attackerBody);
				}

				// Time modification in the void fields
				if (spiked == true)
				{
					RoR2.Run.instance.time += 1 + (RoR2.Run.instance.difficultyCoefficient / 4);
					RoR2.Run.instance.fixedTime += 1 + (RoR2.Run.instance.difficultyCoefficient / 4);
				}
			}
		}

		/// <summary>
		/// Give a random item at a select position to the player
		/// </summary>
		public void giveItem(Vector3 locale, CharacterBody ch)
		{
			// item index lists
			List<ItemIndex> commons = ItemCatalog.tier1ItemList;
			List<ItemIndex> rares = ItemCatalog.tier2ItemList;
			List<ItemIndex> legendaries = ItemCatalog.tier3ItemList;

			// rarity drop chances
			double rareDropChance = 70.0;
			double legendaryDropChance = 96.8;

			// determine what rarity will drop
			Random rnd = new Random();
			double gen = rnd.Next(1, 100);
			int colorID = 0;

			ItemIndex dex;
		
			if (gen >= legendaryDropChance)
			{
				Debug.Log("Spawning Legendary...");
				int len = legendaries.Count;
				int index = rnd.Next(0, len);
				dex = legendaries[index];
				colorID = 3;
			}
			else if (gen >= rareDropChance)
			{
				Debug.Log("Spawning Rare...");
				int len = rares.Count;
				int index = rnd.Next(0, len);
				dex = rares[index];
				colorID = 2;
			}
			else
			{
				Debug.Log("Spawning Common...");
				int len = commons.Count;
				int index = rnd.Next(0, len);
				dex = commons[index];
				colorID = 1;

			}

			String itemName = PickupCatalog.FindPickupIndex(dex).pickupDef.nameToken;

			if (containsStr(player_limiters, itemName) != -1)
			{
				// player softcap
				if (player_counters[containsStr(player_limiters, itemName)] >= player_limits[containsStr(player_limiters, itemName)])
				{
					// roll a logarithmic chance to ignore this give.
					int roll = rnd.Next(0, 100);

					// log adjustment vs roll
					if (roll > Math.Log(player_counters[containsStr(player_limiters, itemName)] -
						player_limits[containsStr(player_limiters, itemName)], 1.024))
					{
						// continue on normally
						Debug.Log("Item Roll: " + roll + " > " + Math.Log(player_counters[containsStr(player_limiters, itemName)] -
						player_limits[containsStr(player_limiters, itemName)], 1.024));
					}
					else
					{
						// nope
						Debug.Log("Capped item Blocked");
						return;
					}
				}
				else
				{
					player_counters[containsStr(player_limiters, itemName)]++;

				}
			}


			int plyr = rnd.Next(0, PlayerCharacterMasterController.instances.Count);
			//Debug.Log("Amount of players in Master Controller = " + PlayerCharacterMasterController.instances.Count);
			for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
			{
				Debug.Log(PlayerCharacterMasterController.instances[i]);
			}

			CharacterBody user = NetworkUser.readOnlyInstancesList[plyr].GetCurrentBody();


			if (ArtifactOfUnity.enabled == false)
			{
				NetworkUser.readOnlyInstancesList[plyr].GetCurrentBody().inventory.GiveItem(PickupCatalog.FindPickupIndex(dex).itemIndex);
				MessageHandler.globalItemGetMessage(NetworkUser.readOnlyInstancesList[plyr].GetCurrentBody(), dex, colorID);
			}
			else
			{
				//Debug.Log("Unity Give-->");
				// get item index from item controller
				ItemIndex give = dex;

				for (int x = 0; x < NetworkUser.readOnlyInstancesList.Count; x++)
				{
					if (deathStorage.dead_players.Contains(user.GetUserName()) == false)
					{
						
						NetworkUser.readOnlyInstancesList[x].GetCurrentBody().inventory.GiveItem(give);
						MessageHandler.globalItemGetMessage(NetworkUser.readOnlyInstancesList[x].GetCurrentBody(), dex, colorID);
					}
					else
					{
						ArtifactOfUnity.giveToDeadPlayer(dex, x);
					}
				}
			}
			//PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(dex), locale, transform.forward * 0f);
		}
		void evolveEnemies()
		{
			for (int i = 0; i < RoR2.ItemCatalog.itemNames.Length; i++)
			{
				Debug.Log(RoR2.ItemCatalog.itemNames[i]);
			}

			double items = 0.0;
			if (RoR2.Run.instance.selectedDifficulty == DifficultyIndex.Easy)
			{
				Debug.Log("in ez mode");
				items = Math.Max(Math.Pow(RoR2.Run.instance.difficultyCoefficient + 0.3, OptionsLink.AOW_EvolutionExpScaling.Value), Math.Pow(RoR2.Run.instance.stageClearCount + 2, OptionsLink.AOW_EvolutionExpScaling.Value - 0.7) * 2.5);
			}
			else
			{
				// comparisons built to punish speedrunning exploitation(to an extent)
				if (RoR2.Run.instance.selectedDifficulty == DifficultyIndex.Normal)
				{                    // scaling according to time passed * diffCoeff               vs  strict stage based scaling
					items = Math.Max(Math.Pow(RoR2.Run.instance.difficultyCoefficient + 0.9, OptionsLink.AOW_EvolutionExpScaling.Value), Math.Pow(RoR2.Run.instance.stageClearCount + 2, OptionsLink.AOW_EvolutionExpScaling.Value - 0.6) * 2.7);
				} 
				else
				{                    // scaling according to time passed * diffCoeff               vs  strict stage based scaling
					items = Math.Max(Math.Pow(RoR2.Run.instance.difficultyCoefficient + 0.9, OptionsLink.AOW_EvolutionExpScaling.Value), Math.Pow(RoR2.Run.instance.stageClearCount + 3, OptionsLink.AOW_EvolutionExpScaling.Value - 0.4) * 1.4);
				} 
			}
			Debug.Log("items to give: " + items.ToString());
			// roll for a certain rarity	
			Random rnd = new Random();
			for (int i = 0; i < items; i++)
			{
				bool flag = true;
				while (flag)
				{
					flag = false;
					double t2Rarity = 32.0;
					double t3Rarity = 2.5;
					double ultraRarity = 0.5;

					double gen = rnd.Next(1, 100);
					string selectedItem = "Syringe";
					if (gen < ultraRarity)
					{
						int len = ultraRares.Length;
						int index = rnd.Next(0, len);
						selectedItem = ultraRares[index];
					}
					else if (gen < t3Rarity)
					{
						int len = t3Items.Length;
						int index = rnd.Next(0, len);
						selectedItem = t3Items[index];
					}
					else if (gen < t2Rarity)
					{
						int len = t2Items.Length;
						int index = rnd.Next(0, len);
						selectedItem = t2Items[index];
					}
					else
					{
						int len = t1Items.Length;
						int index = rnd.Next(0, len - 1);
						selectedItem = t1Items[index];
					}

					if (containsStr(enemy_limiters, selectedItem) != -1)
					{
						//Debug.Log("Kappa?");
						// enemy hard cap
						if (enemy_counters[containsStr(enemy_limiters, selectedItem)] >= enemy_limits[containsStr(enemy_limiters, selectedItem)])
						{
							//Debug.Log("Kapped");
							// recursive call to get a different item this roll then the capped ones
							flag = true;
						}
						else
						{
							enemy_counters[containsStr(enemy_limiters, selectedItem)]++;
						}
					}

					if (flag == false)
					{
						Debug.Log("item given: " + " i = " + i.ToString() + " " + selectedItem);
						RoR2.Artifacts.MonsterTeamGainsItemsArtifactManager.monsterTeamInventory.GiveItemString(selectedItem);
					}
				}
			}
		}
		bool containsID(List<string> dupelist, string s)
		{
			for (int i = 0; i < dupelist.Count; i++)
			{
				if (dupelist[i].CompareTo(s) == 0)
				{
					return true;
				}
			}
			return false;
		}

		int containsStr(String[] dlist, string s)
		{
			for (int i = 0; i < dlist.Length; i++)
			{
				//Debug.Log(s + " to " + dlist[i] + "?");
				if (dlist[i].CompareTo(s) == 0)
				{
					//Debug.Log("int return");
					return i;
				}
			}
			//Debug.Log("fail return");
			return -1;
		}
	}

}

