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
		StateInit,
		StateA,
		StateB,
		StateC,
	}

	public StateInit StateInit;
	public StateA StateA;
	public StateB StateB;
	public StateC StateC;

	public TypeId CurrentTypeId;

	public MyStateMachine(in StateInit c)
	{
		StateA = default;
		StateB = default;
		StateC = default;
		StateInit = c;
		CurrentTypeId = TypeId.StateInit;
	}

	public MyStateMachine(in StateA c)
	{
		StateInit = default;
		StateB = default;
		StateC = default;
		StateA = c;
		CurrentTypeId = TypeId.StateA;
	}

	public MyStateMachine(in StateB c)
	{
		StateInit = default;
		StateA = default;
		StateC = default;
		StateB = c;
		CurrentTypeId = TypeId.StateB;
	}

	public MyStateMachine(in StateC c)
	{
		StateInit = default;
		StateA = default;
		StateB = default;
		StateC = c;
		CurrentTypeId = TypeId.StateC;
	}


	public void OnStateEnter(TypeId previousState, ref Translation translation, ref RandomComp random)
	{
		switch (CurrentTypeId)
		{
			case TypeId.StateInit:
				StateInit.OnStateEnter(previousState, ref translation, ref random);
				break;
			case TypeId.StateA:
				StateA.OnStateEnter(previousState, ref translation, ref random);
				break;
			case TypeId.StateB:
				StateB.OnStateEnter(previousState, ref translation, ref random);
				break;
			case TypeId.StateC:
				StateC.OnStateEnter(previousState, ref translation, ref random);
				break;
		}
	}

	public void OnStateExit(TypeId nextState)
	{
		switch (CurrentTypeId)
		{
			case TypeId.StateInit:
				StateInit.OnStateExit(nextState);
				break;
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

	public void Update(Single deltaTime, ref MyStateMachine stateMachine, ref Translation translation, ref Rotation rotation, ref RandomComp random)
	{
		switch (CurrentTypeId)
		{
			case TypeId.StateInit:
				StateInit.Update(deltaTime, ref stateMachine, ref translation, ref rotation, ref random);
				break;
			case TypeId.StateA:
				StateA.Update(deltaTime, ref stateMachine, ref translation, ref rotation, ref random);
				break;
			case TypeId.StateB:
				StateB.Update(deltaTime, ref stateMachine, ref translation, ref rotation, ref random);
				break;
			case TypeId.StateC:
				StateC.Update(deltaTime, ref stateMachine, ref translation, ref rotation, ref random);
				break;
		}
	}
}
