using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct MeleeAttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        NativeList<RaycastHit> raycastHitList = new NativeList<RaycastHit>(Allocator.Temp);

        foreach ((
            RefRO<LocalTransform> localTransform,
            RefRW<MeleeAttack> meleeAttack,
            RefRO<Target> target,
            RefRW<UnitMover> unitMover)
            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRW<MeleeAttack>,
                RefRO<Target>,
                RefRW<UnitMover>>().WithDisabled<MoveOverride>()) {

            if (target.ValueRO.targetEntity == Entity.Null) {
                continue;
            }

            RefRO<LocalTransform> targetLocalTransform = SystemAPI.GetComponentRO<LocalTransform>(target.ValueRO.targetEntity);
            bool isCloseEnoughToAttack = math.distancesq(localTransform.ValueRO.Position, targetLocalTransform.ValueRO.Position) > meleeAttack.ValueRO.attackDistance;

            bool isTouchingTarget = false;

            if (!isCloseEnoughToAttack) {
                float3 directionToTarget = targetLocalTransform.ValueRO.Position - localTransform.ValueRO.Position;
                directionToTarget = math.normalize(directionToTarget);
                float distanceExtra = 0.4f;

                RaycastInput raycastInput = new RaycastInput {
                    Start = localTransform.ValueRO.Position,
                    End = localTransform.ValueRO.Position + directionToTarget * (meleeAttack.ValueRO.colliderSize + distanceExtra),
                    Filter = CollisionFilter.Default
                };
                raycastHitList.Clear();
                if (collisionWorld.CastRay(raycastInput, ref raycastHitList)) {
                    foreach (RaycastHit raycastHit in raycastHitList) {
                        if (raycastHit.Entity == target.ValueRO.targetEntity) {
                            isTouchingTarget = true;
                            break;
                        }
                    }
                }
            }

            if (!isCloseEnoughToAttack && !isTouchingTarget) {
                unitMover.ValueRW.targetPosition = targetLocalTransform.ValueRO.Position;
            } else {
                unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;

                meleeAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if (meleeAttack.ValueRO.timer > 0) {
                    continue;
                }
                meleeAttack.ValueRW.timer = meleeAttack.ValueRO.timerMax;

                RefRW<Health> health = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                health.ValueRW.onHealthChanged = true;
                health.ValueRW.healthAmount -= meleeAttack.ValueRO.damageAmount;
            }
        }
    }
}
