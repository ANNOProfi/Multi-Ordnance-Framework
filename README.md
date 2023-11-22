# Multi Ordnance Framework

This is the github page for the Multi Ordnance Framework (hence the title).<br>
Below is a short tutorial, on how you can create your own strike permits with this framework.<br>
<br>

### Simple:

If you just want to make a few quick permits, then this is for you.<br>
A basic understanding of RimWorld XML structure is expected, although for this, it is quick to learn<br>

This is the structure of the vanilla royalty orbital strikes
```xml
<RoyalTitlePermitDef>
  <defName>CallOrbitalStrike</defName>
  <label>call aerodrone strike</label>
  <description>Call a single-impact aerodrone strike at a target position.</description>
  <workerClass>RoyalTitlePermitWorker_OrbitalStrike</workerClass>
  <minTitle>Knight</minTitle>
  <faction>Empire</faction>
  <permitPointCost>1</permitPointCost>
  <cooldownDays>45</cooldownDays>
  <uiPosition>(0,5)</uiPosition>
  <royalAid>
    <favorCost>6</favorCost>
    <targetingRange>44.9</targetingRange>
    <radius>2.9</radius>
    <explosionRadiusRange>7.9</explosionRadiusRange>
    <explosionCount>1</explosionCount>
    <intervalTicks>60</intervalTicks>
    <warmupTicks>120</warmupTicks>
  </royalAid>
</RoyalTitlePermitDef>
```
As stated, it is relatively simple to understand, with the <royalAid> section governing the most important bits.<br>
To use this framework, all you have to add to your strike is these two things:
```xml
<modExtensions>
  <li Class="MultiOrdnanceFramework.MOF_RoyalAid">
    <shellType>
      ...
    </shellType>
  </li>
</modExtensions>
```
and
```xml
<RoyalTitlePermitDef ParentName="MOF_DamageBase">
```
the latter of which, goes into the beginning of your Def.<br>
Now, you simply add whatever damage you want the strike to do, with the number of strikes of that damage and in what order like this:
```xml
<shellType>
  <damage1>x</damage1>
  <damage2>y</damage2>
</shellType>
```
This will fire of x shots with damage1 and y shots with damage2. This pattern is theoretically infinite and can support any damage, be it vanilla or modded.<br>
Be warned, the usage of the ToxGas damage type requires the Biotech DLC and some obscure damage types, that were never meant for this may give you errors about missing sounds or the like.<br>


### Advanced:

This part is a bit more advanced and deals with the addition of custom Things or Gases to be spawned on impact.<br>
Knowledge of how to patch RimWorld's XML is required for this.<br>

In the framework, you will find a Def that looks like this
```xml
<RoyalTitlePermitDef Abstract="True" Name="MOF_DamageBase">
  <modExtensions>
    <li Class="MultiOrdnanceFramework.MOF_RoyalOrdnance">
      <explosionThings>
        <li>
          <damage>Extinguish</damage>
          <explosionThing>Filth_FireFoam</explosionThing>
        </li>
      </explosionThings>
      <explosionGases>
        <li>
          <damage>Smoke</damage>
          <explosionGas>BlindSmoke</explosionGas>
        </li>
        <li MayRequire="Ludon.RimWorld.Biotech">
          <damage>ToxGas</damage>
          <explosionGas>ToxGas</explosionGas>
        </li>
      </explosionGases>
    </li>
  </modExtensions>
</RoyalTitlePermitDef>
```
This governs, what extra things are spawned, when the shell hits.<br>

To add to this, you must patch your damage and thing into it, like this:
```xml
<explosionThings>
  <li>
    <damage>yourDamage</damage>
    <explosionThing>yourThing<explosionThing>
  </li>
</explosionThings>
```
The same applies to the Gases.<br>
As long as the damage and the thing/gas are properly defined, it should accept all possible combinations. Expect wonky stuff to happen with obviously crazy stuff, like spawning mechanoids on impact.<br>

Please note: Currently, the system is not designed to spawn multiple things for the same damage type, so expect errors if you try it.
