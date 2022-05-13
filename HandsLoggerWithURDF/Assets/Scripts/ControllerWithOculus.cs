using System;
//using Unity.Robotics;
using UnityEngine;
using System.Collections.Generic; // JLM 04/2022
using TMPro;
// using Unity.Robotics.UrdfImporter.Control; // JLM 04/2022

// This script has been modified in order to take inputs from the Oculus Quest. Changed lines are initialed: JM 04/2022

//Commenting again =P

// namespace Unity.Robotics.UrdfImporter.OculusControl
// {
//     public enum RotationDirection { None = 0, Positive = 1, Negative = -1 };
//     public enum ControlType { PositionControl };

    public class ControllerWithOculus : MonoBehaviour
    {
        private ArticulationBody[] articulationChain;
        // Stores original colors of the part being highlighted
        private Color[] prevColor;
        private int previousIndex;
        private Vector2 temp_controls;

        private bool questConnected = false;
        public TextMeshPro DebugReport1;
        public TextMeshPro DebugReport2;
        public TextMeshPro DebugReport3;

        // [InspectorReadOnly(hideInEditMode: true)]
        public string selectedJoint;
        // [HideInInspector]
        public int selectedIndex;

        // public ControlType control = PositionControl;
        public float stiffness;
        public float damping;
        public float forceLimit;
        public float speed = 5f; // Units: degree/s
        public float torque = 100f; // Units: Nm or N
        public float acceleration = 5f;// Units: m/s^2 / degree/s^2
        

        [Tooltip("Color to highlight the currently selected join")]
        public Color highLightColor = new Color(1.0f, 0, 0, 1.0f);

        void Start()
        {
            previousIndex = selectedIndex = 1;
            temp_controls = new Vector2(0,0);
            this.gameObject.AddComponent<Unity.Robotics.UrdfImporter.Control.FKRobot>();
            articulationChain = this.GetComponentsInChildren<ArticulationBody>();
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
            DisplaySelectedJoint(selectedIndex);
            StoreJointColors(selectedIndex);

            // Check for Oculus Quest connection - JLM 04/2022
            var inputDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevices(inputDevices);

            foreach (var device in inputDevices){
                Debug.Log(string.Format("Device found h name '{0}' and role '{1}'", device.name, device.characteristics.ToString()));
                questConnected = true;
            }

            // DebugReport = GameObject.Find("Debug Report").GetComponent<TextMeshPro>();
            if (questConnected){
                DebugReport1.SetText("Debug Info: Quest is connected");// + ((int) statusUpdate["RedTeamScore"].n));
            }
            else {
                DebugReport1.SetText("Debug Info: Quest is not connected;\n listening for keyboard input");// + ((int) statusUpdate["RedTeamScore"].n));
            }
            
        }

        void SetSelectedJointIndex(int index)
        {
            if (articulationChain.Length > 0) 
            {
                selectedIndex = (index + articulationChain.Length) % articulationChain.Length;
            }
        }

        void Update()
        {
            bool SelectionInput1 = false;
            bool SelectionInput2 = false;

            OVRInput.Update();

            Vector2 prev_controls = temp_controls;
            temp_controls = GatherControls();
            if (prev_controls[0]==0 && temp_controls[0]==1){
                SelectionInput1 = true;
            }
            else{
                SelectionInput1 = false;
            }
            if (prev_controls[1]==0 && temp_controls[1]==1){
                SelectionInput2 = true;
            }
            else{
                SelectionInput2 = false;
            }
            Debug.Log("SelectionInput1 = " + SelectionInput1);
            Debug.Log("SelectionInput2 = " + SelectionInput2);
            

            // bool SelectionInput1 = Input.GetKeyDown("right"); 
            // bool SelectionInput2 = Input.GetKeyDown("left");

            SetSelectedJointIndex(selectedIndex); // to make sure it is in the valid range
            UpdateDirection(selectedIndex);

            if (SelectionInput2)
            {
                SetSelectedJointIndex(selectedIndex - 1);
                Highlight(selectedIndex);
            }
            else if (SelectionInput1)
            {
                SetSelectedJointIndex(selectedIndex + 1);
                Highlight(selectedIndex);
            }
            DebugReport1.SetText("Debug Info: selectedIndex = "+ selectedIndex.ToString());
            UpdateDirection(selectedIndex);
        }

        /// <summary>
        /// Highlights the color of the robot by changing the color of the part to a color set by the user in the inspector window
        /// </summary>
        /// <param name="selectedIndex">Index of the link selected in the Articulation Chain</param>
        private void Highlight(int selectedIndex)
        {
            if (selectedIndex == previousIndex || selectedIndex < 0 || selectedIndex >= articulationChain.Length) 
            {
                return;
            }

            // reset colors for the previously selected joint
            ResetJointColors(previousIndex);

            // store colors for the current selected joint
            StoreJointColors(selectedIndex);

            DisplaySelectedJoint(selectedIndex);
            Renderer[] rendererList = articulationChain[selectedIndex].transform.GetChild(0).GetComponentsInChildren<Renderer>();

            // set the color of the selected join meshes to the highlight color
            foreach (var mesh in rendererList)
            {
                Unity.Robotics.MaterialExtensions.SetMaterialColor(mesh.material, highLightColor);
            }
        }

        void DisplaySelectedJoint(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= articulationChain.Length) 
            {
                return;
            }
            selectedJoint = articulationChain[selectedIndex].name + " (" + selectedIndex + ")";
        }

        /// <summary>
        /// Sets the direction of movement of the joint on every update
        /// </summary>
        /// <param name="jointIndex">Index of the link selected in the Articulation Chain</param>
        private void UpdateDirection(int jointIndex)
        {
            if (jointIndex < 0 || jointIndex >= articulationChain.Length) 
            {
                return;
            }

            float moveDirection = (float) 0.0;

            if (questConnected){
                if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickUp)){
                    moveDirection = (float) 1;
                    DebugReport3.SetText("Up button active.");
                }
                else if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown)){
                    moveDirection = (float) -1;
                    DebugReport3.SetText("Down button active.");
                }
                else{
                    moveDirection = (float) 0;
                    DebugReport3.SetText("No movement requested.");
                }
            }
            else{
                moveDirection = Input.GetAxis("Vertical");
            }
            
            JointControl current = articulationChain[jointIndex].GetComponent<JointControl>();
            if (previousIndex != jointIndex)
            {
                JointControl previous = articulationChain[previousIndex].GetComponent<JointControl>();
                previous.direction = Unity.Robotics.UrdfImporter.Control.RotationDirection.None;
                previousIndex = jointIndex;
            }

            // if (current.controltype != control) 
            // {
            //     UpdateControlType(current);
            // }

            if (moveDirection > 0)
            {
                current.direction = Unity.Robotics.UrdfImporter.Control.RotationDirection.Positive;
            }
            else if (moveDirection < 0)
            {
                current.direction = Unity.Robotics.UrdfImporter.Control.RotationDirection.Negative;
            }
            else
            {
                current.direction = Unity.Robotics.UrdfImporter.Control.RotationDirection.None;
            }
        }

        /// <summary>
        /// Stores original color of the part being highlighted
        /// </summary>
        /// <param name="index">Index of the part in the Articulation chain</param>
        private void StoreJointColors(int index)
        {
            Renderer[] materialLists = articulationChain[index].transform.GetChild(0).GetComponentsInChildren<Renderer>();
            prevColor = new Color[materialLists.Length];
            for (int counter = 0; counter < materialLists.Length; counter++)
            {
                prevColor[counter] = Unity.Robotics.MaterialExtensions.GetMaterialColor(materialLists[counter]);
            }
        }

        /// <summary>
        /// Resets original color of the part being highlighted
        /// </summary>
        /// <param name="index">Index of the part in the Articulation chain</param>
        private void ResetJointColors(int index)
        {
            Renderer[] previousRendererList = articulationChain[index].transform.GetChild(0).GetComponentsInChildren<Renderer>();
            for (int counter = 0; counter < previousRendererList.Length; counter++)
            {
                Unity.Robotics.MaterialExtensions.SetMaterialColor(previousRendererList[counter].material, prevColor[counter]);
            }
        }

