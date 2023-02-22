import os
from random import randint
from datetime import timedelta, datetime
import numpy as np
from numpy import ndarray
import tensorflow as tf
from tensorflow import Tensor
from keras.models import Sequential
from keras.layers import Dense, LSTM, Dropout

audio_unit_byte_count = 32 // 8
input_shape = (None, audio_unit_byte_count)
training_shape = (None, None, audio_unit_byte_count)

def main():
    while True:
        if get_boolean_input('Do you wish to train a network?'):
            load_path = None
            if get_boolean_input('Do you wish to load it from disk?'):
                load_path = get_input_name()

            save_path_file_name = get_input_name(action='save the network')

            model = generate_model(load_path)
            train(model, save_path_file_name)
            delete_model(model)
        elif get_boolean_input('Do you wish to expand an audio file?'):
            pass
        elif get_boolean_input('Do you wish to exit the program?'):
            quit(0)

def train(model: Sequential, save_name: str) -> None:
    train_until_time = get_boolean_input('Do you wish to train until a certain time?')
    training_until_time_time = get_input_date_time('training the network', ask=train_until_time)

    epoch_prompt = 'For how many epochs do you wish to train the network'
    if train_until_time:
        epoch_prompt += ' iteratively until the desired time'
    epoch_prompt += '?'
    epochs = get_input_int(epoch_prompt)
    trained_once = False
    get_data_each_fit = get_boolean_input('Do you want to get new data each time the model fits for the selected epochs?')
    X, Y = (None, None)
    tracks_to_load = None
    while get_current_date_time() < training_until_time_time or not trained_once:
        if not trained_once or get_data_each_fit:
            X, Y, tracks_to_load = generate_training_data(tracks_to_load=tracks_to_load)
        model.fit(x=X, y=Y, batch_size=8, epochs=epochs, use_multiprocessing=True)
        save_model(model, save_name)
        trained_once = True

def get_boolean_input(prompt: str) -> bool:
    accepted_options = ['yes', 'no', 'y', 'n', '1', '0']
    positive_options = ['yes', 'y', '1']
    print(prompt)
    print('Accepted options: ' + str(accepted_options))
    answer = None
    while not answer in accepted_options:
        answer = input().lower()

    return answer in positive_options

def get_input_int(prompt: str, return_abs: bool = True) -> int:
    while True:
        print(prompt)
        print('Accepted options are integers. If the integer is negative, it will be converted to positive')
        try:
            answer = int(input())
            if return_abs:
                answer = abs(answer)
            return answer
        except:
            pass

def get_input_name(action: str = 'load the network') -> str:
    prompt = f'Enter a file name to {action}.'
    print(prompt)
    return remove_illegal_characters(input())
    
def remove_illegal_characters(file_name: str) -> str:
    return file_name.replace('\\', '').replace('>', '').replace('<', '').replace('con', '').replace('"', '').replace('?', '').replace('|', '').replace('*', '').replace(' ', '')

def get_input_date_time(action: str, ask: bool = True) -> datetime | None:
    if not ask:
        return None
    current_date_time = get_current_date_time()
    time_delta = get_input_time_delta(action)
    return current_date_time + time_delta

def get_current_date_time() -> datetime:
     return datetime.now()

def get_input_time_delta(action: str) -> timedelta:
    hours = get_input_int(f'Select how many hours of {action}. (Later there will be a question of how many days)')
    days = get_input_int(f'Select how many days of {action}.')
    output = timedelta(days=days, hours=hours)
    return output

def generate_model(path: str = None) -> Sequential:
    model = Sequential()
    model.add(LSTM(audio_unit_byte_count, return_sequences=True, input_shape=input_shape))
    model.add(LSTM(150, return_sequences=True))
    model.add(LSTM(100, return_sequences=True))
    model.add(LSTM(75))
    model.add(Dense(35))
    model.add(Dense(audio_unit_byte_count))

    if path:
        model.load_weights(path)

    model.compile(optimizer='nadam')
    return model

def save_model(model: Sequential, name: str) -> None:
    model.save_weights('./' + name + '.hdf5')

def generate_training_data(tracks_to_load: int | None = None) -> tuple[list[list[list[int]]], Tensor, int]:
    tracks, tracks_to_load = extract_audio_from_directory('./Converted/', tracks_to_load=tracks_to_load)
    X = []
    Y = []
    for i, track in enumerate(tracks):
        X.append([])
        Y.append([])
        print(f'Appending data of track {i + 1} of {len(tracks)} tracks to training data')
        j_delta_counter = 0
        for j in range(0, len(track) - audio_unit_byte_count * 2, audio_unit_byte_count):
            X[i].append([])
            Y[i].append([])
            for k in range(audio_unit_byte_count):
                current_X = track[j + k]
                current_Y = track[j + k + audio_unit_byte_count]
                X[i][j_delta_counter].append(current_X)
                Y[i][j_delta_counter].append(current_Y)

            j_delta_counter += 1

    return (X, Y, tracks_to_load)


def extract_audio_from_directory(path: str, tracks_to_load: int | None = None) -> tuple[list[bytearray], int]:
    if not tracks_to_load:
        tracks_to_load = get_input_int('How many tracks do you want to load in? (I recommend 15 maximum number of loaded tracks with 16GB of RAM)')
    tracks_paths = []
    for folder, _, files in os.walk(path):
        files = files
        filtered = [f for f in files if f.__contains__('.wav')]
        for file in filtered:
            file_path = os.path.join(folder, file)
            tracks_paths.append(file_path)

    output = []
    for i in range(tracks_to_load):
        track_i = randint(0, len(tracks_paths) - 1)
        output.append(extract_audio(tracks_paths[track_i]))
        tracks_paths.remove(tracks_paths[track_i])
        print(f'Extracted audio of {i + 1} tracks of {tracks_to_load}')
    return (output, tracks_to_load)

# read as binary and transform to decimal
def extract_audio(file_path: str) -> bytearray:
    file = open(file_path, mode='rb')
    audio_data = bytearray(file.read())
    file.close()
    return audio_data

def generate_track(model: Sequential, input_file_path: str, output_file_path: str) -> None:
    pass

def delete_model(model: Sequential):
    del model

if __name__ == '__main__':
    main()