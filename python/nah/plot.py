"""Plotting utilities"""
from matplotlib import pyplot as plt
import numpy as np


def plot_norm(warp_path, x_norm, y_norm):
    """Show normalized plots"""
    fig, ax = plt.subplots(figsize=(12, 6))

    # Remove the border and axes ticks
    fig.patch.set_visible(True)
    ax.axis('on')

    max_distance = 0
    i = 0

    for [map_x, map_y] in warp_path:

        ax.plot([map_x, map_y], [x_norm[1][map_x], y_norm[1][map_y]],
                '--k',
                linewidth=0.2)
        temp_arr2 = np.array((x_norm[1][map_x], y_norm[1][map_y]))
        max_distance = np.maximum(max_distance, np.linalg.norm(temp_arr2))
        i = i + 1

    ax.plot(x_norm[1][:],
            '-ro',
            label='x',
            linewidth=0.2,
            markersize=2,
            markerfacecolor='lightcoral',
            markeredgecolor='lightcoral')
    ax.plot(y_norm[1][:],
            '-bo',
            label='y',
            linewidth=0.2,
            markersize=2,
            markerfacecolor='skyblue',
            markeredgecolor='skyblue')

    ax.set_title("Normalized DTW Distance", fontsize=10, fontweight="bold")
    # plt.savefig('NormDTW_Y_PID'+str(PID)+"_gesture"+str(gesture_num)+'_'+str(demo_num)+'.png')
    plt.savefig('NormDTW_temp.png')
    # plt.close('all')

    return


def plot_pos(gesture_num, demo_num, warp_path, end_eff_pos_aligned,
             hand_pos_aligned, time_URDF_aligned, time_hand_aligned):
    fig, ax = plt.subplots(figsize=(15, 10))

    # Show the border and axes ticks,
    fig.patch.set_visible(True)
    ax.axis('on')
    ax = plt.axes(projection='3d')
    ax.tick_params(axis='x', labelsize=10)
    ax.tick_params(axis='y', labelsize=10)
    ax.tick_params(axis='z', labelsize=10)

    end_eff_pos_aligned = end_eff_pos_aligned - end_eff_pos_aligned[1]
    hand_pos_aligned = hand_pos_aligned - hand_pos_aligned[1]

    # ax.plot(time_URDF_aligned, end_eff_pos_aligned[:].T[0], '-ro', label='End-effector position', \
    #     linewidth=0.2, markersize=2, markerfacecolor='lightcoral', markeredgecolor='lightcoral')
    # ax.plot(time_URDF_aligned, end_eff_pos_aligned[:].T[1], '-ro', label='End-effector position', \
    #     linewidth=0.2, markersize=2, markerfacecolor='lightcoral', markeredgecolor='lightcoral')
    # ax.plot(time_URDF_aligned, end_eff_pos_aligned[:].T[2], '-ro', label='End-effector position', \
    #     linewidth=0.2, markersize=2, markerfacecolor='lightcoral', markeredgecolor='lightcoral')

    # Unity uses a left-handed coordinate system, so plot your position data in the orientation in which it was gathered:
    #  X moving left to right, Z moving front to back, and Y pointing up and down
    ax.scatter(end_eff_pos_aligned[:].T[0],
               -end_eff_pos_aligned[:].T[2],
               end_eff_pos_aligned[:].T[1],
               c=time_URDF_aligned / max(time_URDF_aligned),
               cmap='Reds',
               label='End-effector position')
    ax.scatter(hand_pos_aligned[:].T[0],
               -hand_pos_aligned[:].T[2],
               hand_pos_aligned[:].T[1],
               c=time_hand_aligned / max(time_hand_aligned),
               cmap='Blues',
               label='Hand position')

    # ax.plot3D(end_eff_pos_aligned[:].T[0], end_eff_pos_aligned[:].T[1], end_eff_pos_aligned[:].T[2], \
    #     '-ro', label='End-effector position', linewidth=0.2, markersize=2, markerfacecolor='lightcoral', markeredgecolor='lightcoral')
    # ax.plot3D(hand_pos_aligned[:].T[0]   , hand_pos_aligned[:].T[1]   , hand_pos_aligned[:].T[2]   , \
    #     '-bo', label='Hand position', linewidth=0.2, markersize=2, markerfacecolor='skyblue', markeredgecolor='skyblue')

    for [map_x, map_y] in warp_path:
        ax.plot3D(
            [end_eff_pos_aligned[map_x].T[0], hand_pos_aligned[map_y].T[0]],
            [-end_eff_pos_aligned[map_x].T[2], -hand_pos_aligned[map_y].T[2]],
            [end_eff_pos_aligned[map_x].T[1], hand_pos_aligned[map_y].T[1]],
            '--k',
            linewidth=0.2)

    ax.set_xlabel('Horizontal position (m)', fontsize=16)
    ax.set_ylabel('Forward/Back position (m)', fontsize=16)
    ax.set_zlabel('Vertical position (m)', fontsize=16)
    ax.legend(loc='lower right', fontsize=14)

    ax.set_title("DTW Alignment of Hand and URDF End-Effector Position",
                 fontsize=18,
                 fontweight="bold")
    plt.savefig('DTW_Pos' + str(demo_num) + '.png')
    plt.close('all')

    return


