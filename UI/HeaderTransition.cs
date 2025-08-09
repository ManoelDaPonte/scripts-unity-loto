using System.Collections;
using UnityEngine;

public class HeaderTransition : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        SetVisibility(false); // Commence caché
    }

    public void SetVisibility(bool isVisible)
    {
        StartCoroutine(Fade(isVisible));
    }

    private IEnumerator Fade(bool isVisible)
    {
        float targetAlpha = isVisible ? 1 : 0;
        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0.0f;
        float duration = 0.5f; // Durée de la transition

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = isVisible; // Permet l'interaction uniquement si visible
        canvasGroup.blocksRaycasts = isVisible; // Bloque les raycasts uniquement si visible
    }
}
