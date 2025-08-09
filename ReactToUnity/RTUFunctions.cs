
using TMPro;
using UnityEngine;

public class RTUFunctions : MonoBehaviour
{
    public TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        text.text = "Hello from nowhere!";
    }

    // Update is called once per frame
    void Update()
    {

    }

    void UpdateMessage(string message)
    {
        //Debug.Log("Message from React: " + message);
        text.text = message;
    }
}
