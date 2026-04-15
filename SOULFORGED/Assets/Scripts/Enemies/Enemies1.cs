using System;
using UnityEngine;

public class Enemies1 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] PlayerData playerData;
    [SerializeField] float damageInterval = 0.5f;
    [SerializeField] int damage = 10;

    private float lastDamageTime = -999f;
    
    
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) {
            if (Time.time >= lastDamageTime + damageInterval) {
                playerData.health -= damage;
                lastDamageTime = Time.time;
            }
        }
    }
}
