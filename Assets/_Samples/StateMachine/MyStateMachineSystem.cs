using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class MyStateMachineSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float dt = Time.DeltaTime;
        EntityCommandBuffer.ParallelWriter ecb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer().AsParallelWriter();

        Entities.ForEach((Entity entity, ref MyStateMachine myStateMachine, ref Translation translation, ref Rotation rotation) => 
        {
            myStateMachine.Update(dt, ref myStateMachine, ref translation, ref rotation, ecb);
        }).Schedule();

        World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().AddJobHandleForProducer(Dependency);
    }
}
