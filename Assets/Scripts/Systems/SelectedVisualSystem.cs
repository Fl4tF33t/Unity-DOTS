using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateBefore(typeof(ResetEventSystem))]
partial struct SelectedVisualSystem : ISystem {

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {

        foreach (RefRO<Selected> selected in SystemAPI.Query<RefRO<Selected>>().WithPresent<Selected>()) {

            if (selected.ValueRO.onSelected) {
                UnityEngine.Debug.Log("Sele");
                RefRW<LocalTransform> visualLocalTransform = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.visualEntity);
                visualLocalTransform.ValueRW.Scale = selected.ValueRO.showScale;
            }
            if (selected.ValueRO.onDeselected) {
                UnityEngine.Debug.Log("UnSele");
                RefRW<LocalTransform> visualLocalTransform = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.visualEntity);
                visualLocalTransform.ValueRW.Scale = 0f;
            }
        }

    }
}
