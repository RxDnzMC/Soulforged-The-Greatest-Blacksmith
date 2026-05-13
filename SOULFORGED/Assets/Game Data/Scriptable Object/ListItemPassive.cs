using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "LIST ITEM PASSIVE", menuName = "ScriptableObjects/ListItemPassive", order = 1)]
public class ListItemPassive : ScriptableObject
{
    public List<ItemsPassiveSO> passiveItems = new List<ItemsPassiveSO>();
}
