using UnityEngine.Networking;
using RoR2;
using R2API.Networking.Interfaces;
using UnityEngine;
using Messenger;
using System.Collections.Generic;

namespace ArtifactsOfDoom
{
    public class deathStorage
    {
        static List<List<ItemIndex>> chest = new List<List<ItemIndex>>(24);
        static List<string> usernames = new List<string>(24);
        // initialize death storage
        static public void init()
        {
            Debug.Log("DeathStorage Init...");

            foreach (NetworkUser user in NetworkUser.readOnlyInstancesList)
            {
                usernames.Add(user.userName);

                List<ItemIndex> box = new List<ItemIndex>();
                chest.Add(box);

                foreach (string name in usernames)
                {
                    Debug.Log("Added " + name + " to deathstorage");
                }
            }
            chest.Clear();
            chest.TrimExcess();
        }

        // Called when a user should get an item on death
        static public void deathUpdate(ItemIndex item, int index)
        {
            chest[index].Add(item);
        }

        // Called on scene change, gives items to dead players
        static public void regenerateItems()
        {
            Debug.Log("Regenerating Items...");
            for (int i = 0; i < chest.Count; i++)
            {
                string usernameOfDead = usernames[i];
                List<ItemIndex> playerChest = chest[i];

                // find the player that matches the username
                int index = 0;
                for (int x = 0; x < NetworkUser.readOnlyInstancesList.Count; x++)
                {
                    if (NetworkUser.readOnlyInstancesList[x].userName.CompareTo(usernameOfDead) == 0)
                    {
                        index = x;

                    }
                }

                if (NetworkUser.readOnlyInstancesList[index].GetCurrentBody().inventory == null)
                {
                    Debug.Log("Issue finding body to give to! (DeathStorage)");
                }
                else
                {
                    //Debug.Log("body found (DeathStorage)");
                    foreach (ItemIndex dex in playerChest)
                    {
                        Debug.Log("Giving " + PickupCatalog.FindPickupIndex(dex).pickupDef.nameToken + " to " + NetworkUser.readOnlyInstancesList[index].userName);
                        NetworkUser.readOnlyInstancesList[index].GetCurrentBody().inventory.GiveItem(PickupCatalog.FindPickupIndex(dex).itemIndex);
                        MessageHandler.globalItemGetMessage(NetworkUser.readOnlyInstancesList[index].GetCurrentBody(), dex, 0);
                    }
                    //Debug.Log("Loop End");
                    // now clear the list of items
                    playerChest.Clear();
                    playerChest.TrimExcess();
                }
            }
        }

        static public bool containsName(string name)
        {
            return usernames.Contains(name);
        }

        // get username of user from death storage
        static public string retrieveUsername(int index)
        {
            return NetworkUser.readOnlyInstancesList[index].userName;
        }
    }
}