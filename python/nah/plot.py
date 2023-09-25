"""Plotting utilities"""
import numpy as np
from matplotlib import pyplot as plt
from nah.loader import load_npzs
from tqdm import tqdm


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


def set_axes_equal(ax: plt.Axes):
    """Set 3D plot axes to equal scale.

    Make axes of 3D plot have equal scale so that spheres appear as
    spheres and cubes as cubes.  Required since `ax.axis('equal')`
    and `ax.set_aspect('equal')` don't work on 3D.
    """
    limits = np.array([
        ax.get_xlim3d(),
        ax.get_ylim3d(),
        ax.get_zlim3d(),
    ])
    origin = np.mean(limits, axis=1)
    radius = 0.5 * np.max(np.abs(limits[:, 1] - limits[:, 0]))
    _set_axes_radius(ax, origin, radius)


def _set_axes_radius(ax, origin, radius):
    x, y, z = origin
    ax.set_xlim3d([x - radius, x + radius])
    ax.set_ylim3d([y - radius, y + radius])
    ax.set_zlim3d([z - radius, z + radius])


def plot_pos(gesture_num, demo_num, warp_path, end_eff_pos_aligned,
             hand_pos_aligned, time_URDF_aligned, time_hand_aligned):
    fig, ax = plt.subplots(figsize=(6, 4))

    # Show the border and axes ticks,
    fig.patch.set_visible(True)
    ax.axis('on')
    ax = plt.axes(projection='3d')
    ax.tick_params(axis='x')  #, labelsize=10)
    ax.tick_params(axis='y')  #, labelsize=10)
    ax.tick_params(axis='z')  #, labelsize=10)
    ax.set_box_aspect([1.0, 1.0, 1.0])

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

    set_axes_equal(ax)

    ax.set_xlabel('Horizontal position (m)', fontsize=16)
    ax.set_ylabel('Forward/Back position (m)', fontsize=16)
    ax.set_zlabel('Vertical position (m)', fontsize=16)
    ax.legend(loc='lower right', fontsize=14)

    ax.set_title("DTW Alignment of Hand and URDF End-Effector Position",
                 fontsize=14,
                 fontweight="bold")
    plt.tight_layout()
    plt.show()
    # plt.savefig('DTW_Pos' + str(demo_num) + '.png')
    # plt.close('all')

    return


def plot_rot(gesture_num, demo_num, warp_path, end_eff_rot_aligned,
             hand_rot_aligned, time_URDF_aligned, time_hand_aligned):
    # Plot DTW-aligned hand/end-effector orientation

    fig, ax = plt.subplots(figsize=(6, 4))

    # Show the border and axes ticks
    fig.patch.set_visible(True)
    ax.axis('on')

    ax = plt.axes(projection='3d')
    ax.set_box_aspect([1.0, 1.0, 1.0])

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

    set_axes_equal(ax)

    ax.set_title("DTW Alignment of Hand and URDF End-Effector Orientation",
                 fontsize=14,
                 fontweight="bold")
    # plt.savefig('DTW_Rot' + str(demo_num) + '.png')
    # plt.close('all')
    plt.tight_layout()
    plt.show()

    return


def plot_rot_2D(time, traj):
    """
    Plot rotation data over time, one line per euler angle
    Arguments: time[n]
               traj[n,3] 
    """

    fig, ax = plt.subplots(figsize=(10, 7))
    ax.plot(time,
            traj[:].T[0],
            '-ko',
            label='x',
            linewidth=0.2,
            markersize=2,
            markerfacecolor='lightcoral',
            markeredgecolor='lightcoral')
    ax.plot(time,
            traj[:].T[1],
            '-bo',
            label='x',
            linewidth=0.2,
            markersize=2,
            markerfacecolor='skyblue',
            markeredgecolor='skyblue')
    ax.plot(time,
            traj[:].T[2],
            '-ro',
            label='x',
            linewidth=0.2,
            markersize=2,
            markerfacecolor='red',
            markeredgecolor='red')

    plt.show()
    # plt.savefig('DTW_Rot_corrected_' + str(demo_num) + '.png')
    # plt.close('all')


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

    subsampled_rh_data = rh_data - camera_data
    subsampled_lh_data = lh_data - camera_data
    subsampled_camera_data = camera_data - camera_data

    subsampled_rh_data = subsampled_rh_data[start_index:end_index, :]
    subsampled_lh_data = subsampled_lh_data[start_index:end_index, :]
    subsampled_camera_data = subsampled_camera_data[start_index:end_index, :]

    # Ideally, also need to normalize by participant height (wingspan)
    # And clip ends (~1sec at beginning, 2sec at end (but DTW should help with this))
    # np.where(time_hand_aligned>time_hand_aligned[0]+1)[0][0]

    #     ax.scatter(rh_data[:].T[1], rh_data[:].T[2], -rh_data[:].T[3],\
    #                 c=time/max(time), cmap='Reds', label='Right-hand position')
    #     ax.scatter(lh_data[:].T[1], lh_data[:].T[2], -lh_data[:].T[3], \
    #                c=time/max(time), cmap='Blues', label='Left-hand position')
    #     ax.scatter(camera_data[:].T[1], camera_data[:].T[2], -camera_data[:].T[3], \
    #                c=time/max(time), cmap='Greens', label='Camera position')

    ax.scatter(subsampled_rh_data[:].T[1],
               subsampled_rh_data[:].T[3],
               subsampled_rh_data[:].T[2],
               c=time[start_index:end_index] / max(time),
               cmap='Reds',
               label='Right-hand position')
    ax.scatter(subsampled_lh_data[:].T[1],
               subsampled_lh_data[:].T[3],
               subsampled_lh_data[:].T[2],
               c=time[start_index:end_index] / max(time),
               cmap='Blues',
               label='Left-hand position')
    ax.scatter(subsampled_camera_data[:].T[1],
               subsampled_camera_data[:].T[3],
               subsampled_camera_data[:].T[2],
               c=time[start_index:end_index] / max(time),
               cmap='Greens',
               label='Camera position')

    ax.legend()


