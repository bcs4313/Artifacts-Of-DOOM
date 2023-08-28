using ArtifactGroup;
using RoR2;
using RoR2.Artifacts;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace csProj.Artifacts.EvilSystem
{
	class PackSpawner
	{
		public static String nextPackName = "ur mom";

		public static void randomPack()
		{
			Xoroshiro128Plus xoro = new Xoroshiro128Plus((uint)ArtifactOfEvil.r.Next(-int.MaxValue, int.MaxValue));


			String groupSizeName = "";
			int totalSpawns = 0;
			double sizeRNG = ArtifactOfEvil.r.NextDouble();
			if (sizeRNG > 0.70)
			{
				groupSizeName = " Soldier";
				totalSpawns = 1;

			}
			else if (sizeRNG > 0.50)
			{
				groupSizeName = " Troop";
				totalSpawns = 2;

			}
			else if (sizeRNG > 0.25)
			{
				groupSizeName = " Group";
				totalSpawns = 4;

			}
			else if (sizeRNG > 0.15)
			{
				groupSizeName = " Crowd";
				totalSpawns = 6;

			}
			else if (sizeRNG > 0.05)
			{
				groupSizeName = " Swarm";
				totalSpawns = 10;

			}
			else
			{
				groupSizeName = " Horde";
				totalSpawns = 20;
			}

			// picks the master type spawned
			switch (ArtifactOfEvil.r.Next(0, 12))
			{
				case (0): // acrid pack
					spawnPack2(xoro, "CrocoMonsterMaster", totalSpawns, "Acrid" + groupSizeName);
					break;
				case (1): // huntress pack
					spawnPack2(xoro, "HuntressMonsterMaster", totalSpawns, "Huntress" + groupSizeName);
					break;
				case (2): // commando pack
					spawnPack2(xoro, "CommandoMonsterMaster", totalSpawns, "Commando" + groupSizeName);
					break;
				case (3): // Mage pack
					spawnPack2(xoro, "MageMonsterMaster", totalSpawns, "Artificer" + groupSizeName);
					break;
				case (4): // Toolbot pack
					spawnPack2(xoro, "ToolbotMonsterMaster", totalSpawns, "MUL-T" + groupSizeName);
					break;
				case (5): // Railgunner
					spawnPack2(xoro, "RailgunnerMonsterMaster", totalSpawns, "Railgunner" + groupSizeName);
					break;
				case (6): // Bandit
					spawnPack2(xoro, "Bandit2MonsterMaster", totalSpawns, "Bandit" + groupSizeName);
					break;
				case (7): // Engi
					spawnPack2(xoro, "EngiMonsterMaster", totalSpawns, "Engineer" + groupSizeName);
					break;
				case (8): // TreeBot
					spawnPack2(xoro, "TreebotMonsterMaster", totalSpawns, "REX" + groupSizeName);
					break;
				case (9): // Captain
					spawnPack2(xoro, "CaptainMonsterMaster", totalSpawns, "Captain" + groupSizeName);
					break;
				case (10): // crab
					spawnPack2(xoro, "VoidSurvivorMonsterMaster", totalSpawns, "Voidling" + groupSizeName);
					break;
				case (11): // Merc
					spawnPack2(xoro, "MercSurvivorMonsterMaster", totalSpawns, "Mercenary" + groupSizeName);
					break;
			}
		}

		public static void spawnPack2(Xoroshiro128Plus rng, String masterTargetString, int quantity, String packName)
		{
			for (int i = CharacterMaster.readOnlyInstancesList.Count - 1; i >= 0; i--)
			{
				CharacterMaster characterMaster = CharacterMaster.readOnlyInstancesList[i];
				if (characterMaster.teamIndex == TeamIndex.Player && characterMaster.playerCharacterMasterController)
				{
					for (int x = 0; x < quantity; x++)
					{
						CreateDoppelgangerStub(characterMaster, rng, masterTargetString, packName);
					}
				}
			}
		}


		public static void CreateDoppelgangerStub(CharacterMaster srcCharacterMaster, Xoroshiro128Plus rng, String masterTargetString, String packName)
		{
			nextPackName = packName;
			Debug.Log("1");
			DoppelgangerSpawnCard spawnCard = DoppelgangerSpawnCard.FromMaster(srcCharacterMaster);
			if (!spawnCard)
			{
				return;
			}
			Debug.Log("2");
			Transform spawnOnTarget;
			DirectorCore.MonsterSpawnDistance input;
			if (TeleporterInteraction.instance)
			{
				spawnOnTarget = TeleporterInteraction.instance.transform;
				input = DirectorCore.MonsterSpawnDistance.Close;
			}
			else
			{
				spawnOnTarget = srcCharacterMaster.GetBody().coreTransform;
				input = DirectorCore.MonsterSpawnDistance.Far;
			}
			DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule
			{
				spawnOnTarget = spawnOnTarget,
				placementMode = DirectorPlacementRule.PlacementMode.NearestNode
			};
			Debug.Log("3");
			// CHANGE SPAWNCARD PARAMETERS TO MATCH NEEDED PARAMETERS
			CharacterMaster[] masters = UnityEngine.Resources.FindObjectsOfTypeAll<RoR2.CharacterMaster>();
			CharacterMaster targetMaster = null;
			foreach (CharacterMaster m in masters)
			{
				Debug.Log(m.name);
				if (m.name.Equals(masterTargetString))
				{
					targetMaster = m;
				}
			}
			Debug.Log("4");
			// CHANGE SPAWNCARD PARAMETERS TO MATCH NEEDED PARAMETERS
			Debug.Log("targetMaster = " + targetMaster);
			spawnCard.srcCharacterMaster = targetMaster;
			spawnCard.prefab = MasterCatalog.GetMasterPrefab(targetMaster.masterIndex);
			Debug.Log("5");
			DirectorCore.GetMonsterSpawnDistance(input, out directorPlacementRule.minDistance, out directorPlacementRule.maxDistance);
			DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, directorPlacementRule, rng);
			directorSpawnRequest.teamIndexOverride = new TeamIndex?(TeamIndex.Monster);
			directorSpawnRequest.ignoreTeamMemberLimit = true;
			Debug.Log("6");
			CombatSquad combatSquad = null;
			DirectorSpawnRequest directorSpawnRequest2 = directorSpawnRequest;
			directorSpawnRequest2.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest2.onSpawnedServer, new Action<SpawnCard.SpawnResult>(delegate (SpawnCard.SpawnResult result)
			{
				if (!combatSquad)
				{
					combatSquad = UnityEngine.Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/Encounters/ShadowCloneEncounter")).GetComponent<CombatSquad>();
				}
				combatSquad.AddMember(result.spawnedInstance.GetComponent<CharacterMaster>());
			}));
			Debug.Log("7");
			DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
			if (combatSquad)
			{
				NetworkServer.Spawn(combatSquad.gameObject);
			}
			Debug.Log("8");
			//UnityEngine.Object.Destroy(spawnCard);
		}








		public static void spawnPack()
		{

			Debug.Log("Prefabs/CharacterBodies/CommandoBody? " + Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody"));
			//SpawnCard card = SpawnCard.CreateInstance<SpawnCard>();
			MasterCopySpawnCard card = MasterCopySpawnCard.FromMaster(LocalUserManager.GetFirstLocalUser().currentNetworkUser.GetCurrentBody().master, false, false, null);
			card.prefab = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody");
			card.directorCreditCost = 0;
			card.name = "Void Warrior";

			RoR2.DirectorCore.spawnedObjects.Capacity = 99999;
			RoR2.SceneDirector.cardSelector.Capacity = 99999;

			/*
			CharacterMaster[] masters = UnityEngine.Resources.FindObjectsOfTypeAll<RoR2.CharacterMaster>();
			CharacterMaster targetMaster = null;
			foreach (CharacterMaster m in masters)
			{
				Debug.Log(m.name);
				if (m.name.Equals("CommandoMonsterMaster"))
				{
					targetMaster = m;
				}
			}
			*/

			Transform spawnOnTarget;
			DirectorCore.MonsterSpawnDistance input;
			if (TeleporterInteraction.instance)
			{
				spawnOnTarget = TeleporterInteraction.instance.transform;
				input = DirectorCore.MonsterSpawnDistance.Close;
			}
			else
			{
				spawnOnTarget = LocalUserManager.GetFirstLocalUser().currentNetworkUser.GetCurrentBody().coreTransform;
				input = DirectorCore.MonsterSpawnDistance.Far;
			}
			DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule
			{
				spawnOnTarget = spawnOnTarget,
				placementMode = DirectorPlacementRule.PlacementMode.NearestNode
			};

			DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(card, directorPlacementRule, new Xoroshiro128Plus((uint)new System.Random().Next(-int.MaxValue, int.MaxValue)));
			directorSpawnRequest.ignoreTeamMemberLimit = true;
			directorSpawnRequest.teamIndexOverride = TeamIndex.Monster;
			Debug.Log(directorSpawnRequest.teamIndexOverride.ToString());
			DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
		}
	}
}
