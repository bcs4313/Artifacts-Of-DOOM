using BepInEx;
using ArtifactGroup;
using R2API;
using R2API.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using R2API.Networking;
using static R2API.SoundAPI;
using RiskOfOptions;
using RiskOfOptions.Options;

namespace ArtifactsOfDoom
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(NetworkingAPI), nameof(ItemAPI), nameof(LanguageAPI))]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [R2APISubmoduleDependency(nameof(SoundAPI))]

    public class Main : BaseUnityPlugin
    {

        public const string ModGuid = "com.Dragonov7733.ArtifactsOfDoom";
        public const string ModName = "Artifacts of Doom";
        public const string ModVer = "1.4.4";

        public static AssetBundle MainAssets;

        public List<ArtifactBase> Artifacts = new List<ArtifactBase>();
        
        private void Awake()
        {
            Debug.Log("Artifacts Of Doom: Main is running...");
            MainAssets = AssetBundle.LoadFromFile(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/artifacticons");

            Debug.Log("Artifacts Of Doom: Loaded Assets");

            Debug.Log("Artifacts Of Doom: Registering Net Message");
            // Register all net messages from the networkBehavior class
            NetworkingAPI.RegisterMessageType<networkBehavior.Amogus>();
            NetworkingAPI.RegisterMessageType<networkBehavior.FattenPlayer>();
            NetworkingAPI.RegisterMessageType<networkBehavior.HookFromClient>();
            NetworkingAPI.RegisterMessageType<networkBehavior.Playsound>();
            NetworkingAPI.RegisterMessageType<networkBehavior.resizeEntity>();
            NetworkingAPI.RegisterMessageType<networkBehavior.olbiterateUI>();

            //This section automatically scans the project for all artifacts
            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));
            
            foreach(Type t in ArtifactTypes)
            {
                Debug.Log("Artifacts Of Doom: Found Assembly Instance = " + t.FullName);
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(t);
                artifact.Init();
            }

            foreach (var artifactType in ArtifactTypes)
            {
                Debug.Log("Artifacts Of Doom: Creating instance = " + artifactType.AssemblyQualifiedName);
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                Artifacts.Add(artifact);
            }

            // add all available sounds...
            using (var bankStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("csProj.Epic.bnk"))
            {
                var bytes = new byte[bankStream.Length];
                bankStream.Read(bytes, 0, bytes.Length);
                SoundBanks.Add(bytes);
            }

            // bind all settings for Risk of Options
            OptionsLink.Config = Config;
            OptionsLink.constructSettings();
        }
      
    }
}