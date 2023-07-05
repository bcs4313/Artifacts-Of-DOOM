using System;
using UnityEngine;
using RoR2;

namespace Messenger
{
    /// <summary>
    /// Sends messages out to all clients in a Run and 
    /// Colors them.
    /// </summary>
    static class MessageHandler
    {
        // color representation in a string
        //<color=HEXCOLOR> text </color>

        // Color hexes
        const string commandoColor = "#f5ed00";
        const string huntressColor = "#ff0000";
        const string banditColor = "#861330";
        const string multColor = "#ff6600";
        const string engineerColor = "#9900ff";
        const string artificerColor = "#f5ed00";
        const string mercenaryColor = "#00fffb";
        const string rexColor = "#ff00d4";
        const string loaderColor = "#1100ff";
        const string acridColor = "#04ff00";
        const string captainColor = "#ffae00";

        const string commonColor = "#c4c4c4";
        const string uncommonColor = "#00ff3f";
        const string legendaryColor = "#ff0000";
        const string voidColor = "#8226a6";

        // Send a message to all players via API
        static public void globalMessage(String message)
        {
            R2API.Utils.ChatMessage.Send(message);
        }


        // Send a colored message to all players
        // indicating which item they received and
        // what class received it, color coded.
        static public void globalItemGetMessage(CharacterBody ch, ItemIndex dex, int rarity)
        {
            // color of charactername
            string charColor = "#ffffff";
            string rarityColor = "#ffffff";

            if (rarity == 1)
            {
                rarityColor = commonColor;
            }
            else if (rarity == 2)
            {
                rarityColor = uncommonColor;
            }
            else if (rarity == 3)
            {
                rarityColor = legendaryColor;
            }
            else
            {
                rarityColor = voidColor;
            }

            if (ch.GetDisplayName().CompareTo("Commando") == 0)
            {
                charColor = commandoColor;
            }
            else if (ch.GetDisplayName().CompareTo("Huntress") == 0)
            {
                charColor = huntressColor;
            }
            else if (ch.GetDisplayName().CompareTo("Bandit") == 0)
            {
                charColor = banditColor;
            }
            else if (ch.GetDisplayName().CompareTo("MUL-T") == 0)
            {
                charColor = multColor;
            }
            else if (ch.GetDisplayName().CompareTo("Engineer") == 0)
            {
                charColor = engineerColor;
            }
            else if (ch.GetDisplayName().CompareTo("Artificer") == 0)
            {
                charColor = artificerColor;
            }
            else if (ch.GetDisplayName().CompareTo("Mercenary") == 0)
            {
                charColor = mercenaryColor;
            }
            else if (ch.GetDisplayName().CompareTo("REX") == 0)
            {
                charColor = rexColor;
            }
            else if (ch.GetDisplayName().CompareTo("Loader") == 0)
            {
                charColor = loaderColor;
            }
            else if (ch.GetDisplayName().CompareTo("Acrid") == 0)
            {
                charColor = acridColor;
            }
            else if (ch.GetDisplayName().CompareTo("Captain") == 0)
            {
                charColor = captainColor;
            }
            else
            {
                charColor = captainColor;
            }

            //Debug.Log(ch.name + " == " + "?");

            globalMessage("<color=" + charColor + ">" + ch.GetUserName() + "</color>" +
                " has received: " + "<color=" + rarityColor + ">" +
                Language.currentLanguage.GetLocalizedStringByToken
                (PickupCatalog.FindPickupIndex(dex).pickupDef.nameToken) + "</color>"); // notify player of getting item
        }

        /// <summary>
        /// Send a message indicating a dead player has received an item.
        /// </summary>
        /// <param name="dex"></param>
        /// <param name="rarity"></param>
        static public void GlobalItemDeadMessage(ItemIndex dex, int rarity, String username)
        {
            string rarityColor = "#ffffff";
            if (rarity == 1)
            {
                rarityColor = commonColor;
            }
            else if (rarity == 2)
            {
                rarityColor = uncommonColor;
            }
            else if (rarity == 3)
            {
                rarityColor = legendaryColor;
            }
            else
            {
                rarityColor = voidColor;
            }

            globalMessage("<color=" + commonColor + ">" + username + " (dead) " + "</color>" +
                "has received: " + "<color=" + rarityColor + ">" +
                Language.currentLanguage.GetLocalizedStringByToken
                (PickupCatalog.FindPickupIndex(dex).pickupDef.nameToken) + "</color>" + "..."); // notify player of getting item
        }
    }

}