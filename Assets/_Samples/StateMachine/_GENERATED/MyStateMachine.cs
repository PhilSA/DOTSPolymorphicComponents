using System;
using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[Serializable]
public struct MyStateMachine : IComponentData
{
	public enum TypeId
	{
		StateA,
		StateB,
		StateC,
	}

	public StateA StateA;
	public StateB StateB;
	public StateC StateC;

	public TypeId CurrentTypeId;

	public MyStateMachine(in StateA c)
	{
		StateB = default;
		StateC = default;
		StateA = c;
		CurrentTypeId = TypeId.StateA;
	}

	public MyStateMachine(in StateB c)
	{
		StateA = default;
		StateC = default;
		StateB = c;
		CurrentTypeId = TypeId.StateB;
	}

	public MyStateMachine(in StateC c)
	{
		StateA = default;
		StateB = default;
		StateC = c;
		CurrentTypeId = TypeId.StateC;
	}


	public void OnStateEnter(TypeId previousState, ref Translation translation)
	{
		switch (CurrentTypeId)
		{
			case TypeId.StateA:
				StateA.OnStateEnter(previousState, ref translation);
				break;
			case TypeId.StateB:
				StateB.OnStateEnter(previousState, ref translation);
				break;
			case TypeId.StateC:
				StateC.OnStateEnter(previousState, ref translation);
				break;
		}
	}

	public void OnStateExit(TypeId nextState)
	{
		switch (CurrentTypeId)
		{
			case TypeId.StateA:
				StateA.OnStateExit(nextState);
				break;
			case TypeId.StateB:
				StateB.OnStateExit(nextState);
				break;
			case TypeId.StateC:
				StateC.OnStateExit(nextState);
				break;
		}
	}

	public void Update(Single deltaTime, ref MyStateMachine stateMachine, ref Translation translation, ref Rotation rotation)
	{
		switch (CurrentTypeId)
		{
			case TypeId.StateA:
				StateA.Update(deltaTime, ref stateMachine, ref translation, ref rotation);
				break;
			case TypeId.StateB:
				StateB.Update(deltaTime, ref stateMachine, ref translation, ref rotation);
				break;
			case TypeId.StateC:
				StateC.Update(deltaTime, ref stateMachine, ref translation, ref rotation);
				break;
		}
	}
}
