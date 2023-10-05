"""Tests for the nah.utils module"""

from nah.loader import load_npzs
from nah.utils import segment_by_demo


def test_segment_by_demo():
    """Test segment_by_demo file."""
    robot_name = 'Reachy'
    PID = 16
    gesture_num = 12
    followup = False
    demo_max = 2

    end_eff_data, camera_data, rh_data, lh_data, joint_data = load_npzs(
        robot_name, PID, followup, gesture_num)

    end_eff, camera, rh, lh, joints = segment_by_demo(end_eff_data,
                                                      camera_data, rh_data,
                                                      lh_data, joint_data,
                                                      demo_max)

    assert len(end_eff) == 2
    assert end_eff[0].shape == (214, 7)
    assert end_eff[1].shape == (214, 7)

    assert len(camera) == 2
    assert camera[0].shape == (214, 7)
    assert camera[1].shape == (214, 7)

    assert len(rh) == 2
    assert rh[0].shape == (214, 7)
    assert rh[1].shape == (214, 7)

    assert len(lh) == 2
    assert lh[0].shape == (214, 7)
    assert lh[1].shape == (214, 7)

    assert len(joints) == 2
    assert joints[0].shape == (214, 8)
    assert joints[1].shape == (214, 8)
