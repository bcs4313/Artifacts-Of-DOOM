using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using ArtifactsOfDoom;
using Random = System.Random;
using static On.RoR2.GlobalEventManager;
using static On.RoR2.CharacterMaster;
using AK; //sound
using RoR2.WwiseUtils; // sound
using static On.RoR2.Run;
using Messenger;
using static On.RoR2.Artifacts.MonsterTeamGainsItemsArtifactManager;
using orig_Start = On.RoR2.Run.orig_Start;
using R2API.Networking.Interfaces;
using RoR2.Artifacts;
using RoR2.Navigation;
using UnityEngine.UI;
using System.Threading.Tasks;
using RoR2.Orbs;
using RoR2.Projectile;
using EntityStates.Huntress;
using EntityStates.GummyClone;
using UnityEngine.Events;

namespace ArtifactGroup
{
    public class entropyBlaster
    {
        public static Random r = new Random();
        public static int airstrikeQueue = 0;
        public static float meteorSize = 1;
        public static int highestStage = 0;
        public static Guid pastRUNID = new Guid();

        // rotation variables
        public double rotTime;
        public static int rotDirection;
        public static int rotv1;
        public static int rotv2;
        public static int rotv3;
        public static int rotv4;

        static public void WarpEnemies()
        {
            /*
            MessageHandler.globalMessage("Is this hell?");
            double x = r.NextDouble() * 2 + 0.3;
            double y = r.NextDouble() * 2 + 0.3;
            double z = r.NextDouble() * 2 + 0.3;
            foreach (CharacterMaster c in RoR2.CharacterMaster._readOnlyInstancesList)
            {
                CharacterBody body = c.GetBody();
                Vector3 size = new Vector3();
                Transform newTrans = c.gameObject.transform.root;
               
                size = body.gameObject.transform.localScale;

                if (body.teamComponent.teamIndex == TeamIndex.Monster)
                {
                    size.x = body.gameObject.transform.localScale.x * (float)x;
                    size.y = body.gameObject.transform.localScale.y * (float)y;
                    size.z = body.gameObject.transform.localScale.z * (float)z;


                    ModelLocator modelLocator = c.GetBody().GetComponent<ModelLocator>();
                    if (modelLocator)
                    {
                        Transform modelTransform = modelLocator.modelBaseTransform;
                        //Mesh m = c.gameObject.GetComponent<MeshFilter>().sharedMesh;
                        if (modelTransform)
                        {
                            modelLocator._modelTransform.localScale = size;

                            // trigger size change for client
                            // generate a raw client package
                            uint idTarget = c.networkIdentity.netId.Value;

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
            */
        }

