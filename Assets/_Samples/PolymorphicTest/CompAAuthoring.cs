using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class CompAAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public CompA CompA;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new MyPolyComponent { CompA = CompA, TypeId = MyPolyComponent.ComponentType.CompA });
    }
}
