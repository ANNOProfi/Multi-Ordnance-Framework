using System;
using System.Collections.Generic;
using System.Xml;
using Verse;

namespace ShieldbreakerPermits
{
    public class SP_ExplosionThings
    {
        public DamageDef damage;

        public ThingDef explosionThing;

        /*public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "damage", xmlRoot.Name, null, null, null);
		}*/
    }
}