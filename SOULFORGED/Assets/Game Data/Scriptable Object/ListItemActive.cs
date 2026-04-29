using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "LIST ITEM ACTIVE", menuName = "ScriptableObjects/ListItemActive", order = 1)]
public class ListItemActive : ScriptableObject
{
    public List<ItemsSO> activeItems = new List<ItemsSO>();
}
