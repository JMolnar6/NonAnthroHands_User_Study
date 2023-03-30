using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartButtonHandler : MonoBehaviour
{
    public int ParticipantID = 0;
    public bool clicked = false;

    private TextMeshPro DebugReport2;

    // private List<GameObject> Buttons;
    private GameObject Canvas;

    private GameObject WelcomeButton;
    private GameObject BeginButton;
    private GameObject RecordButton;
    private GameObject PlayButton;
    // private GameObject PlayResultButton;
    // private GameObject ReplayButton;
    private List<GameObject> Buttons = new List<GameObject>();

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

        // Buttons = GameObject.Find("Button");
        var Buttons = GameObject.FindGameObjectsWithTag("button");

        WelcomeButton = GameObject.Find("Welcome Button");
        BeginButton   = GameObject.Find("Begin Study Button");
        RecordButton  = GameObject.Find("Record Button");
        PlayButton    = GameObject.Find("Play Button");
        // PlayResultButton = GameObject.Find("Replay Hand Motion");
        // ReplayButton  = GameObject.Find("Play Result Button");

        // Buttons.Add(RecordButton);
        // Buttons.Add(PlayButton);
        // Buttons.Add(PlayResultButton);
        // Buttons.Add(ReplayButton);

        WelcomeButton.GetComponent<Button>().onClick.AddListener(TaskOnClickOpen);
        BeginButton.GetComponent<Button>().onClick.AddListener(TaskOnClickBegin);
        WelcomeButton.GetComponent<Button>().enabled = true; 

        // If you want to include any instructions before the user gets started, do that here, now.
        // Remember that it's easier to read instructions if the debug info is on a canvas background
        // that's at least halfway non-transparent (you can make a pretty one or a plain one; doesn't much matter)
        
        BeginButton.GetComponent<Button>().enabled = false;
        foreach (GameObject Button in Buttons){
            Button.transform.localScale = new Vector3(0, 0, 0);
        }
        WelcomeButton.transform.localScale = new Vector3(0.025f,0.025f,0.025f);


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
        WelcomeButton.SetActive(false);
        DebugReport2.SetText("");
        clicked = true;

        // Info needed from partiicpant: height, wingspan, participant ID number
        // At the end of each robot: will get info about control scheme reasoning
        GatherParticipantInfo();

        BeginButton.GetComponent<Button>().enabled = true;
        BeginButton.transform.localScale = new Vector3(0.025f,0.025f,0.025f);

    }

    private void TaskOnClickBegin()
    {
        // Close demographic info, open first robot (and possibly a demo)
        BeginButton.GetComponent<Button>().enabled=false;
        BeginButton.SetActive(false);

        foreach (GameObject Button in Buttons){
            // if (Button!=PlayResultButton && Button!=ReplayButton){ // These buttons useful for viewing user motion or pre-synthesized robot commands; 
                                                                   //  do not include in normal operation
                Button.GetComponent<Button>().transform.localScale = new Vector3(0.025f,0.025f,0.025f);
            // }
            
        }
        PlayButton.GetComponent<Button>().enabled = true;
        // Now, load the first robot and initialize the controller and any other pieces that may be necessary
        Instantiate(Robot1, new Vector3(0,0,0), Quaternion.identity);
        
    }

    private void ConnectToQuest(){
        // Check for Oculus Quest connection - JLM 04/2022
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices){
            // Debug.Log(string.Format("Device found h name '{0}' and role '{1}'", device.name, device.characteristics.ToString()));
            questConnected = true;
        }

        if (questConnected){
            // DebugReport2.SetText("Debug Info: Quest is connected");
            Debug.Log("Debug Info: Quest is connected");
        }
        else {
            // DebugReport2.SetText("Debug Info: Quest is not connected;\n listening for keyboard input");
            Debug.Log("Debug Info: Quest is not connected; listening for keyboard input");
        }            

    }

    private void GatherParticipantInfo(){
        // Provide mark for person to stand on (optional)

        // Set debug instructions to tell the person to stand straight and stretch their arms out to either side
        // (ideally with animation)



        // Wait for hands and head to stop moving, then take data OR have the participant pull on the trigger 
        // (or press any button?) to collect data. Leave markers in VR space to mark where the person's hands were
        // and allow them to retake if necessary.

        // Also have a GUI that allows numerical keyboard input: participant ID will be used in the names of all files saved
        // ParticipantID = input("Please enter your ID number here: \n");

    }
}
