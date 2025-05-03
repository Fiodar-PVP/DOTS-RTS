using UnityEngine;

[CreateAssetMenu()]
public class AnimationDataSO : ScriptableObject
{
    public AnimationType animationType;
    public Mesh[] meshArray;
    public float frameTimerMax;
}
