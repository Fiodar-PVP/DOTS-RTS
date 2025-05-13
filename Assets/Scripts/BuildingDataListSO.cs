using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BuildingDataListSO : ScriptableObject
{
    public List<BuildingDataSO> buildingDataSOList;

    public BuildingDataSO none;

    public BuildingDataSO GetBuildingDataSO(BuildingType buildingType)
    {
        foreach(BuildingDataSO buildingDataSO in buildingDataSOList)
        {
            if(buildingDataSO.buildingType == buildingType)
            {
                return buildingDataSO;
            }
        }

        Debug.LogError("Couldn't find BuildingType: " + buildingType);

        return null;
    }
}
