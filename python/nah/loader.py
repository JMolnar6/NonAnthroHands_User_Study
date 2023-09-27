"""Code for loading data"""

from pathlib import Path

import numpy as np
import pandas as pd


def get_filename(participant_id,
                 robot_name,
                 end_eff_name,
                 gesture_num,
                 demo_num,
                 follow_up=False):
    """Get the experiment data CSV filename based on arguments."""

    # Path to project root directory
    project_root = Path(__file__).parent.parent.parent.resolve()
    data_path = project_root / "data"

    if follow_up:
        data_path /= "Follow-up Study"
        is_followup = 'B'
    else:
        is_followup = ''

    participant_directory = f"PID{participant_id}{is_followup}"
    experiment_file = f"{robot_name}_PID{participant_id}{is_followup}_{end_eff_name}"\
        f"Motion_gesture_{gesture_num}_{demo_num}.csv"

    filename = data_path / participant_directory / experiment_file

    return filename


def load_alternate_data(robot_name, gesture_num, demo_num, followup=True):
    """
    If JointMotion data is missing, the good news is that
    it should be the same for all participants.

    Try to find a participant/demo that exists, and copy that over instead.
    
    NOTE that time increments may be different for JointMotions
        vs all other data timestamps because of this.
    The start time should be identical, though, and the overall time as well

    (I prefer to have the code do this for me,
    rather than keeping track of which participant IDs I had to adjust manually)
    """
    if followup:
        participant_id = 9
    else:
        participant_id = 3

    filename = get_filename(participant_id=participant_id,
                            robot_name=robot_name,
                            end_eff_name="Joint",
                            follow_up=followup,
                            gesture_num=gesture_num,
                            demo_num=demo_num)
    try:
        jointangles = pd.read_csv(filename)
        joint_data = jointangles.to_numpy()

        print(f"Using participant ID {participant_id} instead")
        return joint_data

    except FileNotFoundError as exc:
        error_msg = f"JointMotion file not found AND backup JointMotion file not found: {filename}"
        raise FileNotFoundError(error_msg) from exc


def load_raw_csv_data(robot_name, end_eff_name, participant_id, followup,
                      gesture_num, demo_num):
    """
    Import the raw data from the CSV file.

    :param: controller: Tells us which end effector was used
        ("", "RightHand Controller_", "LeftHand Controller_",
         "Main Camera_", "Joint").
    """

    # Y-position of hand and end-effector tend to be most consistently aligned.
    # Use those to discover the time warp mapping

    filename = get_filename(participant_id, robot_name, end_eff_name,
                            gesture_num, demo_num, followup)

    try:
        # Load data from CSV
        controller_raw = pd.read_csv(filename)
        controller_data = controller_raw.to_numpy()
        return controller_data

    except FileNotFoundError:
        print(f"{filename} not found, loading alternate data from followup")

        return load_alternate_data(robot_name,
                                   gesture_num,
                                   demo_num,
                                   followup=followup)


def get_npz_filename(robot_name, participant_id, gesture_num, followup=False):
    """Get the filename for the processed npz file containing the user and robot trajectories."""
    # Path to project root directory
    project_root = Path(__file__).parent.parent.parent.resolve()
    data_path = project_root / "npz_files"

    follow_up = "B" if followup else ""
    filename = f"data_PID{participant_id}{follow_up}_{robot_name}_gesture_{gesture_num}.npz"
    return data_path / filename


def load_npzs(robot_name, participant_id, followup, gesture_num):
    """
    Load .npz file corresponding to provided arguments.

    The trajectories returned are concatenated demos.
    Please use `segment_by_demo` to separate them.
    """
    filename = get_npz_filename(robot_name, participant_id, gesture_num,
                                followup)
    # Import data from npz file
    data = np.load(filename)
    end_eff_data = data['end_eff_data']
    camera_data = data['camera_data']
    rh_data = data['rh_data']
    lh_data = data['lh_data']
    joint_data = data['joint_data']

    return end_eff_data, camera_data, rh_data, lh_data, joint_data


def process_data(robot_name,
                 end_eff_name,
                 participant_id,
                 gesture_num,
                 demo_num,
                 start_index=1,
                 end_index=-1,
                 followup=False):
    # Load the data for each type of end effector
    end_eff_data_raw = load_raw_csv_data(robot_name, end_eff_name,
                                         participant_id, followup, gesture_num,
                                         demo_num)
    camera_data_raw = load_raw_csv_data(robot_name, "Main Camera_",
                                        participant_id, followup, gesture_num,
                                        demo_num)
    rh_data_raw = load_raw_csv_data(robot_name, "RightHand Controller_",
                                    participant_id, followup, gesture_num,
                                    demo_num)
    lh_data_raw = load_raw_csv_data(robot_name, "LeftHand Controller_",
                                    participant_id, followup, gesture_num,
                                    demo_num)
    joint_data_raw = load_raw_csv_data(robot_name, "Joint", participant_id,
                                       followup, gesture_num, demo_num)

    # Make time start from 0.0
    end_eff_data_raw[:, 0] = end_eff_data_raw[:, 0] - end_eff_data_raw[0, 0]
    camera_data_raw[:, 0] = camera_data_raw[:, 0] - camera_data_raw[0, 0]
    rh_data_raw[:, 0] = rh_data_raw[:, 0] - rh_data_raw[0, 0]
    lh_data_raw[:, 0] = lh_data_raw[:, 0] - lh_data_raw[0, 0]
    joint_data_raw[:, 0] = joint_data_raw[:, 0] - joint_data_raw[0, 0]

    # Crop out the trajectory data
    end_eff_data_raw = end_eff_data_raw[start_index:end_index, :]
    camera_data_raw = camera_data_raw[start_index:end_index, :]
    rh_data_raw = rh_data_raw[start_index:end_index, :]
    lh_data_raw = lh_data_raw[start_index:end_index, :]
    joint_data_raw = joint_data_raw[start_index:end_index, :]

    # More participant-specific exceptions:
    if (participant_id == 3 and not followup and gesture_num >= 3):
        holding_variable = rh_data_temp
        rh_data_temp = lh_data_temp
        lh_data_temp = holding_variable

    #TODO Normalize by participant wingspan

