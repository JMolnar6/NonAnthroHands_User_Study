using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventSystemManager : MonoBehaviour
{
    public int ParticipantID = 0;
    public float catchupTime = 2.0f;
    public bool begin = false;

    private bool questConnected = false;

    private bool gesture_not_robot = true;

    private GameObject Canvas;
    private TextMeshPro DebugReport1;
    private TextMeshPro DebugReport2;
    
    // private PosRotRecorder data_recorder; //Need to expand this; currently focusing only on Right Hand but could be grabbing all in the scene
    // private List<PosRotRecorder> motiontrackers = new List<PosRotRecorder>();
    // private JointRecorder robot_recorder;
    private ControllerFromLogFile controller;
    
    public GameObject[] Robots;
    private GameObject robot;
    // public float[] BufferTime; // Adds extra recording time after a robot's animation, just in case the user doesn't finish instantaneously
                               // BufferTime values can be customized, but should not be longer than the countdown (currently 3.0 sec) - 1sec

    // private GameObject[] Buttons;
    private List<GameObject> ButtonsList = new List<GameObject>();

    private int robot_num = 0;
    // public int[] startjoint_nums;
    public int demo_num = 0;
    public int demo_max = 1; // Temp for faster debugging. Should = 5
    
    private int gesture_num = 1; // 0-based indexing? Double-check after gesture generation
    public int gesture_max = 10; // Set this to whatever the number of gestures/set is
    



    // Start is called before the first frame update
    void Start()
    {
        Canvas = GameObject.Find("Canvas");
        GameObject[] Buttons = GameObject.FindGameObjectsWithTag("button");

        GameObject WelcomeButton = GameObject.Find("Welcome Button");
        GameObject BeginButton   = GameObject.Find("Begin Study Button");
        GameObject NextButton    = GameObject.Find("Next");

        Button RecordButton  = GameObject.Find("Record Button").GetComponent<Button>();
        Button PlayButton    = GameObject.Find("Play Button").GetComponent<Button>();
        
        WelcomeButton.GetComponent<Button>().onClick.AddListener(TaskOnClickOpen);
        WelcomeButton.GetComponent<Button>().enabled = true; 
        BeginButton.GetComponent<Button>().onClick.AddListener(TaskOnClickBegin);
        BeginButton.GetComponent<Button>().enabled = false;
        NextButton.GetComponent<Button>().onClick.AddListener(TaskOnClickNext);
        NextButton.GetComponent<Button>().enabled = false;

        RecordButton.onClick.AddListener(TaskOnRecord);
        PlayButton.onClick.AddListener(TaskOnPlay);
        

        foreach (GameObject Button in Buttons){
            Button.transform.localScale = new Vector3(0, 0, 0);
            ButtonsList.Add(Button);
        }
        WelcomeButton.transform.localScale = new Vector3(0.025f,0.025f,0.025f);
        
        GameObject IDField       = GameObject.Find("Participant ID");
        IDField.transform.localScale = new Vector3(0,0,0);
        // Debug.Log("Robots = " + Robots[1]);

        // If you want to include any instructions before the user gets started, do that here, now.
        // Remember that it's easier to read instructions if the debug info is on a canvas background
        // that's at least halfway non-transparent (you can make a pretty one or a plain one; doesn't much matter)

        DebugReport1 = GameObject.Find("Debug Report 1").GetComponent<TextMeshPro>();
        DebugReport1.SetText("");
        DebugReport2 = GameObject.Find("Debug Report 2").GetComponent<TextMeshPro>();
        DebugReport2.SetText("");

        ConnectToQuest();
        // // data_recorder = GameObject.Find("RightHandAnchor").GetComponent<PosRotRecorder>();
        // data_recorder = GameObject.Find("RightHand Controller").GetComponent<PosRotRecorder>();
        
        demo_num = 0; 
        Debug.Log("Demo num = "+demo_num.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        if(begin==true){
            
            if ((demo_num < demo_max) && (controller.demo_complete)){ //At the very beginning, Update is being called and no controller is set
                controller.demo_complete=false; //reset "demo_complete" marker
                
                StartCoroutine(ReactivateButtons());
            }

            if ((demo_num == demo_max) && (controller.demo_complete)){
                controller.demo_complete=false;

                StartCoroutine(ReactivateButtons());

                if (gesture_num >= gesture_max){
                    // Swap out robots, or swap out gesture sets? I think we said each user got their own gesture set, not all of them
                    gesture_num = 1;
                    
                    controller.gesture_num=gesture_num;
                    gesture_not_robot = false;

                    Debug.Log("On to the next robot!");
                    DebugReport1.SetText("On to the next robot!");
                    }
                else{
                    Debug.Log("On to the next gesture!");
                    DebugReport1.SetText("On to the next gesture!");
                }

                GameObject NextButton = GameObject.Find("Next");
                NextButton.GetComponent<Button>().enabled = true;
                NextButton.transform.localScale = new Vector3(0.025f,0.025f,0.025f);
            }

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
        GameObject IDField       = GameObject.Find("Participant ID");
        IDField.transform.localScale = new Vector3(0.0f,0.0f,0.0f);
        ParticipantID = IDField.GetComponent<Dropdown>().value;
        Debug.Log("Participant ID = "+ ParticipantID.ToString());
        // Close demographic info, open first robot (and possibly a demo)
        GameObject BeginButton   = GameObject.Find("Begin Study Button");   
        BeginButton.SetActive(false);
        begin = true;

        foreach (GameObject Button in ButtonsList){
            //Skip "previous" and "next" here--make them visible after all demos for that gesture have been recorded
            if ((Button.name == "Previous") || (Button.name=="Next")) {
                continue;
            }
            if ((Button.name == "Play Result Button") || (Button.name=="Replay Hand Motion")) {
                continue;
            }
            Button.GetComponent<Button>().transform.localScale = new Vector3(0.025f,0.025f,0.025f);            
        }
        GameObject PlayButton   = GameObject.Find("Play Button");
        PlayButton.GetComponent<Button>().enabled = true;
        // Now, load the first robot and initialize the controller and any other pieces that may be necessary
        
        robot = Instantiate(Robots[0], new Vector3(0f,0.4f,0f), Quaternion.identity); //Ideally, set robot up ~1m off the floor
                                                                                 // You can change the Quaternion.identity argument
                                                                                 // to something that rotates the robot for you so
                                                                                 // that you and it face the same or opposite directions,
                                                                                 // but I prefer that to be manually arranged in the prefab
        // robot.transform.Rotate(0.0f, 180.0f, 0.0f, Space.World);
        // catchupTime=BufferTime[0];
        // Controller is instantiated with the prefab, already attached. Let's grab it
        controller = robot.GetComponentsInChildren<ControllerFromLogFile>()[0]; //Should be only one controller enabled
        gesture_num = 1;//controller.gesture_num; //Allows us to set a gesture in the public edit field for debug       
        // controller.startJoint = startjoint_nums[0];

        string URDFName = controller.transform.root.gameObject.name;
        // URDFName = URDFName.Substring(0, URDFName.IndexOf("("));
        Debug.Log("URDF Name = "+URDFName);
        // robot_recorder = GameObject.Find(URDFName).GetComponent<JointRecorder>();
    }

    private void TaskOnRecord(){
        demo_num=demo_num+1; 
        Debug.Log("Demo num = "+demo_num.ToString()); 
        Button RecordButton = GameObject.Find("Record Button").GetComponent<Button>();
        Button PlayButton   = GameObject.Find("Play Button").GetComponent<Button>();
        RecordButton.interactable = false;
        PlayButton.interactable = false;
    }

    private void TaskOnPlay(){
        // Don't increment the demo number
        Button RecordButton = GameObject.Find("Record Button").GetComponent<Button>();
        Button PlayButton   = GameObject.Find("Play Button").GetComponent<Button>();
        RecordButton.interactable = false;
        PlayButton.interactable = false;
    }

    private IEnumerator ReactivateButtons(){
        yield return new WaitForSecondsRealtime((float) 0.5);
        Button RecordButton       = GameObject.Find("Record Button").GetComponent<Button>();
        RecordButton.interactable = true;
        Button PlayButton         = GameObject.Find("Play Button").GetComponent<Button>();
        PlayButton.interactable   = true;
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
        
        // It's faster to add participant ID options with a for loop rather than manually adding them in the Editor:
        GameObject IDField       = GameObject.Find("Participant ID");
        
        Dropdown ID_Dropdown= IDField.GetComponent<Dropdown>();
        // Debug.Log("IDField = "+ IDField + "; ID Dropdown = " + ID_Dropdown);
        List<string> DropOptions = new List<string>();
        ID_Dropdown.ClearOptions();
        DropOptions.Add("0 (test)");      
        int max_participants = 30;
        for (int i=1; i<=max_participants; i++){
            DropOptions.Add(i.ToString());
        }
        ID_Dropdown.AddOptions(DropOptions);

        IDField.transform.localScale = new Vector3(0.025f,0.025f,0.025f);
        DebugReport2.SetText("Please select your participant ID number. \n The experimenter will tell you which to choose.");
    }

    private void TaskOnClickNext(){
        DebugReport1.SetText("");

        demo_num=0;
        Debug.Log("Demo num = "+demo_num.ToString());

        if (gesture_not_robot){    
            gesture_num = gesture_num+1;
            controller.gesture_num=gesture_num;
        }
        else{
            gesture_not_robot = true;
            robot_num = robot_num+1;
            Destroy(robot);
            robot = Instantiate(Robots[robot_num], new Vector3(0,0.4f,0), Quaternion.identity);
            // catchupTime=BufferTime[robot_num];
            controller = robot.GetComponentsInChildren<ControllerFromLogFile>()[0]; //Should be only one controller enabled
            // controller.startJoint = startjoint_nums[robot_num];
            controller.gesture_num=gesture_num;
        }

        GameObject NextButton   = GameObject.Find("Next");   
        Debug.Log("Next button identified: " + NextButton);
        NextButton.transform.localScale = new Vector3(0,0,0);
        NextButton.GetComponent<Button>().enabled = false;

    }
}
