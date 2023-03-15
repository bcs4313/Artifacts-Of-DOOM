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
using static On.RoR2.BarrelInteraction;
using static On.RoR2.PressurePlateController;
using static On.RoR2.GenericSkill;
using static On.RoR2.UI.CurrentRunArtifactDisplayDataDriver;
using orig_OnEnable = On.RoR2.UI.CurrentRunArtifactDisplayDataDriver.orig_OnEnable;
using System.Threading.Tasks;

namespace ArtifactGroup
{
	class ArtifactOfEntropy : ArtifactBase
	{
		private int loop = 5;
		int storedStage = 0;
		public static int awaitAmogus = 0;

		Int32 runSeed = -1;

		public static bool enabled = false;
		Inventory globalInventory = new Inventory();
		public static Random rnd = new Random();
		public static EntropyHost entropyHost;
		private HUD hud = null;
		public static GameObject Amogus;

		public static ConfigEntry<int> TimesToPrintMessageOnStart;
		public override string ArtifactName => "Artifact of Entropy";
		public override string ArtifactLangTokenName => "ARTIFACT_OF_ENTROPY";
		public override string ArtifactDescription => "Random effects are assigned to certain actions, with a randomly defined chance to occur. \n" +
			"Figure out what horrible events occur for ANYthing you do, or else...";
		public override Sprite ArtifactEnabledIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/ArtifactOfEntropy.png");
		public override Sprite ArtifactDisabledIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/ArtifactOfEntropyDisabled.png");

