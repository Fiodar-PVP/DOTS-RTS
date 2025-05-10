using Unity.Entities;
using UnityEngine;

[CreateAssetMenu()]
public class UnitDataSO : ScriptableObject
{
    public UnitType unitType;
    public float progressMax;

    public Entity GetPrefabEntity(EntitiesReferences entitiesReferences)
    {
        switch(unitType)
        {
            default:
            case UnitType.None:
            case UnitType.Soldier: return entitiesReferences.soldierPrefabEntity;
            case UnitType.Scout: return entitiesReferences.scoutPrefabEntity;
            case UnitType.Zombie: return entitiesReferences.zombiePrefabEntity;
        }
    }
}
