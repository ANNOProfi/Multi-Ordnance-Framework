using System;
using System.Xml;
using UnityEngine.Android;
using Verse;

namespace ShieldbreakerPermits
{
    public class SP_ExplosionGases
    {
        public DamageDef damage;

        public GasType? explosionGas;

        /*public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "damage", xmlRoot.Name, null, null, null);
			this.explosionGas = ParseHelper.FromString<GasType?>(xmlRoot.FirstChild.Value);
		}*/
    }
}