		public override void Init()
		{
			//CreateConfig(config);
			CreateLang();
			CreateArtifact();
			Hooks();
		}
		public override void Hooks()
		{
			On.RoR2.BarrelInteraction.CoinDrop += (orig_CoinDrop orig, global::RoR2.BarrelInteraction self) =>
			{
				orig.Invoke(self);
				if (this.ArtifactEnabled && NetworkServer.active)
				{
					uint target = positionalUID(self.gameObject.transform.position);
					if (target != 0)
					{
						entropyHost.queueHook("CoinBarrel", target, true);
					}
					else
                    {
						Debug.Log("Artifact of Entropy: Coin Barrel Target trigger failed!");
						entropyHost.queueHook("CoinBarrel", getLocalUID(), true);
					}
				}
			};
			On.RoR2.CharacterBody.OnSkillActivated += (orig_OnSkillActivated orig, global::RoR2.CharacterBody self, global::RoR2.GenericSkill skill) =>
			{
				orig.Invoke(self, skill);
				if (this.ArtifactEnabled && NetworkServer.active && self.teamComponent.teamIndex == TeamIndex.Player)
				{
					// check if primary skill is activated
					if (self.skillLocator.FindSkillSlot(skill) == SkillSlot.Primary && self.isActiveAndEnabled)
					{
						if (NetworkServer.active)
						{
							entropyHost.queueHook("PrimarySkill", self.netId.Value, true);
						}
						else
                        {
							new networkBehavior.HookFromClient(self.netId.Value, "PrimarySkill").Send(NetworkDestination.Server);
						}
					}

					// check if secondary skill is activated
					if (self.skillLocator.FindSkillSlot(skill) == SkillSlot.Secondary && self.isActiveAndEnabled)
					{
						if (NetworkServer.active)
						{
							entropyHost.queueHook("SecondarySkill", self.netId.Value, true);
						}
						else
						{
							new networkBehavior.HookFromClient(self.netId.Value, "SecondarySkill").Send(NetworkDestination.Server);
						}
					}

					// check if utility skill is activated
					if (self.skillLocator.FindSkillSlot(skill) == SkillSlot.Utility && self.isActiveAndEnabled)
					{
						if (NetworkServer.active)
						{
							entropyHost.queueHook("UtilitySkill", self.netId.Value, true);
						}
						else
						{
							new networkBehavior.HookFromClient(self.netId.Value, "UtilitySkill").Send(NetworkDestination.Server);
						}
					}

					// check if special skill is activated
					if (self.skillLocator.FindSkillSlot(skill) == SkillSlot.Special && self.isActiveAndEnabled)
					{
						if (NetworkServer.active)
						{
							entropyHost.queueHook("SpecialSkill", self.netId.Value, true);
						}
						else
						{
							new networkBehavior.HookFromClient(self.netId.Value, "SpecialSkill").Send(NetworkDestination.Server);
						}
					}
				}
			};

			/// We can Access the Run object on initialization here...
			On.RoR2.Run.Update += (orig_Update orig, global::RoR2.Run self) =>
			{
				if (this.ArtifactEnabled && NetworkServer.active && RoR2.Run.instance.isRunStopwatchPaused == false) // only do these checks if the artifact is enabled. Preserves frames.
				{
					// airstrike queuer
					if (entropyBlaster.airstrikeQueue > 0)
					{
						if (loop <= 0 && NetworkServer.active)
						{
							loop = 5 / (((entropyBlaster.airstrikeQueue + 50) / 200) + 1);
							Vector3 locale = entropyBlaster.findRandomPosAir();
							BombArtifactManager.bombRequestQueue.Enqueue(new BombArtifactManager.BombRequest());
							BombArtifactManager.BombRequest bombRequest = BombArtifactManager.bombRequestQueue.Dequeue();
							if (ArtifactOfWar.enabled)
							{
								bombRequest.bombBaseDamage = (float)Math.Pow(70 * (RoR2.Run.instance.difficultyCoefficient / 2), (RoR2.Run.instance.stageClearCount * 0.4) + 1);
							}
							else
							{
								bombRequest.bombBaseDamage = 70 + (RoR2.Run.instance.difficultyCoefficient * 5);
							}

							bombRequest.raycastOrigin = locale;

							Ray ray = new Ray(bombRequest.raycastOrigin + new Vector3(0f, 8f, 0f), Vector3.down);
							float maxDistance = BombArtifactManager.maxBombStepUpDistance + BombArtifactManager.maxBombFallDistance;
							RaycastHit raycastHit;
							Physics.Raycast(ray, out raycastHit, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
							BombArtifactManager.SpawnBomb(bombRequest, raycastHit.point.y);
							entropyBlaster.airstrikeQueue -= 1;
						}
						else
						{
							loop--;
						}
					}
					// amogus effect
					if (awaitAmogus > 1)
					{
						awaitAmogus++;
						if (awaitAmogus == 100)
						{
							Amogus.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
							awaitAmogus = 0;
						}
					}
				}

				if (this.ArtifactEnabled && RoR2.Run.instance.isRunStopwatchPaused == false)
				{
					// key based hooks (skill checks) // w
					if (Input.GetKeyDown(KeyCode.W))
					{
						if (NetworkServer.active)
						{
							entropyHost.queueHook("W", getLocalUID(), true);
						}
						else
						{
							new networkBehavior.HookFromClient(getLocalUID(), "W").Send(NetworkDestination.Server);
						}
					}

					// key based hooks (skill checks) // ctrl
					if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
					{
						if (NetworkServer.active)
						{
							entropyHost.queueHook("Ctrl", getLocalUID(), true);
						}
						else
						{
							new networkBehavior.HookFromClient(getLocalUID(), "Ctrl").Send(NetworkDestination.Server);
						}
					}

					// key based hooks (skill checks) // a
					if (Input.GetKeyDown(KeyCode.A))
					{
						if (NetworkServer.active)
						{
							entropyHost.queueHook("A", getLocalUID(), true);
						}
						else
						{
							new networkBehavior.HookFromClient(getLocalUID(), "A").Send(NetworkDestination.Server);
						}
					}

					// key based hooks (skill checks) // s
					if (Input.GetKeyDown(KeyCode.S))
					{
						if (NetworkServer.active)
						{
							entropyHost.queueHook("S", getLocalUID(), true);
						}
						else
						{
							new networkBehavior.HookFromClient(getLocalUID(), "S").Send(NetworkDestination.Server);
						}
					}

					// key based hooks (skill checks) // d
					if (Input.GetKeyDown(KeyCode.D))
					{
						if (NetworkServer.active)
						{
							entropyHost.queueHook("D", getLocalUID(), true);
						}
						else
						{
							new networkBehavior.HookFromClient(getLocalUID(), "D").Send(NetworkDestination.Server);
						}
					}
				}

				orig.Invoke(self);
			};

			On.RoR2.Run.Start += (orig_Start orig, global::RoR2.Run self) =>
			{
				if (ArtifactEnabled && NetworkServer.active)
				{
					// seed the run
					try
                    {
						int seed = int.Parse(OptionsLink.AOE_Seed.Value);
						if (seed == -1)
                        {
							runSeed = new Random().Next(int.MinValue, int.MaxValue);
							rnd = new Random(runSeed);
                        }
						else
                        {
							rnd = new Random(seed);
							runSeed = seed;
						}
						Messenger.MessageHandler.globalMessage("(Artifact of Entropy Seed): " + runSeed);
					}
					catch (Exception e)
                    {
						Messenger.MessageHandler.globalMessage("Issue parsing the entropy seed! Make sure the input is an integer within the range of its description.");
					}
					
					entropyBlaster.airstrikeQueue = 0;
					storedStage = 0;
					entropyHost = new EntropyHost();
					EntropySubsetGen.generateMonsterSubset();
					EntropySubsetGen.generateTransformSubset();
					EntropySubsetGen.generateItemSubset();
				}
				orig.Invoke(self);
			};

			On.RoR2.Run.BeginStage += (orig_BeginStage orig, global::RoR2.Run self) =>
			{
				orig.Invoke(self);
				if (ArtifactEnabled && NetworkServer.active)
				{
					if (RoR2.Run.instance.stageClearCount != 0)
					{
						printSeed(runSeed);
						if(OptionsLink.AOE_NewEventsPerStage.Value == true)
                        {
							entropyHost = new EntropyHost();
							EntropySubsetGen.generateMonsterSubset();
							EntropySubsetGen.generateTransformSubset();
							EntropySubsetGen.generateItemSubset();
						}
					}

				}
			};

			// All hooks here link to the entropy host from earlier
			On.RoR2.GlobalEventManager.OnCharacterHitGroundServer += (orig_OnCharacterHitGroundServer orig, global::RoR2.GlobalEventManager self, global::RoR2.CharacterBody characterBody, Vector3 impactVelocity) =>
			{
				if (ArtifactEnabled && NetworkServer.active)
				{
					if (characterBody != null && characterBody.teamComponent != null
						&& characterBody.networkIdentity != null && characterBody.teamComponent.teamIndex == TeamIndex.Player && RoR2.Run.instance.fixedTime > 5)
					{
						entropyHost.queueHook("Land", characterBody.networkIdentity.netId.Value, true);
					}
				}
				orig.Invoke(self, characterBody, impactVelocity);
			};

			GlobalEventManager.onCharacterDeathGlobal += (global::RoR2.DamageReport damageReport) =>
			{
				if (damageReport != null && damageReport.victim != null && damageReport.attacker != null)
				{
					CharacterBody victim = damageReport.victim.body;
					CharacterBody attacker = damageReport.attackerBody;
					if (ArtifactEnabled && NetworkServer.active)
					{ // applying strict null safety here because it is a major culprit of errors.
						if (attacker != null && victim != null && attacker.teamComponent != null && attacker.teamComponent.teamIndex == TeamIndex.Player &&
						attacker.networkIdentity != null && attacker.networkIdentity.netId != null)
						{
							entropyHost.queueHook("Kill", attacker.networkIdentity.netId.Value, true);
						}
					}
				}
			};

			On.RoR2.CharacterBody.OnTakeDamageServer += (orig_OnTakeDamageServer orig, global::RoR2.CharacterBody self, global::RoR2.DamageReport damageReport) =>
			{
				if (ArtifactEnabled && NetworkServer.active)
				{
					CharacterBody victim = damageReport.victim.body;
					CharacterBody attacker = damageReport.attackerBody;
					// more strict null safety
					if (victim != null && attacker != null && victim.teamComponent != null 
					&& attacker.teamComponent != null && victim.networkIdentity != null && attacker.networkIdentity != null)
					{
						if (victim.teamComponent.teamIndex == TeamIndex.Monster && attacker.teamComponent.teamIndex == TeamIndex.Player)
						{
							entropyHost.queueHook("DamageDeal", attacker.networkIdentity.netId.Value, true);
						}
						else if (victim.teamComponent.teamIndex == TeamIndex.Player)
						{
							entropyHost.queueHook("DamageTake", victim.networkIdentity.netId.Value, true);
						}
					}
				}
				orig.Invoke(self, damageReport);
			};

			On.RoR2.TeleporterInteraction.OnBossDirectorSpawnedMonsterServer += (orig_OnBossDirectorSpawnedMonsterServer orig, global::RoR2.TeleporterInteraction self, GameObject masterObject) =>
			{
				if (ArtifactEnabled && NetworkServer.active)
				{
					// get player closest to the item as our target
					uint target = positionalUID(self.gameObject.transform.position);

					if (target != 0) // if the retrieval was successful
					{
						entropyHost.queueHook("Teleporter", target, true);
					}
					else
					{
						Debug.Log("Artifact of Entropy: Teleporter Target trigger failed!");
						entropyHost.queueHook("Teleporter", 0, false);
					}
				}
				orig.Invoke(self, masterObject);
			};

			On.RoR2.EquipmentSlot.Execute += (orig_Execute orig, global::RoR2.EquipmentSlot self) =>
			{
				if (ArtifactEnabled && NetworkServer.active)
				{
					entropyHost.queueHook("EquipmentActivate", self.characterBody.networkIdentity.netId.Value, true);
				}
				orig.Invoke(self);
			};

			On.RoR2.PickupDropletController.CreatePickupDroplet += (orig_CreatePickupDroplet orig, global::RoR2.PickupIndex pickupIndex, Vector3 position, Vector3 velocity) =>
			{
				if (ArtifactEnabled && NetworkServer.active)
				{
					// get player closest to the item as our target
					uint target = positionalUID(position);

					if (target != 0) // if the retrieval was successful
					{
						entropyHost.queueHook("PickupSpawn", target, true);
					}
					else
					{
						Debug.Log("Artifact of Entropy: Pickup Spawn Target trigger failed!");
						entropyHost.queueHook("PickupSpawn", 0, false);
					}
				}
				orig.Invoke(pickupIndex, position, velocity);
			};

			On.RoR2.ShrineChanceBehavior.AddShrineStack += (On.RoR2.ShrineChanceBehavior.orig_AddShrineStack orig, ShrineChanceBehavior self, Interactor activator) =>
			{ 
				if (ArtifactEnabled && NetworkServer.active && activator != null)
				{
					entropyHost.queueHook("ShrineChance", activator.netId.Value, true);
				}
				orig.Invoke(self, activator);
			};
			
			On.RoR2.ShrineBossBehavior.AddShrineStack += (On.RoR2.ShrineBossBehavior.orig_AddShrineStack orig, ShrineBossBehavior self, Interactor activator) =>
			{
				if (ArtifactEnabled && NetworkServer.active && activator != null)
				{
					entropyHost.queueHook("ShrineBoss", activator.netId.Value, true);
				}
				orig.Invoke(self, activator);
			};

			On.RoR2.ShrineBloodBehavior.AddShrineStack += (On.RoR2.ShrineBloodBehavior.orig_AddShrineStack orig, ShrineBloodBehavior self, Interactor activator) =>
			{
				if (ArtifactEnabled && NetworkServer.active && activator != null)
				{
					entropyHost.queueHook("ShrineBlood", activator.netId.Value, true);
				}
				orig.Invoke(self, activator);
            };

            On.RoR2.ShrineCleanseBehavior.CleanseInventoryServer += (On.RoR2.ShrineCleanseBehavior.orig_CleanseInventoryServer orig, global::RoR2.Inventory inventory) =>
            {
                if (ArtifactEnabled && NetworkServer.active && inventory != null)
                {
                    entropyHost.queueHook("ShrineCleanse", inventory.netId.Value, true);
                }
                orig.Invoke(inventory);
				return 0;
            };

			On.RoR2.ShrineCombatBehavior.AddShrineStack += (On.RoR2.ShrineCombatBehavior.orig_AddShrineStack orig, ShrineCombatBehavior self, Interactor activator) =>
			{
				if (ArtifactEnabled && NetworkServer.active && activator != null)
				{
					entropyHost.queueHook("ShrineCombat", activator.netId.Value, true);
				}
				orig.Invoke(self, activator);
			};

			On.RoR2.ShrineHealingBehavior.AddShrineStack += (On.RoR2.ShrineHealingBehavior.orig_AddShrineStack orig, ShrineHealingBehavior self, Interactor activator) =>
			{
				if (ArtifactEnabled && NetworkServer.active && activator != null)
				{
					entropyHost.queueHook("ShrineHealing", activator.netId.Value, true);
				}
				orig.Invoke(self, activator);
			};

			// now for ability based hooks

			//.RoR2.ShrineBloodBehavior.Start

			// UI amogus hook
			On.RoR2.UI.HUD.Awake += MyFunc;
		}

		void MyFunc(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
		{
			/*
				orig(self); // Don't forget to call this, or the vanilla / other mods' codes will not execute!
				hud = self;
				//hud.mainContainer.transform // This will return the main container. You should put your UI elements under it or its children!
				// Rest of the code is to go here
				Amogus = new GameObject("Amogus");
				Amogus.transform.SetParent(hud.mainUIPanel.transform);
				RectTransform rectTransform = Amogus.AddComponent<RectTransform>();
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = Vector2.one;
				rectTransform.sizeDelta = Vector2.zero;
				rectTransform.anchoredPosition = Vector2.zero;
				Amogus.AddComponent<Image>();
				Amogus.GetComponent<Image>().sprite = Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/Amogus.jpg");
				Amogus.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
				//Debug.Log("Amogus layer = " + Amogus.layer.ToString());
				Amogus.layer = 0;
			*/
		}

		public static void adjustAmogus()
        {
			Amogus.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
			awaitAmogus = 2;
		}

		// get id of player playing th game atm
		uint getLocalUID()
        {
			try
			{
				if (PlayerCharacterMasterController.instances != null)
				{
					for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
					{
						var net = PlayerCharacterMasterController.instances[i];
						if (LocalUserManager.GetFirstLocalUser().currentNetworkUser.userName.Equals(net.networkUser.userName))
						{
							return PlayerCharacterMasterController.instances[i].body.netId.Value;
						}
					}
					Messenger.MessageHandler.globalMessage("issue obtaining user with matching UID... (getLocalUID)");
				}
			}
			catch 
			{
				Debug.Log("(Artifact of Entropy) UID retrieval failed, hook aborted. (This isn't an error, just make sure this doesn't appear too often).");
			}
			return 0;
		}

		// get nearest player and grab their uid
		uint positionalUID(Vector3 origin)
        {
			uint bestPlayerID = 0;
			try
			{
				float closestDistance = float.MaxValue;
				if (NetworkUser.readOnlyInstancesList != null)
				{
					for (int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++)
					{
						var net = NetworkUser.readOnlyInstancesList[i];
						if (net != null && net.GetCurrentBody() != null)
						{
							var distance = Math.Abs((net.GetCurrentBody().corePosition - origin).magnitude);
							if (distance < closestDistance)
							{
								closestDistance = distance;
								bestPlayerID = net.GetCurrentBody().netId.Value;
							}
						}
					}
					if (bestPlayerID == 0)
					{
						Messenger.MessageHandler.globalMessage("issue obtaining user with matching UID... (getLocalUID)");
					}
				}
			}
			catch
			{
				Debug.Log("(Artifact of Entropy) UID retrieval failed, hook aborted. (This isn't an error, just make sure this doesn't appear too often).");
			}
			return bestPlayerID; 
		}

		async public void printSeed(int runSeed)
        {
			// prevents the seed from being hidden in chat
			await Task.Delay(2000);
			Messenger.MessageHandler.globalMessage("(Artifact of Entropy Seed): " + runSeed);
		}
	}
}