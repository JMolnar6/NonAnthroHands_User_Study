using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartButtonHandler : MonoBehaviour
{
    public int ParticipantID = 0;

    private TextMeshPro DebugReport2;

    // private List<GameObject> Buttons;
    private GameObject Canvas;

    private GameObject m_WelcomeButton;
    private GameObject m_BeginButton;

    private bool questConnected = false;

    // public GameObject Canvas;
    public GameObject Robot1;
    public GameObject Robot2;
    public GameObject Robot3;
    public GameObject Robot4;

    // Start is called before the first frame update
    void Start()
    {
        Canvas = GameObject.Find("Canvas");

        m_WelcomeButton = GameObject.Find("Welcome Button");
        m_BeginButton   = GameObject.Find("Begin Study Button");
        m_WelcomeButton.GetComponent<Button>().onClick.AddListener(TaskOnClickOpen);
        m_BeginButton.GetComponent<Button>().onClick.AddListener(TaskOnClickBegin);
        // m_WelcomeButton.onClick.AddListener(TaskOnClickOpen);
        // m_BeginButton.onClick.AddListener(TaskOnClickBegin);

        m_WelcomeButton.GetComponent<Button>().enabled = true; 
        
        m_BeginButton.GetComponent<Button>().enabled = false;

        DebugReport2 = GameObject.Find("Debug Report 2").GetComponent<TextMeshPro>();
        DebugReport2.SetText("");

        ConnectToQuest();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void TaskOnClickOpen(){
        // You'll want to collect participant info here: ID, height, armspan, demographic or background info?
        m_WelcomeButton.SetActive(false);
        DebugReport2.SetText("");

        m_BeginButton.GetComponent<Button>().enabled = true;
                                    


    }
    private void TaskOnClickBegin()
    {
        // Close demographic info, open first robot (and possibly a demo)
        m_BeginButton.SetActive(false);

        
    }

    private void ConnectToQuest(){
        // Check for Oculus Quest connection - JLM 04/2022
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices){
            // Debug.Log(string.Format("Device found h name '{0}' and role '{1}'", device.name, device.characteristics.ToString()));
            questConnected = true;
        }

        // DebugReport = GameObject.Find("Debug Report").GetComponent<TextMeshPro>();
        if (questConnected){
            DebugReport2.SetText("Debug Info: Quest is connected");// + ((int) statusUpdate["RedTeamScore"].n));
        }
        else {
            DebugReport2.SetText("Debug Info: Quest is not connected;\n listening for keyboard input");// + ((int) statusUpdate["RedTeamScore"].n));
        }            

    }
}
