using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

public class ControllerFromLogFile : MonoBehaviour {
    public GameObject urdf;
    private ArticulationBody[] articulationChain;
    // Stores original colors of the part being highlighted
    private Color[] prevColor;
    private int previousIndex;
    private Vector2 temp_controls;

    private bool questConnected = false;
    // public Button PlayButton;
    public TextMeshPro DebugReport1;
    public TextMeshPro DebugReport2;
    // public TextMeshPro DebugReport3;

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
        Button PlayButton = GameObject.Find("Play Button").GetComponent<Button>();
        Button RecordButton = GameObject.Find("Record Button").GetComponent<Button>();
        // Both the Start and Record buttons should initiate animation, so assign them the same listener
        PlayButton.onClick.AddListener(AnimateURDF);
        RecordButton.onClick.AddListener(AnimateURDF);
        // StartCoroutine(ReadCSV());

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
            DebugReport1.SetText("Debug Info: Quest is connected");// + ((int) statusUpdate["RedTeamScore"].n));
        }
        else {
            DebugReport1.SetText("Debug Info: Quest is not connected;\n listening for keyboard input");// + ((int) statusUpdate["RedTeamScore"].n));
        }            
    }

    void SetSelectedJointIndex(int index){
        if (articulationChain.Length > 0) 
        {
            selectedIndex = (index + articulationChain.Length) % articulationChain.Length;
        }
    }

    void Update(){
        OVRInput.Update();

    }

    private IEnumerator ReadCSV(){
        string[] PositionLines = System.IO.File.ReadAllLines(Application.persistentDataPath+"/corrected_positions.csv");
        // string[] VelocityLines = System.IO.File.ReadAllLines(Application.persistentDataPath+"/corrected_velocities.csv");
        animationTime = PositionLines.Length/replayRefreshRate;


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

                drive.target = float.Parse(Positions[j-1]);
                // Debug.Log("Setting drive target to "+Positions[j-1]);
                // drive.targetVelocity = float.Parse(Velocities[j-1]);
                // Debug.Log("Setting drive target to "+Velocities[j-1]);
                joint.xDrive = drive;
                // joint.xDrive.target         = Positions[j-1];
                // joint.xDrive.targetVelocity = Velocities[j-1];
                
                // JointControl current = articulationChain[j-1].GetComponent<JointControl>();
                
            }
        yield return new WaitForSecondsRealtime((float) 1.0/replayRefreshRate);

        }

    }

    private void AnimateURDF()
    {
        //Output this to console when Button1 is clicked
        Debug.Log("Starting now! (robot motion)");
        StartCoroutine(ReadCSV());

    }

}
