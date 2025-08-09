using UnityEngine;

// Classe qui représente une cible pour la caméra avec son objet associé
[System.Serializable]
public class CameraTargetItem
{
    public string name; // Nom de l'option
    public Transform target; // Transform pour la cible de la caméra
    public GameObject outlineObject; // GameObject associé à cet élément

    public CameraTargetItem(string name, Transform target, GameObject outlineObject)
    {
        this.name = name;
        this.target = target;
        this.outlineObject = outlineObject;
    }
}

