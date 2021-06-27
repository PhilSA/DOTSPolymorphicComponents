using System;
using Unity.Entities;
using Unity.Mathematics;
using System.Runtime.InteropServices;
using Unity.Transforms;

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 20)]
public struct MyPolyComponent : IComponentData
{
	public enum TypeId
	{
		CompA,
		CompB,
	}

	[FieldOffset(0)]
	public CompA CompA;
	[FieldOffset(0)]
	public CompB CompB;

	[FieldOffset(16)]
	private TypeId CurrentTypeId;

	public TypeId GetTypeId()
	{
		return CurrentTypeId;
	}

	public MyPolyComponent(in CompA c)
	{
		CompB = default;
		CompA = c;
		CurrentTypeId = TypeId.CompA;
	}

	public MyPolyComponent(in CompB c)
	{
		CompA = default;
		CompB = c;
		CurrentTypeId = TypeId.CompB;
	}


	public void Update(Single deltaTime, ref Translation translation, ref Rotation rotation)
	{
		switch (CurrentTypeId)
		{
			case TypeId.CompA:
				CompA.Update(deltaTime, ref translation, ref rotation);
				break;
			case TypeId.CompB:
				CompB.Update(deltaTime, ref translation, ref rotation);
				break;
		}
	}
}
