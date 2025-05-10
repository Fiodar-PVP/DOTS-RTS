using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class UnitDataListSO : ScriptableObject
{
    public List<UnitDataSO> UnitDataSOList;

    public UnitDataSO GetUnitDataSO(UnitType unitType)
    {
        foreach(UnitDataSO unitDataSO in UnitDataSOList)
        {
            if(unitDataSO.unitType == unitType)
            {
                return unitDataSO;
            }
        }

        Debug.LogError("Couldn't find UnitType: " + unitType);

        return null;
    }
}