//         public void UpdateControlType(JointControl joint)
//         {
//             joint.controltype = control;
//             if (control == ControlType.PositionControl)
//             {
//                 ArticulationDrive drive = joint.joint.xDrive;
//                 drive.stiffness = stiffness;
//                 drive.damping = damping;
//                 joint.joint.xDrive = drive;
//             }
//         }

// This section modified to be able to take inputs from either keyboard or Oculus - JLM 04/2022
        public Vector2 GatherControls(){
            bool SelectionInput1 = false;
            bool SelectionInput2 = false;

            if (questConnected){
                // controls = GatherControlsVR();
                // Debug.Log("Oculus connected. Listening to hand controller input.");
                // DebugReport.SetText("Debug Info: Oculus connected. Listening to hand controller input.");
                SelectionInput1 = OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight);
                Debug.Log("Right joystick True/False? "+ SelectionInput1);
                // DebugReport1.SetText("Debug Info: Joystick=Right, True/False? "+ SelectionInput1);
                SelectionInput2 = OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft);
                Debug.Log("Left joystick True/False? "+ SelectionInput2);
                // DebugReport1.SetText("Debug Info: Joystick=Left, True/False? "+ SelectionInput2);
            }
            else{
                // controls = GatherControlsKeyboard();
                // Debug.Log("Oculus not connected. Listening to keyboard input.");
                // DebugReport.SetText("Debug Info: Oculus not connected. Listening to keyboard input.");
                SelectionInput1 = Input.GetKeyDown("right");
                SelectionInput2 = Input.GetKeyDown("left");
            } 
            
            Vector2 controls = new Vector2(0,0);
            
            if (SelectionInput1){
                controls[0] = 1;
            }
            else{
                controls[0] = 0;
            }
            if (SelectionInput2){
                controls[1] = 1;
            }
            else{
                controls[1] = 0;
            }
            DebugReport2.SetText("Debug Info: "+ controls[1].ToString() + ", " + controls[0].ToString());
            return controls;
        }


//         public void OnGUI()
//         {
//             GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
//             centeredStyle.alignment = TextAnchor.UpperCenter;
//             GUI.Label(new Rect(Screen.width / 2 - 200, 10, 400, 20), "Press left/right arrow keys to select a robot joint.", centeredStyle);
//             GUI.Label(new Rect(Screen.width / 2 - 200, 30, 400, 20), "Press up/down arrow keys to move " + selectedJoint + ".", centeredStyle);
//         }
    }
// }
