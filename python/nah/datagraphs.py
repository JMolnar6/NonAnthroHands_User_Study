"""Figure Generation Utilities"""

import numpy as np
from nah.loader import load_npzs
from nah.trajectory import Alignment, get_evo_metrics
from nah.utils import segment_by_demo, study_range_vals


def generate_self_similarity_heat_map(robot_name, followup, demo_max):

    PIDmax, gesturemax = study_range_vals(followup)

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
            is_right_hand = right_handedness(rh_multi_demo[0],
                                             lh_multi_demo[0])

            camera_range = hand_range(camera_multi_demo[0])
            # print("Camera range:" + str(np.linalg.norm(camera_range)))

            if (is_right_hand):
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
    # print("Right hand range:" + str(hand_range(rh_data)))
    # print("Left hand range:" + str(hand_range(lh_data)))
    if (hand_range(rh_data) > hand_range(lh_data)):
        return True
    else:
        return False
    #TODO(Jennifer) Make unit tests for this
    #TODO(Jennifer) Mkae something that tests for 2-handedness


def generate_pairwise_comparison(participant_1,
                                 participant_2,
                                 robot_name,
                                 gesture,
                                 followup,
                                 demo_max,
                                 alignment=Alignment.SpatioTemporal,
                                 isfollowup2=False,
                                 followup_2=False):
    """Aligns two participant hand motions and produces a numerical error metric between them.
       Tests for handedness and compares both participants' dominant hand motions for the gesture.
    """

    followup1 = followup
    if isfollowup2:
        followup2 = followup_2
    else:
        followup2= followup
    #Check to make sure demos for this gesture exist for both participants:

    try:
        end_eff_1, camera_1, rh_1, lh_1, joint_1 = load_npzs(
            robot_name, participant_1, followup1, gesture)
    except:
        print("No demos available for participant" + str(participant_1) +
              " for this gesture")
        return np.nan

    try:
        end_eff_2, camera_2, rh_2, lh_2, joint_2 = load_npzs(
            robot_name, participant_2, followup2, gesture)
    except:
        print("No demos available for participant" + str(participant_2) +
            " for this gesture")
        return np.nan

    for demo_max_temp in range(demo_max,0,-1):
        try:
            end_eff_multi_demo1, camera_multi_demo1, rh_multi_demo1, lh_multi_demo1, joints_multi_demo1 = segment_by_demo(
                end_eff_1, camera_1, rh_1, lh_1, joint_1, demo_max_temp)
            break
        except:
            print("Demo_max not equal to "+str(demo_max) +"for PID "+str(participant_1) +\
                  ", gesture "+str(gesture)+", followup = "+str(followup1))
            continue

    for demo_max_temp in range(demo_max,0,-1):
        try:
            end_eff_multi_demo2, camera_multi_demo2, rh_multi_demo2, lh_multi_demo2, joints_multi_demo2 = segment_by_demo(
                end_eff_2, camera_2, rh_2, lh_2, joint_2, demo_max_temp)
            break
        except:
            print("Demo_max not equal to "+str(demo_max) +"for PID "+str(participant_2) +\
                  ", gesture "+str(gesture)+", followup = "+str(followup2))
            continue

    
    is_right_hand1 = right_handedness(rh_multi_demo1[0], lh_multi_demo1[0])
    is_right_hand2 = right_handedness(rh_multi_demo2[0], lh_multi_demo2[0])

    #Insert manual handedness override for Reachy PID9, gesture 5, and PID2, gesture 2
    if (participant_1==9 and gesture==5 and robot_name=="Reachy" and not followup1):
        is_right_hand1=True
    elif (participant_1==2 and gesture==2 and robot_name=="Reachy" and not followup1):
        is_right_hand1=True
    if (participant_2==9 and gesture==5 and robot_name=="Reachy" and not followup2):
        is_right_hand2=True
    elif (participant_2==2 and gesture==2 and robot_name=="Reachy" and not followup2):
        is_right_hand2=True

    temp_metrics = np.zeros([demo_max, demo_max])

    for demo_num1 in range(0, demo_max):
        for demo_num2 in range(0, demo_max):
            # print("Comparing demos "+str(demo_num1+1)+" and "+str(demo_num2+1))
            # Check to make sure all demos exist
            try:
                if is_right_hand1:
                    traj1 = rh_multi_demo1[demo_num1]
                else:
                    traj1 = lh_multi_demo1[demo_num1]

                if is_right_hand2:
                    traj2 = rh_multi_demo2[demo_num2]
                else:
                    traj2 = lh_multi_demo2[demo_num2]
            except:
                
                temp_metrics[demo_num1][demo_num2] = np.nan
                continue

            # Sometimes something goes wrong and get_evo_metrics fails,
            # but if you try it again it succeeds. We don't want to lose
            # all our work if this is the case.
            max_tries = 10
            for tries in range(max_tries):
                try:
                    metrics = get_evo_metrics(traj1,
                                              traj2,
                                              alignment=alignment)
                    # print("get_evo_metrics_succeeded on try "+str(tries+1))
                    break
                except:
                    print("Gesture " + str(gesture) +
                          ": Failed to get metrics for participant " +
                          str(participant_1) + " demo " + str(demo_num1 + 1) +
                          ", " + str(participant_2) + " demo " +
                          str(demo_num2 + 1) + ".\n Retrying...")
                    if tries < max_tries - 1:
                        continue
                    else:
                        print("Unable to compare participants " +
                              str(participant_1) + " demo " +
                              str(demo_num1 + 1) + " and " +
                              str(participant_2) + " demo " +
                              str(demo_num2 + 1))
                        raise

            temp_metrics[demo_num1, demo_num2] = metrics['rmse']
            # print(temp_metrics)

    return np.nanmean(temp_metrics), is_right_hand1


