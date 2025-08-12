using UnityEngine;

public class MeshColliderFixer : MonoBehaviour
{
    [Header("Target Object")]
    public GameObject targetObject; // Assignez votre "Controller_Robotic_ARM"
    
    [ContextMenu("Fix MeshCollider for OnMouseDown")]
    public void FixMeshCollider()
    {
        if (targetObject == null)
        {
            Debug.LogError("❌ Target Object non assigné !");
            return;
        }
        
        MeshCollider meshCol = targetObject.GetComponent<MeshCollider>();
        
        if (meshCol == null)
        {
            Debug.LogError($"❌ Pas de MeshCollider trouvé sur {targetObject.name}");
            return;
        }
        
        Debug.Log($"🔧 Correction du MeshCollider sur {targetObject.name}");
        
        // CRUCIAL : Mettre convex à true pour OnMouseDown
        meshCol.convex = true;
        
        // S'assurer que ce n'est PAS un trigger
        meshCol.isTrigger = false;
        
        // Vérifier qu'il y a un mesh assigné
        if (meshCol.sharedMesh == null)
        {
            Debug.LogWarning("⚠️ Pas de mesh assigné au MeshCollider, tentative d'auto-assignation...");
            
            MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                meshCol.sharedMesh = meshFilter.sharedMesh;
                Debug.Log("✅ Mesh auto-assigné depuis MeshFilter");
            }
            else
            {
                Debug.LogError("❌ Impossible de trouver un mesh à assigner");
                return;
            }
        }
        
        Debug.Log("✅ MeshCollider configuré correctement :");
        Debug.Log($"  - Convex: {meshCol.convex}");
        Debug.Log($"  - IsTrigger: {meshCol.isTrigger}");
        Debug.Log($"  - SharedMesh: {meshCol.sharedMesh?.name}");
        
        // Vérifier le script trigger
        ControllerRoboticArmTrigger trigger = targetObject.GetComponent<ControllerRoboticArmTrigger>();
        if (trigger == null)
        {
            trigger = targetObject.AddComponent<ControllerRoboticArmTrigger>();
            Debug.Log("✅ ControllerRoboticArmTrigger ajouté");
        }
        
        Debug.Log("🎯 Correction terminée ! Testez maintenant en cliquant sur l'objet.");
    }
    
    [ContextMenu("Test Click Detection")]
    public void TestClickDetection()
    {
        if (targetObject == null) return;
        
        Debug.Log("🧪 Test de détection de clic...");
        
        ControllerRoboticArmTrigger trigger = targetObject.GetComponent<ControllerRoboticArmTrigger>();
        if (trigger != null)
        {
            trigger.TriggerTraining();
            Debug.Log("✅ Formation déclenchée manuellement !");
        }
        else
        {
            Debug.LogError("❌ ControllerRoboticArmTrigger non trouvé !");
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 200, 300, 100));
        GUILayout.Label("🔧 MeshCollider Fixer:");
        
        if (GUILayout.Button("Fix MeshCollider"))
        {
            FixMeshCollider();
        }
        
        if (GUILayout.Button("Test Click"))
        {
            TestClickDetection();
        }
        
        GUILayout.EndArea();
    }
}