using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


/// <summary>
/// target Controller is built to accept 3-DOF control inputs and to control the position of the tentacle tip (via "target") accordingly
/// </summary>

public class TentacleControllerMultiSegment : MonoBehaviour {
    
    private bool questConnected = false;
    // private float growth = 0;
    public float tentacleLength = (float) 0.325; // Approximately half the length of FBX tentacle file + sphere radius
    private Vector3[] vertices;
    
    //public Spline spline; //Should match the spline being used to generate the tentacle
    public GameObject baseSegmentBottom = null;
    public GameObject tipSegmentBottom = null;

    public GameObject targetBase = null;
    public GameObject poleBase = null;
    public GameObject targetTip = null;
    public GameObject poleTip = null;
    
    
    public int schemeSelection = 2;
    public float speed = (float) 0.01;
    // KEYBOARD ZONES:
    // 0 = QWE/ASD
    // 1 = UIO/JKL
    // 2 = RTY/FGH
    // 3 = ... invent your own and add the appropriate if statements in the GatherControls(Keyboard/Oculus) functions
    // Recognize that this is for test and debug; the tentacle will actually be pre-animated by specifying the location 
    //  of the end-effector and poleBase, and then the 6DOF position/rotation recordings will be trained to match the 
    //  tentacle motion
    // Also recognize that we are constraining the QCP so that the tentacle does not bend sideways. Otherwise there would
    //  be 6DOF in each segment instead of 3DOF.
    // public int keyboardZone = 0; 
    
    public float TentacleLength {
            get { return tentacleLength; }
            set {
                if (value == tentacleLength) return;
                //SetDirty();
                tentacleLength = value;
            }
        }
    


