using UnityEngine;

public class ColorChange : MonoBehaviour
{
    private Renderer sphereRenderer;
    private Color startColor;
    private Color startEmissionColor;
    private bool isChangingColor = false;

    [SerializeField]
    private float lerpTime = 0.5f; // Durée du cycle complet (montée + descente)
    
    [SerializeField]
    private float peakTimeRatio = 0.66f; // Proportion du temps passé au sommet

    private float currentLerpTime = 0f;

    void Start()
    {
        sphereRenderer = GetComponent<Renderer>();
        startColor = sphereRenderer.material.color;

        // Stocker la couleur d'émission initiale
        if (sphereRenderer.material.HasProperty("_EmissionColor"))
        {
            startEmissionColor = sphereRenderer.material.GetColor("_EmissionColor");
        }
        else
        {
            startEmissionColor = Color.black;
        }

        // Assurez-vous que l'émission est activée dans le matériau
        sphereRenderer.material.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        if (isChangingColor)
        {
            // Avancement temporel
            currentLerpTime += Time.deltaTime;

            // Temps de montée et de descente ajustés selon la proportion peakTimeRatio
            float cycleTime = lerpTime;
            float peakTime = cycleTime * peakTimeRatio;
            float decayTime = cycleTime - peakTime;

            // Calculer le facteur sinusoïdal ajusté
            float normalizedTime = currentLerpTime % cycleTime / cycleTime;
            float emissionStrength;
            
            if (normalizedTime < peakTime / cycleTime)
            {
                // Phase de montée : augmentation plus douce
                emissionStrength = Mathf.SmoothStep(0f, 1f, normalizedTime * (cycleTime / peakTime));
            }
            else
            {
                // Phase de descente : diminution plus rapide
                emissionStrength = Mathf.SmoothStep(1f, 0f, (normalizedTime - peakTime / cycleTime) * (cycleTime / decayTime));
            }

            // Couleur d'émission cible basée sur l'intensité ajustée
            Color targetEmissionColor = Color.red * emissionStrength * 5f;

            // Appliquer la couleur d'émission au matériau
            sphereRenderer.material.SetColor("_EmissionColor", targetEmissionColor);

            // Mettre à jour l'émission dans la scène
            DynamicGI.SetEmissive(sphereRenderer, targetEmissionColor);

            // Réinitialiser le temps après un cycle complet
            if (currentLerpTime >= lerpTime)
            {
                currentLerpTime = 0f;
            }
        }
    }

    public void ToggleColorChange()
    {
        isChangingColor = !isChangingColor;

        if (!isChangingColor)
        {
            // Revenir à la couleur initiale
            sphereRenderer.material.color = startColor;
            sphereRenderer.material.SetColor("_EmissionColor", startEmissionColor);
            DynamicGI.SetEmissive(sphereRenderer, startEmissionColor);
        }
    }
}
