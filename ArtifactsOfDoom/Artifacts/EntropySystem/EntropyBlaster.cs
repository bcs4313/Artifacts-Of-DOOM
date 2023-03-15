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
        public static List<transformedPlayer> transformedPlayers = new List<transformedPlayer>();

        static public void WarpEnemies()
        {
            try
            {
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
                                MessageHandler.globalMessage("Is this hell?");
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

            }
            catch { }
        }

        static public void makeChonky(uint targetID)
        {
            // select a target player
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
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

                    MessageHandler.globalMessage("\"ooga I shit pant\"");
                    Transform modelTransform = modelLocator.modelBaseTransform;
                    //Mesh m = c.gameObject.GetComponent<MeshFilter>().sharedMesh;
                    if (modelTransform)
                    {
                        modelLocator._modelTransform.localScale = size;

                        MessageHandler.globalMessage(c.GetUserName() + " needs to lay off the food...");
                        
                        // reduce firerate by 35%, Increase Dmg by 35%, 
                        // Reduce speed by 25%, increase health by 25%
                        c.baseAttackSpeed *= (float)0.75;
                        c.baseDamage *= (float)1.2;
                        c.baseMoveSpeed *= (float)0.85;
                        c.baseMaxHealth *= (float)1.25;
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
        }

        static public void makeTiny(uint targetID)
        {
            // select a target player
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
                ModelLocator modelLocator = c.GetComponent<ModelLocator>();
                if (modelLocator && c.baseMaxHealth > 1)
                {
                    Vector3 size = new Vector3();
                    size.x = modelLocator._modelTransform.localScale.x * (float)0.7;
                    size.y = modelLocator._modelTransform.localScale.y * (float)0.7;
                    size.z = modelLocator._modelTransform.localScale.z * (float)0.7;

                    if (size.x > 2)
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
        }

        static public void bananaPeel(uint targetID)
        {
            // select a target player
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
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
            if (c != null)
            {
                MessageHandler.globalMessage(c.GetUserName() + " is super high bruh...");
                c.AddTimedBuff(RoR2.RoR2Content.Buffs.TonicBuff, 20, 1);
                AkSoundEngine.PostEvent(3190451810, c.gameObject);

                // play sound for all clients
                new networkBehavior.Playsound(3190451810, c.networkIdentity.netId.Value).Send(R2API.Networking.NetworkDestination.Clients);
            }
        }
        /*
        static public void superSonic(uint targetID)
        {
            // select a target player
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            MessageHandler.globalMessage(c.GetUserName() + " is going supersonic!");
            c.AddTimedBuff(RoR2.RoR2Content.Buffs.AttackSpeedOnCrit, 20, 5);
        }
        */
            static public void perfection(uint targetID)
            {
            // select a target player
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
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
        }


        static public void freeItem(uint targetID)
        {
            // get player using UID
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
                MessageHandler.globalMessage(c.GetUserName() + " just got a freebie!");
                new ArtifactOfWar().giveItem(Vector3.zero, c);
            }
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
            try
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
            catch { }
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
            try
            {

                CharacterBody body = grabUser(targetID); // get user body to affect using UID
                if (body.teamComponent.teamIndex == TeamIndex.Player && body != null)
                {
                    AkSoundEngine.PostEvent(515509094, body.gameObject);

                    new networkBehavior.Playsound(515509094, targetID).Send(R2API.Networking.NetworkDestination.Clients);
                }
                ArtifactOfEntropy.adjustAmogus();
                new networkBehavior.Amogus().Send(R2API.Networking.NetworkDestination.Clients);
            }
            catch { }
        }

        static async public Task thunderBolt(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            
            if (c != null)
            {
                MessageHandler.globalMessage(c.GetUserName() + " has been SMITED!");
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
        }

        static async public Task thunderBoltFriendly(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            CharacterBody m = findRandomMonster();
            if (m != null && c != null)
            {
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

            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
                MessageHandler.globalMessage("Free stuff!!! I guess?");
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

        }

        // Fire a BFG projectile in front of the player, dealing massive DMG
        static public void BFGFriendly(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
                MessageHandler.globalMessage("A Big Fuckin Blast to kill a Big Monster's ASS");
                c.equipmentSlot.bfgChargeTimer = 0.01f;
                c.equipmentSlot.subcooldownTimer = 0.01f;
            }
        }

        // Fire a BFG projectile at the player, with a random monster
        static public void BFGEvil(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID

            CharacterBody monsterShooter = findRandomMonster();
            if (monsterShooter != null && c != null)
            {
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
                MessageHandler.globalMessage("Looks like DoomGuy's a little pissed at you...");
            }
        }

        static public void HelfireFriendly(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
                MessageHandler.globalMessage("IT BUUUURNS! BUT IT BURNS GOOOOOD!");
                c.AddHelfireDuration(12f);

                AkSoundEngine.PostEvent(333560252, c.gameObject);

                new networkBehavior.Playsound(333560252, c.netId.Value).Send(R2API.Networking.NetworkDestination.Clients);
            }
        }

        // Fire a BFG projectile at the player, with a random monster
        static public void HelfireEvil()
        {

            CharacterBody monsterShooter = findRandomMonster();
            if (monsterShooter != null)
            {
                MessageHandler.globalMessage("YOU DO NOT GRASP THE FLAME! THE FLAME... GRASPS YOU!");
                monsterShooter.AddHelfireDuration(12f);

                AkSoundEngine.PostEvent(61879681, monsterShooter.gameObject);

                new networkBehavior.Playsound(61879681, monsterShooter.netId.Value).Send(R2API.Networking.NetworkDestination.Clients);
            }
        }
        
        // the dumb lunar thing that slows everything and increases the damage it takes (hell yeah)
        static public void crippleWard(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
                MessageHandler.globalMessage("I have CrIpPlInG dEpReSsIoN");
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
        }

        static async public void randomTransformSubset(CharacterBody c)
        {
            // become a singular monster from subset
            String[] options = EntropySubsetGen.transformSubset;
            String randoMob = options[r.Next(0, options.Length)];
            for (int i = 0; i < RoR2.NetworkUser.instancesList.Count; i++)
            {
                var user = RoR2.NetworkUser.instancesList[i];
                if (user != null && user.GetCurrentBody() != null)
                {
                    if (user.GetCurrentBody().netId == c.netId && !isTransformed(user.netId.Value)) // if the body is our target, with the applied net user
                    {
                        var tp = new transformedPlayer();
                        var transformBack = user.GetCurrentBody().bodyIndex;
                        tp.originalIndex = transformBack;
                        tp.userID = user.netId.Value;
                        transformedPlayers.Add(tp);
                        Random r = new Random();
                        var n = r.Next(3, 60);
                        MessageHandler.globalMessage(c.GetUserName() + " become a gang member for " + n + " seconds!");

                        user.SetBodyPreference(BodyCatalog.FindBodyIndex(randoMob));
                        c.master.Respawn(c.footPosition, c.transform.rotation);
                        await Task.Delay(1000 * n);
                        if (getTransformBack(user.netId.Value) != BodyIndex.None)
                        {
                            user.SetBodyPreference(getTransformBack(user.netId.Value));
                            c.master.Respawn(RoR2.NetworkUser.instancesList[i].GetCurrentBody().footPosition, RoR2.NetworkUser.instancesList[i].transform.rotation);
                            removeTransformed(user.netId.Value);
                        }
                        else
                        {
                            Debug.Log("(Artifact of Entropy) Transform Back Failed! Survivor is stuck as a transformed Object!!!");
                        }
                    }
                }
            }

        }

        static public void randomMonsterSubset()
        {
            try
            {
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
            catch
            {
                Debug.Log("Entropy Spawn Failed. (Unless this message is spammed its fine.");
            }
        }

        static public void randomItemSubset(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
                // give a singular item from subset
                ItemIndex[] options = EntropySubsetGen.itemSubset;
                int rand = r.Next(0, options.Length);
                ItemIndex randoItem = options[rand];

                c.inventory.GiveItem(randoItem);

                MessageHandler.globalMessage("You have received a gang item!");
            }
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
            if (c != null)
            {
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
        }

        static public void nukeEvil()
        {
            CharacterBody monsterOwner = findRandomMonster();
            if (monsterOwner != null)
            {
                MessageHandler.globalMessage("R U N!!! THE SENTIENT PINK GOO COMES FOR CONSUMPTION!!!");
                Vector3 position = findRandomPosGround();
                GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/NetworkedObjects/OrbitalLaser"), position, Quaternion.identity);
                gameObject.GetComponent<OrbitalLaserController>().ownerBody = monsterOwner;
                NetworkServer.Spawn(gameObject);
                GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/NetworkedObjects/OrbitalLaser"), position, Quaternion.identity);
                gameObject2.GetComponent<OrbitalLaserController>().ownerBody = monsterOwner;
                NetworkServer.Spawn(gameObject2);

                playAll(736019426); // play sound for EVERYONE
            }
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
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (findRandomMonster() != null && c != null)
            {
                try
                {
                    MessageHandler.globalMessage("KOBE");
                    for (int i = 0; i < 5; i++)
                    {
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

                        // play sound for all clients
                        playAll(4044108823);

                        monsterShooter.healthComponent.TakeDamageForce(aimRay.direction * (100 * monsterShooter.characterMotor.mass), true, false);
                        c.gameObject.transform.Rotate(new Vector3(0, 0, 45) * Time.deltaTime);
                    }
                }
                catch {
                    Debug.Log("kobe :: E");
                }
            }
        }

        // kill and revive the player (with dios), giving a funny ragdoll motion
        static async public void ragdoll(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
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
        }

        // get a fren (unlimited summons!)
        static public void gummyFriend(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
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
        }

        // fire some sus sawblades out of your body
        static public void friendlySaw(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
                MessageHandler.globalMessage("LooK! I SaWeD thIS MONSTER iN hAlF!!1!11");
                c.equipmentSlot.FireSaw();
            }
        }

        // trigger the elephant on self
        static public void freeArmor(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
                MessageHandler.globalMessage("hehe thicc boi");
                c.equipmentSlot.FireGainArmor();
            }
        }

        // get a free vending machine!
        static public void freeVending(uint targetID)
        {  
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
                MessageHandler.globalMessage("drink up ur sprote ya goatt");
                Ray ray = new Ray(c.equipmentSlot.GetAimRay().origin, Vector3.down);
                RaycastHit raycastHit;
                GameObject prefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/VendingMachineProjectile");
                ProjectileManager.instance.FireProjectile(prefab, c.footPosition, Quaternion.identity, c.gameObject, c.damage, 0f, Util.CheckRoll(c.crit, c.master), DamageColorIndex.Default, null, -1f);
            }
        }

        static public void freeHole(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
                MessageHandler.globalMessage("S U C C");
                c.equipmentSlot.FireBlackhole();
            }
        }

        static async public void freeMissiles(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
                MessageHandler.globalMessage("Diplomatic Missiles Imbound!");
                for (int i = 0; i < 10; i++)
                {
                    c.equipmentSlot.FireMissile();
                    await Task.Delay(250);
                }
            }
        }

        static public void freeScan(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
                MessageHandler.globalMessage("ZZZZZZZZZ SCANNNING ZZZZZZ CONTROLSZZZZZZZZZZZZZZZZZZ MANIPULATING SCIENTIFIC DATA SHWOWAHEUAHS TAKE OVER BLAST CONTROLLLLLLLLLLLLLL WORLD GOVMENT SHOWUAHSOUDSHEWWWW SHUT DOWN INFASTRUCTURE HWOUDHSO SSHASI HSHIP EVERUHTN TO CHINA SOHWSUHAWYUH ITS JUST BERUUUUUUUMPHHHHMMMMMMMMMMMMM LOOK AT THIS PERSON *HHIZZZ STATIC NOISES*");
                c.equipmentSlot.FireScanner();
            }
        }

        static public void fireball(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
                c.equipmentSlot.FireFireBallDash();
                MessageHandler.globalMessage("You got dat fire burnin on the dannceee flowW");
            }
        }

        static public void freeMolotov(uint targetID)
        {
            CharacterBody c = grabUser(targetID); // get user body to affect using UID
            if (c != null)
            {
                c.equipmentSlot.FireMolotov();
                MessageHandler.globalMessage("I SAY YOU GOT DAT FIYAH BURNIN ON DUH DANCE FLOWWWWWWW");
            }
        }

        // Buff ALL Monsters with war cry equipment
        static public void monsterWarCry()
        {
            CharacterBody c = findRandomMonster();
            if (c != null)
            {
                MessageHandler.globalMessage("Rebellion blood fuels the great enemy horde...");
                c.equipmentSlot.FireTeamWarCry();
            }
        }

        // Buff ALL Monsters with war cry equipment
        static public void monsterDash()
        {
            if (findRandomMonster() != null)
            {
                MessageHandler.globalMessage("How dO i FlY thIS tHIngnNGNGNGNGNGnng???!");
                for (int i = 0; i < 1; i++)
                {
                    CharacterBody c = findRandomMonster();
                    c.equipmentSlot.FireFireBallDash();
                }
            }
        }

        // Buff ALL Monsters with war cry equipment
        static public void monsterSaw()
        {
            if (findRandomMonster() != null)
            {
                MessageHandler.globalMessage("You should've 'saw' that coming...");
                for (int i = 0; i < 3; i++)
                {
                    CharacterBody c = findRandomMonster();
                    c.equipmentSlot.FireSaw();
                }
            }
        }

        // Buff ALL Monsters with war cry equipment
        static public void monsterMolotov()
        {
            if (findRandomMonster() != null)
            {
                MessageHandler.globalMessage("Burn in hell. Just like the monster you a--wait a minute");
                for (int i = 0; i < 3; i++)
                {
                    CharacterBody c = findRandomMonster();
                    c.equipmentSlot.FireMolotov();
                }
            }
        }

        // Buff ALL Monsters with war cry equipment
        static public void monsterArmor()
        {
            if (findRandomMonster() != null)
            {
                MessageHandler.globalMessage("He's a big chungusssssssss big chungusssssssss");
                for (int i = 0; i < 5; i++)
                {
                    CharacterBody c = findRandomMonster();
                    c.equipmentSlot.FireGainArmor();
                }
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
            if (c != null)
            {
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
        }

        // Directly Corrupt the appearance of the UI
        static async public void obliterateUI(uint targetID)
        {
            new networkBehavior.olbiterateUI(targetID).Send(R2API.Networking.NetworkDestination.Clients);
        }

        static public void enemyArrowRain()
        {
            var owner = findRandomMonster();
            if (owner != null)
            {
                MessageHandler.globalMessage("The volleys tear at your flesh and bone!");
                // 30 random spots
                for (int i = 0; i < 30; i++)
                {
                    ProjectileManager.instance.FireProjectile(ArrowRain.projectilePrefab,
                        findRandomPosGround(), owner.gameObject.transform.rotation, owner.gameObject, owner.baseDamage
                        * ArrowRain.damageCoefficient * 3, 0f, Util.CheckRoll(owner.crit, owner.master), DamageColorIndex.Default, null, -1f);
                }
            }
        }
        // fire an arrow rain from your aimray
        static public void friendlyArrowRain(uint targetID)
        {
            var owner = grabUser(targetID);
            if (owner != null)
            {
                MessageHandler.globalMessage("May the spectral arrows guide your killing...");
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
            if (c != null)
            {
                MessageHandler.globalMessage(c.GetUserName() + " believes he can flyyyyyyyyyyy");
                c.equipmentSlot.FireJetpack();

                AkSoundEngine.PostEvent(3706064369, c.gameObject);

                // play sound for all players, localized to id of target
                new networkBehavior.Playsound(3706064369, targetID).Send(R2API.Networking.NetworkDestination.Clients);
            }
        }

        // enemies grow wings and flyyyyyyyyyyyyyyy
        static public void evilJetpack()
        {
            if (findRandomMonster() != null)
            {
                MessageHandler.globalMessage("Look at the little demons go!");
                for (int i = 0; i < 10; i++)
                {
                    CharacterBody c = findRandomMonster();
                    c.equipmentSlot.FireJetpack();
                }
            }
        }

        static public void freeCrit(uint targetID)
        {
            var c = grabUser(targetID);
            if (c != null)
            {
                MessageHandler.globalMessage("aaah spicy crit");
                c.AddTimedBuff(RoR2Content.Buffs.FullCrit, 8f);
            }
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
            try
            {
                var c = grabUser(targetID);
                if (c != null)
                {
                    MessageHandler.globalMessage("BRO FIST");
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
            }
            catch
            {
                Debug.Log("basicGolemFistFriendly ::: E");
            }
        }

        // code is such a strange little thing...
        static public void basicGolemFistEvil(uint targetID)
        {
            try
            {
                var c = grabUser(targetID);
                if (c != null)
                {
                    MessageHandler.globalMessage("DONT GET FISTED!!!!?!?");
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
                    fireProjectileInfo.damage = c.maxHealth * 0.25f;
                    fireProjectileInfo.force = EntityStates.TitanMonster.FireFist.fistForce;
                    fireProjectileInfo.crit = c.RollCrit();
                    fireProjectileInfo.fuseOverride = EntityStates.TitanMonster.FireFist.entryDuration - EntityStates.TitanMonster.FireFist.trackingDuration;
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
            }
            catch {
                Debug.Log("GolemFistEvil ::: E");
            }
        }

        // code is such a strange little thing...
        static public void avengersASSEMBLE()
        {
            MessageHandler.globalMessage("AVENGERS... ASSEMBLE!!!");

            for (int i = 0; i < 3; i++)
            {
                // select a random network player
                int randomPlayer = new Random().Next(0, NetworkUser.readOnlyInstancesList.Count);
                var target = NetworkUser.readOnlyInstancesList[randomPlayer].GetCurrentBody();

                var proj = new RoR2.Projectile.GummyCloneProjectile();

                // gummy spawning code
                MasterCopySpawnCard masterCopySpawnCard = MasterCopySpawnCard.FromMaster(target.master, false, false, null);


                masterCopySpawnCard.GiveItem(DLC1Content.Items.GummyCloneIdentifier, 1);
                //masterCopySpawnCard.GiveItem(RoR2Content.Items.BoostDamage, proj.damageBoostCount);
                //masterCopySpawnCard.GiveItem(RoR2Content.Items.BoostHp, proj.hpBoostCount);

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
        
        static public void Yummy(uint targetID)
        {
            var c = grabUser(targetID);
            if (c != null)
            {
                MessageHandler.globalMessage("mmmm yummy froot");
                c.equipmentSlot.FireFruit();

                // generate a raw client package
                uint idTarget = c.networkIdentity.netId.Value;

                // play sound for all clients
                new networkBehavior.Playsound(625680363, idTarget).Send(R2API.Networking.NetworkDestination.Clients);
            }
        }

        static public void friendlyVoidBlast(uint targetID)
        {
            try
            {
                MessageHandler.globalMessage("DOOM BLAST");
                var c = grabUser(targetID);
                var b = new EntityStates.VoidSurvivor.Weapon.FireCorruptDisks();
                for (int i = 0; i < b.projectileCount; i++)
                {
                    float num = (float)i - (float)(b.projectileCount - 1) / 2f;
                    float bonusYaw = num * b.yawPerProjectile;
                    float d = num * b.offsetPerProjectile;
                    Ray aimRay = c.equipmentSlot.GetAimRay();
                    aimRay.direction = Util.ApplySpread(aimRay.direction, 0f, c.spreadBloomAngle + b.spread, 1f, 1f, bonusYaw, b.bonusPitch);
                    Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
                    Vector3.ProjectOnPlane(onUnitSphere, aimRay.direction);
                    Quaternion rotation = Util.QuaternionSafeLookRotation(aimRay.direction, onUnitSphere);
                    FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                    fireProjectileInfo.projectilePrefab = b.projectilePrefab;
                    fireProjectileInfo.position = aimRay.origin + Vector3.Cross(aimRay.direction, Vector3.up) * d;
                    fireProjectileInfo.rotation = rotation;
                    fireProjectileInfo.owner = c.gameObject;
                    fireProjectileInfo.damage = c.damage * 10;
                    fireProjectileInfo.force = b.force;
                    fireProjectileInfo.crit = Util.CheckRoll(b.critStat, c.master);
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
            }
            catch
            {
                Debug.Log("Void Blast::: E");
            }
        }

        static public void evilVoidBlast()
        {
            var b = new EntityStates.VoidSurvivor.Weapon.FireCorruptDisks();
            if (findRandomMonster() != null)
            {
                MessageHandler.globalMessage("A Group of DOOM BlASTS SEEK YOUR DEATH");
                for (int i = 0; i < 5; i++)
                {
                    var c = findRandomMonster();
                    float num = (float)i - (float)(b.projectileCount - 1) / 2f;
                    float bonusYaw = num * b.yawPerProjectile;
                    float d = num * b.offsetPerProjectile;
                    Ray aimRay = c.equipmentSlot.GetAimRay();
                    aimRay.direction = Util.ApplySpread(aimRay.direction, 0f, c.spreadBloomAngle + b.spread, 1f, 1f, bonusYaw, b.bonusPitch);
                    Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
                    Vector3.ProjectOnPlane(onUnitSphere, aimRay.direction);
                    Quaternion rotation = Util.QuaternionSafeLookRotation(aimRay.direction, onUnitSphere);
                    FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                    fireProjectileInfo.projectilePrefab = b.projectilePrefab;
                    fireProjectileInfo.position = aimRay.origin + Vector3.Cross(aimRay.direction, Vector3.up) * d;
                    fireProjectileInfo.rotation = rotation;
                    fireProjectileInfo.owner = c.gameObject;
                    fireProjectileInfo.damage = c.damage * 3;
                    fireProjectileInfo.force = b.force;
                    fireProjectileInfo.crit = Util.CheckRoll(b.critStat, c.master);
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
            }       
        }

        static async public void megaTarBallFriendly(uint targetID)
        {
            var c = grabUser(targetID);
            if(c != null)
            {
                MessageHandler.globalMessage("This amount of poo is very unhealthy, you need to stop.");
                for (int i = 0; i < 10; i++)
                {
                    c = grabUser(targetID);
                    var b = new EntityStates.ClayBoss.FireTarball();

                    Util.PlaySound(EntityStates.ClayBoss.FireTarball.attackSoundString, c.gameObject);

                    Vector3 forward = Vector3.ProjectOnPlane(c.equipmentSlot.GetAimRay().direction, Vector3.up);
                    ProjectileManager.instance.FireProjectile(EntityStates.ClayBoss.FireTarball.projectilePrefab, c.equipmentSlot.GetAimRay().origin, Util.QuaternionSafeLookRotation(forward), c.gameObject, c.damage * 10, 0f, Util.CheckRoll(c.crit, c.master), DamageColorIndex.Default, null, -1f);
                    c.AddSpreadBloom(EntityStates.ClayBoss.FireTarball.spreadBloomValue);
                    await Task.Delay(200);
                }
            }
        }

        static async public void megaTarBallEVIL()
        {
            if (findRandomMonster() != null)
            {
                MessageHandler.globalMessage("Sludge, sludge everywhere.");
                for (int i = 0; i < 10; i++)
                {
                    var c = findRandomMonster();
                    var b = new EntityStates.VoidMegaCrab.Weapon.FireCrabWhiteCannon();
                    Util.PlaySound(EntityStates.ClayBoss.FireTarball.attackSoundString, c.gameObject);
                    Vector3 forward = Vector3.ProjectOnPlane(c.equipmentSlot.GetAimRay().direction, Vector3.up);
                    ProjectileManager.instance.FireProjectile(EntityStates.ClayBoss.FireTarball.projectilePrefab, c.equipmentSlot.GetAimRay().origin, Util.QuaternionSafeLookRotation(forward), c.gameObject, c.damage * 2, 0f, Util.CheckRoll(c.crit, c.master), DamageColorIndex.Default, null, -1f);
                    c.AddSpreadBloom(EntityStates.ClayBoss.FireTarball.spreadBloomValue);
                    await Task.Delay(200);
                }
            }
        }

        static public void bloodShrineSpawn()
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
                MessageHandler.globalMessage(spawnCount + " Blood Shrines have spawned on the map!");
            }
            else
            {
                MessageHandler.globalMessage("A Blood Shrine has spawned on the map!");
            }
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnCard card = Resources.Load<SpawnCard>("spawncards/interactablespawncard/iscShrineBlood");
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

        static public void chanceShrineSpawn()
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
                MessageHandler.globalMessage(spawnCount + " Chance Shrines have spawned on the map!");
            }
            else
            {
                MessageHandler.globalMessage("A Chance Shrine has spawned on the map!");
            }
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnCard card = Resources.Load<SpawnCard>("spawncards/interactablespawncard/iscShrineChance");
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

        static public void orderShrineSpawn()
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
                MessageHandler.globalMessage(spawnCount + " Order Shrines have spawned on the map (dear god).");
            }
            else
            {
                MessageHandler.globalMessage("An Order Shrine has spawned on the map!");
            }
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnCard card = Resources.Load<SpawnCard>("spawncards/interactablespawncard/iscShrineRestack");
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

        static public void combatShrineSpawn()
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
                MessageHandler.globalMessage(spawnCount + " Combat Shrines have spawned on the map!");
            }
            else
            {
                MessageHandler.globalMessage("A Combat Shrine has spawned on the map!");
            }
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnCard card = Resources.Load<SpawnCard>("spawncards/interactablespawncard/iscShrineCombat");
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

        static public void healingShrineSpawn()
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
                MessageHandler.globalMessage(spawnCount + " Healing Shrines have spawned on the map!");
            }
            else
            {
                MessageHandler.globalMessage("A Healing Shrine has spawned on the map!");
            }
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnCard card = Resources.Load<SpawnCard>("spawncards/interactablespawncard/iscShrineHealing");
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

        static public void cleanseShrineSpawn()
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
                MessageHandler.globalMessage(spawnCount + " Cleansing Pools have spawned on the map!");
            }
            else
            {
                MessageHandler.globalMessage("A Cleansing Pool has spawned on the map!");
            }
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnCard card = Resources.Load<SpawnCard>("spawncards/interactablespawncard/iscShrineCleanse");
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

        static async public void spawnWallFriendly(uint targetID)
        {
            var c = grabUser(targetID);
            if (c != null)
            {
                MessageHandler.globalMessage("We will build a great great wall.");
                var prefab = EntityStates.Mage.Weapon.PrepWall.projectilePrefab;
                playAll(1161093638);
                for (int i = 0; i < 20; i++)
                {
                    Vector3 forward = Vector3.forward * new Random().Next(0, 360);
                    forward.y = 0f;
                    forward.Normalize();
                    Vector3 vector = Vector3.Cross(Vector3.up, forward);
                    bool crit = Util.CheckRoll(c.crit, c.master);
                    Vector3 pos = findRandomPosGround();
                    prefab.transform.localScale.Set(5, 5, 5);
                    ProjectileManager.instance.FireProjectile(prefab, pos, Util.QuaternionSafeLookRotation(vector), c.gameObject, c.damage * 4, 0f, crit, DamageColorIndex.Default, null, -1f);
                    ProjectileManager.instance.FireProjectile(prefab, pos, Util.QuaternionSafeLookRotation(-vector), c.gameObject, c.damage * 4, 0f, crit, DamageColorIndex.Default, null, -1f);
                    await Task.Delay(250);
                }
                prefab.transform.localScale.Set(1, 1, 1);
            }
        }

        static async public void spawnWallEvil()
        {
            try
            {
                var c = findRandomMonster();
                if (c != null)
                {
                    MessageHandler.globalMessage("Curse these stupid WALLS.");
                    var prefab = EntityStates.Mage.Weapon.PrepWall.projectilePrefab;
                    playAll(1161093638);
                    for (int i = 0; i < 20; i++)
                    {
                        Vector3 forward = Vector3.forward * new Random().Next(0, 360);
                        forward.y = 0f;
                        forward.Normalize();
                        Vector3 vector = Vector3.Cross(Vector3.up, forward);
                        bool crit = Util.CheckRoll(c.crit, c.master);
                        Vector3 pos = findRandomPosGround();
                        prefab.transform.localScale.Set(5, 5, 5);
                        ProjectileManager.instance.FireProjectile(prefab, pos, Util.QuaternionSafeLookRotation(vector), c.gameObject, c.damage * 4, 0f, crit, DamageColorIndex.Default, null, -1f);
                        ProjectileManager.instance.FireProjectile(prefab, pos, Util.QuaternionSafeLookRotation(-vector), c.gameObject, c.damage * 4, 0f, crit, DamageColorIndex.Default, null, -1f);
                        await Task.Delay(250);
                    }
                    prefab.transform.localScale.Set(1, 1, 1);
                }
            }
            catch { }
        }

        static async public void randomTransformation(uint targetID)
        {
            var c = grabUser(targetID);
            if (c != null)
            {
                String[] options2 = { "cscBeetle", "cscBeetleGuard", "cscBeetleQueen", "cscBell", "cscBrother", "cscClayBoss", "cscClayBruiser",
            "cscElectricWorm", "cscGolem", "cscGravekeeper", "cscGreaterWisp", "cscHermitCrab", "cscImp", "cscImpBoss", "cscJellyfish"
            , "cscLemurian", "cscLemurianBruiser", "cscLesserWisp", "cscLunarExploder", "cscLunarGolem", "cscLunarWisp", "cscMagmaWorm",
            "cscMiniMushroom", "cscNullifier", "cscParent", "cscParentPod", "cscRoboBallBoss", "cscRoboBallGreenBuddy", "cscRoboBallRedBuddy",
            "cscScav", "cscScavLunar", "cscSquidTurret", "cscSuperRoboBallBoss", "cscTitanGold", "cscVagrant", "cscVulture",
            "cscGrandparent", "cscBackupDrone", "cscArchWisp"};

                String[] options = { "WispSoulBody", "Bandit2Body", "BeetleBody", "BeetleGuardBody", "ClayBruiserBody", "CommandoBody", "CrocoBody",
                "BeetleQueen2Body", "BellBody", "BisonBody", "BrotherBody", "BrotherHurtBody", "BrotherHauntBody", "CaptainBody", "ClayBossBody",
                "Drone1Body", "Drone2Body", "FlameDroneBody", "MegaDroneBody", "MissileDroneBody", "ElectricWormBody", "EngiBody", "EngiWalkerTurretBody"
            , "GolemBody", "GrandParentBody", "GreaterWispBody", "HermitCrabBody", "HuntressBody", "ImpBody", "ImpBossBody", "JellyfishBody",
            "LemurianBody", "LoaderBody", "LunarExploderBody", "LunarGolemBody", "LunarWispBody", "MageBody", "MagmaWormBody", "MercBody", "MiniMushroomBody",
            "ParentBody", "RoboBallBossBody", "RoboBallMiniBody", "SuperRoboBallBossBody", "ScavBody", "ScavLunar1Body", "TitanBody", "TitanGoldBody",
            "ToolbotBody", "VagrantBody", "VultureBody", "WispBody", "Assassin2Body", "ClayGrenadierBody", "FlyingVerminBody", "RailgunnerBody", "VoidSurvivorBody"
            , "AssassinBody", "BomberBody"};

                String randoMob = options[r.Next(0, options.Length)];

                for (int i = 0; i < RoR2.NetworkUser.instancesList.Count; i++)
                {
                    var user = RoR2.NetworkUser.instancesList[i];
                    if (user.GetCurrentBody().netId == c.netId && !isTransformed(user.netId.Value)) // if the body is our target, with the applied net user
                    {
                        var tp = new transformedPlayer();
                        var transformBack = user.GetCurrentBody().bodyIndex;
                        tp.originalIndex = transformBack;
                        tp.userID = user.netId.Value;
                        transformedPlayers.Add(tp);
                        Random r = new Random();
                        var n = r.Next(3, 60);
                        MessageHandler.globalMessage(c.GetUserName() + " has undergone a transformation for " + n + " seconds!");

                        user.SetBodyPreference(BodyCatalog.FindBodyIndex(randoMob));
                        c.master.Respawn(c.footPosition, c.transform.rotation);
                        await Task.Delay(1000 * n);
                        if (getTransformBack(user.netId.Value) != BodyIndex.None)
                        {
                            user.SetBodyPreference(getTransformBack(user.netId.Value));
                            c.master.Respawn(RoR2.NetworkUser.instancesList[i].GetCurrentBody().footPosition, RoR2.NetworkUser.instancesList[i].transform.rotation);
                            removeTransformed(user.netId.Value);
                        }
                        else
                        {
                            Debug.Log("(Artifact of Entropy) Transform Back Failed! Survivor is stuck as a transformed Object!!!");
                        }
                    }
                }
            }
            //c.master.Respawn(c.footPosition, c.gameObject.transform.rotation);
            //Debug.Log("10");
        }

        public static void levelUpPlayer(uint targetID)
        {
            var c = grabUser(targetID);
            if (c != null)
            {
                MessageHandler.globalMessage(c.GetUserName() + " has received a bunch of exp!");
                ExperienceManager.instance.AwardExperience(c.aimOrigin, c, (ulong)c.experience * (ulong)c.level + 150);
            }
        }

        public static void giveVoidItem(uint targetID)
        {
            var c = grabUser(targetID);
            if (c != null)
            {
                MessageHandler.globalMessage(c.GetUserName() + " has received a void item!");
                Random rnd = new Random();
                int index = rnd.Next(1, 15);
                ItemIndex dex = ItemIndex.None;
                switch (index)
                {
                    case 1:
                        dex = DLC1Content.Items.VoidMegaCrabItem.itemIndex;
                        break;
                    case 2:
                        dex = DLC1Content.Items.BearVoid.itemIndex;
                        break;
                    case 3:
                        dex = DLC1Content.Items.BleedOnHitVoid.itemIndex;
                        break;
                    case 4:
                        dex = DLC1Content.Items.ChainLightningVoid.itemIndex;
                        break;
                    case 5:
                        dex = DLC1Content.Items.CloverVoid.itemIndex;
                        break;
                    case 6:
                        dex = DLC1Content.Items.TreasureCacheVoid.itemIndex;
                        break;
                    case 7:
                        dex = DLC1Content.Items.SlowOnHitVoid.itemIndex;
                        break;
                    case 8:
                        dex = DLC1Content.Items.CritGlassesVoid.itemIndex;
                        break;
                    case 9:
                        dex = DLC1Content.Items.MissileVoid.itemIndex;
                        break;
                    case 11:
                        dex = DLC1Content.Items.ElementalRingVoid.itemIndex;
                        break;
                    case 12:
                        dex = DLC1Content.Items.EquipmentMagazineVoid.itemIndex;
                        break;
                    case 13:
                        dex = DLC1Content.Items.ExplodeOnDeathVoid.itemIndex;
                        break;
                    case 14:
                        dex = DLC1Content.Items.ExtraLifeVoid.itemIndex;
                        break;
                }
                c.inventory.GiveItem(dex);
                MessageHandler.globalItemGetMessage(c, dex, 4);
            }
        }


        public static bool isTransformed(uint netID) // is the player currently not in their original form?
        {
            for(int i = 0; i < transformedPlayers.Count; i++)
            {
                if(transformedPlayers[i].userID == netID)
                {
                    return true;
                }
            }
            return false;
        }

        public static BodyIndex getTransformBack(uint netID)
        {
            for (int i = 0; i < transformedPlayers.Count; i++)
            {
                if (transformedPlayers[i].userID == netID)
                {
                    return transformedPlayers[i].originalIndex;
                }
            }
            return BodyIndex.None;
        }

        public static void removeTransformed(uint netID)
        {
            for (int i = 0; i < transformedPlayers.Count; i++)
            {
                if (transformedPlayers[i].userID == netID)
                {
                    transformedPlayers.RemoveAt(i);
                    return;
                }
            }
        }

        static public void randomTransformationGang(uint targetID)
        {
            var c = grabUser(targetID);
            if (c != null)
            {
                randomTransformSubset(c);
            }
        }

        static public CharacterBody grabUser(uint targetID) // get body to affect using UID info
        { 
            for(int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++)
            {
                NetworkUser n = NetworkUser.readOnlyInstancesList[i];
                if (n != null)
                {
                    CharacterBody c = NetworkUser.readOnlyInstancesList[i].GetCurrentBody();
                    if (c != null && c.networkIdentity.netId.Value == targetID && c.master.hasBody == true)
                    {
                        return c;
                    }
                }
            }

            //MessageHandler.globalMessage("Error: issue locating target body! (Grabuser)");
            return null; 
        }

        static public GameObject spawnRandomMonster()
        {
            String[] options = { "cscBeetle", "cscBeetleGuard", "cscBeetleQueen", "cscBell", "cscBrother", "cscClayBoss", "cscClayBruiser",
            "cscElectricWorm", "cscGolem", "cscGravekeeper", "cscGreaterWisp", "cscHermitCrab", "cscImp", "cscImpBoss", "cscJellyfish"
            , "cscLemurian", "cscLemurianBruiser", "cscLunarExploder", "cscLunarGolem", "cscLunarWisp", "cscMagmaWorm",
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
            if (monsters.Count != 0)
            {
                int randomMonster = Math.Max((int)(Math.Floor((monsters.Count * r.NextDouble()) - 1)), 0);
                var m = monsters[randomMonster];

                return m.body;
            }
            else
            {
                return null;
            }
        }

        // play sound to ALL player locations
        private static void playAll(uint uid)
        {
            if (NetworkServer.active)
            {
                // local sound playing
                for (int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++)
                {
                    var controller = NetworkUser.readOnlyInstancesList[i];
                    if (controller != null && controller.GetCurrentBody() != null 
                        && controller.GetCurrentBody().gameObject != null && controller.GetCurrentBody().networkIdentity != null
                        && controller.GetCurrentBody().networkIdentity.netId != null)
                    {
                        AkSoundEngine.PostEvent(uid, controller.GetCurrentBody().gameObject);

                        // play sound for all clients
                        new networkBehavior.Playsound(uid, controller.GetCurrentBody().networkIdentity.netId.Value).Send(R2API.Networking.NetworkDestination.Clients);
                    }
                }
            }
            }
        public struct transformedPlayer
        {
            public BodyIndex originalIndex;
            public uint userID;
        }
    }
}

