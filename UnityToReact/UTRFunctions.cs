using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class UTRFunctions : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void UTRSendGameObjects(string gameObjectsJson);
    private void SimulateUTRSendGameObjects(string gameObjectsJson)
    {
        //Debug.Log("Simulated UTRSendGameObjects called with JSON: " + gameObjectsJson);
    }


    [SerializeField]
    private GameObject parentGameObject; // Référence au GameObject parent

    [System.Serializable]
    private class GameObjectList
    {
        public string[] names;
    }

    void Start()
    {
        SendGameObjects();
    }

    void SendGameObjects()
    {
        if (parentGameObject == null)
        {
            //Debug.LogError("Parent GameObject is not set.");
            return;
        }

        Transform[] allChildTransforms = parentGameObject.GetComponentsInChildren<Transform>();
        List<string> objectNames = new List<string>();

        foreach (Transform childTransform in allChildTransforms)
        {
            if (childTransform.gameObject != parentGameObject) // Ignorer le parent lui-même
            {
                objectNames.Add(childTransform.gameObject.name);
            }
        }

        GameObjectList gameObjectList = new GameObjectList();
        gameObjectList.names = objectNames.ToArray();

        string json = JsonUtility.ToJson(gameObjectList);

        // Ajouter un journal pour vérifier les données JSON
        //Debug.Log("Generated JSON: " + json);

#if UNITY_WEBGL && !UNITY_EDITOR
        UTRSendGameObjects(json);
#else
        // Méthode de substitution pour les tests dans l'éditeur
        SimulateUTRSendGameObjects(json);
#endif
    }
}
