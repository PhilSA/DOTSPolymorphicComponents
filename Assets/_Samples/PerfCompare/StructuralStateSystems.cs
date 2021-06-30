using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(BatchStateChangeSystem))]
public class StateUpdateSystem : SystemBase
{
    BatchStateChangeSystem BatchStateChangeSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        BatchStateChangeSystem = World.GetOrCreateSystem<BatchStateChangeSystem>();
    }

    protected override void OnUpdate()
    {
        var StateInitEnterEntities = BatchStateChangeSystem.StateInitEnterEntities.AsParallelWriter();
        var StateInitExitEntities = BatchStateChangeSystem.StateInitExitEntities.AsParallelWriter();
        var StateAEnterEntities = BatchStateChangeSystem.StateAEnterEntities.AsParallelWriter();
        var StateAExitEntities = BatchStateChangeSystem.StateAExitEntities.AsParallelWriter();
        var StateBEnterEntities = BatchStateChangeSystem.StateBEnterEntities.AsParallelWriter();
        var StateBExitEntities = BatchStateChangeSystem.StateBExitEntities.AsParallelWriter();
        var StateCEnterEntities = BatchStateChangeSystem.StateCEnterEntities.AsParallelWriter();
        var StateCExitEntities = BatchStateChangeSystem.StateCExitEntities.AsParallelWriter();

        float deltaTime = Time.DeltaTime;

        Entities
            .WithAll<StructuralStateInitActive>()
            .ForEach((Entity entity, int nativeThreadIndex, ref StructuralStateInit state) =>
            {
                StateInitExitEntities.Enqueue(entity);
                StateCEnterEntities.Enqueue(entity);
            }).ScheduleParallel();

        Entities
            .WithAll<StructuralStateAActive>()
            .ForEach((Entity entity, int nativeThreadIndex, ref StructuralStateA state, ref Translation translation) =>
            {
                translation.Value += state.MoveDirection * state.MoveSpeed * deltaTime;

                if (state.DurationCounter <= 0f)
                {
                    StateAExitEntities.Enqueue(entity);
                    StateBEnterEntities.Enqueue(entity);
                }
                state.DurationCounter -= deltaTime;
            }).ScheduleParallel();

        Entities
            .WithAll<StructuralStateBActive>()
            .ForEach((Entity entity, int nativeThreadIndex, ref StructuralStateB state, ref Translation translation, ref Rotation rotation) =>
            {
                rotation.Value = math.mul(rotation.Value, quaternion.Euler(math.normalizesafe(state.RotationAxis) * state.RotationSpeed * deltaTime));

                if (state.DurationCounter <= 0f)
                {
                    StateBExitEntities.Enqueue(entity);
                    StateCEnterEntities.Enqueue(entity);
                }
                state.DurationCounter -= deltaTime;
            }).ScheduleParallel();

        Entities
            .WithAll<StructuralStateCActive>()
            .ForEach((Entity entity, int nativeThreadIndex, ref StructuralStateC state, ref Translation translation) =>
            {
                translation.Value = state.StartPosition + (state.MoveDirection * math.sin(((math.PI * 2f) / state.Duration) * math.clamp(state.DurationCounter, 0f, state.Duration)) * state.MoveAmplitude);

                if (state.DurationCounter <= 0f)
                {
                    StateCExitEntities.Enqueue(entity);
                    StateAEnterEntities.Enqueue(entity);
                }
                state.DurationCounter -= deltaTime;
            }).ScheduleParallel();

        BatchStateChangeSystem.DependenciesToComplete.Add(Dependency);
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(BatchStateChangeSystem))]
[UpdateBefore(typeof(StateUpdateSystem))]
public class StateEnterSystem : SystemBase
{
    BatchStateChangeSystem BatchStateChangeSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        BatchStateChangeSystem = World.GetOrCreateSystem<BatchStateChangeSystem>();
    }

    protected override void OnUpdate()
    {
        var StateInitEnterRemoveEntities = BatchStateChangeSystem.StateInitEnterRemoveEntities.AsParallelWriter();
        var StateAEnterRemoveEntities = BatchStateChangeSystem.StateAEnterRemoveEntities.AsParallelWriter();
        var StateBEnterRemoveEntities = BatchStateChangeSystem.StateBEnterRemoveEntities.AsParallelWriter();
        var StateCEnterRemoveEntities = BatchStateChangeSystem.StateCEnterRemoveEntities.AsParallelWriter();

        Entities
            .WithAll<StructuralStateInitEnter>()
            .ForEach((Entity entity, int nativeThreadIndex, ref StructuralStateInit state) =>
            {
                StateInitEnterRemoveEntities.Enqueue(entity);
            }).ScheduleParallel();

        Entities
            .WithAll<StructuralStateAEnter>()
            .ForEach((Entity entity, int nativeThreadIndex, ref StructuralStateA state, ref RandomComp random) =>
            {
                // Flip direction
                state.MoveDirection = -state.MoveDirection;
                state.DurationCounter = random.Random.NextFloat(state.Duration * 0.5f, state.Duration * 2f);

                StateAEnterRemoveEntities.Enqueue(entity);
            }).ScheduleParallel();

        Entities
            .WithAll<StructuralStateBEnter>()
            .ForEach((Entity entity, int nativeThreadIndex, ref StructuralStateB state, ref RandomComp random) =>
            {
                state.DurationCounter = random.Random.NextFloat(state.Duration * 0.5f, state.Duration * 2f);

                StateBEnterRemoveEntities.Enqueue(entity);
            }).ScheduleParallel();

        Entities
            .WithAll<StructuralStateCEnter>()
            .ForEach((Entity entity, int nativeThreadIndex, ref StructuralStateC state, ref RandomComp random, in Translation translation) =>
            {
                state.DurationCounter = random.Random.NextFloat(state.Duration * 0.5f, state.Duration * 2f);
                state.StartPosition = translation.Value;

                StateCEnterRemoveEntities.Enqueue(entity);
            }).ScheduleParallel();

        BatchStateChangeSystem.DependenciesToComplete.Add(Dependency);
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(BatchStateChangeSystem))]
[UpdateBefore(typeof(StateEnterSystem))]
public class StateExitSystem : SystemBase
{
    BatchStateChangeSystem BatchStateChangeSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        BatchStateChangeSystem = World.GetOrCreateSystem<BatchStateChangeSystem>();
    }

    protected override void OnUpdate()
    {
        var StateInitExitRemoveEntities = BatchStateChangeSystem.StateInitExitRemoveEntities.AsParallelWriter();
        var StateAExitRemoveEntities = BatchStateChangeSystem.StateAExitRemoveEntities.AsParallelWriter();
        var StateBExitRemoveEntities = BatchStateChangeSystem.StateBExitRemoveEntities.AsParallelWriter();
        var StateCExitRemoveEntities = BatchStateChangeSystem.StateCExitRemoveEntities.AsParallelWriter();

        Entities
            .WithAll<StructuralStateInitExit>()
            .ForEach((Entity entity, int nativeThreadIndex, ref StructuralStateInit state) =>
            {
                StateInitExitRemoveEntities.Enqueue(entity);
            }).ScheduleParallel();

        Entities
            .WithAll<StructuralStateAExit>()
            .ForEach((Entity entity, int nativeThreadIndex, ref StructuralStateA state) =>
            {
                StateAExitRemoveEntities.Enqueue(entity);
            }).ScheduleParallel();

        Entities
            .WithAll<StructuralStateBExit>()
            .ForEach((Entity entity, int nativeThreadIndex, ref StructuralStateB state) =>
            {
                StateBExitRemoveEntities.Enqueue(entity);
            }).ScheduleParallel();

        Entities
            .WithAll<StructuralStateCExit>()
            .ForEach((Entity entity, int nativeThreadIndex, ref StructuralStateC state) =>
            {
                StateCExitRemoveEntities.Enqueue(entity);
            }).ScheduleParallel();

        BatchStateChangeSystem.DependenciesToComplete.Add(Dependency);
    }
}



