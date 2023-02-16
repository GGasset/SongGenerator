import os
from time import sleep
import pandas as pd
import sys

def main():
    if len(sys.argv) < 2:
        print('Usage: python setup.py songs_csv_input_path songs_csv_output_path')
        sleep(1)
        quit(1)
    songs_csv_from_path = sys.argv[0]
    songs_csv_to_path = sys.argv[1]

    if os.path.isfile(songs_csv_to_path):
        print('No need for setup')
        sleep(1)
        return

    if not os.path.isfile(songs_csv_from_path):
        print('Original Songs.csv file missing, you need to clone submodules too.')
        sleep(1)
        quit(1)

    try:
        songs_csv = pd.read_csv(songs_csv_from_path)
        songs_csv.to_csv(songs_csv_to_path)
    except Exception:
        print('Error while moving data.')
        sleep(1)
        exit(1)

if __name__ == '__main__':
    main()