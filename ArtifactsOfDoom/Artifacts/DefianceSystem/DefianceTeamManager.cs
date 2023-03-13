using System;
using System.Collections.Generic;
using Messenger;
using RoR2;

namespace csProj.Artifacts.Left4DeadSystem
{
	internal class DefianceTeamManager
	{
		public static List<DefianceTeamManager.player> humanPlayers;

		public static List<DefianceTeamManager.player> monsterPlayers;

		public static List<uint> controlledBodies;

		public static bool synchronizing;

		public struct player
		{
			public bool loading;
			public uint UID;
			public bool valid;
		}

		public DefianceTeamManager()
		{
			DefianceTeamManager.humanPlayers = new List<DefianceTeamManager.player>();
			DefianceTeamManager.monsterPlayers = new List<DefianceTeamManager.player>();
			DefianceTeamManager.controlledBodies = new List<uint>();
		}

		public void resetPlayers()
		{
			for (int i = 0; i < DefianceTeamManager.monsterPlayers.Count; i++)
			{
				DefianceTeamManager.player player = DefianceTeamManager.monsterPlayers[i];
				player.loading = true;
			}
		}

		public void playerLoaded(uint player)
		{
			for (int i = 0; i < DefianceTeamManager.monsterPlayers.Count; i++)
			{
				DefianceTeamManager.player player2 = DefianceTeamManager.monsterPlayers[i];
				bool flag = player2.UID == player;
				if (flag)
				{
					player2.loading = false;
				}
			}
		}

		public bool allMonstersReady()
		{
			for (int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++)
			{
				NetworkUser networkUser = NetworkUser.readOnlyInstancesList[i];
				bool flag = DefianceTeamManager.containsPMonster(networkUser.netId.Value);
				if (flag)
				{
					DefianceTeamManager.player pmonster = DefianceTeamManager.getPMonster(networkUser.netId.Value);
					bool flag2 = pmonster.valid && pmonster.loading;
					if (flag2)
					{
						return false;
					}
				}
			}
			return true;
		}

		public void joinMonsters(uint player)
		{
			while (DefianceTeamManager.containsPHuman(player))
			{
				DefianceTeamManager.removePlayer(player, DefianceTeamManager.humanPlayers);
			}
			DefianceTeamManager.player item = default(DefianceTeamManager.player);
			item.loading = false;
			item.UID = player;
			item.valid = true;
			DefianceTeamManager.monsterPlayers.Add(item);
			string userName = DefianceTeamManager.getUserName(player);
			bool flag = userName != null;
			if (flag)
			{
				MessageHandler.globalMessage(userName + " has joined the monster team!");
				CharacterBody userBody = DefianceTeamManager.getUserBody(player);
				bool flag2 = new Func<uint, CharacterBody>(DefianceTeamManager.getUserBody) != null;
				if (flag2)
				{
					userBody.healthComponent.health = -420f;
				}
			}
			else
			{
				MessageHandler.globalMessage("There was an issue retrieving the user's name (is there an issue with the connection?).");
			}
		}

		public void joinHumans(uint player)
		{
			while (DefianceTeamManager.containsPMonster(player))
			{
				DefianceTeamManager.removePlayer(player, DefianceTeamManager.monsterPlayers);
			}
			DefianceTeamManager.player item = default(DefianceTeamManager.player);
			item.loading = false;
			item.UID = player;
			item.valid = true;
			DefianceTeamManager.humanPlayers.Add(item);
			string userName = DefianceTeamManager.getUserName(player);
			bool flag = userName != null;
			if (flag)
			{
				MessageHandler.globalMessage(userName + " has joined the survivor team!");
			}
			else
			{
				MessageHandler.globalMessage("There was an issue retrieving the user's name (is there an issue with the connection?).");
			}
		}

		public static bool containsPMonster(uint playerUID)
		{
			for (int i = 0; i < DefianceTeamManager.monsterPlayers.Count; i++)
			{
				DefianceTeamManager.player player = DefianceTeamManager.monsterPlayers[i];
				bool flag = player.UID == playerUID;
				if (flag)
				{
					return true;
				}
			}
			return false;
		}

		public static DefianceTeamManager.player getPMonster(uint playerUID)
		{
			for (int i = 0; i < DefianceTeamManager.monsterPlayers.Count; i++)
			{
				DefianceTeamManager.player player = DefianceTeamManager.monsterPlayers[i];
				bool flag = player.UID == playerUID;
				if (flag)
				{
					return player;
				}
			}
			return new DefianceTeamManager.player
			{
				valid = false
			};
		}

		public static bool containsPHuman(uint playerUID)
		{
			for (int i = 0; i < DefianceTeamManager.humanPlayers.Count; i++)
			{
				DefianceTeamManager.player player = DefianceTeamManager.humanPlayers[i];
				bool flag = player.UID == playerUID;
				if (flag)
				{
					return true;
				}
			}
			return false;
		}

		public static void removePlayer(uint playerUID, List<DefianceTeamManager.player> targetList)
		{
			for (int i = 0; i < targetList.Count; i++)
			{
				DefianceTeamManager.player player = targetList[i];
				bool flag = player.UID == playerUID;
				if (flag)
				{
					targetList.RemoveAt(i);
					break;
				}
			}
		}

		public static string getUserName(uint player)
		{
			for (int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++)
			{
				NetworkUser networkUser = NetworkUser.readOnlyInstancesList[i];
				bool flag = networkUser.netId.Value == player;
				if (flag)
				{
					return networkUser.userName;
				}
			}
			return null;
		}

		public static CharacterBody getUserBody(uint player)
		{
			for (int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++)
			{
				NetworkUser networkUser = NetworkUser.readOnlyInstancesList[i];
				bool flag = networkUser.netId.Value == player;
				if (flag)
				{
					return networkUser.GetCurrentBody();
				}
			}
			return null;
		}

		public static NetworkUser getUser(uint player)
		{
			for (int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++)
			{
				NetworkUser networkUser = NetworkUser.readOnlyInstancesList[i];
				bool flag = networkUser.netId.Value == player;
				if (flag)
				{
					return networkUser;
				}
			}
			return null;
		}
	}
}
