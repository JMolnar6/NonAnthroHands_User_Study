"""Run batch trajectory alignment"""

# Plotting Packages
import matplotlib as mpl
import numpy as np
# from ipywidgets import *
from nah.datagraphs import generate_pairwise_comparison
from nah.loader import load_npzs
from nah.trajectory import Alignment
from nah.utils import segment_by_demo

mpl.rcParams['figure.dpi'] = 150
savefig_options = dict(format="png", dpi=150, bbox_inches="tight")

# robot_name='j2s6s300'
robot_name = 'Reachy'
gesture_num = 2

total_end_eff = np.array([])
total_camera = np.array([])
total_rh = np.array([])
total_lh = np.array([])
total_joint = np.array([])

singlePID = True
singlePIDval = 3
followup = True

if singlePID:
    PID_begin_range = singlePIDval
    PID_end_range = singlePIDval + 1  #Don't forget to +1 to whatever your last PID is
else:
    PID_begin_range = 1
    if followup:
        PID_end_range = 10  #Don't forget to +1 to whatever your last PID is
    else:
        PID_end_range = 17
for PID in range(PID_begin_range, PID_end_range):
    end_eff, camera, rh, lh, joint = load_npzs(robot_name, PID, followup,
                                               gesture_num)
    if PID == PID_begin_range:
        total_end_eff = end_eff
        total_camera = camera
        total_rh = rh
        total_lh = lh
        total_joint = joint
    else:
        total_end_eff = np.vstack((total_end_eff, end_eff))
        total_camera = np.vstack((total_camera, camera))
        total_rh = np.vstack((total_rh, rh))
        total_lh = np.vstack((total_lh, lh))
        total_joint = np.vstack((total_joint, joint))

# plot_raw_data(5, total_end_eff, total_camera, total_rh, total_lh, total_joint)

demo_max = 5
end_eff, camera, rh, lh, joints = segment_by_demo(total_end_eff, total_camera,
                                                  total_rh, total_lh,
                                                  total_joint, demo_max)

# for i in range(0,5):
#     plot_raw_data(5, end_eff[i], camera[i], rh[i], lh[i], joint[i])

# plot_raw_data(5, total_end_eff, total_camera, total_rh, total_lh, total_joint)

# robot_name = "j2s6s300"
robot_name = "Reachy"
followup = False
demo_max = 5
gesture = 1

participant_1, participant_2 = 12, 13
print(f"{participant_1=}\t{participant_2=}")

generate_pairwise_comparison(participant_1,
                             participant_2,
                             robot_name,
                             gesture,
                             followup,
                             demo_max,
                             alignment=Alignment.Spatial)

# correlation_array, hand_array = generate_cross_correlation_matrix(
#     robot_name, gesture, followup, demo_max)
# plot_heatmap(robot_name, followup, correlation_array, hand_array)
