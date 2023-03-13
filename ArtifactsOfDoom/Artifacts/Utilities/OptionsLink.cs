using System;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using UnityEngine;
using UnityEngine.Events;

namespace ArtifactsOfDoom
{
	// Token: 0x02000003 RID: 3
	internal static class OptionsLink
	{
		// Token: 0x06000008 RID: 8 RVA: 0x000020B0 File Offset: 0x000002B0
		public static void constructSettings()
		{
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
			ModSettingsManager.AddOption(new GenericButtonOption("Return to Default Settings", "Artifact of War", "Return to the base settings of this artifact.", "Default Settings", new UnityAction(OptionsLink.warBase)));
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
			OptionsLink.AOE_SimultaneousOutcomes = OptionsLink.Config.Bind<bool>("Artifact of Entropy", "Simultaneous Outcomes", false, "All Events under the same action will occur at the same time. Default: False");
			OptionsLink.AOE_SimultaneousOutcomes_O = new CheckBoxOption(OptionsLink.AOE_SimultaneousOutcomes);
			ModSettingsManager.AddOption(OptionsLink.AOE_SimultaneousOutcomes_O);
			OptionsLink.AOE_NewEventsPerStage = OptionsLink.Config.Bind<bool>("Artifact of Entropy", "New Events Per Stage", false, "A new set of outcomes will be generated and linked to actions each time a stage is completed. Default: False");
			OptionsLink.AOE_NewEventsPerStage_O = new CheckBoxOption(OptionsLink.AOE_NewEventsPerStage);
			ModSettingsManager.AddOption(OptionsLink.AOE_NewEventsPerStage_O);
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
			OptionsLink.AOS_EnemyDMGMult = OptionsLink.Config.Bind<float>("Artifact of Smash", "Enemy Impact Dmg Mult.", 1f, "Multiplier for damage done to enemies via impact. \nRange: (0, 5) \nDefault: 1");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOS_EnemyDMGMult, new SliderConfig
			{
				min = 0f,
				max = 5f,
				formatString = "{0:F2}"
			}));
			OptionsLink.AOS_PlayerDMGMult = OptionsLink.Config.Bind<float>("Artifact of Smash", "Player Impact Dmg Mult.", 1f, "Multiplier for damage done to survivors via impact. \nRange: (0, 5) \nDefault: 1");
			ModSettingsManager.AddOption(new SliderOption(OptionsLink.AOS_PlayerDMGMult, new SliderConfig
			{
				min = 0f,
				max = 5f,
				formatString = "{0:F2}"
			}));
			ModSettingsManager.AddOption(new GenericButtonOption("Return to Default Settings", "Artifact of Smash", "Return to the base settings of this artifact.", "Default Settings", new UnityAction(OptionsLink.smashBase)));
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000029B0 File Offset: 0x00000BB0
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

		// Token: 0x0600000A RID: 10 RVA: 0x00002A7C File Offset: 0x00000C7C
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

