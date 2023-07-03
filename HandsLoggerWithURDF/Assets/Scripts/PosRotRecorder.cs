using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class PosRotRecorder : MonoBehaviour
{
    public int iteration = 1;
    public Transform go;
    public float catchupTime = (float) 2.0;
    public bool allowPlaybackRecording = false;
    // public ControllerFromLogFile fp;

    // public Button RecordButton;

    private bool isRec = false;
    private bool playLaunched = true;
    private bool hitRecord = true;
    private float startTime = 0;
    private float animationTime = 15;
    private ControllerFromLogFile controller;
    // private ControllerFullExploration controller;
    private EventSystemManager eventHandler;
    
    List<Vector3> pos = new List<Vector3>();
    List<Quaternion> rot = new List<Quaternion>();
    List<float> tim = new List<float>();    

    // Start is called before the first frame update
    void Start()
    {
        //   isRec = true;
        Button RecordButton = GameObject.Find("Record Button").GetComponent<Button>();
        RecordButton.onClick.AddListener(delegate{TaskOnRecordClick(true);});
        
        Button PlayResultButton = GameObject.Find("Play Result Button").GetComponent<Button>();
        PlayResultButton.onClick.AddListener(delegate{TaskOnRecordClick(false);});
        
        eventHandler = GameObject.Find("Event System").GetComponent<EventSystemManager>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if(eventHandler.begin==true){
            controller = GameObject.Find("Controller").GetComponent<ControllerFromLogFile>();
            // controller = GameObject.Find("Controller").GetComponent<ControllerFullExploration>();
            //Animation time = the amount of time it will take to run the animation
            animationTime = controller.animationTime; 
            // Debug.Log("Animation runtime = " + animationTime.ToString());
            if(isRec == true & startTime == 0.0){ // isRec gets set to "true" upon button click
                startTime = Time.time; 
                playLaunched = false;
                // Debug.Log("Time = " + startTime);
                // Debug.Log("End time = " + (startTime+animationTime).ToString());
            }

            if (isRec == true){
                Vector3 tempPos = go.position;
                Quaternion tempRot = go.rotation;

                pos.Add(tempPos);
                rot.Add(tempRot);
                tim.Add(Time.time);
                
                // Debug.Log("Position at time " + Time.time + " = " + go.position);
                // Debug.Log("Rotation at time " + Time.time + " = " + go.rotation);            
            }
            // The end time needs to equal the animationTime + 1 sec at the beginning between 
            //  "GO" and when the robot starts its movement. "Catchuptime" will add a (currently 2sec) 
            //  buffer afterwards.
            if ((Time.time > startTime + animationTime + 1.0 + catchupTime) & (!playLaunched)){ 
                    Debug.Log("Recording complete at " + Time.time.ToString());
                    playLaunched = true;
                    isRec = false;
                    LogAndConfirm();
            }
        }


    }


    public void LogAndConfirm() {
        WriteLogFile();
        Reset();
        iteration++;
        Debug.Log("Iteration = "+ iteration);
    }

    private void WriteLogFile(){
        // output log file of user motions and times
        
        // Debug.Log("filepath directory = " + Application.persistentDataPath + "/" + go.name + "_Motion_gesture_" + controller.gesture_num.ToString() + "_" + iteration + ".csv");
        
        // string filePath = Application.persistentDataPath + "/Data/" + "goMotion";
        string filePath = "";
        if (hitRecord){
            filePath = Application.persistentDataPath + "/" + controller.URDFName + "_PID" + eventHandler.ParticipantID + "_" + go.name + "_Motion_gesture_" + controller.gesture_num.ToString() + "_" + iteration + ".csv";
        }
        else{
            filePath = Application.persistentDataPath + "/" + controller.URDFName + "_PID" + eventHandler.ParticipantID + "_" + go.name + "_Playback.csv";
        }
        
        // Debug.Log("filepath = " + filePath);
        
        StreamWriter writer = new StreamWriter(filePath);
        writer.WriteLine("Time, Position (X), Position (Y), Position (Z), Rotation (X), Rotation (Y), Rotation (Z)");
        for (int i = 0; i < tim.Count; i++) {
            writer.WriteLine(tim[i]+","+pos[i][0]+","+pos[i][1]+","+pos[i][2]+","+rot[i][0]+","+rot[i][1]+","+rot[i][2]);
        }
        writer.Flush();
        writer.Close();

    }
 
    public void Reset () {
        pos.Clear();
        rot.Clear();
        tim.Clear();

        isRec = false;
        startTime = (float) 0.0;
    }

    private void TaskOnRecordClick(bool initRecord)
    {
        if (initRecord){
            hitRecord = true;
            // Wait for countdown to initiate recording: 5.5 sec
            StartCoroutine(WaitForCountdown());
        }
        else{
            hitRecord = false;
            Debug.Log("Starting recording now: " + go.name + " at time " + Time.time);
            if (allowPlaybackRecording){
                isRec = true;
            }
        }
        

    }
    
    private IEnumerator WaitForCountdown(){
        // The time to wait here should reflect the amount of time the countdown takes.
        // Start recording at 2 seconds: "ready + 1 sec + set + 1 sec + GO" 
        // The amount of time the animation takes should have a 1 second buffer,
        //  since the robot doesn't start moving until 1 sec after "GO" (currently)
        yield return new WaitForSecondsRealtime((float) 3.0);
        //Output this to console when Button1 is clicked
        Debug.Log("Starting recording now: " + go.name + " at time " + Time.time);
        isRec=true;
    }

}
