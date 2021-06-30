using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[Serializable]
[GenerateAuthoringComponent]
public struct SpawnerComp : IComponentData
{
    public Entity Prefab;
    public int Count;
    public float Spacing;
}

// RESULTS: (200k instances)

// POLYMORPH:
// systems: 4.33ms
// frame: 17.5ms
// when staying on same state: 17.5ms

// STRUCTURAL WITH ECB:
// systems: 3ms
// frame: 260ms

// STRUCTURAL WITH BATCH CHANGES:
// systems: 3ms
// frame: 110ms
// when staying on same state: 16ms



public class Spawner : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .WithoutBurst()
            .WithStructuralChanges()
            .ForEach((Entity entity, ref SpawnerComp spawner) =>
        {
            int spawnResolution = (int)math.ceil(math.sqrt(spawner.Count));
            float totalWidth = (spawnResolution - 1) * spawner.Spacing;
            float3 spawnBottomCorner = (-math.right() * totalWidth * 0.5f) + (-math.forward() * totalWidth * 0.5f);

            uint counter = 0;
            for (int x = 0; x < spawnResolution; x++)
            {
                for (int z = 0; z < spawnResolution; z++)
                {
                    if (counter >= spawner.Count)
                    {
                        break;
                    }

                    Entity spawnedCharacter = EntityManager.Instantiate(spawner.Prefab);

                    float3 spawnPos = spawnBottomCorner + (math.right() * x * spawner.Spacing) + (math.forward() * z * spawner.Spacing);
                    EntityManager.SetComponentData(spawnedCharacter, new Translation { Value = spawnPos });
                    EntityManager.SetComponentData(spawnedCharacter, new RandomComp { Random = new Unity.Mathematics.Random(counter + 1) });
                    counter++;
                }
            }

            EntityManager.DestroyEntity(entity);
        }).Run();
    }
}
