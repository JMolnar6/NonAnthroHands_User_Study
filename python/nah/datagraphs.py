"""Figure Generation Utilities"""

import numpy as np
from nah.utils import segment_by_demo
from nah.loader import load_npzs
from nah.trajectory import get_evo_metrics


def generate_self_similarity_heat_map(robot_name, followup, demo_max):

    if followup:
        PIDmax = 9
        gesturemax = 6
    else:
        PIDmax = 16
        gesturemax = 15

    heatmap_array = np.array([])
    handedness_array = np.zeros([PIDmax, gesturemax])

    shift_limit = 0.3

    for PID in range(1, PIDmax + 1):
        gesture_metrics = np.array([])

        for gesture_num in range(1, gesturemax + 1):
            end_eff, camera, rh, lh, joint = load_npzs(robot_name, PID,
                                                       followup, gesture_num)
            try:
                end_eff_multi_demo, camera_multi_demo, rh_multi_demo, lh_multi_demo, joints_multi_demo = segment_by_demo(
                    end_eff, camera, rh, lh, joint, demo_max)
            except:
                print("PID " + str(PID) + " is missing demos for gesture " +
                      str(gesture_num) + ".")

            demo_metrics_separate = np.array([])

            # Check for which hand the participant was using:
            #TODO(Jennifer): Figure out whether the LH or RH had a bigger range and assume that was
            # the primary hand that they used. If both hands were used, can we calculate and present
            # both? Not the way the heat map is currently drawn, but at least print out a statement
            # so that I can resolve it with my notes.
            rh_range = hand_range(rh_multi_demo[0])
            lh_range = hand_range(lh_multi_demo[0])
            camera_range = hand_range(camera_multi_demo[0])

            print("Right hand range:" + str(np.linalg.norm(rh_range)))
            print("Left hand range:" + str(np.linalg.norm(lh_range)))
            print("Camera range:" + str(np.linalg.norm(camera_range)))

            if (rh_range > lh_range):
                hand_data = rh_multi_demo
            else:
                hand_data = lh_multi_demo
                handedness_array[PID - 1, gesture_num - 1] = 1
            """TODO(Jennifer): Handle it when both rh_range and lh_range are above the threshold that 
                indicates this is a 2-handed motion"""

            for i in range(0, 4):
                for j in range(i + 1, 5):
                    try:
                        # Centering code, for participants who clearly walked around during or between their demos:
                        camera_range_i = hand_range(camera_multi_demo[i])
                        camera_range_j = hand_range(camera_multi_demo[j])
                        camera_shift = np.linalg.norm(
                            camera_multi_demo[i][1:4] -
                            camera_multi_demo[j][1:4])
                        if (camera_range_i > shift_limit
                                or camera_range_j > shift_limit
                                or camera_shift > shift_limit):
                            print("Centering data for participant " +
                                  str(PID) + " gesture " + str(gesture_num))
                            temp_metrics = get_evo_metrics(
                                hand_data[i] - camera_multi_demo[i],
                                hand_data[j] - camera_multi_demo[j])
                        else:
                            # print("Using non-centered data for participant "+str(PID)+" gesture "+str(gesture_num))
                            temp_metrics = get_evo_metrics(
                                hand_data[i], hand_data[j])
                    except:
                        print("Demo " + str(j) + " is missing for PID " +
                              str(PID) + ", gesture " + str(gesture_num) + ".")
                        # How do we want to compensate for missing data?
                        # temp_metrics['rmse'] = 10
                        # raise

                    if (i == 1 and j == 2):
                        demo_metrics_separate = temp_metrics['rmse']
                    else:
                        demo_metrics_separate = np.hstack(
                            (demo_metrics_separate, temp_metrics['rmse']))

            total_demo_rmse = np.mean(demo_metrics_separate)
            # print("PID " + str(PID)+" gesture "+str(gesture_num)+ " demo_rmse: " + str(total_demo_rmse))

            if (gesture_num == 1):
                gesture_metrics = total_demo_rmse
            else:
                gesture_metrics = np.hstack((gesture_metrics, total_demo_rmse))

        if PID == 1:
            heatmap_array = gesture_metrics
        else:
            heatmap_array = np.vstack((heatmap_array, gesture_metrics))

    # print(heatmap_array)
    return heatmap_array, handedness_array


def hand_range(hand_data):
    movement_range = np.zeros([3, 1])
    for i in range(1, 4):
        movement_range[i -
                       1] = np.max(hand_data[:, i]) - np.min(hand_data[:, i])

    return np.linalg.norm(movement_range)


def right_handedness(rh_data, lh_data):
    if (hand_range(rh_data) > hand_range(lh_data)):
        return True
    else:
        return False
    #TODO(Jennifer) Make unit tests for this
    #TODO(Jennifer) Mkae something that tests for 2-handedness


def study_range_vals(followup):
    if followup:
        PIDmax = 10
        gesturemax = 7
    else:
        PIDmax = 17
        gesturemax = 16
    return PIDmax, gesturemax
