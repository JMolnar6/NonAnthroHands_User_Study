"""Plot trajectories for a demo before and after spatial alignment."""

import argparse

from matplotlib import pyplot as plt
from nah.loader import load_npzs
from nah.plot import plot_raw_data_subsampled
from nah.trajectory import convert_evo_to_np, evo_align, get_evo_trajectory
from nah.utils import segment_by_demo


def parse_args():
    """Parse commandline args"""
    parser = argparse.ArgumentParser()
    parser.add_argument("--participant_id",
                        "-p",
                        type=int,
                        help="The participant ID",
                        default=1)
    parser.add_argument("--gesture_num",
                        type=int,
                        help="The gesture number",
                        default=1)
    parser.add_argument("--demo_num",
                        type=int,
                        help="Which demo to use",
                        default=1)
    parser.add_argument("--followup", "-f", action='store_true', default=False)
    parser.add_argument("--robot_name", "-r", choices=("j2s6s300", "Reachy"))

    return parser.parse_args()


def main():
    """Main runner"""
    args = parse_args()

    end_eff_data, camera_data, rh_data, lh_data, joint_data = load_npzs(
        args.robot_name, args.participant_id, args.followup, args.gesture_num)

    demo_max = 5
    end_eff, camera, rh, lh, joint = segment_by_demo(end_eff_data, camera_data,
                                                     rh_data, lh_data,
                                                     joint_data, demo_max)

    plot_raw_data_subsampled(1,
                             end_eff[0],
                             camera[0],
                             rh[0],
                             lh[0],
                             joint[0],
                             title="Unaligned trajectories")

    end_eff_traj = get_evo_trajectory(end_eff[0])
    rh_traj = get_evo_trajectory(rh[0])

    # Just align without doing explicit timestamp matching.
    rh_traj_aligned = evo_align(rh_traj, end_eff_traj)

    plot_raw_data_subsampled(1,
                             end_eff[0],
                             camera[0],
                             convert_evo_to_np(rh_traj_aligned),
                             lh[0],
                             joint[0],
                             title="Spatially Aligned Trajectories")

    plt.show()


if __name__ == "__main__":
    main()
