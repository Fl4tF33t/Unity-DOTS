using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class UnitMoverAuthoring : MonoBehaviour {

    public float moveSpeed;
    public float rotationSpeed;

    public class Baker : Baker<UnitMoverAuthoring> {
        public override void Bake(UnitMoverAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitMover {
                moveSpeed = authoring.moveSpeed,
                rotationSpeed = authoring.rotationSpeed
            });
        }
    }
}

public struct UnitMover : IComponentData {

    public float moveSpeed;
    public float rotationSpeed;
    public float3 targetPosition;
}
