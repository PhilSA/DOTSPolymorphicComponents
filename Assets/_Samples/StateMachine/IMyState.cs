using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[PolymorphicComponentDefinition(
    "MyStateMachine",
    "/_Samples/StateMachine/_GENERATED",
    false,
    false,
    null)]
public interface IMyState
{
    public void OnStateEnter(MyStateMachine.TypeId previousState, ref Translation translation);
    public void OnStateExit(MyStateMachine.TypeId nextState);
    public void Update(float deltaTime, ref MyStateMachine stateMachine, ref Translation translation, ref Rotation rotation);
}

public static class MyStateMachineUtils
{
    public static void TransitionTo(MyStateMachine.TypeId newState, ref MyStateMachine sm, ref Translation translation, bool force = false)
    {
        if (force || sm.CurrentTypeId != newState)
        {
            MyStateMachine.TypeId previousState = sm.CurrentTypeId;
            sm.CurrentTypeId = newState;

            sm.OnStateExit(previousState);
            sm.OnStateEnter(sm.CurrentTypeId, ref translation);
        }
    }
}