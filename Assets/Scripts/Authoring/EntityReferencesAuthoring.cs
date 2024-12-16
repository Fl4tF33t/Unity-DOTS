using Unity.Entities;
using UnityEngine;

public class EntityReferencesAuthoring : MonoBehaviour {

    public GameObject bulletPrefabGameObject;
    public GameObject zombiePrefabGameObject;

    public class Baker : Baker<EntityReferencesAuthoring> {
        public override void Bake(EntityReferencesAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntityRefernces {
                bulletPrefabEntity = GetEntity(authoring.bulletPrefabGameObject, TransformUsageFlags.Dynamic),
                zombiePrefabEntity = GetEntity(authoring.zombiePrefabGameObject, TransformUsageFlags.Dynamic)
            });
        }
    }

}

public struct EntityRefernces : IComponentData {

    public Entity bulletPrefabEntity;
    public Entity zombiePrefabEntity;
}
