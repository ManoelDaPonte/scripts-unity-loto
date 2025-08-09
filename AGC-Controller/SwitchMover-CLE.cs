using System.Collections;
using UnityEngine;

public class SwitchMoverCleDAcces : MonoBehaviour
{
    public bool isInitiallyOn = false; // Définir si le commutateur commence "On" ou "Off"
    private bool isOn; // État logique du commutateur
    private bool isFirstClick = true; // Indicateur pour savoir si c'est le premier clic
    private Quaternion initialRotation; // Rotation initiale du GameObject
    private Quaternion targetRotation; // Rotation cible après le premier clic
    public float rotationSpeed = 5f; // Vitesse de la rotation

    public GameObject cle; // Référence au GameObject "CLE"

    void Start()
    {
        initialRotation = transform.rotation; // Rotation initiale du GameObject
        isOn = isInitiallyOn; // L'état logique du commutateur est basé sur isInitiallyOn
    }

    public void ToggleSwitch()
    {
        // Vérifier si isOn est false avant d'appliquer l'effet sur "CLE"
        if (!isOn && cle != null)
        {
            Debug.Log("Le commutateur est OFF. Appel de ResetPosition() sur CLE.");
            cle.SendMessage("ResetPosition", SendMessageOptions.DontRequireReceiver);
        }

        // Si c'est le premier clic, définir la direction de la rotation selon isInitiallyOn
        if (isFirstClick)
        {
            isFirstClick = false; // Marquer que le premier clic est effectué

            // Définir la rotation cible en fonction de l'état initial
            targetRotation = isInitiallyOn 
                ? initialRotation * Quaternion.Euler(0, 0, -90) // Rotation vers -90°
                : initialRotation * Quaternion.Euler(0, 0, 90); // Rotation vers +90°
        }
        else
        {
            // Alternance entre +90° et -90° uniquement
            targetRotation = isOn 
                ? initialRotation * Quaternion.Euler(0, 0, 0) // Rotation vers +90°
                : initialRotation * Quaternion.Euler(0, 0, -90); // Rotation vers -90°
        }

        StopAllCoroutines(); // Arrêter toutes les rotations en cours
        StartCoroutine(RotateSwitch());
    }

    private IEnumerator RotateSwitch()
    {
        // Inverser l'état logique après la rotation
        isOn = !isOn;

        // Animation de rotation
        float time = 0f;
        Quaternion startRotation = transform.rotation;

        // Boucle de l'animation de rotation
        while (time < 1f)
        {
            time += Time.deltaTime * rotationSpeed;
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, time);
            yield return null;
        }

        transform.rotation = targetRotation; // Assurer que la rotation finale est exacte

        // Optionnel : Debug pour vérifier les rotations
        Debug.Log("Rotation finale: " + transform.rotation.eulerAngles.z);
    }
public void ResetSwitch()
{
    StopAllCoroutines(); // Arrêter toute animation en cours
    transform.rotation = initialRotation; // Remettre la rotation initiale
    isOn = isInitiallyOn; // Réinitialiser l'état logique
    isFirstClick = true; // Permettre à ToggleSwitch() de redéfinir correctement la rotation

    Debug.Log("Le commutateur a été réinitialisé.");
}

}