def plot_rot(gesture_num, demo_num, warp_path, end_eff_rot_aligned,
             hand_rot_aligned, time_URDF_aligned, time_hand_aligned):
    # Plot DTW-aligned hand/end-effector orientation

    fig, ax = plt.subplots(figsize=(15, 10))

    # Show the border and axes ticks
    fig.patch.set_visible(True)
    ax.axis('on')

    ax = plt.axes(projection='3d')

    ax.scatter(end_eff_rot_aligned[:].T[0],
               -end_eff_rot_aligned[:].T[2],
               end_eff_rot_aligned[:].T[1],
               c=time_URDF_aligned / max(time_URDF_aligned),
               cmap='Reds',
               label='End-effector orientation')
    ax.scatter(hand_rot_aligned[:].T[0],
               -hand_rot_aligned[:].T[2],
               hand_rot_aligned[:].T[1],
               c=time_hand_aligned / max(time_hand_aligned),
               cmap='Blues',
               label='Hand orientation')

    for [map_x, map_y] in warp_path:
        ax.plot3D(
            [end_eff_rot_aligned[map_x].T[0], hand_rot_aligned[map_y].T[0]],
            [-end_eff_rot_aligned[map_x].T[2], -hand_rot_aligned[map_y].T[2]],
            [end_eff_rot_aligned[map_x].T[1], hand_rot_aligned[map_y].T[1]],
            '--k',
            linewidth=0.2)

    ax.set_title("DTW Alignment of Hand and URDF End-Effector Orientation",
                 fontsize=20,
                 fontweight="bold")
    plt.savefig('DTW_Rot' + str(demo_num) + '.png')
    plt.close('all')

    return


def plot_raw_data(end_eff_data, rh_data, lh_data, camera_data, joint_data,
                  start_index, end_index):

    #     # Quick and dirty clipping (should be done by DTW instead)
    time = end_eff_data[..., 0]
    # start_index = np.where(time>time[0]+1)[0][0]
    # end_index   = np.where(time>time[-1]-1)[0][0]

    fig, ax = plt.subplots(figsize=(12, 16))
    fig.patch.set_visible(True)
    ax.axis('off')
    ax = plt.axes(projection='3d')
    ax.view_init(30, 60)
    ax.set_xlabel('X')
    ax.set_ylabel('Y')
    ax.set_zlabel('Z')

    ax.set_xlim(-.5, .5)
    ax.set_ylim(-.25, .75)
    ax.set_zlim(-.5, .5)

    centered_rh_data = rh_data - camera_data
    centered_lh_data = lh_data - camera_data
    centered_camera_data = camera_data - camera_data

    centered_rh_data = centered_rh_data[start_index:end_index, :]
    centered_lh_data = centered_lh_data[start_index:end_index, :]
    centered_camera_data = centered_camera_data[start_index:end_index, :]

    # Ideally, also need to normalize by participant height (wingspan)
    # And clip ends (~1sec at beginning, 2sec at end (but DTW should help with this))
    # np.where(time_hand_aligned>time_hand_aligned[0]+1)[0][0]

    #     ax.scatter(rh_data[:].T[1], rh_data[:].T[2], -rh_data[:].T[3],\
    #                 c=time/max(time), cmap='Reds', label='Right-hand position')
    #     ax.scatter(lh_data[:].T[1], lh_data[:].T[2], -lh_data[:].T[3], \
    #                c=time/max(time), cmap='Blues', label='Left-hand position')
    #     ax.scatter(camera_data[:].T[1], camera_data[:].T[2], -camera_data[:].T[3], \
    #                c=time/max(time), cmap='Greens', label='Camera position')

    ax.scatter(centered_rh_data[:].T[1],
               centered_rh_data[:].T[3],
               centered_rh_data[:].T[2],
               c=time[start_index:end_index] / max(time),
               cmap='Reds',
               label='Right-hand position')
    ax.scatter(centered_lh_data[:].T[1],
               centered_lh_data[:].T[3],
               centered_lh_data[:].T[2],
               c=time[start_index:end_index] / max(time),
               cmap='Blues',
               label='Left-hand position')
    ax.scatter(centered_camera_data[:].T[1],
               centered_camera_data[:].T[3],
               centered_camera_data[:].T[2],
               c=time[start_index:end_index] / max(time),
               cmap='Greens',
               label='Camera position')

    ax.legend()


