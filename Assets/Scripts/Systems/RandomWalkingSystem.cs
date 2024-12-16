using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct RandomWalkingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((
            RefRO<LocalTransform> localTransform,
            RefRW<UnitMover> unitMover,
            RefRW<RandomWalking> randomWalking)
                in SystemAPI.Query<
                    RefRO<LocalTransform>,
                    RefRW<UnitMover>,
                    RefRW<RandomWalking>>()) {

            if(math.distancesq(localTransform.ValueRO.Position, randomWalking.ValueRO.targetPosition) < UnitMoverSystem.REACHED_TARGET_DISTANCE_SQ) {

                Random random = randomWalking.ValueRO.random;
                float3 randomDirection = new float3(random.NextFloat(-1f, 1f), 0, random.NextFloat(-1f, 1f));
                randomDirection = math.normalize(randomDirection);

                randomWalking.ValueRW.targetPosition =
                    randomWalking.ValueRO.originPosition +
                    randomDirection * random.NextFloat(randomWalking.ValueRO.distanceMin, randomWalking.ValueRO.distanceMax);

                randomWalking.ValueRW.random = random;
            } else {
                unitMover.ValueRW.targetPosition = randomWalking.ValueRO.targetPosition;
            }

        }
    }
}
