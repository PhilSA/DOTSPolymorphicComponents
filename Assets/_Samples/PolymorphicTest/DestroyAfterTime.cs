using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct DestroyAfterTime : IComponentData
{
    public float Time;
}

public class DestroyAfterTimeSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float dt = Time.DeltaTime;
        EntityCommandBuffer ecb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer();

        Entities.ForEach((Entity entity, ref DestroyAfterTime destroyAfterTime) =>
        {
            destroyAfterTime.Time -= dt;

            if(destroyAfterTime.Time <= 0f)
            {
                ecb.DestroyEntity(entity);
            }

        }).Schedule();

        World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().AddJobHandleForProducer(Dependency);
    }
}