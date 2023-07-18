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
		public override Sprite ArtifactEnabledIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/ArtifactOfUnity.png");
		public override Sprite ArtifactDisabledIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/ArtifactOfUnityDisabled.png");

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

				// death storage regeneration.
				if (ArtifactEnabled && body.isPlayerControlled && body != null && deathStorage.dead_players.Contains(body.GetUserName()) && OptionsLink.AOU_DeathRecovery.Value == true)
				{
					for (int i = 0; i < deathStorage.chest.Count; i++)
					{
						if (body.master.netId == NetworkUser.readOnlyInstancesList[i].master.netId)
						{
							deathStorage.regeneratePlayer(i);
							deathStorage.dead_players.RemoveAt(i);
						}
					}
				}
			};

			On.RoR2.GenericPickupController.AttemptGrant += (orig_AttemptGrant orig, global::RoR2.GenericPickupController self, global::RoR2.CharacterBody body) =>
			{
				orig.Invoke(self, body);
				if (NetworkServer.active && ArtifactEnabled)
				{
					Debug.Log("(Artifact Of UNITY) grant trigger (code GRANT_P)");
					// get item index from item controller
					ItemIndex give = self.pickupIndex.itemIndex;
					for (int x = 0; x < NetworkUser.readOnlyInstancesList.Count; x++)
					{
						if (NetworkUser.readOnlyInstancesList[x].GetCurrentBody() != null) // if alive
						{
							if (NetworkUser.readOnlyInstancesList[x].GetCurrentBody().netId.Value.CompareTo(body.netId.Value) != 0) // if not the person who picked up the item
							{
								NetworkUser.readOnlyInstancesList[x].GetCurrentBody().inventory.GiveItem(give);
                            }
								
						}
						else // if dead
                        {
							giveToDeadPlayer(give, x);
						}
					}
				}
			};

			On.RoR2.GlobalEventManager.OnCharacterDeath += (orig_OnCharacterDeath orig, global::RoR2.GlobalEventManager self, global::RoR2.DamageReport damageReport) =>
			{
				orig.Invoke(self, damageReport);
				if (ArtifactEnabled && damageReport.victim.body.isPlayerControlled && OptionsLink.AOU_DeathRecovery.Value == true) // check if a player died
				{
					MessageHandler.globalMessage(damageReport.victim.body.GetUserName() + " Has died! He will still receive items over time, though");
					if (!deathStorage.dead_players.Contains(damageReport.victim.body.GetUserName()))
					{
						deathStorage.dead_players.Add(damageReport.victim.body.GetUserName());
					}
				}
			};

			On.RoR2.GlobalEventManager.OnCharacterHitGroundServer += (orig_OnCharacterHitGroundServer orig, global::RoR2.GlobalEventManager self, global::RoR2.CharacterBody characterBody, Vector3 impactVelocity) =>
			{
				orig.Invoke(self, characterBody, impactVelocity);
			};

			// remove items for other users upon deleting an item from someone else
			On.RoR2.Inventory.RemoveItem_ItemIndex_int += (orig_RemoveItem_ItemIndex_int orig, global::RoR2.Inventory self, ItemIndex itemIndex, int count) =>
			{
				try
				{
					if (NetworkServer.active && ArtifactEnabled)
					{
						//Debug.Log("(Artifact of UNITY) delete trigger");
						CharacterBody source = null;
						// get characterbody of interest
						for (int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++)
						{
							if(NetworkUser.readOnlyInstancesList[i].GetCurrentBody() == null) // dead player (for deathstorage
                            {
								deathStorage.deathDestroy(itemIndex, i);
							}
							else if (NetworkUser.readOnlyInstancesList[i].GetCurrentBody().inventory == self)
							{
								source = NetworkUser.readOnlyInstancesList[i].GetCurrentBody();
							}
						}

						if (source == null)
						{
							Debug.Log("ISSUE WITH DELETE TRIGGER! (changing target to host)");
							source = NetworkUser.readOnlyInstancesList[0].GetCurrentBody();
						}

						// RoR2 deletes items to replace them with void ones, which needs to be accounted for as a special case (DO NOT REMOVE)
						if (RoR2.Items.ContagiousItemManager.FindInventoryReplacementCandidateIndex(source.inventory, itemIndex) == -1) // if a contagious item replacement doesn't exist
						{ // remove the item as normal
							if (source.teamComponent.teamIndex == TeamIndex.Player)
							{
								Debug.Log("(Artifact Of UNITY) delete trigger (code DELETE_T)");
								for (int x = 0; x < NetworkUser.readOnlyInstancesList.Count; x++)
								{
									if (NetworkUser.readOnlyInstancesList[x].GetCurrentBody() != source)
									{
										var body = NetworkUser.readOnlyInstancesList[x].GetCurrentBody();
										orig.Invoke(body.inventory, itemIndex, count);
									}
								}
							}
							else
							{
								Debug.Log("(Artifact Of UNITY) delete trigger denied (code DELETE_D)");
							}
						}
					}
				}
				catch { } // this thing barfs errors for pretty much no reason, even if the entire starting if statement fails. WHY.
				orig.Invoke(self, itemIndex, count);
			};
			

			void overrides(Run run)
			{
				if (NetworkServer.active && ArtifactEnabled)
				{
					MessageHandler.globalMessage("Together We Fight in Unity...");
					enabled = true;
					deathStorage.init();
				}
			}
		}
		static public void giveToDeadPlayer(ItemIndex dex, int plyr)
		{
			if (OptionsLink.AOU_DeathRecovery.Value == true)
			{
				int colorID = 1;
				NetworkUser user = NetworkUser.readOnlyInstancesList[plyr];

				deathStorage.deathUpdate(dex, plyr);
				MessageHandler.GlobalItemDeadMessage(dex, colorID, deathStorage.retrieveUsername(plyr));
			}
		}
	}
}

