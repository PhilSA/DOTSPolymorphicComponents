using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[Serializable]
public struct StateInit : IMyState
{
    public void OnStateEnter(MyStateMachine.TypeId previousState, ref Translation translation)
    {
    }

    public void OnStateExit(MyStateMachine.TypeId nextState)
    {
    }

    public void Update(float deltaTime, ref MyStateMachine stateMachine, ref Translation translation, ref Rotation rotation)
    {
        MyStateMachineUtils.TransitionTo(MyStateMachine.TypeId.StateC, ref stateMachine, ref translation);
    }
}

[Serializable]
public struct StateA : IMyState
{
    public float Duration;
    public float MoveSpeed;
    public float3 MoveDirection;

    [NonSerialized]
    public float DurationCounter;

    public void OnStateEnter(MyStateMachine.TypeId previousState, ref Translation translation)
    {
        // Flip direction
        MoveDirection = -MoveDirection;

        DurationCounter = Duration;
    }

    public void OnStateExit(MyStateMachine.TypeId nextState)
    {
    }

    public void Update(float deltaTime, ref MyStateMachine stateMachine, ref Translation translation, ref Rotation rotation)
    {
        translation.Value += MoveDirection * MoveSpeed * deltaTime;

        if (DurationCounter <= 0f)
        {
            MyStateMachineUtils.TransitionTo(MyStateMachine.TypeId.StateB, ref stateMachine, ref translation);
        }
        DurationCounter -= deltaTime;
    }
}

[Serializable]
public struct StateB : IMyState
{
    public float Duration;
    public float RotationSpeed;
    public float3 RotationAxis;

    [NonSerialized]
    public float DurationCounter;

    public void OnStateEnter(MyStateMachine.TypeId previousState, ref Translation translation)
    {
        DurationCounter = Duration;
    }

    public void OnStateExit(MyStateMachine.TypeId nextState)
    {
    }

    public void Update(float deltaTime, ref MyStateMachine stateMachine, ref Translation translation, ref Rotation rotation)
    {
        rotation.Value = math.mul(rotation.Value, quaternion.Euler(math.normalizesafe(RotationAxis) * RotationSpeed * deltaTime));

        if (DurationCounter <= 0f)
        {
            MyStateMachineUtils.TransitionTo(MyStateMachine.TypeId.StateC, ref stateMachine, ref translation);
        }
        DurationCounter -= deltaTime;
    }
}

[Serializable]
public struct StateC : IMyState
{
    public float Duration;
    public float MoveAmplitude;
    public float3 MoveDirection;

    [NonSerialized]
    public float DurationCounter;
    [NonSerialized]
    public float3 StartPosition;

    public void OnStateEnter(MyStateMachine.TypeId previousState, ref Translation translation)
    {
        DurationCounter = Duration;
        StartPosition = translation.Value;
    }

    public void OnStateExit(MyStateMachine.TypeId nextState)
    {
    }

    public void Update(float deltaTime, ref MyStateMachine stateMachine, ref Translation translation, ref Rotation rotation)
    {
        translation.Value = StartPosition + (MoveDirection * math.sin(((math.PI * 2f) / Duration) * math.clamp(DurationCounter, 0f, Duration)) * MoveAmplitude);

        if (DurationCounter <= 0f)
        {
            MyStateMachineUtils.TransitionTo(MyStateMachine.TypeId.StateA, ref stateMachine, ref translation);
        }
        DurationCounter -= deltaTime;
    }
}