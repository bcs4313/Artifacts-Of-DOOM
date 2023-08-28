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

namespace ArtifactGroup
{
    /*
     * Use of this class is to generate and run random outcomes from the hooks
     * in ArtifactOfEntropy
     */
    public class EntropyHost
    {
		public static int eventTotal = 80; // total random events to handle / work with

		private List<Event> events; // holds all events, with corresponding weights, hooks, and outcomes

		public EntropyHost()
        {
			events = new List<Event>();
			for(int i = 0; i < eventTotal; i++)
            {
				Event env = new Event(i);
				env.printData(); // print debug info for event objects
				events.Add(env);
            }
			
		}

		//@param isSelective: event points an action to a particular player
		public void queueHook(String hookID, uint targetID, bool isSelective)
        {
			// iterate through events and trigger if hook outcome is approved
			for (int i = 0; i < events.Count; i++)
			{
				Event env = events[i]; // get event
				if(env.hooksAttached.Contains(hookID))
                {
					// run an outcome
					bool triggered = env.runOutcome(hookID);
					if(triggered == true)
                    {
						if (OptionsLink.AOE_SimultaneousOutcomes.Value == true)
						{
							// logically, I think its ideal to queue events with the same hook
							// together, since its more entertaining and further destabilizes chance.
							for (int z = 0; z < events.Count; z++)
							{
								Event tie = events[z];
								if (tie.hooksAttached.Contains(hookID))
								{
									runEvent(tie.eventID, targetID, isSelective);
								}
							}
						}
						else
                        {
							runEvent(events[i].eventID, targetID, isSelective);
						}
						return;
					}
                }
			}
		}

