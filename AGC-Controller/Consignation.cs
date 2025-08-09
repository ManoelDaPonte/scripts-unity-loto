using System.Collections;
using UnityEngine;

public class Consignation : MonoBehaviour
{
    public GameObject consignation; // L'objet qui deviendra visible après le clic
    public float bounceDuration = 0.3f; // Durée de l'effet bounce
    public float bounceScale = 1.3f; // Facteur de grossissement pendant le bounce

    private void Start()
    {
        // S'assurer que l'autre objet est initialement invisible
        if (consignation != null) consignation.SetActive(false);
    }

    private void Update()
    {
        // Désactiver "consignation" si le cadenas est désactivé
        if (!gameObject.activeSelf && consignation.activeSelf)
        {
            consignation.SetActive(false);
        }
    }

    public void OnMouseDown()
    {
        // Vérifier si le cadenas est actif avant d'agir
        if (gameObject.activeSelf && consignation != null && !consignation.activeSelf)
        {
            consignation.SetActive(true);
            StartCoroutine(AnimateBounce(consignation));
        }
    }

    // Animation d'apparition avec effet bounce
    private IEnumerator AnimateBounce(GameObject target)
    {
        float elapsedTime = 0f;
        Vector3 originalScale = target.transform.localScale;
        Vector3 targetScale = originalScale * bounceScale;

        // Grossissement initial
        while (elapsedTime < bounceDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (bounceDuration / 2);
            target.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // Retour à la taille normale avec un rebond
        elapsedTime = 0f;
        while (elapsedTime < bounceDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (bounceDuration / 2);
            target.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        target.transform.localScale = originalScale; // Assurer la taille finale exacte
    }
}
