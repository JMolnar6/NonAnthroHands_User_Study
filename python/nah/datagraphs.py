"""Figure Generation Utilities"""

import numpy as np
import pandas as pd
from nah.utils import segment_by_demo
from nah.loader import load_npzs
from nah.trajectory import get_evo_metrics


def generate_self_similarity_heat_map(robot_name, followup, demo_max):
    if followup:
        PIDmax = 10
        gesturemax = 7
    else:
        PIDmax = 17
        gesturemax = 16

    heatmap_array = np.array([])

    for PID in range(1, PIDmax):
        gesture_metrics = np.array([])

        for gesture_num in range(1, gesturemax):
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
            rh_range = np.max(rh[:, 1:4], axis=0)[1:4] - np.min(rh[:, 1:4],
                                                                axis=0)[1:4]
            lh_range = np.max(lh[:, 1:4], axis=0)[1:4] - np.min(lh[:, 1:4],
                                                                axis=0)[1:4]
            camera_range = np.max(camera[:, 1:4], axis=0)[1:4] - np.min(
                camera[:, 1:4], axis=0)[1:4]

            # print("Right hand range:"+ str(np.linalg.norm(rh_range)))
            # print("Left hand range:" + str(np.linalg.norm(lh_range)))
            # print("Camera range:" + str(np.linalg.norm(camera_range)))
            """ TODO(Jennifer): Is it possible to print the heatmap with different colors for different 
                entries, depending on which hand was used? I'd want the luminance to be similar, so that
                a B/W print still makes it easy to see how well things correlate
            """
            if (np.linalg.norm(rh_range)) > (np.linalg.norm(lh_range)):
                hand_data = rh_multi_demo
            else:
                hand_data = lh_multi_demo
            """TODO(Jennifer): Handle it when both rh_range and lh_range are above the threshold that 
                indicates this is a 2-handed motion"""
            """TODO(Jennifer)
            Check these results: I guess it makes sense that you get a black line for gesture 6, b/c
            it's almost purely rotation and very little movement. Does APE take rotation into account,
            or is this low error simply because the gesture is so short? Is there a way to normalize
            across gestures? (Should we bother?)
            Ans: No need to bother--we'll showcase participant self-similarity anyway, through the 
            cross-correlation matrices.

            Follow-up question: should we align participant gestures in any way? I didn't think we 
            needed to, but Participant 4 wandered all over the room, so at least for them, it might
            be worth it
            """
            for i in range(0, 4):
                for j in range(i + 1, 5):
                    try:
                        # Centering code, for participants who clearly walked around during or between their demos:
                        camera_shift = np.linalg.norm(
                            camera_multi_demo[i][1:4] -
                            camera_multi_demo[j][1:4])
                        if (np.linalg.norm(camera_range) > 0.25
                                or camera_shift > 0.25):
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

    print(heatmap_array)
    return heatmap_array
