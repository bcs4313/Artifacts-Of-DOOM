
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Messenger;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using R2API;
using static On.RoR2.CharacterMaster;
using UnityEngine.Networking;
using System.Threading.Tasks;
using csProj.Artifacts.MorphSystem;
using R2API.Networking;
using ArtifactsOfDoom;
using System.Threading;

namespace ArtifactGroup
{
    [BepInDependency("com.bepis.r2api")]
    public class NetworkBehavior : BaseUnityPlugin // class handles all network behavior for now. 
    { // may be decomposed later

        public class informWarSettings : INetMessage
        {
            public double enemyDmgScaling = 2.0;
            public double playerHealthScaling = 2.0;
            public double enemyHealthScaling = 2.0;

            public void Deserialize(NetworkReader reader)
            { // must be read in serialization order, like a scanner
                enemyDmgScaling = reader.ReadDouble();
                playerHealthScaling = reader.ReadDouble();
                enemyHealthScaling = reader.ReadDouble();
            }

            public informWarSettings()
            {
            }

            public informWarSettings(float _enemyDmgScaling, float _playerHealthScaling, float _enemyHealthScaling)
            {
                enemyDmgScaling = _enemyDmgScaling;
                playerHealthScaling = _playerHealthScaling;
                enemyHealthScaling = _enemyHealthScaling;
            }

            public void OnReceived()
            {
                if (NetworkServer.active)
                {
                    return;
                }

                // adjust clientside parameters to match the server
                ArtifactOfWar.enemyDmgScaling = (float)enemyDmgScaling;
                ArtifactOfWar.playerHealthScaling = (float)playerHealthScaling;
                ArtifactOfWar.enemyHealthScaling = (float)enemyHealthScaling;
                Debug.Log("Adjusted War Parameters to match the host");
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(enemyDmgScaling);
                writer.Write(playerHealthScaling);
                writer.Write(enemyHealthScaling);
            }
        }


        public class informMaxHealth : INetMessage
        {
            public double newMax; // the host report of the player max health
            public uint targetId; // network id of the player

            public void Deserialize(NetworkReader reader)
            { // must be read in serialization order, like a scanner
                newMax = reader.ReadDouble();
                targetId = reader.ReadUInt32(); // default Unsigned Integer Size
            }

            public informMaxHealth()
            {
            }

            public informMaxHealth(uint _targetId, double _newMax)
            {
                targetId = _targetId;
                newMax = _newMax;
            }

            public void OnReceived()
            {
                if (NetworkServer.active)
                {
                    return;
                }

                // first we must find the monster body in the instances list for this client. Then we resize em.
                var allBodies = TeamComponent.GetTeamMembers(TeamIndex.Player).Select((x) => x.body);
                if (allBodies == null)
                {
                    Debug.Log("Master is null... aborting");
                    return;
                }

                foreach (CharacterBody c in allBodies)
                {
                    if (c.networkIdentity.netId.Value == targetId)
                    {
                        float oldMax = c.maxHealth;
                        c.baseMaxHealth = (float)newMax;
                        Debug.Log("HealthInform Packet::: " + oldMax + " => " + newMax);
                    }
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(newMax);
                writer.Write(targetId);
            }
        }

        public class FattenPlayer : INetMessage
        {
            public double xscalar; // the host report of the player approximate size
            public double yscalar; // the host report of the player approximate size
            public double zscalar; // the host report of the player approximate size
            public uint targetId; // network id of the player

            public void Deserialize(NetworkReader reader)
            { // must be read in serialization order, like a scanner
                xscalar = reader.ReadDouble();
                yscalar = reader.ReadDouble();
                zscalar = reader.ReadDouble();
                targetId = reader.ReadUInt32(); // default Unsigned Integer Size
            }

            public FattenPlayer()
            {
            }

            public FattenPlayer(uint _targetId, double _xscalar, double _yscalar, double _zscalar)
            {
                targetId = _targetId;
                xscalar = _xscalar;
                yscalar = _yscalar;
                zscalar = _zscalar;
            }

