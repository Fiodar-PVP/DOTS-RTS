using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class DOTSEventManager : MonoBehaviour
{
    public event EventHandler OnBuildingBarrackUnitQueueChanged;
    public event EventHandler OnBuildingHQDead;
    public static DOTSEventManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void TriggerOnBuildinBarrackUnitQueueChanged(NativeList<Entity> entityList)
    {
        foreach (Entity entity in entityList)
        {
            OnBuildingBarrackUnitQueueChanged.Invoke(entity, EventArgs.Empty);
        }
    }

    public void TriggerOnBuildingHQDead()
    {
        OnBuildingHQDead?.Invoke(this, EventArgs.Empty);
    }
}
