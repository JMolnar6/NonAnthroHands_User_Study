using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

// The object of this script is, instead of reading from a log file, to generate a set of poses (waypoints) 
// that span a reasonable range of positions and orientations utilizing all DoF of the URDF. To do this, 
// we select a few sample angles for each DOF (two at the extremes of the joint, with num_subsamples 
// throughout the range) and randomly combine them (the random seed can be set for consistency across runs.)
// Our goal is to have a movement that spans 30sec-5min of movement, which can be demonstrated eight or ten 
// times. Shorter movements may not be comprehensive, but a 5min test would ideally show a good sampling of
// poses. The num_subsamples may be selected to facilitate this. Also note that, depending on joint 
// configuration, proximal joints may be better subsampled at higher resolution than distal joints or vice
// versa, because of their relative influence on the end-effector (if there is one) or gross/fine motor control.

// But also note that data is being taken continuously throughout the motion, and not just at waypoints. A 
// good multi-pose movement will balance the speed so that jerking is unnecessary and a quality set of samples
// can be taken between (hopefully relatively few) waypoints.

public class ControllerFullExploration : MonoBehaviour {
    public GameObject urdf;
    private ArticulationBody[] articulationChain;
    // Stores original colors of the part being highlighted
    private Color[] prevColor;
    private int previousIndex;
    private Vector2 temp_controls;

    private bool questConnected = false;
    private TextMeshPro DebugReport1;
    private TextMeshPro DebugReport2;

    private int randSeed = 3162;

    // public TextMeshPro DebugReport2;
    // public TextMeshPro DebugReport3;

    // public Button PlayButton;
    // public Button RecordButton;

    public float replayRefreshRate = 15;

    // [InspectorReadOnly(hideInEditMode: true)]
    public string selectedJoint;
    // [HideInInspector]
    public int selectedIndex;
    public int startJoint = 3; //If the first few joints of the URDF includes a root and a base, 
                                // increment the startJoint number so that the position and velocity
                                // commands will apply to the first moveable joint

    // public ControlType control = PositionControl;
    public float stiffness;
    public float damping;
    public float forceLimit;
    public float speed = 5f; // Units: degree/s
    public float torque = 100f; // Units: Nm or N
    public float acceleration = 5f;// Units: m/s^2 / degree/s^2

    public float animationTime;
    

    [Tooltip("Color to highlight the currently selected join")]
    public Color highLightColor = new Color(1.0f, 0, 0, 1.0f);

