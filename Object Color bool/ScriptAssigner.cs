using UnityEngine;

public class ScriptAssigner : MonoBehaviour
{
    public string layerName = "hasData";

    public void AssignScriptToLayerObjects()
    {
        int layer = LayerMask.NameToLayer(layerName);

        if (layer == -1)
        {
            Debug.LogError("Le Layer spécifié n'existe pas.");
            return;
        }

        GameObject[] allGameObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allGameObjects)
        {
            if (obj.layer == layer)
            {
                if (obj.GetComponent<ObjectInformationBool>() == null)
                {
                    obj.AddComponent<ObjectInformationBool>();
                    //Debug.Log($"Script ajouté à {obj.name}");
                }
                else
                {
                    Debug.Log($"{obj.name} possède déjà le script.");
                }
            }
        }
    }
}