[UpdateInGroup(typeof(SimulationSystemGroup))]
public class BatchStateChangeSystem : SystemBase
{
    public NativeQueue<Entity> StateInitEnterEntities;
    public NativeQueue<Entity> StateInitExitEntities;
    public NativeQueue<Entity> StateAEnterEntities;
    public NativeQueue<Entity> StateAExitEntities;
    public NativeQueue<Entity> StateBEnterEntities;
    public NativeQueue<Entity> StateBExitEntities;
    public NativeQueue<Entity> StateCEnterEntities;
    public NativeQueue<Entity> StateCExitEntities;

    public NativeQueue<Entity> StateInitEnterRemoveEntities;
    public NativeQueue<Entity> StateInitExitRemoveEntities;
    public NativeQueue<Entity> StateAEnterRemoveEntities;
    public NativeQueue<Entity> StateAExitRemoveEntities;
    public NativeQueue<Entity> StateBEnterRemoveEntities;
    public NativeQueue<Entity> StateBExitRemoveEntities;
    public NativeQueue<Entity> StateCEnterRemoveEntities;
    public NativeQueue<Entity> StateCExitRemoveEntities;

    public NativeList<JobHandle> DependenciesToComplete;

    protected override void OnCreate()
    {
        base.OnCreate();

        StateInitEnterEntities = new NativeQueue<Entity>(Allocator.Persistent);
        StateInitExitEntities = new NativeQueue<Entity>(Allocator.Persistent);
        StateAEnterEntities = new NativeQueue<Entity>(Allocator.Persistent);
        StateAExitEntities = new NativeQueue<Entity>(Allocator.Persistent);
        StateBEnterEntities = new NativeQueue<Entity>(Allocator.Persistent);
        StateBExitEntities = new NativeQueue<Entity>(Allocator.Persistent);
        StateCEnterEntities = new NativeQueue<Entity>(Allocator.Persistent);
        StateCExitEntities = new NativeQueue<Entity>(Allocator.Persistent);

        StateInitEnterRemoveEntities = new NativeQueue<Entity>(Allocator.Persistent);
        StateInitExitRemoveEntities = new NativeQueue<Entity>(Allocator.Persistent);
        StateAEnterRemoveEntities = new NativeQueue<Entity>(Allocator.Persistent);
        StateAExitRemoveEntities = new NativeQueue<Entity>(Allocator.Persistent);
        StateBEnterRemoveEntities = new NativeQueue<Entity>(Allocator.Persistent);
        StateBExitRemoveEntities = new NativeQueue<Entity>(Allocator.Persistent);
        StateCEnterRemoveEntities = new NativeQueue<Entity>(Allocator.Persistent);
        StateCExitRemoveEntities = new NativeQueue<Entity>(Allocator.Persistent);

        DependenciesToComplete = new NativeList<JobHandle>(Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        StateInitEnterEntities.Dispose();
        StateInitExitEntities.Dispose();
        StateAEnterEntities.Dispose();
        StateAExitEntities.Dispose();
        StateBEnterEntities.Dispose();
        StateBExitEntities.Dispose();
        StateCEnterEntities.Dispose();
        StateCExitEntities.Dispose();

        StateInitEnterRemoveEntities.Dispose();
        StateInitExitRemoveEntities.Dispose();
        StateAEnterRemoveEntities.Dispose();
        StateAExitRemoveEntities.Dispose();
        StateBEnterRemoveEntities.Dispose();
        StateBExitRemoveEntities.Dispose();
        StateCEnterRemoveEntities.Dispose();
        StateCExitRemoveEntities.Dispose();

        DependenciesToComplete.Dispose();
    }

    protected override void OnUpdate()
    {
        for (int i = 0; i < DependenciesToComplete.Length; i++)
        {
            DependenciesToComplete[i].Complete();
        }
        DependenciesToComplete.Clear();

        var stateInitEnterEntities = StateInitEnterEntities.ToArray(Allocator.Temp);
        var stateInitExitEntities = StateInitExitEntities.ToArray(Allocator.Temp);
        var stateAEnterEntities = StateAEnterEntities.ToArray(Allocator.Temp);
        var stateAExitEntities = StateAExitEntities.ToArray(Allocator.Temp);
        var stateBEnterEntities = StateBEnterEntities.ToArray(Allocator.Temp);
        var stateBExitEntities = StateBExitEntities.ToArray(Allocator.Temp);
        var stateCEnterEntities = StateCEnterEntities.ToArray(Allocator.Temp);
        var stateCExitEntities = StateCExitEntities.ToArray(Allocator.Temp);

        var stateInitEnterRemoveEntities = StateInitEnterRemoveEntities.ToArray(Allocator.Temp);
        var stateInitExitRemoveEntities = StateInitExitRemoveEntities.ToArray(Allocator.Temp);
        var stateAEnterRemoveEntities = StateAEnterRemoveEntities.ToArray(Allocator.Temp);
        var stateAExitRemoveEntities = StateAExitRemoveEntities.ToArray(Allocator.Temp);
        var stateBEnterRemoveEntities = StateBEnterRemoveEntities.ToArray(Allocator.Temp);
        var stateBExitRemoveEntities = StateBExitRemoveEntities.ToArray(Allocator.Temp);
        var stateCEnterRemoveEntities = StateCEnterRemoveEntities.ToArray(Allocator.Temp);
        var stateCExitRemoveEntities = StateCExitRemoveEntities.ToArray(Allocator.Temp);

        // State enters
        if (stateInitEnterEntities.Length > 0)
        {
            EntityManager.AddComponent<StructuralStateInitEnter>(stateInitEnterEntities);
            EntityManager.AddComponent<StructuralStateInitActive>(stateInitEnterEntities);
        }
        if (stateAEnterEntities.Length > 0)
        {
            EntityManager.AddComponent<StructuralStateAEnter>(stateAEnterEntities);
            EntityManager.AddComponent<StructuralStateAActive>(stateAEnterEntities);
        }
        if (stateBEnterEntities.Length > 0)
        {
            EntityManager.AddComponent<StructuralStateBEnter>(stateBEnterEntities);
            EntityManager.AddComponent<StructuralStateBActive>(stateBEnterEntities);
        }
        if (stateCEnterEntities.Length > 0)
        {
            EntityManager.AddComponent<StructuralStateCEnter>(stateCEnterEntities);
            EntityManager.AddComponent<StructuralStateCActive>(stateCEnterEntities);
        }

        // state exits
        if (stateInitExitEntities.Length > 0)
        {
            EntityManager.AddComponent<StructuralStateInitExit>(stateInitExitEntities);
            EntityManager.RemoveComponent<StructuralStateInitActive>(stateInitExitEntities);
        }
        if (stateAExitEntities.Length > 0)
        {
            EntityManager.AddComponent<StructuralStateAExit>(stateAExitEntities);
            EntityManager.RemoveComponent<StructuralStateAActive>(stateAExitEntities);
        }
        if (stateBExitEntities.Length > 0)
        {
            EntityManager.AddComponent<StructuralStateBExit>(stateBExitEntities);
            EntityManager.RemoveComponent<StructuralStateBActive>(stateBExitEntities);
        }
        if (stateCExitEntities.Length > 0)
        {
            EntityManager.AddComponent<StructuralStateCExit>(stateCExitEntities);
            EntityManager.RemoveComponent<StructuralStateCActive>(stateCExitEntities);
        }

        // enter removes
        if (stateInitEnterRemoveEntities.Length > 0)
        {
            EntityManager.RemoveComponent<StructuralStateInitEnter>(stateInitEnterRemoveEntities);
        }
        if (stateAEnterRemoveEntities.Length > 0)
        {
            EntityManager.RemoveComponent<StructuralStateAEnter>(stateAEnterRemoveEntities);
        }
        if (stateBEnterRemoveEntities.Length > 0)
        {
            EntityManager.RemoveComponent<StructuralStateBEnter>(stateBEnterRemoveEntities);
        }
        if (stateCEnterRemoveEntities.Length > 0)
        {
            EntityManager.RemoveComponent<StructuralStateCEnter>(stateCEnterRemoveEntities);
        }

        // exit removes
        if (stateInitExitRemoveEntities.Length > 0)
        {
            EntityManager.RemoveComponent<StructuralStateInitExit>(stateInitExitRemoveEntities);
        }
        if (stateAExitRemoveEntities.Length > 0)
        {
            EntityManager.RemoveComponent<StructuralStateAExit>(stateAExitRemoveEntities);
        }
        if (stateBExitRemoveEntities.Length > 0)
        {
            EntityManager.RemoveComponent<StructuralStateBExit>(stateBExitRemoveEntities);
        }
        if (stateCExitRemoveEntities.Length > 0)
        {
            EntityManager.RemoveComponent<StructuralStateCExit>(stateCExitRemoveEntities);
        }

        stateInitEnterEntities.Dispose();
        stateInitExitEntities.Dispose();
        stateAEnterEntities.Dispose();
        stateAExitEntities.Dispose();
        stateBEnterEntities.Dispose();
        stateBExitEntities.Dispose();
        stateCEnterEntities.Dispose();
        stateCExitEntities.Dispose();

        stateInitEnterRemoveEntities.Dispose();
        stateInitExitRemoveEntities.Dispose();
        stateAEnterRemoveEntities.Dispose();
        stateAExitRemoveEntities.Dispose();
        stateBEnterRemoveEntities.Dispose();
        stateBExitRemoveEntities.Dispose();
        stateCEnterRemoveEntities.Dispose();
        stateCExitRemoveEntities.Dispose();

        StateInitEnterEntities.Clear();
        StateInitExitEntities.Clear();
        StateAEnterEntities.Clear();
        StateAExitEntities.Clear();
        StateBEnterEntities.Clear();
        StateBExitEntities.Clear();
        StateCEnterEntities.Clear();
        StateCExitEntities.Clear();
        
        StateInitEnterRemoveEntities.Clear();
        StateInitExitRemoveEntities.Clear();
        StateAEnterRemoveEntities.Clear();
        StateAExitRemoveEntities.Clear();
        StateBEnterRemoveEntities.Clear();
        StateBExitRemoveEntities.Clear();
        StateCEnterRemoveEntities.Clear();
        StateCExitRemoveEntities.Clear();
    }
}
