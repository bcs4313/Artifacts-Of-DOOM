
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
using EntityStates.BrotherMonster;
using RoR2.Projectile;

namespace ArtifactGroup
{
	class ArtifactOfSmash : ArtifactBase
	{
		private float bossDMGMult = 1.0f;
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

		// stores vector coordinates related to the velocity of the most recent hit the player has taken
		public struct knockMark
        {
			// stored "knock" values give the maximum velocity damage a player can take from one source
			public float x;
			public float y;
			public float z;
        }

		public void applyForce(DamageReport report, CharacterBody smasher, CharacterBody defender, float forceCoefficient)
		{
			float scalar = 0;
			float percentMultiplier = 0;
			if (defender.teamComponent.teamIndex == TeamIndex.Monster)
			{
				scalar = (float)Math.Log((double)(report.damageDealt / defender.healthComponent.health) * 160, 1.5);

				// multiply knockback depending on "percent" health remaining as well, like in smash
				// maximum percent multiplier is 8x for missing ALL health (for survivors)
				percentMultiplier = (float)(Math.Pow(1.8 - defender.healthComponent.health / defender.baseMaxHealth, 3.2));
			}
			else
            {
				scalar = (float)Math.Log((double)(report.damageDealt / defender.healthComponent.health) * 80 + 6, 1.5);

				// multiply knockback depending on "percent" health remaining as well, like in smash
				// maximum percent multiplier is 8x for missing ALL health (for survivors)
				percentMultiplier = (float)(Math.Pow(1.8 - defender.healthComponent.health / defender.baseMaxHealth, 2.73));
			}

			// enforce a scalar that accounts for mass
			float controlledScalar = (float)Math.Abs(defender.rigidbody.mass * scalar / 3) * percentMultiplier;


			// debug
			//Debug.Log(defender.name + ": Base Scalar = " + scalar);
			//Debug.Log(defender.name + ": Base Mass = " + defender.rigidbody.mass);
			//Debug.Log(defender.name + ": Controlled Scalar / Mass = " + (controlledScalar / defender.rigidbody.mass));

			Vector3 launch = defender.corePosition - smasher.footPosition;
			launch.Normalize();
			launch *= controlledScalar * forceCoefficient;
			//Debug.Log(defender.name + ": Final Vector = " + launch + " Acceleration: " + (launch.magnitude / defender.rigidbody.mass / 10) + " m/s^2");
			//Debug.Log("x: " + launch.x + " y:" + launch.y + " z:" + launch.z + " ");
			if (float.IsNaN(launch.x) || float.IsNaN(launch.x) || float.IsNaN(launch.x) || float.IsInfinity(launch.magnitude))
            {
				//Debug.Log(defender.name + " Launch Vector is invalid, returning from launch function: ");
				return;
            }

			if (defender.characterMotor != null)
			{
				//Debug.Log("MOT: ");
				defender.characterMotor.ApplyForce(launch);

				if (defender.teamComponent.teamIndex == TeamIndex.Player)
				{
					/*
					knockMark knock = new knockMark();
					knock.x = launch.x / defender.rigidbody.mass;
					knock.y = launch.y / defender.rigidbody.mass;
                    knock.z = launch.z / defender.rigidbody.mass;
					Debug.Log("KNOCK: X: " + knock.x + " Y: " + knock.y + " Z: " + knock.);
					*/
				}
            }
			else
			{
				//Debug.Log("RIG: ");
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

			// all actor health gets a 50% boost to account for extra damage caused by knockbacks, and to reduce
			// overall knockback sensitivity
			On.RoR2.CharacterMaster.OnBodyStart += (orig_OnBodyStart orig, global::RoR2.CharacterMaster self, global::RoR2.CharacterBody body) =>
			{
				orig.Invoke(self, body);
				if (NetworkServer.active && ArtifactEnabled)
				{
					if (body.teamComponent.teamIndex == TeamIndex.Monster)
					{
						uint idTarget = body.networkIdentity.netId.Value;
						body.baseMaxHealth *= 2f;
						new networkBehavior.informMaxHealth(idTarget, body.baseMaxHealth); // inform client of new max health
					}
					else
					{
						body.baseMaxHealth *= 2f;
						uint idTarget = body.networkIdentity.netId.Value;
						new networkBehavior.informMaxHealth(idTarget, body.baseMaxHealth); // inform client of new max health
					}
				}
			};

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
			/*
			On.RoR2.CharacterBody.Update += (orig_Update orig, RoR2.CharacterBody self) =>
			{
				if (ArtifactEnabled && NetworkServer.active && self.teamComponent.teamIndex == TeamIndex.Player)
                {
					// placeholder
					knockMark v = new knockMark();
					Vector3 cv = self.characterMotor.velocity;


					// unmark coordinate values that have undergone a 75% or more change
					// (in the opposite direction)
					float xdiff = v.x - cv.x;
					float ydiff = v.y - cv.y;
					float zdiff = v.z - cv.z;
                    if ((xdiff / v.x) > 0.75)
                    {
						v.x = 0;
                    }
					if ((ydiff / v.y) > 0.75)
					{
						v.y = 0;
					}
					if ((zdiff / v.z) > 0.75)
					{
						v.z = 0;
					}
				}
                orig.Invoke(self);
			};
			*/

			// hook to detect impacts
			On.RoR2.CharacterMotor.AfterCharacterUpdate += (orig_AfterCharacterUpdate orig, global::RoR2.CharacterMotor self, float deltaTime) =>
			{
				orig.Invoke(self, deltaTime); 
				if (ArtifactEnabled && NetworkServer.active)
				{ 
					CharacterBody characterBody = self.body;
					DamageInfo info = new DamageInfo();

					// impact damage for monsters
					if (characterBody.maxJumpHeight * 3 < Math.Abs(calculateImpact(self.lastVelocity, self.velocity)))
					{
						if (characterBody.teamComponent.teamIndex == TeamIndex.Monster)
						{
							if (characterBody.isBoss)
							{
								info.damage = (Math.Max(calculateImpact(self.lastVelocity, self.velocity), 0) * (characterBody.healthComponent.fullHealth)) * 0.006f * bossDMGMult;
								info.damageType = DamageType.BlightOnHit;
								characterBody.healthComponent.TakeDamage(info);
							}
							else
							{
								info.damage = (Math.Max(calculateImpact(self.lastVelocity, self.velocity), 0) * (characterBody.healthComponent.fullHealth)) * 0.014f * enemyDMGMult;
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

							// play a funny explosion animation on death
							if(info.damage > characterBody.healthComponent.health)
                            {
								new BlastAttack
								{
									attacker = characterBody.gameObject,
									inflictor = characterBody.gameObject,
									teamIndex = TeamIndex.Player,
									baseDamage = 0,
									baseForce = 0,
									position = characterBody.corePosition,
									radius = EntityStates.JellyfishMonster.JellyNova.novaRadius * 2,
									procCoefficient = 2f,
									attackerFiltering = AttackerFiltering.NeverHitSelf
								}.Fire();
								if (EntityStates.LunarExploderMonster.DeathState.deathExplosionEffect)
								{
									EffectManager.SpawnEffect(EntityStates.JellyfishMonster.JellyNova.novaEffectPrefab, new EffectData
									{
										origin = self.body.corePosition,
										scale = EntityStates.JellyfishMonster.JellyNova.novaRadius * 1.5f

									}, true); ;
								}

								new BlastAttack
								{
									attacker = characterBody.gameObject,
									inflictor = characterBody.gameObject,
									teamIndex = TeamIndex.Player,
									baseDamage = 0,
									baseForce = 0,
									position = characterBody.corePosition,
									radius = EntityStates.JellyfishMonster.JellyNova.novaRadius,
									procCoefficient = 2f,
									attackerFiltering = AttackerFiltering.NeverHitSelf
								}.Fire();
								if (EntityStates.LunarExploderMonster.DeathState.deathExplosionEffect)
								{
									EffectManager.SpawnEffect(EntityStates.JellyfishMonster.JellyNova.novaEffectPrefab, new EffectData
									{
										origin = self.body.corePosition,
										scale = EntityStates.JellyfishMonster.JellyNova.novaRadius * 0.5f

									}, true); ;
								}
							}

							characterBody.healthComponent.TakeDamage(info);
							//Debug.Log(characterBody.maxJumpHeight * 5.5 + " < " + Math.Abs(self.lastVelocity.magnitude - self.velocity.magnitude));
							//Debug.Log("Monster: Damage Log DMG = " + self.velocity.magnitude);

	
						}
					}

					// 
				}
			};

			// knockback hook
			On.RoR2.CharacterBody.OnTakeDamageServer += (orig_OnTakeDamageServer orig, global::RoR2.CharacterBody self, global::RoR2.DamageReport damageReport) =>
			{
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
											applyForce(damageReport, smasher, defender, 3 * OptionsLink.AOS_EnemyForceCoefficient.Value);
										}
										else
										{
											applyForce(damageReport, smasher, defender, 6.3f * OptionsLink.AOS_BossForceCoefficient.Value);
										}
									}
									else
									{
										applyForce(damageReport, smasher, defender, 4.5f * OptionsLink.AOS_PlayerForceCoefficient.Value);
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
						bossDMGMult = OptionsLink.AOS_EnemyDMGMult.Value;
					}
				}
			}
	}
}
		
