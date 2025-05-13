using UnityEngine;
using UnityEngine.UI;

public class BuildingPlacementManagerUI_SingleButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image selectedVisual;

    public void Setup(BuildingDataSO buildingDataSO)
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            BuildingPlacementManager.Instance.SetActiveBuildingDataSO(buildingDataSO);
        });

        iconImage.sprite = buildingDataSO.sprite;
    }

    public void ShowVisual()
    {
        selectedVisual.enabled = true;
    }

    public void HideVisual()
    {
        selectedVisual.enabled = false;
    }
}
