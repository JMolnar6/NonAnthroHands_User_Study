using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class JointRecorder : MonoBehaviour
{
    public int iteration = 1;
    public GameObject urdf;

    private bool isRec = false;
    private bool playLaunched = true;
    private float startTime = 0;
    private float animationTime = 15;
    private ControllerFromLogFile controller;
    private ArticulationBody[] articulationChain;
    private StreamWriter writer;
    
    List<float> angles = new List<float>();
    List<float> tim = new List<float>();    

    // Start is called before the first frame update
    void Start()
    {
        //   isRec = true;
        Button RecordButton = GameObject.Find("Record Button").GetComponent<Button>();
        // Debug.Log("Record button found");
        RecordButton.onClick.AddListener(TaskOnRecordClick);
        Debug.Log("Recording: " + urdf.name);

        controller = GameObject.Find("Controller").GetComponent<ControllerFromLogFile>();
        
        articulationChain = urdf.GetComponentsInChildren<ArticulationBody>();
        
        string filePath = Application.persistentDataPath + "/" + urdf.name + "_JointMotion_" + iteration + ".csv";
        Debug.Log("filepath = " + filePath);
        
        writer = new StreamWriter(filePath);
        string headerLine = "Time,";
        foreach (ArticulationBody joint in articulationChain){
            // Debug.Log("Joint name is " + joint.name);
            headerLine = headerLine+joint.name+",";
        }
        writer.WriteLine(headerLine);
    }

    // Update is called once per frame
    void Update()
    {
        animationTime = controller.animationTime;
        // Debug.Log("Animation runtime = " + animationTime.ToString());
        if(isRec == true & startTime == 0.0){ // isRec gets set to "true" upon button click
            startTime = Time.time; 
            playLaunched = false;
            // Debug.Log("Time = " + startTime);
            // Debug.Log("End time = " + (startTime+animationTime).ToString());
        }

        if (isRec == true){

            foreach (ArticulationBody joint in articulationChain)
            {
                // ArticulationDrive currentDrive = joint.xDrive;
                joint.GetJointPositions(angles);
                Debug.Log("Joint position at time "+Time.time.ToString()+" is: "+joint.name+" "+angles.ToString());
                // Debug.Log("Length of angles list is "+angles.Count);
                
            }
            string angleString = "";
            for (int i=0; i<angles.Count; i++){
                // Debug.Log("Angle is: "+angles[i].ToString());
                angleString = angleString+","+angles[i].ToString();
                Debug.Log("AngleString is: "+angleString);
            }
            Debug.Log("Writing this line to file: "+angleString);
            writer.WriteLine(Time.time.ToString()+angleString);
        }

        if ((Time.time > animationTime + startTime) & (!playLaunched)){ 
                Debug.Log("Recording complete at " + Time.time.ToString());
                playLaunched = true;
                isRec = false;
                Reset();
                iteration++;
                Debug.Log("Iteration = "+ iteration);
        }
    }
 
    public void Reset () {
        angles.Clear();
        tim.Clear();

        writer.Flush();
        writer.Close();

        isRec = false;
        startTime = (float) 0.0;
    }

    private void TaskOnRecordClick()
    {
        // Wait for countdown to initiate recording: 5.5 sec
        StartCoroutine(WaitForCountdown());
        

    }
    
    private IEnumerator WaitForCountdown(){
        yield return new WaitForSecondsRealtime((float) 5.5);
        //Output this to console when Button1 is clicked
        Debug.Log("Starting recording now: " + urdf.name + " at time " + Time.time);
        isRec=true;
    }

}
