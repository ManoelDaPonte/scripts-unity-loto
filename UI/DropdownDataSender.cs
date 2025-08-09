using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;


public class DropdownDataSender : MonoBehaviour
{
    public DropdownManager dropdownManager;  // Référence au DropdownManager

    // Déclaration de la fonction externe pour appeler la fonction JavaScript
    [DllImport("__Internal")]
    private static extern void UTRSendDropdownData(string dropdownDataJson);

    // Cette méthode sera appelée pour envoyer les données des dropdowns vers React
    public void SendDropdownDataToReact()
    {
        // Crée un objet structuré avec les données de tous les dropdowns
        var dropdownData = new DropdownData
        {
            globalViews = GetDropdownOptions(dropdownManager.globalViews),
            sensors = GetDropdownOptions(dropdownManager.sensors),
        };

        // Convertir l'objet en JSON
        string json = JsonUtility.ToJson(dropdownData);
        Debug.Log("Envoi des données du Dropdown : " + json);

        // Envoi du JSON via WebGL vers la fonction JavaScript
        #if UNITY_WEBGL && !UNITY_EDITOR
            UTRSendDropdownData(json);
        #else
            Debug.Log("Envoi simulé des données du Dropdown : " + json);
        #endif
    }

    // Méthode pour récupérer les options d'un dropdown sous forme de liste de chaînes
    private List<string> GetDropdownOptions(List<CameraTargetItem> items)
    {
        List<string> options = new List<string>();
        foreach (var item in items)
        {
            options.Add(item.name);  // Utilisez item.name pour obtenir le nom de chaque élément
        }
        return options;
    }

    // Classe pour structurer les données avant de les convertir en JSON
    [System.Serializable]
    public class DropdownData
    {
        public List<string> globalViews;
        public List<string> sensors;
    }
}
