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

namespace ArtifactGroup
{
	public class ArtifactOfMorph : ArtifactBase
	{
		// where client spawn stats are stored...
		// basically the respawn from morph occurs AFTER the stats are modified by the client packet.
		// This replaces the gameobject, which changes its scale and stats back to normal in some ways.
		// I hate it. But its true. So we will hook Recalculate stats to force this to work!
		public static List<GameObject> usedOBJS = new List<GameObject>(); // needed to prevent multiple resizes
		public static List<CharacterBody> clientBodies = new List<CharacterBody>();
		public static List<CharacterMaster> clientMasters = new List<CharacterMaster>();
		public static bool bodyTriggered = false;
		public static float sizeMultSync = 1;
		public static float healthMultSync = 1;
		public static float damageMultSync = 1;
		public static float speedMultSync = 1;
		public static float attackSpeedMultSync = 1;
		public static float cooldownMultSync = 1;
		public static String eliteStringSync = "";

		public static ConfigEntry<int> TimesToPrintMessageOnStart;
		public override string ArtifactName => "Artifact of Reconstruction";
		public override string ArtifactLangTokenName => "ARTIFACT_OF_MORPH";
		public override string ArtifactDescription => "Play the game as any monster with any base stats / traits.\nPress the UI shortcut (Default: F2) to open/close the monster selection UI.";
		public override Sprite ArtifactEnabledIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/ArtifactOfMetamorphosis.png");
		public override Sprite ArtifactDisabledIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/ArtifactOfMetamorphosisDisabled.png");

		public static List<uint> recentTransforms = new List<uint>();

		private HUD hud = null;

		public struct resizeAwait
		{

		}

		public static GameObject selectOBJ;
		public static GameObject selectOBJInstance;
		public static GameObject selectUIOBJ;
		public static GameObject statUIOBJ;
		public static StatController statOBJ;

		public override void Init()
		{
			//CreateConfig(config);
			CreateLang();
			CreateArtifact();
			Hooks();
			statOBJ = new StatController(this);
			Debug.Log("MORPH INVOKED");
		}

