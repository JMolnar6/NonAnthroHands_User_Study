import numpy as np
from nah.utils import dtw_data_import


def test_dtw_data_import():
    robot_name = "j2s6s300"
    end_eff_name = "j2s6s300_end_effector_"
    participant_id = 1
    followup = False
    gesture_num = 3
    demo_num = 1
    controller_data = dtw_data_import(robot_name, end_eff_name, participant_id,
                                      followup, gesture_num, demo_num)

    assert controller_data.shape == (507, 7)

    # test if controller_data is not all zeros
    assert (not np.allclose(controller_data, np.zeros(controller_data.shape)))

    participant_id = 3
    gesture_num = 4
    demo_num = 1
    controller_data = dtw_data_import(robot_name, end_eff_name, participant_id,
                                      followup, gesture_num, demo_num)

    assert controller_data.shape == (555, 7)
    assert (not np.allclose(controller_data, np.zeros(controller_data.shape)))
