"""Plotting utilities"""
import numpy as np
from matplotlib import pyplot as plt
from nah.loader import load_npzs
from tqdm import tqdm


def plot_norm(warp_path, x_norm, y_norm, fig_size=(12, 6)):
    """Show normalized plots"""
    fig, ax = plt.subplots(figsize=fig_size)

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


def plot_pos(gesture_num,
             demo_num,
             warp_path,
             end_eff_pos_aligned,
             hand_pos_aligned,
             time_URDF_aligned,
             time_hand_aligned,
             fig_size=(6, 4)):
    fig, ax = plt.subplots(figsize=fig_size)

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

    # ax.plot(time_URDF_aligned, end_eff_pos_aligned[:, 0], '-ro', label='End-effector position', \
    #     linewidth=0.2, markersize=2, markerfacecolor='lightcoral', markeredgecolor='lightcoral')
    # ax.plot(time_URDF_aligned, end_eff_pos_aligned[:, 1], '-ro', label='End-effector position', \
    #     linewidth=0.2, markersize=2, markerfacecolor='lightcoral', markeredgecolor='lightcoral')
    # ax.plot(time_URDF_aligned, end_eff_pos_aligned[:, 2], '-ro', label='End-effector position', \
    #     linewidth=0.2, markersize=2, markerfacecolor='lightcoral', markeredgecolor='lightcoral')

    # Unity uses a left-handed coordinate system, so plot your position data in the orientation in which it was gathered:
    #  X moving left to right, Z moving front to back, and Y pointing up and down
    ax.scatter(end_eff_pos_aligned[:, 0],
               -end_eff_pos_aligned[:, 2],
               end_eff_pos_aligned[:, 1],
               c=time_URDF_aligned / max(time_URDF_aligned),
               cmap='Reds',
               label='End-effector position')
    ax.scatter(hand_pos_aligned[:, 0],
               -hand_pos_aligned[:, 2],
               hand_pos_aligned[:, 1],
               c=time_hand_aligned / max(time_hand_aligned),
               cmap='Blues',
               label='Hand position')

    # ax.plot3D(end_eff_pos_aligned[:, 0], end_eff_pos_aligned[:, 1], end_eff_pos_aligned[:, 2], \
    #     '-ro', label='End-effector position', linewidth=0.2, markersize=2, markerfacecolor='lightcoral', markeredgecolor='lightcoral')
    # ax.plot3D(hand_pos_aligned[:, 0]   , hand_pos_aligned[:, 1]   , hand_pos_aligned[:, 2]   , \
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