def plot_raw_data_subsampled(subsample,
                             end_eff_data,
                             camera_data,
                             rh_data,
                             lh_data,
                             joint_data,
                             title="Raw Data"):

    # Quick and dirty clipping (should be done by DTW instead)
    time = end_eff_data[..., 0]
    # start_index = np.where(time>time[0]+1)[0][0]
    # end_index   = np.where(time>time[-1]-1)[0][0]

    start_index = 1  #77
    end_index = -1  #-154

    fig, ax = plt.subplots(figsize=(7, 6))
    fig.patch.set_visible(True)
    fig.suptitle(title)
    fig.canvas.manager.set_window_title(title.lower())

    ax.axis('off')
    ax = plt.axes(projection='3d')
    ax.view_init(30, 60)
    ax.set_xlabel('X')
    ax.set_ylabel('Y')
    ax.set_zlabel('Z')

    ax.set_xlim(-1.5, 1.5)
    ax.set_ylim(-.75, 2.15)
    ax.set_zlim(-1.5, 1.5)

    # centered_rh_data = rh_data - camera_data
    # centered_lh_data = lh_data - camera_data
    # centered_camera_data = camera_data - camera_data

    subsampled_rh_data = rh_data[start_index:end_index:subsample, :]
    subsampled_lh_data = lh_data[start_index:end_index:subsample, :]
    subsampled_camera_data = camera_data[start_index:end_index:subsample, :]
    subsampled_end_eff_data = end_eff_data[start_index:end_index:subsample, :]

    # ax.scatter(rh_data[:].T[1], rh_data[:].T[2], -rh_data[:].T[3],\
    #             c=time/max(time), cmap='Reds', label='Right-hand position')
    # ax.scatter(lh_data[:].T[1], lh_data[:].T[2], -lh_data[:].T[3], \
    #             c=time/max(time), cmap='Blues', label='Left-hand position')
    # ax.scatter(camera_data[:].T[1], camera_data[:].T[2], -camera_data[:].T[3], \
    #             c=time/max(time), cmap='Greens', label='Camera position')

    ax.scatter(subsampled_rh_data[:, 1],
               subsampled_rh_data[:, 3],
               subsampled_rh_data[:, 2],
               c=rh_data[start_index:end_index:subsample, 0] /
               max(rh_data[:, 0]),
               cmap='Reds',
               label='Right-hand position')
    ax.scatter(subsampled_lh_data[:, 1],
               subsampled_lh_data[:, 3],
               subsampled_lh_data[:, 2],
               c=lh_data[start_index:end_index:subsample, 0] /
               max(lh_data[:, 0]),
               cmap='Blues',
               label='Left-hand position')
    ax.scatter(subsampled_camera_data[:, 1],
               subsampled_camera_data[:, 3],
               subsampled_camera_data[:, 2],
               c=camera_data[start_index:end_index:subsample, 0] /
               max(camera_data[:, 0]),
               cmap='Greens_r',
               label='Camera position')
    ax.scatter(subsampled_end_eff_data[:, 1],
               subsampled_end_eff_data[:, 3],
               subsampled_end_eff_data[:, 2],
               c=end_eff_data[start_index:end_index:subsample, 0] /
               max(end_eff_data[:, 0]),
               cmap='copper',
               label='End-effector position')

    ax.legend()
    leg = ax.get_legend()
    leg.legendHandles[0].set_color('red')
    leg.legendHandles[1].set_color('blue')
    leg.legendHandles[2].set_color('green')
    leg.legendHandles[3].set_color('#B87333')


def view_participant_robot_gesture(robot_name, particiant_ids, gesture_num,
                                   followup):
    """
    Provides a quick way to visualize a single gesture for one or all participants.

    `participant_ids` is a tuple of ids, e.g. (1, 2, 3, 7).
    """
    if followup:
        assert max(
            particiant_ids
        ) <= 9, "followup is true, so the maximum ID value should be 9"
    else:
        assert max(particiant_ids) <= 16

    #Initialize data arrays
    total_end_eff = np.empty((0, 7))
    total_camera = np.empty((0, 7))
    total_rh = np.empty((0, 7))
    total_lh = np.empty((0, 7))
    total_joint = np.empty((0, 8))

    for participant_id in tqdm(particiant_ids):
        end_eff, camera, rh, lh, joint = load_npzs(robot_name, participant_id,
                                                   followup, gesture_num)

        total_end_eff = np.vstack((total_end_eff, end_eff))
        total_camera = np.vstack((total_camera, camera))
        total_rh = np.vstack((total_rh, rh))
        total_lh = np.vstack((total_lh, lh))
        total_joint = np.vstack((total_joint, joint))

    # plot_raw_data(end_eff, rh, lh, camera, joint, start_index, end_index)
    plot_raw_data_subsampled(1, total_end_eff, total_camera, total_rh,
                             total_lh, total_joint)
