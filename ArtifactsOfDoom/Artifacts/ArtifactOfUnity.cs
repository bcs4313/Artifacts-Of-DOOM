using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using ArtifactsOfDoom;
using static On.RoR2.GlobalEventManager;
using static On.RoR2.Run;
using Messenger;
using static On.RoR2.CharacterMaster;
using static On.RoR2.GenericPickupController;
using UnityEngine.Networking;
using R2API.Networking.Interfaces;
using R2API.Networking;
using System;
using static On.RoR2.Inventory;
using System.Collections.Generic;

namespace ArtifactGroup
{
	class ArtifactOfUnity : ArtifactBase
	{
		public static bool enabled = false;
		Inventory globalInventory = new Inventory();

		public static ConfigEntry<int> TimesToPrintMessageOnStart;

		public override string ArtifactName => "Artifact of Unity";
		public override string ArtifactLangTokenName => "ARTIFACT_OF_UNITY";
		public override string ArtifactDescription => "'We have become one'\n Items are shared amongst players, but enemies scale significantly faster.";
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
			/// We can Access the Run object on initialization here...
			Run.onRunStartGlobal += overrides;
			On.RoR2.CharacterMaster.OnBodyStart += (orig_OnBodyStart orig, global::RoR2.CharacterMaster self, global::RoR2.CharacterBody body) =>
			{
				orig.Invoke(self, body);
				if (NetworkServer.active && ArtifactEnabled)
				{
					if (NetworkServer.active)
					{
						if (!ArtifactOfWar.enabled)
						{
							if (body.teamComponent.teamIndex == TeamIndex.Monster)
							{
								body.baseMaxHealth += body.levelMaxHealth * (float)Math.Pow((RoR2.Run.instance.difficultyCoefficient * 0.1 * NetworkUser.readOnlyInstancesList.Count)+1, 1.1);
								body.baseRegen += body.levelRegen * (float)Math.Pow((RoR2.Run.instance.difficultyCoefficient * 0.08 * NetworkUser.readOnlyInstancesList.Count)+1, 1.1);
								body.baseDamage += body.levelDamage * (float)Math.Pow((RoR2.Run.instance.difficultyCoefficient * 0.03 * NetworkUser.readOnlyInstancesList.Count)+1, 1.1);
								//Debug.Log("baseHMaxMultiplier: " + (float)Math.Pow((RoR2.Run.instance.difficultyCoefficient * 0.05 * NetworkUser.readOnlyInstancesList.Count) + 1, 1.1));
								//Debug.Log("baseRMaxMultiplier: " + (float)Math.Pow((RoR2.Run.instance.difficultyCoefficient * 0.07 * NetworkUser.readOnlyInstancesList.Count) + 1, 1.1));
								//Debug.Log("baseDMaxMultiplier: " + (float)Math.Pow((RoR2.Run.instance.difficultyCoefficient * 0.03 * NetworkUser.readOnlyInstancesList.Count) + 1, 1.1));
							}
						}
						else
						{
							if (body.teamComponent.teamIndex == TeamIndex.Monster)
							{
								body.baseMaxHealth += body.levelMaxHealth * (float)Math.Pow((RoR2.Run.instance.difficultyCoefficient * 0.15 * NetworkUser.readOnlyInstancesList.Count) + 1, 1.1);
								body.baseRegen += body.levelRegen * (float)Math.Pow((RoR2.Run.instance.difficultyCoefficient * 0.1 * NetworkUser.readOnlyInstancesList.Count) + 1, 1.1);
								body.baseDamage += body.levelDamage * (float)Math.Pow((RoR2.Run.instance.difficultyCoefficient * 0.2 * NetworkUser.readOnlyInstancesList.Count) + 1, 1.1);
								//Debug.Log("baseHMaxMultiplier: " + (float)Math.Pow((RoR2.Run.instance.difficultyCoefficient * 0.10 * NetworkUser.readOnlyInstancesList.Count) + 1, 1.1));
								//Debug.Log("baseRMaxMultiplier: " + (float)Math.Pow((RoR2.Run.instance.difficultyCoefficient * 0.1 * NetworkUser.readOnlyInstancesList.Count) + 1, 1.1));
								//Debug.Log("baseDMaxMultiplier: " + (float)Math.Pow((RoR2.Run.instance.difficultyCoefficient * 0.4 * NetworkUser.readOnlyInstancesList.Count) + 1, 1.1));
							}
						}
					}
				}
			};

			On.RoR2.GenericPickupController.AttemptGrant += (orig_AttemptGrant orig, global::RoR2.GenericPickupController self, global::RoR2.CharacterBody body) =>
			{
				orig.Invoke(self, body);
				if (NetworkServer.active && ArtifactEnabled)
				{
					Debug.Log("(Artifact Of UNITY) grant trigger");
					// get item index from item controller
					ItemIndex give = self.pickupIndex.itemIndex;

					for (int x = 0; x < NetworkUser.readOnlyInstancesList.Count; x++)
					{
						if (NetworkUser.readOnlyInstancesList[x].GetCurrentBody().netId.Value.CompareTo(body.netId.Value) != 0)
						{
							NetworkUser.readOnlyInstancesList[x].GetCurrentBody().inventory.GiveItem(give);
						}
					}
				}
			};
			// remove items for other users upon deleting an item from someone else
			On.RoR2.Inventory.RemoveItem_ItemIndex_int += (orig_RemoveItem_ItemIndex_int orig, global::RoR2.Inventory self, ItemIndex itemIndex, int count) =>
			{
				if (NetworkServer.active && ArtifactEnabled)
				{
					//Debug.Log("(Artifact of UNITY) delete trigger");
					CharacterBody source = null;
					// get characterbody of interest
					for (int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++)
					{
						if (NetworkUser.readOnlyInstancesList[i].GetCurrentBody().inventory == self)
						{
							source = NetworkUser.readOnlyInstancesList[i].GetCurrentBody();
						}
					}

					if (source == null)
					{
						Debug.Log("ISSUE WITH DELETE TRIGGER! (changing target to host)");
						source = NetworkUser.readOnlyInstancesList[0].GetCurrentBody();
					}
					
					if (source.teamComponent.teamIndex == TeamIndex.Player)
					{
						for (int x = 0; x < NetworkUser.readOnlyInstancesList.Count; x++)
						{
							if (NetworkUser.readOnlyInstancesList[x].GetCurrentBody() != source)
							{
								var body = NetworkUser.readOnlyInstancesList[x].GetCurrentBody();
								orig.Invoke(body.inventory, itemIndex, count);
							}
						}
					}
				}
				orig.Invoke(self, itemIndex, count);
			};

			void overrides(Run run)
			{
				if (NetworkServer.active && ArtifactEnabled)
				{
					MessageHandler.globalMessage("Together We Fight in Unity...");
					enabled = true;
				}
			}
		}
    }
}