def plot_rot(gesture_num,
             demo_num,
             warp_path,
             end_eff_rot_aligned,
             hand_rot_aligned,
             time_URDF_aligned,
             time_hand_aligned,
             fig_size=(6, 4)):
    # Plot DTW-aligned hand/end-effector orientation

    fig, ax = plt.subplots(figsize=fig_size)

    # Show the border and axes ticks
    fig.patch.set_visible(True)
    ax.axis('on')

    ax = plt.axes(projection='3d')
    ax.set_box_aspect([1.0, 1.0, 1.0])

    ax.scatter(end_eff_rot_aligned[:, 0],
               -end_eff_rot_aligned[:, 2],
               end_eff_rot_aligned[:, 1],
               c=time_URDF_aligned / max(time_URDF_aligned),
               cmap='Reds',
               label='End-effector orientation')
    ax.scatter(hand_rot_aligned[:, 0],
               -hand_rot_aligned[:, 2],
               hand_rot_aligned[:, 1],
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


def plot_rot_2D(time,
                traj,
                fig_size=(10, 7),
                save_name='DTW_Rot_corrected.png'):
    """
    Plot rotation data over time, one line per euler angle
    Arguments: time[n]
               traj[n,3] 
    """

    _, ax = plt.subplots(figsize=fig_size)
    ax.plot(time,
            traj[:, 0],
            '-ko',
            label='x',
            linewidth=0.2,
            markersize=2,
            markerfacecolor='lightcoral',
            markeredgecolor='lightcoral')
    ax.plot(time,
            traj[:, 1],
            '-bo',
            label='x',
            linewidth=0.2,
            markersize=2,
            markerfacecolor='skyblue',
            markeredgecolor='skyblue')
    ax.plot(time,
            traj[:, 2],
            '-ro',
            label='x',
            linewidth=0.2,
            markersize=2,
            markerfacecolor='red',
            markeredgecolor='red')

    plt.show()
    # plt.savefig(save_name)
    # plt.close('all')


def plot_raw_data(
    subsample,
    end_eff_data,
    camera_data,
    rh_data,
    lh_data,
    joint_data,
    centered=False,
    start_index=1,  #77
    end_index=-1,  #-154
    title="Raw Data",
    fig_size=(7, 6)):

    # Quick and dirty clipping (should be done by DTW instead)
    time = end_eff_data[..., 0]
    # start_index = np.where(time>time[0]+1)[0][0]
    # end_index   = np.where(time>time[-1]-1)[0][0]

    fig, ax = plt.subplots(figsize=fig_size)
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

    if centered:
        rh_data[:, 1:7] = rh_data[:, 1:7] - camera_data[:, 1:7]
        lh_data[:, 1:7] = lh_data[:, 1:7] - camera_data[:, 1:7]
        camera_data[:, 1:7] = camera_data[:, 1:7] - camera_data[:, 1:7]

    # Subsample the data.
    subsampled_rh_data = rh_data[start_index:end_index:subsample, :]
    subsampled_lh_data = lh_data[start_index:end_index:subsample, :]
    subsampled_camera_data = camera_data[start_index:end_index:subsample, :]
    subsampled_end_eff_data = end_eff_data[start_index:end_index:subsample, :]

    # ax.scatter(rh_data[:, 1], rh_data[:, 2], -rh_data[:, 3],\
    #             c=time/max(time), cmap='Reds', label='Right-hand position')
    # ax.scatter(lh_data[:, 1], lh_data[:, 2], -lh_data[:, 3], \
    #             c=time/max(time), cmap='Blues', label='Left-hand position')
    # ax.scatter(camera_data[:, 1], camera_data[:, 2], -camera_data[:, 3], \
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


def view_participant_robot_gesture(robot_name,
                                   particiant_ids,
                                   gesture_num,
                                   followup,
                                   centered=False):
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

    plot_raw_data(1,
                  total_end_eff,
                  total_camera,
                  total_rh,
                  total_lh,
                  total_joint,
                  centered=centered)


def generate_self_similarity_heat_map(robot_name, followup, demo_max):
    """ Self-similarity heat map:
    Note: participant demos do not have to be time or space-aligned before processing.
    1) For each robot, go through all participants and gestures and do 
        pairwise comparisons between all 5 participant demos (10 comparisons each)
        (If there are fewer than 5 demos, leave it blank for now. (Assert something
        ludicrious, like infinity or zero.))
    2) Average the RMSEs and put into a matrix. Each row is for one participant; 
        each column is for a gesture 
    3) Produce a heatmap for the results and title it based off the robot and whether 
        this is the original or follow-up study. Clarify that this heatmap is for 
        self-similarity of participant demonstrations
    """

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


def plot_heatmap(robot_name, followup, demo_heatmap_array):
    participant_labels = []
    gesture_labels = []
    if followup:
        participant_max = 9
        gesture_max = 6
    else:
        participant_max = 16
        gesture_max = 15
    for i in range(1, participant_max + 1):
        participant_labels.append("Participant " + str(i))
    for i in range(1, gesture_max + 1):
        gesture_labels.append("Gesture " + str(i))

    df = pd.DataFrame(demo_heatmap_array, columns=gesture_labels)
    df.index = participant_labels

    title = "Participant Demonstration Self-Similarity (RMSE)\n for " + robot_name + " Robot"
    if followup:
        title += ",\n Follow-up Study"

    ax = sns.heatmap(df)
    ax.set_title(title, fontsize=14, fontweight="bold")
    plt.tight_layout()
    plt.show()
    figname = 'Self_Similarity_' + robot_name + '_v0_LinearPlot_withCentering'
    if followup:
        figname += '_FollowUpStudy'
    figname += '.png'
    plt.savefig(figname)
    plt.close("all")

    figname = 'Self_Similarity_' + robot_name + '_v0_LogPlot_withCentering'
    if followup:
        figname += '_FollowUpStudy'
    figname += '.png'
    ax = sns.heatmap(df, norm=LogNorm())
    ax.set_title(title, fontsize=14, fontweight="bold")
    plt.tight_layout()
    plt.show()
    plt.savefig(figname)
