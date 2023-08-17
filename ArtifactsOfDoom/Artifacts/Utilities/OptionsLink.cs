using System;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using UnityEngine;
using UnityEngine.Events;

namespace ArtifactsOfDoom
{
	internal static class OptionsLink
	{
		public static void constructSettings()
		{
			Debug.Log("AOD) Setting Icon and Desc");
			Sprite s = Main.MainAssets.LoadAsset<Sprite>("Assets/Icons/icon.png");
			ModSettingsManager.SetModIcon(s);
			ModSettingsManager.SetModDescription("Epic Gaming Moment.");
			Debug.Log("(AOD) Done");

			OptionsLink.AOW_BaseDropChance = OptionsLink.Config.Bind<float>("Artifact of War", "Base Item Droprate", 5f, "Base value for the player to get an item on a monster kill. Scales exponentially in later stages. \nRange: (0, 100) \nDefault: 5");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOW_BaseDropChance, new SliderConfig
			{
				min = 0f,
				max = 100f,
				formatString = "{0:F1}%"
			}));
			OptionsLink.AOW_DropChanceExpScaling = OptionsLink.Config.Bind<float>("Artifact of War", "Droprate Exp. Scaling", 1.4f, "Exponent that increases the base droprate value per stage. \nRange: (0, 4) \nDefault: 1.4");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOW_DropChanceExpScaling, new SliderConfig
			{
				min = 0f,
				max = 4f,
				formatString = "{0:F2}"
			}));
			OptionsLink.AOW_EvolutionExpScaling = OptionsLink.Config.Bind<float>("Artifact of War", "Monster Item Scaling", 2.4f, "Exponent that increases the amount of items given to monsters. (Scales with time/stage). \nRange: (0, 4) \nDefault: 2.4");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOW_EvolutionExpScaling, new SliderConfig
			{
				min = 0f,
				max = 4f,
				formatString = "{0:F2}"
			}));
			OptionsLink.AOW_SwarmTime = OptionsLink.Config.Bind<string>("Artifact of War", "Swarm Multiplier Timer", "5", "Total Time (in minutes) it takes to increment the swarm multiplier by 1. \nDefault: 5");
			ModSettingsManager.AddOption(new StringInputFieldOption(OptionsLink.AOW_SwarmTime));
			OptionsLink.AOW_MaxSwarm = OptionsLink.Config.Bind<string>("Artifact of War", "Monster Swarm Cap", "5", "Highest amount of monsters that can spawn per director spawn (swarm effect). \nRange: (0.0-inf) \nDefault: 5");
			ModSettingsManager.AddOption(new StringInputFieldOption(OptionsLink.AOW_MaxSwarm));
			OptionsLink.AOW_EntityCap = OptionsLink.Config.Bind<string>("Artifact of War", "Monster Count Cap", "250", "Max amount of monsters that can exist on a stage at once. \nRange: (0.0-inf) \nDefault: 250");
			ModSettingsManager.AddOption(new StringInputFieldOption(OptionsLink.AOW_EntityCap));

			OptionsLink.AOW_OmitMessages = OptionsLink.Config.Bind<bool>("Artifact of War", "Omit Messages", false, "Players will not receive item messages if they are given an item by this artifact. \nDefault: false");
			ModSettingsManager.AddOption(new CheckBoxOption(OptionsLink.AOW_OmitMessages));

			
			OptionsLink.AOW_EnemyDmgScaling = OptionsLink.Config.Bind<float>("Artifact of War", "Enemy Damage Scaling", 1f, "An exponent for the damage scaling of monsters. Used to counter player health scaling. \nChange this setting pre-run \nRange: (0.1-2.0) \nDefault: 1");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOW_EnemyDmgScaling, new SliderConfig
			{
				min = 0.1f,
				max = 2f,
				formatString = "{0:F2}"
			}));

			OptionsLink.AOW_EnemyHealthScaling = OptionsLink.Config.Bind<float>("Artifact of War", "Enemy Health Scaling", 1f, "An exponent for the health scaling of monsters. Increases with difficulty and stage. Used to counter the player's high item count. \nChange this setting pre-run \nRange: (0.1-2.0) \nDefault: 1");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOW_EnemyHealthScaling, new SliderConfig
			{
				min = 0.1f,
				max = 2f,
				formatString = "{0:F2}"
			}));

			OptionsLink.AOW_PlayerHealthScaling = OptionsLink.Config.Bind<float>("Artifact of War", "Player Health Scaling", 1f, "An exponent for the health scaling of players. Base health increases every stage. \nUsed to counter the monster team's high item count. \nChange this setting pre-run \nRange: (0.1-2.0) \nDefault: 1");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOW_PlayerHealthScaling, new SliderConfig
			{
				min = 0.1f,
				max = 2f,
				formatString = "{0:F2}"
			}));
			



			ModSettingsManager.AddOption(new GenericButtonOption("Return to Default Settings", "Artifact of War", "Return to the base settings of this artifact.", "Default Settings", new UnityAction(OptionsLink.warBase)));
			
			OptionsLink.AOU_DeathRecovery = OptionsLink.Config.Bind<bool>("Artifact of Unity", "Death Recovery", true, "Players that die will continue to receive the items collected by other players. The items will be restored once they are revived.");
			ModSettingsManager.AddOption(new CheckBoxOption(OptionsLink.AOU_DeathRecovery));

			ModSettingsManager.AddOption(new GenericButtonOption("Import Entropy Config", "Artifact of Entropy", "Import settings from someone else to get their Entropy run experience! Pasted from your clipboard, CTRL+C to copy.", "Import", new UnityAction(OptionsLink.loadConfig)));
			ModSettingsManager.AddOption(new GenericButtonOption("Export Entropy Config", "Artifact of Entropy", "Export your entropy run setup to share with other people! Be sure to input the run seed in the settings before you export. Saved via clipboard, CTRL+V to paste.", "Export", new UnityAction(OptionsLink.saveConfig)));
			OptionsLink.AOE_Seed = OptionsLink.Config.Bind<string>("Artifact of Entropy", "Generation Seed", "-1", "Numerical seed that strictly defines the outcome to action links for the run. An input of -1 creates a random seed.A run seed is printed both in chat and in the Debug Log at the start of each stage in this format: (Artifact of Entropy Seed): num This seed can be shared with other players so they can experience your run!. Note: To produce an identical run, settings have to match, or some things may be different. \nRange: (-2,147,483,647, 2,147,483,647)\nDefault: -1");
			OptionsLink.AOE_Seed_O = new StringInputFieldOption(OptionsLink.AOE_Seed);
			ModSettingsManager.AddOption(OptionsLink.AOE_Seed_O);
			OptionsLink.AOE_EventHookMultiplier = OptionsLink.Config.Bind<string>("Artifact of Entropy", "Event Hook Multiplier", "1.0", "This is a multiplier for the likelihood of an event (ex: summoning a random monster) to link itself to an individual action (ex: player damaging a monster) upon starting the game. It does not affect its assigned probability of each event occuring, only the average quantity of event to action links. \nRange: (0.0-inf) \nDefault: 1.0");
			OptionsLink.AOE_EventHookMultiplier_O = new StringInputFieldOption(OptionsLink.AOE_EventHookMultiplier);
			ModSettingsManager.AddOption(OptionsLink.AOE_EventHookMultiplier_O);
			OptionsLink.AOE_EventChanceMultiplier = OptionsLink.Config.Bind<string>("Artifact of Entropy", "Event Chance Multiplier", "1.0", "Multiplies the percentage chance of all events. If an event with a 30% chance had this setting at 2.0, its new probability would then be 60%. Applied BEFORE the offset. \nRange: (0.0-inf)\nDefault: 1.0");
			OptionsLink.AOE_EventChanceMultiplier_O = new StringInputFieldOption(OptionsLink.AOE_EventChanceMultiplier);
			ModSettingsManager.AddOption(OptionsLink.AOE_EventChanceMultiplier_O);
			OptionsLink.AOE_EventChanceOffset = OptionsLink.Config.Bind<float>("Artifact of Entropy", "Event Chance Offset", 0f, "Adds a fixed percentage chance to all events. If an event with a 27% chance to occur had this setting at -25.0, its new probability would then be 2%. \nRange: (-100.0, 100.0) \nDefault: 0.00");
			OptionsLink.AOE_EventChanceOffset_O = new SliderOption(OptionsLink.AOE_EventChanceOffset, new SliderConfig
			{
				min = -100f,
				max = 100f,
				formatString = "{0:F2}"
			});
			ModSettingsManager.AddOption(OptionsLink.AOE_EventChanceOffset_O);
			OptionsLink.AOE_SimultaneousOutcomes = OptionsLink.Config.Bind<bool>("Artifact of Entropy", "Simultaneous Outcomes", false, "All Events under the same action will occur at the same time. \nDefault: False");
			OptionsLink.AOE_SimultaneousOutcomes_O = new CheckBoxOption(OptionsLink.AOE_SimultaneousOutcomes);
			ModSettingsManager.AddOption(OptionsLink.AOE_SimultaneousOutcomes_O);
			OptionsLink.AOE_NewEventsPerStage = OptionsLink.Config.Bind<bool>("Artifact of Entropy", "New Events Per Stage", false, "A new set of outcomes will be generated and linked to actions each time a stage is completed. \nDefault: False");
			OptionsLink.AOE_NewEventsPerStage_O = new CheckBoxOption(OptionsLink.AOE_NewEventsPerStage);
			ModSettingsManager.AddOption(OptionsLink.AOE_NewEventsPerStage_O);
			OptionsLink.AOE_OmitMessages = OptionsLink.Config.Bind<bool>("Artifact of Entropy", "Omit Messages", false, "Players will not receive messages from the effects triggered by this artifact. \nDefault: false");
			ModSettingsManager.AddOption(new CheckBoxOption(OptionsLink.AOE_OmitMessages));
			ModSettingsManager.AddOption(new GenericButtonOption("Return to Default Settings", "Artifact of Entropy", "Return to the base settings of this artifact.", "Default Settings", new UnityAction(OptionsLink.voidBase)));
			OptionsLink.AOT_CreditScalingMultiplier = OptionsLink.Config.Bind<float>("Artifact of The Titans", "Credit Scaling", 0.15f, "How much monsters scale to credit cost (linear). Doesn't apply to teleporter bosses. \nRange: (0, 4) \nDefault: 0.15");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOT_CreditScalingMultiplier, new SliderConfig
			{
				min = 0f,
				max = 4f,
				formatString = "{0:F2}"
			}));
			OptionsLink.AOT_BossGrowthPerStage = OptionsLink.Config.Bind<float>("Artifact of The Titans", "Boss Growth", 2.6f, "The exponential growth of bosses per stage (max 10 stages). This setting can produce some dramatic differences with small changes. \nRange: (0, 5) \nDefault: 2.60");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOT_BossGrowthPerStage, new SliderConfig
			{
				min = 0f,
				max = 5f,
				formatString = "{0:F2}"
			}));
			OptionsLink.AOT_EliteMultiplier = OptionsLink.Config.Bind<float>("Artifact of The Titans", "Elite Multiplier", 0.4f, "Applies an additional scaling factor to elite enemies and bosses. \nRange: (0, 4) \nDefault: 0.40");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOT_EliteMultiplier, new SliderConfig
			{
				min = 0f,
				max = 4f,
				formatString = "{0:F2}"
			}));
			OptionsLink.AOT_FixedMonsterScaling = OptionsLink.Config.Bind<float>("Artifact of The Titans", "Fixed Monster Scale", 0f, "An unchanging value to additionaly scale to all non-Boss monsters. For example, a beetle guard would normally be scaled to a factor of 3.4. \nRange: (0, 10) \nDefault: 0");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOT_FixedMonsterScaling, new SliderConfig
			{
				min = 0f,
				max = 10f,
				formatString = "{0:F2}"
			}));
			OptionsLink.AOT_FixedBossScaling = OptionsLink.Config.Bind<float>("Artifact of The Titans", "Fixed Boss Scale", 0f, "An unchanging value to additionaly scale to all Boss monsters. This setting is sensitive. \nRange: (0, 10) \nDefault: 0");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOT_FixedBossScaling, new SliderConfig
			{
				min = 0f,
				max = 10f,
				formatString = "{0:F2}"
			}));
			ModSettingsManager.AddOption(new GenericButtonOption("Return to Default Settings", "Artifact of The Titans", "Return to the base settings of this artifact.", "Default Settings", new UnityAction(OptionsLink.titanBase)));


			OptionsLink.AOS_PlayerDMGMult = OptionsLink.Config.Bind<float>("Artifact of Smash", "Player Impact Dmg Mult.", 1f, "Multiplier for damage done to survivors via impact. \nRange: (0, 5) \nDefault: 1");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOS_PlayerDMGMult, new SliderConfig
			{
				min = 0f,
				max = 5f,
				formatString = "{0:F2}"
			}));

			OptionsLink.AOS_PlayerForceCoefficient = OptionsLink.Config.Bind<float>("Artifact of Smash", "Player Knockback Coefficient.", 1f, "All changes in acceleration to the player are multiplied by this coefficient. \nRange: (0, 10) \nDefault: 1");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOS_PlayerForceCoefficient, new SliderConfig
			{
				min = 0f,
				max = 10f,
				formatString = "{0:F2}"
			}));
			OptionsLink.AOS_EnemyDMGMult = OptionsLink.Config.Bind<float>("Artifact of Smash", "Enemy Impact Dmg Mult.", 1f, "Multiplier for damage done to non-boss enemies via impact. \nRange: (0, 5) \nDefault: 1");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOS_EnemyDMGMult, new SliderConfig
			{
				min = 0f,
				max = 5f,
				formatString = "{0:F2}"
			}));
			OptionsLink.AOS_EnemyForceCoefficient = OptionsLink.Config.Bind<float>("Artifact of Smash", "Enemy Knockback Coefficient.", 1f, "All changes in acceleration to non-boss enemies are multiplied by this coefficient. \nRange: (0, 10) \nDefault: 1");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOS_EnemyForceCoefficient, new SliderConfig
			{
				min = 0f,
				max = 10f,
				formatString = "{0:F2}"
			}));
			OptionsLink.AOS_BossDMGMult = OptionsLink.Config.Bind<float>("Artifact of Smash", "Boss Impact Dmg Mult.", 1f, "Multiplier for damage done to boss enemies via impact. \nRange: (0, 5) \nDefault: 1");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOS_BossDMGMult, new SliderConfig
			{
				min = 0f,
				max = 5f,
				formatString = "{0:F2}"
			}));
			OptionsLink.AOS_BossForceCoefficient = OptionsLink.Config.Bind<float>("Artifact of Smash", "Boss Knockback Coefficient.", 1f, "All changes in acceleration to boss enemies are multiplied by this coefficient. \nRange: (0, 10) \nDefault: 1");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOS_BossForceCoefficient, new SliderConfig
			{
				min = 0f,
				max = 10f,
				formatString = "{0:F2}"
			}));
			ModSettingsManager.AddOption(new GenericButtonOption("Return to Default Settings", "Artifact of Smash", "Return to the base settings of this artifact.", "Default Settings", new UnityAction(OptionsLink.smashBase)));
			
			OptionsLink.AOD_KeyUI = OptionsLink.Config.Bind<KeyboardShortcut>("Artifact of Reconstruction", "UI Keybind", new KeyboardShortcut(KeyCode.F2, Array.Empty<KeyCode>()), "Key to open and close the ui to transform into a monster (Client Side). \nDefault: F2");
			ModSettingsManager.AddOption(new KeyBindOption(OptionsLink.AOD_KeyUI));

			/*
			OptionsLink.AOM_DamageMultiplier = OptionsLink.Config.Bind<string>("Artifact of Reconstruction", "Default Base Dmg Multiplier", "1.0", "Default Multiplier for the base damage of your morph. \nRange: (0.0-inf) \nDefault: 1.0");
			OptionsLink.AOM_DamageMultiplier_O = new StringInputFieldOption(OptionsLink.AOM_DamageMultiplier);
			ModSettingsManager.AddOption(AOM_DamageMultiplier_O);

			OptionsLink.AOM_HealthMultiplier = OptionsLink.Config.Bind<string>("Artifact of Reconstruction", "Default Base Health Multiplier", "1.0", "Default Multiplier for the base health of your morph. \nRange: (0.0-inf) \nDefault: 1.0");
			OptionsLink.AOM_HealthMultiplier_O = new StringInputFieldOption(OptionsLink.AOM_HealthMultiplier);
			ModSettingsManager.AddOption(AOM_HealthMultiplier_O);

			OptionsLink.AOM_SpeedMultiplier = OptionsLink.Config.Bind<string>("Artifact of Reconstruction", "Default Base Speed Multiplier", "1.0", "Default Multiplier for the base speed of your morph. \nRange: (0.0-inf) \nDefault: 1.0");
			OptionsLink.AOM_SpeedMultiplier_O = new StringInputFieldOption(OptionsLink.AOM_SpeedMultiplier);
			ModSettingsManager.AddOption(AOM_SpeedMultiplier_O);

			OptionsLink.AOM_AttackSpeedMultiplier = OptionsLink.Config.Bind<string>("Artifact of Reconstruction", "Default Attack Speed Multiplier", "1.0", "Default Multiplier for the attack speed of your morph. \nRange: (0.0-inf) \nDefault: 1.0");
			OptionsLink.AOM_AttackSpeedMultiplier_O = new StringInputFieldOption(OptionsLink.AOM_AttackSpeedMultiplier);
			ModSettingsManager.AddOption(AOM_AttackSpeedMultiplier_O);

			OptionsLink.AOM_CooldownMultiplier = OptionsLink.Config.Bind<string>("Artifact of Reconstruction", "Default Cooldown Multiplier", "1.0", "Default Multiplier for the ability cooldowns of your morph. (smaller numbers reduce cooldowns) \nRange: (0.0-inf) \nDefault: 1.0");
			OptionsLink.AOM_CooldownMultiplier_O = new StringInputFieldOption(OptionsLink.AOM_CooldownMultiplier);
			ModSettingsManager.AddOption(AOM_CooldownMultiplier_O);

			OptionsLink.AOM_SizeMultiplier = OptionsLink.Config.Bind<float>("Artifact of Reconstruction", "Default Size Multiplier", 1f, "Default for how large your morph is.. Values > 3 produce extremely chunky transformations. \nRange: (0.05, 5) \nDefault: 1");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOM_SizeMultiplier, new SliderConfig
			{
				min = 0.05f,
				max = 5f,
				formatString = "{0:F2}"
			}));
			*/
			ModSettingsManager.AddOption(new GenericButtonOption("Return to Default Settings", "Artifact of Reconstruction", "Return to the base settings of this artifact.", "Default Settings", new UnityAction(OptionsLink.morphBase)));
			/*
			OptionsLink.AOM_MorphSelect = OptionsLink.Config.Bind<string>("Artifact of Metamorphosis", "Morph Select", "Beetle", "The monster that you will transform into upon entering the world. \n For now, this applies to ALL players when the artifact is enabled. (Default: Beetle)");
			ModSettingsManager.AddOption(new ChoiceOption(Config.Bind("Disable",
				"Beetle", "Beetle Guard"),
				new ChoiceConfig { checkIfDisabled = Disabled }));
			*/

			/*
			OptionsLink.AOD_Blacklist = OptionsLink.Config.Bind<string>("Artifact of Defiance", "Enemy Blacklist", "Beetle, LesserWisp", "A list of monsters that the enemy monster team no longer can spawn as. Enter each monster as its name in the game (middle clicking on a monster will reveal the name). Each value is comma separated, without quotations.");
			ModSettingsManager.AddOption(new StringInputFieldOption(OptionsLink.AOD_Blacklist));
			OptionsLink.AOD_ForceBossSpawn = OptionsLink.Config.Bind<bool>("Artifact of Defiance", "Force Teleporter Boss Spawn", true, "Forces a monster team member to always spawn as the teleporter boss. The enemy blacklist overrides this rule (Default: true).");
			ModSettingsManager.AddOption(new CheckBoxOption(OptionsLink.AOD_ForceBossSpawn));
			OptionsLink.AOD_VisiblePlayers = OptionsLink.Config.Bind<bool>("Artifact of Defiance", "Distinguished Players", true, "Should players be able to see who is controlliing the monsters? Player controlled monsters will have a red light attached to them (Default: true).");
			ModSettingsManager.AddOption(new CheckBoxOption(OptionsLink.AOD_VisiblePlayers));
			OptionsLink.AOD_DeathMessages = OptionsLink.Config.Bind<bool>("Artifact of Defiance", "Death Messages", false, "If a player on the monster team dies, should a death message in chat, with their name, be displayed? (Default: false).");
			ModSettingsManager.AddOption(new CheckBoxOption(OptionsLink.AOD_DeathMessages));
			OptionsLink.AOD_OneShotProtection = OptionsLink.Config.Bind<bool>("Artifact of Defiance", "One Shot Protection", false, "Should an enemy monster have one shot protection? (Default: false).");
			ModSettingsManager.AddOption(new CheckBoxOption(OptionsLink.AOD_OneShotProtection));
			OptionsLink.AOD_ExtraLevels = OptionsLink.Config.Bind<float>("Artifact of Defiance", "Additional Levels", 1f, "Additional levels given to the monster team players (after the level multiplier). \nRange: (1, 99) \nDefault: 1");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOD_ExtraLevels, new SliderConfig
			{
				min = 1f,
				max = 99f,
				formatString = "{0:F1}"
			}));
			OptionsLink.AOD_LevelMultiplier = OptionsLink.Config.Bind<float>("Artifact of Defiance", "Level Multiplier", 1f, "A multiplier of a monster team player's level, with the original monster serving as the base level value.  \nRange: (0, 10) \nDefault: 1");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOD_LevelMultiplier, new SliderConfig
			{
				min = 0f,
				max = 10f,
				formatString = "{0:F1}"
			}));
			OptionsLink.AOD_KeySuicide = OptionsLink.Config.Bind<KeyboardShortcut>("Artifact of Defiance", "Suicide Keybind", new KeyboardShortcut(KeyCode.Alpha3, Array.Empty<KeyCode>()), "Press this key to kill yourself as a monster. Useful for situations where you spawn as an immobile monster or something that is too weak to fight with.  \nDefault: 3");
			ModSettingsManager.AddOption(new KeyBindOption(OptionsLink.AOD_KeySuicide));
			OptionsLink.AOD_KeyChooseHuman = OptionsLink.Config.Bind<KeyboardShortcut>("Artifact of Defiance", "Join Survivors Keybind", new KeyboardShortcut(KeyCode.Alpha1, Array.Empty<KeyCode>()), "Press this key to join the survivor team when the run starts.  \nDefault: 1");
			ModSettingsManager.AddOption(new KeyBindOption(OptionsLink.AOD_KeyChooseHuman));
			OptionsLink.AOD_KeyChooseMonster = OptionsLink.Config.Bind<KeyboardShortcut>("Artifact of Defiance", "Join Monsters Keybind", new KeyboardShortcut(KeyCode.Alpha2, Array.Empty<KeyCode>()), "Press this key to join the monster team when the run starts.  \nDefault: 2");
			ModSettingsManager.AddOption(new KeyBindOption(OptionsLink.AOD_KeyChooseMonster));
			*/
		}


		private static void saveConfig()
		{
			string text = "";
			text = text + OptionsLink.AOE_Seed.Value + "|";
			text = text + OptionsLink.AOE_EventHookMultiplier.Value + "|";
			text = text + OptionsLink.AOE_EventChanceMultiplier.Value + "|";
			text = text + OptionsLink.AOE_EventChanceOffset.Value.ToString() + "|";
			text = text + OptionsLink.AOE_SimultaneousOutcomes.Value.ToString() + "|";
			text += OptionsLink.AOE_NewEventsPerStage.Value.ToString();
			GUIUtility.systemCopyBuffer = text;
			Debug.Log("Copied Entropy Code " + GUIUtility.systemCopyBuffer + " to the clipboard!");
		}

		private static void loadConfig()
		{
			string systemCopyBuffer = GUIUtility.systemCopyBuffer;
			string[] array = systemCopyBuffer.Split(new char[]
			{
				'|'
			});
			OptionsLink.AOE_Seed.Value = array[0];
			OptionsLink.AOE_EventHookMultiplier.Value = array[1];
			OptionsLink.AOE_EventChanceMultiplier.Value = array[2];
			OptionsLink.AOE_EventChanceOffset.Value = float.Parse(array[3]);
			OptionsLink.AOE_SimultaneousOutcomes.Value = bool.Parse(array[4]);
			OptionsLink.AOE_NewEventsPerStage.Value = bool.Parse(array[5]);
			Debug.Log("Pasted " + GUIUtility.systemCopyBuffer + " into the Entropy settings!");
			OptionsLink.Config.Reload();
			OptionsLink.AOE_Seed_O.Value = array[0];
			OptionsLink.AOE_EventHookMultiplier_O.Value = array[1];
			OptionsLink.AOE_EventChanceMultiplier_O.Value = array[2];
			OptionsLink.AOE_EventChanceOffset_O.Value = float.Parse(array[3]);
			OptionsLink.AOE_SimultaneousOutcomes_O.Value = bool.Parse(array[4]);
			OptionsLink.AOE_NewEventsPerStage_O.Value = bool.Parse(array[5]);
		}

		private static void voidBase()
		{
			OptionsLink.AOE_Seed.Value = "-1";
			OptionsLink.AOE_EventHookMultiplier.Value = "1.0";
			OptionsLink.AOE_EventChanceMultiplier.Value = "1.0";
			OptionsLink.AOE_EventChanceOffset.Value = 0f;
			OptionsLink.AOE_SimultaneousOutcomes.Value = false;
			OptionsLink.AOE_NewEventsPerStage.Value = false;
			OptionsLink.AOE_OmitMessages.Value = false;
		}

		private static void warBase()
		{
			OptionsLink.AOW_BaseDropChance.Value = 5f;
			OptionsLink.AOW_DropChanceExpScaling.Value = 1.4f;
			OptionsLink.AOW_EvolutionExpScaling.Value = 2.4f;
			OptionsLink.AOW_EnemyHealthScaling.Value = 1.0f;
			OptionsLink.AOW_PlayerHealthScaling.Value = 1.0f;
			OptionsLink.AOW_EnemyDmgScaling.Value = 1.0f;
			OptionsLink.AOW_EntityCap.Value = "250";
			OptionsLink.AOW_MaxSwarm.Value = "5";
			OptionsLink.AOW_SwarmTime.Value = "5";
			OptionsLink.AOW_OmitMessages.Value = false;
		}

		private static void titanBase()
		{
			OptionsLink.AOT_CreditScalingMultiplier.Value = 0.15f;
			OptionsLink.AOT_BossGrowthPerStage.Value = 2.6f;
			OptionsLink.AOT_EliteMultiplier.Value = 0.4f;
			OptionsLink.AOT_FixedBossScaling.Value = 0f;
			OptionsLink.AOT_FixedMonsterScaling.Value = 0f;
		}

		private static void smashBase()
		{
			OptionsLink.AOS_EnemyDMGMult.Value = 1f;
			OptionsLink.AOS_PlayerDMGMult.Value = 1f;
			OptionsLink.AOS_BossDMGMult.Value = 1f;
			OptionsLink.AOS_PlayerForceCoefficient.Value = 1f;
			OptionsLink.AOS_EnemyForceCoefficient.Value = 1f;
			OptionsLink.AOS_BossForceCoefficient.Value = 1f;
		}

		private static void morphBase()
        {
			OptionsLink.AOM_AttackSpeedMultiplier.Value= "1.0";
			OptionsLink.AOM_CooldownMultiplier.Value = "1.0";
			OptionsLink.AOM_DamageMultiplier.Value = "1.0";
			OptionsLink.AOM_HealthMultiplier.Value = "1.0";
			OptionsLink.AOM_SizeMultiplier.Value = 1.0f;
			OptionsLink.AOM_SpeedMultiplier.Value = "1.0";
		}

		public static ConfigFile Config;

		public static ConfigEntry<float> AOW_BaseDropChance;
		public static ConfigEntry<float> AOW_DropChanceExpScaling;
		public static ConfigEntry<float> AOW_EvolutionExpScaling;
		public static ConfigEntry<float> AOW_EnemyDmgScaling;
		public static ConfigEntry<float> AOW_PlayerHealthScaling;
		public static ConfigEntry<float> AOW_EnemyHealthScaling;
		public static ConfigEntry<string> AOW_SwarmTime;
		public static ConfigEntry<string> AOW_MaxSwarm;
		public static ConfigEntry<string> AOW_EntityCap;
		public static ConfigEntry<bool> AOW_OmitMessages;

		// Token: 0x04000009 RID: 9
		public static ConfigEntry<string> AOE_Seed;
		public static ConfigEntry<string> AOE_EventHookMultiplier;
		public static ConfigEntry<string> AOE_EventChanceMultiplier;
		public static ConfigEntry<float> AOE_EventChanceOffset;
		public static ConfigEntry<bool> AOE_SimultaneousOutcomes;
		public static ConfigEntry<bool> AOE_NewEventsPerStage;
		public static ConfigEntry<bool> AOE_OmitMessages;

		public static ConfigEntry<bool> AOU_ShareLunarItems;
		public static ConfigEntry<bool> AOU_ShareVoidItems;
		public static ConfigEntry<bool> AOU_DeathRecovery;

		public static StringInputFieldOption AOE_Seed_O;
		public static StringInputFieldOption AOE_EventHookMultiplier_O;
		public static StringInputFieldOption AOE_EventChanceMultiplier_O;
		public static SliderOption AOE_EventChanceOffset_O;
		public static CheckBoxOption AOE_SimultaneousOutcomes_O;
		public static CheckBoxOption AOE_NewEventsPerStage_O;

		public static ConfigEntry<float> AOT_CreditScalingMultiplier;
		public static ConfigEntry<float> AOT_BossGrowthPerStage;
		public static ConfigEntry<float> AOT_EliteMultiplier;
		public static ConfigEntry<float> AOT_FixedMonsterScaling;
		public static ConfigEntry<float> AOT_FixedBossScaling;

		public static ConfigEntry<KeyboardShortcut> AOD_KeyUI;
		public static ConfigEntry<string> AOM_DamageMultiplier;
		public static StringInputFieldOption AOM_DamageMultiplier_O;
		public static ConfigEntry<string> AOM_HealthMultiplier;
		public static StringInputFieldOption AOM_HealthMultiplier_O;
		public static ConfigEntry<string> AOM_SpeedMultiplier;
		public static StringInputFieldOption AOM_SpeedMultiplier_O;
		public static ConfigEntry<string> AOM_AttackSpeedMultiplier;
		public static StringInputFieldOption AOM_AttackSpeedMultiplier_O;
		public static ConfigEntry<string> AOM_CooldownMultiplier;
		public static StringInputFieldOption AOM_CooldownMultiplier_O;
		public static ConfigEntry<float> AOM_SizeMultiplier;

		public static ConfigEntry<string> AOD_Blacklist;
		public static ConfigEntry<float> AOD_LevelMultiplier;
		public static ConfigEntry<float> AOD_ExtraLevels;
		public static ConfigEntry<bool> AOD_ForceBossSpawn;
		public static ConfigEntry<bool> AOD_OneShotProtection;
		public static ConfigEntry<bool> AOD_VisiblePlayers;
		public static ConfigEntry<bool> AOD_DeathMessages;
		public static ConfigEntry<KeyboardShortcut> AOD_KeySuicide;
		public static ConfigEntry<KeyboardShortcut> AOD_KeyChooseHuman;
		public static ConfigEntry<KeyboardShortcut> AOD_KeyChooseMonster;

		public static ConfigEntry<string> AOM_MorphSelect;

		public static ConfigEntry<float> AOS_EnemyDMGMult;
		public static ConfigEntry<float> AOS_EnemyForceCoefficient;
		public static ConfigEntry<float> AOS_BossDMGMult;
		public static ConfigEntry<float> AOS_BossForceCoefficient;
		public static ConfigEntry<float> AOS_PlayerDMGMult;
		public static ConfigEntry<float> AOS_PlayerForceCoefficient;

	}
}
