
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
using static On.RoR2.CharacterBody;
using KinematicCharacterController;
using static On.RoR2.CharacterMotor;
using System;
using orig_Update = On.RoR2.CharacterBody.orig_Update;

namespace ArtifactGroup
{
	class ArtifactOfSmash : ArtifactBase
	{

		private float enemyDMGMult = 1.0f;
		private float playerDMGMult = 1.0f;

		public static bool enabled = false;
		Inventory globalInventory = new Inventory();

		public static ConfigEntry<int> TimesToPrintMessageOnStart;
		public override string ArtifactName => "Artifact Of Smash";
		public override string ArtifactLangTokenName => "ARTIFACT_OF_SMASH";
		public override string ArtifactDescription => "Survivors grow in brute strength, knocking back enemies in proportion to the damage dealt and their remaining health." +
			" However, monsters can do this as well, and both are injured by impacts on surfaces." +
			" Its SMASHIN TIME!";
		public override Sprite ArtifactEnabledIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/ArtifactOfSmash.png");
		public override Sprite ArtifactDisabledIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/ArtifactOfSmashDisabled.png");

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

			// hook to detect impacts
			On.RoR2.CharacterMotor.AfterCharacterUpdate += (orig_AfterCharacterUpdate orig, global::RoR2.CharacterMotor self, float deltaTime) =>
			{
				orig.Invoke(self, deltaTime); 
				if (ArtifactEnabled && NetworkServer.active)
				{ 
					CharacterBody characterBody = self.body;
					DamageInfo info = new DamageInfo();

					// impact damage
					if (characterBody.acceleration * 1.5 < Math.Max(self.lastVelocity.magnitude - self.velocity.magnitude, 0) * 10)
					{
						if (characterBody.teamComponent.teamIndex == TeamIndex.Monster)
						{
							info.damage = (Math.Max(self.lastVelocity.magnitude - self.velocity.magnitude, 0) * (characterBody.healthComponent.fullHealth)) * 0.024f * enemyDMGMult;
							info.damageType = DamageType.BlightOnHit;
							characterBody.healthComponent.TakeDamage(info);
							//Debug.Log("Monster: Damage Log DMG = " + self.velocity.magnitude);
						}
						else
                        {
							info.damage = (Math.Max(self.lastVelocity.magnitude - self.velocity.magnitude, 0) * (characterBody.healthComponent.fullHealth)) * 0.004f * playerDMGMult;
							info.damageType = DamageType.BlightOnHit;
							characterBody.healthComponent.TakeDamage(info);
							//Debug.Log("Monster: Damage Log DMG = " + self.velocity.magnitude);
						}
					}
				}
			};

			
			On.RoR2.CharacterBody.OnTakeDamageServer += (orig_OnTakeDamageServer orig, global::RoR2.CharacterBody self, global::RoR2.DamageReport damageReport) =>
			{
				orig.Invoke(self, damageReport);
				if (NetworkServer.active && ArtifactEnabled)
				{
					if (self.isFlying == false && damageReport.victim.body.characterMotor != null)
					{
						if (self != null && damageReport != null)
						{
							if (damageReport.damageInfo.damageType != DamageType.BlightOnHit)
							{
								CharacterBody smasher = damageReport.attackerBody;
								CharacterBody defender = damageReport.victimBody;
								if (smasher != null && defender != null && smasher != defender)
								{
									if (defender.teamComponent.teamIndex == TeamIndex.Monster)
									{
										if (defender.isBoss == false)
										{
											// find angle between victim and smasher
											//float angleToSMASH = Vector3.Angle(smasher.corePosition, defender.corePosition) ;
											Vector3 launch = Vector3.LerpUnclamped(smasher.footPosition, defender.corePosition,
												Math.Max((((damageReport.damageDealt / defender.healthComponent.health) * defender.characterMotor.mass) / Vector3.Distance(smasher.corePosition, defender.corePosition) * 80), 10 * defender.characterMotor.mass / Vector3.Distance(smasher.corePosition, defender.corePosition))) + Vector3.up *
												((float)Math.Log(damageReport.damageDealt / defender.healthComponent.health * 20, 1.6)) * 20;
											float scalar = damageReport.damageDealt;
											// generate vector from angle
											//Vector3 vecForAng = new Vector3(Mathf.Sin(Mathf.Deg2Rad * angleToSMASH), Mathf.Tan(Mathf.Deg2Rad * angleToSMASH), Mathf.Cos(Mathf.Deg2Rad * angleToSMASH));
											defender.characterMotor.ApplyForce(launch);

										}
										else
										{
											// find angle between victim and smasher
											//float angleToSMASH = Vector3.Angle(smasher.corePosition, defender.corePosition);
											Vector3 launch = Vector3.LerpUnclamped(smasher.footPosition, defender.corePosition,
												Math.Max((((damageReport.damageDealt / defender.healthComponent.health) * defender.characterMotor.mass) / Vector3.Distance(smasher.corePosition, defender.corePosition) * 80), 10 * defender.characterMotor.mass / Vector3.Distance(smasher.corePosition, defender.corePosition))) + Vector3.up *
												((float)Math.Log(damageReport.damageDealt / defender.healthComponent.health * 20, 1.6)) * 2;
											float scalar = damageReport.damageDealt;
											// generate vector from angle
											//Vector3 vecForAng = new Vector3(Mathf.Sin(Mathf.Deg2Rad * angleToSMASH), Mathf.Tan(Mathf.Deg2Rad * angleToSMASH), Mathf.Cos(Mathf.Deg2Rad * angleToSMASH));
											defender.characterMotor.ApplyForce(launch);
										}
									}
									else
									{
										float a = 2.6f;
										float angleToSMASH = Vector3.Angle(smasher.corePosition, defender.corePosition);
										// Set vector position as a fraction of the distance between these two markers / distance is divided for consistent smashing
										Vector3 launch = Vector3.LerpUnclamped(smasher.corePosition, defender.corePosition, Math.Max((((damageReport.damageDealt / defender.healthComponent.health) * defender.characterMotor.mass) / Vector3.Distance(smasher.corePosition, defender.corePosition) * 100), 24 / Vector3.Distance(smasher.corePosition, defender.corePosition)));
										float scalar = damageReport.damageDealt;
										// generate vector from angle
										//Vector3 vecForAng = new Vector3(Mathf.Sin(Mathf.Deg2Rad * angleToSMASH), Mathf.Tan(Mathf.Deg2Rad * angleToSMASH), Mathf.Cos(Mathf.Deg2Rad * angleToSMASH));
										defender.characterMotor.ApplyForce(launch);
									}
								}
							}
						}
					}
					else
					{
						CharacterBody smasher = damageReport.attackerBody;
						CharacterBody defender = damageReport.victimBody;
						if (smasher != null && defender != null)
						{
							if (defender.characterMotor == null || defender.characterMotor.mass == null)
							{
								//Debug.Log("x");
								// find angle between victim and smasher
								//float angleToSMASH = Vector3.Angle(smasher.corePosition, defender.corePosition);
								Vector3 launch = Vector3.LerpUnclamped(smasher.footPosition, defender.corePosition,
									Math.Max((((damageReport.damageDealt / defender.healthComponent.health)) * defender.rigidbody.mass / Vector3.Distance(smasher.corePosition, defender.corePosition) * 80), 10 * defender.rigidbody.mass * Vector3.Distance(smasher.corePosition, defender.corePosition))) + Vector3.up *
									((float)Math.Log(damageReport.damageDealt / defender.healthComponent.health * 20, 1.6)) / 420;
								float scalar = damageReport.damageDealt;
								// generate vector from angle
								//Vector3 vecForAng = new Vector3(Mathf.Sin(Mathf.Deg2Rad * angleToSMASH), Mathf.Tan(Mathf.Deg2Rad * angleToSMASH), Mathf.Cos(Mathf.Deg2Rad * angleToSMASH));
								//Debug.Log("Force = " + launch.magnitude);
								//Debug.Log("Acceleration = " + (launch.magnitude / defender.rigidbody.mass) / 140 + " m/s^2");
								//Debug.Log("Mass = " + defender.rigidbody.mass);
								if (launch.magnitude == float.NaN)
								{
									// do nothing because somehow the vector was lost (crash prevention)
								}
								else
								{
									Vector3 acceleration = (launch / defender.rigidbody.mass) / 10;
									defender.rigidbody.AddForce(acceleration, ForceMode.Acceleration);
								}
							}
							else
							{
								//Debug.Log("y");
								// find angle between victim and smasher
								//float angleToSMASH = Vector3.Angle(smasher.corePosition, defender.corePosition) ;
								Vector3 launch = Vector3.LerpUnclamped(smasher.footPosition, defender.corePosition,
									Math.Max((((damageReport.damageDealt / defender.healthComponent.health) * defender.characterMotor.mass) / Vector3.Distance(smasher.corePosition, defender.corePosition) * 80), 10 * defender.characterMotor.mass / Vector3.Distance(smasher.corePosition, defender.corePosition))) + Vector3.up *
									((float)Math.Log(damageReport.damageDealt / defender.healthComponent.health * 20, 1.6)) * 10;
								float scalar = damageReport.damageDealt;
								// generate vector from angle
								//Vector3 vecForAng = new Vector3(Mathf.Sin(Mathf.Deg2Rad * angleToSMASH), Mathf.Tan(Mathf.Deg2Rad * angleToSMASH), Mathf.Cos(Mathf.Deg2Rad * angleToSMASH));
								//Debug.Log("Force = " + launch.magnitude);
								defender.characterMotor.ApplyForce(launch);
							}
						}
					}
				}			
			};

			// replace this with info that will reward the player for basically any kill
			On.RoR2.DeathRewards.OnKilledServer += (orig, self, damageReport) =>
			{
				if(ArtifactEnabled && (damageReport.attacker == null) || damageReport.attackerBody == null)
                {
					TeamManager.instance.GiveTeamMoney(TeamIndex.Player, self.fallbackGold);
				}
				orig.Invoke(self, damageReport);
			};


			void overrides(Run run)
			{
				if (NetworkServer.active && ArtifactEnabled)
				{
					MessageHandler.globalMessage("You're making a big mistake... or Smash... I don't friggin know SMASHY SMASHY");
					enabled = true;
					playerDMGMult = OptionsLink.AOS_PlayerDMGMult.Value;
					enemyDMGMult = OptionsLink.AOS_EnemyDMGMult.Value;
				}
			}
		}
	}
}
		
