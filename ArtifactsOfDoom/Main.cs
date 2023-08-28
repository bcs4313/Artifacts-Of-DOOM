using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ArtifactGroup;
using BepInEx;
using R2API;
using R2API.Networking;
using R2API.Utils;
using RoR2.ExpansionManagement;
using UnityEngine;

namespace ArtifactsOfDoom
{
	[BepInPlugin("com.Dragonov7733.ArtifactsOfDoom", "Artifacts of Doom", "1.6.3")]
	[BepInDependency("com.bepis.r2api", "5.0.5")]
	[BepInDependency("com.rune580.riskofoptions")]
	[R2APISubmoduleDependency(new string[]
	{
		"NetworkingAPI",
		"ItemAPI",
		"LanguageAPI"
	})]
	[R2APISubmoduleDependency(new string[]
	{
		"SoundAPI"
	})]

	public class Main : BaseUnityPlugin
	{
		public const string PluginGUID = "com.Dragonov7733.ArtifactsOfDoom";

		public const string PluginAuthor = "bcs4313";

		public const string PluginName = "Artifacts of Doom";

		public const string PluginVersion = "1.6.3";



		public static AssetBundle MorphAssets;
		private void Awake()
		{
			Debug.Log("Artifacts Of Doom: Main is running...");
			Type[] types = Assembly.GetExecutingAssembly().GetExportedTypes();

			Main.MainAssets = AssetBundle.LoadFromFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/artifacticons");
			Main.MorphAssets = AssetBundle.LoadFromFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/transformui");
			Debug.Log("Artifacts Of Doom: Loaded Assets");
			Debug.Log("Artifacts of Doom: Loading L4D UI Layout...");
			Debug.Log("Artifacts Of Doom: Registering Net Messages");
			NetworkingAPI.RegisterMessageType<NetworkBehavior.Amogus>();
			NetworkingAPI.RegisterMessageType<NetworkBehavior.FattenPlayer>();
			NetworkingAPI.RegisterMessageType<NetworkBehavior.HookFromClient>();
			NetworkingAPI.RegisterMessageType<NetworkBehavior.Playsound>();
			NetworkingAPI.RegisterMessageType<NetworkBehavior.resizeEntity>();
			NetworkingAPI.RegisterMessageType<NetworkBehavior.resizeEntityRebound>();
			NetworkingAPI.RegisterMessageType<NetworkBehavior.olbiterateUI>();
			NetworkingAPI.RegisterMessageType<NetworkBehavior.TransformRequest>();
			NetworkingAPI.RegisterMessageType<NetworkBehavior.statRequest>();
			NetworkingAPI.RegisterMessageType<NetworkBehavior.statRequestRebound>();
			NetworkingAPI.RegisterMessageType<NetworkBehavior.informWarSettings>();
			//NetworkingAPI.RegisterMessageType<DefianceNetBehavior.joinDefianceTeam>();
			//NetworkingAPI.RegisterMessageType<DefianceNetBehavior.updateTargetMaster>();
			//NetworkingAPI.RegisterMessageType<DefianceNetBehavior.clientSyncSettings>();
			IEnumerable<Type> enumerable = from type in Assembly.GetExecutingAssembly().GetTypes()
										   where !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase))
										   select type;
			foreach (Type type3 in enumerable)
			{
				Debug.Log("Artifacts Of Doom: Found Assembly Instance = " + type3.FullName);
				ArtifactBase artifactBase = (ArtifactBase)Activator.CreateInstance(type3);
				artifactBase.Init();
			}
			foreach (Type type2 in enumerable)
			{
				Debug.Log("Artifacts Of Doom: Creating instance = " + type2.AssemblyQualifiedName);
				ArtifactBase item = (ArtifactBase)Activator.CreateInstance(type2);
				this.Artifacts.Add(item);
			}
			using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("csProj.Epic.bnk"))
			{
				byte[] array = new byte[manifestResourceStream.Length];
				manifestResourceStream.Read(array, 0, array.Length);
				SoundAPI.SoundBanks.Add(array);
			}
			OptionsLink.Config = base.Config;
			OptionsLink.constructSettings();
		}

		// this section is strictly to increase compatibility with other mods
		public const string ModGuid = "com.Dragonov7733.ArtifactsOfDoom";
		public const string ModName = "Artifacts of Doom";
		public const string ModAutho = "bcs4313";
		public const string GUID = "com.Dragonov7733.ArtifactsOfDoom";
		public const string Name = "Artifacts of Doom";
		public const string ModVer = "1.6.3";

		// Token: 0x0400002D RID: 45
		public static AssetBundle MainAssets;

		// Token: 0x0400002E RID: 46
		public List<ArtifactBase> Artifacts = new List<ArtifactBase>();
	}
}
