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

namespace ArtifactGroup
{
	class ArtifactOfEntropy : ArtifactBase
	{
		private int loop = 5;
		int storedStage = 0;
		public static int awaitAmogus = 0;

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
			"Figure out what horrible events occur for ANYthing you do, or else... \n General tip: all effects under a certain action will activate at the same time.";
		public override Sprite ArtifactEnabledIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/MonkeAngre.jpg");
		public override Sprite ArtifactDisabledIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/Monke.jpg");

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
				if (this.ArtifactEnabled)
				{
					entropyHost.queueHook("CoinBarrel", getLocalUID(), true);
				}
			};
			On.RoR2.PressurePlateController.SetSwitch += (orig_SetSwitch orig, global::RoR2.PressurePlateController self, bool switchIsDown) =>
			{
				if (this.ArtifactEnabled)
				{
					entropyHost.queueHook("PressurePlate", getLocalUID(), true);
				}
				orig.Invoke(self, switchIsDown);
			};
			On.RoR2.CharacterBody.OnSkillActivated += (orig_OnSkillActivated orig, global::RoR2.CharacterBody self, global::RoR2.GenericSkill skill) =>
			{
				orig.Invoke(self, skill);
				if (this.ArtifactEnabled && self.teamComponent.teamIndex == TeamIndex.Player)
				{
					// check if primary skill is activated
					if (self.skillLocator.FindSkillSlot(skill) == SkillSlot.Primary && NetworkServer.active && self.isActiveAndEnabled)
					{
						if (entropyHost != null || NetworkServer.active)
						{
							entropyHost.queueHook("PrimarySkill", getLocalUID(), true);
						}
					}

					// check if secondary skill is activated
					if (self.skillLocator.FindSkillSlot(skill) == SkillSlot.Secondary && NetworkServer.active && self.isActiveAndEnabled)
					{
						if (entropyHost != null || NetworkServer.active)
						{
							entropyHost.queueHook("SecondarySkill", getLocalUID(), true);
						}
					}

					// check if utility skill is activated
					if (self.skillLocator.FindSkillSlot(skill) == SkillSlot.Utility && NetworkServer.active && self.isActiveAndEnabled)
					{
						if (entropyHost != null || NetworkServer.active)
						{
							entropyHost.queueHook("UtilitySkill", getLocalUID(), true);
						}
					}

					// check if special skill is activated
					if (self.skillLocator.FindSkillSlot(skill) == SkillSlot.Special && NetworkServer.active && self.isActiveAndEnabled)
					{
						if (entropyHost != null || NetworkServer.active)
						{
							entropyHost.queueHook("SpecialSkill", getLocalUID(), true);
						}
					}
				}
			};

			/// We can Access the Run object on initialization here...
			On.RoR2.Run.Update += (orig_Update orig, global::RoR2.Run self) =>
			{
				if (this.ArtifactEnabled) // only do these checks if the artifact is enabled. Preserves frames.
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

					// key based hooks (skill checks) // w
					if (Input.GetKeyDown(KeyCode.W))
					{
						if (entropyHost != null || NetworkServer.active)
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
						if (entropyHost != null || !NetworkServer.active)
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
						if (entropyHost != null || !NetworkServer.active)
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
						if (entropyHost != null || !NetworkServer.active)
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
						if (entropyHost != null || !NetworkServer.active)
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
					entropyHost = new EntropyHost();
					entropyBlaster.airstrikeQueue = 0;
					storedStage = 0;
				}
				orig.Invoke(self);
			};

			// All hooks here link to the entropy host from earlier
			On.RoR2.GlobalEventManager.OnCharacterHitGroundServer += (orig_OnCharacterHitGroundServer orig, global::RoR2.GlobalEventManager self, global::RoR2.CharacterBody characterBody, Vector3 impactVelocity) =>
			{
				if (ArtifactEnabled && NetworkServer.active)
				{
					if (characterBody.teamComponent.teamIndex == TeamIndex.Player && RoR2.Run.instance.fixedTime > 5)
					{
						entropyHost.queueHook("Land", characterBody.networkIdentity.netId.Value, true);
					}
				}
				orig.Invoke(self, characterBody, impactVelocity);
			};

			GlobalEventManager.onCharacterDeathGlobal += (global::RoR2.DamageReport damageReport) =>
			{
				CharacterBody victim = damageReport.victim.body;
				CharacterBody attacker = damageReport.attackerBody;
				if (ArtifactEnabled && NetworkServer.active)
				{
					entropyHost.queueHook("Kill", attacker.networkIdentity.netId.Value, true);
				}
			};

