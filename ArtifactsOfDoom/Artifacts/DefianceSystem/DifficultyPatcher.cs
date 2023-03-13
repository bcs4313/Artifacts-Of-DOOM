using System;
using HarmonyLib;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace csProj.Artifacts.DefianceSystem
{
	internal class DifficultyPatcher
	{
		[HarmonyPatch(typeof(Run), "livingPlayerCount")]
		[HarmonyPatch(typeof(Run), "participatingPlayerCount")]
		[HarmonyPostfix]
		private static int AdjustPlayerCount(int realPlayerCount)
		{
			return DifficultyPatcher.survivorCount;
		}

		public static int GetRealPlayerCount(int adjustedPlayerCount)
		{
			return DifficultyPatcher.survivorCount;
		}

		public void addPatches()
		{
			bool flag = DifficultyPatcher.harmony == null && NetworkServer.active;
			if (flag)
			{
				DifficultyPatcher.harmony = Harmony.CreateAndPatchAll(typeof(DifficultyPatcher), null);
				SceneDirector.onPrePopulateSceneServer += DifficultyPatcher.AdjustInteractableCredits;
				BossGroup.onBossGroupStartServer += DifficultyPatcher.AdjustBReward;
			}
		}

		private static void AdjustBReward(BossGroup group)
		{
			group.scaleRewardsByPlayerCount = false;
			group.bonusRewardCount = DifficultyPatcher.survivorCount;
		}

		private static void AdjustInteractableCredits(SceneDirector __instance)
		{
			int num = 0;
			ClassicStageInfo instance = ClassicStageInfo.instance;
			foreach (ClassicStageInfo.BonusInteractibleCreditObject bonusInteractibleCreditObject in ((instance != null) ? instance.bonusInteractibleCreditObjects : null) ?? new ClassicStageInfo.BonusInteractibleCreditObject[0])
			{
				GameObject objectThatGrantsPointsIfEnabled = bonusInteractibleCreditObject.objectThatGrantsPointsIfEnabled;
				bool flag = objectThatGrantsPointsIfEnabled != null && objectThatGrantsPointsIfEnabled.activeSelf;
				if (flag)
				{
					num += bonusInteractibleCreditObject.points;
				}
			}
			decimal num2 = (__instance.interactableCredit - num) * DifficultyPatcher.survivorCount / (Run.instance.participatingPlayerCount + 1);
		}

		private static Harmony harmony;

		public static int survivorCount;
	}
}
