using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFactorySceneHDRP;

public class SwitchMoverCommutateur : MonoBehaviour
{
    private bool isOn = false; // État du commutateur
    private Quaternion initialRotation;
    private Quaternion targetRotation;
    public float rotationSpeed = 5f; // Vitesse de rotation

    // Référence au script ArmIK (s'il est directement contrôlé par ce commutateur)
    public ArmIK armIK;
    
    // Liste des CustomSplineAnimate à contrôler
    public List<CustomSplineAnimate> splineAnimators = new List<CustomSplineAnimate>();

    void Start()
    {
        initialRotation = transform.rotation;
        targetRotation = initialRotation * Quaternion.Euler(90, 0, 0); // Rotation de 90° autour de X
    }

    public void ToggleSwitch()
    {
        StopAllCoroutines(); // Stopper les animations en cours
        StartCoroutine(RotateSwitch());
    }

    private IEnumerator RotateSwitch()
    {
        isOn = !isOn; // Inverser l'état
        Quaternion newRotation = isOn ? targetRotation : initialRotation;

        // Modifier l'état du bras IK et des splines en fonction de la position du commutateur
        if (armIK != null)
        {
            if (isOn)
            {
                armIK.DeactivateArm(); // Activer le bras lorsque le commutateur est en position 'On'
                
                // Arrêter tous les CustomSplineAnimate
                foreach (var splineAnimator in splineAnimators)
                {
                    if (splineAnimator != null)
                    {
                        splineAnimator.StopSplineMovement();
                        Debug.Log("Arrêt du spline animator: " + splineAnimator.gameObject.name);
                    }
                }
            }
            else
            {
                armIK.ActivateArm(); // Désactiver le bras lorsque le commutateur est en position 'Off'
                
                // Démarrer tous les CustomSplineAnimate
                foreach (var splineAnimator in splineAnimators)
                {
                    if (splineAnimator != null)
                    {
                        splineAnimator.StartSplineMovement();
                        Debug.Log("Démarrage du spline animator: " + splineAnimator.gameObject.name);
                    }
                }
            }
        }

        float time = 0f;
        Quaternion startRotation = transform.rotation;

        while (time < 1f)
        {
            time += Time.deltaTime * rotationSpeed;
            transform.rotation = Quaternion.Lerp(startRotation, newRotation, time);
            yield return null;
        }

        transform.rotation = newRotation; // S'assurer que la rotation est exacte à la fin
    }
    public void ResetSwitch()
{
    StopAllCoroutines(); // Arrêter toute animation en cours
    transform.rotation = initialRotation; // Remettre la rotation initiale
    isOn = false; // Remettre l'état du commutateur à OFF

    // Réactiver/Désactiver les éléments liés en conséquence
    if (armIK != null)
    {
        armIK.ActivateArm(); // Activer le bras en mode OFF
    }

    foreach (var splineAnimator in splineAnimators)
    {
        if (splineAnimator != null)
        {
            splineAnimator.StartSplineMovement(); // Relancer les animations spline
        }
    }

    Debug.Log("Le commutateur a été réinitialisé.");
}

}