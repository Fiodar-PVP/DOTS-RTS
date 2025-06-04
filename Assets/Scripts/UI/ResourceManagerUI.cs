using System.Collections.Generic;
using UnityEngine;

public class ResourceManagerUI : MonoBehaviour
{
    [SerializeField] private Transform resourceContainer;
    [SerializeField] private Transform resourceTemplate;
    [SerializeField] private ResourceDataListSO resourceDataListSO;

    private Dictionary<ResourceType, ResourceManagerUI_Single> resourceTypeSingleUIDict;

    private void Start()
    {
        resourceTypeSingleUIDict = new Dictionary<ResourceType, ResourceManagerUI_Single>();

        Setup();
        UpdateVisual();

        ResourceManager.Instance.OnResourceAmountChanged += ResourceManager_OnResourceAmountChanged;
    }

    private void ResourceManager_OnResourceAmountChanged(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }

    private void Setup()
    {
        resourceTemplate.gameObject.SetActive(false);

        foreach(Transform child in resourceContainer)
        {
            if(child == resourceTemplate)
            {
                continue;
            }

            Destroy(child.gameObject);
        }

        foreach(ResourceDataSO resourceDataSO in resourceDataListSO.resourceDataSOList)
        {
            Transform resourceTransform = Instantiate(resourceTemplate, resourceContainer);
            resourceTransform.gameObject.SetActive(true);
            ResourceManagerUI_Single resourceManagerUI_Single = resourceTransform.GetComponent<ResourceManagerUI_Single>();
            resourceManagerUI_Single.Setup(resourceDataSO);
            resourceTypeSingleUIDict[resourceDataSO.resourceType] = resourceManagerUI_Single;
        }
    }

    private void UpdateVisual()
    {
        foreach (ResourceDataSO resourceDataSO in resourceDataListSO.resourceDataSOList)
        {
            int currentAmount = ResourceManager.Instance.GetResourceAmount(resourceDataSO.resourceType);
            resourceTypeSingleUIDict[resourceDataSO.resourceType].UpdateAmount(currentAmount);
        }
    }
}
