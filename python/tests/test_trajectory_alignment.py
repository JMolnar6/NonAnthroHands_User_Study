from pathlib import Path

import numpy as np
import pytest
from nah.loader import get_filename, load_raw_csv_data


@pytest.fixture
def trajectory():
    """Fixture to load a trajectory"""
    print("Loading traj")
    cwd = Path(__file__).parent.resolve()
    traj_file = cwd / ".." / ".." / "npz_files" / "data_PID13_Reachy_gesture_7.npz"
    print(traj_file)
    traj = np.load(traj_file)
    return traj
