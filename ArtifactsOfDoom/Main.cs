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
        public const string ModVer = "0.0.1";

        public static AssetBundle MainAssets;

        public List<ArtifactBase> Artifacts = new List<ArtifactBase>();
        
        private void Awake()
        {
            Debug.Log("WAR: Main is running...");
            MainAssets = AssetBundle.LoadFromFile(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/artifacticons");

            /**
             * Event List
             * 3256861171 Funny Fart
            */

            Debug.Log("WAR: Loaded Assets");

            Debug.Log("Registering Net Message");
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
                Debug.Log("WAR: Found Assembly Instance = " + t.FullName);
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(t);
                artifact.Init();
            }

            foreach (var artifactType in ArtifactTypes)
            {
                Debug.Log("WAR: Creating instance = " + artifactType.AssemblyQualifiedName);
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                if (ValidateArtifact(artifact, Artifacts))
                {
                    //artifact.Init();
                }
                else
                {
                    //artifact.Init();
                }
            }

            // add all available sounds...
            using (var bankStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("csProj.Epic.bnk"))
            {
                var bytes = new byte[bankStream.Length];
                bankStream.Read(bytes, 0, bytes.Length);
                SoundBanks.Add(bytes);
            }

            // in this step we shall add UI elements
            new textBox().Start();
        }
        public bool ValidateArtifact(ArtifactBase artifact, List<ArtifactBase> artifactList)
        {
            var enabled = Config.Bind<bool>("Artifact: " + artifact.ArtifactName, "Enable Artifact?", true, "Should this artifact appear for selection?").Value;
            if (enabled)
            {
                artifactList.Add(artifact);
            }
            return enabled;
        }
    }
}