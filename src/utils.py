import os
import sys

REPO_DIR_PATH = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), ".."))
sys.path.append(REPO_DIR_PATH)

CONFIG_FILE_PATH = os.path.join(REPO_DIR_PATH, "config.yaml")

from src.logger import logging
from src.exception import CustomException
import yaml


def load_config_file():
    """
    This function will load the yaml configuration file.
    input: 
        file_path: yaml file path
    output:
        config: configuration file
    """
    try:
        with open(CONFIG_FILE_PATH, "r") as file:
            config = yaml.safe_load(file)
        return config
    except Exception as e:
        logging.error(f"Error loading config file: {e}")
        raise CustomException("Error loading config file", sys)
    