def generate_all_cross_correlation_matrix(robot_name,
                                          gesture,
                                          followup,
                                          demo_max,
                                          alignment=Alignment.SpatioTemporal,
                                          isfollowup2=False,
                                          followup2=False):
    
    PID_max1, gesture_max1 = study_range_vals(followup)
    if isfollowup2:
        PID_max2, gesture_max2 = study_range_vals(followup2)
    else:
        PID_max2 = PID_max1

    correlation_array = np.zeros([PID_max1, PID_max2])
    handedness_array = np.zeros([PID_max1, PID_max2])

    for PID1 in range(1, PID_max1 + 1):
        for PID2 in range(1, PID_max2 + 1):
            print("Getting metrics for Participants " + str(PID1) + " and " +
                  str(PID2) + ": ")
            temp_metrics, is_right_hand1 = generate_pairwise_comparison(
                PID1,
                PID2,
                robot_name,
                gesture,
                followup,
                demo_max,
                alignment=alignment,
                isfollowup2=isfollowup2,
                followup_2=followup2)

            if is_right_hand1:
                handedness_array[PID1 - 1, PID2 - 1] = 1

            correlation_array[PID1 - 1, PID2 - 1] = temp_metrics
            print(str(temp_metrics))

    return correlation_array, handedness_array


def generate_hand_endeff_similarity_matrix(robot_name, followup, demo_max, alignment=Alignment.SpatioTemporal):

    PIDmax, gesturemax = study_range_vals(followup)

    heatmap_array = np.zeros([PIDmax, gesturemax])
    handedness_array = np.zeros([PIDmax, gesturemax])

    for PID in range(1, PIDmax + 1):
        gesture_metrics = np.array([])

        for gesture_num in range(1, gesturemax + 1):
            try: 
                end_eff, camera, rh, lh, joint = load_npzs(robot_name, PID,
                                                       followup, gesture_num)
            except:
                print("PID " + str(PID) + " is missing demos for gesture " +
                      str(gesture_num) + ".")
                heatmap_array[PID-1,gesture_num-1] = np.nan
                break          
                
            for demo_max_temp in range(demo_max,0,-1):
                try:
                    end_eff_multi_demo, camera_multi_demo, rh_multi_demo, lh_multi_demo, joints_multi_demo = segment_by_demo(
                        end_eff, camera, rh, lh, joint, demo_max)
                    break
                except:
                    print("Demo_max not equal to "+str(demo_max) +"for PID "+str(PID) +", gesture "+str(gesture_num))
                    continue

            temp_metrics = np.zeros(demo_max)

            # Check for which hand the participant was using:
            #TODO(Jennifer): Figure out whether the LH or RH had a bigger range and assume that was
            # the primary hand that they used. If both hands were used, can we calculate and present
            # both? Not the way the heat map is currently drawn, but at least print out a statement
            # so that I can resolve it with my notes.
            is_right_hand = right_handedness(rh_multi_demo[0],
                                             lh_multi_demo[0])

            #Insert manual handedness override for Reachy PID9, gesture 5, and PID2, gesture 2
            if (PID==9 and gesture_num==5 and robot_name=="Reachy"):
                is_right_hand=True
            elif (PID==2 and gesture_num==2 and robot_name=="Reachy"):
                is_right_hand=True
            
            # print("Camera range:" + str(np.linalg.norm(camera_range)))

            if (is_right_hand):
                hand_data = rh_multi_demo
            else:
                hand_data = lh_multi_demo
                handedness_array[PID - 1, gesture_num - 1] = 1
            """TODO(Jennifer): Handle it when both rh_range and lh_range are above the threshold that 
                indicates this is a 2-handed motion"""

            # Compare each demo to the robot end-effector's motion
            for i in range(0, demo_max):
                # Sometimes something goes wrong and get_evo_metrics fails,
                # but if you try it again it succeeds. We don't want to lose
                # all our work if this is the case.
                max_tries = 10
                for tries in range(max_tries):
                    try:
                        metrics = get_evo_metrics(hand_data[i],
                                                end_eff_multi_demo[i],
                                                alignment=alignment)
                        # print("get_evo_metrics_succeeded on try "+str(tries+1))
                        break
                    except:
                        print("Gesture " + str(gesture_num) +
                            ": Failed to get metrics for participant " +
                            str(PID) + " demo " + str(i + 1) +
                            ".\n Retrying...")
                        if tries < max_tries - 1:
                            continue
                        else:
                            print("Unable to compare hand and end-effector for participant " +
                                str(PID) + ", gesture "+str(gesture_num)+", demo "+str(i+1))
                            metrics['rmse']=np.nan
                            raise

                temp_metrics[i] = metrics['rmse']
            
            heatmap_array[PID-1,gesture_num-1] = np.nanmean(temp_metrics)

    return heatmap_array, handedness_array


