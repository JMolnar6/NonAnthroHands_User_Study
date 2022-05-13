using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class RecordHandData : MonoBehaviour
{
    bool isRec = false;
    bool playLaunched = false;
    
    public bool isLeft = false;
    public GameObject hand;
    public GameObject handPrefab;

    private GameObject duplicateHand;
    
    // List<float> nums = new List<float>();
    List<Vector3> pos = new List<Vector3>();
    List<Quaternion> rot = new List<Quaternion>();
    List<float> tim = new List<float>();

    List<Vector3> ft1 = new List<Vector3>();
    

    // Start is called before the first frame update
    void Start()
    {
          isRec = true;
    }

    // Update is called once per frame
    void Update()
    {
        if( isRec == true){
            Debug.Log("Time = " + Time.time);

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

        if ((Time.time > 5) & (!playLaunched)){
            Debug.Log("Playback sequence initiated.");
            playLaunched = true;
            isRec = false;
            duplicateHand = GameObject.Instantiate(handPrefab, hand.transform.position, hand.transform.rotation);
            RunIt();
        }
    }

    public IEnumerator Playback ()
    { 
        // playedNoRep = true;
        
        // output log file of user motions and times
        
        Debug.Log("filepath directory = " + Application.dataPath);
        string filePath = Application.dataPath + "/Data/" + "HandMotion";
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


        for (int i = 0; i < pos.Count; i+=3) {

            duplicateHand.transform.position = pos[i];
            duplicateHand.transform.rotation = rot[i];

            yield return null;
        }
    }
 
    public void Reset () {
        pos.Clear();
        rot.Clear();
        // Application.LoadLevel("SciFi Level");
    }
    public void RunIt () {
        StartCoroutine("Playback");
    }
}
