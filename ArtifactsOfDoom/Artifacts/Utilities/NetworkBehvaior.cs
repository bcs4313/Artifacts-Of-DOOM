
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

namespace ArtifactGroup
{
    [BepInDependency("com.bepis.r2api")]
    public class networkBehavior : BaseUnityPlugin // class handles all network behavior for now. 
    { // may be decomposed later
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
                        Debug.Log("Found monster using universalID. hooking...");
                        master = c.master;
                        ModelLocator modelLocator = c.GetComponent<ModelLocator>();
                        if (modelLocator)
                        {
                            Vector3 size = new Vector3();
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
                MessageHandler.globalMessage("Resize Complete");
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(xscalar);
                writer.Write(yscalar);
                writer.Write(zscalar);
                writer.Write(targetId);
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
                Debug.Log("keyLog from Client: " + hook);
                // queue received hook into host object, with given identity
                ArtifactOfEntropy.entropyHost.queueHook(hook, UID, true);
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(UID);
                writer.Write(hook);
            }
        }

        public class resizeEntity : INetMessage
        {
            double scalar;
            uint idTarget;

            public void Deserialize(NetworkReader reader)
            {
                scalar = reader.ReadDouble();
                idTarget = reader.ReadUInt32();
            }

            public resizeEntity()
            {

            }

            public resizeEntity(double _scalar, uint _idTarget)
            {
                scalar = _scalar;
                idTarget = _idTarget;
            }

            public void OnReceived()
            {
                //if (NetworkServer.active)
                //{
                //    return;
                //}
                uint universalId = idTarget;

                // first we must find the monster body in the instances list for this client. Then we resize em.
                var allMonsterBodiesInStage = TeamComponent.GetTeamMembers(TeamIndex.Monster).Select((x) => x.body);
                if (allMonsterBodiesInStage == null)
                {
                    Debug.Log("Master is null... aborting");
                    return;
                }
                else
                {
                    Debug.Log("Master ");
                }

                CharacterMaster master = null;
                foreach (CharacterBody c in allMonsterBodiesInStage)
                {
                    if (c.networkIdentity.netId.Value == universalId)
                    {
                        Debug.Log("Found monster using universalID. hooking...");
                        master = c.master;
                        master.onBodyStart += (global::RoR2.CharacterBody body) =>
                        {
                            resizeMonster(body, scalar);
                        };
                    }
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(scalar);
                writer.Write(idTarget);
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

                // first we must find the monster body in the instances list for this client. Then we resize em.
                var allMonsterBodiesInStage = TeamComponent.GetTeamMembers(TeamIndex.Monster).Select((x) => x.body);
                if (allMonsterBodiesInStage == null)
                {
                    Debug.Log("Master is null... aborting");
                    return;
                }
                else
                {
                    Debug.Log("Master ");
                }

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

        static void resizeMonster(CharacterBody body, double scalar)
        {
            Debug.Log("(hooked) Modifying for client...");

            if (body == null)
            {
                Debug.Log("Body is null... aborting");
                return;
            }

            if (body.teamComponent.teamIndex == TeamIndex.Player)
            {
                return;
            }

            if (body == null)
            {
                Debug.Log("Body is null... aborting");
                return;
            }

            Vector3 size = new Vector3();
            size = body.gameObject.transform.localScale;
            if (body.isElite)
            {
                scalar += (float)0.4;
            }
            size.x = body.gameObject.transform.localScale.x * (float)scalar;
            size.y = body.gameObject.transform.localScale.y * (float)scalar;
            size.z = body.gameObject.transform.localScale.z * (float)scalar;
            Debug.Log("Size adjust for " + body.name + "... = " + scalar);
            body.gameObject.transform.localScale = size;
        }

        static async public void distortScreen(uint universalId)
        {
            CharacterBody c = grabUser(universalId); // get user body to affect using UID

            var rnd = new System.Random();
            var cam = CameraRigController.readOnlyInstancesList[0].uiCam;
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
        }

        static public CharacterBody grabUser(uint targetID) // get body to affect using UID info
        {
            for (int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++)
            {
                CharacterBody c = NetworkUser.readOnlyInstancesList[i].GetCurrentBody();
                if (c.networkIdentity.netId.Value == targetID)
                {
                    return c;
                }
            }

            //MessageHandler.globalMessage("Error: issue locating target body! (Grabuser)");
            return NetworkUser.readOnlyInstancesList[0].GetCurrentBody();
        }
    }
}