		// Token: 0x0600000B RID: 11 RVA: 0x00002B90 File Offset: 0x00000D90
		private static void voidBase()
		{
			OptionsLink.AOE_Seed.Value = "-1";
			OptionsLink.AOE_EventHookMultiplier.Value = "1.0";
			OptionsLink.AOE_EventChanceMultiplier.Value = "1.0";
			OptionsLink.AOE_EventChanceOffset.Value = 0f;
			OptionsLink.AOE_SimultaneousOutcomes.Value = false;
			OptionsLink.AOE_NewEventsPerStage.Value = false;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002BF8 File Offset: 0x00000DF8
		private static void warBase()
		{
			OptionsLink.AOW_BaseDropChance.Value = 5f;
			OptionsLink.AOW_DropChanceExpScaling.Value = 1.4f;
			OptionsLink.AOW_EvolutionExpScaling.Value = 2.4f;
			OptionsLink.AOW_EntityCap.Value = "250";
			OptionsLink.AOW_MaxSwarm.Value = "5";
			OptionsLink.AOW_SwarmTime.Value = "5";
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002C68 File Offset: 0x00000E68
		private static void titanBase()
		{
			OptionsLink.AOT_CreditScalingMultiplier.Value = 0.15f;
			OptionsLink.AOT_BossGrowthPerStage.Value = 2.6f;
			OptionsLink.AOT_EliteMultiplier.Value = 0.4f;
			OptionsLink.AOT_FixedBossScaling.Value = 0f;
			OptionsLink.AOT_FixedMonsterScaling.Value = 0f;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002CC6 File Offset: 0x00000EC6
		private static void smashBase()
		{
			OptionsLink.AOS_EnemyDMGMult.Value = 1f;
			OptionsLink.AOS_PlayerDMGMult.Value = 1f;
		}

		// Token: 0x04000002 RID: 2
		public static ConfigFile Config;

		// Token: 0x04000003 RID: 3
		public static ConfigEntry<float> AOW_BaseDropChance;

		// Token: 0x04000004 RID: 4
		public static ConfigEntry<float> AOW_DropChanceExpScaling;

		// Token: 0x04000005 RID: 5
		public static ConfigEntry<float> AOW_EvolutionExpScaling;

		// Token: 0x04000006 RID: 6
		public static ConfigEntry<string> AOW_SwarmTime;

		// Token: 0x04000007 RID: 7
		public static ConfigEntry<string> AOW_MaxSwarm;

		// Token: 0x04000008 RID: 8
		public static ConfigEntry<string> AOW_EntityCap;

		// Token: 0x04000009 RID: 9
		public static ConfigEntry<string> AOE_Seed;

		// Token: 0x0400000A RID: 10
		public static ConfigEntry<string> AOE_EventHookMultiplier;

		// Token: 0x0400000B RID: 11
		public static ConfigEntry<string> AOE_EventChanceMultiplier;

		// Token: 0x0400000C RID: 12
		public static ConfigEntry<float> AOE_EventChanceOffset;

		// Token: 0x0400000D RID: 13
		public static ConfigEntry<bool> AOE_SimultaneousOutcomes;

		// Token: 0x0400000E RID: 14
		public static ConfigEntry<bool> AOE_NewEventsPerStage;

		// Token: 0x0400000F RID: 15
		public static ConfigEntry<bool> AOU_ShareVoidItems;

		// Token: 0x04000010 RID: 16
		public static ConfigEntry<bool> AOU_ShareLunarItems;

		// Token: 0x04000011 RID: 17
		public static StringInputFieldOption AOE_Seed_O;

		// Token: 0x04000012 RID: 18
		public static StringInputFieldOption AOE_EventHookMultiplier_O;

		// Token: 0x04000013 RID: 19
		public static StringInputFieldOption AOE_EventChanceMultiplier_O;

		// Token: 0x04000014 RID: 20
		public static SliderOption AOE_EventChanceOffset_O;

		// Token: 0x04000015 RID: 21
		public static CheckBoxOption AOE_SimultaneousOutcomes_O;

		// Token: 0x04000016 RID: 22
		public static CheckBoxOption AOE_NewEventsPerStage_O;

		// Token: 0x04000017 RID: 23
		public static ConfigEntry<float> AOT_CreditScalingMultiplier;

		// Token: 0x04000018 RID: 24
		public static ConfigEntry<float> AOT_BossGrowthPerStage;

		// Token: 0x04000019 RID: 25
		public static ConfigEntry<float> AOT_EliteMultiplier;

		// Token: 0x0400001A RID: 26
		public static ConfigEntry<float> AOT_FixedMonsterScaling;

		// Token: 0x0400001B RID: 27
		public static ConfigEntry<float> AOT_FixedBossScaling;

		// Token: 0x0400001C RID: 28
		public static ConfigEntry<string> AOD_Blacklist;

		// Token: 0x0400001D RID: 29
		public static ConfigEntry<float> AOD_LevelMultiplier;

		// Token: 0x0400001E RID: 30
		public static ConfigEntry<float> AOD_ExtraLevels;

		// Token: 0x0400001F RID: 31
		public static ConfigEntry<bool> AOD_ForceBossSpawn;

		// Token: 0x04000020 RID: 32
		public static ConfigEntry<bool> AOD_OneShotProtection;

		// Token: 0x04000021 RID: 33
		public static ConfigEntry<bool> AOD_VisiblePlayers;

		// Token: 0x04000022 RID: 34
		public static ConfigEntry<bool> AOD_DeathMessages;

		// Token: 0x04000023 RID: 35
		public static ConfigEntry<KeyboardShortcut> AOD_KeySuicide;

		// Token: 0x04000024 RID: 36
		public static ConfigEntry<KeyboardShortcut> AOD_KeyChooseHuman;

		// Token: 0x04000025 RID: 37
		public static ConfigEntry<KeyboardShortcut> AOD_KeyChooseMonster;

		// Token: 0x04000026 RID: 38
		public static ConfigEntry<float> AOS_EnemyDMGMult;

		// Token: 0x04000027 RID: 39
		public static ConfigEntry<float> AOS_PlayerDMGMult;
	}
}
