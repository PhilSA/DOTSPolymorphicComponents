using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class CompBAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public CompB CompB;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new MyPolyComponent(CompB));
    }
}
