using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;


public class ControllerFromLogFile : MonoBehaviour {
    public GameObject urdf;
    public GameObject handPrefab;
    private List<ArticulationBody> articulationChain = new List<ArticulationBody>();
    private bool hasMoved = false;


    private bool questConnected = false;
    private TextMeshPro DebugReport1;
    private TextMeshPro DebugReport2;

    private int selectedIndex = 1;
    public int gesture_num = 1; // 0-based indexing? Double-check after gesture generation
    // private int demo_num = 1;
    // private PosRotRecorder data_recorder;

    // public TextMeshPro DebugReport2;
    // public TextMeshPro DebugReport3;

    // public Button PlayButton;
    // public Button RecordButton;

    public float replayRefreshRate = 15;

    // [InspectorReadOnly(hideInEditMode: true)]
    // public string selectedJoint;
    // [HideInInspector]
    
    public bool debugHandMotion = false;
    public bool playFinalMotion = false;

    // public ControlType control = PositionControl;
    public float stiffness = 100000;
    public float damping = 100000;
    public float forceLimit = 10000;  
    public float speed = 15f; // Units: degree/s
    public float torque = 100f; // Units: Nm or N
    public float acceleration = 5f;// Units: m/s^2 / degree/s^2

    public float animationTime;
    

    [Tooltip("Color to highlight the currently selected joint")]
    public Color highLightColor = new Color(1.0f, 0, 0, 1.0f);

    void Start()
    {
        Button PlayButton       = GameObject.Find("Play Button").GetComponent<Button>();
        Button RecordButton     = GameObject.Find("Record Button").GetComponent<Button>();
        
        DebugReport1 = GameObject.Find("Debug Report 1").GetComponent<TextMeshPro>();
        DebugReport2 = GameObject.Find("Debug Report 2").GetComponent<TextMeshPro>();
        DebugReport1.SetText("");
        DebugReport2.SetText("");
        // Both the Start and Record buttons should initiate animation, so assign them the same listener
        PlayButton.onClick.AddListener(delegate{AnimateURDF(false);});
        RecordButton.onClick.AddListener(delegate{AnimateURDF(true);});
        
        
        // StartCoroutine(PlayFromCSV());

        if (playFinalMotion==false){
            GameObject PlayResultButtonObject = GameObject.Find("Play Result Button");
            PlayResultButtonObject.transform.localScale = new Vector3(0, 0, 0);
        }
        else{
            Button PlayResultButton = GameObject.Find("Play Result Button").GetComponent<Button>();
            PlayResultButton.onClick.AddListener(Playback);
        }

        if (debugHandMotion==false){
            // handPrefab.SetActive(false);
            GameObject ReplayHandButtonObject = GameObject.Find("Replay Hand Motion");
            ReplayHandButtonObject.transform.localScale = new Vector3(0, 0, 0);
        }
        else{
            Button ReplayHandButton = GameObject.Find("Replay Hand Motion").GetComponent<Button>();
            ReplayHandButton.onClick.AddListener(EndEffPlayback);
            // handPrefab.Instantiate();
        }
        // DebugReport1.SetText("Debug Info: We are setting up the robot.");// + ((int) statusUpdate["RedTeamScore"].n));
        SetUpRobot();

        // Check for Oculus Quest connection - JLM 04/2022
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);

        // DebugReport1.SetText("Debug Info: Looking for Quest");// + ((int) statusUpdate["RedTeamScore"].n));
        foreach (var device in inputDevices){
            // Debug.Log(string.Format("Device found h name '{0}' and role '{1}'", device.name, device.characteristics.ToString()));
            questConnected = true;
            // DebugReport1.SetText("Debug Info: identifying input devices.");
        }

