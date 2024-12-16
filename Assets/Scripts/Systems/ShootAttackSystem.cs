using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct ShootAttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityRefernces entityRefences = SystemAPI.GetSingleton<EntityRefernces>();

        foreach ((
            RefRW<ShootAttack> shootAttack,
            RefRO<Target> target,
            RefRW<LocalTransform> localTransform,
            RefRW<UnitMover> unitMover)
            in SystemAPI.Query<
                RefRW<ShootAttack>,
                RefRO<Target>,
                RefRW<LocalTransform>,
                RefRW<UnitMover>>().WithDisabled<MoveOverride>()) {

            if (target.ValueRO.targetEntity == Entity.Null) {
                continue;
            }

            RefRO<LocalTransform> targetLocalTransform = SystemAPI.GetComponentRO<LocalTransform>(target.ValueRO.targetEntity);
            float targetDistance = math.distance(localTransform.ValueRO.Position, targetLocalTransform.ValueRO.Position);

            if (targetDistance > shootAttack.ValueRO.attackDistance) {
                unitMover.ValueRW.targetPosition = targetLocalTransform.ValueRO.Position;
                continue;
            } else {
                unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
            }

            float3 aimDirection = targetLocalTransform.ValueRO.Position - localTransform.ValueRO.Position;
            aimDirection = math.normalize(aimDirection);
            localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, quaternion.LookRotation(aimDirection, math.up()), SystemAPI.Time.DeltaTime * unitMover.ValueRO.rotationSpeed);


            shootAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if (shootAttack.ValueRO.timer > 0) {
                continue;
            }
            shootAttack.ValueRW.timer = shootAttack.ValueRO.timerMax;

            Entity bulletEntity = state.EntityManager.Instantiate(entityRefences.bulletPrefabEntity);
            float3 bulletSpawnWorldPosition = localTransform.ValueRO.TransformPoint(shootAttack.ValueRO.bulletSpawnLocalPosition);
            SystemAPI.SetComponent(bulletEntity, LocalTransform.FromPosition(bulletSpawnWorldPosition));

            RefRW<Bullet> bulletBullet = SystemAPI.GetComponentRW<Bullet>(bulletEntity);
            bulletBullet.ValueRW.damageAmount = shootAttack.ValueRO.damageAmount;

            RefRW<Target> bulletTarget = SystemAPI.GetComponentRW<Target>(bulletEntity);
            bulletTarget.ValueRW.targetEntity = target.ValueRO.targetEntity;

            shootAttack.ValueRW.onShoot.isTriggered = true;
            shootAttack.ValueRW.onShoot.shootFromPosition = bulletSpawnWorldPosition;

            //Entity shootLightEntity = state.EntityManager.Instantiate(entityRefences.shootLightEntity);
            //SystemAPI.SetComponent(shootLightEntity, LocalTransform.FromPosition(bulletSpawnWorldPosition));
        }
    }
}
