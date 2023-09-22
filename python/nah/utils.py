""" Contains useful functions for accessing/processing user data"""
from nah.alignments import dtw_data_import, get_filename

def norm_data(x,y):
    """Take in two time-stamped data streams. Scale and 
    align them vertically based on their max/min. Trim
    the ends to make them even, if necessary. 
    Return the DTW alignment between them as a 'warp path.' """
    # Separate time from position
    # x = (end_eff_data[...,0],end_eff_data[...,1])
    # y = (hand_data[...,0],hand_data[...,1])
    
    
    # Normalize x and y to prevent scaling issues from creating DTW misalignment

    scale_x = 1/(np.max(x[1])-np.min(x[1]))
    scale_y = 1/(np.max(y[1])-np.min(y[1]))
    
    # Center should not be the mean; it should center based on the max and min
    # center_x = np.mean(x[1])
    # center_y = np.mean(y[1])
    center_x = np.min(x[1])
    center_y = np.min(y[1])

    x_norm = np.vstack((x[0],(x[1] - center_x)*scale_x))
    y_norm = np.vstack((y[0],(y[1] - center_y)*scale_y))

    # TO-DO: trim ends for cleaner DTW
    # (Not done yet)
    
    # If X and Y are different lengths, fastdtw has issues
    lim = min(x.shape[1],y.shape[1])

    dtw_distance, warp_path = fastdtw(x_norm[1,0:lim], y_norm[1,0:lim]) #, dist=euclidean) 
    plot_norm(warp_path, x_norm, y_norm)
    # dtw_distance, warp_path = fastdtw(x[1,0:lim], y[1,0:lim])
    # plot_norm(warp_path, x,y)
    
    return warp_path


def full_align(warp_path, end_eff_data, hand_data):
    """Take the warp_path generated from normalized hand/URDF data and use that to align all other hand data"""
    # Time marks: 
    time_URDF = end_eff_data[...,0]
    time_hand = hand_data[...,0]

    # remember that x = end_eff_pos
    #               y = hand_pos

    # Z-data (forward/back) is offset by the distance between the viewer and the robot. Let's remove that distance for comparison purposes

    wp_size = len(warp_path)
    time_URDF_aligned = np.zeros(wp_size)
    time_hand_aligned = np.zeros(wp_size)
    end_eff_pos_aligned = np.zeros((wp_size,3))
    end_eff_rot_aligned = np.zeros((wp_size,3))
    hand_pos_aligned = np.zeros((wp_size,3))
    hand_rot_aligned = np.zeros((wp_size,3))

    for i, [map_x, map_y] in enumerate(warp_path, start=0):   
        time_URDF_aligned[i] = time_URDF[map_x]
        time_hand_aligned[i] = time_hand[map_y]
        end_eff_pos_aligned[i][0:3] = end_eff_data[map_x][1:4]
        end_eff_rot_aligned[i][0:3] = end_eff_data[map_x][4:]
        hand_pos_aligned[i][0:3]    = hand_data[map_y][1:4]
        hand_rot_aligned[i][0:3]    = hand_data[map_y][4:]
    
    return time_URDF_aligned, time_hand_aligned, end_eff_pos_aligned, end_eff_rot_aligned, hand_pos_aligned, hand_rot_aligned

def full_joint_align(time_URDF_aligned, joint_data):
    # Time marks: 
    time_ja   = joint_data[...,0]

    # remember that x = end_eff_pos
    #               y = hand_pos

    # Z-data (forward/back) is offset by the distance between the viewer and the robot. Let's remove that distance for comparison purposes

    wp_size = len(warp_path)
    time_ja_aligned   = np.zeros(wp_size)
    joint_data_aligned = np.zeros((wp_size,6))

    for i, [map_x, map_y] in enumerate(warp_path, start=0):   
        time_ja_aligned[i]   = time_ja[map_x]
        time_hand_aligned[i] = time_hand[map_y]
        joint_data_aligned[i][0:5] = end_eff_data[map_x][1:6]
        hand_data_aligned[i][0:5]    = hand_data[map_y][1:6]
    
    return time_ja_aligned, joint_data_aligned

