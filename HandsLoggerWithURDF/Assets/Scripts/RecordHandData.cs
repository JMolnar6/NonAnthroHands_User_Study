using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.EventSystems;
using System.IO;

public class RecordHandData : MonoBehaviour
{
    public bool isRec = false;
    bool playLaunched = false;
    
    public bool isLeft = false;
    public GameObject hand;
    public GameObject handPrefab;

    public Button m_StartButton;

    private GameObject duplicateHand;
    private float startTime = 0;
    
    // List<float> nums = new List<float>();
    List<Vector3> pos = new List<Vector3>();
    List<Quaternion> rot = new List<Quaternion>();
    List<float> tim = new List<float>();

    List<Vector3> ft1 = new List<Vector3>();
    

    // Start is called before the first frame update
    void Start()
    {
        //   isRec = true;
        m_StartButton.onClick.AddListener(TaskOnClick);
    }

    // Update is called once per frame
    void Update()
    {
        if(isRec == true & startTime == 0.0){ // isRec gets set to "true" upon button click
            startTime = Time.time; 
            Debug.Log("Time = " + startTime);
        }

        if (isRec == true){
            Vector3 tempPos = hand.transform.position;
            Quaternion tempRot = hand.transform.rotation;

            // float tempX = hand.transform.position.x;
            // float tempY = hand.transform.position.y;
            // float tempZ = hand.transform.position.z;
            // nums.Add(tempX);
            // nums.Add(tempY);
            // nums.Add(tempZ);

            pos.Add(tempPos);
            rot.Add(tempRot);
            tim.Add(Time.time);
            
            Debug.Log("Position at time " + Time.time + " = " + hand.transform.position);
            Debug.Log("Rotation at time " + Time.time + " = " + hand.transform.rotation);            
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
        duplicateHand = GameObject.Instantiate(handPrefab, hand.transform.position, hand.transform.rotation);
        StartCoroutine("Playback");
        Reset();
    }

    private void WriteLogFile(){
        // output log file of user motions and times
        
        Debug.Log("filepath directory = " + Application.persistentDataPath);
        // string filePath = Application.persistentDataPath + "/Data/" + "HandMotion";
        string filePath = Application.persistentDataPath + "/HandMotion";
        if(isLeft){
            filePath = filePath + "_left.csv";
        }
        else{
            filePath = filePath + "_right.csv";
        }
        Debug.Log("filepath = " + filePath);
        
        StreamWriter writer = new StreamWriter(filePath);
        writer.WriteLine("Time, Position (X), Position (Y), Position (Z), Orientation (X), Orientation (Y), Orientation (Z)");
        for (int i = 0; i < tim.Count; i++) {
            writer.WriteLine(tim[i]+","+pos[i][0]+","+pos[i][1]+","+pos[i][2]+","+rot[i][0]+","+rot[i][1]+","+rot[i][2]);
        }
        writer.Flush();
        writer.Close();

    }

    public IEnumerator Playback ()
    { 
        for (int i = 0; i < pos.Count-1; i+=3) {

            duplicateHand.transform.position = pos[i];
            duplicateHand.transform.rotation = rot[i];

            yield return new WaitForSecondsRealtime((float) tim[i+1] - tim[i]);
        }
    }
 
    public void Reset () {
        pos.Clear();
        rot.Clear();
        tim.Clear();
    }

    private void TaskOnClick()
    {
        //Output this to console when Button1 is clicked
        Debug.Log("Starting now! (hand recorder)");
        isRec=true;

    }

}
