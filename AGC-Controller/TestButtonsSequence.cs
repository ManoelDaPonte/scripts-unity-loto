using UnityEngine;

// Script à ajouter à votre scène pour tester la séquence
public class TestButtonsSequence : MonoBehaviour
{
    public GameObjectSequenceController sequenceController;
    
    void Start()
    {
        if (sequenceController == null)
        {
            sequenceController = FindObjectOfType<GameObjectSequenceController>();
            if (sequenceController == null)
            {
                Debug.LogError("Aucun GameObjectSequenceController trouvé dans la scène!");
                return;
            }
        }
        
        Debug.Log("TestButtonsSequence initialisé. Appuyez sur la touche 'T' pour démarrer le tutoriel.");
    }
    
    void Update()
    {
        // Démarrer le tutoriel avec la touche T
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Démarrage du tutoriel via la touche T");
            sequenceController.StartTraining(); // CHANGÉ: StartTutorial() → StartTraining()
        }
        
        // Clics de test avec les touches numériques
        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
            {
                if (i < sequenceController.sequenceButtons.Length && 
                    sequenceController.sequenceButtons[i] != null)
                {
                    Debug.Log("Simulation de clic sur le bouton " + (i+1));
                    ClickableButton clickable = sequenceController.sequenceButtons[i].GetComponent<ClickableButton>();
                    if (clickable != null)
                    {
                        clickable.ForceClick();
                    }
                    else
                    {
                        Debug.LogError("Pas de composant ClickableButton sur le bouton " + (i+1));
                    }
                }
            }
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 100, 300, 300));
        GUILayout.Label("Appuyez sur 'T' pour démarrer/redémarrer le tutoriel");
        GUILayout.Label("Appuyez sur '1-5' pour simuler des clics sur les boutons");
        GUILayout.EndArea();
    }
}