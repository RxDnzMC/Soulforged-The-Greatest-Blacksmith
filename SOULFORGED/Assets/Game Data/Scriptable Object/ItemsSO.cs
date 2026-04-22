    using UnityEngine;

    [CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Items", order = 1)]
    public class ItemsSO : ScriptableObject
    {
        public string itemName;
        public Sprite itemIcon;
        public float baseDamage;
        public float cooldown;
        public float speed;
        public float lifetime;
        public GameObject projectilePrefab;

        public virtual void Use(Transform spawnPoint)
        {
            if (projectilePrefab == null) return;

            // Spawn prefab-nya
            GameObject go = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);
            
            // Kasih datanya ke script yang nempel di prefab (ProjectileLogic)
            if (go.TryGetComponent(out ProjectileLogic logic))
            {
                logic.Setup(speed, baseDamage, lifetime);
            }
        }
    }
