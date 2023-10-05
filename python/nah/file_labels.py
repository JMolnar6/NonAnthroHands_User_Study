"""Manual adjustments for participant data labels"""

from pathlib import Path
import os


def rename_demos(participant_id, robot, gesture, demo_delete, demo_new):
    end_eff_options = [
        "", "RightHand Controller_", "LeftHand Controller_", "Main Camera_",
        "Joint"
    ]
    filename = Path(
        "C:\\Users\\jmoln\\Dropbox (GaTech)\\Non-Anthropomorphic Hands User Study Data"
    )
    filename /= "PID" + str(participant_id)
    if (robot == "Reachy"):
        end_eff_options[0] = "r_wrist2hand_"
    else:
        if (robot == "j2s6s300"):
            end_eff_options[0] = "j2s6s300_end_effector_"
    for end_eff_name in end_eff_options:
        filename1 = filename
        filename2 = filename
        filename1 /= robot + "_PID" + str(
            participant_id) + "_" + end_eff_name + "Motion_gesture_" + str(
                gesture) + "_" + str(demo_delete) + ".csv"
        filename2 /= robot + "_PID" + str(
            participant_id) + "_" + end_eff_name + "Motion_gesture_" + str(
                gesture) + "_" + str(demo_new) + ".csv"

        try:
            os.rename(filename1, filename2)
        except:
            continue


def right_left_swap(participant_id, robot, gesture):
    end_eff_options = ["RightHand Controller_", "LeftHand Controller_"]
    filename = Path(
        "C:\\Users\\jmoln\\Dropbox (GaTech)\\Non-Anthropomorphic Hands User Study Data"
    )
    filename /= "PID" + str(participant_id)

    for demo_num in range(1, 6):
        filename1 = filename
        filename2 = filename
        filename1 /= robot + "_PID" + str(
            participant_id) + "_RightHand Controller_Motion_gesture_" + str(
                gesture) + "_" + str(demo_num) + ".csv"
        filename2 /= robot + "_PID" + str(
            participant_id) + "_RightHand Controller_Motion_gesture_" + str(
                gesture) + "_" + str(demo_num) + "B.csv"
        try:
            os.rename(filename1, filename2)
        except:
            continue

        filename1 = filename
        filename2 = filename
        filename1 /= robot + "_PID" + str(
            participant_id) + "_LeftHand Controller_Motion_gesture_" + str(
                gesture) + "_" + str(demo_num) + ".csv"
        filename2 /= robot + "_PID" + str(
            participant_id) + "_RightHand Controller_Motion_gesture_" + str(
                gesture) + "_" + str(demo_num) + ".csv"
        try:
            os.rename(filename1, filename2)
        except:
            continue

        filename1 = filename
        filename2 = filename
        filename1 /= robot + "_PID" + str(
            participant_id) + "_RightHand Controller_Motion_gesture_" + str(
                gesture) + "_" + str(demo_num) + "B.csv"
        filename2 /= robot + "_PID" + str(
            participant_id) + "_LeftHand Controller_Motion_gesture_" + str(
                gesture) + "_" + str(demo_num) + ".csv"

        try:
            os.rename(filename1, filename2)
        except:
            continue
