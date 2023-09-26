from pathlib import Path

import numpy as np
import pytest
from nah.alignments import Alignment, evo_to_gtsam, manifold_align
from nah.loader import load_npzs
from nah.trajectory import get_evo_metrics, get_evo_trajectory
from nah.utils import segment_by_demo


@pytest.fixture
def trajectory():
    """Fixture to load a trajectory"""
    print("Loading traj")
    cwd = Path(__file__).parent.resolve()
    traj_file = cwd / ".." / ".." / "npz_files" / "data_PID13_Reachy_gesture_7.npz"
    print(traj_file)
    traj = np.load(traj_file)
    return traj


def test_get_evo_metrics():
    """Test APE metrics for vanilla trajectories."""
    robot_name = "Reachy"
    participant_id = 13
    followup = False
    gesture_num = 1
    demo_max = 2

    end_eff_data, camera_data, rh_data, lh_data, joint_data = load_npzs(
        robot_name, participant_id, followup, gesture_num)
    end_eff, _, rh, lh, _ = segment_by_demo(end_eff_data, camera_data, rh_data,
                                            lh_data, joint_data, demo_max)

    metrics = get_evo_metrics(end_eff[0], end_eff[1])
    assert metrics['mean'] < 0.01

    metrics = get_evo_metrics(rh[0], rh[1])
    assert (metrics['mean'] > 0.1 and metrics['mean'] < 0.14)


def test_get_aligned_evo_metrics():
    """Test APE metrics for aligned trajectories."""
    robot_name = "Reachy"
    participant_id = 13
    followup = False
    gesture_num = 1
    demo_max = 2

    end_eff_data, camera_data, rh_data, lh_data, joint_data = load_npzs(
        robot_name, participant_id, followup, gesture_num)
    end_eff, _, rh, lh, _ = segment_by_demo(end_eff_data, camera_data, rh_data,
                                            lh_data, joint_data, demo_max)

    metrics = get_evo_metrics(end_eff[0],
                              end_eff[1],
                              alignment=Alignment.Spatial)
    assert metrics['mean'] < 0.01

    metrics = get_evo_metrics(rh[0], rh[1])
    assert (metrics['mean'] > 0.1 and metrics['mean'] < 0.14)


def test_evo_to_gtsam():
    """Test conversion of evo.PoseTrajectory3D to list of gtsam.Pose3"""
    robot_name = "Reachy"
    participant_id = 13
    followup = False
    gesture_num = 1
    demo_max = 2

    end_eff_data, camera_data, rh_data, lh_data, joint_data = load_npzs(
        robot_name, participant_id, followup, gesture_num)
    end_eff, _, _, _, _ = segment_by_demo(end_eff_data, camera_data, rh_data,
                                          lh_data, joint_data, demo_max)

    evo_traj = get_evo_trajectory(end_eff[0])
    gtsam_traj = evo_to_gtsam(evo_traj)

    assert len(gtsam_traj) == 401


def test_manifold_align():
    """Test manifold alignment function"""
    robot_name = "Reachy"
    participant_id = 13
    followup = False
    gesture_num = 1
    demo_max = 2

    end_eff_data, camera_data, rh_data, lh_data, joint_data = load_npzs(
        robot_name, participant_id, followup, gesture_num)
    end_eff, _, _, _, _ = segment_by_demo(end_eff_data, camera_data, rh_data,
                                          lh_data, joint_data, demo_max)

    evo_traj1 = get_evo_trajectory(end_eff[0])
    evo_traj2 = get_evo_trajectory(end_eff[1])

    traj2_aligned = manifold_align(evo_traj1, evo_traj2)

    assert traj2_aligned.timestamps.shape == (396, )
