using RimWorld;
using Verse;

namespace ShieldbreakerPermits
{
    [DefOf]
    public static class SP_DefOf
    {
        static SP_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SP_DefOf));
        }

        [MayRequireRoyalty]
        public static ThingDef ShieldbreakerBombardment;
    }
}