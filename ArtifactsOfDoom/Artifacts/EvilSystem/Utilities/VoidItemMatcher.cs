using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace csProj.Artifacts.EvilSystem
{
    class VoidItemMatcher
    {
        // could potentially conflict with non DLC players. Fix?

        public static ItemIndex[] voidItems = {RoR2.DLC1Content.Items.BleedOnHitVoid.itemIndex, RoR2.DLC1Content.Items.ChainLightningVoid.itemIndex, RoR2.DLC1Content.Items.CloverVoid.itemIndex, 
            RoR2.DLC1Content.Items.CritGlassesVoid.itemIndex, RoR2.DLC1Content.Items.ElementalRingVoid.itemIndex, RoR2.DLC1Content.Items.EquipmentMagazineVoid.itemIndex, RoR2.DLC1Content.Items.ExplodeOnDeathVoid.itemIndex,
        RoR2.DLC1Content.Items.ExtraLifeVoid.itemIndex, RoR2.DLC1Content.Items.MissileVoid.itemIndex, RoR2.DLC1Content.Items.MushroomVoid.itemIndex, RoR2.DLC1Content.Items.SlowOnHitVoid.itemIndex, RoR2.DLC1Content.Items.TreasureCacheVoid.itemIndex,
        RoR2.DLC1Content.Items.VoidmanPassiveItem.itemIndex, RoR2.DLC1Content.Items.VoidMegaCrabItem.itemIndex};
 
        public static bool isVoidItem(ItemIndex item)
        {
            for(int i = 0; i < voidItems.Length; i++)
            {
                ItemIndex dex = voidItems[i];
                if(dex == item)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
