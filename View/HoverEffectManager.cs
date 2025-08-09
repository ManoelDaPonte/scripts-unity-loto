using UnityEngine;

public class HoverEffectManager : MonoBehaviour
{
    public LayerMask clickableLayer;

    private Transform currentHitTransform = null;
    private Renderer currentRenderer = null;
    private Color originalColor;

    // Couleur de hover en RGB directement
    private Color hoverColor = new Color(210f / 255f, 200f / 255f, 199f / 255f, 0.05f); 

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickableLayer))
        {
            if (currentHitTransform != hit.transform)
            {
                ResetLastHitObject();
                StoreHitObject(hit.transform);
            }

            if (currentRenderer != null)
            {
                currentRenderer.material.color = hoverColor;
            }
        }
        else
        {
            ResetLastHitObject();
        }
    }

    private void StoreHitObject(Transform hitTransform)
    {
        currentHitTransform = hitTransform;
        currentRenderer = currentHitTransform.GetComponent<Renderer>();

        if (currentRenderer != null)
        {
            originalColor = currentRenderer.material.color;
        }
    }

    private void ResetLastHitObject()
    {
        if (currentRenderer != null)
        {
            currentRenderer.material.color = originalColor;
            currentRenderer = null;
        }

        currentHitTransform = null;
    }
}
