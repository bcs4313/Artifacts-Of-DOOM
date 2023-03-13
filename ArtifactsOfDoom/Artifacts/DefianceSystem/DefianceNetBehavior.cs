using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BepInEx;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ArtifactGroup
{
	[BepInDependency("com.bepis.r2api")]
	public class DefianceNetBehavior : BaseUnityPlugin
	{
		public static CharacterBody grabUser(uint targetID)
		{
			for (int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++)
			{
				NetworkUser networkUser = NetworkUser.readOnlyInstancesList[i];
				bool flag;
				if (networkUser != null && networkUser.GetCurrentBody() != null && networkUser.GetCurrentBody().networkIdentity != null)
				{
					NetworkInstanceId netId = networkUser.GetCurrentBody().networkIdentity.netId;
					flag = true;
				}
				else
				{
					flag = false;
				}
				bool flag2 = flag;
				if (flag2)
				{
					CharacterBody currentBody = NetworkUser.readOnlyInstancesList[i].GetCurrentBody();
					bool flag3 = currentBody.networkIdentity.netId.Value == targetID;
					if (flag3)
					{
						return currentBody;
					}
				}
			}
			Debug.Log("MessageHandler: issue locating target body! Grabuser()");
			return NetworkUser.readOnlyInstancesList[0].GetCurrentBody();
		}

		private static CharacterMaster getLocalMaster(uint UID)
		{
			try
			{
				bool flag = NetworkUser.readOnlyInstancesList != null;
				if (flag)
				{
					for (int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++)
					{
						NetworkUser networkUser = NetworkUser.readOnlyInstancesList[i];
						bool flag2 = networkUser.netId.Value == UID;
						if (flag2)
						{
							return NetworkUser.readOnlyInstancesList[i].master;
						}
					}
				}
			}
			catch
			{
				Debug.Log("(Artifact of Defiance) UID retrieval failed, hook aborted. (This is very likely a bad thing for this artifact).");
			}
			return null;
		}

		private static CharacterMaster getMonsterMaster(uint UID)
		{
			ReadOnlyCollection<CharacterMaster> readOnlyInstancesList = CharacterMaster.readOnlyInstancesList;
			bool flag = readOnlyInstancesList != null;
			if (flag)
			{
				for (int i = 0; i < readOnlyInstancesList.Count; i++)
				{
					CharacterMaster characterMaster = readOnlyInstancesList[i];
					bool flag2 = characterMaster.netId.Value == UID;
					if (flag2)
					{
						return characterMaster;
					}
				}
			}
			else
			{
				Debug.Log("Client Failure: masterList is NULL. (Artifact of Defiance)");
			}
			return null;
		}

		public class Playsound : INetMessage, ISerializableObject
		{
			public void Deserialize(NetworkReader reader)
			{
				this.soundID = reader.ReadUInt32();
				this.universalID = reader.ReadUInt32();
			}

			public Playsound()
			{
			}

			public Playsound(uint _soundID, uint _universalID)
			{
				this.soundID = _soundID;
				this.universalID = _universalID;
			}

			public void OnReceived()
			{
				IEnumerable<CharacterBody> enumerable = from x in TeamComponent.GetTeamMembers(TeamIndex.Player)
														select x.body;
				bool flag = enumerable == null;
				if (flag)
				{
					Debug.Log("Master is null... aborting");
				}
				else
				{
					foreach (CharacterBody characterBody in enumerable)
					{
						bool flag2 = characterBody.networkIdentity.netId.Value == this.universalID;
						if (flag2)
						{
							AkSoundEngine.PostEvent(this.soundID, characterBody.gameObject);
						}
					}
				}
			}

			public void Serialize(NetworkWriter writer)
			{
				writer.Write(this.soundID);
				writer.Write(this.universalID);
			}

			private uint soundID;

			private uint universalID;
		}

		public class clientSyncSettings : INetMessage, ISerializableObject
		{
			public void Deserialize(NetworkReader reader)
			{
				this.clientLight = reader.ReadBoolean();
				this.clientOneShot = reader.ReadBoolean();
				this.clientExtraLevels = reader.ReadDouble();
				this.clientLevelMultiplier = reader.ReadDouble();
			}

			public clientSyncSettings()
			{
			}

			public clientSyncSettings(bool _clientLight, bool _clientOneShot, float _clientExtraLevels, float _clientLevelMultiplier)
			{
				this.clientLight = _clientLight;
				this.clientOneShot = _clientOneShot;
				this.clientExtraLevels = (double)_clientExtraLevels;
				this.clientLevelMultiplier = (double)_clientLevelMultiplier;
			}

			public void OnReceived()
			{
				Debug.Log("Artifact of Defiance: Adjusting settings to match the host player.");
				ArtifactOfDefiance.clientLight = this.clientLight;
				ArtifactOfDefiance.clientOneShot = this.clientOneShot;
				ArtifactOfDefiance.clientExtraLevels = (float)this.clientExtraLevels;
				ArtifactOfDefiance.clientLevelMultiplier = (float)this.clientLevelMultiplier;
			}

			public void Serialize(NetworkWriter writer)
			{
				writer.Write(this.clientLight);
				writer.Write(this.clientOneShot);
				writer.Write(this.clientExtraLevels);
				writer.Write(this.clientLevelMultiplier);
			}

			// Token: 0x040000D3 RID: 211
			private bool clientLight;

			// Token: 0x040000D4 RID: 212
			private bool clientOneShot;

			// Token: 0x040000D5 RID: 213
			private double clientExtraLevels;

			// Token: 0x040000D6 RID: 214
			private double clientLevelMultiplier;
		}

		// Token: 0x02000021 RID: 33
		public class updateTargetMaster : INetMessage, ISerializableObject
		{
			public void Deserialize(NetworkReader reader)
			{
				Debug.Log("Deserializing...");
				this.playerUID = reader.ReadUInt32();
				this.monsterUID = reader.ReadUInt32();
			}

			public updateTargetMaster()
			{
			}

			public updateTargetMaster(uint _playerUID, uint _monsterUID)
			{
				this.playerUID = _playerUID;
				this.monsterUID = _monsterUID;
			}

			public void OnReceived()
			{
				Debug.Log("Artifact of Defiance: Processing net message::: ");
				CharacterMaster localMaster = DefianceNetBehavior.getLocalMaster(this.playerUID);
				string str = "1::: ";
				CharacterMaster characterMaster = localMaster;
				Debug.Log(str + ((characterMaster != null) ? characterMaster.ToString() : null));
				bool flag = localMaster != null;
				if (flag)
				{
					Debug.Log("Artifact of Defiance: Retrieving monster master object");
					CharacterMaster monsterMaster = DefianceNetBehavior.getMonsterMaster(this.monsterUID);
					bool flag2 = monsterMaster != null;
					if (flag2)
					{
						Debug.Log("Artifact of Defiance: Found master object!");
						Debug.Log("Artifact of Defiance: Changing prefab + inventory of player");
						localMaster.bodyPrefab = monsterMaster.bodyPrefab;
						localMaster.inventory = monsterMaster.inventory;
					}
					else
					{
						Debug.Log("Artifact of Defiacnce: Missing master object...");
					}
				}
				else
				{
					Debug.Log("Artifact of Defiance: Issue with buffing player monster. Master retrieved is null.");
				}
			}

			public void Serialize(NetworkWriter writer)
			{
				Debug.Log("Serializing...");
				writer.Write(this.playerUID);
				writer.Write(this.monsterUID);
			}

			private uint playerUID;

			private uint monsterUID;
		}
		public class notifyServerOfClientLoad : INetMessage, ISerializableObject
		{
			public void Deserialize(NetworkReader reader)
			{
				this.UID = reader.ReadUInt32();
			}

			public notifyServerOfClientLoad()
			{
			}

			public notifyServerOfClientLoad(uint _UID)
			{
				this.UID = _UID;
			}

			public void OnReceived()
			{
				Debug.Log("Artifact of Defiacnce: Received Client Scene Load MSG: uid = " + this.UID.ToString());
				bool flag = !NetworkServer.active;
				if (flag)
				{
					Debug.Log("Artifact of Defiacnce: Received INVALID SCENE LOAD UPDATE! (Clientside)");
				}
				else
				{
					ArtifactOfDefiance.tm.playerLoaded(this.UID);
					bool flag2 = ArtifactOfDefiance.tm.allMonstersReady();
					if (flag2)
					{
						Debug.Log("Artifact of Defiance: All clients are loaded into the server. Reactivating artifact!");
						ArtifactOfDefiance.loadingScene = false;
					}
				}
			}

			public void Serialize(NetworkWriter writer)
			{
				writer.Write(this.UID);
			}

			private uint UID;
		}

		public class joinDefianceTeam : INetMessage, ISerializableObject
		{
			public void Deserialize(NetworkReader reader)
			{
				this.UID = reader.ReadUInt32();
				this.team = reader.ReadString();
			}

			public joinDefianceTeam()
			{
			}

			public joinDefianceTeam(uint _UID, string _team)
			{
				this.UID = _UID;
				this.team = _team;
			}

			public void OnReceived()
			{
				bool flag = this.team.Equals("Monster");
				if (flag)
				{
					ArtifactOfDefiance.tm.joinMonsters(this.UID);
				}
				else
				{
					ArtifactOfDefiance.tm.joinHumans(this.UID);
				}
			}

			public void Serialize(NetworkWriter writer)
			{
				writer.Write(this.UID);
				writer.Write(this.team);
			}

			private string team;

			private uint UID;
		}
	}
}
