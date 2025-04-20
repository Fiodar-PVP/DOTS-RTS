using System;
using Unity.Entities;

public enum Faction
{
    None,
    Friendly,
    Zombie,
}

public static class FactionExtension
{
    public static ComponentType GetComponentType(this Faction faction)
    {
        return faction switch
        {
            Faction.Friendly => typeof(Friendly),
            Faction.Zombie => typeof(Zombie),
            _ => throw new ArgumentOutOfRangeException()
        } ;
    }
}

public struct Friendly : IComponentData
{

}

public struct Zombie : IComponentData
{

}