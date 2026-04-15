using System;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    [SerializeField] PlayerData initialPlayerData;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        VisualElement playerData = uiDocument.rootVisualElement.Q<VisualElement>("PlayerData");
        playerData.dataSource = initialPlayerData;
        Label healthLabel = playerData.Q<Label>("HP");
        
        
    }

    
}
