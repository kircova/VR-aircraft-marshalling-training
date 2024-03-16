using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartStopToggle : MonoBehaviour
{
    public bool isRunning = false;
    private Button button;
    private TMP_Text buttonText;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        buttonText = button.GetComponentInChildren<TMP_Text>();
    }

    public void ToggleRunningState()
    {
        Debug.Log(isRunning);
        if(isRunning){ //Stopped
            buttonText.SetText("Start");
            isRunning = false;
        }
        else{//Started
            buttonText.SetText("Stop");
            isRunning = true;
        }
        // Here you can add any additional logic you want to trigger when toggling the state.
    }
}