            public void OnReceived()
            {
                if (NetworkServer.active)
                {
                    return;
                }

                // first we must find the monster body in the instances list for this client. Then we resize em.
                var allBodies = TeamComponent.GetTeamMembers(TeamIndex.Player).Select((x) => x.body);
                if (allBodies == null)
                {
                    Debug.Log("Master is null... aborting");
                    return;
                }

                CharacterMaster master = null;
                foreach (CharacterBody c in allBodies)
                {
                    if (c.networkIdentity.netId.Value == targetId)
                    {
                        //Debug.Log("Found monster using universalID. hooking...");
                        master = c.master;
                        ModelLocator modelLocator = c.GetComponent<ModelLocator>();
                        if (modelLocator)
                        {
                            Vector3 size = modelLocator.modelTransform.localScale;
                            size.x = (float)xscalar;
                            size.y = (float)yscalar;
                            size.z = (float)zscalar;

                            Transform modelTransform = modelLocator.modelBaseTransform;
                            //Mesh m = c.gameObject.GetComponent<MeshFilter>().sharedMesh;
                            if (modelTransform)
                            {
                                modelLocator.modelTransform.localScale = size;
                            }
                            else
                            {
                                MessageHandler.globalMessage("??");
                            }
                        }
                        else
                        {
                            MessageHandler.globalMessage("?");
                        }
                    }
                }
                //MessageHandler.globalMessage("Resize Complete");
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(xscalar);
                writer.Write(yscalar);
                writer.Write(zscalar);
                writer.Write(targetId);
            }
        }

        public class statRequestRebound : INetMessage
        {

            public uint targetId; // network id of the player
            public String sender;

            // stat modifications for request
            public double speedMult;
            public double healthMult;
            public double attackSpeedMult;
            public double dmgMult;
            public double size;
            public double cooldownMult;
            public String eliteString;

            public void Deserialize(NetworkReader reader)
            { // must be read in serialization order, like a scanner
                targetId = reader.ReadUInt32();
                speedMult = reader.ReadDouble();
                healthMult = reader.ReadDouble();
                attackSpeedMult = reader.ReadDouble();
                dmgMult = reader.ReadDouble();
                size = reader.ReadDouble();
                cooldownMult = reader.ReadDouble();
                eliteString = reader.ReadString();
                sender = reader.ReadString();
            }

            public statRequestRebound()
            {
                //new NetworkBehavior.statRequest(targetId, speedMult, healthMult, attackSpeedMult, dmgMult, size, cooldownMult, eliteString, LocalUserManager.GetFirstLocalUser().userProfile.name).Send(NetworkDestination.Clients);
            }

            public statRequestRebound(uint _targetId, double _speedMult, double _healthMult, double _attackSpeedMult, double _dmgMult, double _size, double _cooldownMult, String _eliteString, String _sender)
            {
                targetId = _targetId;
                speedMult = _speedMult;
                healthMult = _healthMult;
                attackSpeedMult = _attackSpeedMult;
                dmgMult = _dmgMult;
                size = _size;
                sender = _sender;
                cooldownMult = _cooldownMult;
                eliteString = _eliteString;
            }

            // now just get the player and respawn them!
            public async void OnReceived()
            {
                if (!NetworkServer.active)
                {
                    Debug.Log("Denied a rebound request (isClient)");
                    return;
                }
                Debug.Log("Rebounding stat Request from " + sender);

                StatController.injectStatPackage(targetId, speedMult, healthMult, attackSpeedMult, dmgMult, size, cooldownMult, eliteString);
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(targetId);
                writer.Write(speedMult);
                writer.Write(healthMult);
                writer.Write(attackSpeedMult);
                writer.Write(dmgMult);
                writer.Write(size);
                writer.Write(cooldownMult);
                writer.Write(eliteString);
                writer.Write(sender);
            }
        }

        // package to send to server that tells the server
        // what person's stats to change, and what the stats are.

        // package to send to server that tells the server
        // what person's stats to change, and what the stats are.
        public class statRequest : INetMessage
        {

            public uint targetId; // network id of the player
            public String sender;

            // stat modifications for request
            public double speedMult;
            public double healthMult;
            public double attackSpeedMult;
            public double dmgMult;
            public double size;
            public double cooldownMult;
            public String eliteString;

            public void Deserialize(NetworkReader reader)
            { // must be read in serialization order, like a scanner
                targetId = reader.ReadUInt32();
                speedMult = reader.ReadDouble();
                healthMult = reader.ReadDouble();
                attackSpeedMult = reader.ReadDouble();
                dmgMult = reader.ReadDouble();
                size = reader.ReadDouble();
                cooldownMult = reader.ReadDouble();
                eliteString = reader.ReadString();
                sender = reader.ReadString();
            }

