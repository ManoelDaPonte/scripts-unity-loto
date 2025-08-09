using System.Collections;
using UnityEngine;

public class PorteRotation : MonoBehaviour
{
    public float rotationAngle = 30f; // Angle de rotation
    public float rotationSpeed = 5f; // Vitesse de rotation
    private Quaternion initialRotation; // Rotation initiale de la porte
    private Quaternion targetRotation; // Rotation cible
    private bool isOpen = false; // Indique si la porte est ouverte

    private void Start()
    {
        initialRotation = transform.rotation; // Stocke la rotation initiale
    }

    public void RotateDoor()
    {
        // Définir la rotation cible
        targetRotation = isOpen 
            ? initialRotation // Revenir à la rotation initiale
            : initialRotation * Quaternion.Euler(0, rotationAngle, 0); // Tourner de 30°
        
        StopAllCoroutines(); // Arrêter toute rotation en cours
        StartCoroutine(RotateSmoothly()); // Lancer la nouvelle rotation
        isOpen = !isOpen; // Inverser l'état de la porte
    }

    private IEnumerator RotateSmoothly()
    {
        float time = 0f;
        Quaternion startRotation = transform.rotation;
        
        while (time < 1f)
        {
            time += Time.deltaTime * rotationSpeed;
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, time);
            yield return null;
        }
        
        transform.rotation = targetRotation; // Assurer la position finale exacte
    }

    public void ResetRotation()
    {
        StopAllCoroutines(); // Arrêter toute animation en cours
        transform.rotation = initialRotation; // Remettre la rotation initiale
        isOpen = false; // Réinitialiser l'état
    }
}