using System;
using Unity.Entities;
using Unity.Mathematics;
using System.Runtime.InteropServices;
using Unity.Transforms;

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 20)]
public struct MyPolyComponent : IComponentData
{
	public enum ComponentType
	{
		CompA,
		CompB,
	}

	[FieldOffset(0)]
	public CompA CompA;

	[FieldOffset(0)]
	public CompB CompB;

	[FieldOffset(16)]
	public ComponentType TypeId;

	public void Update(Single deltaTime, ref Translation translation, ref Rotation rotation)
	{
		switch (TypeId)
		{
			case ComponentType.CompA:
				CompA.Update(deltaTime, ref translation, ref rotation);
				break;
			case ComponentType.CompB:
				CompB.Update(deltaTime, ref translation, ref rotation);
				break;
		}
	}
}