            public statRequest()
            {

            }

            public statRequest(uint _targetId, double _speedMult, double _healthMult, double _attackSpeedMult, double _dmgMult, double _size, double _cooldownMult, String _eliteString, String _sender)
            {
                targetId = _targetId;
                speedMult = _speedMult;
                healthMult = _healthMult;
                attackSpeedMult = _attackSpeedMult;
                dmgMult = _dmgMult;
                size = _size;
                sender = _sender;
                cooldownMult = _cooldownMult;
                eliteString = _eliteString;
            }

            // now just get the player and respawn them!
            public async void OnReceived()
            {
                //Debug.Log("(statRequest) Received request to change stats (Artifact of Metamorphosis). Sender: " + sender);

                // find network user and transform them (threaded loop)
                for (int i = 0; i < RoR2.NetworkUser.readOnlyInstancesList.Count; i++)
                {
                    try
                    {
                        Debug.Log("Starting Thread :: " + i);
                        var user2 = RoR2.NetworkUser.readOnlyInstancesList[i];
                        Thread thread = new Thread(() => statModifThread(targetId, speedMult, healthMult, attackSpeedMult, dmgMult, size, cooldownMult, eliteString, sender, user2));
                        thread.Start();
                        //statModifThread(targetId, speedMult, healthMult, attackSpeedMult, dmgMult, size, cooldownMult, eliteString, sender, user2);

                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Can't start statRequest thread! " + e.Message.ToString());
                    }
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(targetId);
                writer.Write(speedMult);
                writer.Write(healthMult);
                writer.Write(attackSpeedMult);
                writer.Write(dmgMult);
                writer.Write(size);
                writer.Write(cooldownMult);
                writer.Write(eliteString);
                writer.Write(sender);
            }
        }

