from datetime import timedelta, datetime
import numpy as np
from numpy import ndarray
import tensorflow as tf
from tensorflow import Tensor
from tensorflow import keras
from keras.models import Sequential
from keras.layers import Dense, LSTM

audio_chunk_size = 1
input_shape = (None, audio_chunk_size)
training_shape = (None, None, audio_chunk_size)

def main():
    while True:
        if get_boolean_input('Do you wish to train a network?'):
            load_path = None
            if get_boolean_input('Do you wish to load it from disk?'):
                load_path = get_input_name()

            save_path_file_name = get_input_name(action='save the network')

            model = generate_model(load_path)
            train(model, save_path_file_name)
        elif get_boolean_input('Do you wish to expand an audio file?'):
            pass
        elif get_boolean_input('Do you wish to exit the program?'):
            quit(0)
            



def train(model: Sequential, save_name: str) -> None:
    train_until_time = get_boolean_input('Do you wish to train until a certain time?')
    training_until_time_time = get_input_date_time('train the network', ask=train_until_time)

    epoch_prompt = 'For how many epochs do you wish to train the network'
    if train_until_time:
        epoch_prompt += ' iteratively until the desired time'
    epoch_prompt += '?'
    epochs = get_input_int(epoch_prompt)
    X, Y = generate_training_data()
    trained_once = False
    while get_current_date_time() < training_until_time_time or not trained_once:
        model.fit(x=X, y=Y, batch_size=8, epochs=epochs, use_multiprocessing=True)
        save_model(model, save_name)
        trained_once = True

def get_boolean_input(prompt: str) -> bool:
    accepted_options = ['yes', 'no', 'y', 'n', '1', '0']
    positive_options = ['yes', 'y', '1']
    print(prompt)
    print('Accepted options: ' + accepted_options)
    answer = None
    while not answer in accepted_options:
        answer = input().lower()
    return answer in positive_options

def get_input_int(prompt: str) -> int:
    pass

def get_input_name(action: str = 'load the network') -> str:
    pass

def get_input_date_time(action: str, ask: bool = True) -> datetime | None:
    pass

def get_current_date_time() -> datetime:
    pass

def get_input_time_delta(action: str) -> timedelta:
    pass

def generate_model(path: str = None) -> Sequential:
    pass

def save_model(model: Sequential, name: str) -> None:
    pass

def generate_training_data() -> tuple[Tensor, Tensor]:
    pass

def extract_audio_from_directory(path: str) -> ndarray[Tensor]:
    pass

def extract_audio(file_path: str) -> Tensor:
    pass

def generate_track(input_file_path: str, output_file_path: str) -> None:
    pass

if __name__ == '__main__':
    main()