		void AddMonsterSelect(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
		{
			orig(self); // Don't forget to call this, or the vanilla / other mods' codes will not execute!
			if (ArtifactEnabled)
			{
				hud = self;
				//Debug.Log("0");
				//hud.mainContainer.transform // This will return the main container. You should put your UI elements under it or its children!
				// Rest of the code is to go here
				// instantiate gameobject...
				//Debug.Log("1");
				GameObject selectOBJ = Main.MorphAssets.LoadAsset<GameObject>("Assets/GameObjects/TransformSelectMasterAdvanced.prefab");
				//Debug.Log("SelectOBJ: " + selectOBJ.ToString());
				selectOBJInstance = UnityEngine.Object.Instantiate<GameObject>(selectOBJ);
				//Debug.Log("SelectOBJInstance: " + selectOBJ.ToString());
				selectOBJInstance.transform.SetParent(hud.mainContainer.transform);

				// zero out local position of uiCanvas
				selectOBJInstance.transform.localPosition = Vector3.zero;
				//Debug.Log("2");
				RectTransform rectTransform = selectOBJInstance.GetComponent<RectTransform>();
				//Debug.Log("3");
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = Vector2.one;

				// scale differently based on screen size
				//Debug.Log("Screen width: " + Screen.width + " width / 1920 = " + (((float)Screen.width) / 1920.0f));
				Vector3 scaleVector = new Vector3(rectTransform.localScale.x * (((float)Screen.width) / 1920.0f), rectTransform.localScale.y * (((float)Screen.width) / 1920.0f), rectTransform.localScale.z);
				rectTransform.localScale = scaleVector;

				// more zeroing out...
				selectUIOBJ = selectOBJInstance.transform.GetChild(0).gameObject; // SELECTUIOBJ
				//Debug.Log("selectUIOBJ: " + selectUIOBJ.ToString());
				// EVEN MORE ZEROING OUT...
				
				statUIOBJ = selectOBJInstance.transform.GetChild(1).gameObject; // STATUIOBJ
				
				//Debug.Log("statUIOBJ: " + statUIOBJ.ToString());
				//Debug.Log("4");
				MorphController.injectButtons();

				// could be a custom setting, but this block is just stupid. It screws up the entire artifact lol.
				//if(RoR2.Run.instance.stageClearCount != 0)
                //{
				//	selectOBJInstance.SetActive(false);
				//}
			}
		}

		public static void updateStats(CharacterBody self, bool init)
        {
			if(init == true && bodyTriggered == true)
            {
				Debug.Log("Cancelling stat update... init == true && bodyTriggered == true");
				return;
            }

			// if the body is in the list, the server must have updated the client with
			// its respective stat values. Be sure to modify them.
			if (clientBodies.Contains(self))
			{
				//Debug.Log("(Artifact of Morph) Recalculating stats. " + self);
				var modelLocator = self.modelLocator;

				self.attackSpeed *= attackSpeedMultSync;
				self.moveSpeed *= speedMultSync;
				self.damage *= damageMultSync;

				// flag for preventing multiple scales of a single gameObject, ever.
				if (!usedOBJS.Contains(self.gameObject))
				{
					self.baseMaxHealth *= healthMultSync;
					self.levelMaxHealth *= healthMultSync;
					Vector3 size = self.modelLocator.modelTransform.localScale;
					size.x = (float)sizeMultSync;
					size.y = (float)sizeMultSync;
					size.z = (float)sizeMultSync;
					Debug.Log("Size adjust for " + self.name + "... = " + sizeMultSync);
					Transform modelTransform = modelLocator.modelBaseTransform;
					//Mesh m = c.gameObject.GetComponent<MeshFilter>().sharedMesh;
					if (modelTransform)
					{
						modelLocator.modelTransform.localScale *= sizeMultSync;
					}
					else
					{
						MessageHandler.globalMessage("??");
					}


					NetworkBehavior.setElite(eliteStringSync, self);

					Debug.Log("Size Adjust Done");
					usedOBJS.Add(self.gameObject);
				}
				else
                {
					//Debug.Log("(Artifact of Morph) body rejected (SIZE): " + self + " : " + self.GetUserName() + " :: " + init);
				}
			}
			else
			{
				if (self.isPlayerControlled)
				{
					//Debug.Log("(Artifact of Morph) body rejected: " + self + " : " + self.GetUserName() + " :: " + init);
				}
			}
		}

		public static void setInteractArea(CharacterBody body)
		{
			if (body.isPlayerControlled)
			{
				//Debug.Log("(morph) BodyStartHook");
				//Debug.Log("(morph) PlayerBody = " + body);
				//Debug.Log("Reactive Hook");
				//Debug.Log("1");
				Interactor picker = body.gameObject.GetComponent<Interactor>();
				//Debug.Log("2");
				if (picker != null)
				{
					//Debug.Log("4");
					picker.maxInteractionDistance = (body.corePosition - body.footPosition).magnitude * 4.4f;
				}
				else
				{
					//Debug.Log("Picker is null. Adding Component to monster.");
					picker = body.gameObject.AddComponent<Interactor>();
					InteractionDriver driver = body.gameObject.AddComponent<InteractionDriver>();
					//Debug.Log("Picker now = " + picker);
					picker.maxInteractionDistance = (body.corePosition - body.footPosition).magnitude * 4.4f;
					driver.highlightInteractor = true;
					driver.enabled = true;
					driver.interactor = picker;
				}
				//Debug.Log("3");
				//Debug.Log("Picker new max distance: " + picker.maxInteractionDistance);

				// workaround: get collider component and use its bounds instead.
				Collider c = body.gameObject.GetComponent<Collider>();
				//Debug.Log("5");
				if (c != null)
				{
					//Debug.Log("6");
					picker.maxInteractionDistance = (c.bounds.size.y + c.bounds.size.x + c.bounds.size.z) * 2.5f;
					if (body.gameObject.name.Contains("ClayBoss")) // this entity has a very tiny model for literally no reason
					{
						picker.maxInteractionDistance *= 6.5f;
					}
				}
				else
				{
					//Debug.Log("Collider is null. Searching for alternate colliders to monster.");
					SphereCollider s = body.gameObject.GetComponent<SphereCollider>();
					if (s != null)
					{
						//Debug.Log("Sphere collider found. Adjusting interactor.");
						picker.maxInteractionDistance = (s.bounds.size.y + s.bounds.size.x + s.bounds.size.z) * 2.5f;
					}
					else
					{
						//Debug.Log("No Sphere Collider found. Attempting to find meshcollider");
						MeshCollider m = body.gameObject.GetComponent<MeshCollider>();
						if (m != null)
						{
							//Debug.Log("Mesh collider found. Adjusting interactor.");
							picker.maxInteractionDistance = (m.bounds.size.y + m.bounds.size.x + m.bounds.size.z) * 2.5f;
						}
						else
						{
							//Debug.Log("No Mesh collider found. Resorting to irregular picker area");
						}
					}
				}
			}
		}

		public static String eliteString()
        {
			String eliteString = "";
			GameObject statLayoutPanel = statUIOBJ.transform.GetChild(0).gameObject; // LayoutPanel
			GameObject elitePanel = statLayoutPanel.transform.GetChild(2).gameObject; // ElitePanel

			try
			{
				GameObject elementPanel = elitePanel.transform.GetChild(1).gameObject;
				//Debug.Log("elementPanel = " + elementPanel.ToString());
				if (elementPanel.GetComponent<UnityEngine.UI.Toggle>().isOn)
                {
					eliteString += "Fire";
				}
			}
			catch (Exception e)
            {
				Debug.Log("(Artifacts of Doom) Fire Elite retrieve Failure.");
            }

			try
			{
				GameObject elementPanel = elitePanel.transform.GetChild(2).gameObject;
				//Debug.Log("elementPanel = " + elementPanel.ToString());
				if (elementPanel.GetComponent<UnityEngine.UI.Toggle>().isOn)
				{
					eliteString += "Ice";
				}
			}
			catch (Exception e)
			{
				Debug.Log("(Artifacts of Doom) Ice Elite retrieve Failure.");
			}

			try
			{
				GameObject elementPanel = elitePanel.transform.GetChild(3).gameObject;
				//Debug.Log("elementPanel = " + elementPanel.ToString());
				if (elementPanel.GetComponent<UnityEngine.UI.Toggle>().isOn)
				{
					eliteString += "Lightning";
				}
			}
			catch (Exception e)
			{
				Debug.Log("(Artifacts of Doom) Lightning Elite retrieve Failure.");
			}

			try
			{
				GameObject elementPanel = elitePanel.transform.GetChild(4).gameObject;
				//Debug.Log("elementPanel = " + elementPanel.ToString());
				if (elementPanel.GetComponent<UnityEngine.UI.Toggle>().isOn)
				{
					eliteString += "Celestine";
				}
			}
			catch (Exception e)
			{
				Debug.Log("(Artifacts of Doom) Celestine Elite retrieve Failure.");
			}
			
			try
			{
				GameObject elementPanel = elitePanel.transform.GetChild(5).gameObject;
				//Debug.Log("elementPanel = " + elementPanel.ToString());
				if (elementPanel.GetComponent<UnityEngine.UI.Toggle>().isOn)
				{
					eliteString += "Mending";
				}
			}
			catch (Exception e)
			{
				Debug.Log("(Artifacts of Doom) Mending Elite retrieve Failure.");
			}

			try
			{
				GameObject elementPanel = elitePanel.transform.GetChild(6).gameObject;
				//Debug.Log("elementPanel = " + elementPanel.ToString());
				if (elementPanel.GetComponent<UnityEngine.UI.Toggle>().isOn)
				{
					eliteString += "Perfected";
				}
			}
			catch (Exception e)
			{
				Debug.Log("(Artifacts of Doom) Perfected Elite retrieve Failure.");
			}

			try
			{
				GameObject elementPanel = elitePanel.transform.GetChild(7).gameObject;
				//Debug.Log("elementPanel = " + elementPanel.ToString());
				if (elementPanel.GetComponent<UnityEngine.UI.Toggle>().isOn)
				{
					eliteString += "Malachite";
				}
			}
			catch (Exception e)
			{
				Debug.Log("(Artifacts of Doom) Malachite Elite retrieve Failure.");
			}

			try
			{
				GameObject elementPanel = elitePanel.transform.GetChild(8).gameObject;
				//Debug.Log("elementPanel = " + elementPanel.ToString());
				if (elementPanel.GetComponent<UnityEngine.UI.Toggle>().isOn)
				{
					eliteString += "Void";
				}
			}
			catch (Exception e)
			{
				Debug.Log("(Artifacts of Doom) Void Elite retrieve Failure.");
			}

			//Debug.Log("Elite String: " + eliteString);

			return eliteString;
		}

		public override void Hooks()
		{
			/// We can Access the Run object on initialization here...
			Run.onRunStartGlobal += overrides;

			On.RoR2.UI.HUD.Awake += AddMonsterSelect;

			On.RoR2.Run.AdvanceStage += (On.RoR2.Run.orig_AdvanceStage orig, global::RoR2.Run self, global::RoR2.SceneDef nextScene) =>
			{
				orig.Invoke(self, nextScene);
				if (ArtifactEnabled)
				{
					usedOBJS.Clear();
					clientBodies.Clear();
					clientMasters.Clear();
				}
			};

			On.RoR2.CharacterBody.RecalculateStats += (On.RoR2.CharacterBody.orig_RecalculateStats orig, global::RoR2.CharacterBody self) =>
			{
				orig.Invoke(self);
				if (ArtifactEnabled )
				{
					bodyTriggered = true;
					updateStats(self, false);
					setInteractArea(self);
				}
			};

				// hook for synchronizing stats from local user settings
				On.RoR2.CharacterMaster.OnBodyStart += (orig_OnBodyStart orig, global::RoR2.CharacterMaster self, global::RoR2.CharacterBody body) =>
				{
				orig.Invoke(self, body);

				// for the case of an initial spawn of the player master
				if(ArtifactEnabled && body.isPlayerControlled && !clientMasters.Contains(self))
                {
					Debug.Log("Added Body to clientBodies and Masters :: " + body.name);
					clientMasters.Add(self);
					clientBodies.Add(body);
                }

				bool flag = ArtifactEnabled && body.isPlayerControlled && NetworkServer.active;
				if (flag)
				{
					// attempt to retrieve a stat pack. Only update if the stat pack exists...
					StatController.statPack pack = StatController.retrieveStatPackage(self, body);
					if (pack.valid)
					{
						float healthMult = (float)pack.healthMult;
						float dmgMult = (float)pack.dmgMult;
						float speedMult = (float)pack.speedMult;
						float attackSpeedMult = (float)pack.attackSpeedMult;
						float cooldownMult = (float)pack.cooldownMult;
						double size = (float)pack.size;
						String eliteTypes = pack.eliteTypes;


						Debug.Log("SERVER: Sending... Cooldown Mult = " + cooldownMult + " size = " + size + " :: " + body.name);
						new NetworkBehavior.statRequest(pack.playerID, speedMult, healthMult, attackSpeedMult, dmgMult, size, cooldownMult, eliteTypes, "SERVER").Send(NetworkDestination.Clients);
					}
					else
                    {
						Debug.Log("(ArtifactsOfDoom) Stat Pack Invalid... For " + body.GetUserName());
                    }
				}
				else
                {
					//Debug.Log("Body denied. Not player or isClient-> " + body.GetUserName());
                }
			};

			// modify skill cooldown on calculation
			On.RoR2.GenericSkill.CalculateFinalRechargeInterval += (On.RoR2.GenericSkill.orig_CalculateFinalRechargeInterval orig, global::RoR2.GenericSkill self) =>
			{
				float rechargeInterval = orig.Invoke(self);
				if (RoR2.Run.instance != null && RoR2.Run.instance.isActiveAndEnabled && ArtifactEnabled && self.characterBody.isPlayerControlled)
				{

					try
					{
						if (statUIOBJ != null)
						{
							// find all number parameters...
							GameObject statLayoutPanel = statUIOBJ.transform.GetChild(0).gameObject; // LayoutPanel
							GameObject statPanel = statLayoutPanel.transform.GetChild(1).gameObject; // StatPanel
							GameObject cooldownPanel = statPanel.transform.GetChild(6).gameObject;
							//Debug.Log("cooldownPanel = " + cooldownPanel.ToString());
							cooldownMultSync = float.Parse(cooldownPanel.transform.GetChild(1).gameObject.GetComponent<UnityEngine.UI.InputField>().text);
							//Debug.Log("value = " + cooldownMultSync);
						}
					}
					catch (Exception e)
                    {

                    }

					//Debug.Log("New Interval: " + (rechargeInterval * cooldownMultSync) + " for " + self.characterBody.name);
					return rechargeInterval *= cooldownMultSync;
				}
				else
				{
					return rechargeInterval;
				}
			};
			// get recharge interval normally

			On.RoR2.CharacterBody.OnDeathStart += (On.RoR2.CharacterBody.orig_OnDeathStart orig, global::RoR2.CharacterBody self) =>
			{
                if (ArtifactEnabled && self.isPlayerControlled && selectUIOBJ != null && selectUIOBJ.activeInHierarchy == true)
				{
					//Debug.Log("Player Controlled: " + self.isPlayerControlled);
					//Debug.Log("OBJ: " + selectUIOBJ);
					//Debug.Log("Active" + selectUIOBJ.activeInHierarchy);
					//Debug.Log("Death Start");
					// lock mouse
					//Debug.Log("Cursor4 " + RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount);
					RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount--;
					selectUIOBJ.SetActive(false);
					selectUIOBJ.active = false;
					orig.Invoke(self);
				}
				else
                {
					orig.Invoke(self);
				}
			};

			On.RoR2.Run.OnDisable += (On.RoR2.Run.orig_OnDisable orig, global::RoR2.Run self) =>
			{
				orig.Invoke(self);
				Debug.Log("RunDisable");
				// more rough fixes for this cursor stuff
				while (RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount > 1)
                {
					//Debug.Log("Cursor8");
					RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount--;
				}
			};
			

			On.RoR2.Run.BeginGameOver += (On.RoR2.Run.orig_BeginGameOver orig, global::RoR2.Run self, global::RoR2.GameEndingDef gameEndingDef) =>
			{
				orig.Invoke(self, gameEndingDef);
				if (ArtifactEnabled && self.isLocalPlayer && selectUIOBJ != null && selectUIOBJ.activeInHierarchy == true)
				{
					// lock mouse
					//Debug.Log("Cursor5 " + RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount);
					RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount--;
					selectOBJInstance.SetActive(false);
					selectOBJInstance.active = false;
				}
			};

			On.RoR2.UI.HUD.Update += (On.RoR2.UI.HUD.orig_Update orig, global::RoR2.UI.HUD self) =>
			{
				orig.Invoke(self);

				// we will gather options from the HUD and then send them to the server, which manages player stats
				// the stat controller will manage the list of players and retrieve information for us at the server
				if (RoR2.Run.instance != null && RoR2.Run.instance.isActiveAndEnabled == true && ArtifactEnabled && statUIOBJ != null)
				{
					float healthMult = 1;
					float dmgMult = 1;
					float speedMult = 1;
					float attackSpeedMult = 1;
					float cooldownMult = 1;
					double size = 1;
					String eliteTypes = eliteString();

					// find all number parameters...
					//Debug.Log("StatUIOBJ = " + statUIOBJ.ToString());
					GameObject statLayoutPanel = statUIOBJ.transform.GetChild(0).gameObject; // LayoutPanel
					//Debug.Log("LayoutPanel = " + statLayoutPanel.ToString());
					GameObject statPanel = statLayoutPanel.transform.GetChild(1).gameObject; // StatPanel
					//Debug.Log("StatPanel = " + statPanel.ToString());
					GameObject elitePanel = statLayoutPanel.transform.GetChild(2).gameObject; // ElitePanel
					//Debug.Log("elitePanel = " + elitePanel.ToString());

					// grab each value from the stat panel
					// damage panel
					try
					{
						GameObject damagePanel = statPanel.transform.GetChild(1).gameObject;
						//Debug.Log("damagePanel = " + damagePanel.ToString());
						dmgMult = float.Parse(damagePanel.transform.GetChild(1).gameObject.GetComponent<UnityEngine.UI.InputField>().text);
						//Debug.Log("value = " + dmgMult);
					}
					catch (Exception e)
					{
						//MessageHandler.globalMessage("(Artifact of Metamorphosis) Damage mult parsing failed! Using multiplier of 1 for: ");
					}

					// health panel
					try
					{
						GameObject healthPanel = statPanel.transform.GetChild(2).gameObject;
						//Debug.Log("healthPanel = " + healthPanel.ToString());
						healthMult = float.Parse(healthPanel.transform.GetChild(1).gameObject.GetComponent<UnityEngine.UI.InputField>().text);
						//Debug.Log("value = " + healthMult);
					}
					catch (Exception e)
					{
						//MessageHandler.globalMessage("(Artifact of Metamorphosis) Health mult parsing failed! Using multiplier of 1 for: ");
					}

					// movement speed panel
					try
					{
						GameObject movementSpeedPanel = statPanel.transform.GetChild(3).gameObject;
						//Debug.Log("movementSpeedPanel = " + movementSpeedPanel.ToString());
						speedMult = float.Parse(movementSpeedPanel.transform.GetChild(1).gameObject.GetComponent<UnityEngine.UI.InputField>().text);
						//Debug.Log("value = " + speedMult);
					}
					catch (Exception e)
					{
						//MessageHandler.globalMessage("(Artifact of Metamorphosis) Movement speed mult parsing failed! Using multiplier of 1 for: ");
					}

					// attack speed panel
					try
					{
						GameObject attackSpeedPanel = statPanel.transform.GetChild(4).gameObject;
						//Debug.Log("attackSpeedPanel = " + attackSpeedPanel.ToString());
						attackSpeedMult = float.Parse(attackSpeedPanel.transform.GetChild(1).gameObject.GetComponent<UnityEngine.UI.InputField>().text);
						//Debug.Log("value = " + attackSpeedMult);
					}
					catch (Exception e)
					{
						//MessageHandler.globalMessage("(Artifact of Metamorphosis) Attack speed mult parsing failed! Using multiplier of 1 for: ");
					}

					// size panel
					try
					{
						GameObject sizePanel = statPanel.transform.GetChild(5).gameObject;
						//Debug.Log("sizePanel = " + sizePanel.ToString());
						size = float.Parse(sizePanel.transform.GetChild(1).gameObject.GetComponent<UnityEngine.UI.InputField>().text);
						//Debug.Log("value = " + size);
					}
					catch (Exception e)
					{
						//MessageHandler.globalMessage("(Artifact of Metamorphosis) Size mult parsing failed! Using multiplier of 1 for: ");
					}

					// cooldown panel
					try
					{
						GameObject cooldownPanel = statPanel.transform.GetChild(6).gameObject;
						//Debug.Log("cooldownPanel = " + cooldownPanel.ToString());
						cooldownMult = float.Parse(cooldownPanel.transform.GetChild(1).gameObject.GetComponent<UnityEngine.UI.InputField>().text);
						//Debug.Log("value = " + cooldownMult);
					}
					catch (Exception e)
					{
						//MessageHandler.globalMessage("(Artifact of Metamorphosis) Cooldown mult parsing failed! Using multiplier of 1 for: ");
					}

					// send stat info with first local user as the netId we want to use
					//Debug.Log("netid = " + LocalUserManager.GetFirstLocalUser().currentNetworkUser.netId.Value);

					if (LocalUserManager.GetFirstLocalUser().currentNetworkUser.GetCurrentBody() != null)
					{
						if (!NetworkServer.active)
						{
							//Debug.Log("(statRequest) Sending stat request rebound to server (isClient)");
							new NetworkBehavior.statRequestRebound(LocalUserManager.GetFirstLocalUser().currentNetworkUser.master.networkIdentity.netId.Value, speedMult, healthMult, attackSpeedMult, dmgMult, size, cooldownMult, eliteTypes, LocalUserManager.GetFirstLocalUser().userProfile.name).Send(NetworkDestination.Server);
						}
						else
						{
							//Debug.Log("(statRequest) injecting package into self (isServer)");
							StatController.injectStatPackage(LocalUserManager.GetFirstLocalUser().currentNetworkUser.master.networkIdentity.netId.Value, speedMult, healthMult, attackSpeedMult, dmgMult, size, cooldownMult, eliteTypes);
						}
					}
				}
			};

			On.RoR2.Run.Update += (On.RoR2.Run.orig_Update orig, global::RoR2.Run self) =>
			{
				orig.Invoke(self);

				// dirty fix for cursor bug
				if(ArtifactEnabled && RoR2.Run.instance.isGameOverServer)
                {
					while (RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount >= 2 && RoR2.PauseManager.isPaused == false)
					{
						//Debug.Log("Cursor1 " + RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount);
						RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount--;
					}
				}

				if (ArtifactEnabled && !RoR2.Run.instance.isGameOverServer)
				{
					bool flag = Input.GetKeyDown(OptionsLink.AOD_KeyUI.Value.MainKey) && !PauseManager.isPaused;
					if (flag)
					{
						bool act = false;
						if (selectUIOBJ != null && selectUIOBJ.activeInHierarchy)
						{
							act = false;
						}
						if (selectUIOBJ != null && !selectUIOBJ.activeInHierarchy)
						{
							act = true;
						}

						if(!act)
                        {
							//Debug.Log("SETACTIVEFALSE");
							selectOBJInstance.SetActive(false);
							selectOBJInstance.active = false;
							// lock mouse
							//Debug.Log("Cursor2 " + RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount);
							RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount--;
						}
						else
                        {
							//Debug.Log("SETACTIVETRUE");
							selectOBJInstance.SetActive(true);
							selectOBJInstance.active = true;
							// unlock mouse
							//Debug.Log("Cursor3 " + RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount);
							RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount++;
						}
					}
				}
			};
		}


		private void overrides(Run run)
		{
			if (NetworkServer.active && ArtifactEnabled)
			{
				MessageHandler.globalMessage("Want a monster energy drink? I swear I wasn't paid for this.");
				StatController.statList.Clear();
				recentTransforms.Clear();
				clientMasters.Clear();
				clientBodies.Clear();
				sizeMultSync = 1;
				healthMultSync = 1;
				damageMultSync = 1;
				speedMultSync = 1;
				attackSpeedMultSync = 1;
				cooldownMultSync = 1;
				eliteStringSync = "";
			}

		}
	}

}

