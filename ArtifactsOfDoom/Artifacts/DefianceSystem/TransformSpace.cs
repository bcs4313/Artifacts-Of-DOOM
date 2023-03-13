using System;
using System.IO;
using System.Reflection;
using ArtifactsOfDoom;
using csProj.Artifacts.Left4DeadSystem;
using Messenger;
using On.RoR2;
using On.RoR2.UI;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using LocalUserManager = RoR2.LocalUserManager;
using NetworkUser = RoR2.NetworkUser;
using Object = UnityEngine.Object;
using Path = System.IO.Path;

namespace ArtifactGroup
{
	internal class TransformSpace
	{
		public TransformSpace(ArtifactOfDefiance host)
		{
			TransformSpace.parent = host;
			this.addHooks();
		}

		public void init()
		{
			Debug.Log("Artifacts Of Doom (L4D): Loading asset bundle from path: " + Assembly.GetExecutingAssembly().Location + "/doomuielements");
			TransformSpace.UIAssets = AssetBundle.LoadFromFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/doomuielements");
			Debug.Log("Artifacts Of Doom (L4D): Loading prefab from Assets/UI/DefianceCanvas.prefab...");
			TransformSpace.UICanvas = TransformSpace.UIAssets.LoadAsset<GameObject>("Assets/UI/DefianceCanvas.prefab");
			Debug.Log("Artifacts Of Doom (L4D): Prefab name = " + TransformSpace.UICanvas.name);
		}

		private void InitializeHud(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
		{
			orig.Invoke(self);
			bool artifactEnabled = TransformSpace.parent.ArtifactEnabled;
			if (artifactEnabled)
			{
				bool flag = !NetworkServer.active;
				if (flag)
				{
					ArtifactOfDefiance.loadingScene = false;
					new DefianceNetBehavior.notifyServerOfClientLoad(this.getLocalUID());
				}
				else
				{
					ArtifactOfDefiance.tm.playerLoaded(this.getLocalUID());
					bool flag2 = ArtifactOfDefiance.tm.allMonstersReady();
					if (flag2)
					{
						Debug.Log("Artifact of Defiacnce: All clients are loaded into the server. Reactivating artifact!");
						ArtifactOfDefiance.loadingScene = false;
					}
				}
			}
			bool flag3 = TransformSpace.parent.ArtifactEnabled && !ArtifactOfDefiance.loadingScene;
			if (flag3)
			{
				TransformSpace.hud = self;
				TransformSpace.keyReact = true;
				TransformSpace.UICanvasInstance = Object.Instantiate<GameObject>(TransformSpace.UICanvas);
				TransformSpace.UICanvasInstance.transform.SetParent(self.mainUIPanel.transform);
				TMP_Text[] componentsInChildren = TransformSpace.UICanvasInstance.GetComponentsInChildren<TMP_Text>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					bool flag4 = componentsInChildren[i].text.Contains("Monster");
					if (flag4)
					{
						componentsInChildren[i].text = "Monsters: Press " + OptionsLink.AOD_KeyChooseMonster.Value.MainKey.ToString();
					}
					bool flag5 = componentsInChildren[i].text.Contains("Survivor");
					if (flag5)
					{
						componentsInChildren[i].text = "Survivors: Press " + OptionsLink.AOD_KeyChooseHuman.Value.MainKey.ToString();
					}
				}
				RectTransform component = TransformSpace.UICanvasInstance.GetComponent<RectTransform>();
				component.anchorMin = Vector2.zero;
				component.anchorMax = Vector2.one;
				component.sizeDelta = Vector2.zero;
				component.anchoredPosition = Vector2.zero;
				Debug.Log("Artifacts Of Doom (L4D): Loaded Hud");
                RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount++;
			}
		}

