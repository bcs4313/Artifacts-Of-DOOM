using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using ArtifactsOfDoom;
using static On.RoR2.CharacterMaster;
using System;
using R2API.Networking.Interfaces;
using R2API.Networking;
namespace ArtifactGroup
{
	class ArtifactOfTitans : ArtifactBase
	{
		public static bool enabled = false;
		Inventory globalInventory = new Inventory();

		int mostRecentCreditCost = 0; // used for scaling

		public static ConfigEntry<int> TimesToPrintMessageOnStart;
		public override string ArtifactName => "Artifact of The Titans";
		public override string ArtifactLangTokenName => "ARTIFACT_OF_TITANS";
		public override string ArtifactDescription => "You and everything supporting you is smaller. \n " +
			"ALL Enemy Sizes Scale according to credit cost. Boss size additionaly scales with stage (max 10). \n" +
			"Note: Enemies spawned upon entering the stage do not scale!";
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
				double scalar = 1;
				if (ArtifactEnabled && NetworkServer.active)
				{
					if (body.teamComponent.teamIndex == TeamIndex.Monster)
					{
						if (body.isBoss)
						{
							//Vector3 size = new Vector3();
							scalar = 1;
							scalar *= (double)Math.Pow((double)cost * (double)(0.00035 * Mathf.Min(10, RoR2.Run.instance.stageClearCount + 1)) + 1, 2.6); // violent growth...																   //size.x = body.gameObject.transform.localScale.x * scalar;
																																					   //size.y = body.gameObject.transform.localScale.y * scalar;
							if (body.isElite)
							{
								scalar += (float)0.3;
							}
							body.masterObject.transform.localScale *= (float)scalar;
							//body.gameObject.transform.localScale = size;
						}
						else
						{
							Debug.Log("Scaling...");
							Vector3 size = new Vector3();
							scalar = 1;
							scalar *= Mathf.Log(cost * (float)0.15 + 1, (float)1.75);
							size = body.masterObject.transform.localScale;
							if (body.isElite)
							{
								scalar += 0.4;
							}
						}
					}

				}
				if (ArtifactEnabled && NetworkServer.active && body.teamComponent.teamIndex == TeamIndex.Monster)
				{
					// generate a raw client package
					uint idTarget = body.networkIdentity.netId.Value;

					// resize entities for all clients
					new networkBehavior.resizeEntity(scalar, idTarget).Send(NetworkDestination.Clients);
				}
				orig.Invoke(self, body);
			};

			//RoR2.EliteDef
		}

		void printBuffs()
		{
			foreach (BuffDef b in BuffCatalog.buffDefs)
			{

				//Debug.Log("BUFF NAME: " + b.name + " BUFF INDEX:" + b.buffIndex);
			}

			foreach (EliteDef b in EliteCatalog.eliteDefs)
			{

				//Debug.Log("ELITE NAME: " + b.name + " ELITE INDEX:" + b.eliteIndex);
			}
		}
	}

}
