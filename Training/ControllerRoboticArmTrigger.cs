using UnityEngine;

public class ControllerRoboticArmTrigger : MonoBehaviour
{
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    public RoboticArmTraining trainingSystem;
    
    void Start()
    {
        // Trouver le système de formation
        trainingSystem = FindFirstObjectByType<RoboticArmTraining>();
        
        if (trainingSystem == null)
        {
            Debug.LogError("RoboticArmTraining non trouvé ! Assurez-vous qu'il est présent dans la scène.");
        }
        
        // S'assurer qu'il y a un collider sur cet objet
        if (GetComponent<Collider>() == null)
        {
            Debug.LogWarning("Aucun Collider trouvé sur " + gameObject.name + ". Ajout automatique d'un BoxCollider.");
            gameObject.AddComponent<BoxCollider>();
        }
        
        if (showDebugLogs)
        {
            Debug.Log("ControllerRoboticArmTrigger initialisé sur : " + gameObject.name);
        }
    }
    
    void OnMouseDown()
    {
        if (showDebugLogs)
        {
            Debug.Log("Clic détecté sur Controller_Robotic_ARM !");
        }
        
        if (trainingSystem != null)
        {
            trainingSystem.StartTraining();
        }
        else
        {
            Debug.LogError("Impossible de démarrer la formation : RoboticArmTraining non trouvé !");
        }
    }
    
    // Méthode alternative pour les systèmes de raycasting personnalisés
    public void TriggerTraining()
    {
        if (showDebugLogs)
        {
            Debug.Log("Formation déclenchée via TriggerTraining()");
        }
        
        if (trainingSystem != null)
        {
            trainingSystem.StartTraining();
        }
    }
}