    void Start()
    {
        Button PlayButton       = GameObject.Find("Play Button").GetComponent<Button>();
        Button PlayResultButton = GameObject.Find("Play Result Button").GetComponent<Button>();
        Button RecordButton     = GameObject.Find("Record Button").GetComponent<Button>();
        DebugReport1 = GameObject.Find("Debug Report 1").GetComponent<TextMeshPro>();
        DebugReport2 = GameObject.Find("Debug Report 2").GetComponent<TextMeshPro>();
        DebugReport1.SetText("");
        DebugReport2.SetText("");
        // Both the Start and Record buttons should initiate animation, so assign them the same listener
        PlayButton.onClick.AddListener(AnimateURDF);
        RecordButton.onClick.AddListener(AnimateURDF);
        PlayResultButton.onClick.AddListener(Playback);
        // StartCoroutine(PlayFromCSV());

        previousIndex = selectedIndex = 1;
        temp_controls = new Vector2(0,0);
        // this.gameObject.AddComponent<Unity.Robotics.UrdfImporter.Control.FKRobot>();
        urdf.gameObject.AddComponent<Unity.Robotics.UrdfImporter.Control.FKRobot>();
        // articulationChain = this.GetComponentsInChildren<ArticulationBody>();
        articulationChain = urdf.GetComponentsInChildren<ArticulationBody>();
        int defDyanmicVal = 10;
        foreach (ArticulationBody joint in articulationChain)
        {
            joint.gameObject.AddComponent<JointControl>();
            joint.jointFriction = defDyanmicVal;
            joint.angularDamping = defDyanmicVal;
            ArticulationDrive currentDrive = joint.xDrive;
            currentDrive.forceLimit = forceLimit;
            joint.xDrive = currentDrive;
        }
    
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

    void SetSelectedJointIndex(int index){
        if (articulationChain.Length > 0){
            selectedIndex = (index + articulationChain.Length) % articulationChain.Length;
        }
    }

    void Update(){
        OVRInput.Update();

    }

    private IEnumerator PlayFromCSV(String filename, float refreshRate){
        string[] PositionLines = System.IO.File.ReadAllLines(Application.persistentDataPath+"/"+filename);
        // string[] VelocityLines = System.IO.File.ReadAllLines(Application.persistentDataPath+"/corrected_velocities.csv");
        // animationTime = PositionLines.Length/replayRefreshRate;

        string URDFName = transform.root.gameObject.name;
        // Debug.Log("Root URDF is named "+ URDFName);

        for (int i=0; i<=PositionLines.Length-1; i++){
            string[] Positions = PositionLines[i].Split(',');
            // string[] Velocities= VelocityLines[i].Split(',');

            int numJoints = Positions.Length;
            // Debug.Log("Number of joints specified");
            // Debug.Log(PositionLines[i]);
            // Debug.Log("Line "+i.ToString()+" of preplanned file. Position control.");
            // Debug.Log("Line "+i.ToString()+" of preplanned file. Velocity control.");
            
            for (int j=1; j<=numJoints; j++){
                string linkName = URDFName+"_link_"+j;
                // Debug.Log("Joint link: "+ linkName);

                ArticulationBody joint = GameObject.Find(linkName).GetComponent<ArticulationBody>();
                var drive = joint.xDrive;
                // Debug.Log("Drive found successfully");

                drive.target = float.Parse(Positions[j-1])*180/(float)Math.PI; // If you insert this line of code, you never have to translate your MoveIt trajectories to degrees
                // Debug.Log("Setting drive target to "+Positions[j-1]);
                // drive.targetVelocity = float.Parse(Velocities[j-1]);
                // Debug.Log("Setting drive target to "+Velocities[j-1]);
                joint.xDrive = drive;
                // joint.xDrive.target         = Positions[j-1];
                // joint.xDrive.targetVelocity = Velocities[j-1];
                
                // JointControl current = articulationChain[j-1].GetComponent<JointControl>();
                
            }

            if (i==PositionLines.Length){
                Debug.Log("Final animation time: " + Time.time.ToString());
            }

        yield return new WaitForSecondsRealtime((float) 1.0/replayRefreshRate);
        }
    }

    private void AnimateURDF()
    {
        String filename = "corrected_positions.csv";
        // Clear any distracting debug text
        DebugReport2.SetText("");

        // If URDF is not already in start position, return it there 
        string[] PositionLines = System.IO.File.ReadAllLines(Application.persistentDataPath+"/"+filename);
        Debug.Log(Application.persistentDataPath);
        animationTime = PositionLines.Length/replayRefreshRate;
        string URDFName = transform.root.gameObject.name;

        string[] Positions = PositionLines[1].Split(',');
        int numJoints = Positions.Length;
    
        for (int j=1; j<=numJoints; j++){
            string linkName = URDFName+"_link_"+j;

            ArticulationBody joint = GameObject.Find(linkName).GetComponent<ArticulationBody>();
            var drive = joint.xDrive;

            drive.target = float.Parse(Positions[j-1]);
            joint.xDrive = drive;            
        }

        // Begin countdown to animation 
        StartCoroutine(BeginCountdown(filename));
    }

    private IEnumerator BeginCountdown(String filename){
        DebugReport1.SetText("Ready?");
        yield return new WaitForSecondsRealtime((float) 2.0);
        DebugReport1.SetText("3");
        yield return new WaitForSecondsRealtime((float) 1.0);
        DebugReport1.SetText("2");
        yield return new WaitForSecondsRealtime((float) 1.0);
        DebugReport1.SetText("1");
        yield return new WaitForSecondsRealtime((float) 1.0);
        DebugReport1.SetText("GO");
        yield return new WaitForSecondsRealtime((float) 0.5);
        DebugReport1.SetText("");
        Debug.Log("Starting now! (robot motion): Time " + Time.time.ToString());
        StartCoroutine(PlayFromCSV(filename, replayRefreshRate));
    }

    private void Playback()
    {
        String filename = "trained_endeff_mean.csv";
        // Clear any distracting debug text
        DebugReport2.SetText("");
        Debug.Log(Application.persistentDataPath);

        // // If URDF is not already in start position, return it there 
        // string[] PositionLines = System.IO.File.ReadAllLines(Application.persistentDataPath+"/"+filename);
        // animationTime = PositionLines.Length/replayRefreshRate;
        // string URDFName = transform.root.gameObject.name;
        // Debug.Log("URDFName = "+URDFName);

        // string[] Positions = PositionLines[1].Split(',');
        
        // int numJoints = Positions.Length;
        // Debug.Log("numJoints length = "+numJoints.ToString());
    
        // for (int j=1; j<=numJoints; j++){
        //     string linkName = URDFName+"_link_"+j;

        //     ArticulationBody joint = GameObject.Find(linkName).GetComponent<ArticulationBody>();
        //     var drive = joint.xDrive;

        //     drive.target = float.Parse(Positions[j-1]);
        //     joint.xDrive = drive;            
        // }
        StartCoroutine(PlayFromCSV(filename, replayRefreshRate));
    }

}
