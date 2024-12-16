using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthBarSystem : ISystem
{
    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Vector3 cameraForward = Vector3.zero;
        if (Camera.main != null) {
            cameraForward = Camera.main.transform.forward;
        }

        foreach ((
            RefRW<LocalTransform> localTransform,
            RefRO<HealthBar> healthBar)
                in SystemAPI.Query<
                    RefRW<LocalTransform>,
                    RefRO <HealthBar>>()) {

            RefRO<LocalTransform> parentLocalTransform = SystemAPI.GetComponentRO<LocalTransform>(healthBar.ValueRO.healthEntity);
            if (localTransform.ValueRO.Scale == 1f) {
                localTransform.ValueRW.Rotation = parentLocalTransform.ValueRO.InverseTransformRotation(quaternion.LookRotation(cameraForward, math.up()));
            }

            RefRO<Health> health = SystemAPI.GetComponentRO<Health>(healthBar.ValueRO.healthEntity);

            if (!health.ValueRO.onHealthChanged) {
                continue;
            }

            float healthNormalized = (float)health.ValueRO.healthAmount / health.ValueRO.healthAmountMax;

            if (healthNormalized == 1f) {
                localTransform.ValueRW.Scale = 0f;
            } else {
                localTransform.ValueRW.Scale = 1f;
            }


            RefRW<PostTransformMatrix> barVisualPostTransformMatrix = SystemAPI.GetComponentRW<PostTransformMatrix>(healthBar.ValueRO.barVisualEntity);
            barVisualPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);
        }
    }
}
