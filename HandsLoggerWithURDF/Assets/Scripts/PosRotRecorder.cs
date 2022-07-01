using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class PosRotRecorder : MonoBehaviour
{
    public int iteration = 1;
    public Transform go;

    // public Button RecordButton;

    private bool isRec = false;
    private bool playLaunched = false;
    private float startTime = 0;
    
    List<Vector3> pos = new List<Vector3>();
    List<Quaternion> rot = new List<Quaternion>();
    List<float> tim = new List<float>();    

    // Start is called before the first frame update
    void Start()
    {
        //   isRec = true;
        Button RecordButton = GameObject.Find("Record Button").GetComponent<Button>();
        Debug.Log("Record button found");
        RecordButton.onClick.AddListener(TaskOnClick);
        Debug.Log("Recording name: " + go.name);
    }

    // Update is called once per frame
    void Update()
    {
        if(isRec == true & startTime == 0.0){ // isRec gets set to "true" upon button click
            startTime = Time.time; 
            playLaunched = false;
            Debug.Log("Time = " + startTime);
        }

        if (isRec == true){
            Vector3 tempPos = go.position;
            Quaternion tempRot = go.rotation;

            pos.Add(tempPos);
            rot.Add(tempRot);
            tim.Add(Time.time);
            
            Debug.Log("Position at time " + Time.time + " = " + go.position);
            Debug.Log("Rotation at time " + Time.time + " = " + go.rotation);            
        }

        if ((Time.time - startTime > 15) & (!playLaunched)){
                Debug.Log("Recording complete.");
                playLaunched = true;
                isRec = false;
                LogAndConfirm();
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
        
        // Debug.Log("filepath directory = " + Application.persistentDataPath + "/" + go.name + "_Motion_" + iteration + ".csv");
        
        // string filePath = Application.persistentDataPath + "/Data/" + "goMotion";
        string filePath = Application.persistentDataPath + "/" + go.name + "_Motion_" + iteration + ".csv";
        Debug.Log("filepath = " + filePath);
        
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

    private void TaskOnClick()
    {
        //Output this to console when Button1 is clicked
        Debug.Log("Starting recording now: " + go.name);
        isRec=true;

    }

}
