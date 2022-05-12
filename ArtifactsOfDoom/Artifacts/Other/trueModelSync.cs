/*
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Messenger;
using RoR2;
using UnityEngine;
using R2API;
using static On.RoR2.CharacterMaster;

namespace ArtifactGroup
{
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency(MiniRpcPlugin.Dependency)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class rpcModelSync : BaseUnityPlugin
    {
        private const string ModVer = "1.0";
        private const string ModName = "MiniRpcDemo";
        private const string ModGuid = "dev.wildbook.minirpc_demo";

        // Define two actions sending/receiving a single string
        public IRpcAction<monsterPackage> ExampleCommandHost { get; set; }
        public IRpcAction<monsterPackage> ExampleCommandClient { get; set; }

        public IRpcAction<morphPackage> RealTimeFat { get; set; }

        public IRpcAction<bool> Amogus { get; set; }

        public IRpcAction<soundPackage> Playsound { get; set; }

        public IRpcAction<hookPackage> HookOntoServer { get; set; }

        /*
        // Define two actions that manages reading/writing messages themselves
        public IRpcAction<Action<NetworkWriter>> ExampleCommandHostCustom { get; set; }
        public IRpcAction<Action<NetworkWriter>> ExampleCommandClientCustom { get; set; }

        // Define two functions of type `string Function(bool);`
        public IRpcFunc<bool, string> ExampleFuncClient { get; set; }
        public IRpcFunc<bool, string> ExampleFuncHost { get; set; }

        // Define two functions of type `ExampleObject Function(ExampleObject);`
        //public IRpcFunc<ExampleObject, ExampleObject> ExampleFuncClientObject { get; set; }
        public rpcModelSync()
        {
            // Create a MiniRpcInstance that automatically registers all commands to our ModGuid
            // This lets us support multiple mods using the same command ID
            // We could also just generate new command ID's without "isolating" them by mod as well, so it would break if mod load order was different for different clients
            // I opted for the ModGuid instead of an arbitrary number or GUID to encourage mods not to set the same ID
            var miniRpc = MiniRpc.CreateInstance(ModGuid);

            // Define two commands, both transmitting a single string
            //ExampleCommandHost = miniRpc.RegisterAction(Target.Server, (NetworkUser user, monsterPackage pack)
            // => Debug.Log("bruh"))
            ExampleCommandClient = miniRpc.RegisterAction(Target.Client, (NetworkUser user, monsterPackage pack)
                => {

                    if (NetworkServer.active)
                    {
                        return;
                    }
                    double scalar = pack.mscalar;
                    uint universalId = pack.mid;

                    // first we must find the monster body in the instances list for this client. Then we resize em.
                    var allMonsterBodiesInStage = TeamComponent.GetTeamMembers(TeamIndex.Monster).Select((x) => x.body);
                    if (allMonsterBodiesInStage == null)
                    {
                        Debug.Log("Master is null... aborting");
                        return;
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
                    //MessageHandler.globalMessage("Resize Complete");
                });

            HookOntoServer = miniRpc.RegisterAction(Target.Server, (NetworkUser user, hookPackage pack)
                => {
                    MessageHandler.globalMessage("Packed Received! Identity: " + user.userName + ", hook type: " + pack.hook);

                    // queue received hook into host object, with given identity
                    ArtifactOfEntropy.entropyHost.queueHook(pack.hook, pack.UID, true);
                });

            RealTimeFat = miniRpc.RegisterAction(Target.Client, (NetworkUser user, morphPackage pack)
                => {

                    if (NetworkServer.active)
                    {
                        return;
                    }
                    double xscalar = pack.xscalar;
                    double yscalar = pack.yscalar;
                    double zscalar = pack.zscalar;

                    uint universalId = pack.mid;

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
                        if (c.networkIdentity.netId.Value == universalId)
                        {
                            Debug.Log("Found monster using universalID. hooking...");
                            master = c.master;
                            ModelLocator modelLocator = c.GetComponent<ModelLocator>();
                            if (modelLocator)
                            {
                                Vector3 size = new Vector3();
                                size.x = (float)pack.xscalar;
                                size.y = (float)pack.yscalar;
                                size.z = (float)pack.zscalar;

                                Transform modelTransform = modelLocator.modelBaseTransform;
                                //Mesh m = c.gameObject.GetComponent<MeshFilter>().sharedMesh;
                                if (modelTransform)
                                {
                                    modelLocator._modelTransform.localScale = size;
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
                });

            Playsound = miniRpc.RegisterAction(Target.Client, (NetworkUser user, soundPackage pack)
                => {

                    uint soundID = pack.soundID;
                    uint universalId = pack.mid;

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
                        if (c.networkIdentity.netId.Value == universalId)
                        {
                            AkSoundEngine.PostEvent(soundID, c.gameObject);
                        }
                    }
                    //MessageHandler.globalMessage("Resize Complete");
                });


            void resizeMonster(CharacterBody body, double scalar)
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

            Amogus = miniRpc.RegisterAction(Target.Client, (NetworkUser user, bool amogo) =>
            {
                ArtifactOfEntropy.adjustAmogus();
            });

            /// Waits for an arbitrary amount of time
            /// before body creation to enable client
            /// syncup of spawn
            IEnumerator bodyLock()
            {
                yield return new WaitForSeconds(1);
            }

            // Define two commands, both deserializing the data themselves

            // This command will be called by a client (including the host), and executed on the server (host)
            ExampleCommandHostCustom = miniRpc.RegisterAction(Target.Server, (user, x) =>
            {
                // This is what the server will execute when a client invokes the IRpcAction

                var str = x.ReadString();
                var int32 = x.ReadInt32();

                Debug.Log($"[Host] {user?.userName} sent us: {str} {int32}");
            });

            // This command will be called by the host, and executed on all clients
            ExampleCommandClientCustom = miniRpc.RegisterAction(Target.Client, (user, x) =>
            {
                // This is what all clients will execute when the server invokes the IRpcAction

                var str = x.ReadString();
                var int32 = x.ReadInt32();

                Debug.Log($"[Client] Host sent us: {str} {int32}");
            });

            // Here's three examples of RegisterFunc, where you also need to return a value to the caller
            ExampleFuncHost = miniRpc.RegisterFunc<bool, string>(Target.Server, (user, x) =>
            {
                Debug.Log($"[Host] {user?.userName} sent us: {x}");
                return $"Hello from the server, received {x}!";
            });

            // By default, MiniRpcLib will create an ID based on the registration order (first command is ID 0, second command is ID 1, and so on
            // If you want to specify an ID manually, you can choose to do so by doing either of these:
            //
            // RpcActions and RpcFuncs have separate IDs, so both an RpcFunc and an RpcAction can have the same ID without collisions.
            // That said, there's nothing stopping you from using the same Enum for both, as all ID values are valid.
            // (1, 2, 3 being Actions, 4 being a Func, 5 being an action again and so on is okay and valid)


            /*
            _ = miniRpc.RegisterFunc(Target.Client, (NetworkUser user, ExampleObject obj) =>
            {
                Debug.Log($"[Client] Host sent us: {obj}");
                obj.StringExample = "Edited client-side!";
                return obj;
            }, 1234); // <-- Optional ID

            _ = miniRpc.RegisterFunc(Target.Client, (NetworkUser user, ExampleObject obj) =>
            {
                Debug.Log($"[Client] Host sent us: {obj}");
                obj.StringExample = "Edited client-side!";
                return obj;
            }, CommandId.SomeCommandName); // <-- Optional ID
            // The "_ ="'s above mean that the return value will be ignored. In your code you should assign the return value to something to be able to call the function.
        }

        // This enum only exists to show that it can be used as ID for an RpcAction/RpcFunc
        enum CommandId
        {
            //                     ----|    This number is only needed because we already created an RpcFunc with ID 0 (the first one we made without an ID).
            SomeCommandName = 2345, // If you use IDs in your own code, you will most likely want to give all commands explicit IDs, which will avoid this issue.
            SomeOtherCommandName,
        }


        public async void Update()
        {
            // If we hit PageUp on a client, execute ExampleCommandHost on the server with the parameter "C2S!"
            if (Input.GetKeyDown(KeyCode.PageUp))
                ExampleCommandClient.Invoke("CLIENT!");
            ExampleCommandHost.Invoke("HOST!");
        }

    }

    public class monsterPackage : MessageBase
    {
        public double mscalar; // the host report of the monsters approximate size
        public uint mid; // network id of this monster

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(mscalar);
            writer.Write(mid);
        }

        public override void Deserialize(NetworkReader reader)
        {
            mscalar = reader.ReadDouble();
            mid = reader.ReadUInt32();
        }
        public monsterPackage(double monsterCreditCost, uint universalID)
        {
            mscalar = monsterCreditCost;
            mid = universalID;
        }

        public override string ToString() => $"ExampleObject:";
    }

    public class morphPackage : MessageBase
    {
        public double xscalar; // the host report of the monsters approximate size
        public double yscalar; // the host report of the monsters approximate size
        public double zscalar; // the host report of the monsters approximate size

        public uint mid; // network id of this monster

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(xscalar);
            writer.Write(yscalar);
            writer.Write(zscalar);
            writer.Write(mid);
        }

        public override void Deserialize(NetworkReader reader)
        {
            xscalar = reader.ReadDouble();
            yscalar = reader.ReadDouble();
            zscalar = reader.ReadDouble();
            mid = reader.ReadUInt32();
        }
        public morphPackage(uint universalID, double x, double y, double z)
        {
            xscalar = x;
            yscalar = y;
            zscalar = z;
            mid = universalID;
        }

        public override string ToString() => $"ExampleObject:";
    }

    public class soundPackage : MessageBase
    {
        public uint soundID;

        public uint mid; // network id of this monster

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(mid);
            writer.Write(soundID);
        }

        public override void Deserialize(NetworkReader reader)
        {
            mid = reader.ReadUInt32();
            soundID = reader.ReadUInt32();
            mid = reader.ReadUInt32();
        }
        public soundPackage(uint universalID, uint sd)
        {
            soundID = sd;
            mid = universalID;
        }

        public override string ToString() => $"ExampleObject:";
    }
    //hookPackage package = new hookPackage(getLocalUID(), "R");

    public class hookPackage : MessageBase
    {
        public uint UID;

        public String hook; // hook sent by user

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(UID);
            writer.Write(hook);
        }

        public override void Deserialize(NetworkReader reader)
        {
            UID = reader.ReadUInt32();
            hook = reader.ReadString();
        }
        public hookPackage(uint universalID, String h)
        {
            UID = universalID;
            hook = h;
        }

        public override string ToString() => $"ExampleObject:";
    }
}
*/