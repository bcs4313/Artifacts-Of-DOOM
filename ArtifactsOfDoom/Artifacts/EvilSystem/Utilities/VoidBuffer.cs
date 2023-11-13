using ArtifactGroup;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace csProj.Artifacts.EvilSystem
{
    class VoidBuffer
    {
        public static void BuffBoss(CharacterBody body)
        {
            //body.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>().isDoppelganger = true;
            //body.AddBuff(DLC1Content.Buffs.EliteVoid);


            if (ArtifactOfEvil.totalVoidCoins > 250)
            {
                //body.inventory.GiveRandomEquipment();
            }
        }
    }
}
