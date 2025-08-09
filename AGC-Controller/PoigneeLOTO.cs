using System.Collections;
using UnityEngine;

public class PoigneeLOTO : MonoBehaviour
{
    public bool isInitiallyOn = true; // Position initiale du commutateur (true par défaut)
    public bool isOn; // État logique du commutateur
    private Vector3 initialPosition; // Position initiale du GameObject
    public float moveDistance = 3f; // Distance de déplacement sur l'axe Z
    public float moveSpeed = 2f; // Vitesse de déplacement

    public GameObject cadenas; // Référence au cadenas à activer/désactiver
    public float bounceDuration = 0.3f; // Durée de l'effet de bounce
    public float bounceScale = 1.3f; // Facteur de grossissement pendant le bounce

    private void Start()
    {
        // Sauvegarder la position initiale
        initialPosition = transform.position;
        isOn = isInitiallyOn;

        // S'assurer que le cadenas est bien invisible au départ
        if (cadenas != null) cadenas.SetActive(!isOn);
    }

    // Bascule entre la position initiale et la nouvelle position (Z + moveDistance)
    public void TogglePosition()
    {
        // Détermine la nouvelle position en fonction de l'état actuel
        Vector3 targetPosition = isOn 
            ? initialPosition + new Vector3(0, 0, moveDistance) // Déplace de +3f en Z
            : initialPosition; // Revient à la position initiale

        // Lancer le déplacement
        StopAllCoroutines();
        StartCoroutine(MoveObject(targetPosition));

        // Inverser l'état logique après chaque clic
        isOn = !isOn;
    }

    // Déplacer l'objet vers la position cible (sur Z)
    private IEnumerator MoveObject(Vector3 targetPosition)
    {
        float time = 0;
        Vector3 startPosition = transform.position;

        while (time < 1f)
        {
            time += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPosition, targetPosition, time);
            yield return null;
        }

        transform.position = targetPosition; // Assurer la position finale exacte

        // Mettre à jour la visibilité du cadenas uniquement à la fin du mouvement
        UpdateLockVisibility();
    }

    // Met à jour la visibilité du cadenas avec une animation de bounce
    private void UpdateLockVisibility()
    {
        if (cadenas != null)
        {
            if (!isOn) // Le cadenas doit apparaître
            {
                cadenas.SetActive(true);
                StartCoroutine(AnimateLockAppearance());
            }
            else // Le cadenas disparaît immédiatement
            {
                cadenas.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("Cadenas non assigné dans PoigneeLOTO.");
        }
    }

    // Animation d'apparition avec un effet "bounce"
    private IEnumerator AnimateLockAppearance()
    {
        float elapsedTime = 0f;
        Vector3 originalScale = cadenas.transform.localScale;
        Vector3 targetScale = originalScale * bounceScale;

        // Grossissement initial
        while (elapsedTime < bounceDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (bounceDuration / 2);
            cadenas.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // Retour à la taille normale avec un rebond
        elapsedTime = 0f;
        while (elapsedTime < bounceDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (bounceDuration / 2);
            cadenas.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        cadenas.transform.localScale = originalScale; // Assurer la taille finale exacte
    }
    public void ResetPoignee()
    {
        if (!isOn) // Si la poignée est dans sa position avancée, on la ramène
        {
            isOn = true;
            StopAllCoroutines();
            StartCoroutine(MoveObject(initialPosition)); // Ramène la poignée
        }
        // Désactiver le cadenas
        if (cadenas != null)
        {
            cadenas.SetActive(false);
        }
    }

}
