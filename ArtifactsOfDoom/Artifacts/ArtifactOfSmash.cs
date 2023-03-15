
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

		public void applyForce(DamageReport report, CharacterBody smasher, CharacterBody defender, float forceCoefficient)
		{
			// determine scalar quantity for applied force
			float scalar = (report.damageDealt / defender.healthComponent.health) * 70;

			// enforce a maximum force value to make forces more stable
			float controlledScalar = (float)Math.Abs(defender.rigidbody.mass * scalar / 3);


			// debug
			Debug.Log(defender.name + ": Base Scalar = " + scalar);
			Debug.Log(defender.name + ": Base Mass = " + defender.rigidbody.mass);
			Debug.Log(defender.name + ": Controlled Scalar / Mass = " + (controlledScalar / defender.rigidbody.mass));

			Vector3 launch = defender.corePosition - smasher.footPosition;
			launch.Normalize();
			launch *= controlledScalar * forceCoefficient;
			Debug.Log(defender.name + ": Final Vector = " + launch + " Acceleration: " + (launch.magnitude / defender.rigidbody.mass / 10) + " m/s^2");
			Debug.Log("x: " + launch.x + " y:" + launch.y + " z:" + launch.z + " ");
			if (float.IsNaN(launch.x) || float.IsNaN(launch.x) || float.IsNaN(launch.x) || float.IsInfinity(launch.magnitude))
            {
				Debug.Log(defender.name + " Launch Vector is invalid, returning from launch function: ");
				return;
            }

			if (defender.characterMotor != null)
			{
				Debug.Log("MOT: ");
				defender.characterMotor.ApplyForce(launch);
			}
			else
			{
				Debug.Log("RIG: ");
				defender.rigidbody.AddForce((launch / defender.rigidbody.mass) * 45, ForceMode.Acceleration);
			}
		}

		// this calculation takes into account redirected forces from impacts, which isn't accounted for
		// by magnitude calculations alone
		public float calculateImpact(Vector3 lastVelocity, Vector3 newVelocity)
        {
			return Math.Abs(lastVelocity.x - newVelocity.x) + Math.Abs(lastVelocity.y - newVelocity.y) + Math.Abs(lastVelocity.z - newVelocity.z);
		}

		public override void Hooks()
		{
			/// We can Access the Run object on initialization here...
			Run.onRunStartGlobal += overrides;

			// replace this with info that will reward the player for basically any kill
			On.RoR2.DeathRewards.OnKilledServer += (orig, self, damageReport) =>
			{
				if (ArtifactEnabled && damageReport.victimBody.teamComponent.teamIndex == TeamIndex.Monster)
				{
					TeamManager.instance.GiveTeamMoney(TeamIndex.Player, damageReport.victimMaster.money);
				}
				else
                {
					orig.Invoke(self, damageReport);
				}
			};

			// hook to detect impacts
			On.RoR2.CharacterMotor.AfterCharacterUpdate += (orig_AfterCharacterUpdate orig, global::RoR2.CharacterMotor self, float deltaTime) =>
			{
				orig.Invoke(self, deltaTime); 
				if (ArtifactEnabled && NetworkServer.active)
				{ 
					CharacterBody characterBody = self.body;
					DamageInfo info = new DamageInfo();

					// impact damage for monsters
					if (characterBody.maxJumpHeight * 1.5 < Math.Abs(calculateImpact(self.lastVelocity, self.velocity)))
					{
						if (characterBody.teamComponent.teamIndex == TeamIndex.Monster)
						{
							if (characterBody.isBoss)
							{
								info.damage = (Math.Max(calculateImpact(self.lastVelocity, self.velocity), 0) * (characterBody.healthComponent.fullHealth)) * 0.006f * enemyDMGMult;
								info.damageType = DamageType.BlightOnHit;
								characterBody.healthComponent.TakeDamage(info);
							}
							else
							{
								info.damage = (Math.Max(calculateImpact(self.lastVelocity, self.velocity), 0) * (characterBody.healthComponent.fullHealth)) * 0.02f * enemyDMGMult;
								info.damageType = DamageType.BlightOnHit;
								characterBody.healthComponent.TakeDamage(info);
                            }
							
						}
					}

					// impact damage for survivors
					if (characterBody.maxJumpHeight * 8 < Math.Abs(calculateImpact(self.lastVelocity, self.velocity)))
					{
						if (characterBody.teamComponent.teamIndex == TeamIndex.Player)
						{
							info.damage = (Math.Max(calculateImpact(self.lastVelocity, self.velocity), 0) * (characterBody.healthComponent.fullHealth)) * 0.0025f * playerDMGMult;
							info.damageType = DamageType.BlightOnHit;
							characterBody.healthComponent.TakeDamage(info);
							Debug.Log(characterBody.maxJumpHeight * 5.5 + " < " + Math.Abs(self.lastVelocity.magnitude - self.velocity.magnitude));
							//Debug.Log("Monster: Damage Log DMG = " + self.velocity.magnitude);
						}
					}
				}
			};

			// knockback hook
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
											applyForce(damageReport, smasher, defender, 3);
										}
										else
										{
											applyForce(damageReport, smasher, defender, 3.3f);
										}
									}
									else
									{
										applyForce(damageReport, smasher, defender, 4.5f);
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
								applyForce(damageReport, smasher, defender, 3);
							}
							else
							{
								applyForce(damageReport, smasher, defender, 3);
							}
						}
					}
				};
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
		