def clean_rot_data(gesture_num, demo_num, hand_rot_aligned):
    """Fix angle inversion issues for hand data"""

    fig, ax = plt.subplots(figsize=(10, 7))

    for i, [x_rot,y_rot,z_rot] in enumerate(hand_rot_aligned, start=1):
        # Singularities should occur in all axes simultaneously
        if i==len(hand_rot_aligned):
            continue
        elif np.abs(hand_rot_aligned[i].T[0] - hand_rot_aligned[i-1].T[0])>np.abs(hand_rot_aligned[i].T[0] + hand_rot_aligned[i-1].T[0]):
    #         print(time_hand_aligned[i], hand_rot_aligned[i-1], hand_rot_aligned[i])
            hand_rot_aligned[i] = -hand_rot_aligned[i]
        elif np.abs(hand_rot_aligned[i].T[1] - hand_rot_aligned[i-1].T[1])>np.abs(hand_rot_aligned[i].T[1] + hand_rot_aligned[i-1].T[1]):
    #         print(time_hand_aligned[i], hand_rot_aligned[i-1], hand_rot_aligned[i])
            hand_rot_aligned[i] = -hand_rot_aligned[i]
        elif np.abs(hand_rot_aligned[i].T[2] - hand_rot_aligned[i-1].T[2])>np.abs(hand_rot_aligned[i].T[2] + hand_rot_aligned[i-1].T[2]):
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

    ax.plot(time_hand_aligned, hand_rot_aligned[:].T[0], '-ko', label='x', linewidth=0.2, markersize=2, markerfacecolor='lightcoral', markeredgecolor='lightcoral')
    ax.plot(time_hand_aligned, hand_rot_aligned[:].T[1], '-bo', label='x', linewidth=0.2, markersize=2, markerfacecolor='skyblue', markeredgecolor='skyblue')
    ax.plot(time_hand_aligned, hand_rot_aligned[:].T[2], '-ro', label='x', linewidth=0.2, markersize=2, markerfacecolor='red', markeredgecolor='red')

    plt.savefig('DTW_Rot_corrected_'+str(demo_num)+'.png')
    plt.close('all')
    
    return hand_rot_aligned



def load_npzs(robot_name, PID, followup, gesture_num):
    try:
        if followup:
            filename = "C:\\Users\\jmoln\\Dropbox (GaTech)\\Non-Anthropomorphic Hands User Study Data\\npz files\\data_PID"+str(PID)+"B_"+str(robot_name)+"_gesture_"+str(gesture_num)+".npz"
        else:
            filename = "C:\\Users\\jmoln\\Dropbox (GaTech)\\Non-Anthropomorphic Hands User Study Data\\npz files\\data_PID"+str(PID)+"_"+str(robot_name)+"_gesture_"+str(gesture_num)+".npz"
    except:
        print(filename+" NOT FOUND")
    # Import data from csvs
    data = np.load(filename)
    end_eff_data = data['end_eff_data']
    camera_data  = data['camera_data']
    rh_data      = data['rh_data']
    lh_data      = data['lh_data']
    joint_data   = data['joint_data']
    
    return end_eff_data, camera_data, rh_data, lh_data, joint_data

def segmentbydemo(end_eff_data, camera_data, rh_data, lh_data, joint_data, demo_max, peaks):    
    end_eff=['']*demo_max
    camera =['']*demo_max
    rh     =['']*demo_max
    lh     =['']*demo_max
    joints =['']*demo_max

    for i in range(0,demo_max):
        if i==0:
            end_eff[i] = end_eff_data[1:peaks[0],:]
            camera[i]  =  camera_data[1:peaks[0],:]
            rh[i]      =      rh_data[1:peaks[0],:]
            lh[i]      =      lh_data[1:peaks[0],:]
            joints[i]  =   joint_data[1:peaks[0],:]
        else:
            end_eff[i] = end_eff_data[peaks[i-1]:peaks[i],:]
            camera[i]  =  camera_data[peaks[i-1]:peaks[i],:]
            rh[i]      =      rh_data[peaks[i-1]:peaks[i],:]
            lh[i]      =      lh_data[peaks[i-1]:peaks[i],:]
            joints[i]  =   joint_data[peaks[i-1]:peaks[i],:]

    # end_eff = np.array(end_eff)
    # rh      = np.array(rh)
    # lh      = np.array(lh)
    # joints  = np.array(joints)
    return end_eff, camera, rh, lh, joints

def sumofsquares(a):
    return np.vstack((a[:,0],np.sum(np.multiply(a[:,1:7],a[:,1:7]),axis=1)))
