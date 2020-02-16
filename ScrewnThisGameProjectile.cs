using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ScrewThisGame
{
    public class ScrewThisGameProjectile : ModProjectile
    {
        int iType;
        public ScrewThisGameProjectile(int type)
        {
            iType = type;
        }

        public override void SetDefaults()
        {
            projectile.CloneDefaults(iType);
            projectile.hostile = true;
        }
    }
}