from datetime import datetime
import logging
import os

# Get the current time
current_time = str(datetime.now().strftime("%Y-%m-%d-%I-%M-%S"))
# Define the logs path
log_path = os.getcwd() + "/logs/" + current_time + ".log"
# Create a logs directory if it doesn't exist
os.makedirs(os.path.dirname(log_path), exist_ok=True)

# Create a custom logger
logging.basicConfig(level=logging.INFO,
                    filename=log_path,
                    filemode='a',
                    format='time: ' + current_time + '\n'
                    'name: ' + '%(name)s' + '\n'
                    'level: ' + '%(levelname)s' + '\n'
                    'path: ' + '%(pathname)s' + '\n'
                    'line number' + '%(lineno)d' + '\n'
                    'message: ' + '%(message)s' + '\n\n')
