using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int OculusID;
    
    private JSONObject statusUpdate;
    private string HandName;
    private GameObject leftHand;
    private GameObject rightHand;

    private TextMeshPro DebugReport;
    // private List<GameObject> MyTeam = new List<GameObject>();
    

    // Start is called before the first frame update
    void Start()
    {
        // Start by figuring out which team I'm on: 
        // Generate an OculusID and send to http://server/register
        // The reply will include the OculusIDs and Team (either 0 or 1)
        
        var now = System.DateTime.UtcNow.ToBinary().ToString();
        // Debug.Log(now);
        // Debug.Log(now.Substring(now.Length-7));
        int OculusID = int.Parse(now.Substring(now.Length-7));
        Debug.Log("OculusID = " + OculusID.ToString());

        

        StartCoroutine(GetTeamNumber(OculusID));

        // Now we know what team we're on (0=Blue, 1=Red). We won't change this from here on out

        // // instantiate the flags at the base
        // redFlag = Instantiate(redFlagPrefab, GameObject.Find("RedGoal").transform.position, GameObject.Find("RedGoal").transform.rotation);
        // blueFlag = Instantiate(blueFlagPrefab, GameObject.Find("BlueGoal").transform.position, GameObject.Find("RedGoal").transform.rotation);

        // redScoreText = GameObject.Find("Red Score Text").GetComponent<TextMeshPro>();
        // blueScoreText = GameObject.Find("Blue Score Text").GetComponent<TextMeshPro>();

        // // Collect players (ghost players are the user-movable ones)
        // for (int i=1;i<=4;i++){
            
        //     GameObject playerObj = GameObject.Find("anki_cozmo ("+i.ToString()+")");
        //     MyTeam.Add(playerObj);
        //     // Debug.Log("My team member added: "+playerObj.name);

        //     playerObj = GameObject.Find("anki_cozmo_ghost ("+i.ToString()+")");
        //     MyGhostTeam.Add(playerObj);
        //     // Debug.Log("My ghost team member added: "+playerObj.name);

        //     playerObj = GameObject.Find("anki_cozmo ("+(i+4).ToString()+")");
        //     OpposingTeam.Add(playerObj);
        //     // Debug.Log("Opposing team member added: "+playerObj.name);
        // }
        
        StartCoroutine(LocationCycle());
        
    }

    // Update is called once per frame
    void Update()
    {
        // Use OVRInput and IsGrabbable Script to notice when a hand has grabbed a robot. Flag that robot and the
        // hand that's moving it. Upon release, (wait half a second and) update the waypoint location (send via http:.../put)
        
        // OVRGrabber leftHand = ;
        StartCoroutine(SendWaypoint());

    }

    IEnumerator LocationCycle(){
        while(true){
            // StartCoroutine(GetRequest("http://1f82-2610-148-1f02-3000-b082-3101-7d86-202.ngrok.io/get")); 
            StartCoroutine(GetRequest("http://143.215.128.175:4193/get/")); 
            yield return new WaitForSeconds(0.1f);


            GameObject go;
            // DO STUFF HERE TO URDF
            // if (HandName == "LeftHand"){
            //     for (int i = 1; i<=4; i++){
            //         // ignore if the robot does not exist in the data sent
            //         if (!statusUpdate["LeftHand"]["cozmo_" + i.ToString()]) {
            //             //Debug.Log("No data for cozmo_" + i.ToString());
            //             continue;
            //         }

            //         // get the game object for the robot
            //         go = GameObject.Find("anki_cozmo ( " + i.ToString() + ")");

            //         // update the robot's location
            //         float location_x = (float) statusUpdate["LeftHand"]["cozmo_" + i.ToString()]["Location"][0].n;
            //         float location_y = (float) statusUpdate["LeftHand"]["cozmo_" + i.ToString()]["Location"][1].n;
            //         go.transform.position = new Vector3(location_x + offset_x, 0.75f, location_y + offset_y);

            //         // update HasRedFlag
            //         if (statusUpdate["LeftHand"]["cozmo_" + i.ToString()]["HasRedFlag"].b) {
            //             redFlag.transform.position = new Vector3(location_x + offset_x, 0.85f, location_y + offset_y);
            //         }
                    
            //         // update HasBlueFlag                
            //         if (statusUpdate["LeftHand"]["cozmo_" + i.ToString()]["HasBlueFlag"].b) {
            //             blueFlag.transform.position = new Vector3(location_x + offset_x, 0.85f, location_y + offset_y);
            //         }           
            //     }
        
            //     for (int i = 5; i<=8; i++) {
                    
            //         // update location
            //         if (!statusUpdate["RightHand"]["cozmo_" + i.ToString()]) {
            //             //Debug.Log("No data for cozmo_" + i.ToString());
            //             continue;
            //         }

            //         go = GameObject.Find("anki_cozmo ( " + i.ToString() + ")");

            //         // update the robot's location
            //         float location_x = (float) statusUpdate["RightHand"]["cozmo_" + i.ToString()]["Location"][0].n;
            //         float location_y = (float) statusUpdate["RightHand"]["cozmo_" + i.ToString()]["Location"][1].n;
            //         go.transform.position = new Vector3(location_x + offset_x, 0.75f, location_y + offset_y);

            //         // update HasRedFlag
            //         if (statusUpdate["RightHand"]["cozmo_" + i.ToString()]["HasRedFlag"].b) {
            //             redFlag.transform.position = new Vector3(location_x + offset_x, 0.85f, location_y + offset_y);
            //         }
                    
            //         // update HasBlueFlag                
            //         if (statusUpdate["RightHand"]["cozmo_" + i.ToString()]["HasBlueFlag"].b) {
            //             blueFlag.transform.position = new Vector3(location_x + offset_x, 0.85f, location_y + offset_y);
            //         }  

            //     }
            // }
            // if (HandName == "RightHand"){
            //     for (int i = 1; i<=4; i++){
            //         // ignore if the robot does not exist in the data sent
            //         if (!statusUpdate["RightHand"]["cozmo_" + i.ToString()]) {
            //             //Debug.Log("No data for cozmo_" + i.ToString());
            //             continue;
            //         }

            //         // get the game object for the robot
            //         go = GameObject.Find("anki_cozmo ( " + i.ToString() + ")");

            //         // update the robot's location
            //         float location_x = (float) statusUpdate["RightHand"]["cozmo_" + i.ToString()]["Location"][0].n;
            //         float location_y = (float) statusUpdate["RightHand"]["cozmo_" + i.ToString()]["Location"][1].n;
            //         go.transform.position = new Vector3(location_x + offset_x, 0.75f, location_y + offset_y);

            //         // update HasRedFlag
            //         if (statusUpdate["RightHand"]["cozmo_" + i.ToString()]["HasRedFlag"].b) {
            //             redFlag.transform.position = new Vector3(location_x + offset_x, 0.85f, location_y + offset_y);
            //         }
                    
            //         // update HasBlueFlag                
            //         if (statusUpdate["RightHand"]["cozmo_" + i.ToString()]["HasBlueFlag"].b) {
            //             blueFlag.transform.position = new Vector3(location_x + offset_x, 0.85f, location_y + offset_y);
            //         }           
            //     }
        
                // for (int i = 5; i<=8; i++) {
                    
                //     // update location
                //     if (!statusUpdate["LeftHand"]["cozmo_" + i.ToString()]) {
                //         //Debug.Log("No data for cozmo_" + i.ToString());
                //         continue;
                //     }

                //     go = GameObject.Find("anki_cozmo ( " + i.ToString() + ")");

                //     // update the robot's location
                //     float location_x = (float) statusUpdate["LeftHand"]["cozmo_" + i.ToString()]["Location"][0].n;
                //     float location_y = (float) statusUpdate["LeftHand"]["cozmo_" + i.ToString()]["Location"][1].n;
                //     go.transform.position = new Vector3(location_x + offset_x, 0.75f, location_y + offset_y);

                //     // update HasRedFlag
                //     if (statusUpdate["LeftHand"]["cozmo_" + i.ToString()]["HasRedFlag"].b) {
                //         redFlag.transform.position = new Vector3(location_x + offset_x, 0.85f, location_y + offset_y);
                //     }
                    
                //     // update HasBlueFlag                
                //     if (statusUpdate["LeftHand"]["cozmo_" + i.ToString()]["HasBlueFlag"].b) {
                //         blueFlag.transform.position = new Vector3(location_x + offset_x, 0.85f, location_y + offset_y);
                //     }  

                // }
            // }
        }
    }
    IEnumerator SendWaypoint(){
        Debug.Log("Now sending waypoints.");
        WWWForm form = new WWWForm();
        // Team: (0,1)
        // Robot: e.g. cozmo_1
        // Field: Location OR Waypoint
        // Value: [x,y] where (x,y) e [0,1]
        // OculusID: int

        // Dummy variables for now:

        // float waypoint_x = linkedGhost.transform.position.x + 0.5f;
        // float waypoint_y = linkedGhost.transform.position.z - 0.25f;
        // waypoint = new Vector2(waypoint_x, waypoint_y);
        // waypointMarker.transform.position = new Vector3(linkedGhost.transform.position.x, 0.75f, linkedGhost.transform.position.z);

        // form.AddField("Hand", HandName);
        // form.AddField("Position", Position);
        // form.AddField("Field", "Waypoint");
        // form.AddField("Value", "[" + waypoint_x + "," + waypoint_y + "]");
        form.AddField("OculusId", OculusID);

        UnityWebRequest www = UnityWebRequest.Post("http://143.215.128.175:4193/put", form); //http://1f82-2610-148-1f02-3000-b082-3101-7d86-202.ngrok.io

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) { Debug.Log(www.error); }
        else { Debug.Log("added waypoint!"); }

    //     // send the ghost back to the robot
    //     linkedGhost.transform.position = transform.position;
    //     linkedGhost.transform.rotation = transform.rotation;
    }

    // from https://forum.unity.com/threads/turn-string-into-list-of-ints.340341/
    public List<float> GetFloatsFromString(string str){
        str = str.Substring(1, str.Length);
        List<float> floats = new List<float>();
     
        string[] splitString = str.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string item in splitString)
        {
            try
            {
                floats.Add(float.Parse(item));
            }
            catch (System.Exception e)
            {
                Debug.LogError("Value in string was not an int.");
                Debug.LogException(e);
            }
        }
        return floats;
    }


    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    statusUpdate = new JSONObject(webRequest.downloadHandler.text); //JsonUtility.FromJson<GSRequest>(webRequest.downloadHandler.text);
                    break;
            }
        }
    }

    IEnumerator GetTeamNumber(int HeadsetID)
    {
        WWWForm form = new WWWForm();

        form.AddField("OculusId", HeadsetID);

        UnityWebRequest www = UnityWebRequest.Post("http://143.215.128.175:4193/register", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!\n" + www.downloadHandler.text);
            GSRequest TeamIDUpdate = JsonUtility.FromJson<GSRequest>(www.downloadHandler.text);
            if (TeamIDUpdate.Status == 200){
                Debug.Log("Team is logged in!");
                // Parse return value to determine team: either 0 or 1
                int TeamID = TeamIDUpdate.HandName;
                if (TeamID == 0){
                    HandName = "LeftHand";
                    Debug.Log("TeamID is " + TeamID + ". You are the " + HandName);
                    GameObject.Find("Team Text").GetComponent<TextMeshPro>().SetText("Team: " + HandName);
                    
                }
                if (TeamID == 1){
                    HandName = "RightHand";
                    Debug.Log("TeamID is " + TeamID + ". You are the " + HandName);
                    GameObject.Find("Team Text").GetComponent<TextMeshPro>().SetText("Team: " + HandName);
                }
            }
            else {
                Debug.Log("Error registering: code " + TeamIDUpdate.Status.ToString());
            }
        }
    }
 

    [System.Serializable]
    public class HandStats
    {
        public bool isLeft;
        public List<float> Position;
        public List<float> Orientation;
        public bool ButtonA;
        public bool ButtonB;
        public float JoystickUpDown;
        public float JoystickLeftRight;
        public bool JoystickPress;
        public float IndexTriggerPress;
        public float GripTriggerPress;
    }

    [System.Serializable]
    public class GSRequest
    {
        public int HandName;
        public string Robot;
        public string Field;
        public List<float> Value;
        public int OculusId;
        public int Status;

        public Dictionary<string, HandStats> LeftHand; 
        public Dictionary<string, HandStats> RightHand; 
    }
}