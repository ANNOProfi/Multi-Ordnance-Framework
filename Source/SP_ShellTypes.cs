using System;
using System.Xml;
using Verse;

namespace ShieldbreakerPermits
{
    public class SP_ShellTypes
    {
        public DamageDef damage;

        public int volleySize;

        public ThingDef explosionThing = null;

        public GasType? explosionGas = null;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "damage", xmlRoot.Name, null, null, null);
			this.volleySize = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
		}
    }
}