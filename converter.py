import os

script_path = os.path.dirname(os.path.realpath(__file__)) + '\\'
ffmpeg_path = f'{script_path}ffmpeg.exe'
from_folder = f'{script_path}Songs/'
to_folder = f'{script_path}Converted/'
to_format = '.wav'

for _, folder, files in os.walk(from_folder):
    for file in files:
        file_name = file[:-4]
        command = f'{ffmpeg_path} -i {from_folder}{file} {to_folder}{file_name}{to_format}'
        result = os.system(command)
        print(result)