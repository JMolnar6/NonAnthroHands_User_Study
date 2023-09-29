""" Contains useful functions for accessing/processing user data"""
import numpy as np
from fastdtw import fastdtw
from scipy.signal import find_peaks
from sklearn.cluster import AgglomerativeClustering


def norm_data(x, y):
    """
    Take in two time-stamped data streams.
    Scale and align them vertically based on their max/min.
    Trim the ends to make them even, if necessary. 
    Return the DTW alignment between them as a 'warp path.'
    """
    # Separate time from position
    # x = (end_eff_data[...,0],end_eff_data[...,1])
    # y = (hand_data[...,0],hand_data[...,1])

    # Normalize x and y to prevent scaling issues from creating DTW misalignment

    scale_x = 1 / (np.max(x[1]) - np.min(x[1]))
    scale_y = 1 / (np.max(y[1]) - np.min(y[1]))

    # Center should not be the mean; it should center based on the max and min
    # center_x = np.mean(x[1])
    # center_y = np.mean(y[1])
    center_x = np.min(x[1])
    center_y = np.min(y[1])

    x_norm = np.vstack((x[0], (x[1] - center_x) * scale_x))
    y_norm = np.vstack((y[0], (y[1] - center_y) * scale_y))

    # TODO: trim ends for cleaner DTW
    # (Not done yet)

    # If X and Y are different lengths, fastdtw has issues
    lim = min(x.shape[1], y.shape[1])

    return x_norm[1, 0:lim], y_norm[1, 0:lim]


def full_align(warp_path, traj1, traj2):
    """
    Take the warp_path generated from whatever parts 
    of the traj1/traj2 data were needed for DTW alignment,
    and use that to align the two trajectories
    """

    wp_size = len(warp_path)
    traj1_aligned = np.zeros((wp_size, 7))
    traj2_aligned = np.zeros((wp_size, 7))

    for i, [map_x, map_y] in enumerate(warp_path, start=0):
        traj1_aligned[i] = traj1[map_x, :]
        traj2_aligned[i] = traj2[map_y, :]

    return traj1_aligned, traj2_aligned


# def full_joint_align(time_URDF_aligned, joint_data):
#     # Time marks:
#     time_ja = joint_data[..., 0]

#     # remember that x = end_eff_pos
#     #               y = hand_pos

#     # Z-data (forward/back) is offset by the distance between the viewer and the robot. Let's remove that distance for comparison purposes

#     wp_size = len(warp_path)
#     time_ja_aligned = np.zeros(wp_size)
#     joint_data_aligned = np.zeros((wp_size, 6))

#     for i, [map_x, map_y] in enumerate(warp_path, start=0):
#         time_ja_aligned[i] = time_ja[map_x]
#         time_hand_aligned[i] = time_hand[map_y]
#         joint_data_aligned[i][0:5] = end_eff_data[map_x][1:6]
#         hand_data_aligned[i][0:5] = hand_data[map_y][1:6]

#     return time_ja_aligned, joint_data_aligned


def clean_rot_data(hand_rot_aligned):
    """Fix angle inversion issues for hand data"""

    for i, [x_rot, y_rot, z_rot] in enumerate(hand_rot_aligned, start=1):
        # Singularities should occur in all axes simultaneously
        if i == len(hand_rot_aligned):
            continue
        elif np.abs(hand_rot_aligned[i].T[0] - hand_rot_aligned[i - 1].T[0]
                    ) > np.abs(hand_rot_aligned[i].T[0] +
                               hand_rot_aligned[i - 1].T[0]):
            #         print(time_hand_aligned[i], hand_rot_aligned[i-1], hand_rot_aligned[i])
            hand_rot_aligned[i] = -hand_rot_aligned[i]
        elif np.abs(hand_rot_aligned[i].T[1] - hand_rot_aligned[i - 1].T[1]
                    ) > np.abs(hand_rot_aligned[i].T[1] +
                               hand_rot_aligned[i - 1].T[1]):
            #         print(time_hand_aligned[i], hand_rot_aligned[i-1], hand_rot_aligned[i])
            hand_rot_aligned[i] = -hand_rot_aligned[i]
        elif np.abs(hand_rot_aligned[i].T[2] - hand_rot_aligned[i - 1].T[2]
                    ) > np.abs(hand_rot_aligned[i].T[2] +
                               hand_rot_aligned[i - 1].T[2]):
            #         print(time_hand_aligned[i], hand_rot_aligned[i-1], hand_rot_aligned[i])
            hand_rot_aligned[i] = -hand_rot_aligned[i]

    # for i, [x_rot,y_rot,z_rot] in enumerate(hand_rot_aligned, start=2):
    #     # Singularities should occur in all axes simultaneously
    #     if np.abs(hand_rot_aligned[i].T[0] - hand_rot_aligned[i-2].T[0])>np.abs(hand_rot_aligned[i].T[0] + hand_rot_aligned[i-2].T[0]):
    #         print(time_hand_aligned[i], hand_rot_aligned[i-2], hand_rot_aligned[i])
    #         hand_rot_aligned[i] = -hand_rot_aligned[i]
    #     elif np.abs(hand_rot_aligned[i].T[1] - hand_rot_aligned[i-2].T[1])>np.abs(hand_rot_aligned[i].T[1] + hand_rot_aligned[i-2].T[1]):
    #         print(time_hand_aligned[i], hand_rot_aligned[i-2], hand_rot_aligned[i])
    #         hand_rot_aligned[i] = -hand_rot_aligned[i]
    #     elif np.abs(hand_rot_aligned[i].T[2] - hand_rot_aligned[i-2].T[2])>np.abs(hand_rot_aligned[i].T[2] + hand_rot_aligned[i-2].T[2]):
    #         print(time_hand_aligned[i], hand_rot_aligned[i-2], hand_rot_aligned[i])
    #         hand_rot_aligned[i] = -hand_rot_aligned[i]

    return hand_rot_aligned


