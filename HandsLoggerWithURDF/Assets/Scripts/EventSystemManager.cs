using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventSystemManager : MonoBehaviour
{
    public int ParticipantID = 0;
    public bool clicked = false;

    private int gesture_num = 0; // 0-based indexing? Double-check after gesture generation

    private TextMeshPro DebugReport1;
    private TextMeshPro DebugReport2;

    // private List<GameObject> Buttons;
    private GameObject Canvas;
    private int demo_num = 1;
    private PosRotRecorder data_recorder; //Need to expand this; currently focusing only on Right Hand but could be grabbing all in the scene
    private JointRecorder robot_recorder;
    private ControllerFromLogFile controller;
    
    private List<GameObject> ButtonsList = new List<GameObject>();

    private bool questConnected = false;

    // public GameObject Canvas;
    public GameObject Robot1;
    public GameObject Robot2;
    public GameObject Robot3;
    public GameObject Robot4;
    public int demo_max = 1; // Temp for faster debugging. Should = 5


    // Start is called before the first frame update
    void Start()
    {
        Canvas = GameObject.Find("Canvas");

        // Buttons = GameObject.Find("Button");
        GameObject[] Buttons = GameObject.FindGameObjectsWithTag("button");

        GameObject WelcomeButton = GameObject.Find("Welcome Button");
        GameObject BeginButton   = GameObject.Find("Begin Study Button");
        GameObject NextButton    = GameObject.Find("Next");
        
        WelcomeButton.GetComponent<Button>().onClick.AddListener(TaskOnClickOpen);
        WelcomeButton.GetComponent<Button>().enabled = true; 
        BeginButton.GetComponent<Button>().onClick.AddListener(TaskOnClickBegin);
        BeginButton.GetComponent<Button>().enabled = false;
        NextButton.GetComponent<Button>().onClick.AddListener(TaskOnClickNext);
        NextButton.GetComponent<Button>().enabled = false;
        
        foreach (GameObject Button in Buttons){
            Button.transform.localScale = new Vector3(0, 0, 0);
            ButtonsList.Add(Button);
        }
        WelcomeButton.transform.localScale = new Vector3(0.025f,0.025f,0.025f);
        
        // If you want to include any instructions before the user gets started, do that here, now.
        // Remember that it's easier to read instructions if the debug info is on a canvas background
        // that's at least halfway non-transparent (you can make a pretty one or a plain one; doesn't much matter)

        DebugReport1 = GameObject.Find("Debug Report 1").GetComponent<TextMeshPro>();
        DebugReport2 = GameObject.Find("Debug Report 2").GetComponent<TextMeshPro>();
        DebugReport2.SetText("");

        ConnectToQuest();

        data_recorder = GameObject.Find("RightHandAnchor").GetComponent<PosRotRecorder>();
        demo_num = data_recorder.iteration;
        Debug.Log("Demo num = "+demo_num.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        demo_num = data_recorder.iteration;
        DebugReport2.SetText("Demo num: "+demo_num.ToString());

        if (demo_num > demo_max){
            data_recorder.iteration  = 1;
            robot_recorder.iteration = 1;
            
            Debug.Log("On to the next gesture!");
            DebugReport1.SetText("On to the next gesture!");
            GameObject NextButton = GameObject.Find("Next");
            NextButton.GetComponent<Button>().enabled = true;
            NextButton.transform.localScale = new Vector3(0.025f,0.025f,0.025f);
        }
    }

    private void TaskOnClickOpen(){
        // You'll want to collect participant info here: ID, height, armspan, demographic or background info?
        GameObject WelcomeButton = GameObject.Find("Welcome Button");
        WelcomeButton.SetActive(false);
        DebugReport2.SetText("");

        // Info needed from partiicpant: height, wingspan, participant ID number
        // At the end of each robot: will get info about control scheme reasoning
        GatherParticipantInfo();

        GameObject BeginButton   = GameObject.Find("Begin Study Button");
        BeginButton.GetComponent<Button>().enabled = true;
        BeginButton.transform.localScale = new Vector3(0.025f,0.025f,0.025f);

    }

    private void TaskOnClickBegin()
    {
        // Close demographic info, open first robot (and possibly a demo)
        GameObject BeginButton   = GameObject.Find("Begin Study Button");   
        BeginButton.SetActive(false);
        clicked = true;

        foreach (GameObject Button in ButtonsList){
            //Skip "previous" and "next" here--make them visible after all demos for that gesture have been recorded
            if ((Button.name == "Previous") || (Button.name=="Next")) {
                continue;
            }
            Button.GetComponent<Button>().transform.localScale = new Vector3(0.025f,0.025f,0.025f);            
        }
        GameObject PlayButton   = GameObject.Find("Play Button");
        PlayButton.GetComponent<Button>().enabled = true;
        // Now, load the first robot and initialize the controller and any other pieces that may be necessary
        var robot1 = Instantiate(Robot1, new Vector3(0,0,0), Quaternion.identity);
        
        // Controller is instantiated with the prefab, already attached. Let's grab it
        controller = robot1.GetComponentsInChildren<ControllerFromLogFile>()[0]; //Should be only one controller enabled
        gesture_num = controller.gesture_num;        

        string URDFName = controller.transform.root.gameObject.name;
        // URDFName = URDFName.Substring(0, URDFName.IndexOf("("));
        Debug.Log("URDF Name = "+URDFName);
        robot_recorder = GameObject.Find(URDFName).GetComponent<JointRecorder>();
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

    private void TaskOnClickNext(){
        DebugReport1.SetText("");
        Debug.Log("DR 1 text cleared");
        gesture_num = gesture_num+1;
        Debug.Log("Moving on to gesture number " + gesture_num.ToString());
        controller.gesture_num=gesture_num;
        Debug.Log("Set gesture number " + gesture_num.ToString() + " in controller, theoretically.");

        GameObject NextButton   = GameObject.Find("Next");   
        Debug.Log("Next button identified: " + NextButton);
        NextButton.transform.localScale = new Vector3(0,0,0);
        NextButton.GetComponent<Button>().enabled = false;
    }
}