        static async void statModifThread(uint targetId, double speedMult, double healthMult, double attackSpeedMult, double dmgMult, double size, double cooldownMult, String eliteString, String sender, NetworkUser user2)
        {
            //Debug.Log("(statRequest) loop :: ");
            // if the body is null or if the body hasn't completed its transformation yet (ASYNC).
            if (user2.GetCurrentBody() == null || ArtifactOfMorph.clientBodies.Contains(user2.GetCurrentBody()))
            {
                //Debug.Log("(statRequest) async attempt -- targetId -> " + targetId);
                if (user2.GetCurrentBody() == null)
                {
                    //Debug.Log("(statRequest) No master/body on networkUser in request... Awaiting for body.");
                }
                else
                {
                    //Debug.Log("(statRequest) Client is STILL OLD MORPH. Awaiting for client to become a different monster.");
                }
                CharacterBody awaitingBody = null;
                int timeout = 50;
                while ((awaitingBody == null || ArtifactOfMorph.clientBodies.Contains(awaitingBody)) && timeout >= 0)
                {
                   // Debug.Log("wait. ");
                    await Task.Delay(100);
                    awaitingBody = user2.GetCurrentBody();
                    timeout--;
                }

                if (awaitingBody != null && !ArtifactOfMorph.clientBodies.Contains(awaitingBody))
                {
                    //Debug.Log("(statRequest) STAT: Body Found! SUCCESS. " + " = " + awaitingBody);
                    //Debug.Log("(statRequest) targetId -> " + targetId + " netId -> " + awaitingBody.master.networkIdentity.netId.Value + " body: " + awaitingBody);
                    if (targetId == awaitingBody.master.networkIdentity.netId.Value)
                    {
                        //Debug.Log("(statRequest) ASYNC STAT --> Body target = " + awaitingBody.GetUserName() + " Sender: " + sender);
                        //Debug.Log("(statRequest) attackSpeedMult: " + attackSpeedMult);
                        //Debug.Log("(statRequest) speedMult: " + speedMult);
                        //Debug.Log("(statRequest) dmgMult: " + dmgMult);
                        //Debug.Log("(statRequest) healthMult: " + healthMult);
                        //Debug.Log("(statRequest) cooldownMult: " + cooldownMult);
                        //Debug.Log("(statRequest) size:" + size);

                        //Debug.Log("(statRequest) Adding awaitingBody to clientBodies " + awaitingBody.name);
                        ArtifactOfMorph.clientBodies.Add(awaitingBody);
                        //Debug.Log("(statRequest) added " + awaitingBody.name);
                        ArtifactOfMorph.sizeMultSync = (float)size;
                        ArtifactOfMorph.damageMultSync = (float)dmgMult;
                        ArtifactOfMorph.healthMultSync = (float)healthMult;
                        ArtifactOfMorph.speedMultSync = (float)speedMult;
                        ArtifactOfMorph.attackSpeedMultSync = (float)attackSpeedMult;
                        ArtifactOfMorph.cooldownMultSync = (float)cooldownMult;
                        ArtifactOfMorph.bodyTriggered = false;
                        ArtifactOfMorph.eliteStringSync = eliteString;

                        //Debug.Log("(statRequest) updating stats... " + awaitingBody.name);
                        ArtifactOfMorph.updateStats(awaitingBody, true);
                        return;
                    }
                }

                // failure case
                if (awaitingBody == null)
                {
                    //Debug.LogError("(statRequest) SYNC --> Failed to find body/user target in request! (Artifact of Metamorphosis)." + " Sender: " + sender);
                }
                else
                {
                    //Debug.LogError("(statRequest) SYNC --> Failed to detect change from original body!" + " Sender: " + sender + " original body: " + awaitingBody);
                }

            }
            else
            {
                CharacterBody syncBody = user2.GetCurrentBody();
                //Debug.Log("(statRequest) sync attempt -- targetId -> " + targetId + " netId -> " + syncBody.master.networkIdentity.netId.Value + " body: " + syncBody);
                if (targetId == syncBody.master.networkIdentity.netId.Value)
                {
                    //Debug.Log("(statRequest) SYNC STAT --> Body target = " + syncBody.GetUserName() + " Sender: " + sender);
                    //Debug.Log("(statRequest) attackSpeedMult: " + attackSpeedMult);
                    //Debug.Log("(statRequest) speedMult: " + speedMult);
                    //Debug.Log("(statRequest) dmgMult: " + dmgMult);
                    //Debug.Log("(statRequest) healthMult: " + healthMult);
                    //Debug.Log("(statRequest) cooldownMult: " + cooldownMult);
                    //Debug.Log("(statRequest) size:" + size);

                    //Debug.Log("(statRequest) Adding syncBody to clientBodies" + syncBody.name);
                    ArtifactOfMorph.clientBodies.Add(syncBody);
                    //Debug.Log("(statRequest) added " + syncBody.name);
                    ArtifactOfMorph.sizeMultSync = (float)size;
                    ArtifactOfMorph.damageMultSync = (float)dmgMult;
                    ArtifactOfMorph.healthMultSync = (float)healthMult;
                    ArtifactOfMorph.speedMultSync = (float)speedMult;
                    ArtifactOfMorph.attackSpeedMultSync = (float)attackSpeedMult;
                    ArtifactOfMorph.cooldownMultSync = (float)cooldownMult;
                    ArtifactOfMorph.bodyTriggered = false;
                    ArtifactOfMorph.eliteStringSync = eliteString;

                    //Debug.Log("(statRequest) updating stats... " + syncBody.name);
                    ArtifactOfMorph.updateStats(syncBody, true);
                    return;
                }
            }
        }