		private void listenForTeam(On.RoR2.Run.orig_Update orig, RoR2.Run self)
		{
			orig.Invoke(self);
			bool flag = TransformSpace.parent.ArtifactEnabled && !ArtifactOfDefiance.loadingScene;
			if (flag)
			{
				bool flag2 = Input.GetKeyDown(OptionsLink.AOD_KeyChooseHuman.Value.MainKey) && TransformSpace.keyReact;
				if (flag2)
				{
					TransformSpace.clientIsHuman = true;
					TransformSpace.clientIsMonster = false;
					TransformSpace.UICanvasInstance.SetActive(false);
					bool active = NetworkServer.active;
					if (active)
					{
						ArtifactOfDefiance.tm.joinHumans(this.getLocalUID());
					}
					else
					{
						new DefianceNetBehavior.joinDefianceTeam(this.getLocalUID(), "Human");
					}
					bool flag3 = TransformSpace.keyReact;
					if (flag3)
					{
                        RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount--;
						TransformSpace.keyReact = false;
					}
				}
				bool flag4 = Input.GetKeyDown(OptionsLink.AOD_KeyChooseMonster.Value.MainKey) && TransformSpace.keyReact;
				if (flag4)
				{
					TransformSpace.clientIsMonster = true;
					TransformSpace.clientIsHuman = false;
					TransformSpace.UICanvasInstance.SetActive(false);
					bool active2 = NetworkServer.active;
					if (active2)
					{
						ArtifactOfDefiance.tm.joinMonsters(this.getLocalUID());
					}
					else
					{
						DefianceTeamManager.getUserBody(this.getLocalUID()).healthComponent.health = -69f;
						new DefianceNetBehavior.joinDefianceTeam(this.getLocalUID(), "Monster");
					}
					bool flag5 = TransformSpace.keyReact;
					if (flag5)
					{
                        RoR2.MPEventSystemManager.primaryEventSystem.cursorOpenerCount--;
						TransformSpace.keyReact = false;
					}
				}
				bool flag6 = Input.GetKeyDown(OptionsLink.AOD_KeySuicide.Value.MainKey) && !TransformSpace.keyReact;
				if (flag6)
				{
					TransformSpace.UICanvasInstance.SetActive(false);
					bool flag7 = TransformSpace.clientIsMonster;
					if (flag7)
					{
                        RoR2.CharacterBody userBody = DefianceTeamManager.getUserBody(this.getLocalUID());
						bool flag8 = userBody != null;
						if (flag8)
						{
							userBody.master.TrueKill();
						}
					}
				}
			}
		}
		public void addHooks()
		{
            On.RoR2.UI.HUD.Awake += new On.RoR2.UI.HUD.hook_Awake(this.InitializeHud);
            On.RoR2.Run.Update += new On.RoR2.Run.hook_Update(this.listenForTeam);
			this.isHooked = true;
		}

		public void removeHooks()
		{
            On.RoR2.UI.HUD.Awake -= new On.RoR2.UI.HUD.hook_Awake(this.InitializeHud);
            On.RoR2.Run.Update -= new On.RoR2.Run.hook_Update(this.listenForTeam);
			this.isHooked = false;
		}
		private uint getLocalUID()
		{
			try
			{
				bool flag = NetworkUser.readOnlyInstancesList != null;
				if (flag)
				{
					for (int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++)
					{
						NetworkUser networkUser = NetworkUser.readOnlyInstancesList[i];
						bool flag2 = LocalUserManager.GetFirstLocalUser().currentNetworkUser.userName.Equals(networkUser.userName);
						if (flag2)
						{
							return NetworkUser.readOnlyInstancesList[i].netId.Value;
						}
					}
					MessageHandler.globalMessage("issue obtaining user with matching UID... (getLocalUID)");
				}
			}
			catch
			{
				Debug.Log("(Artifact of Defiance) UID retrieval failed, hook aborted. (This is very likely a bad thing for this artifact).");
			}
			return 0U;
		}

		public static bool firstRun = true;

		public static bool keyReact = true;

		public static RoR2.UI.HUD hud = null;

		public static bool clientIsMonster = false;

		public static bool clientIsHuman = false;

		public bool isHooked = false;

		public static GameObject RectHud;

		public static GameObject UICanvas;

		public static GameObject UICanvasInstance;

		public static AssetBundle UIAssets;

		public static ArtifactOfDefiance parent;
	}
}
