"""Apply alignments"""

# Plotting Packages
from pathlib import Path

import matplotlib as mpl
# import matplotlib.pyplot as plt
# import numpy as np
import pandas as pd

# from scipy.signal import find_peaks

# from fastdtw import fastdtw
# from mpl_toolkits import mplot3d
# from mpl_toolkits.mplot3d import Axes3D

# Computation packages
# from scipy.spatial.distance import euclidean

mpl.rcParams['figure.dpi'] = 150
savefig_options = dict(format="png", dpi=150, bbox_inches="tight")


def get_filename(participant_id,
                 robot_name,
                 end_eff_name,
                 gesture_num,
                 demo_num,
                 follow_up=False):
    """Get the experiment data filename based on arguments."""

    data_path = Path("data")
    if follow_up:
        data_path /= "Follow-up Study"
        follow_up_experiment = 'B'
    else:
        follow_up_experiment = ''

    participant_directory = f"PID{participant_id}{follow_up_experiment}"
    experiment_file = f"{robot_name}_PID{participant_id}{follow_up_experiment}_{end_eff_name}"\
        f"_Motion_gesture_{gesture_num}_{demo_num}.csv"

    filename = data_path / participant_directory / experiment_file

    return filename


# def load_alternate_data(robot_name, followup=True):
#     if followup:
#         PID_temp=9
#         filename = get_filename(participant_id=PID_temp, contoller=JointMotion_gesture follow_up=followup)
#         try:
#             filename = "C:\\Users\\jmoln\\Dropbox (GaTech)\\Non-Anthropomorphic Hands User Study Data\\Follow-up Study\\PID"+str(PID_temp)+"B\\"+str(robot_name)+"_PID"+str(PID_temp)+"B_JointMotion_gesture_"+str(gesture_num)+"_"+str(demo_num)+".csv"
#             jointangles = pd.read_csv(filename)
#             joint_data = jointangles.to_numpy()
#         except:
#             print("ERROR: JointMotion file not found. Backup JointMotion file not found.")
#     else:
#         PID_temp=3
#         try:
#             filename = "C:\\Users\\jmoln\\Dropbox (GaTech)\\Non-Anthropomorphic Hands User Study Data\\PID"+str(PID_temp)+"\\"+str(robot_name)+"_PID"+str(PID_temp)+"_JointMotion_gesture_"+str(gesture_num)+"_"+str(demo_num)+".csv"
#             jointangles = pd.read_csv(filename)
#             joint_data = jointangles.to_numpy()
#         except:
#             print("ERROR: JointMotion file not found. Backup JointMotion file not found.")

#     print("Using PID "+str(PID_temp)+" instead")
#     return


def dtw_data_import(robot_name, end_eff_name, PID, followup, gesture_num,
                    demo_num):
    """
    Import data for running DTW.

    :param: controller: Tells us which end effector was used
        ("", "RightHand Controller", "LeftHand Controller",
         "Main Camera", "JointMotion").
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

        # If JointMotion data is missing, the good news is that
        # it should be the same for all participants.

        #  Try to find a participant/demo that exists, and copy that over instead.
        # NOTE that time increments may be different for JointMotions
        # vs all other data timestamps because of this.
        # The start time should be identical,
        # though, and the overall time as well
        # (I prefer to have the code do this for me,
        # rather than keeping track of which PIDs I had to adjust manually)

        #TODO(Varun)
        # load_alternate
