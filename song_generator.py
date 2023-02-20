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
    pass

def train():
    pass

def get_current_date_time() -> datetime:
    pass

def get_input_time_delta(prompt: str) -> timedelta:
    pass

def get_input_date_time(prompt: str) -> datetime:
    pass

def generate_model() -> Sequential:
    pass

def save_model(model: Sequential):
    pass

def extract_audio_from_directory(path: str) -> ndarray[Tensor]:
    pass

def extract_audio(file_path: str) -> Tensor:
    pass

if __name__ == '__main__':
    main()