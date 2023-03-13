using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ArtifactGroup;
using ArtifactsOfDoom;
using BepInEx.Configuration;
using csProj.Artifacts.Left4DeadSystem;
using EntityStates.Missions.BrotherEncounter;
using Messenger;
using On.EntityStates.Missions.BrotherEncounter;
using On.RoR2;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using BossGroup = RoR2.BossGroup;
using CharacterBody = RoR2.CharacterBody;
using Debug = UnityEngine.Debug;
using NetworkUser = RoR2.NetworkUser;
using Random = System.Random;
using Run = RoR2.Run;
using TeleporterInteraction = RoR2.TeleporterInteraction;
using orig_Start = On.RoR2.Run.orig_Start;
using PreEncounter = EntityStates.Missions.BrotherEncounter.PreEncounter;

namespace ArtifactGroup
{
	internal class ArtifactOfDefiance : ArtifactBase
	{
		private TransformSpace ts;

		public static DefianceTeamManager tm;

		private static Random rnd;

		public static ConfigEntry<int> TimesToPrintMessageOnStart;

		private GameObject mostRecentPrefab;

		public static bool clientLight = true;

		public static bool clientOneShot = true;

		public static float clientExtraLevels;

		public static float clientLevelMultiplier;

		public static bool loadingScene = false;

		public static int s = 0;

		public List<string> blackList;

		private float mostRecentLevel = 1f;

		public override string ArtifactName
		{
			get
			{
				return "Artifact of Defiance";
			}
		}

		public override string ArtifactLangTokenName
		{
			get
			{
				return "ARTIFACT_OF_DEFIANCE";
			}
		}

		public override string ArtifactDescription
		{
			get
			{
				return "Opt into playing on the monster team or on the Survivor team.Monster players are assigned monsters when they are naturally spawned. \n The goal: KILL ALL SURVIVORS! \n(The host should adjust the RiskOfOptions settings for this artifact to their liking).";
			}
		}

		public override Sprite ArtifactEnabledIcon
		{
			get
			{
				return Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/ArtifactOfUnity.png");
			}
		}

		public override Sprite ArtifactDisabledIcon
		{
			get
			{
				return Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/ArtifactOfUnityDisabled.png");
			}
		}

		public override void Init()
		{
			base.CreateLang();
			base.CreateArtifact();
			this.Hooks();
			this.ts = new TransformSpace(this);
			this.ts.init();
			ArtifactOfDefiance.rnd = new Random();
		}

