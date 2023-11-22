using RimWorld;
using Verse;

namespace MultiOrdnanceFramework
{
    [DefOf]
    public static class MOF_DefOf
    {
        static MOF_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MOF_DefOf));
        }

        [MayRequireRoyalty]
        public static ThingDef MOFBombardment;
    }
}