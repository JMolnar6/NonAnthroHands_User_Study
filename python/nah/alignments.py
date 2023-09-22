"""Apply alignments"""

# Plotting Packages
import os
from pathlib import Path

import matplotlib as mpl
# import matplotlib.pyplot as plt
# import numpy as np
import pandas as pd

# from scipy.signal import find_peaks

# from fastdtw import fastdtw
# from mpl_toolkits import mplot3d
# from mpl_toolkits.mplot3d import Axes3D

# Computation packages
# from scipy.spatial.distance import euclidean

mpl.rcParams['figure.dpi'] = 150
savefig_options = dict(format="png", dpi=150, bbox_inches="tight")
