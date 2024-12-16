using Unity.Entities;
using UnityEngine;

public class BulletAuthoring : MonoBehaviour {

    public float bulletSpeed;
    public int damageAmount;

    public class Baker : Baker<BulletAuthoring> {
        public override void Bake(BulletAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Bullet {
                bulletSpeed = authoring.bulletSpeed,
                damageAmount = authoring.damageAmount,
            });
        }
    }

}

public struct Bullet : IComponentData {

    public float bulletSpeed;
    public int damageAmount;

}
