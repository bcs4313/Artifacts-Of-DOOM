using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using ArtifactsOfDoom;
using static On.RoR2.CharacterMaster;
using System;
using R2API.Networking.Interfaces;
using R2API.Networking;
using System.Collections.Generic;

namespace ArtifactGroup
{
	class ArtifactOfTitans : ArtifactBase
	{
		public static bool enabled = false;
		Inventory globalInventory = new Inventory();

		int mostRecentCreditCost = 1; // used for scaling
		public List<monsterIdentity> creditIDS = new List<monsterIdentity>();
		
		// client synchronization data structure
		public static List<desyncedMonster> desynchronizedMonsters = new List<desyncedMonster>();

		public static ConfigEntry<int> TimesToPrintMessageOnStart;
		public override string ArtifactName => "Artifact of The Titans";
		public override string ArtifactLangTokenName => "ARTIFACT_OF_TITANS";
		public override string ArtifactDescription => "You and everything supporting you is smaller. \n " +
			"ALL Enemy Sizes Scale according to credit cost. Boss size additionaly scales with stage (max 10). \n" +
			"Note: Enemies spawned upon entering the stage do not scale!";
		public override Sprite ArtifactEnabledIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/ArtifactOfTheTitans.png");
		public override Sprite ArtifactDisabledIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/ArtifactOfTheTitansDisabled.png");

		public override void Init()
		{
			//CreateConfig(config);
			CreateLang();
			CreateArtifact();
			Hooks();
		}
		public override void Hooks()
		{
			RoR2.SpawnCard.onSpawnedServerGlobal += (SpawnCard.SpawnResult spawn) =>
			{
				if (ArtifactEnabled && NetworkServer.active)
				{
					mostRecentCreditCost = spawn.spawnRequest.spawnCard.directorCreditCost;
				}
			};

			On.RoR2.CharacterMaster.OnBodyStart += (orig_OnBodyStart orig, global::RoR2.CharacterMaster self, global::RoR2.CharacterBody body) =>
			{
				int cost = mostRecentCreditCost;

				// client handler for lost packets and desynchronized body spawns
				if(ArtifactEnabled && !NetworkServer.active)
                {
					for(int i = 0; i < desynchronizedMonsters.Count; i++)
                    {
						if(body.netId.Value == desynchronizedMonsters[i].uid)
						{
							Debug.Log("(Artifact of The Titans): Resolved resize failure of UID: " + body.netId.Value);
							networkBehavior.resizeMonster(body, desynchronizedMonsters[i].scalar);
							desynchronizedMonsters.RemoveAt(i);
                        }
						else
                        {
							var m = desynchronizedMonsters[i];
							m.overload += 1;
							if(m.overload == 50)
                            {
								Debug.Log("(Artifact of The Titans): Resize failure remains unresolved of UID: " + body.netId.Value);
								desynchronizedMonsters.RemoveAt(i);
							}
						}
                    }
                }

				if(cost > 0 && ArtifactEnabled && NetworkServer.active)
                {
					bool inLog = false;
					// first we will check if this monster is already logged
					for(int i = 0; i < creditIDS.Count; i++)
                    {
						if(creditIDS[i].baseNameToken == body.baseNameToken && creditIDS[i].isElite == body.isElite)
                        {
							inLog = true;
                        }
                    }

					// we can store it in a log if the monster isn't inside already
					if (inLog == false)
					{
						var mlog = new monsterIdentity();
						mlog.creditCost = cost;
						mlog.baseNameToken = body.baseNameToken;
						mlog.isElite = body.isElite;
						Debug.Log("Stored Base Monster for usage: name: " + mlog.baseNameToken + " isElite: " + mlog.isElite + " cost: " + mlog.creditCost);
						creditIDS.Add(mlog);
					}

				}
				else if (ArtifactEnabled && NetworkServer.active)
                {
					bool foundReference = false;
					// now we encounter a special situation where the monster needs to be assigned a cost
					cost = 3; // assigned an initial base cost
					// first we will check if this monster is already logged
					for (int i = 0; i < creditIDS.Count; i++)
					{
						if (creditIDS[i].baseNameToken == body.baseNameToken && creditIDS[i].isElite == body.isElite)
						{
							Debug.Log("Monster spawned without cost, but was found in the log!: name: " 
								+ creditIDS[i].baseNameToken + " isElite: " + creditIDS[i].isElite + " cost: " + creditIDS[i].creditCost);
							cost = creditIDS[i].creditCost;
							foundReference = true;
							break;
						}
					}

					if(foundReference == false)
                    {
						Debug.Log("Monster spawned without cost, and without a log. Scaling to a cost of 3... name: " + body.baseNameToken);
                    }
				}

				double scalar = 1;
				if (ArtifactEnabled && NetworkServer.active)
				{
					if (body.teamComponent.teamIndex == TeamIndex.Monster)
					{
						if (body.isBoss)
						{
							//Vector3 size = new Vector3();
							scalar = 1;
							scalar *= (double)Math.Pow((double)cost * (double)(0.00035 * Mathf.Min(10, RoR2.Run.instance.stageClearCount + 1)) + 1, OptionsLink.AOT_BossGrowthPerStage.Value); // violent growth...																   //size.x = body.gameObject.transform.localScale.x * scalar;
																																		   //size.y = body.gameObject.transform.localScale.y * scalar;
							if (body.isElite)
							{
								scalar += (float)OptionsLink.AOT_EliteMultiplier.Value;
							}
							scalar += OptionsLink.AOT_FixedBossScaling.Value;

							//body.gameObject.transform.localScale = size;
						}
						else
						{
							//Debug.Log("Scaling...");
							Vector3 size = new Vector3();
							scalar = 1;
							scalar *= Mathf.Log(cost * (float)OptionsLink.AOT_CreditScalingMultiplier.Value + 1, (float)1.75);
							size = body.masterObject.transform.localScale;
							if (body.isElite)
							{
								scalar += OptionsLink.AOT_EliteMultiplier.Value;
							}
							scalar += OptionsLink.AOT_FixedMonsterScaling.Value;
							//body.modelLocator.transform.localScale = 
						}
					}

					// generate a raw client package
					uint idTarget = body.networkIdentity.netId.Value;

					// resize entities for all clients
					new networkBehavior.resizeEntity(scalar, idTarget).Send(NetworkDestination.Clients);
				}
				orig.Invoke(self, body);
			};

            //RoR2.EliteDef
        }

		// using this as a cross reference when monsters spawn with a 0 credit cost
        public struct monsterIdentity
        {
			public int creditCost;
			public String baseNameToken;
			public bool isElite;
		}

		public struct desyncedMonster
        {
			public uint uid;
			public double scalar;
			public int overload; // once an overload value hits 50 (50 spawn triggers), the monster is deleted. This is done to save space in our list.
        }
    }

}