        // set elite traits
        public static void setElite(String eliteString, CharacterBody body)
        {
            if (NetworkServer.active)
            {
                Debug.Log("SETELITE");
                if (eliteString.Contains("Fire"))
                {
                    Debug.Log("(Artifact of Reconstruction) Buffed Body with Fire Elite Buff");
                    body.AddBuff(RoR2Content.Buffs.AffixRed);
                    body.inventory.GiveEquipmentString(RoR2Content.Equipment.AffixRed.name);
                }
                if (eliteString.Contains("Ice"))
                {
                    Debug.Log("(Artifact of Reconstruction) Buffed Body with Ice Elite Buff");
                    body.AddBuff(RoR2Content.Buffs.AffixWhite);
                    body.inventory.GiveEquipmentString(RoR2Content.Equipment.AffixWhite.name);
                }
                if (eliteString.Contains("Lightning"))
                {
                    Debug.Log("(Artifact of Reconstruction) Buffed Body with Lightning Elite Buff");
                    body.AddBuff(RoR2Content.Buffs.AffixBlue);
                    body.inventory.GiveEquipmentString(RoR2Content.Equipment.AffixBlue.name);
                }
                if (eliteString.Contains("Celestine"))
                {
                    Debug.Log("(Artifact of Reconstruction) Buffed Body with Celestine Elite Buff");
                    body.AddBuff(RoR2Content.Buffs.AffixHaunted);
                    body.inventory.GiveEquipmentString(RoR2Content.Equipment.AffixHaunted.name);
                }
                if (eliteString.Contains("Mending"))
                {
                    Debug.Log("(Artifact of Reconstruction) Buffed Body with Mending Elite Buff");
                    body.AddBuff(DLC1Content.Buffs.EliteEarth);
                    //body.equipmentSlot.equipmentIndex = DLC1Content.Equipment.AffixEarth.equipmentIndex;
                }
                if (eliteString.Contains("Perfected"))
                {
                    Debug.Log("(Artifact of Reconstruction) Buffed Body with Perfected Elite Buff");
                    body.AddBuff(RoR2Content.Buffs.AffixLunar);
                    body.inventory.GiveEquipmentString(RoR2Content.Equipment.AffixLunar.name);
                }
                if (eliteString.Contains("Malachite"))
                {
                    Debug.Log("(Artifact of Reconstruction) Buffed Body with Malachite Elite Buff");
                    body.AddBuff(RoR2Content.Buffs.AffixPoison);
                    body.inventory.GiveEquipmentString(RoR2Content.Equipment.AffixPoison.name);
                }
                if (eliteString.Contains("Void"))
                {
                    Debug.Log("(Artifact of Reconstruction) Buffed Body with Void Elite Buff");
                    body.AddBuff(DLC1Content.Buffs.EliteVoid);
                    body.inventory.GiveEquipmentString(DLC1Content.Equipment.EliteVoidEquipment.name);
                }
            }
        }

        // package to send to server that tells the server
        // what person to transform and what monster they chose.
        public class TransformRequest : INetMessage
        {

            public uint targetId; // network id of the player
            public String targetMonster; // string value of the requested monster

            public void Deserialize(NetworkReader reader)
            { // must be read in serialization order, like a scanner
                targetId = reader.ReadUInt32();
                targetMonster = reader.ReadString();
            }

            public TransformRequest()
            {

            }

            public TransformRequest(uint _targetId, String _targetMonster)
            {
                targetId = _targetId;
                targetMonster = _targetMonster;
            }

            // now just get the player and respawn them!
            public async void OnReceived()
            {
                Debug.Log("Received request to change body (Artifact of Metamorphosis)");
                // a server must receive this message
                if (!NetworkServer.active)
                {
                    Debug.Log("Request denied (isClient) (Artifact of Metamorphosis)");
                    return;
                }
                else
                {
                    Debug.Log("1");
                    // get body
                    NetworkUser user = null;
                    Debug.Log("2");
                    // find network user
                    for (int i = 0; i < RoR2.NetworkUser.readOnlyInstancesList.Count; i++)
                    {
                        Debug.Log("3");
                        var user2 = RoR2.NetworkUser.readOnlyInstancesList[i];
                        if(user2.GetCurrentBody() == null)
                        {
                            Debug.Log("No master/body on networkUser in request... Awaiting for body.");
                            CharacterBody awaitingBody = null;
                            int timeout = 50;
                            while (awaitingBody == null && timeout >= 0)
                            {
                                Debug.Log("wait. ");
                                await Task.Delay(100);
                                awaitingBody = user2.GetCurrentBody();
                                timeout--;
                            }

                            if (awaitingBody != null)
                            {
                                Debug.Log("STAT: Body Found! SUCCESS. ");
                                Debug.Log("targetId -> " + targetId + " netId -> " + user2.netId.Value);
                                if (targetId == user2.netId.Value)
                                {
                                    Debug.Log("ASYNC Transform--> Body target = " + awaitingBody.GetUserName());
                                    MorphController.transformBody(user, awaitingBody, targetMonster);
                                    return;
                                }
                            }
                            Debug.LogError("SYNC --> Failed to find body/user target in request! (Artifact of Metamorphosis).");
                        }
                        else if (user2.GetCurrentBody().netId.Value == targetId) // if the body is our target, with the applied net user
                        {
                            //Debug.Log("4");
                            user = user2;
                        }
                    }
                    //Debug.Log("5");
                    if (user != null && user.GetCurrentBody() != null)
                    {
                        Debug.Log("Body target = " + user.GetCurrentBody().GetUserName());
                        MorphController.transformBody(user, user.GetCurrentBody(), targetMonster);
                    }
                    else
                    {
                        Debug.LogError("SYNC -> Failed to find body/user target in request! (Artifact of Metamorphosis)");
                    }
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(targetId);
                writer.Write(targetMonster);
            }
        }

