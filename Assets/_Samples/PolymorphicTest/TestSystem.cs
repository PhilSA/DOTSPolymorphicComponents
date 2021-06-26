using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


public class TestSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((Entity entity, ref MyPolyComponent polyComp, ref Translation translation, ref Rotation rotation) =>
        {
            polyComp.Update(deltaTime, ref translation, ref rotation);
        }).Schedule();
    }
}