        // DebugReport = GameObject.Find("Debug Report").GetComponent<TextMeshPro>();
        if (questConnected){
            // DebugReport1.SetText("Debug Info: Quest is connected");// + ((int) statusUpdate["RedTeamScore"].n));
        }
        else {
            // DebugReport1.SetText("Debug Info: Quest is not connected;\n listening for keyboard input");// + ((int) statusUpdate["RedTeamScore"].n));
        }            

    }

    void SetSelectedJointIndex(int index){
        if (articulationChain.Count > 0){
            selectedIndex = (index + articulationChain.Count) % articulationChain.Count;
        }
    }

    void Update(){
        OVRInput.Update();
    }

    private IEnumerator PlayFromCSV(String URDFName, String filename){
        Debug.Log("Filename for BeginCountdown method:" + filename);
        DebugReport1.SetText("");
        string[] PositionLines = System.IO.File.ReadAllLines(Application.persistentDataPath+"/"+filename);

        for (int i=0; i<PositionLines.Length; i++){
            string[] Positions = PositionLines[i].Split(',');
            int numJoints = Positions.Length;
            
            int j=0;
            foreach (ArticulationBody joint in articulationChain){
                // ArticulationBody joint = GameObject.Find(linkName).GetComponent<ArticulationBody>();
                var drive = joint.xDrive;

                drive.target = float.Parse(Positions[j])*180/(float)Math.PI; // If you insert this line of code, you never have to translate your MoveIt trajectories to degrees
                // joint.xDrive.target         = Positions[j];
                // Debug.Log("Setting drive "+joint.ToString() +" target to "+drive.target.ToString());
                joint.xDrive = drive;
                

                // drive.targetVelocity = float.Parse(Velocities[j]);
                // Debug.Log("Setting drive target to "+Velocities[j]);
                // joint.xDrive.targetVelocity = Velocities[j];
                
                // JointControl current = articulationChain[j].GetComponent<JointControl>();
                j=j+1;
            }

            if (i==PositionLines.Length-1){
                Debug.Log("Final animation time: " + Time.time.ToString());
                DebugReport1.SetText("Gesture complete: animation time = "+animationTime.ToString());

            }
        
        yield return new WaitForSecondsRealtime((float) 1.0/replayRefreshRate);
        }
    }

    // public void SetGestureNum(int num) {
    //     this.gesture_num = num;
    //     GestureNumber = num;
    //     Debug.Log("Setting Gesture Number in Controller File to "+GestureNumber.ToString());
    // }
    
    // public int GetGestureNum() {
    //     Debug.Log("Getting Gesture Number from Controller File: "+GestureNumber.ToString());
    //     return GestureNumber;
    // }

    private void SetUpRobot(){
        hasMoved = false;
        selectedIndex = 1;
        // this.gameObject.AddComponent<Unity.Robotics.UrdfImporter.Control.FKRobot>();
        urdf.gameObject.AddComponent<Unity.Robotics.UrdfImporter.Control.FKRobot>();
        // articulationChain = this.GetComponentsInChildren<ArticulationBody>();
        ArticulationBody[] tempArticulationChain = urdf.GetComponentsInChildren<ArticulationBody>();
        // int defDyanmicVal = 10;

        // Make a file containing the joints that you're interested in so that you can screen out all others
        string URDFName = transform.root.gameObject.name;
        URDFName = URDFName.Substring(0, URDFName.IndexOf("("));
        Debug.Log("URDF Name = "+URDFName);
        // DebugReport1.SetText("URDF = " + URDFName);
        String filename = URDFName+"_joints.csv";
        string[] JointNames = System.IO.File.ReadAllLines(Application.persistentDataPath+"/"+filename);
        // if (JointNames.Length>0){
            // DebugReport1.SetText("Joint names file successfully read: "+ JointNames.Length.ToString()+ " lines.");
        // }
        
        // Skip joints that are not listed in the ..._joints.csv file
        for (int i = 0; i<JointNames.Length; i++){
            string jointname = JointNames[i];
            foreach (ArticulationBody joint in tempArticulationChain){
                if (joint.ToString().Substring(0, joint.ToString().IndexOf("(")-1)==jointname){
                    articulationChain.Add(joint);
                    // Debug.Log("Added joint "+jointname+" to articulationChain.");
                }
                else {
                    // Set joint target to 0 so it doesn't wobble, or increase stiffness and damping to something like 100000
                }
            }
        }        

        foreach (ArticulationBody joint in articulationChain)
        {
            joint.gameObject.AddComponent<JointControl>();
            // joint.jointFriction = defDyanmicVal;
            // joint.angularDamping = defDyanmicVal;
            ArticulationDrive currentDrive = joint.xDrive;
            currentDrive.forceLimit = forceLimit;
            // DebugReport1.SetText("ForceLimit = " + forceLimit.ToString() + " Stiffness = " + stiffness.ToString());
            joint.xDrive = currentDrive;
        }

    }

    private void AnimateURDF(bool countdown)
    {
        string URDFName = transform.root.gameObject.name;
        URDFName = URDFName.Substring(0, URDFName.IndexOf("("));
        String filename = URDFName + "_corrected_positions_"+gesture_num.ToString()+".csv";
        Debug.Log("Reading from "+Application.persistentDataPath+"/"+filename);
        // Clear any distracting debug text
        DebugReport2.SetText("");

        // If URDF is not already in start position, return it there 
        string[] PositionLines = System.IO.File.ReadAllLines(Application.persistentDataPath+"/"+filename);
        if (PositionLines.Length>0){
            Debug.Log("Control log file successfully read");
            // DebugReport1.SetText("Control log file successfully read.");
        }
        animationTime = PositionLines.Length/replayRefreshRate;
        Debug.Log("Animation time = "+animationTime.ToString());
        // DebugReport1.SetText("Animation time = "+animationTime.ToString());

        string[] Positions = PositionLines[1].Split(',');
        int numJoints = Positions.Length;
    
        Debug.Log("Put URDF in starting position: "+PositionLines[1]);
        int j=0; //Does your positions file keep track of which joints you cared about? You don't want to map the first 6 positions regardless of column, if there are more
        foreach (ArticulationBody joint in articulationChain){   
            var drive = joint.xDrive;
            drive.target = float.Parse(Positions[j])*180/(float)Math.PI; // If you insert this line of code, you never have to translate your MoveIt trajectories to degrees        
            joint.xDrive = drive;            
            // Debug.Log("Setting joint "+joint.ToString() + " to position " + Positions[j].ToString());
            j=j+1;
        }

        // Begin countdown to animation 
        if (countdown){
            StartCoroutine(BeginCountdown(URDFName, filename));
        }
        else if (hasMoved == false){
            StartCoroutine(PlayFromCSV(URDFName, filename));
            hasMoved = true;
        }
        else{
            StartCoroutine(PauseBeforeStart(URDFName, filename));
        }
    }

    private IEnumerator BeginCountdown(String URDFName, String filename){
        Debug.Log("Filename for BeginCountdown method:" + filename);
        DebugReport1.SetText("Ready?");
        yield return new WaitForSecondsRealtime((float) 1.0);
        DebugReport1.SetText("Set");
        yield return new WaitForSecondsRealtime((float) 1.0);
        DebugReport1.SetText("GO");
        yield return new WaitForSecondsRealtime((float) 1.0);
        DebugReport1.SetText("");
        Debug.Log("Starting now! (robot motion): Time " + Time.time.ToString());
        StartCoroutine(PlayFromCSV(URDFName, filename));
    }

    private IEnumerator PauseBeforeStart(String URDFName, String filename){
        DebugReport1.SetText("");
        yield return new WaitForSecondsRealtime((float) 1.0);
        StartCoroutine(PlayFromCSV(URDFName, filename));
    }

    private void Playback()
    {
        String filename = "trained_endeff_mean.csv";
        // Clear any distracting debug text
        DebugReport2.SetText("");

        string URDFName = transform.root.gameObject.name;
        URDFName = URDFName.Substring(0, URDFName.IndexOf("("));
        StartCoroutine(PlayFromCSV(URDFName, filename));
    }

    private void EndEffPlayback()
    {
        String filename = "pos_rot_hand.csv";
        // Clear any distracting debug text
        DebugReport2.SetText("");
        Instantiate(handPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        StartCoroutine(PlaybackHandMotion(filename));
    }

    private IEnumerator PlaybackHandMotion(string filename){
        string[] PositionLines = System.IO.File.ReadAllLines(Application.persistentDataPath+"/"+filename);
        
        for (int i=0; i<=PositionLines.Length-1; i++){
            string[] Positions = PositionLines[i].Split(','); 

            //Do these need to be modified for Unity's inverted y-axis?
            Vector3 tempPos = new Vector3(float.Parse(Positions[0]),float.Parse(Positions[1]),float.Parse(Positions[2])); 
            Vector3 tempRot = new Vector3(float.Parse(Positions[3])*180/(float)Math.PI,float.Parse(Positions[4])*180/(float)Math.PI,float.Parse(Positions[5])*180/(float)Math.PI);
            Debug.Log("Rotation = "+tempRot[0].ToString() + " " + tempRot[1].ToString() + " " + tempRot[2].ToString());
            handPrefab.transform.position = tempPos;
                        
            Quaternion safeRot = Quaternion.Euler(tempRot[0],-tempRot[2],tempRot[1]);
            handPrefab.transform.rotation = safeRot;

            if (i==PositionLines.Length){
                Debug.Log("Final animation time: " + Time.time.ToString());
            }

        yield return new WaitForSecondsRealtime((float) 0.5/replayRefreshRate);
        }
    }

}
