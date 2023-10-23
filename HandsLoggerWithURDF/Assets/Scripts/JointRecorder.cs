using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class JointRecorder : MonoBehaviour
{
    // public int iteration = 1;
    public GameObject urdf;

    private string URDFName;

    private bool isRec = false;
    private bool playLaunched = true;
    private int demo_num;
    private int gesture_num;
    private float startTime = 0;
    private float animationTime = 15;
    private float countdownTime = 2.0f;
    private ControllerFromLogFile controller;
    private EventSystemManager eventHandler;
    private string[] JointNames;
    private List<ArticulationBody> articulationChain = new List<ArticulationBody>();
    private StreamWriter writer;
    
    
    List<float> angles = new List<float>();
    List<string> angleData = new List<string>();   

    // Start is called before the first frame update
    void Start()
    {
        //   isRec = true;
        Button RecordButton = GameObject.Find("Record Button").GetComponent<Button>();
        // Add script here that checks for the existence or desirability of the Play Result Button first
        Button PlayResultButton = GameObject.Find("Play Result Button").GetComponent<Button>();
        PlayResultButton.onClick.AddListener(TaskOnPlayResultClick);
        
        // Debug.Log("Record button found");
        RecordButton.onClick.AddListener(TaskOnRecordClick);
        
        // Debug.Log("Recording: " + urdf.name);

        controller = GameObject.Find("Controller").GetComponent<ControllerFromLogFile>();
        eventHandler = GameObject.Find("Event System").GetComponent<EventSystemManager>();
        
        ArticulationBody[] tempArticulationChain = urdf.GetComponentsInChildren<ArticulationBody>();

        URDFName = transform.root.gameObject.name;
        URDFName = URDFName.Substring(0, URDFName.IndexOf("("));
        // Debug.Log("URDF Name = "+URDFName);
        // DebugReport1.SetText("URDF = " + URDFName);
        String filename = URDFName+"_joints.csv";
        JointNames = System.IO.File.ReadAllLines(Application.persistentDataPath+"/"+filename);
        // if (JointNames.Length>0){
            // DebugReport1.SetText("Joint names file successfully read: "+ JointNames.Length.ToString()+ " lines.");
        // }
        
        // Skip joints that are not listed in the ..._joints.csv file
        for (int i = 0; i<JointNames.Length; i++){
            string jointname = JointNames[i];
            foreach (ArticulationBody joint in tempArticulationChain){
                Debug.Log("Joint name = "+joint.name);
                if (joint.ToString().Substring(0, joint.ToString().IndexOf("(")-1)==jointname){
                    articulationChain.Add(joint);
                    // Debug.Log("Added joint "+jointname+" to articulationChain.");
                }
                else {
                    // Set joint target to 0 so it doesn't wobble, or increase stiffness and damping to something like 100000
                }
            }
        }        
        
        
        // LaunchCSVfile();
       
    }

    // Update is called once per frame
    void Update()
    {
        animationTime = controller.animationTime; //Note that this doesn't get updated until after the first "record"
        // Debug.Log("Animation runtime = " + animationTime.ToString());
        if(isRec == true & startTime == 0.0){ // isRec gets set to "true" two seconds after button click
            startTime = Time.time; 
            playLaunched = false;            
        }

        if (isRec == true){
            foreach (ArticulationBody joint in articulationChain)
            {
                // joint.GetJointPositions(angles); // This technically grabs all joint positions for the entire hierarchy, 
                                                 // so we don't need to iterate over all joints
                // Debug.Log("Joint position at time "+Time.time.ToString()+" is: "+joint.name+" "+angles.ToString());

                float angle = joint.jointPosition[0]; // Note: jointPosition can have 1-3 angles, depending on the dof
                                                      // programmed into the joint. Standard is to have the X angle (twist)
                                                      // be the primary angle of motion, stated first. I confirmed this 
                                                      // was the only angle of motion for each of the gestures on the 
                                                      // Reachy robot, and am now storing only that value
                // Debug.Log("Joint position at time "+Time.time.ToString()+" is: "+joint.name+" "+angle.ToString());
                angles.Add(angle);
                
                
                // Debug.Log("Length of angles list is "+angles.Count);
                for (int i=0; i<angles.Count; i++){
                    // Debug.Log("Angle "+i.ToString()+" is: "+angles[i].ToString());
                    // angleString = angleString+","+angles[i].ToString();
                    // Debug.Log("AngleString is: "+angleString);
                }    
            }
            string angleString = Time.time.ToString();
            for (int i=0; i<angles.Count; i++){
                // Debug.Log("Angle is: "+angles[i].ToString());
                angleString = angleString+","+angles[i].ToString();
                // Debug.Log("AngleString is: "+angleString);
            }
            // Debug.Log("Writing this line to file: "+angleString);
            // writer.WriteLine(Time.time.ToString()+angleString);
            angleData.Add(angleString);
            angles.Clear();
        }
            // The end time needs to equal the animationTime + countdown time + 1 sec between
            //  "GO" and when the robot starts its movement. "Catchuptime" will add a (currently 1.5sec) 
            //  buffer afterwards (can be customized in the EventSystemManager GUI).
        if ((Time.time > startTime + 1.5 + animationTime + eventHandler.catchupTime) & (!playLaunched)){ 
                Debug.Log("JointMotion recording complete at " + Time.time.ToString());
                playLaunched = true;
                isRec = false;
                LogAndConfirm();
        }
    }
 
    public void Reset () {
        angleData.Clear();

        isRec = false;
        startTime = (float) 0.0;
        
        
    }

    private void TaskOnRecordClick()
    {  
        StartCoroutine(WaitForCountdown());
    }

    private void TaskOnPlayResultClick()
    {
        isRec=true;
    }
    
    private IEnumerator WaitForCountdown(){
        yield return new WaitForSecondsRealtime(countdownTime); //Change this if you change the countdown in the ControllerLogFile
        //Output this to console when Button1 is clicked
        // Debug.Log("Starting recording now: " + urdf.name + " at time " + Time.time);
        isRec=true;
        demo_num = eventHandler.demo_num; // Grab the demo num now, not immediately upon click, 
                                          //  just in case there's still a file under the old demo_num
                                          //  that needs to be written
        gesture_num = controller.gesture_num;
    }

    // private void LaunchCSVfile(){
    //     string filePath = Application.persistentDataPath + "/" + URDFName + "_PID" + eventHandler.ParticipantID + "_JointMotion_gesture_" + gesture_num.ToString() + "_" + demo_num.ToString() + ".csv";
    //     // Debug.Log("filepath = " + filePath);
        
    //     writer = new StreamWriter(filePath);
    //     string headerLine = "Time,";
    //     foreach (ArticulationBody joint in articulationChain){
    //         // Debug.Log("Joint name is " + joint.name);
    //         // Debug.Log("DOF count for joint "+joint.name+" is: "+  joint.dofCount.ToString());
    //         for (int i=1; i<=joint.dofCount; i++){
    //             headerLine = headerLine+joint.name+",";
    //         }
            
    //     }
    //     writer.WriteLine(headerLine);
        
    // }

    public void LogAndConfirm() {
        WriteLogFile();
        Reset();
    }

    private void WriteLogFile(){
        string filePath = Application.persistentDataPath + "/" + URDFName + "_PID" + eventHandler.ParticipantID + "_JointMotion_gesture_" + gesture_num.ToString() + "_" + demo_num.ToString() + ".csv";
        // Debug.Log("filepath = " + filePath);
        
        writer = new StreamWriter(filePath);
        string headerLine = "Time,";
        foreach (ArticulationBody joint in articulationChain){
            // Debug.Log("Joint name is " + joint.name);
            // Debug.Log("DOF count for joint "+joint.name+" is: "+  joint.dofCount.ToString());
            for (int i=1; i<=joint.dofCount; i++){
                headerLine = headerLine+joint.name+",";
            }
            
        }
        writer.WriteLine(headerLine);

        for (int i = 0; i < angleData.Count; i++) {
            writer.WriteLine(angleData[i]);
        }
        writer.Flush();
        writer.Close();

    }
}
