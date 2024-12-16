using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct ShootLightSpawnerSystem : ISystem {

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {

        EntityRefernces entityRefences = SystemAPI.GetSingleton<EntityRefernces>();
        foreach (var shootAttack in SystemAPI.Query<RefRO<ShootAttack>>()) {

            if (!shootAttack.ValueRO.onShoot.isTriggered) {
                continue;
            }

            Entity shootLightEntity = state.EntityManager.Instantiate(entityRefences.shootLightEntity);
            SystemAPI.SetComponent(shootLightEntity, LocalTransform.FromPosition(shootAttack.ValueRO.onShoot.shootFromPosition));
        }
    }
}
