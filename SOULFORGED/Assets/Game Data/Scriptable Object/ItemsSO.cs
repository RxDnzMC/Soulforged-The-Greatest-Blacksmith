using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public struct ActiveItemLevelData
{
    public int level;
    
    [Header("Stats di Level Ini")]
    public float damage;
    public float cooldown;
    public float speed;
    public float lifetime;
    public float sizeMultiplier;
    public int projectileCount;
    
    public string levelDescription;
}

[CreateAssetMenu(fileName = "New Active Item", menuName = "ScriptableObjects/Items/Active", order = 1)]
public class ItemsSO : ScriptableObject, IAttackItem
{
    public string itemName;
    public Sprite itemIcon;
    public GameObject projectilePrefab;
    
    [Header("Homing Swarm Settings (Tetap)")]
    public float turnSpeed = 15f;
    public float homingDelay = 0.3f;
    public float scatterAngle = 45f;

    [Header("Level Data (Isi Manual per Level)")]
    public List<ActiveItemLevelData> levels = new List<ActiveItemLevelData>();

    [HideInInspector] public PlayerData playerData;
    
    private float _lastUsedTime = -999f;

    public int CurrentLevel
    {
        get
        {
            if (playerData != null)
                return playerData.GetItemLevel(this);
            return 1;
        }
    }
    
    public string CurrentLevelName
    {
        get
        {
            ActiveItemLevelData data = GetCurrentLevelData();
            return $"Lv.{data.level} - {data.levelDescription}";
        }
    }
    
    public int MaxLevel
    {
        get { return levels.Count; }
    }
    
    public bool IsMaxLevel
    {
        get { return CurrentLevel >= MaxLevel; }
    }
    
    public ActiveItemLevelData GetCurrentLevelData()
    {
        int level = CurrentLevel;
        int index = Mathf.Clamp(level - 1, 0, levels.Count - 1);
        
        if (levels.Count == 0)
        {
            Debug.LogError($"Item {itemName} gak punya level data!");
            return new ActiveItemLevelData();
        }
        
        return levels[index];
    }
    
    public ActiveItemLevelData GetLevelData(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, levels.Count - 1);
        if (levels.Count == 0) return new ActiveItemLevelData();
        return levels[index];
    }
    
    public float CurrentCooldown 
    { 
        get { return GetCurrentLevelData().cooldown; }
    }
    
    public bool isReady 
    {
        get { return Time.time >= _lastUsedTime + CurrentCooldown; }
    }

    public int GetCurrentLevel()
    {
        return CurrentLevel;
    }

    public void OnEquipped()
    {
        if (playerData != null)
        {
            int savedLevel = playerData.GetItemLevel(this);
            if (savedLevel <= 0)
                playerData.SetPermanentLevel(this, 1);
            
            Debug.Log($"Item {itemName} equipped! Level: {CurrentLevel}/{MaxLevel}");
        }
    }

    public virtual void Use(Transform spawnPoint)
    {
        if (projectilePrefab == null) return;

        _lastUsedTime = Time.time;

        ActiveItemLevelData levelData = GetCurrentLevelData();

        float currentDamage = levelData.damage;
        float currentSize = levelData.sizeMultiplier;
        float currentSpeed = levelData.speed;
        float currentLifetime = levelData.lifetime;
        int currentCount = levelData.projectileCount;

        if (currentCount > 1)
        {
            for (int i = 0; i < currentCount; i++)
            {
                float angle = -scatterAngle / 2 + (scatterAngle / (currentCount - 1)) * i;
                Quaternion rot = spawnPoint.rotation * Quaternion.Euler(0, angle, 0);
                
                GameObject go = Instantiate(projectilePrefab, spawnPoint.position, rot);
                
                if (go.TryGetComponent(out IProjectile projectile))
                {
                    projectile.Setup(currentSpeed, currentDamage, currentLifetime, 
                                   turnSpeed, homingDelay, scatterAngle, 
                                   CurrentCooldown, currentSize);
                }
            }
        }
        else
        {
            GameObject go = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);
            
            if (go.TryGetComponent(out IProjectile projectile))
            {
                projectile.Setup(currentSpeed, currentDamage, currentLifetime, 
                               turnSpeed, homingDelay, scatterAngle, 
                               CurrentCooldown, currentSize);
            }
        }
    }

    private void OnEnable()
    {
        _lastUsedTime = -999f;
    }
}