        // simple package to trigger clients the AMOGUS pop up
        public class Amogus : INetMessage
        {
            public void Deserialize(NetworkReader reader)
            { // must be read in serialization order, like a scanner
            }

            public Amogus()
            {

            }

            public void OnReceived()
            {
                ArtifactOfEntropy.adjustAmogus();
            }

            public void Serialize(NetworkWriter writer)
            {
            }
        }

        public class Playsound : INetMessage
        {
            uint soundID; // id of sound from wwise application
            uint universalID; // id of entity to play sound from

            public void Deserialize(NetworkReader reader)
            { // must be read in serialization order, like a scanner
                soundID = reader.ReadUInt32();
                universalID = reader.ReadUInt32();
            }

            public Playsound()
            {
                
            }

            public Playsound(uint _soundID, uint _universalID)
            {
                soundID = _soundID;
                universalID = _universalID;
            }

            public void OnReceived()
            {
                // first we must find the monster body in the instances list for this client. Then we resize em.
                var allBodies = TeamComponent.GetTeamMembers(TeamIndex.Player).Select((x) => x.body);
                if (allBodies == null)
                {
                    Debug.Log("Master is null... aborting");
                    return;
                }

                CharacterMaster master = null;
                foreach (CharacterBody c in allBodies)
                {
                    if (c.networkIdentity.netId.Value == universalID)
                    {
                        AkSoundEngine.PostEvent(soundID, c.gameObject);
                    }
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(soundID);
                writer.Write(universalID);
            }
        }

        public class HookFromClient : INetMessage
        {
            String hook; // String of hook to be triggered on the EntropyHost
            uint UID; // entity id of player who triggered this client package

            public void Deserialize(NetworkReader reader)
            {
                UID = reader.ReadUInt32();
                hook = reader.ReadString();
            }

            public HookFromClient()
            {

            }

            public HookFromClient(uint _UID, String _hook)
            {
                UID = _UID;
                hook = _hook;
            }

            public void OnReceived()
            {
                //MessageHandler.globalMessage("Pack Received! Identity: " + UID + ", hook type: " + hook);
                //Debug.Log("keyLog from Client: " + hook);
                // queue received hook into host object, with given identity
                ArtifactOfEntropy.entropyHost.queueHook(hook, UID, true);
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(UID);
                writer.Write(hook);
            }
        }

        /*
        // retrieve client info and then modify the server's size info using their settings.
        public class resizeEntityReboundHost : INetMessage
        {
            uint idTarget;
            String sender;

            public void Deserialize(NetworkReader reader)
            {
                idTarget = reader.ReadUInt32();
                sender = reader.ReadString();
            }

            public resizeEntityReboundHost()
            {

            }

            // we need to retrieve the settings from the client, and send it to the host
            public resizeEntityReboundHost(uint _idTarget, String _sender)
            {
                idTarget = _idTarget;
                sender = _sender;
            }

            public void OnReceived()
            {
                // if this is the target network user, then send the packet to the server.
                Debug.Log("Receuved resize REBOUNDHOST packet from " + sender);
                Debug.Log("IsServer: " + NetworkServer.active);

                if (LocalUserManager.GetFirstLocalUser().cachedBody == null)
                {
                    Debug.Log("REBOUNDHOST-> Linking ID TARGET...");
                    LocalUserManager.GetFirstLocalUser().cachedMaster.onBodyStart += (global::RoR2.CharacterBody body) =>
                    {
                        Debug.Log("REBOUNDHOST-> FOUND ID TARGET");
                        new networkBehavior.resizeEntity(OptionsLink.AOM_SizeMultiplier.Value, idTarget, sender).Send(NetworkDestination.Server);
                    };
                    new networkBehavior.resizeEntity(OptionsLink.AOM_SizeMultiplier.Value, idTarget, sender).Send(NetworkDestination.Clients);
                   
                }
                else if(LocalUserManager.GetFirstLocalUser().cachedBody.netId.Value == idTarget)
                {
                    Debug.Log("REBOUNDHOST-> FOUND ID TARGET");
                    new networkBehavior.resizeEntity(OptionsLink.AOM_SizeMultiplier.Value, idTarget, sender).Send(NetworkDestination.Server);
                }

            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(idTarget);
                writer.Write(sender);
            }
        }
        */
        public class resizeEntityRebound : INetMessage
        {
            double scalar;
            uint idTarget;
            String sender;