    private void Start() {
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices)
        {
            Debug.Log(string.Format("Device found with name '{0}' and role '{1}'", device.name, device.characteristics.ToString()));
            questConnected = true;
        }


    }

    void FixedUpdate() {
        Contort(targetBase, poleBase, 0);
        Contort(targetTip, poleTip, 1);


    }

    public void Contort(GameObject target, GameObject pole, int keyboardZone){
        Vector3 controls = new Vector3();

    // Here is where you can build your own hardware I/O method to pull in 3dof that will control
    //  how the tentacle should move. Two options are given here, one for VR and one for a flat screen.
        if (questConnected){
            controls = GatherControlsVR(keyboardZone);
            Debug.Log("Oculus connected. Listening to hand controller input.");
        }
        else{
            controls = GatherControlsKeyboard(keyboardZone);
            Debug.Log("Oculus not connected. Listening to keyboard input.");
        }

        Vector3[] kinematics = new Vector3[2];
        if (schemeSelection == 1){
            kinematics = PositionChangeCartesian(target, pole, controls);
            Debug.Log("Cartesian control scheme chosen (1).");
        }
        else if (schemeSelection == 2){
            kinematics = PositionChangeCylindrical(target, pole, controls);
            Debug.Log("Cylindrical control scheme chosen (2).");
        }
        else if (schemeSelection == 3){
            kinematics = PositionChangeSpherical(target, pole, controls);
            Debug.Log("Spherical control scheme chosen (3).");
        }
        else if (schemeSelection == 4){
            kinematics = PositionChangeOdd(target, pole, controls);
            Debug.Log("Odd control scheme chosen (4).");
        }
        else{
            Debug.Log("Choose a valid scheme selection (integer, 1-4)");
        }

        Vector3 position = kinematics[0];  //transform position of target/tip
        Vector3 direction = kinematics[1]; // transform position of pole
        Debug.Log("target location "+ keyboardZone + " = "+ kinematics[0]);
        Debug.Log("poleBase location "+ keyboardZone + " = "+ kinematics[1]);

        // Debug.Log(string.Format("position is: ", position.ToString()));
        // Debug.Log(string.Format("direction is: ", direction.ToString()));

        //  base_length = Sqrt(x^2+y^2+z^2)
        float base_length = Mathf.Sqrt(Mathf.Pow(position.x,2)+Mathf.Pow(position.y,2)+Mathf.Pow(position.z,2));
        // float base_length = 0f; //INSERTED FOR DEBUGGING PURPOSES. THE target IS NO LONGER REQUIRED TO BE WITHIN THE UNIT SPHERE, SO THIS SHOULD BE IRRELEVANT ANYWAY

            // Stop and check to see if tentacle tip has been moved out of bounds
            if (base_length<=tentacleLength){
                //If not, then we're good to go. 
                
                // ~ QCP DIRECTION DETERMINATION: ~
                // The control points, or "directions" for node0 and node1, are points 2/3 of the way towards the QCP on 
                //  a line from the node to the QCP
                Vector3 rotation = baseSegmentBottom.transform.eulerAngles;
                Debug.Log("Rotation = "+rotation);

                Vector3 qcposition = GetQCPos(base_length, position, rotation);

                // Position  of node0: always (0,0,0)
                // Direction of node0: 
                // spline.nodes[0].Direction = (float)2.0/(float)3.0*(qcposition); // Is actually 2/3*(qcposition - (0,0,0))
                pole.transform.position = qcposition; 
                // Debug.Log("pole position transformed");

                // Direction of node1: 
                // Vector3 direction1 = (float)2.0/(float)3.0*(position - qcposition)+position;

                // // Set position and direction for node1
                // spline.nodes[1].Position  = position;
                // spline.nodes[1].Direction = direction1;
                target.transform.position = position;
                
                // Debug.Log("Tip position transformed");


                // vertices = new Vector3[3];
                // vertices[0] = qcposition; //black
                // vertices[1] = spline.nodes[0].Direction; //red
                // vertices[2] = spline.nodes[1].Direction; //blue
            }
            
            // return spline;

        // Insert your own I/O method for a different hardware controller. Should return a Vector3. 
        //  Comment the particular 
        // Optional 4-DOF and 6-DOF controls for tentacle are described ***
        
    }


    private Vector3 GatherControlsKeyboard(int keyboardZone) {
        Vector3 controls = Vector3.zero;
        if (keyboardZone==0){
            // Debug.Log("Keyboard zone = 0");
            if (Input.GetKey("w")){
                controls.y++;
                // Debug.Log("Hit w!");
            }
            if (Input.GetKey("s")){
                controls.y--;
            }
            if (Input.GetKey("a")){
                controls.z++;
            }
            if (Input.GetKey("d")){
                controls.z--;
            }
            if (Input.GetKey("q")){
                controls.x++;
            }
            if (Input.GetKey("e")){
                controls.x--;
            }
        }
        else if (keyboardZone==1){
            // Debug.Log("Keyboard zone = 1");
            if (Input.GetKey("i")){
                controls.y++;
                // Debug.Log("Hit i!");
            }
            if (Input.GetKey("k")){
                controls.y--;
            }
            if (Input.GetKey("j")){
                controls.z++;
            }
            if (Input.GetKey("l")){
                controls.z--;
            }
            if (Input.GetKey("u")){
                controls.x++;
            }
            if (Input.GetKey("o")){
                controls.x--;
            }
        }
        else if (keyboardZone==2){
            // Debug.Log("Keyboard zone = 2");
            if (Input.GetKey("t")){
                controls.y++;
                // Debug.Log("Hit t!");
            }
            if (Input.GetKey("g")){
                controls.y--;
            }
            if (Input.GetKey("f")){
                controls.z++;
            }
            if (Input.GetKey("h")){
                controls.z--;
            }
            if (Input.GetKey("r")){
                controls.x++;
            }
            if (Input.GetKey("y")){
                controls.x--;
            }
        }
        // Debug.Log("Control signal = " + controls.ToString());
        return controls;
    }

    private Vector3 GatherControlsVR(int keyboardZone){
        // NB: VR controls are currently joystick-based, not based on the hand position/orientation. This is for testing purposes.
        Vector3 controls = Vector3.zero;
        if (keyboardZone==0){
            controls[0] = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick)[0];
            controls[1] = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick)[1];
            controls[2] = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) - OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger);
        }
        else if (keyboardZone==1){
            controls[0] = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick)[0];
            controls[1] = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick)[1];
            controls[2] = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) - OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);
        }

        return controls;
    }

    
    private Vector3 GetQCPos(float base_length, Vector3 position, Vector3 rotation){
        // Algorithm: 
        // If changing position will make spline.Length>tentacleLength, do not change position.
        // (The tentacle's length will not stay absolutely constant, but we will position the control point so that
        //  it stays roughly the same.)

        // The "Direction" tells you the location of the control points: 
        // We are imitating a quadratic spline, which uses a single control point instead of one per node. 
        //  We will position it equidistant from both endpoints, at the point that is (tentacleLength/2) from 
        //  either node, in the vertical plane.
        // The cubic control point (i.e. the new "direction" for each end point) should then be 2/3 of the way 
        //  towards the quadratic control point (QCP).
        // Keep in mind that the direction for the different nodes will be opposite (+/-) in order for there to be a
        //  C-shaped and not an S-shaped curve

        // Find QCP by creating an isoceles triangle with base between origin and endpt; 
        //  length of symmetric sides = (tentacleLength/2)
        // If base were flat and on the x axis, QCP would be at the point <0,h,0>,
        //  where h = (by the Pythagorean theorem, applied to one of the two right triangles made by bisecting the isoceles triangle)
        //  h = Sqrt((side_length)^2 - (base_length/2)^2)
        float h = Mathf.Sqrt(Mathf.Pow(tentacleLength/2,2)-Mathf.Pow(base_length/2,2));

        // Next we need to rotate this point (0,h,0) by an angle, arctan(y/base_length) about the z axis
        //  and then by arctan(z/x) about the y axis (in the world frame).
        // 
        // QCP = Ry(arctan(z/x)) * Rz(arctan(y/base_length)) * <base_length/2,h,0>
            
        // Unity has lovely rotation functions for Transforms, but Transforms only work on Game Objects, not on nodes.
        //  We'll do this manually instead, with Matrix4x4s. NOTE: Unity has a left-handed coordinate system, so the 
        //  rotational transformation matrices will look a little different than usual.

        float angle1 = Mathf.Atan2(position.z,position.x);
        float angle2 = Mathf.Atan2(position.y,base_length);
            
        Vector3 height = new Vector3((float) base_length/2, h, (float) 0.0);
        // Vector3 tempPos = spline.nodes[0].Position + height;
        Vector3 tempPos = baseSegmentBottom.transform.position + height; // Useful if the tentacle is anchored to a moving object, i.e. the user's arm
        // Vector3 tempPos = new Vector3(0,0,0) + height; // If the tentacle is anchored in space, we don't need the GameObject transform.
        // Unity uses a left-hand coordinate system. All angles about the z axis are negative
        Matrix4x4 Zrotation = rotateZ(angle2);
        Matrix4x4 Yrotation = rotateY(angle1);

        Vector3 qcposition = Yrotation*Zrotation*tempPos;
        Debug.Log("QCPosition, baseline = " + qcposition);
        qcposition = qcposition + rotation;
        Debug.Log("QCPosition, with rotation adjusted: " + qcposition);
        return qcposition;
    }

    private Matrix4x4 rotateZ(float theta){
        Matrix4x4 rotationMatrix = new Matrix4x4();

        // Unity uses a left-hand coordinate system. All angles about the z axis are negative
        rotationMatrix.SetColumn(0, new Vector4(Mathf.Cos(theta),  Mathf.Sin(theta), 0, 0));
        rotationMatrix.SetColumn(1, new Vector4(-Mathf.Sin(theta), Mathf.Cos(theta), 0, 0));
        rotationMatrix.SetColumn(2, new Vector4(0,0,1,0));
        rotationMatrix.SetColumn(3, new Vector4(0,0,0,1));

        return rotationMatrix;
    }

    private Matrix4x4 rotateY(float theta){
        Matrix4x4 rotationMatrix = new Matrix4x4();

        rotationMatrix.SetColumn(0, new Vector4(Mathf.Cos(theta),  0, Mathf.Sin(theta), 0));
        rotationMatrix.SetColumn(1, new Vector4(0,1,0,0));
        rotationMatrix.SetColumn(2, new Vector4(-Mathf.Sin(theta), 0, Mathf.Cos(theta), 0));
        rotationMatrix.SetColumn(3, new Vector4(0,0,0,1));

        return rotationMatrix;
    }

    private Vector3[] PositionChangeCartesian(GameObject target, GameObject pole, Vector3 controls) {
        // Vector3 position  = spline.nodes[1].Position;
        // Vector3 direction = spline.nodes[1].Direction;
        Vector3 position = target.transform.position;
        Vector3 direction = pole.transform.position;
        
        // Keyboard controls: 
        // i/k = +/- y (up direction)
        // j/l = +/- z
        // u/p = +/- x 

        position.y = position.y + speed*controls.y;
        position.x = position.x + speed*controls.x;
        position.z = position.z + speed*controls.z;
    
        Vector3[] return_vals = new Vector3[2];
        return_vals[0] = position;
        return_vals[1] = direction;
        return return_vals;
    }

    private Vector3[] PositionChangeCylindrical(GameObject target, GameObject pole, Vector3 controls) {
        // Vector3 position  = spline.nodes[1].Position;
        // Vector3 direction = spline.nodes[1].Direction;
        Vector3 position = target.transform.position;
        Vector3 direction = pole.transform.position;

        float angle_speed = speed*2*Mathf.PI/tentacleLength;
        
        // Singularity-catching code:
        if (position.x == 0){
            position.x = 0.000001f;
        }
        if (position.z == 0){
            position.z = 0.000001f;
        }

        float r     = Mathf.Sqrt(Mathf.Pow(position.x,2) + Mathf.Pow(position.z,2));
        float theta = Mathf.Atan2(position.z,position.x);
        // if (r<1){ // Creating a hedge around the singularity
        //     theta = Mathf.Atan2(position.z*1/r,position.x*1/r);
        // }
        if (r<tentacleLength/25){
            r = tentacleLength/25;
        }

        // Keyboard controls: 
        // i/k = +/- y (up direction)
        // j/l = +/- r
        // u/o = +/- theta 

        // I.e. controls = (r,z,theta) where z = up direction, or y in this case

        // There's a singularity at r==0, but for some reason the Mathf.Atan2 function gives weird
        //  results in a rather large radius around it. Expand X/Z coordinates by enough of a multiplicative
        //  factor that Atan2 doesn't give weird results
        
        r = r + controls.x*speed;
        theta = theta + controls.z*angle_speed; //Scale theta increment b/c max is 2Pi instead of tentacleLength
        position.y = position.y + controls.y*speed;
        
        position.x = r*Mathf.Cos(theta);
        position.z = r*Mathf.Sin(theta);

        Vector3[] return_vals = new Vector3[2];
        return_vals[0] = position;
        return_vals[1] = direction;
        return return_vals;
    }

    private Vector3[] PositionChangeSpherical(GameObject target, GameObject pole, Vector3 controls) {
        // Vector3 position  = spline.nodes[1].Position;
        // Vector3 direction = spline.nodes[1].Direction;
        Vector3 position = target.transform.position;
        Vector3 direction = pole.transform.position;

        float angle_speed = speed*2*Mathf.PI/tentacleLength;
        
        float r     = Mathf.Sqrt(Mathf.Pow(position.x,2) + Mathf.Pow(position.y,2) + Mathf.Pow(position.z,2));
        float theta = Mathf.Atan2(position.z,position.x);
        float b = Mathf.Sqrt(Mathf.Pow(position.x,2)+Mathf.Pow(position.z,2));
        float phi   = Mathf.Acos(position.y/r);

        // Keyboard controls: 
        // i/k = +/- phi 
        // j/l = +/- r
        // u/o = +/- theta 
        phi   = phi   + controls.y*angle_speed; //Scale phi increment b/c max is 2Pi instead of tentacleLength
        r     = r     + controls.z*speed;
        theta = theta + controls.x*angle_speed; //If phi is not at a singularity, increment theta
        
        // Creating a hedge around the singularity
        if (b<1){
            theta = Mathf.Atan2(position.z*1/b,position.x*1/b);

            if (phi>Mathf.PI-angle_speed){
                phi = (float) Mathf.PI-angle_speed;
            }
            if (phi<angle_speed){
                phi = (float) angle_speed;
            }
        }

        position.x = r*Mathf.Cos(theta)*Mathf.Sin(phi);
        position.z = r*Mathf.Sin(theta)*Mathf.Sin(phi);
        position.y = r*Mathf.Cos(phi);

        Vector3[] return_vals = new Vector3[2];
        return_vals[0] = position;
        return_vals[1] = direction;
        return return_vals;
    }

    private Vector3[] PositionChangeOdd(GameObject target, GameObject pole, Vector3 controls) {
        // Vector3 position  = spline.nodes[1].Position;
        // Vector3 direction = spline.nodes[1].Direction;
        Vector3 position = target.transform.position;
        Vector3 direction = pole.transform.position;

        float angle_speed = speed*2*Mathf.PI/tentacleLength;
        float b = Mathf.Sqrt(Mathf.Pow(position.x,2)+Mathf.Pow(position.z,2));
        
        float r     = Mathf.Sqrt(Mathf.Pow(position.x,2) + Mathf.Pow(position.y,2) + Mathf.Pow(position.z,2));
        float theta = Mathf.Atan2(position.z,position.x);
        float phi   = Mathf.Acos(position.y/r);

        // Keyboard controls: ***future version should take arbitrary control inputs
        // i/k = +/- phi 
        // j/l = +/- r
        // u/o = +/- theta 

        phi   = phi   + controls.y*angle_speed; //Scale phi increment b/c max is 2Pi instead of tentacleLength
        r     = r     + controls.z*speed;
        theta = theta + controls.x*angle_speed; //If phi is not at a singularity, increment theta
        
        // Creating a hedge around the singularity
        if (b<1){
            if (phi>Mathf.PI-angle_speed){
                phi = (float) Mathf.PI-angle_speed;
            }
            if (phi<angle_speed){
                phi = (float) angle_speed;
            }
        }

        position.x = r*Mathf.Cos(theta)*Mathf.Sin(phi);
        position.z = r*Mathf.Sin(theta)*Mathf.Sin(phi);
        position.y = r*Mathf.Cos(phi);

        Vector3[] return_vals = new Vector3[2];
        return_vals[0] = position;
        return_vals[1] = direction;
        return return_vals;
    }

    

    private void OnDrawGizmos () {
        if (vertices == null) {
            return;
        }
        // else {
        //     Gizmos.color = Color.black;
        //     Gizmos.DrawSphere(vertices[0], 0.05f);
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawSphere(vertices[1], 0.05f);
        //     Gizmos.color = Color.blue;
        //     Gizmos.DrawSphere(vertices[2], 0.05f);
        //     // Gizmos.color = Color.green;
        //     // Gizmos.DrawSphere(vertices[3], 0.75f);
        //     // Gizmos.color = Color.green;
        //     // Gizmos.DrawSphere(vertices[4], 0.75f);
        // }
    // for (int i = 0; i < vertices.Length; i++) {
        // Gizmos.DrawSphere(vertices[i], 0.1f);
    // }
    }
}
