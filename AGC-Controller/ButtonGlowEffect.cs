using System.Collections;
using UnityEngine;

public class ButtonGlowEffect : MonoBehaviour
{
    [Header("Configuration")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public Color pressedColor = Color.cyan;
    public float glowIntensity = 1.5f;
    public string emissionColorProperty = "_EmissionColor";
    
    private Renderer buttonRenderer;
    private Material buttonMaterial;
    private bool isGlowing = false;
    
    void Awake()
    {
        buttonRenderer = GetComponent<Renderer>();
        
        if (buttonRenderer != null)
        {
            // Utiliser un matériau pour chaque instance pour éviter d'affecter d'autres objets
            buttonMaterial = new Material(buttonRenderer.sharedMaterial);
            buttonRenderer.material = buttonMaterial;
            
            // Activer l'émission pour l'effet de lueur
            buttonMaterial.EnableKeyword("_EMISSION");
            
            // Configurer les couleurs initiales
            SetNormalState();
        }
        else
        {
            Debug.LogError("Pas de Renderer trouvé sur " + gameObject.name);
        }
    }
    
    public void SetNormalState()
    {
        if (buttonMaterial != null)
        {
            buttonMaterial.color = normalColor;
            StopGlowing();
        }
    }
    
    public void SetHighlightState()
    {
        if (buttonMaterial != null)
        {
            buttonMaterial.color = highlightColor;
            StartGlowing(highlightColor);
        }
    }
    
    public void SetPressedState()
    {
        if (buttonMaterial != null)
        {
            buttonMaterial.color = pressedColor;
            StartGlowing(pressedColor);
        }
    }
    
    private void StartGlowing(Color glowColor)
    {
        if (buttonMaterial != null)
        {
            Color emissionColor = glowColor * glowIntensity;
            buttonMaterial.SetColor(emissionColorProperty, emissionColor);
            isGlowing = true;
        }
    }
    
    private void StopGlowing()
    {
        if (buttonMaterial != null && isGlowing)
        {
            buttonMaterial.SetColor(emissionColorProperty, Color.black);
            isGlowing = false;
        }
    }
    
    // Peut être appelé manuellement pour simuler un clic
    public void SimulateClick()
    {
        StartCoroutine(ClickAnimation());
    }
    
    private IEnumerator ClickAnimation()
    {
        SetPressedState();
        yield return new WaitForSeconds(0.1f);
        SetNormalState();
    }
    
    void OnDestroy()
    {
        // Nettoyer le matériel
        if (buttonMaterial != null)
        {
            Destroy(buttonMaterial);
        }
    }
}