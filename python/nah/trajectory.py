"""Trajectory utilities"""

import numpy as np
from evo.core import metrics, sync
from evo.core.trajectory import PoseTrajectory3D
from nah.alignments import Alignment, dtw_align, evo_align, manifold_align, pose_dist
from scipy.spatial.transform import Rotation


def get_evo_trajectory(trajectory):
    """Convert trajectory [timestamp, tx, ty, tz, rx, ry, rz] to evo.PoseTrajectory3D."""
    timestamps = trajectory[:, 0]
    xyz = trajectory[:, 1:4]

    euler_angles = trajectory[:, 4:7]
    Rs = Rotation.from_euler('xyz', euler_angles)
    quat_xyzw = Rs.as_quat()
    quat_wxyz = quat_xyzw[:, (3, 0, 1, 2)]

    return PoseTrajectory3D(positions_xyz=xyz,
                            orientations_quat_wxyz=quat_wxyz,
                            timestamps=timestamps)


def convert_evo_to_np(traj: PoseTrajectory3D):
    """
    Convert an Evo PoseTrajectory3D to a Tx7 numpy array,
    where the 7 dimensions are timestamp, tx, ty, tz, rx, ry, rz.
    """
    array = np.empty((traj.timestamps.shape[0], 7))
    array[:, 0] = traj.timestamps
    array[:, 1:4] = traj.positions_xyz
    array[:, 4:7] = traj.get_orientations_euler()
    return array


def evo_sync(traj1: PoseTrajectory3D, traj2: PoseTrajectory3D):
    """Synchronize trajectories using Evo's associate_trajectories method"""
    traj1, traj2 = sync.associate_trajectories(traj1, traj2)
    return traj1, traj2


def evaluate_ape(traj1: PoseTrajectory3D, traj2: PoseTrajectory3D):
    """Evaluate the Absolute Pose Error (APE) between 2 trajectories."""
    metric = metrics.APE(metrics.PoseRelation.full_transformation)
    metric.process_data((traj1, traj2))

    return metric


def get_evo_metrics(traj1, traj2, alignment=Alignment.No, suppress_plots=True):
    """
    Take two trajectories of equal length and calculate
    the error between them.
    """
    traj1_evo = get_evo_trajectory(traj1)
    traj2_evo = get_evo_trajectory(traj2)

    # Synchronize the trajectories based on timestamps
    traj1_evo, traj2_evo = evo_sync(traj1_evo, traj2_evo)

    if alignment == Alignment.No:
        # Don't do any alignment
        pass

    elif alignment == Alignment.Spatial:
        traj2_evo = evo_align(traj1_evo, traj2_evo)
        #TODO(Varun) Sim3 based alignment doesn't work as well as Umeyama above. Why?
        # traj2_evo = manifold_align(traj1_evo, traj2_evo)

    elif alignment == Alignment.Temporal:
        traj1_evo = convert_evo_to_np(traj1_evo)
        traj2_evo = convert_evo_to_np(traj2_evo)
        traj1_aligned, traj2_aligned = dtw_align(traj1_evo, traj2_evo, dist=pose_dist)
        traj1_evo = get_evo_trajectory(traj1_aligned)
        traj2_evo = get_evo_trajectory(traj2_aligned)

        # raise NotImplementedError("Temporal alignment not implemented.")

    elif alignment == Alignment.SpatioTemporal:
        #TODO(Jennifer) Varun, I wanted to do spatiotemporal alignment and 
        # right now, evo_alignment is working better. I commented out the
        # manifold_alignment but it's ready here for you to re-insert. The 
        # Alignment description in nah.alignments matches what's here
        # traj2_evo_spatial = manifold_align(traj1_evo, traj2_evo)
        traj2_evo_spatial = evo_align(traj1_evo, traj2_evo)
        traj1_spatial = convert_evo_to_np(traj1_evo)
        traj2_spatial = convert_evo_to_np(traj2_evo_spatial)
        traj1_aligned, traj2_aligned = dtw_align(traj1_spatial, traj2_spatial, dist=pose_dist)
        traj1_evo = get_evo_trajectory(traj1_aligned)
        traj2_evo = get_evo_trajectory(traj2_aligned)

    else:
        raise RuntimeError("Invalid Alignment specified.")

    if not suppress_plots:
        # The following is just for debug. Comment it out when you're actually running
        # the correlation matrix code; this is for making sure the DTW stuff is working
        from nah.plot import plot_pos, plot_rot
        try:
            traj1_np = convert_evo_to_np(traj1_evo)
            traj2_np = convert_evo_to_np(traj2_evo)
            plot_pos(traj1_np[:, 1:4], traj2_np[:, 1:4], traj1_np[:, 0],
                    traj2_np[:, 0])
            plot_rot(traj1_np[:, 4:7], traj2_np[:, 4:7], traj1_np[:, 0],
                    traj2_np[:, 0])
        except:
            print("Plot data failed")
            raise

    metric = evaluate_ape(traj1_evo, traj2_evo)
    return metric.get_all_statistics()
