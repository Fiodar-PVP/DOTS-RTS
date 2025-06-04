using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public event EventHandler OnResourceAmountChanged;

    public static ResourceManager Instance { get; private set; }

    [SerializeField] private ResourceDataListSO resourceDataListSO;

    private Dictionary<ResourceType, int> resourceTypeAmountDictionary;

    private void Awake()
    {
        Instance = this;

        resourceTypeAmountDictionary = new Dictionary<ResourceType, int>();

        foreach(ResourceDataSO resourceDataSO in resourceDataListSO.resourceDataSOList)
        {
            resourceTypeAmountDictionary[resourceDataSO.resourceType] = 0;
        }
    }

    public void AddResourceAmount(ResourceType resourceType, int amount)
    {
        resourceTypeAmountDictionary[resourceType] += amount;

        OnResourceAmountChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetResourceAmount(ResourceType resourceType)
    {
        return resourceTypeAmountDictionary[resourceType];
    }
}