		// run an event on the entropy blaster, depending on a
		// supplied event ID
		//@param method - simply the identity number assigned to the desired method
		//@param targetID the id of the person who triggered the event
		//@param isSelective if the event points to a particular player. 
		//Allows certain events to function properly.
        public void runEvent(int method, uint targetID, bool isSelective)
        {
			switch (method)
			{
				case 0:
					if (isSelective)
					{
						entropyBlaster.makeChonky(targetID);
					}
					break;
				case 1:
					if (isSelective)
					{
						entropyBlaster.bananaPeel(targetID);
					}
					break;
				case 2:
					//entropyBlaster.oopsDuplicated();
					break;
				case 3:
					entropyBlaster.WarpEnemies();
					break;
				case 4:
					entropyBlaster.BlackHoleOfDoom();
					break;
				case 5:
					if (isSelective)
					{
						entropyBlaster.drugz(targetID);
					}
					break;
				case 6:
					if (isSelective)
					{
						entropyBlaster.freeItem(targetID);
					}
					break;
				case 7:
					entropyBlaster.shrineSpawn();
					break;
				case 8:
					if (isSelective)
					{
						entropyBlaster.perfection(targetID);
					}
					break;
				case 9:
					entropyBlaster.airStrike();
					break;
				case 10:
					if (isSelective)
					{
						entropyBlaster.makeTiny(targetID);
					}
					break;
				case 11:
					entropyBlaster.chaChing();
					break;
				case 12:
					entropyBlaster.spawnShops();
					break;
				case 13:
					if (isSelective)
					{
						entropyBlaster.amogus(targetID);
					}
					break;
				case 14:
					if (isSelective)
					{
						entropyBlaster.thunderBolt(targetID);
					}
					break;
				case 15:
					entropyBlaster.spawnRandomMonster();
					break;
				case 16:
					entropyBlaster.meteorStrike();
					break;
				case 17:
					if (isSelective)
					{
						entropyBlaster.giveLunarItem(targetID);
					}
					break;
				case 18:
					if (isSelective)
					{
						entropyBlaster.BFGFriendly(targetID);
					}
					break;
				case 19:
					if (isSelective)
                    {
						entropyBlaster.BFGEvil(targetID);

					}
					break;
				case 20:
					if(isSelective)
                    {
						entropyBlaster.thunderBoltFriendly(targetID);
					}
					break;
				case 21:
					if(isSelective)
                    {
						entropyBlaster.HelfireFriendly(targetID);
                    }
					break;
				case 22:
					entropyBlaster.HelfireEvil();
					break;
				case 23:
					if(isSelective)
                    {
						entropyBlaster.crippleWard(targetID);
					}
					break;
				case 24:
					entropyBlaster.randomMonsterSubset();
					break;
				case 25:
					entropyBlaster.quantumTunnel();
					break;
				case 26:
					if (isSelective)
					{
						entropyBlaster.nukeFriendly(targetID);
					}
					break;
				case 27:
					entropyBlaster.nukeEvil();
					break;
				case 28:
					entropyBlaster.newOutcome();
					break;
				case 29:
					if (isSelective)
					{
						entropyBlaster.randomItemSubset(targetID);
					}
					break;
				case 30:
					if (isSelective)
                    {
						entropyBlaster.kobe(targetID);
					}
					break;
				case 31:
					if(isSelective)
                    {
						entropyBlaster.ragdoll(targetID);
					}
					break;
				case 32:
					if (isSelective)
					{
						entropyBlaster.gummyFriend(targetID);
					}
					break;
				case 33:
					if (isSelective)
					{
						entropyBlaster.friendlySaw(targetID);
					}
					break;
				case 34:
					if (isSelective)
					{
						entropyBlaster.freeArmor(targetID);
					}
					break;
				case 35:
					if (isSelective)
					{
						entropyBlaster.freeVending(targetID);
					}
					break;
				case 36:
					if (isSelective)
					{
						entropyBlaster.freeHole(targetID);
					}
					break;
				case 37:
					if (isSelective)
					{
						entropyBlaster.freeMissiles(targetID);
					}
					break;
				case 38:
					if (isSelective)
					{
						entropyBlaster.freeScan(targetID);
					}
					break;
				case 39:
					if (isSelective)
					{
						entropyBlaster.fireball(targetID);
					}
					break;
				case 40:
					if (isSelective)
					{
						entropyBlaster.freeMolotov(targetID);
					}
					break;
				case 41:
					entropyBlaster.monsterWarCry();
					break;
				case 42:
					entropyBlaster.monsterDash();
					break;
				case 43:
					entropyBlaster.monsterSaw();
					break;
				case 44:
					entropyBlaster.monsterMolotov();
					break;
				case 45:
					entropyBlaster.monsterSaw();
					break;
				case 46:
					entropyBlaster.monsterArmor();
					break;
				case 47:
					entropyBlaster.legendaryChest();
					break;
				case 48:
					entropyBlaster.voidChest();
					break;
				case 49:
					if (isSelective)
					{
						entropyBlaster.confuse(targetID);
					}
					break;
				case 50:
						entropyBlaster.enemyArrowRain();
					break;
				case 51:
					if (isSelective)
					{
						entropyBlaster.friendlyArrowRain(targetID);
					}
					break;
				case 52:
					if( isSelective)
                    { // todo, make it work on client
						entropyBlaster.obliterateUI(targetID);
					}
					break;
				case 53:
					entropyBlaster.crabRaid();
					break;
				case 54:
					if(isSelective)
                    {
						entropyBlaster.jetpack(targetID);
					}
					break;
				case 55:
					if (isSelective)
					{
						entropyBlaster.freeCrit(targetID);
					}
					break;
				case 56:
					entropyBlaster.evilJetpack();
					break;
				case 57:
					if (isSelective)
					{
						//entropyBlaster.fireGolemLaserFriendly(targetID);
					}
					break;
				case 58:
					if (isSelective)
					{
						entropyBlaster.basicGolemFistFriendly(targetID);
					}
					break;
				case 59:
					if (isSelective)
                    {
						entropyBlaster.basicGolemFistEvil(targetID);
					}
					break;
				case 60:
					if (isSelective)
					{
						entropyBlaster.avengersASSEMBLE();
					}
					break;
				case 61:
					if (isSelective)
                    {
						entropyBlaster.randomTransformation(targetID);
                    }
					break;
				case 62:
					if (isSelective)
                    {
						entropyBlaster.randomTransformationGang(targetID);
                    }
					break;
				case 64:
					entropyBlaster.distortScreen();
					break;
				case 65:
					if(isSelective)
                    {
						entropyBlaster.Yummy(targetID);
                    }
					break;
				case 66:
					if(isSelective)
                    {
						entropyBlaster.friendlyVoidBlast(targetID);
                    }
					break;
				case 67:
					entropyBlaster.bloodShrineSpawn();
					break;
				case 68:
					entropyBlaster.chanceShrineSpawn();
					break;
				case 69:
					entropyBlaster.combatShrineSpawn();
					break;
				case 70:
					entropyBlaster.healingShrineSpawn();
					break;
				case 71:
					entropyBlaster.orderShrineSpawn();
					break;
				case 72:
					entropyBlaster.cleanseShrineSpawn();
					break;
				case 73:
					entropyBlaster.evilVoidBlast();
					break;
				case 74:
					if (isSelective)
					{
						entropyBlaster.megaTarBallFriendly(targetID);
					}
					break;
				case 75:
					entropyBlaster.megaTarBallEVIL();
					break;
				case 76:
					if (isSelective)
					{
						entropyBlaster.spawnWallFriendly(targetID);
					}
					break;
				case 77:
						entropyBlaster.spawnWallEvil();
					break;
				case 78:
					if (isSelective)
					{
						entropyBlaster.levelUpPlayer(targetID);
					}
					break;
				case 79:
					if (isSelective)
                    {
						entropyBlaster.giveVoidItem(targetID);
                    }
					break;
			}
		}

		// entropy blaster specific method that
		// adds a new outcome to the event pool.
		public void addRandomOutcome()
		{
			Random rnd = new Random();
			int eventTarget = rnd.Next(0, events.Count);
			Event env = events[eventTarget];
			int hookTarget = rnd.Next(0, env.availableHooks.Length);

			env.hooksAttached.Add(env.availableHooks[hookTarget]);

			// generate chance for newly generated outcome

			// roll the chance of this event to occur on the hook. 
			// made to be fairly unstable (for fun)
			double r1 = rnd.NextDouble();
			double r2 = rnd.NextDouble();
			double r3 = rnd.NextDouble();
			double bigMult = rnd.NextDouble() + env.implicitWeights[hookTarget];

			// include the implicit weight for the chance!
			env.hookChances.Add(100.0 * r1 * r2 * r3 * bigMult);
		}
	}
}