def segment_by_demo(end_eff_data, camera_data, rh_data, lh_data, joint_data,
                    demo_max):

    peaks, _ = find_peaks(end_eff_data[:, 0], height=0)

    peaks = np.hstack((0, peaks))
    peaks = np.hstack((peaks, -1))
    end_eff = [''] * demo_max
    camera = [''] * demo_max
    rh = [''] * demo_max
    lh = [''] * demo_max
    joints = [''] * demo_max

    for i in range(0, demo_max):
        end_eff[i] = end_eff_data[peaks[i]:peaks[i + 1], :]
        camera[i] = camera_data[peaks[i]:peaks[i + 1], :]
        rh[i] = rh_data[peaks[i]:peaks[i + 1], :]
        lh[i] = lh_data[peaks[i]:peaks[i + 1], :]
        joints[i] = joint_data[peaks[i]:peaks[i + 1], :]

    # end_eff = np.array(end_eff)
    # rh      = np.array(rh)
    # lh      = np.array(lh)
    # joints  = np.array(joints)
    return end_eff, camera, rh, lh, joints


def sum_of_squares(a):
    """
    Returns the L2 norm (distance from the origin). Used for DTW alignment,
    but other distance metrics could be better. 
    """
    return np.vstack((a[:, 0], np.sum(np.multiply(a[:, 1:4], a[:, 1:4]),
                                      axis=1)))


def study_range_vals(followup):
    if followup:
        PIDmax = 9
        gesturemax = 6
    else:
        PIDmax = 16
        gesturemax = 15
    return PIDmax, gesturemax

def translate_followup_participants(num):
    """Rearrange the order of followup participants so that returning
    participants are processed first. Provide an index in order from 0-9
    and receive a followup participant ID in the order [8,5,9,2,7,1,3,4,6],
    where [1,3,4,6] are the new participants"""
    followup_participant_list = [8,5,9,2,7,1,3,4,6]
    participant_matching_list = [[11,2],[6,5],[13,7],[1,8],[10,9]]
    new_participants = [1,3,4,6]
    return followup_participant_list(num)


def translate_followup_gesture(robot_name, num):
    """Provide the new gesture number, see which gesture it matched to from the original set"""
    if robot_name == "Reachy":
        gesture_matching_list=[15,12,11,10,3,2]
    elif robot_name == 'j2s6s300':
        gesture_matching_list=[15,12,11,10,4,1]

    gesture_matching_list_Reachy=[[2,6],[3,5],[10,4],[11,3],[12,2],[15,1]]
    gesture_list_Reachy_original=[15,12,11,10,3,2]
    gesture_matching_list_Jaco=[[1,6],[4,5],[10,4],[11,3],[12,2],[15,1]]
    gesture_list_Jaco_original=[15,12,11,10,4,1]
    
    print("New gesture number: "+str(num)+", original gesture: "+str(gesture_matching_list[num-1]))
    return gesture_matching_list[num-1]

def cluster(robot_name, followup, alignment, threshold=1.7, linkage='single'):

    PID_max, gesture_max = study_range_vals(followup)
    gesture_start = 1
    gesture_end=gesture_max
    
    # if followup:
    #     clustering_vals=np.zeros([gesture_max, 16])
    # else:
    clustering_vals=np.zeros([gesture_max, PID_max])

    for gesture in range(gesture_start,gesture_end+1):
        filename = str(robot_name)+"_gesture_"+str(gesture)+"_cross_correlation_"
        if (followup):
            filename+="w_FollowUpPs_"
        filename+=str(alignment)+".npz"
        # print(filename)
        data = np.load(filename)
        correlation_array = data['correlation_array']
        if followup:
            correlation_array=correlation_array.T
        # print(correlation_array.shape)
        # hand_array = data['hand_array']
        clustering = AgglomerativeClustering(n_clusters=None, distance_threshold=threshold, compute_full_tree=True, linkage=linkage).fit(correlation_array)
        clustering
        clustering_vals[gesture-1]=clustering.labels_

    return clustering_vals


def compare_rows(row_1, row_2):
    unique_row_2_vals = np.unique(row_2)
    # We want to maximize the number of elements that match from one row to the next
    for i in range(0,unique_row_2_vals.shape[0]-1):
        for j in range(0,unique_row_2_vals.shape[0]):
            temp_row = switch_elements(row_2, unique_row_2_vals[i], unique_row_2_vals[j])
            if count_matched_elements(row_1, temp_row)>count_matched_elements(row_1, row_2):
                row_2 = np.copy(temp_row)
    return row_2
    

def count_matched_elements(row_1, row_2):
    matches = 0
    for i in range(0,row_1.shape[0]):
        if row_1[i]==row_2[i]:
            matches += 1
    # print(matches)
    return matches

def switch_elements(row_1, val1, val2):
    # Find all occurances of a value in row 1
    row_swap_indices_1 = np.where(row_1==val1)
    row_swap_indices_2 = np.where(row_1==val2)
    new_row = np.copy(row_1)
    for i in row_swap_indices_1:
        new_row[i] = val2
    for i in row_swap_indices_2:
        new_row[i] = val1
    
    # print("New row:"+str(new_row)+"\nOld row: "+str(row_1))
    return new_row

def organize_cluster_graph(cluster_array):
    for i in range(0,cluster_array.shape[0]-1):
        cluster_array[i+1]=compare_rows(cluster_array[i],cluster_array[i+1])
    return cluster_array

