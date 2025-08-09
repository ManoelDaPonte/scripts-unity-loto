using UnityEngine;

public class ClickableButton : MonoBehaviour
{
    public GameObjectSequenceController sequenceController;
    public bool debug = true;
    
    private void Start()
    {
        // Trouver automatiquement le contrôleur s'il n'est pas assigné
        if (sequenceController == null)
        {
            sequenceController = FindObjectOfType<GameObjectSequenceController>();
            if (sequenceController == null)
            {
                Debug.LogError("Aucun GameObjectSequenceController trouvé dans la scène!");
            }
            else if (debug)
            {
                Debug.Log("SequenceController trouvé automatiquement pour " + gameObject.name);
            }
        }
        
        // Vérifier que ce GameObject a un collider
        if (GetComponent<Collider>() == null)
        {
            Debug.LogWarning("Ajout automatique d'un BoxCollider à " + gameObject.name);
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            
            // S'assurer que le collider est de bonne taille
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                boxCollider.size = renderer.bounds.size;
                boxCollider.center = renderer.bounds.center - transform.position;
            }
        }
        
        // Vérifier que le GameObject n'est pas trop petit pour être cliqué
        Collider collider = GetComponent<Collider>();
        if (collider != null && collider.bounds.size.magnitude < 0.1f)
        {
            Debug.LogWarning("Le collider de " + gameObject.name + " est peut-être trop petit pour être cliqué facilement.");
        }
    }
    
    private void OnMouseDown()
    {
        if (debug)
        {
            Debug.Log("OnMouseDown détecté sur " + gameObject.name);
        }
        
        // Informer le contrôleur que ce bouton a été cliqué
        if (sequenceController != null)
        {
            sequenceController.OnButtonClicked(gameObject);
        }
        else
        {
            Debug.LogError("SequenceController non assigné pour " + gameObject.name);
        }
    }
    
    // Méthode alternative pour les tests et débogage
    public void ForceClick()
    {
        if (debug)
        {
            Debug.Log("ForceClick appelé sur " + gameObject.name);
        }
        
        if (sequenceController != null)
        {
            sequenceController.OnButtonClicked(gameObject);
        }
        else
        {
            Debug.LogError("SequenceController non assigné pour " + gameObject.name);
        }
    }
}