        static public void makeChonky(uint targetID)
        {
            // select a target player
            CharacterBody c = grabUser(targetID); // get user body to affect using UID

            // play sound off of player's GameObject
            AkSoundEngine.PostEvent(3256861171, c.gameObject);

            ModelLocator modelLocator = c.GetComponent<ModelLocator>();
            if (modelLocator)
            {
                Vector3 size = new Vector3();
                size.x = modelLocator._modelTransform.localScale.x + (float)1.8;
                size.y = modelLocator._modelTransform.localScale.y + (float)0.4;
                size.z = modelLocator._modelTransform.localScale.z + (float)1.4;

                if (size.x < 0.55)
                {
                    return;
                }

                MessageHandler.globalMessage("ooga I shit pant");
                Transform modelTransform = modelLocator.modelBaseTransform;
                //Mesh m = c.gameObject.GetComponent<MeshFilter>().sharedMesh;
                if (modelTransform)
                {
                    modelLocator._modelTransform.localScale = size;

                    MessageHandler.globalMessage(c.GetUserName() + " needs to lay off the food... (fat fuck)");

                    // reduce firerate by 35%, Increase Dmg by 35%, 
                    // Reduce speed by 25%, increase health by 25%
                    c.baseAttackSpeed *= (float)0.65;
                    c.baseDamage *= (float)1.35;
                    c.baseMoveSpeed *= (float)0.75;
                    c.baseMaxHealth *= (float)1.25;

                    // trigger size change for client
                    // generate a raw client package
                    uint idTarget = c.networkIdentity.netId.Value;

                    // make player eat some mcds
                    new networkBehavior.FattenPlayer(idTarget, size.x, size.y, size.z).Send(R2API.Networking.NetworkDestination.Clients);

                    // play sound for all clients
                    new networkBehavior.Playsound(3256861171, idTarget).Send(R2API.Networking.NetworkDestination.Clients);
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

        static public void makeTiny(uint targetID)
        {
            // select a target player
            CharacterBody c = grabUser(targetID); // get user body to affect using UID

            ModelLocator modelLocator = c.GetComponent<ModelLocator>();
            if (modelLocator && c.baseMaxHealth > 1)
            {
                Vector3 size = new Vector3();
                size.x = modelLocator._modelTransform.localScale.x * (float)0.7;
                size.y = modelLocator._modelTransform.localScale.y * (float)0.7;
                size.z = modelLocator._modelTransform.localScale.z * (float)0.7;

                if(size.x > 2)
                {
                    return;
                }
                
                Transform modelTransform = modelLocator.modelBaseTransform;
                //Mesh m = c.gameObject.GetComponent<MeshFilter>().sharedMesh;
                if (modelTransform)
                {
                    modelLocator._modelTransform.localScale = size;

                    c.baseAttackSpeed *= (float)1.25;
                    c.baseDamage *= (float)0.8;
                    c.baseMoveSpeed *= (float)1.25;
                    c.baseMaxHealth *= (float)0.65;

                    MessageHandler.globalMessage(c.GetUserName() + " lost some weight. Anorexia?");

                    // trigger size change for client
                    // generate a raw client package
                    uint idTarget = c.networkIdentity.netId.Value;

                    // starve the player
                    new networkBehavior.FattenPlayer(idTarget, size.x, size.y, size.z).Send(R2API.Networking.NetworkDestination.Clients);

                    AkSoundEngine.PostEvent(1149063705, c.gameObject);

                    // play sound for all players, localized to id of target
                    new networkBehavior.Playsound(1149063705, idTarget).Send(R2API.Networking.NetworkDestination.Clients);
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

        static public void bananaPeel(uint targetID)
        {
            // select a target player
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            double x = r.NextDouble() * 40;
            double y = r.NextDouble() * 40 + 12;
            double z = r.NextDouble() * 40;

            MessageHandler.globalMessage("Oopsies! Poor " + c.GetUserName() + " is stupid and yeeted himself!");
            Vector3 size = new Vector3();
            size.x = (float)x * c.characterMotor.mass;
            size.y = (float)y * c.characterMotor.mass;
            size.z = (float)z * c.characterMotor.mass;
            c.characterMotor.ApplyForce(size, true, false);
            AkSoundEngine.PostEvent(402528823, c.gameObject);

            new networkBehavior.Playsound(402528823, c.networkIdentity.netId.Value).Send(R2API.Networking.NetworkDestination.Clients);

        }

        static public void BlackHoleOfDoom()
        {
            int spawnCount = r.Next(1, 8);
            MessageHandler.globalMessage("BLACK HOLES! Danger Level: " + spawnCount);
            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 position = findRandomPosGround();
                Quaternion randQuad = new Quaternion();

                // three angles need to be generated here (random)
                float x = (float)r.NextDouble() * 360;
                float y = (float)r.NextDouble() * 360;
                float z = (float)r.NextDouble() * 360;
                float w = (float)r.NextDouble() * 360;
                randQuad.Set(x, y, z, w);

                RoR2.Projectile.ProjectileManager.instance.FireProjectile(Resources.Load<GameObject>("Prefabs/Projectiles/GravSphere"), position, randQuad, null, 0f, 0f, false, DamageColorIndex.Default, null);
            }
        }

        static public void oopsDuplicated()
        {
            //MessageHandler.globalMessage("They Reproduce Like Rabbits!!!");
            foreach (CharacterMaster c in RoR2.CharacterMaster._readOnlyInstancesList)
            {
                CharacterBody body = c.GetBody();
                if (body.teamComponent.teamIndex == TeamIndex.Monster)
                {
                    SpawnCard card = Resources.Load<SpawnCard>("spawncards/CharacterSpawnCards/" + body.GetUserName());
                    card.prefab = body.gameObject;
                    card.directorCreditCost = 0;

                    DirectorCore.spawnedObjects.Capacity = 99999;
                    RoR2.SceneDirector.cardSelector.Capacity = 99999;

                    DirectorPlacementRule rule = new DirectorPlacementRule();
                    rule.placementMode = DirectorPlacementRule.PlacementMode.NearestNode;

                    DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(card, rule, new Xoroshiro128Plus(1));
                    directorSpawnRequest.ignoreTeamMemberLimit = true;
                    Debug.Log(directorSpawnRequest.teamIndexOverride.ToString());
                    DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                }
            }
        }

        static public void drugz(uint targetID)
        {
            // select a target player
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            MessageHandler.globalMessage(c.GetUserName() + " is super high bruh...");
            c.AddTimedBuff(RoR2.RoR2Content.Buffs.TonicBuff, 20, 1);
            AkSoundEngine.PostEvent(3190451810, c.gameObject);

            // play sound for all clients
            new networkBehavior.Playsound(3190451810, c.networkIdentity.netId.Value).Send(R2API.Networking.NetworkDestination.Clients);
        }

        static public void perfection(uint targetID)
        {
            // select a target player
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            MessageHandler.globalMessage(c.GetUserName() + " Received the buff of the gods!");
            c.AddTimedBuff(RoR2.RoR2Content.Buffs.AffixBlue, 22, 1);
            c.AddTimedBuff(RoR2.RoR2Content.Buffs.AffixRed, 22, 1);
            c.AddTimedBuff(RoR2.RoR2Content.Buffs.AffixWhite, 22, 1);
            c.AddTimedBuff(RoR2.RoR2Content.Buffs.FullCrit, 22, 1);
            c.AddTimedBuff(RoR2.RoR2Content.Buffs.CrocoRegen, 22, 1);
            c.AddTimedBuff(RoR2.RoR2Content.Buffs.NoCooldowns, 22, 1);
            c.AddTimedBuff(RoR2.RoR2Content.Buffs.WhipBoost, 22, 1);
            c.AddTimedBuff(RoR2.RoR2Content.Buffs.CloakSpeed, 22, 1);
            c.AddTimedBuff(RoR2.RoR2Content.Buffs.TeamWarCry, 22, 1);
            AkSoundEngine.PostEvent(116063087, c.gameObject);

            // play sound for all clients
            new networkBehavior.Playsound(116063087, c.networkIdentity.netId.Value).Send(R2API.Networking.NetworkDestination.Clients);
        }


        static public void freeItem(uint targetID)
        {
            // get player using UID
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            MessageHandler.globalMessage(c.GetUserName() + " just got a freebie!");
            new ArtifactOfWar().giveItem(Vector3.zero, c);
        }

        static public void shrineSpawn()
        {
            double outcome = 1;
            int spawnCount = 0;
            while(outcome > 0.5)
            {
                spawnCount++;
                outcome = r.NextDouble();
            }
            if (spawnCount > 1)
            {
                MessageHandler.globalMessage(spawnCount + " Boss Shrines have spawned on the map!");
            }
            else
            {
                MessageHandler.globalMessage("A Boss Shrine has spawned on the map!");
            }
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnCard card = Resources.Load<SpawnCard>("spawncards/interactablespawncard/iscShrineBoss");               
                card.directorCreditCost = 0;
                DirectorCore.spawnedObjects.Capacity = 99999;
                RoR2.SceneDirector.cardSelector.Capacity = 99999;

                DirectorPlacementRule rule = new DirectorPlacementRule();
                rule.placementMode = DirectorPlacementRule.PlacementMode.Random;

                DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(card, rule, new Xoroshiro128Plus(1));
                directorSpawnRequest.ignoreTeamMemberLimit = true;
                Debug.Log(directorSpawnRequest.teamIndexOverride.ToString());
                DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
            }
        }

        static public void airStrike()
        {
            int spawnCount = r.Next(10, 500);

            MessageHandler.globalMessage("AIR STRIKE IMMINENT! Danger level: " + spawnCount);

            entropyBlaster.airstrikeQueue += spawnCount;
        }

        static public void chaChing()
        {
            foreach (CharacterMaster c in RoR2.CharacterMaster._readOnlyInstancesList)
            {
                CharacterBody body = c.GetBody();
                if (body.teamComponent.teamIndex == TeamIndex.Player)
                {
                    AkSoundEngine.PostEvent(1288205242, body.gameObject);

                    // play sound for all clients
                    new networkBehavior.Playsound(1288205242, c.networkIdentity.netId.Value).Send(R2API.Networking.NetworkDestination.Clients);
                }
            }
            MessageHandler.globalMessage("Free Money!");
            TeamManager.instance.GiveTeamMoney(TeamIndex.Player, (uint)(25 * Math.Pow(RoR2.Run.instance.difficultyCoefficient, 1.25)));
        }

        static public void spawnShops()
        {
            double outcome = 1;
            int spawnCount = 0;
            while (outcome > 0.5)
            {
                spawnCount++;
                outcome = r.NextDouble();
            }
            if (spawnCount > 1)
            {
                MessageHandler.globalMessage(spawnCount + " Crates have spawned on the map!");
            }
            else
            {
                MessageHandler.globalMessage("A Crate has spawned on the map!");
            }
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnCard card = Resources.Load<SpawnCard>("spawncards/interactablespawncard/iscChest1");
                card.directorCreditCost = 0;
                DirectorCore.spawnedObjects.Capacity = 99999;
                RoR2.SceneDirector.cardSelector.Capacity = 99999;

                DirectorPlacementRule rule = new DirectorPlacementRule();
                rule.placementMode = DirectorPlacementRule.PlacementMode.Random;

                DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(card, rule, new Xoroshiro128Plus(1));
                directorSpawnRequest.ignoreTeamMemberLimit = true;
                Debug.Log(directorSpawnRequest.teamIndexOverride.ToString());
                DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
            }
        }

        static public void amogus(uint targetID)
        {
            MessageHandler.globalMessage("A M O G U S");
            CharacterBody body = grabUser(targetID); // get user body to affect using UID
            if (body.teamComponent.teamIndex == TeamIndex.Player)
            {
                AkSoundEngine.PostEvent(515509094, body.gameObject);

                new networkBehavior.Playsound(515509094, targetID).Send(R2API.Networking.NetworkDestination.Clients);
            }
            ArtifactOfEntropy.adjustAmogus();
            new networkBehavior.Amogus().Send(R2API.Networking.NetworkDestination.Clients);
        }

        static async public Task thunderBolt(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            
            MessageHandler.globalMessage(c.GetUserName()+ " has been SMITED!");

            if (c.mainHurtBox)
            {
                RoR2.Orbs.OrbManager.instance.AddOrb(new LightningStrikeOrb
                {
                    attacker = new GameObject(),
                    damageColorIndex = DamageColorIndex.Item,
                    damageValue = c.healthComponent.health / 5,
                    isCrit = false,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = 1f,
                    target = c.mainHurtBox
                });
            }
            
            c.AddTimedBuff(RoR2.RoR2Content.Buffs.Entangle, 0.3f, 999);
            await Task.Delay(500);
            //c.AddTimedBuff(RoR2.RoR2Content.Buffs.OnFire, 10, 999);
            //c.AddTimedBuff(RoR2.RoR2Content.Buffs.Overheat, 10, 999);
        }

        static async public Task thunderBoltFriendly(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            CharacterBody m = findRandomMonster();

            if (c.mainHurtBox)
            {
                RoR2.Orbs.OrbManager.instance.AddOrb(new LightningStrikeOrb
                {
                    attacker = c.gameObject,
                    damageColorIndex = DamageColorIndex.Item,
                    damageValue = c.damage * 50,
                    isCrit = false,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = 1f,
                    target = m.mainHurtBox
                });
            }

            m.AddTimedBuff(RoR2.RoR2Content.Buffs.Entangle, 0.3f, 999);
            await Task.Delay(500);
            MessageHandler.globalMessage("The power of Zeus, in your hands!");
            //c.AddTimedBuff(RoR2.RoR2Content.Buffs.OnFire, 10, 999);
            //c.AddTimedBuff(RoR2.RoR2Content.Buffs.Overheat, 10, 999);
        }

        static public void meteorStrike()
        {
            //if (RoR2.Run.instance.stageClearCount >= highestStage || RoR2.Run.instance.GetUniqueId() != pastRUNID)
            //{
            //    meteorSize = 1;
            //    highestStage = RoR2.Run.instance.stageClearCount+1;
            //    pastRUNID = RoR2.Run.instance.GetUniqueId();
            //}

            MessageHandler.globalMessage("The Meteors come... In increasing numbers...");
            CharacterBody[] targets = new CharacterBody[RoR2.CharacterMaster._readOnlyInstancesList.Count];
            for(int i = 0; i < RoR2.CharacterMaster._readOnlyInstancesList.Count; i++)
            {
                targets[i] = RoR2.CharacterMaster._readOnlyInstancesList[i].GetBody();
            }
            MeteorStormController component = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NetworkedObjects/MeteorStorm"), targets[0].corePosition, Quaternion.identity).GetComponent<MeteorStormController>();

            /*
            //Vector3 size = new Vector3();
            //size.x = component.impactEffectPrefab.gameObject.transform.localScale.x * meteorSize;
            //size.y = component.impactEffectPrefab.gameObject.transform.localScale.y * meteorSize;
            //size.z = component.impactEffectPrefab.gameObject.transform.localScale.z * meteorSize;
            //component.impactEffectPrefab.gameObject.transform.localScale = size;
            Debug.Log("1");
            component.impactDelay *= meteorSize;
            Debug.Log("2");
            component.blastRadius *= meteorSize;
            Debug.Log("3");
            component.owner = targets[0].gameObject;
            Debug.Log("4");
            component.ownerDamage = targets[0].damage;
            Debug.Log("5");
            component.wavesPerformed = component.waveCount - 1;
            Debug.Log("6");
            component.travelEffectDuration = 5;
            Debug.Log("7");
            */
            component.waveMaxInterval /= meteorSize;
            component.waveMinInterval /= meteorSize;
            component.waveCount *= (int)meteorSize;
            component.blastDamageCoefficient *= 1.05f;
            component.owner = targets[0].gameObject;
            component.ownerDamage = targets[0].baseDamage;
            //component.blastForce *= meteorSize;
            NetworkServer.Spawn(component.gameObject);


            // force a singular meteor to drop in the upcoming wave
            /*
            while (component.meteorList.Count > 1)
            {
                component.meteorList.RemoveAt(component.meteorList.Count - 1);
            }
            */

            /*
            MeteorStormController yeetus = new MeteorStormController();
            yeetus.Start();
            yeetus.owner = c.gameObject;
            yeetus
            yeetus.enabled = true;
            yeetus.wavesPerformed = 15;
            yeetus.waveCount = 15;
            yeetus.waveTimer = 0f;
            yeetus.blastDamageCoefficient = 1;
            yeetus.blastRadius = 100;
            yeetus.blastForce = 100;
            yeetus.impactDelay = 5;
            yeetus.ownerDamage = 1.00f;
            yeetus.
            for (int i = 0; i < 100; i++)
            {
                yeetus.meteorList.Add(new MeteorStormController.Meteor());
                yeetus.waveList.Add(new MeteorStormController.MeteorWave(targets, c.corePosition)); 
            }
            */
            //MeteorStormController.MeteorWave wave = new MeteorStormController.MeteorWave(targets, c.corePosition);
        }
        static public void giveLunarItem(uint targetID)
        {
            // item index lists
            List<ItemIndex> lunars = ItemCatalog.lunarItemList;

            MessageHandler.globalMessage("Free stuff!!! I guess?");

            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            ItemIndex dex;
            Debug.Log("Spawning Lunar...");
            int len = lunars.Count;
            Random rnd = new Random();
            int index = rnd.Next(0, len);
            dex = lunars[index];

            String itemName = PickupCatalog.FindPickupIndex(dex).pickupDef.nameToken;

            c.inventory.GiveItem(dex);
            MessageHandler.globalItemGetMessage(c, dex, 3);

        }

        // Fire a BFG projectile in front of the player, dealing massive DMG
        static public void BFGFriendly(uint targetID)
        {
            MessageHandler.globalMessage("A Big Fuckin Blast to kill a Big Monster's ASS");
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            c.equipmentSlot.bfgChargeTimer = 0.01f;
            c.equipmentSlot.subcooldownTimer = 0.01f;
        }

        // Fire a BFG projectile at the player, with a random monster
        static public void BFGEvil(uint targetID)
        {
            MessageHandler.globalMessage("Looks like DoomGuy's a little pissed at you...");
            CharacterBody c = grabUser(targetID); // get user body to affect using UID

            CharacterBody monsterShooter = findRandomMonster();
            Vector3 position = monsterShooter.transform.position;
            Ray aimRay = new Ray(monsterShooter.corePosition, new Vector3(c.corePosition.x - position.x, c.corePosition.y - position.y, c.corePosition.z - position.z));
            Transform transform = monsterShooter.gameObject.transform;
            if (transform)
            {
                ChildLocator componentInChildren = transform.GetComponentInChildren<ChildLocator>();
                if (componentInChildren)
                {
                    Transform transform2 = componentInChildren.FindChild("Muzzle");
                    if (transform2)
                    {
                        aimRay.origin = transform2.position;
                    }
                }
            }
            monsterShooter.healthComponent.TakeDamageForce(aimRay.direction * -1500f, true, false);
            RoR2.Projectile.ProjectileManager.instance.FireProjectile(Resources.Load<GameObject>("Prefabs/Projectiles/BeamSphere"),
                aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), monsterShooter.gameObject,
                monsterShooter.damage * 0.1f + (float)(c.healthComponent.fullCombinedHealth * 0.03), 4f, Util.CheckRoll(monsterShooter.crit, monsterShooter.master), 
                DamageColorIndex.Item, null, -1f);
        }

        static public void HelfireFriendly(uint targetID)
        {
            MessageHandler.globalMessage("IT BUUUURNS! BUT IT BURNS GOOOOOD!");
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            c.AddHelfireDuration(12f);

            AkSoundEngine.PostEvent(333560252, c.gameObject);

            new networkBehavior.Playsound(333560252, c.netId.Value).Send(R2API.Networking.NetworkDestination.Clients);
        }

        // Fire a BFG projectile at the player, with a random monster
        static public void HelfireEvil()
        {
            MessageHandler.globalMessage("YOU DO NOT GRASP THE FLAME! THE FLAME... GRASPS YOU!");

            CharacterBody monsterShooter = findRandomMonster();
            monsterShooter.AddHelfireDuration(12f);

            AkSoundEngine.PostEvent(61879681, monsterShooter.gameObject);

            new networkBehavior.Playsound(61879681, monsterShooter.netId.Value).Send(R2API.Networking.NetworkDestination.Clients);
        }
        
        // the dumb lunar thing that slows everything and increases the damage it takes (hell yeah)
        static public void crippleWard(uint targetID)
        {
            MessageHandler.globalMessage("I have CrIpPlInG dEpReSsIoN");

            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            Vector3 pos = findRandomPosAir();
            Vector3 pos2 = findRandomPosAir();
            Vector3 pos3 = findRandomPosAir();
            Vector3 pos4 = findRandomPosAir();
            Vector3 pos5 = findRandomPosAir();
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NetworkedObjects/CrippleWard"), pos, Util.QuaternionSafeLookRotation(pos, Vector3.forward));
            NetworkServer.Spawn(gameObject);
            GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NetworkedObjects/CrippleWard"), pos2, Util.QuaternionSafeLookRotation(pos, Vector3.forward));
            NetworkServer.Spawn(gameObject2);
            GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NetworkedObjects/CrippleWard"), pos3, Util.QuaternionSafeLookRotation(pos, Vector3.forward));
            NetworkServer.Spawn(gameObject3);
            GameObject gameObject4 = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NetworkedObjects/CrippleWard"), pos4, Util.QuaternionSafeLookRotation(pos, Vector3.forward));
            NetworkServer.Spawn(gameObject4);
            GameObject gameObject5 = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NetworkedObjects/CrippleWard"), pos5, Util.QuaternionSafeLookRotation(pos, Vector3.forward));
            NetworkServer.Spawn(gameObject5);

            playAll(3907938878); // play sound for EVERYONE
        }

        static public void randomMonsterSubset()
        {
            if(RoR2.Run.instance.GetUniqueId() != pastRUNID)
            {
                EntropySubsetGen.generateMonsterSubset();
                pastRUNID = RoR2.Run.instance.GetUniqueId();
            }

            // spawn a singular monster from subset
            String[] options = EntropySubsetGen.enemySubset;
            String randoMob = options[r.Next(0, options.Length)];

            SpawnCard card = Resources.Load<SpawnCard>("spawncards/characterspawncards/" + randoMob);
            card.directorCreditCost = 0;
            DirectorCore.spawnedObjects.Capacity = 99999;
            RoR2.SceneDirector.cardSelector.Capacity = 99999;

            DirectorPlacementRule rule = new DirectorPlacementRule();
            rule.placementMode = DirectorPlacementRule.PlacementMode.Random;


            DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(card, rule, new Xoroshiro128Plus(1));
            directorSpawnRequest.teamIndexOverride = TeamIndex.Monster;
            directorSpawnRequest.ignoreTeamMemberLimit = true;
            Debug.Log(directorSpawnRequest.teamIndexOverride.ToString());
            DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
            MessageHandler.globalMessage("A gang member has appeared!");
        }

        static public void randomItemSubset(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID

            if (RoR2.Run.instance.GetUniqueId() != pastRUNID)
            {
                EntropySubsetGen.generateItemSubset();
                pastRUNID = RoR2.Run.instance.GetUniqueId();
            }

            // give a singular item from subset
            ItemIndex[] options = EntropySubsetGen.itemSubset;
            int rand = r.Next(0, options.Length);
            ItemIndex randoItem = options[rand];

            c.inventory.GiveItem(randoItem);

            MessageHandler.globalMessage("You have received a gang item!");
        }

        static public void quantumTunnel()
        {
            MessageHandler.globalMessage("This is obviously possible... It aint rocket scien-wait...");
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NetworkedObjects/Zipline"));
            ZiplineController component2 = gameObject.GetComponent<ZiplineController>();
            component2.SetPointAPosition(findRandomPosGround());
            component2.SetPointBPosition(findRandomPosAir());
            //gameObject.AddComponent<DestroyOnTimer>().duration = 30f;
            NetworkServer.Spawn(gameObject);

            playAll(3522444083); // play sound for EVERYONE
        }

        static public void nukeFriendly(uint targetID)
        {
            MessageHandler.globalMessage("The Sentient Pink Goo Follows your calling, for now...");

            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            Vector3 position = c.equipmentSlot.transform.position;
            RaycastHit raycastHit;
            if (Physics.Raycast(c.equipmentSlot.GetAimRay(), out raycastHit, 900f, LayerIndex.world.mask | LayerIndex.defaultLayer.mask))
            {
                position = raycastHit.point;
            }
            GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/NetworkedObjects/OrbitalLaser"), position, Quaternion.identity);
            gameObject.GetComponent<OrbitalLaserController>().ownerBody = c;
            NetworkServer.Spawn(gameObject);

            playAll(3790696932); // play sound for EVERYONE
        }

        static public void nukeEvil()
        {
            MessageHandler.globalMessage("R U N!!! THE SENTIENT PINK GOO COMES FOR CONSUMPTION!!!");
            Vector3 position = findRandomPosGround();
            GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/NetworkedObjects/OrbitalLaser"), position, Quaternion.identity);
            gameObject.GetComponent<OrbitalLaserController>().ownerBody = findRandomMonster();
            NetworkServer.Spawn(gameObject);
            GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/NetworkedObjects/OrbitalLaser"), position, Quaternion.identity);
            gameObject2.GetComponent<OrbitalLaserController>().ownerBody = findRandomMonster();
            NetworkServer.Spawn(gameObject2);

            playAll(736019426); // play sound for EVERYONE
        }

        // add a new random outcome to a random hook in the event pool. heheheheheh
        static public void newOutcome()
        {
            MessageHandler.globalMessage("A new outcome was added to the action pool. Good luck...");
            ArtifactOfEntropy.entropyHost.addRandomOutcome();
        }


        // yeet a monster at the player like a madman
        static public void kobe(uint targetID)
        {
            MessageHandler.globalMessage("KOBE");
            CharacterBody c = grabUser(targetID); // get user body to affect using UID

            CharacterBody monsterShooter = findRandomMonster();
            Vector3 position = monsterShooter.transform.position;
            Ray aimRay = new Ray(monsterShooter.corePosition, new Vector3(c.corePosition.x - position.x, (c.corePosition.y - position.y) + 100, c.corePosition.z - position.z));
            Transform transform = monsterShooter.gameObject.transform;
            if (transform)
            {
                ChildLocator componentInChildren = transform.GetComponentInChildren<ChildLocator>();
                if (componentInChildren)
                {
                    Transform transform2 = componentInChildren.FindChild("Muzzle");
                    if (transform2)
                    {
                        aimRay.origin = transform2.position;
                    }
                }
            }
            AkSoundEngine.PostEvent(4044108823, monsterShooter.gameObject);

            // play sound for all clients
            new networkBehavior.Playsound(4044108823, monsterShooter.netId.Value).Send(R2API.Networking.NetworkDestination.Clients);
            
            monsterShooter.healthComponent.TakeDamageForce(aimRay.direction * (100 * monsterShooter.characterMotor.mass), true, false);
            c.gameObject.transform.Rotate(new Vector3(0, 0, 45) * Time.deltaTime);
        }

        // kill and revive the player (with dios), giving a funny ragdoll motion
        static async public void ragdoll(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            MessageHandler.globalMessage("Mr. " + c.GetUserName() + ", I aint feelin so good...");
            if (c.healthComponent.health > 0)
            {
                var oldHealth = c.healthComponent.health;
                if (c.inventory.GetItemCount(ItemCatalog.FindItemIndex("ExtraLife")) == 0)
                {
                    c.inventory.GiveItemString("ExtraLife");
                }
                c.healthComponent.health = -500;
                await Task.Delay(2000);
                c.healthComponent.health = oldHealth;
            }
        }

        // get a fren (unlimited summons!)
        static public void gummyFriend(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            MessageHandler.globalMessage("fren");
            CharacterMaster characterMaster = (c != null) ? c.master : null;

            GameObject projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/GummyCloneProjectile");
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = projectilePrefab,
                crit = c.RollCrit(),
                damage = 0f,
                damageColorIndex = DamageColorIndex.Item,
                owner = c.gameObject,
                force = 0f,
                position = c.corePosition,
                rotation = c.gameObject.transform.rotation
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }

        // fire some sus sawblades out of your body
        static public void friendlySaw(uint targetID)
        {
            MessageHandler.globalMessage("LooK! I SaWeD thIS MONSTER iN hAlF!!1!11");
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            c.equipmentSlot.FireSaw();
        }

        // trigger the elephant on self
        static public void freeArmor(uint targetID)
        {
            MessageHandler.globalMessage("hehe thicc boi");
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            c.equipmentSlot.FireGainArmor();
        }

        // get a free vending machine!
        static public void freeVending(uint targetID)
        {
            MessageHandler.globalMessage("drink up ur sprote ya goatt");
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            Ray ray = new Ray(c.equipmentSlot.GetAimRay().origin, Vector3.down);
            RaycastHit raycastHit;
            GameObject prefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/VendingMachineProjectile");
            ProjectileManager.instance.FireProjectile(prefab, c.footPosition, Quaternion.identity, c.gameObject, c.damage, 0f, Util.CheckRoll(c.crit, c.master), DamageColorIndex.Default, null, -1f);
        }

        static public void freeHole(uint targetID)
        {
            MessageHandler.globalMessage("S U C C");
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            c.equipmentSlot.FireBlackhole();
        }

        static async public void freeMissiles(uint targetID)
        {
            MessageHandler.globalMessage("Diplomatic Missiles Imbound!");
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            for (int i = 0; i < 10; i++)
            {
                c.equipmentSlot.FireMissile();
                await Task.Delay(250);
            }
        }

        static public void freeScan(uint targetID)
        {
            MessageHandler.globalMessage("ZZZZZZZZZ SCANNNING ZZZZZZ CONTROLSZZZZZZZZZZZZZZZZZZ MANIPULATING SCIENTIFIC DATA SHWOWAHEUAHS TAKE OVER BLAST CONTROLLLLLLLLLLLLLL WORLD GOVMENT SHOWUAHSOUDSHEWWWW SHUT DOWN INFASTRUCTURE HWOUDHSO SSHASI HSHIP EVERUHTN TO CHINA SOHWSUHAWYUH ITS JUST BERUUUUUUUMPHHHHMMMMMMMMMMMMM LOOK AT THIS PERSON *HHIZZZ STATIC NOISES*");
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            c.equipmentSlot.FireScanner();
        }

        static public void fireball(uint targetID)
        {
            MessageHandler.globalMessage("You got dat fire burnin on the dannceee flowW");
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            c.equipmentSlot.FireFireBallDash();
        }

        static public void freeMolotov(uint targetID)
        {
            MessageHandler.globalMessage("I SAY YOU GOT DAT FIYAH BURNIN ON DUH DANCE FLOWWWWWWW");
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            c.equipmentSlot.FireMolotov();
        }

        // Buff ALL Monsters with war cry equipment
        static public void monsterWarCry()
        {
            MessageHandler.globalMessage("Rebellion blood fuels the great enemy horde...");
            CharacterBody c = findRandomMonster();
            c.equipmentSlot.FireTeamWarCry();
        }

        // Buff ALL Monsters with war cry equipment
        static public void monsterDash()
        {
            MessageHandler.globalMessage("How dO i FlY thIS tHIngnNGNGNGNGNGnng???!");
            for (int i = 0; i < 1; i++)
            { 
                CharacterBody c = findRandomMonster();
                c.equipmentSlot.FireFireBallDash();
            }
        }

        // Buff ALL Monsters with war cry equipment
        static public void monsterSaw()
        {
            MessageHandler.globalMessage("You should've 'saw' that coming...");
            for (int i = 0; i < 3; i++)
            {
                CharacterBody c = findRandomMonster();
                c.equipmentSlot.FireSaw();
            }
        }

        // Buff ALL Monsters with war cry equipment
        static public void monsterMolotov()
        {
            MessageHandler.globalMessage("Burn in hell. Just like the monster you a--wait a minute");
            for (int i = 0; i < 3; i++)
            {
                CharacterBody c = findRandomMonster();
                c.equipmentSlot.FireMolotov();
            }
        }

        // Buff ALL Monsters with war cry equipment
        static public void monsterArmor()
        {
            MessageHandler.globalMessage("He's a big chungusssssssss big chungusssssssss");
            for (int i = 0; i < 5; i++)
            {
                CharacterBody c = findRandomMonster();
                c.equipmentSlot.FireGainArmor();
            }
        }
        // spawn a legendary chest in a random location on the map
        static public void legendaryChest()
        {
            MessageHandler.globalMessage("A Legendary Chest has spawned on the map!");
            SpawnCard card = Resources.Load<SpawnCard>("spawncards/interactablespawncard/iscGoldChest");
            card.directorCreditCost = 0;
            DirectorCore.spawnedObjects.Capacity = 99999;
            RoR2.SceneDirector.cardSelector.Capacity = 99999;

            DirectorPlacementRule rule = new DirectorPlacementRule();
            rule.placementMode = DirectorPlacementRule.PlacementMode.Random;

            DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(card, rule, new Xoroshiro128Plus(1));
            directorSpawnRequest.ignoreTeamMemberLimit = true;
            Debug.Log(directorSpawnRequest.teamIndexOverride.ToString());
            DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
        }

        // spawn a void chest in a random location on the map
        static public void voidChest()
        {
            MessageHandler.globalMessage("A Void Chest has spawned on the map!");
            SpawnCard card = Resources.Load<SpawnCard>("spawncards/interactablespawncard/iscVoidChest");
            card.directorCreditCost = 0;
            DirectorCore.spawnedObjects.Capacity = 99999;
            RoR2.SceneDirector.cardSelector.Capacity = 99999;

            DirectorPlacementRule rule = new DirectorPlacementRule();
            rule.placementMode = DirectorPlacementRule.PlacementMode.Random;

            DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(card, rule, new Xoroshiro128Plus(1));
            directorSpawnRequest.ignoreTeamMemberLimit = true;
            Debug.Log(directorSpawnRequest.teamIndexOverride.ToString());
            DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
        }

        static async public void confuse(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            MessageHandler.globalMessage(c.GetUserName() + " is feeling a bit dizzy...");

            var rnd = new Random();

            c.baseMoveSpeed *= -1;
            c.moveSpeed *= -1;

            int wait = rnd.Next(300, 3300);
            // await for a set amount of time before setting movement back to normal (in ms)
            await Task.Delay(wait);
            c.baseMoveSpeed *= -1;
            c.moveSpeed *= -1;
        }

        // Directly Corrupt the appearance of the UI
        static async public void obliterateUI(uint targetID)
        {
            new networkBehavior.olbiterateUI(targetID).Send(R2API.Networking.NetworkDestination.Clients);
        }

        static public void enemyArrowRain()
        {
            MessageHandler.globalMessage("The volleys tear at your flesh and bone!");
            var owner = findRandomMonster();

            // 30 random spots
            for (int i = 0; i < 30; i++)
            {
                ProjectileManager.instance.FireProjectile(ArrowRain.projectilePrefab,
                    findRandomPosGround(), owner.gameObject.transform.rotation, owner.gameObject, owner.baseDamage
                    * ArrowRain.damageCoefficient * 3, 0f, Util.CheckRoll(owner.crit, owner.master), DamageColorIndex.Default, null, -1f);
            }
        }
        // fire an arrow rain from your aimray
        static public void friendlyArrowRain(uint targetID)
        {
            MessageHandler.globalMessage("May the spectral arrows guide your killing...");
            var owner = grabUser(targetID);
            Ray aimRay = owner.equipmentSlot.GetAimRay();
            float maxDistance = 1000f;
            RaycastHit raycastHit;

            // fire at where user is pointing to
            if (Physics.Raycast(aimRay, out raycastHit, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            {
                ProjectileManager.instance.FireProjectile(ArrowRain.projectilePrefab,
                    raycastHit.point, owner.gameObject.transform.rotation, owner.gameObject, owner.baseDamage
                    * ArrowRain.damageCoefficient * 5, 0f, Util.CheckRoll(owner.crit, owner.master), DamageColorIndex.Default, null, -1f);
            }
        }
        // fire an engineer mine from your aimray
        static public void crabRaid()
        {
            MessageHandler.globalMessage("AHOY SPONGEBOB! I HAVE BEEN PLACED IN ROR2 TO GET SLAUGHTERED BY A BLOODTHIRSY GOBLIN ARGARGARGARG!!!");
            for (int i = 0; i < 3; i++)
            {
                SpawnCard card = Resources.Load<SpawnCard>("spawncards/characterspawncards/cscHermitCrab");  
                card.directorCreditCost = 0;
                DirectorCore.spawnedObjects.Capacity = 99999;
                RoR2.SceneDirector.cardSelector.Capacity = 99999;

                DirectorPlacementRule rule = new DirectorPlacementRule();
                rule.placementMode = DirectorPlacementRule.PlacementMode.Random;

                DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(card, rule, new Xoroshiro128Plus(1));
                directorSpawnRequest.ignoreTeamMemberLimit = true;
                directorSpawnRequest.teamIndexOverride = TeamIndex.Monster;
                Debug.Log(directorSpawnRequest.teamIndexOverride.ToString());
                DirectorCore.instance.TrySpawnObject(directorSpawnRequest);

                playAll(1452246043); // play sound for EVERYONE
            }
        }
        // grow wings and flyyyyyyyyyyyyyyy
        static public void jetpack(uint targetID)
        {
            var c = grabUser(targetID);
            MessageHandler.globalMessage(c.GetUserName() + " believes he can flyyyyyyyyyyy");
            c.equipmentSlot.FireJetpack();

            AkSoundEngine.PostEvent(3706064369, c.gameObject);

            // play sound for all players, localized to id of target
            new networkBehavior.Playsound(3706064369, targetID).Send(R2API.Networking.NetworkDestination.Clients);
        }

        // enemies grow wings and flyyyyyyyyyyyyyyy
        static public void evilJetpack()
        {
            MessageHandler.globalMessage("Look at the little demons go!");
            for (int i = 0; i < 10; i++)
            {
                CharacterBody c = findRandomMonster();
                c.equipmentSlot.FireJetpack();
            }
        }

        static public void freeCrit(uint targetID)
        {
            MessageHandler.globalMessage("mm yummy crit");
            var c = grabUser(targetID);
            c.AddTimedBuff(RoR2Content.Buffs.FullCrit, 8f);
        }

        // welp this is terrifying and I love it so much.
        static async public void cameraSpin(uint targetID)
        {
            /*
            MessageHandler.globalMessage("SPINNNNNNNNN");
            var c = grabUser(targetID);
            for (int i = 0; i < CameraRigController.instancesList.Count; i++)
            {
                CameraRigController.instancesList[0].
                    }
            */
        }

        // code is such a strange little thing...
        static public void basicGolemFistFriendly(uint targetID)
        {
            MessageHandler.globalMessage("BRO FIST");
            var c = grabUser(targetID);
            var ray = c.equipmentSlot.GetAimRay();
            RaycastHit raycastHit;
            // simulate raycast hitting a surface
            Physics.Raycast(c.equipmentSlot.GetAimRay(), out raycastHit, 900f, LayerIndex.world.mask | LayerIndex.defaultLayer.mask);

            // setup golem vars
            var link = new EntityStates.TitanMonster.FireFist();

            FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
            fireProjectileInfo.projectilePrefab = link.fistProjectilePrefab; // loophoooooooles
            fireProjectileInfo.position = raycastHit.point;
            fireProjectileInfo.rotation = Quaternion.identity;
            fireProjectileInfo.owner = c.gameObject;
            fireProjectileInfo.damage = c.baseDamage * 20;
            fireProjectileInfo.force = EntityStates.TitanMonster.FireFist.fistForce;
            fireProjectileInfo.crit = c.RollCrit();
            fireProjectileInfo.fuseOverride = EntityStates.TitanMonster.FireFist.entryDuration - EntityStates.TitanMonster.FireFist.trackingDuration;
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }

        // code is such a strange little thing...
        static public void basicGolemFistEvil(uint targetID)
        {
            MessageHandler.globalMessage("DONT GET FISTED!!!!?!?");
            var c = grabUser(targetID);
            var ray = c.equipmentSlot.GetAimRay();
            RaycastHit raycastHit;
            // simulate raycast hitting a surface
            Physics.Raycast(c.equipmentSlot.GetAimRay(), out raycastHit, 900f, LayerIndex.world.mask | LayerIndex.defaultLayer.mask);

            // setup golem vars
            var link = new EntityStates.TitanMonster.FireFist();

            FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
            fireProjectileInfo.projectilePrefab = link.fistProjectilePrefab; // loophoooooooles
            fireProjectileInfo.position = c.footPosition;
            fireProjectileInfo.rotation = Quaternion.identity;
            fireProjectileInfo.owner = new GameObject();
            fireProjectileInfo.damage = new EntityStates.TitanMonster.FireFist().damageStat * EntityStates.TitanMonster.FireFist.fistDamageCoefficient;
            fireProjectileInfo.force = EntityStates.TitanMonster.FireFist.fistForce;
            fireProjectileInfo.crit = c.RollCrit();
            fireProjectileInfo.fuseOverride = EntityStates.TitanMonster.FireFist.entryDuration - EntityStates.TitanMonster.FireFist.trackingDuration;
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }

        // code is such a strange little thing...
        static public void avengersASSEMBLE()
        {
            MessageHandler.globalMessage("AVENGERS... ASSEMBLE!!!");

            for (int i = 0; i < 10; i++)
            {
                // select a random network player
                int randomPlayer = new Random().Next(0, NetworkUser.readOnlyInstancesList.Count);
                var target = NetworkUser.readOnlyInstancesList[randomPlayer].GetCurrentBody();

                var proj = new RoR2.Projectile.GummyCloneProjectile();

                // gummy spawning code
                MasterCopySpawnCard masterCopySpawnCard = MasterCopySpawnCard.FromMaster(target.master, false, false, null);


                masterCopySpawnCard.GiveItem(DLC1Content.Items.GummyCloneIdentifier, 1);
                masterCopySpawnCard.GiveItem(RoR2Content.Items.BoostDamage, proj.damageBoostCount);
                masterCopySpawnCard.GiveItem(RoR2Content.Items.BoostHp, proj.hpBoostCount);

                DirectorCore.MonsterSpawnDistance input = DirectorCore.MonsterSpawnDistance.Close;
                DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                };
                DirectorCore.GetMonsterSpawnDistance(input, out directorPlacementRule.minDistance, out directorPlacementRule.maxDistance);
                DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(masterCopySpawnCard, directorPlacementRule, new Xoroshiro128Plus(Run.instance.seed + (ulong)Run.instance.fixedTime));
                directorSpawnRequest.teamIndexOverride = TeamIndex.Monster;
                directorSpawnRequest.ignoreTeamMemberLimit = true;
                directorSpawnRequest.summonerBodyObject = target.gameObject;
                DirectorSpawnRequest directorSpawnRequest2 = directorSpawnRequest;
                directorSpawnRequest2.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest2.onSpawnedServer, new Action<SpawnCard.SpawnResult>(delegate (SpawnCard.SpawnResult result)
                {
                    CharacterMaster component3 = result.spawnedInstance.GetComponent<CharacterMaster>();
                    GameObject bodyObject = component3.GetBodyObject();
                    if (bodyObject)
                    {
                        foreach (EntityStateMachine entityStateMachine in bodyObject.GetComponents<EntityStateMachine>())
                        {
                            if (entityStateMachine.customName == "Body")
                            {
                                entityStateMachine.SetState(new GummyCloneSpawnState());
                                return;
                            }
                        }
                    }
                }));
                DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
            }
        }

        static public void distortScreen()
        {
            MessageHandler.globalMessage("The squad might be losing their minds...");
            new networkBehavior.olbiterateUI().Send(R2API.Networking.NetworkDestination.Clients);
        }
        /*
        static public void randomTransformation(uint targetID)
        {
            var c = grabUser(targetID);
            MessageHandler.globalMessage("??????  " + c.GetUserName() + " undergone a transformation?");
            Debug.Log("0");
            CharacterBody m = spawnRandomMonster().GetComponent<CharacterBody>();
            Debug.Log(m.ToString());
            Debug.Log("1");
            c.master.bodyInstanceObject = m.master.bodyInstanceObject;
            Debug.Log("5");
            c.master.bodyInstanceId = m.master.bodyInstanceId;
            Debug.Log("6");
            c.master._bodyInstanceId = m.master._bodyInstanceId;
            Debug.Log("7");
            c.master.bodyPrefab = m.master.bodyPrefab;
            Debug.Log("9");
            c.linkedToMaster = true;
            Debug.Log("9");

            String[] options = { "cscBeetle", "cscBeetleGuard", "cscBeetleQueen", "cscBell", "cscBrother", "cscClayBoss", "cscClayBruiser",
            "cscElectricWorm", "cscGolem", "cscGravekeeper", "cscGreaterWisp", "cscHermitCrab", "cscImp", "cscImpBoss", "cscJellyfish"
            , "cscLemurian", "cscLemurianBruiser", "cscLesserWisp", "cscLunarExploder", "cscLunarGolem", "cscLunarWisp", "cscMagmaWorm",
            "cscMiniMushroom", "cscNullifier", "cscParent", "cscParentPod", "cscRoboBallBoss", "cscRoboBallGreenBuddy", "cscRoboBallRedBuddy",
            "cscScav", "cscScavLunar", "cscSquidTurret", "cscSuperRoboBallBoss", "cscTitanGold", "cscVagrant", "cscVulture",
            "cscGrandparent", "cscBackupDrone", "cscArchWisp"};

            var randoMob = options[r.Next(0, options.Length)];

            SpawnCard card = Resources.Load<SpawnCard>("spawncards/characterspawncards/" + randoMob);
            card.directorCreditCost = 0;
            DirectorCore.spawnedObjects.Capacity = 99999;
            RoR2.SceneDirector.cardSelector.Capacity = 99999;

            DirectorPlacementRule rule = new DirectorPlacementRule();
            rule.placementMode = DirectorPlacementRule.PlacementMode.Random;

            c.master.bodyPrefab = card.prefab;
            card.ob
            Transform component = c.master.bodyInstanceObject.GetComponent<Transform>();
            Vector3 vector = component.position;
            Quaternion rotation = component.rotation;
            c.master.Respawn(c.footPosition, c.transform.rotation);
            //c.master.Respawn(c.footPosition, c.gameObject.transform.rotation);
            //Debug.Log("10");
        }
        */
        static public CharacterBody grabUser(uint targetID) // get body to affect using UID info
        {
            for(int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++)
            {
                CharacterBody c = NetworkUser.readOnlyInstancesList[i].GetCurrentBody();
                if(c.networkIdentity.netId.Value == targetID)
                {
                    return c;
                }
            }

            //MessageHandler.globalMessage("Error: issue locating target body! (Grabuser)");
            return NetworkUser.readOnlyInstancesList[0].GetCurrentBody(); 
        }

        static public GameObject spawnRandomMonster()
        {
            String[] options = { "cscBeetle", "cscBeetleGuard", "cscBeetleQueen", "cscBell", "cscBrother", "cscClayBoss", "cscClayBruiser",
            "cscElectricWorm", "cscGolem", "cscGravekeeper", "cscGreaterWisp", "cscHermitCrab", "cscImp", "cscImpBoss", "cscJellyfish"
            , "cscLemurian", "cscLemurianBruiser", "cscLesserWisp", "cscLunarExploder", "cscLunarGolem", "cscLunarWisp", "cscMagmaWorm",
            "cscMiniMushroom", "cscNullifier", "cscParent", "cscParentPod", "cscRoboBallBoss", "cscRoboBallGreenBuddy", "cscRoboBallRedBuddy",
            "cscScav", "cscScavLunar", "cscSquidTurret", "cscSuperRoboBallBoss", "cscTitanGold", "cscVagrant", "cscVulture",
            "cscGrandparent", "cscBackupDrone", "cscArchWisp"};

            String randoMob = options[r.Next(0, options.Length)];

            SpawnCard card = Resources.Load<SpawnCard>("spawncards/characterspawncards/" + randoMob);
            card.directorCreditCost = 0;
            DirectorCore.spawnedObjects.Capacity = 99999;
            RoR2.SceneDirector.cardSelector.Capacity = 99999;

            DirectorPlacementRule rule = new DirectorPlacementRule();
            rule.placementMode = DirectorPlacementRule.PlacementMode.Random;


            DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(card, rule, new Xoroshiro128Plus(1));
            directorSpawnRequest.teamIndexOverride = TeamIndex.Monster;
            directorSpawnRequest.ignoreTeamMemberLimit = true;
            Debug.Log(directorSpawnRequest.teamIndexOverride.ToString());
            MessageHandler.globalMessage("A random monster has appeared!");
            return DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
        }

        public static Vector3 findRandomPosAir()
        {
            SpawnCard spawnCard = Resources.Load<SpawnCard>("spawncards/interactablespawncard/iscShrineBoss");
            spawnCard.directorCreditCost = 0;
            DirectorCore.spawnedObjects.Capacity = 99999;
            RoR2.SceneDirector.cardSelector.Capacity = 99999;

            DirectorPlacementRule rule = new DirectorPlacementRule();
            rule.placementMode = DirectorPlacementRule.PlacementMode.Random;

            Xoroshiro128Plus rng = new Xoroshiro128Plus((ulong)r.Next(0, 100000));
            NodeGraph nodeGraph = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Air);
            List<NodeGraph.NodeIndex> activeNodesForHullMaskWithFlagConditions = nodeGraph.GetActiveNodesForHullMaskWithFlagConditions((HullMask)(1 << (int)spawnCard.hullSize), spawnCard.requiredFlags, spawnCard.forbiddenFlags);
            while (activeNodesForHullMaskWithFlagConditions.Count > 0)
            {
                DirectorCore worker = new DirectorCore();
                int index2 = rng.RangeInt(0, activeNodesForHullMaskWithFlagConditions.Count);
                NodeGraph.NodeIndex nodeIndex4 = activeNodesForHullMaskWithFlagConditions[index2];
                Vector3 position;
                if (nodeGraph.GetNodePosition(nodeIndex4, out position))
                {
                    Quaternion rotation4 = Quaternion.Euler(0f, rng.nextNormalizedFloat * 360f, 0f);
                    return position;
                }
                else
                {
                    activeNodesForHullMaskWithFlagConditions.RemoveAt(index2);
                }
            }
            return Vector3.zero;
        }

        private static Vector3 findRandomPosGround()
        {
            SpawnCard spawnCard = Resources.Load<SpawnCard>("spawncards/interactablespawncard/iscShrineBoss");
            spawnCard.directorCreditCost = 0;
            DirectorCore.spawnedObjects.Capacity = 99999;
            RoR2.SceneDirector.cardSelector.Capacity = 99999;

            DirectorPlacementRule rule = new DirectorPlacementRule();
            rule.placementMode = DirectorPlacementRule.PlacementMode.Random;

            Xoroshiro128Plus rng = new Xoroshiro128Plus((ulong)r.Next(0, 100000));
            NodeGraph nodeGraph = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Ground);
            List<NodeGraph.NodeIndex> activeNodesForHullMaskWithFlagConditions = nodeGraph.GetActiveNodesForHullMaskWithFlagConditions((HullMask)(1 << (int)spawnCard.hullSize), spawnCard.requiredFlags, spawnCard.forbiddenFlags);
            while (activeNodesForHullMaskWithFlagConditions.Count > 0)
            {
                DirectorCore worker = new DirectorCore();
                int index2 = rng.RangeInt(0, activeNodesForHullMaskWithFlagConditions.Count);
                NodeGraph.NodeIndex nodeIndex4 = activeNodesForHullMaskWithFlagConditions[index2];
                Vector3 position;
                if (nodeGraph.GetNodePosition(nodeIndex4, out position))
                {
                    Quaternion rotation4 = Quaternion.Euler(0f, rng.nextNormalizedFloat * 360f, 0f);
                    return position;
                }
                else
                {
                    activeNodesForHullMaskWithFlagConditions.RemoveAt(index2);
                }
            }
            return Vector3.zero;
        }

        private static CharacterBody findRandomMonster()
        {
            var monsters = TeamComponent.GetTeamMembers(TeamIndex.Monster);

            int randomMonster = (int)(Math.Round((monsters.Count * r.NextDouble()) - 1));
            var m = monsters[randomMonster];

            return m.body;
        }

        // play sound to ALL player locations
        private static void playAll(uint uid)
        {
            // local sound playing
            for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
                AkSoundEngine.PostEvent(uid, PlayerCharacterMasterController.instances[i].body.gameObject);

                // play sound for all clients
                new networkBehavior.Playsound(uid, PlayerCharacterMasterController.instances[i].body.networkIdentity.netId.Value).Send(R2API.Networking.NetworkDestination.Clients);
            }
            }  
        }
}

