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
	public MyPolyCompSharedData MyPolyCompSharedData;

	[FieldOffset(12)]
	public CompA CompA;
	[FieldOffset(12)]
	public CompB CompB;

	[FieldOffset(28)]
	public readonly TypeId CurrentTypeId;

	public MyPolyComponent(in CompA c, in MyPolyCompSharedData d)
	{
		CompB = default;
		CompA = c;
		CurrentTypeId = TypeId.CompA;
		MyPolyCompSharedData = d;
	}

	public MyPolyComponent(in CompB c, in MyPolyCompSharedData d)
	{
		CompA = default;
		CompB = c;
		CurrentTypeId = TypeId.CompB;
		MyPolyCompSharedData = d;
	}


	public void Update(Single deltaTime, ref MyPolyCompSharedData sharedData, ref Translation translation, ref Rotation rotation)
	{
		switch (CurrentTypeId)
		{
			case TypeId.CompA:
				CompA.Update(deltaTime, ref sharedData, ref translation, ref rotation);
				break;
			case TypeId.CompB:
				CompB.Update(deltaTime, ref sharedData, ref translation, ref rotation);
				break;
		}
	}
}
