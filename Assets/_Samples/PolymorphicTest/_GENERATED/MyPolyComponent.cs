using System;
using Unity.Entities;
using Unity.Mathematics;
using System.Runtime.InteropServices;
using Unity.Transforms;

[Serializable]
public struct MyPolyComponent : IComponentData
{
	public enum TypeId
	{
		CompA,
		CompB,
	}

	public CompA CompA;

	public CompB CompB;

	public TypeId CurrentTypeId;

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