		public override void Hooks()
		{
			Run.onRunStartGlobal += overrides;
			On.RoR2.Run.Start += (orig_Start orig, global::RoR2.Run self) =>
			{
				bool artifactEnabled = base.ArtifactEnabled;
				if (artifactEnabled)
				{
					this.mostRecentLevel = 1f;
					ArtifactOfDefiance.loadingScene = false;
					ArtifactOfDefiance.tm = new DefianceTeamManager();
					TransformSpace.clientIsHuman = false;
					TransformSpace.clientIsMonster = false;
					this.readBlacklist();
					bool active = NetworkServer.active;
					if (active)
					{
						new DefianceNetBehavior.clientSyncSettings(OptionsLink.AOD_VisiblePlayers.Value, OptionsLink.AOD_OneShotProtection.Value, OptionsLink.AOD_ExtraLevels.Value, OptionsLink.AOD_LevelMultiplier.Value);
					}
					bool artifactEnabled2 = base.ArtifactEnabled;
					if (artifactEnabled2)
					{
						bool flag = !this.ts.isHooked;
						if (flag)
						{
							this.ts.addHooks();
						}
					}
					else
					{
						bool isHooked = this.ts.isHooked;
						if (isHooked)
						{
							this.ts.removeHooks();
						}
					}
				}
				orig.Invoke(self);
			};
            On.EntityStates.Missions.BrotherEncounter.PreEncounter.OnEnter += delegate (On.EntityStates.Missions.BrotherEncounter.PreEncounter.orig_OnEnter orig, PreEncounter self)
			{
				List<DefianceTeamManager.player> monsterPlayers = DefianceTeamManager.monsterPlayers;
				for (int i = 0; i < DefianceTeamManager.monsterPlayers.Count; i++)
				{
					DefianceTeamManager.player player = monsterPlayers[i];
					CharacterBody userBody = DefianceTeamManager.getUserBody(player.UID);
					bool flag = userBody != null;
					if (flag)
					{
						Debug.Log("Artifact of Defiance: killing monster players to force mithrix to spawn!");
						userBody.master.TrueKill();
					}
				}
				orig.Invoke(self);
			};
			On.RoR2.GlobalEventManager.OnPlayerCharacterDeath += delegate (On.RoR2.GlobalEventManager.orig_OnPlayerCharacterDeath orig, RoR2.GlobalEventManager self, RoR2.DamageReport damageReport, NetworkUser victimNetworkUser)
			{
				bool flag = base.ArtifactEnabled && !ArtifactOfDefiance.loadingScene;
				if (flag)
				{
					bool flag2 = DefianceTeamManager.containsPMonster(victimNetworkUser.netId.Value);
					if (flag2)
					{
						bool value = OptionsLink.AOD_DeathMessages.Value;
						if (value)
						{
							orig.Invoke(self, damageReport, victimNetworkUser);
						}
					}
				}
				else
				{
					orig.Invoke(self, damageReport, victimNetworkUser);
				}
			};
			//On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, RoR2.GlobalEventManager self, RoR2.DamageReport damageReport;
            On.RoR2.CharacterMaster.OnBodyStart += new On.RoR2.CharacterMaster.hook_OnBodyStart(this.monsterStep);
            On.RoR2.GlobalEventManager.OnCharacterDeath += this.lunarCoinCheck;
			On.RoR2.Run.AdvanceStage += this.prepareClients;
			On.RoR2.CharacterBody.RecalculateStats += delegate (On.RoR2.CharacterBody.orig_RecalculateStats orig, global::RoR2.CharacterBody self)
			{
				bool flag = base.ArtifactEnabled && !ArtifactOfDefiance.loadingScene;
				if (flag)
				{
					bool flag2 = self.isPlayerControlled && self.teamComponent.teamIndex == TeamIndex.Monster;
					if (flag2)
					{
                        On.RoR2.TeamManager.GetTeamLevel += new On.RoR2.TeamManager.hook_GetTeamLevel(this.rigLevel);
						orig.Invoke(self);
                        On.RoR2.TeamManager.GetTeamLevel -= new On.RoR2.TeamManager.hook_GetTeamLevel(this.rigLevel);
						bool flag3 = !ArtifactOfDefiance.clientOneShot;
						if (flag3)
						{
							self.oneShotProtectionFraction = 0f;
						}
					}
					else
					{
						orig.Invoke(self);
					}
				}
				else
				{
					orig.Invoke(self);
				}
			};
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00005172 File Offset: 0x00003372
		public void prepareClients(On.RoR2.Run.orig_AdvanceStage orig, Run self, RoR2.SceneDef nextScene)
		{
			ArtifactOfDefiance.loadingScene = true;
			ArtifactOfDefiance.tm.resetPlayers();
			Debug.Log("Artifact of Defiance: Temporarily Disabled Artifact to protect netID channel");
			orig.Invoke(self, nextScene);
		}

		// Token: 0x06000064 RID: 100 RVA: 0x0000519C File Offset: 0x0000339C
		public void applyFixture(CharacterBody body, bool isBoss)
		{
			try
			{
				Debug.Log("Artifact of Defiance: Adding light component...");
				Light light = body.gameObject.AddComponent<Light>();
				bool flag = !isBoss;
				if (flag)
				{
					light.color = Color.red;
					light.intensity *= 10f;
					light.range *= 5f;
				}
				else
				{
					light.color = Color.red;
					light.intensity *= 10f;
					light.range *= 20f;
				}
				Debug.Log("Artifact of Defiance: Added light visual component to " + body.name);
			}
			catch
			{
				Debug.Log("Artifact of Defiance: Light Fixture Attempt Failed!");
			}
		}

		public uint rigLevel(On.RoR2.TeamManager.orig_GetTeamLevel orig, RoR2.TeamManager self, TeamIndex teamIndex)
		{
			return Convert.ToUInt32(Math.Abs(Math.Ceiling((double)(this.mostRecentLevel * ArtifactOfDefiance.clientLevelMultiplier + ArtifactOfDefiance.clientExtraLevels))));
		}

		public void monsterStep(On.RoR2.CharacterMaster.orig_OnBodyStart orig, RoR2.CharacterMaster self, CharacterBody body)
		{
			/*
			ArtifactOfDefiance.< monsterStep > d__28 < monsterStep > d__ = new ArtifactOfDefiance.< monsterStep > d__28();
			< monsterStep > d__.<> t__builder = AsyncVoidMethodBuilder.Create();
			< monsterStep > d__.<> 4__this = this;
			< monsterStep > d__.orig = orig;
			< monsterStep > d__.self = self;
			< monsterStep > d__.body = body;
			< monsterStep > d__.<> 1__state = -1;
			< monsterStep > d__.<> t__builder.Start < ArtifactOfDefiance.< monsterStep > d__28 > (ref < monsterStep > d__);
			*/
		}

		public void linkMonsterDataToClient(NetworkUser target, RoR2.CharacterMaster monsterMaster)
		{
			bool active = NetworkServer.active;
			if (active)
			{
				new DefianceNetBehavior.updateTargetMaster(target.netId.Value, monsterMaster.netId.Value);
			}
		}

		public void bossTick(NetworkUser user, CharacterBody body)
		{
			body.netIdentity.localPlayerAuthority = true;
			user.SetBodyPreference(body.bodyIndex);
			user.master.teamIndex = TeamIndex.Monster;
			CharacterBody characterBody = user.master.Respawn(body.corePosition, body.transform.rotation);
			DefianceTeamManager.controlledBodies.Add(body.netId.Value);
			DefianceTeamManager.controlledBodies.Add(characterBody.netId.Value);
			this.linkMonsterDataToClient(user, body.master);
			BossGroup bossGroup = TeleporterInteraction.instance.bossGroup;
			bossGroup.OnMemberDiscovered(user.master);
			Run.instance.OnServerBossAdded(bossGroup, user.master);
			bossGroup.combatSquad.AddMember(user.master);
			body.master.DestroyBody();
			bossGroup.OnMemberLost(body.master);
			bossGroup.combatSquad.RemoveMember(body.master);
		}

		[DebuggerStepThrough]
		public void lunarCoinCheck(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, global::RoR2.GlobalEventManager self, global::RoR2.DamageReport r)
		{

			/* assembly decompilation (for recovery)
			ArtifactOfDefiance.< lunarCoinCheck > d__31 < lunarCoinCheck > d__ = new ArtifactOfDefiance.< lunarCoinCheck > d__31();
			< lunarCoinCheck > d__.<> t__builder = AsyncVoidMethodBuilder.Create();
			< lunarCoinCheck > d__.<> 4__this = this;
			< lunarCoinCheck > d__.r = r;
			< lunarCoinCheck > d__.<> 1__state = -1;
			< lunarCoinCheck > d__.<> t__builder.Start < ArtifactOfDefiance.< lunarCoinCheck > d__31 > (ref < lunarCoinCheck > d__);
			*/
		}

		public bool inBlacklist(CharacterBody body)
		{
			for (int i = 0; i < this.blackList.Count; i++)
			{
				bool flag = body.GetDisplayName().Replace(" ", "").ToLower().Equals(this.blackList[i].Replace(" ", "").ToLower());
				if (flag)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00005510 File Offset: 0x00003710
		public void readBlacklist()
		{
			try
			{
				this.blackList = new List<string>();
				string value = OptionsLink.AOD_Blacklist.Value;
				string[] array = value.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					bool flag = array[i].Trim().Length != 0;
					if (flag)
					{
						Debug.Log("Artifact of Defiance: Banned " + array[i] + " from the possible monster spawns of the player-monster team!");
						this.blackList.Add(array[i]);
					}
				}
			}
			catch
			{
				MessageHandler.globalMessage("Issue interpreting the defiance enemy team blacklist! Check your formatting in the RoR2 settings!");
			}
		}

		public ArtifactOfDefiance()
		{
		}

		static ArtifactOfDefiance()
		{
		}

		[CompilerGenerated]
		private void overrides(RoR2.Run run)
		{
			bool flag = NetworkServer.active && base.ArtifactEnabled;
			if (flag)
			{
				MessageHandler.globalMessage("Sus?");
			}
		}
	}
}
