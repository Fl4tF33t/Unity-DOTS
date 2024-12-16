using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct BulletMoverSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((
            RefRW<LocalTransform> localTransform, 
            RefRO<Bullet> bullet,
            RefRO<Target> target,
            Entity entity)
            in SystemAPI.Query<
                RefRW<LocalTransform>,
                RefRO<Bullet>,
                RefRO<Target>>().WithEntityAccess()) {

            if (target.ValueRO.targetEntity == Entity.Null){
                entityCommandBuffer.DestroyEntity(entity);
                continue;
            }

            RefRO<LocalTransform> targetLocation = SystemAPI.GetComponentRO<LocalTransform>(target.ValueRO.targetEntity);
            RefRO<ShootVictim> targetShootVictim = SystemAPI.GetComponentRO<ShootVictim>(target.ValueRO.targetEntity);
            float3 targetPostion = targetLocation.ValueRO.TransformPoint(targetShootVictim.ValueRO.hitLocalPosition);
          

            float distanceBeforeSq = math.distancesq(localTransform.ValueRO.Position, targetPostion);

            float3 moveDirection = targetPostion - localTransform.ValueRO.Position;
            moveDirection = math.normalize(moveDirection);

            localTransform.ValueRW.Position += moveDirection * bullet.ValueRO.bulletSpeed * SystemAPI.Time.DeltaTime;

            float distanceAfterSq = math.distancesq(localTransform.ValueRO.Position, targetPostion);

            if (distanceAfterSq > distanceBeforeSq) {
                localTransform.ValueRW.Position = targetPostion;
            }

            float destroyDistanceSq = 0.2f;
            if (math.distancesq(localTransform.ValueRO.Position, targetPostion) < destroyDistanceSq) {
                RefRW<Health> health = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                health.ValueRW.onHealthChanged = true;
                health.ValueRW.healthAmount -= bullet.ValueRO.damageAmount;

                entityCommandBuffer.DestroyEntity(entity);
            }
        }
    }

}
