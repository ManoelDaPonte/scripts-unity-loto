using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDropdownFamily", menuName = "DropdownManager/DropdownFamily")]
public class DropdownFamily : ScriptableObject
{
    public string familyName; // Nom de la famille
    public List<CameraTargetItem> items = new List<CameraTargetItem>(); // Liste des sous-objets
}
