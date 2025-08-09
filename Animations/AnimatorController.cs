using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public Transform cameraTransform; // Référence à la caméra
    public Transform cameraTarget; // Position cible où l'animation doit s'activer
    public Animator characterAnimator; // Référence à l'Animator du personnage
    public float activationDistance = 5f; // Tolérance pour activer l'animation (à ajuster selon tes besoins)

    void Update()
    {
        // Vérifier si la caméra est proche de la position cible
        if (Vector3.Distance(cameraTransform.position, cameraTarget.position) < activationDistance)
        {
            // Si la caméra est proche de la cible, activer l'animation
            characterAnimator.SetBool("isActive", true); 
        }
        else
        {
            // Si la caméra est trop loin de la cible, désactiver l'animation
            characterAnimator.SetBool("isActive", false); 
        }
    }
}
