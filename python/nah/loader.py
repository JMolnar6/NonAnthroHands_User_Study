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
    """Get the experiment data filename based on arguments."""

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


def get_npz_filename(robot_name, participant_id, gesture_num, followup=False):
    """Get the filename for the processed npz file containing the user and robot trajectories."""
    # Path to project root directory
    project_root = Path(__file__).parent.parent.parent.resolve()
    data_path = project_root / "npz_files"

    follow_up = "B" if followup else ""
    filename = f"data_PID{participant_id}{follow_up}_{robot_name}_gesture_{gesture_num}.npz"
    return data_path / filename


def load_alternate_data(robot_name, gesture_num, demo_num, followup=True):
    """
    If JointMotion data is missing, the good news is that
    it should be the same for all participants.

    Try to find a participant/demo that exists, and copy that over instead.
    
    NOTE that time increments may be different for JointMotions
        vs all other data timestamps because of this.
    The start time should be identical, though, and the overall time as well

    (I prefer to have the code do this for me,
    rather than keeping track of which PIDs I had to adjust manually)
    """
    if followup:
        PID_temp = 9
    else:
        PID_temp = 3

    filename = get_filename(participant_id=PID_temp,
                            robot_name=robot_name,
                            end_eff_name="Joint",
                            follow_up=followup,
                            gesture_num=gesture_num,
                            demo_num=demo_num)
    try:
        jointangles = pd.read_csv(filename)
        joint_data = jointangles.to_numpy()
    except Exception:
        print(
            "ERROR: JointMotion file not found. Backup JointMotion file not found."
        )

    print(f"Using PID {PID_temp} instead")
    return joint_data


def dtw_data_import(robot_name, end_eff_name, PID, followup, gesture_num,
                    demo_num):
    """
    Import data for running DTW.

    :param: controller: Tells us which end effector was used
        ("", "RightHand Controller_", "LeftHand Controller_",
         "Main Camera_", "Joint").
    """

    # Y-position of hand and end-effector tend to be most consistently aligned.
    # Use those to discover the time warp mapping

    filename = get_filename(PID, robot_name, end_eff_name, gesture_num,
                            demo_num, followup)

    try:
        # Import data from csvs
        controller_raw = pd.read_csv(filename)
        controller_data = controller_raw.to_numpy()
        return controller_data

    except RuntimeError:
        print("{filename} NOT FOUND")

        return load_alternate_data(robot_name,
                                   gesture_num,
                                   demo_num,
                                   followup=True)


def load_npzs(robot_name, participant_id, followup, gesture_num):
    """Load .npz file corresponding to provided arguments."""
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
