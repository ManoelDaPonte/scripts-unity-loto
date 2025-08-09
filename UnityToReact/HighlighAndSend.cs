using UnityEngine;
using System.Runtime.InteropServices;

public class HighlightAndSend : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void UTRSendGameObjectData(string gameObjectJson);
private void SimulateUTRSendGameObjectData(string gameObjectJson)
{
    Debug.Log("Simulated UTRSendGameObject called with JSON: " + gameObjectJson);
    Application.ExternalCall("console.log", "JSON envoyé : " + gameObjectJson);
}


    // Exemple de méthode pour envoyer l'objet sélectionné
    public void SendSelectedGameObject(GameObject selectedObject)
    {
        string objectName = selectedObject.name;

        // Créer l'objet JSON en utilisant seulement le name
        ObjectData data = new ObjectData { name = objectName };
        string jsonData = JsonUtility.ToJson(data);

        Debug.Log("Sending data to Web: " + jsonData);

        #if UNITY_WEBGL && !UNITY_EDITOR
            UTRSendGameObjectData(jsonData);
        #else
            SimulateUTRSendGameObjectData(jsonData);
        #endif
    }

    [System.Serializable]
    private class ObjectData
    {
        public string name;
    }
}
