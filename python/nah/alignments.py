"""Module for trajectory alignment"""

from copy import deepcopy
from enum import Enum

import gtsam
import numpy as np
from evo.core.trajectory import PoseTrajectory3D
from fastdtw import fastdtw
from nah.utils import clean_rot_data, full_align, norm_data, sum_of_squares


class Alignment(Enum):
    """Options for trajectory alignment"""
    No = 0
    Spatial = 1  # Use Manifold optimizatio from GTSAM for spatial alignment.
    Temporal = 2  # Use Dynamic Time Waring to temporally align trajectories.
    SpatioTemporal = 3  # Perform spatial and then temporal alignment


def evo_align(traj1, traj2, correct_scale=True):
    """
    Align the second trajectory to the first one.
    Returns the aligned second trajectory.
    """
    traj2_aligned = deepcopy(traj2)
    r, t, s = traj2_aligned.align(traj1, correct_scale=correct_scale)
    return traj2_aligned


def evo_to_gtsam(traj: PoseTrajectory3D):
    """Convert evo.PoseTrajectory3D to a list of gtsam Pose3 objects."""
    gtsam_traj = [gtsam.Pose3(pose) for pose in traj.poses_se3]
    return gtsam_traj


def gtsam_to_evo(traj: list, timestamps):
    """Convert evo.PoseTrajectory3D to a list of gtsam Pose3 objects."""
    xyz = np.empty((len(traj), 3))
    quat_wxyz = np.empty((len(traj), 4))
    for i, pose in enumerate(traj):
        xyz[i] = pose.translation()
        q = pose.rotation().toQuaternion()
        quat_wxyz[i] = np.asarray([q.w(), q.x(), q.y(), q.z()])

    return PoseTrajectory3D(positions_xyz=xyz,
                            orientations_quat_wxyz=quat_wxyz,
                            timestamps=timestamps)


def manifold_align(traj1: PoseTrajectory3D, traj2: PoseTrajectory3D):
    """Perform manifold optimization to find the best Sim(3) alignment between two trajectories."""
    # Get list of (a_pose, b_pose) tuples
    # max_len = max(len(traj1))
    gtsam_traj1 = evo_to_gtsam(traj1)
    gtsam_traj2 = evo_to_gtsam(traj2)

    min_len = min(len(gtsam_traj1), len(gtsam_traj2))
    pose3_pairs = [
        (pose1, pose2)
        for pose1, pose2 in zip(gtsam_traj1[:min_len], gtsam_traj2[:min_len])
    ]
    aSb = gtsam.Similarity3.Align(pose3_pairs)

    gtsam_traj2_aligned = [aSb.transformFrom(bTi) for bTi in gtsam_traj2]
    traj2_aligned = gtsam_to_evo(gtsam_traj2_aligned, traj2.timestamps)
    return traj2_aligned


def pose_dist(p1: np.ndarray, p2: np.ndarray):
    """
    Distance metric between 2 poses to be used with Dynamic Time Warping.

    Given poses p1 and p2, the distance between the two
    is computed as the L2 norm of the vector difference of the Lie algebras.
    """
    # Convert from 1x7 vector of (s, x, y, z, r, p, y) to Pose3 object
    aTb1 = gtsam.Pose3(gtsam.Rot3.Ypr(*p1[4:7][::-1]), p1[1:4])
    aTb2 = gtsam.Pose3(gtsam.Rot3.Ypr(*p2[4:7][::-1]), p2[1:4])

    # Compute the transformation difference in SE(3)
    b1Tb2 = aTb1.inverse() * aTb2

    # Get the se(3) vector corresponding to the difference.
    v = gtsam.Pose3.Logmap(b1Tb2)

    return np.linalg.norm(v)


def compute_dtw_alignment(x, y, dist=None):
    """Align trajectories with DTW"""
    if dist is not None:
        dtw_distance, warp_path = fastdtw(x, y, dist=dist)
    else:
        dtw_distance, warp_path = fastdtw(x, y)

    return dtw_distance, warp_path


def dtw_align(traj1: np.ndarray, traj2: np.ndarray, dist=None):
    """Take two trajectories (preferably already spatially aligned) and 
    return the time-aligned outputs"""
    # x_norm, y_norm = norm_data(sum_of_squares(traj1), sum_of_squares(traj2))

    # _, warp_path = compute_dtw_alignment(x_norm, y_norm)

    dtw_distance, warp_path = compute_dtw_alignment(traj1, traj2, dist=dist)

    traj1_aligned, traj2_aligned = full_align(warp_path, traj1, traj2)
    traj1_rot_aligned = traj1_aligned[:, 4:7]
    traj2_rot_aligned = traj2_aligned[:, 4:7]

    traj1_rot_aligned = clean_rot_data(traj1_rot_aligned)
    traj1_rot_aligned = clean_rot_data(traj1_rot_aligned)
    # Seems to work better if you do it twice for some reason
    traj2_rot_aligned = clean_rot_data(traj2_rot_aligned)
    traj2_rot_aligned = clean_rot_data(traj2_rot_aligned)

    # Return the correct aligned trajectory
    traj1_aligned[:, 4:7] = traj1_rot_aligned

    # Return the correct aligned trajectory
    traj2_aligned[:, 4:7] = traj2_rot_aligned

    return traj1_aligned, traj2_aligned
