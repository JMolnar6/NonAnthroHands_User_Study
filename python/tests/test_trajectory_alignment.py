from pathlib import Path

import numpy as np
import pytest
from nah.alignments import dtw_data_import, get_filename


@pytest.fixture
def trajectory():
    """Fixture to load a trajectory"""
    print("Loading traj")
    cwd = Path(__file__).parent.resolve()
    traj_file = cwd / ".." / ".." / "npz_files" / "data_PID13_Reachy_gesture_7.npz"
    print(traj_file)
    traj = np.load(traj_file)
    return traj


def test_get_filename():
    """
    Test if the util for getting the data filename is correct
    """
    data_path = Path("data")

    expected_file1 = data_path / "PID1" /\
        "j2s6s300_PID1_j2s6s300_end_effector_Motion_gesture_3_1.csv"
    robot_name = "j2s6s300"
    end_eff_name = "j2s6s300_end_effector"
    participant_id = 1
    gesture_num = 3
    demo_num = 1
    actual_file1 = get_filename(participant_id, robot_name, end_eff_name,
                                gesture_num, demo_num, False)

    assert expected_file1 == actual_file1

    expected_file2 = data_path / "PID1" / \
        "j2s6s300_PID1_RightHand Controller_Motion_gesture_3_1.csv"

    robot_name = "j2s6s300"
    end_eff_name = "RightHand Controller"
    participant_id = 1
    gesture_num = 3
    demo_num = 1
    actual_file2 = get_filename(participant_id, robot_name, end_eff_name,
                                gesture_num, demo_num, False)

    assert expected_file2 == actual_file2

    expected_file3 = data_path / "PID1" /\
        "Reachy_PID1_Main Camera_Motion_gesture_14_3.csv"

    robot_name = "Reachy"
    end_eff_name = "Main Camera"
    participant_id = 1
    gesture_num = 14
    demo_num = 3
    actual_file3 = get_filename(participant_id, robot_name, end_eff_name,
                                gesture_num, demo_num, False)

    assert expected_file3 == actual_file3

    # This is a follow up study, hence in "Follow-up Study" directory
    # and has a B at the end of the participant ID
    expected_file3 = data_path / "Follow-up Study" /\
        "PID3B" / "Reachy_PID3B_RightHand Controller_Motion_gesture_5_2.csv"

    robot_name = "Reachy"
    end_eff_name = "RightHand Controller"
    participant_id = 3
    gesture_num = 5
    demo_num = 2
    actual_file3 = get_filename(participant_id, robot_name, end_eff_name,
                                gesture_num, demo_num, True)

    assert expected_file3 == actual_file3

def test_dtw_data_import():
    # dtw_data_import(robot_name, end_eff_name, participant_id, True,
    #                            gesture_num, demo_num)
    # assert
