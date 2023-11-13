using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// its a huge pain to retrieve spawncards, so this class will be able to retrieve one simply by name.
namespace csProj.Artifacts.Utilities
{
    class CardRetriever
    {
        public static SpawnCard getCard(String name)
        {
            SpawnCard[] cards = UnityEngine.Resources.FindObjectsOfTypeAll<RoR2.SpawnCard>();
            foreach(SpawnCard c in cards)
            {
                Debug.Log(c.name);
                if(c.name.Equals(name))
                {
                    return c;
                }
            }
            return null;
        }
    }
}