            public void Deserialize(NetworkReader reader)
            {
                scalar = reader.ReadDouble();
                idTarget = reader.ReadUInt32();
                sender = reader.ReadString();
            }

            public resizeEntityRebound()
            {

            }

            public resizeEntityRebound(double _scalar, uint _idTarget, String _sender)
            {
                scalar = _scalar;
                idTarget = _idTarget;
                sender = _sender;
            }

            public void OnReceived()
            {
                Debug.Log("Receuved resize REBOUND packet from " + sender + " size: " + scalar);
                Debug.Log("IsServer: " + NetworkServer.active);

                if (NetworkServer.active)
                {
                    Debug.Log("Sending resize packet to clients!");
                    new NetworkBehavior.resizeEntity((double)scalar, idTarget, sender).Send(NetworkDestination.Clients);
                    Debug.Log("Done");
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(scalar);
                writer.Write(idTarget);
                writer.Write(sender);
            }
        }

        public class resizeEntity : INetMessage
        {
            double scalar;
            uint idTarget;
            String sender;

            public void Deserialize(NetworkReader reader)
            {
                scalar = reader.ReadDouble();
                idTarget = reader.ReadUInt32();
                sender = reader.ReadString();
            }

            public resizeEntity()
            {

            }

            public resizeEntity(double _scalar, uint _idTarget, String _sender)
            {
                scalar = _scalar;
                idTarget = _idTarget;
                sender = _sender;
            }

            public void OnReceived()
            {
                Debug.Log("Received resize packet from " + sender);
                Debug.Log("IsServer: " + NetworkServer.active);
                uint universalId = idTarget;

                // first we must find the monster body in the instances list for this client. Then we resize em.
                var allMonsterBodiesInStage = TeamComponent.GetTeamMembers(TeamIndex.Monster).Select((x) => x.body);
                // also include player bodies!
                var allPlayerBodiesInStage = TeamComponent.GetTeamMembers(TeamIndex.Player).Select((x) => x.body);

                if (allMonsterBodiesInStage == null)
                {
                    Debug.Log("(Artifact of Titans) Master is null... aborting. Report this if you see this message too much.");
                    return;
                }
                else
                {
                    //Debug.Log("Master ");
                }

                CharacterMaster master = null;
                bool monsterFound = false;

                // monster loop
                foreach (CharacterBody c in allMonsterBodiesInStage)
                {
                    if (c.networkIdentity.netId.Value == universalId)
                    {
                        //Debug.Log("Found monster using universalID. hooking...");
                        master = c.master;
                        if (!master.hasBody)
                        {
                            master.onBodyStart += (global::RoR2.CharacterBody body) =>
                            {
                                resizeMonster(body, scalar);
                            };
                        }
                        else
                        {
                            resizeMonster(master.GetBody(), scalar);
                        }
                        monsterFound = true;
                    }
                }
                if (monsterFound == false)
                {
                    Debug.Log("(Artifact of The Titans): Failed to find monster! UID = " + universalId + " Will attempt to resize later. Sender: " + sender);
                    var mon = new ArtifactOfTitans.desyncedMonster();
                    mon.scalar = this.scalar;
                    mon.overload = 0;
                    mon.uid = this.idTarget;
                    ArtifactOfTitans.desynchronizedMonsters.Add(mon);
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(scalar);
                writer.Write(idTarget);
                writer.Write(sender);
            }
        }

        public class olbiterateUI : INetMessage
        {
            uint idTarget;

            public void Deserialize(NetworkReader reader)
            {
                idTarget = reader.ReadUInt32();
            }

            public olbiterateUI()
            {

            }

            public olbiterateUI(uint _idTarget)
            {
                idTarget = _idTarget;
            }