			On.RoR2.CharacterBody.OnTakeDamageServer += (orig_OnTakeDamageServer orig, global::RoR2.CharacterBody self, global::RoR2.DamageReport damageReport) =>
			{
				if (ArtifactEnabled && NetworkServer.active)
				{
					CharacterBody victim = damageReport.victim.body;
					CharacterBody attacker = damageReport.attackerBody;
					if (victim.teamComponent.teamIndex == TeamIndex.Monster && attacker.teamComponent.teamIndex == TeamIndex.Player)
					{
						entropyHost.queueHook("DamageDeal", attacker.networkIdentity.netId.Value, true);
					}
					else if (victim.teamComponent.teamIndex == TeamIndex.Player)
					{
						entropyHost.queueHook("DamageTake", victim.networkIdentity.netId.Value, true);
					}
				}
				orig.Invoke(self, damageReport);
			};

			On.RoR2.TeleporterInteraction.OnBossDirectorSpawnedMonsterServer += (orig_OnBossDirectorSpawnedMonsterServer orig, global::RoR2.TeleporterInteraction self, GameObject masterObject) =>
			{
				if (ArtifactEnabled && NetworkServer.active)
				{
					entropyHost.queueHook("Teleporter", 0 , false);
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
					entropyHost.queueHook("PickupSpawn", 0, false);
				}
				orig.Invoke(pickupIndex, position, velocity);
			};

			On.RoR2.ShrineChanceBehavior.AddShrineStack += (On.RoR2.ShrineChanceBehavior.orig_AddShrineStack orig, ShrineChanceBehavior self, Interactor activator) =>
			{ 
				if (ArtifactEnabled && NetworkServer.active)
				{
					entropyHost.queueHook("ShrineChance", activator.netId.Value, true);
				}
				orig.Invoke(self, activator);
			};
			
			On.RoR2.ShrineBossBehavior.AddShrineStack += (On.RoR2.ShrineBossBehavior.orig_AddShrineStack orig, ShrineBossBehavior self, Interactor activator) =>
			{
				if (ArtifactEnabled && NetworkServer.active)
				{
					entropyHost.queueHook("ShrineBoss", activator.netId.Value, true);
				}
				orig.Invoke(self, activator);
			};

			On.RoR2.ShrineBloodBehavior.AddShrineStack += (On.RoR2.ShrineBloodBehavior.orig_AddShrineStack orig, ShrineBloodBehavior self, Interactor activator) =>
			{
				if (ArtifactEnabled && NetworkServer.active)
				{
					entropyHost.queueHook("ShrineBlood", activator.netId.Value, true);
				}
				orig.Invoke(self, activator);
            };

            On.RoR2.ShrineCleanseBehavior.CleanseInventoryServer += (On.RoR2.ShrineCleanseBehavior.orig_CleanseInventoryServer orig, global::RoR2.Inventory inventory) =>
            {
                if (ArtifactEnabled && NetworkServer.active)
                {
                    entropyHost.queueHook("ShrineCleanse", inventory.netId.Value, true);
                }
                orig.Invoke(inventory);
				return 0;
            };

			On.RoR2.ShrineCombatBehavior.AddShrineStack += (On.RoR2.ShrineCombatBehavior.orig_AddShrineStack orig, ShrineCombatBehavior self, Interactor activator) =>
			{
				if (ArtifactEnabled && NetworkServer.active)
				{
					entropyHost.queueHook("ShrineCombat", activator.netId.Value, true);
				}
				orig.Invoke(self, activator);
			};

			On.RoR2.ShrineHealingBehavior.AddShrineStack += (On.RoR2.ShrineHealingBehavior.orig_AddShrineStack orig, ShrineHealingBehavior self, Interactor activator) =>
			{
				if (ArtifactEnabled && NetworkServer.active)
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
			if (ArtifactEnabled)
			{
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
				Debug.Log("Amogus layer = " + Amogus.layer.ToString());
				Amogus.layer = 0;
			}
		}

		public static void adjustAmogus()
        {
			Amogus.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
			awaitAmogus = 2;
		}

		// get id of player playing the game atm
		uint getLocalUID()
        {
			for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
			{
				var net = PlayerCharacterMasterController.instances[i];
				if (LocalUserManager.GetFirstLocalUser().currentNetworkUser.userName.Equals(net.networkUser.userName))
				{
					return PlayerCharacterMasterController.instances[i].body.netId.Value;
				}
			}
			Messenger.MessageHandler.globalMessage("issue obtaining user with matching usernames... (getLocalUID)");
			return 1;
		}
	}
}