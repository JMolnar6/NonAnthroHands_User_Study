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
    // private ControllerFromLogFile controller;
    private ControllerFullExploration controller;
    private ArticulationBody[] articulationChain;
    private StreamWriter writer;
    
    List<float> angles = new List<float>();
    List<float> tim = new List<float>();    

    // Start is called before the first frame update
    void Start()
    {
        //   isRec = true;
        Button RecordButton = GameObject.Find("Record Button").GetComponent<Button>();
        Button PlayResultButton = GameObject.Find("Play Result Button").GetComponent<Button>();
        // Debug.Log("Record button found");
        RecordButton.onClick.AddListener(TaskOnRecordClick);
        PlayResultButton.onClick.AddListener(TaskOnPlayResultClick);
        // Debug.Log("Recording: " + urdf.name);

        controller = GameObject.Find("Controller").GetComponent<ControllerFullExploration>();
        
        articulationChain = urdf.GetComponentsInChildren<ArticulationBody>();
        
        LaunchCSVfile();
       
    }

    // Update is called once per frame
    void Update()
    {
        animationTime = controller.animationTime; //Note that this doesn't get updated until after the first "record"
        Debug.Log("Animation runtime = " + animationTime.ToString());
        if(isRec == true & startTime == 0.0){ // isRec gets set to "true" upon button click
            startTime = Time.time; 
            playLaunched = false;
            // Debug.Log("Time = " + startTime);
            // Debug.Log("End time = " + (startTime+animationTime).ToString());
        }

        if (isRec == true){

            foreach (ArticulationBody joint in articulationChain)
            {
                joint.GetJointPositions(angles); // This technically grabs all joint positions for the entire hierarchy, 
                                                 // so we don't need to iterate over all joints

                // Debug.Log("Joint position at time "+Time.time.ToString()+" is: "+joint.name+" "+angles.ToString());
                // Debug.Log("Length of angles list is "+angles.Count);
                for (int i=0; i<angles.Count; i++){
                    // Debug.Log("Angle "+i.ToString()+" is: "+angles[i].ToString());
                    // angleString = angleString+","+angles[i].ToString();
                    // Debug.Log("AngleString is: "+angleString);
                }    
            }
            string angleString = "";
            for (int i=0; i<angles.Count; i++){
                // Debug.Log("Angle is: "+angles[i].ToString());
                angleString = angleString+","+angles[i].ToString();
                // Debug.Log("AngleString is: "+angleString);
            }
            // Debug.Log("Writing this line to file: "+angleString);
            writer.WriteLine(Time.time.ToString()+angleString);
        }

        if ((Time.time > animationTime + startTime) & (!playLaunched)){ 
                // Debug.Log("Recording jointangles complete at " + Time.time.ToString());
                playLaunched = true;
                isRec = false;
                iteration++;
                // Debug.Log("Jointfile Iteration = "+ iteration);
                Reset();
        }
    }
 
    public void Reset () {
        angles.Clear();
        tim.Clear();

        writer.Flush();
        writer.Close();

        isRec = false;
        startTime = (float) 0.0;
        
        LaunchCSVfile();
    }

    private void TaskOnRecordClick()
    {
        // Wait for countdown to initiate recording: 5.5 sec
        StartCoroutine(WaitForCountdown());
    }

    private void TaskOnPlayResultClick()
    {
        isRec=true;
    }
    
    private IEnumerator WaitForCountdown(){
        yield return new WaitForSecondsRealtime((float) 5.5);
        //Output this to console when Button1 is clicked
        // Debug.Log("Starting recording now: " + urdf.name + " at time " + Time.time);
        isRec=true;
    }

    private void LaunchCSVfile(){
        string filePath = Application.persistentDataPath + "/" + urdf.name + "_JointMotion_" + iteration + ".csv";
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
        
    }

}
