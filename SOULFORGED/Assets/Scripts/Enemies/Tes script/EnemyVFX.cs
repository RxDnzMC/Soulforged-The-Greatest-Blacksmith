using UnityEngine;

public class EnemyVFX : MonoBehaviour
{
    [Header("Efek Partikel")]
    [Tooltip("Tarik PREFAB partikel (kotak biru dari folder) ke sini")]
    public GameObject partikelPrefab;

    [Tooltip("Tarik objek kosong 'Titik_Ledakan' dari Hierarchy ke sini")]
    public Transform titikMuncul;

    public void NyalakanPartikelSerangan()
    {
        if (partikelPrefab != null && titikMuncul != null)
        {
            // Instantiate akan "menciptakan" partikel baru secara mandiri, TIDAK menempel pada robot
            GameObject ledakanBaru = Instantiate(partikelPrefab, titikMuncul.position, Quaternion.identity);
            
            // Hancurkan ledakan setelah 2 detik agar memori game tidak penuh
            Destroy(ledakanBaru, 2f);
        }
        else
        {
            Debug.LogWarning("Prefab atau Titik Muncul belum dimasukkan di Inspector!");
        }
    }
}