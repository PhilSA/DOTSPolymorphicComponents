using System;
using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 32)]
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


	public void Update(System.Single deltaTime, ref MyPolyCompSharedData sharedData, ref Unity.Transforms.Translation translation, ref Unity.Transforms.Rotation rotation)
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
