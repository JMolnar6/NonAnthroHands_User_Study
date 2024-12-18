import unittest

import numpy as np
from fastdtw import fastdtw
from nah.alignments import Alignment, evo_to_gtsam, manifold_align, pose_dist
from nah.loader import load_npzs
from nah.trajectory import (convert_evo_to_np, get_evo_metrics,
                            get_evo_trajectory)
from nah.utils import segment_by_demo


class TestAlignment(unittest.TestCase):
    """Class to testing alignment code"""

    def setUp(self):
        robot_name = "Reachy"
        participant_id = 13
        followup = False
        gesture_num = 1
        demo_max = 2

        end_eff_data, camera_data, rh_data, lh_data, joint_data = load_npzs(
            robot_name, participant_id, followup, gesture_num)
        self.end_eff, _, self.rh, self.lh, _ = segment_by_demo(
            end_eff_data, camera_data, rh_data, lh_data, joint_data, demo_max)

    def test_get_evo_metrics(self):
        """Test APE metrics for vanilla trajectories."""
        metrics = get_evo_metrics(self.end_eff[0], self.end_eff[1])
        assert metrics['mean'] < 0.01

        metrics = get_evo_metrics(self.rh[0], self.rh[1])
        assert (metrics['mean'] > 0.1 and metrics['mean'] < 0.14)

    def test_get_aligned_evo_metrics(self):
        """Test APE metrics for aligned trajectories."""
        metrics = get_evo_metrics(self.end_eff[0],
                                  self.end_eff[1],
                                  alignment=Alignment.Spatial)
        assert metrics['mean'] < 0.01

        metrics = get_evo_metrics(self.rh[0], self.rh[1])
        assert (metrics['mean'] > 0.1 and metrics['mean'] < 0.14)

    def test_evo_to_gtsam(self):
        """Test conversion of evo.PoseTrajectory3D to list of gtsam.Pose3"""

        evo_traj = get_evo_trajectory(self.end_eff[0])
        gtsam_traj = evo_to_gtsam(evo_traj)

        assert len(gtsam_traj) == 401

    def test_manifold_align(self):
        """Test manifold alignment function"""
        evo_traj1 = get_evo_trajectory(self.end_eff[0])
        evo_traj2 = get_evo_trajectory(self.end_eff[1])

        traj2_aligned = manifold_align(evo_traj1, evo_traj2)

        assert traj2_aligned.timestamps.shape == (396, )

    def test_pose_dist(self):
        """Test custom pose distance metric"""
        p1 = np.zeros((7, ))
        p2 = np.zeros((7, ))

        # Difference between identities should be 0
        assert pose_dist(p1, p2) == 0.0

        p1[1:4] = np.arange(1, 4)
        p2[1:4] = np.arange(1, 4)
        # Difference between the same poses should be 0
        assert pose_dist(p1, p2) == 0.0

        # Compare different translations
        p2[1:4] = np.arange(4, 7)

        assert pose_dist(p1, p2) == \
            np.linalg.norm(np.arange(1, 4) - np.arange(4, 7))

        # Compare rotations
        p1 = np.zeros((7))
        # Pose with rotation of 180 degrees
        p2 = np.zeros((7))
        p2[4] = np.pi

        assert pose_dist(p1, p2) == np.pi

    def test_dtw_pose_dist(self):
        """Test the use of the pose_dist distance function with DTW"""
        # Trivial case when both trajectories are the same
        traj1 = get_evo_trajectory(self.end_eff[0])
        traj2 = get_evo_trajectory(self.end_eff[0])

        traj1 = convert_evo_to_np(traj1)
        traj2 = convert_evo_to_np(traj2)

        distance, path = fastdtw(traj1, traj2, dist=pose_dist)
        np.testing.assert_almost_equal(distance, 0.0)

        # Since the trajectories are the same
        expected_path = [(i, i) for i, _ in enumerate(traj1)]
        assert path == expected_path
