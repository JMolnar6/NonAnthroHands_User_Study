"""Trajectory utilities"""

from copy import deepcopy

import numpy as np
from evo.core import metrics, sync
from evo.core.trajectory import PoseTrajectory3D
from scipy.spatial.transform import Rotation


def get_evo_trajectory(trajectory):
    """timestamp, tx, ty, tz, rx, ry, rz"""
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


def evo_align(traj1, traj2, correct_scale=True):
    """
    Align the first trajectory to the second one.
    Returns the aligned first trajectory.
    """
    traj1_aligned = deepcopy(traj1)
    r, t, s = traj1_aligned.align(traj2, correct_scale=correct_scale)
    print(r, t, s)
    return traj1_aligned


def evaluate_ape(traj1: PoseTrajectory3D, traj2: PoseTrajectory3D):
    """Evaluate the Absolute Pose Error (APE) between 2 trajectories."""
    metric = metrics.APE(metrics.PoseRelation.full_transformation)
    metric.process_data((traj1, traj2))

    return metric
