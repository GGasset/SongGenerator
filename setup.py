import os
import pandas as pd
import sys

def main():
    songs_csv_from_path = sys.argv[0]
    songs_csv_to_path = sys.argv[1]
    
    if os.path.isfile(songs_csv_to_path):
        print('No need for setup')
        return

    if not os.path.isfile(songs_csv_from_path):
        print('Original Songs.csv file missing')
        quit(1)

    songs_csv = pd.read_csv(songs_csv_from_path)
    songs_csv.to_csv(songs_csv_to_path)

if __name__ == '__main__':
    main()