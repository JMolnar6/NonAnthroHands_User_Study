"""Tests for the nah.loader module."""

from pathlib import Path

import numpy as np
from nah.loader import (dtw_data_import, get_filename, get_npz_filename,
                        load_npzs)


def test_get_filename():
    """
    Test if the util for getting the data filename is correct
    """
    # Path to project root directory
    project_root = Path(__file__).parent.parent.parent.resolve()
    data_path = project_root / "data"

    expected_file1 = data_path / "PID1" / \
        "j2s6s300_PID1_j2s6s300_end_effector_Motion_gesture_3_1.csv"
    robot_name = "j2s6s300"
    end_eff_name = "j2s6s300_end_effector_"
    participant_id = 1
    gesture_num = 3
    demo_num = 1
    actual_file1 = get_filename(participant_id, robot_name, end_eff_name,
                                gesture_num, demo_num, False)

    assert expected_file1 == actual_file1

    expected_file2 = data_path / "PID1" / \
        "j2s6s300_PID1_RightHand Controller_Motion_gesture_3_1.csv"

    robot_name = "j2s6s300"
    end_eff_name = "RightHand Controller_"
    participant_id = 1
    gesture_num = 3
    demo_num = 1
    actual_file2 = get_filename(participant_id, robot_name, end_eff_name,
                                gesture_num, demo_num, False)

    assert expected_file2 == actual_file2

    expected_file3 = data_path / "PID1" / "Reachy_PID1_Main Camera_Motion_gesture_14_3.csv"

    robot_name = "Reachy"
    end_eff_name = "Main Camera_"
    participant_id = 1
    gesture_num = 14
    demo_num = 3
    actual_file3 = get_filename(participant_id, robot_name, end_eff_name,
                                gesture_num, demo_num, False)

    assert expected_file3 == actual_file3

    # This is a follow up study, hence in "Follow-up Study" directory
    # and has a B at the end of the participant ID
    expected_file3 = data_path / "Follow-up Study" / "PID3B" /\
        "Reachy_PID3B_RightHand Controller_Motion_gesture_5_2.csv"

    robot_name = "Reachy"
    end_eff_name = "RightHand Controller_"
    participant_id = 3
    gesture_num = 5
    demo_num = 2
    actual_file3 = get_filename(participant_id, robot_name, end_eff_name,
                                gesture_num, demo_num, True)

    assert expected_file3 == actual_file3


def test_get_npz_filename():
    """
    Test if the util for getting the npz data filename is correct
    """
    # Path to project root directory
    project_root = Path(__file__).parent.parent.parent.resolve()
    data_path = project_root / "npz_files"

    expected_file1 = data_path / "data_PID1_j2s6s300_gesture_3.npz"
    robot_name = "j2s6s300"
    participant_id = 1
    gesture_num = 3
    actual_file1 = get_npz_filename(robot_name, participant_id, gesture_num, False)

    assert expected_file1 == actual_file1


def test_dtw_data_import():
    """Test the dtw_data_import function."""
    robot_name = "j2s6s300"
    end_eff_name = "j2s6s300_end_effector_"
    participant_id = 1
    followup = False
    gesture_num = 3
    demo_num = 1
    controller_data = dtw_data_import(robot_name, end_eff_name, participant_id,
                                      followup, gesture_num, demo_num)

    assert controller_data.shape == (507, 7)

    assert (not np.allclose(controller_data, np.zeros(controller_data.shape)))


def test_load_npzs():
    """Test the load_npzs function."""
    robot_name = "j2s6s300"
    participant_id = 1
    followup = False
    gesture_num = 3

    load_npzs(robot_name, participant_id, followup, gesture_num)
