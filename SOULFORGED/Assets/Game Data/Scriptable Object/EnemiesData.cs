using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "ScriptableObjects/Enemy")] // Tambahkan ini!
public class EnemiesData : ScriptableObject
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public string enemyName;
    public float health;
    public float baseDamage;
    public float speed;
    public float attackInterval;
    public float expReward; // Exp yang diberikan saat musuh mati, bisa diatur per musuh
    public GameObject enemyPrefab;
    public virtual void Use(Transform spawnPoint)
        {
            if (enemyPrefab == null) return;

            // Spawn prefab-nya
            GameObject go = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            
            // Kasih datanya ke script yang nempel di prefab (ProjectileLogic)
            if (go.TryGetComponent(out EnemiesBase logic))
            {
                logic.Setup(health, speed, baseDamage, attackInterval, expReward);
            }
        }
}