def plot_raw_data_subsampled(subsample, end_eff_data, camera_data, rh_data,
                             lh_data, joint_data):

    #     # Quick and dirty clipping (should be done by DTW instead)
    time = end_eff_data[..., 0]
    # start_index = np.where(time>time[0]+1)[0][0]
    # end_index   = np.where(time>time[-1]-1)[0][0]

    start_index = 1  #77
    end_index = -1  #-154

    fig, ax = plt.subplots(figsize=(12, 16))
    fig.patch.set_visible(True)
    ax.axis('off')
    ax = plt.axes(projection='3d')
    ax.view_init(30, 60)
    ax.set_xlabel('X')
    ax.set_ylabel('Y')
    ax.set_zlabel('Z')

    # ax.set_xlim(-.5,.5)
    # ax.set_ylim(-.25,.75)
    # ax.set_zlim(-.5,.5)

    centered_rh_data = rh_data - camera_data
    centered_lh_data = lh_data - camera_data
    centered_camera_data = camera_data - camera_data

    centered_rh_data = centered_rh_data[start_index:end_index:subsample, :]
    centered_lh_data = centered_lh_data[start_index:end_index:subsample, :]
    centered_camera_data = centered_camera_data[
        start_index:end_index:subsample, :]

    #End eff data is not centered, but merely subsampled:
    centered_end_eff_data = end_eff_data[start_index:end_index:subsample, :]

    # Ideally, also need to normalize by participant height (wingspan)
    # And clip ends (~1sec at beginning, 2sec at end (but DTW should help with this))
    # np.where(time_hand_aligned>time_hand_aligned[0]+1)[0][0]

    #     ax.scatter(rh_data[:].T[1], rh_data[:].T[2], -rh_data[:].T[3],\
    #                 c=time/max(time), cmap='Reds', label='Right-hand position')
    #     ax.scatter(lh_data[:].T[1], lh_data[:].T[2], -lh_data[:].T[3], \
    #                c=time/max(time), cmap='Blues', label='Left-hand position')
    #     ax.scatter(camera_data[:].T[1], camera_data[:].T[2], -camera_data[:].T[3], \
    #                c=time/max(time), cmap='Greens', label='Camera position')

    ax.scatter(centered_rh_data[:].T[1],
               centered_rh_data[:].T[3],
               centered_rh_data[:].T[2],
               c=time[start_index:end_index:subsample] / max(time),
               cmap='Reds',
               label='Right-hand position')
    ax.scatter(centered_lh_data[:].T[1],
               centered_lh_data[:].T[3],
               centered_lh_data[:].T[2],
               c=time[start_index:end_index:subsample] / max(time),
               cmap='Blues',
               label='Left-hand position')
    ax.scatter(centered_camera_data[:].T[1],
               centered_camera_data[:].T[3],
               centered_camera_data[:].T[2],
               c=time[start_index:end_index:subsample] / max(time),
               cmap='Greens',
               label='Camera position')
    ax.scatter(centered_end_eff_data[:].T[1],
               centered_end_eff_data[:].T[3],
               centered_end_eff_data[:].T[2],
               c=time[start_index:end_index:subsample] / max(time),
               cmap='Purples',
               label='End-effector position')

    # ax.plot(centered_rh_data[:].T[1], centered_rh_data[:].T[3], centered_rh_data[:].T[2])
    # ax.plot(centered_lh_data[:].T[1], centered_lh_data[:].T[3], centered_lh_data[:].T[2])
    # ax.plot(centered_camera_data[:].T[1], centered_camera_data[:].T[3], centered_camera_data[:].T[2])

    ax.legend()
