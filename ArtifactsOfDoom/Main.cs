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
using UnityEngine;

namespace ArtifactsOfDoom
{
	// Token: 0x02000005 RID: 5
	[BepInPlugin("com.Dragonov7733.ArtifactsOfDoom", "Artifacts of Doom", "1.4.4")]
	[BepInDependency("com.bepis.r2api", "4.0.11")]
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
		// Token: 0x06000016 RID: 22 RVA: 0x00002FF4 File Offset: 0x000011F4
		private void Awake()
		{
			Debug.Log("Artifacts Of Doom: Main is running...");
			Main.MainAssets = AssetBundle.LoadFromFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/artifacticons");
			Debug.Log("Artifacts Of Doom: Loaded Assets");
			Debug.Log("Artifacts of Doom: Loading L4D UI Layout...");
			Debug.Log("Artifacts Of Doom: Registering Net Messages");
			NetworkingAPI.RegisterMessageType<networkBehavior.Amogus>();
			NetworkingAPI.RegisterMessageType<networkBehavior.FattenPlayer>();
			NetworkingAPI.RegisterMessageType<networkBehavior.HookFromClient>();
			NetworkingAPI.RegisterMessageType<networkBehavior.Playsound>();
			NetworkingAPI.RegisterMessageType<networkBehavior.resizeEntity>();
			NetworkingAPI.RegisterMessageType<networkBehavior.olbiterateUI>();
			NetworkingAPI.RegisterMessageType<DefianceNetBehavior.joinDefianceTeam>();
			NetworkingAPI.RegisterMessageType<DefianceNetBehavior.updateTargetMaster>();
			NetworkingAPI.RegisterMessageType<DefianceNetBehavior.clientSyncSettings>();
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

		// Token: 0x0400002A RID: 42
		public const string ModGuid = "com.Dragonov7733.ArtifactsOfDoom";

		// Token: 0x0400002B RID: 43
		public const string ModName = "Artifacts of Doom";

		// Token: 0x0400002C RID: 44
		public const string ModVer = "1.4.4";

		// Token: 0x0400002D RID: 45
		public static AssetBundle MainAssets;

		// Token: 0x0400002E RID: 46
		public List<ArtifactBase> Artifacts = new List<ArtifactBase>();
	}
}