            public void OnReceived()
            {
                uint universalId = idTarget;

                CharacterMaster master = null;
                foreach (NetworkUser u in NetworkUser.readOnlyInstancesList)
                {
                    if (u.netId.Value == universalId)
                    {
                        Debug.Log("Found user using universalID. hooking...");
                        master = u.master;

                        distortScreen(universalId);
                    }
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(idTarget);
            }
        }

        public static void resizeMonster(CharacterBody body, double scalar)
        {
            Debug.Log("(hooked) Modifying for client...");

            if (body == null)
            {
                Debug.Log("Body is null... aborting");
                return;
            }

            Vector3 size = new Vector3();
            size = body.modelLocator.modelTransform.localScale;

            size.x = body.modelLocator.modelTransform.localScale.x * (float)scalar;
            size.y = body.modelLocator.modelTransform.localScale.y * (float)scalar;
            size.z = body.modelLocator.modelTransform.localScale.z * (float)scalar;
            Debug.Log("Size adjust for " + body.name + "... = " + scalar);
            body.modelLocator.modelTransform.localScale = size;
            body.modelLocator.modelTransform.hasChanged = true;
            body.modelLocator.UpdateModelTransform(0);
            Debug.Log("Size Adjust Done");
            // golem bug fix
            if (body.gameObject.name.Contains("GolemBody"))
            {
                Debug.Log("(AOT) Body is GolemBody, removing buggy materials.");
                GameObject golem = body.modelLocator.transform.gameObject;
                Debug.Log("GameObject => " + golem.name);
                SkinnedMeshRenderer[] components = golem.GetComponentsInChildren<SkinnedMeshRenderer>();
                for (int i = 0; i < components.Length; i++)
                {
                    Debug.Log("SkinnedMeshRenderer => " + components[i].name);
                    components[i].rootBone = null;
                }
            }

            /*
            // adjust the height of the monster so it properly aligns with the ground
            var bodyHeight = Mathf.Abs(body.corePosition.y-body.footPosition.y) * 2;
            var offset = (size.x - 1) * (bodyHeight / 2);
            GameObject heightComponent = new GameObject();
            body.modelLocator.transform;
            */
        }

        static async public void distortScreen(uint universalId)
        {/*
            CharacterBody c = grabUser(universalId); // get user body to affect using UID

            var rnd = new System.Random();
            var cam = c.
            int spinMagnitude = rnd.Next(-720, 720);
            int delay = rnd.Next(1, 5);
            var originalPos = cam.transform.position;
            for (int i = 0; i < 600; i++)
            {
                var vec = new Vector3((float)Math.Tan(i / 50) * spinMagnitude, (float)Math.Sin(i / 50) * spinMagnitude, (float)Math.Cos(i / 50) * spinMagnitude);
                cam.transform.position += vec;

                await Task.Delay(delay);
            }
            cam.transform.position = originalPos;

            // random chance to permanently lose mind! (10% chance)
            if (rnd.Next(0, 101) < 10)
            {
                var i = rnd.Next(0, 1801);
                MessageHandler.globalMessage(c.GetUserName() + " has definitely lost his mind...");
                var vec = new Vector3((float)Math.Tan(i / 50) * spinMagnitude, (float)Math.Sin(i / 50) * spinMagnitude, (float)Math.Cos(i / 50) * spinMagnitude);
                cam.transform.position += vec;

                // play sound (GO CRAZY)
                AkSoundEngine.PostEvent(2719873183, c.gameObject);

                // play sound for all players, localized to id of target
                new networkBehavior.Playsound(2719873183, universalId).Send(R2API.Networking.NetworkDestination.Clients);
            }
            */
        }

        static public CharacterBody grabUser(uint targetID) // get body to affect using UID info
        {
            for (int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++)
            {
                var instance = NetworkUser.readOnlyInstancesList[i];
                if (instance != null && instance.GetCurrentBody() != null && instance.GetCurrentBody().networkIdentity != null
                    && instance.GetCurrentBody().networkIdentity.netId != null)
                {
                    CharacterBody c = NetworkUser.readOnlyInstancesList[i].GetCurrentBody();
                    if (c.networkIdentity.netId.Value == targetID)
                    {
                        return c;
                    }
                }
            }

            Debug.Log("MessageHandler: issue locating target body! Grabuser()");
            return null;
        }
    }
}
