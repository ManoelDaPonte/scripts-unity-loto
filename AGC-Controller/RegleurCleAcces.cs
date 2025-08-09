using System.Collections;
using UnityEngine;

public class RegleurCleDAcces : MonoBehaviour
{
    public bool isInitiallyOn = true; // Cette version commence sur "On"
    private bool isOn;
    private bool isFirstClick = true;
    private Quaternion initialRotation;
    private Quaternion targetRotation;
    public float rotationSpeed = 5f;

    public GameObject cle;

    void Start()
    {
        initialRotation = transform.rotation;
        isOn = isInitiallyOn;
    }

    public void ToggleSwitch()
    {
        if (!isOn && cle != null)
        {
            Debug.Log("Le commutateur est OFF. Appel de ResetPosition() sur CLE.");
            cle.SendMessage("ResetPosition", SendMessageOptions.DontRequireReceiver);
        }

        if (isFirstClick)
        {
            isFirstClick = false;
            // Rotation opposée au script original
            targetRotation = isInitiallyOn 
                ? initialRotation * Quaternion.Euler(0, 0, 90)  // Commence à 90° et va à 0°
                : initialRotation * Quaternion.Euler(0, 0, -90);
        }
        else
        {
            // Bascule de manière inverse par rapport à l'autre script
            targetRotation = isOn 
                ? initialRotation * Quaternion.Euler(0, 0, -90)  // Se dirige vers -90°
                : initialRotation * Quaternion.Euler(0, 0, 0);    // Retourne à 0°
        }

        StopAllCoroutines();
        StartCoroutine(RotateSwitch());
    }

    private IEnumerator RotateSwitch()
    {
        isOn = !isOn;

        float time = 0f;
        Quaternion startRotation = transform.rotation;

        while (time < 1f)
        {
            time += Time.deltaTime * rotationSpeed;
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, time);
            yield return null;
        }

        transform.rotation = targetRotation;
        Debug.Log("Rotation finale: " + transform.rotation.eulerAngles.z);
    }

    public void ResetSwitch()
    {
        StopAllCoroutines();
        transform.rotation = initialRotation;
        isOn = isInitiallyOn;
        isFirstClick = true;

        Debug.Log("Le commutateur a été réinitialisé.");
    }
}
