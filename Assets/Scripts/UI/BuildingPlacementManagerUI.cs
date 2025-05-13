using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacementManagerUI : MonoBehaviour
{
    [SerializeField] private RectTransform buttonContainer;
    [SerializeField] private RectTransform buttonTemplate;
    [SerializeField] private BuildingDataListSO buildingDataListSO;

    private Dictionary<BuildingDataSO, BuildingPlacementManagerUI_SingleButton> buttonDictionary;

    private void Awake()
    {
        buttonTemplate.gameObject.SetActive(false);
        buttonDictionary = new Dictionary<BuildingDataSO, BuildingPlacementManagerUI_SingleButton>();

        foreach(BuildingDataSO buildingDataSO in buildingDataListSO.buildingDataSOList)
        {
            if (!buildingDataSO.shouldShowInBuildingPlacementManager)
            {
                continue;
            }

            Transform buttonTemplateTransform = Instantiate(buttonTemplate, buttonContainer);
            buttonTemplateTransform.gameObject.SetActive(true);

            BuildingPlacementManagerUI_SingleButton buildingPlacementManagerUI_SingleButton
                = buttonTemplateTransform.GetComponent<BuildingPlacementManagerUI_SingleButton>();
            buttonDictionary[buildingDataSO] = buildingPlacementManagerUI_SingleButton;
            buildingPlacementManagerUI_SingleButton.Setup(buildingDataSO);
        }
    }

    private void Start()
    {
        BuildingPlacementManager.Instance.OnActiveBuildingDataSOChanged += BuildingPlacementManager_OnActiveBuildingDataSOChanged;

        UpdateSelectedVisual();
    }

    private void BuildingPlacementManager_OnActiveBuildingDataSOChanged(object sender, System.EventArgs e)
    {
        UpdateSelectedVisual();
    }

    private void UpdateSelectedVisual()
    {
        foreach(BuildingDataSO buildingDataSO in buttonDictionary.Keys)
        {
            buttonDictionary[buildingDataSO].HideVisual();
        }

        BuildingDataSO activeBuildingDataSO = BuildingPlacementManager.Instance.GetActiveBuildingDataSO();
        buttonDictionary[activeBuildingDataSO].ShowVisual();
    }
}
