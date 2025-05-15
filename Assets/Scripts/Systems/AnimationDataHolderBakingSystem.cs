using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
[UpdateInGroup(typeof(PostBakingSystemGroup))]
partial struct AnimationDataHolderBakingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        AnimationDataListSO animationDataListSO = null;
        foreach(RefRO<AnimationDataHolderObjectData> animationDataHolderObjectData in SystemAPI.Query<RefRO<AnimationDataHolderObjectData>>())
        {
            animationDataListSO = animationDataHolderObjectData.ValueRO.animtionDataListSO.Value;
        }

        Dictionary<AnimationType, int[]> blobAssetDataDictionary = new Dictionary<AnimationType, int[]>();
        foreach (AnimationType animationType in System.Enum.GetValues(typeof(AnimationType)))
        {
            AnimationDataSO animationDataSO = animationDataListSO.GetAnimationDataSO(animationType);
            blobAssetDataDictionary[animationType] = new int[animationDataSO.meshArray.Length];
        }

        foreach ((
            RefRO<AnimationDataHolderSubEntity> animationDataHolderSubEntity, 
            RefRO<MaterialMeshInfo> materialMeshInfo) 
            in SystemAPI.Query<
                RefRO<AnimationDataHolderSubEntity>,
                RefRO<MaterialMeshInfo>>())
        {
            blobAssetDataDictionary[animationDataHolderSubEntity.ValueRO.animationType]
                [animationDataHolderSubEntity.ValueRO.meshIndex] = materialMeshInfo.ValueRO.Mesh;
        }

        foreach(RefRW<AnimationDataHolder> animationDataHolder in SystemAPI.Query<RefRW<AnimationDataHolder>>())
        {
            BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);
            ref BlobArray<AnimationData> animationDataBlobArray = ref blobBuilder.ConstructRoot<BlobArray<AnimationData>>();

            BlobBuilderArray<AnimationData> animationDataBlobBuilderArray =
                blobBuilder.Allocate(ref animationDataBlobArray, System.Enum.GetValues(typeof(AnimationType)).Length);

            int index = 0;
            foreach (AnimationType animationType in System.Enum.GetValues(typeof(AnimationType)))
            {
                AnimationDataSO animationDataSO = animationDataListSO.GetAnimationDataSO(animationType);
                
                BlobBuilderArray<int> batchMeshArrayBlobBuilder =
                        blobBuilder.Allocate(ref animationDataBlobBuilderArray[index].intMeshArray, animationDataSO.meshArray.Length);
                
                animationDataBlobBuilderArray[index].frameMax = animationDataSO.meshArray.Length;
                animationDataBlobBuilderArray[index].frameTimerMax = animationDataSO.frameTimerMax;
                
                for (int i = 0; i < animationDataSO.meshArray.Length; i++)
                {
                    batchMeshArrayBlobBuilder[i] = blobAssetDataDictionary[animationType][i];
                }

                index++;
            }
            
            animationDataHolder.ValueRW.animationDataBlobArrayBlobAssetReference = 
                blobBuilder.CreateBlobAssetReference<BlobArray<AnimationData>>(Allocator.Domain);

            blobBuilder.Dispose();
        }
    }

    public void OnDestroy(ref SystemState state)
    {
        foreach(RefRW<AnimationDataHolder> animationDataHolder in SystemAPI.Query<RefRW<AnimationDataHolder>>())
        {
            animationDataHolder.ValueRW.animationDataBlobArrayBlobAssetReference.Dispose();
        }
    }
}
