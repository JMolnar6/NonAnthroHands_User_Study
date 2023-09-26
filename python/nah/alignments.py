"""Module for trajectory alignment"""

from copy import deepcopy
from enum import Enum

import gtsam
from evo.core.trajectory import PoseTrajectory3D


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


def manifold_align(traj1, traj2):
    pass
