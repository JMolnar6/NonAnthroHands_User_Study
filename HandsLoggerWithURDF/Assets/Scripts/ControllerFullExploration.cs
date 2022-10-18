using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;


public class ControllerFullExploration : MonoBehaviour {
    public GameObject urdf;
    public GameObject handPrefab;
    private ArticulationBody[] articulationChain;
    // Stores original colors of the part being highlighted
    private Color[] prevColor;
    private int previousIndex;
    private Vector2 temp_controls;

    private bool questConnected = false;
    private TextMeshPro DebugReport1;
    private TextMeshPro DebugReport2;
    private List<GameObject> Buttons;

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
    public bool debugHandMotion = false;

    // public ControlType control = PositionControl;
    public float stiffness = 100000f;
    public float damping = 10000f;
    public float forceLimit = 10000f;
    public float speed = 5f; // Units: degree/s
    public float torque = 100f; // Units: Nm or N
    public float acceleration = 5f;// Units: m/s^2 / degree/s^2

    public float animationTime;
    

    [Tooltip("Color to highlight the currently selected joint")]
    public Color highLightColor = new Color(1.0f, 0, 0, 1.0f);

    void Start()
    {
        ConnectToQuest();

        Buttons.Add(GameObject.Find("Play Button"));
        Buttons.Add(GameObject.Find("Play Result Button"));
        Buttons.Add(GameObject.Find("Record Button"));
        Buttons.Add(GameObject.Find("Replay Hand Motion"));

        // Would rather have study start occur after an opening screen, with participant ID info,
        // a small survey, and robot selection. Possibly also a tutorial (separate scene)
        StartStudy();

    }

    void Update(){
        OVRInput.Update();
    }

    private void StartStudy(){

        for (int i=0;i<Buttons.Count-1;i++){ //Exclude the "Replay Hand Motion" button; useful only for debug
            Buttons[i].SetActive(true);
        }

        Button PlayButton       = Buttons[0].GetComponent<Button>();        
        Button PlayResultButton = Buttons[1].GetComponent<Button>();
        Button RecordButton     = Buttons[2].GetComponent<Button>();
        Button ReplayHandButton = Buttons[3].GetComponent<Button>();


        DebugReport1 = GameObject.Find("Debug Report 1").GetComponent<TextMeshPro>();
        DebugReport2 = GameObject.Find("Debug Report 2").GetComponent<TextMeshPro>();
        DebugReport1.SetText("");
        DebugReport2.SetText("");
        // Both the Start and Record buttons should initiate animation, so assign them the same listener
        PlayButton.onClick.AddListener(delegate{AnimateURDF(false);});
        RecordButton.onClick.AddListener(delegate{AnimateURDF(true);});
        PlayResultButton.onClick.AddListener(Playback);

        if (debugHandMotion==false){
            handPrefab.SetActive(false);
            GameObject ReplayHandButtonObject = GameObject.Find("Replay Hand Motion");
            ReplayHandButtonObject.SetActive(false);
        }
        else{
            ReplayHandButton.onClick.AddListener(EndEffPlayback);
        }

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

    void SetSelectedJointIndex(int index){
        if (articulationChain.Length > 0){
            selectedIndex = (index + articulationChain.Length) % articulationChain.Length;
        }
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

    private void AnimateURDF(bool countdown)
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
        if (countdown){
            StartCoroutine(BeginCountdown(filename));
        }
        else{
            StartCoroutine(PlayFromCSV(filename, replayRefreshRate));
        }
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

    private void EndEffPlayback()
    {
        String filename = "pos_rot_hand.csv";
        // Clear any distracting debug text
        DebugReport2.SetText("");
        Debug.Log(Application.persistentDataPath);
        Instantiate(handPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        StartCoroutine(PlaybackHandMotion(filename, replayRefreshRate));
    }

    private IEnumerator PlaybackHandMotion(string filename, float replayRefreshRate){
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

    private IEnumerator AnimateRandomGridSearchTrajectory(float playSpeed){
        // Figure out the number of joints of the URDF and their joint limits
        string URDFName = transform.root.gameObject.name;
        int i = 0;
        bool moreJoints=true;
        int numJoints=0;

        while (moreJoints==true){
            string linkName = URDFName+"_link_"+i;
            if (GameObject.Find(linkName).GetComponent<ArticulationBody>()){
                i=i++;
            }
            else {
                numJoints = i;
                moreJoints=false;
            }
        }

        for (int j=1; j<numJoints; j++){
                string linkName = URDFName+"_link_"+j;

                ArticulationBody joint = GameObject.Find(linkName).GetComponent<ArticulationBody>();
                var drive = joint.xDrive;

                // How to identify joint limits? Using MoveIt? Can we import this from a file so we're not doing something 
                //  crazy like copying and pasting values into the code?
                string filename = URDFName+"_jointlimits.csv";
                string[] JointLimitLines = System.IO.File.ReadAllLines(Application.persistentDataPath+"/"+filename);


                // drive.target = // CHANGE THIS TO DESIRED TARGET ANGLE: float.Parse(Positions[j-1])*180/(float)Math.PI;
                // joint.xDrive = drive;
        }

        // Calculate the number of angles per joint that we'd like to query
        // Have public flag that indicates whether distal joints should query fewer angles
        //  than proximal joints
        // Can use a public flag to override with manual "weights", where each weight informs 
        //  the number of angles for that particular joint

        // Calculate the length of time that this number of combinations will require, given
        //  playSpeed. If >5 min (or 300 sec), report the probable time cost and suggest subsampling
        //  to a rate that will get it down to 5 min. (Alternatively, break it up into shorter 
        //  motions? I'd still like the total to be <5min. It's an enormous amount of training data, 
        //  let alone fatigue and ergonomics issues.)

        // Use randSeed to select random and non-repeated combinations of joint angles from the 
        //  established set of goal angles above. Update the drive state for the angles for each 
        //  combo, then wait *playSpeed* seconds to move onto the next one.

        // When finished, wait two seconds, then return to home position.

        yield return new WaitForSecondsRealtime(playSpeed);

    }

    
}
