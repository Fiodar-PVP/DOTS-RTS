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

        AddResourceAmount(ResourceType.Iron, 100);
        AddResourceAmount(ResourceType.Gold, 100);
        AddResourceAmount(ResourceType.Oil, 100);
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

    public bool CanSpendResourceAmount(ResourceAmount resourceAmount)
    {
        return resourceTypeAmountDictionary[resourceAmount.resourceType] >= resourceAmount.amount;
    }

    public bool CanSpendResourceAmount(ResourceAmount[] resourceAmountarray)
    {
        for(int i = 0; i < resourceAmountarray.Length; i++)
        {
            if (resourceTypeAmountDictionary[resourceAmountarray[i].resourceType] < resourceAmountarray[i].amount)
            {
                return false;
            }
        }

        return true;
    }

    public void SpendResourceAmount(ResourceAmount resourceAmount)
    {
        resourceTypeAmountDictionary[resourceAmount.resourceType] -= resourceAmount.amount;

        OnResourceAmountChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SpendResourceAmount(ResourceAmount[] resourceAmountArray)
    {
        for (int i = 0; i < resourceAmountArray.Length; i++)
        {
            resourceTypeAmountDictionary[resourceAmountArray[i].resourceType] -= resourceAmountArray[i].amount;
        }

        OnResourceAmountChanged?.Invoke(this, EventArgs.Empty);
    }
}
