using UnityEngine;
using UnityEngine.UI; // Si le bouton est un UI Button

public class CadenasController : MonoBehaviour
{
    public PoigneeLOTO poignee; // Référence à la poignée

    private void Start()
    {
        // Vérifier si la poignée est bien assignée
        if (poignee == null)
        {
            Debug.LogError("PoigneeLOTO non assignée dans CadenasController !");
        }
    }

    // Fonction appelée lors du clic sur le cadenas (via un bouton UI ou un collider)
    public void OnCadenasClicked()
    {
        if (poignee != null)
        {
            poignee.ResetPoignee(); // Appelle la fonction pour ramener la poignée
        }
        else
        {
            Debug.LogWarning("Impossible de réinitialiser la poignée, référence manquante !");
        }
    }
}
