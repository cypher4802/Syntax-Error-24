import os
import sys
REPO_DIR_PATH = os.path.normpath(os.path.join(
    os.path.dirname(os.path.abspath(__file__)), ".."))
sys.path.append(REPO_DIR_PATH)

import mediapipe as mp
from src.utils import load_config_file
from src.exception import CustomException
from src.logger import logging
import cv2
import math


class ImagePreprocessing:
    """
    A class for performing image preprocessing tasks related to pose detection and movement analysis.
    """
    def __init__(self):
        # Logging the creation of ImagePreprocessing object
        logging.info("ImagePreprocessing object created")
        # Loading configuration file
        self.config = load_config_file()
        # Assigning minimum detection confidence from configuration
        self.min_detection_confidence = self.config["min_detection_confidence"]
        # Assigning minimum tracking confidence from configuration
        self.min_tracking_confidence = self.config["min_tracking_confidence"]
        # Assigning lower bound line threshold from configuration
        self.lower_bound_line_threshold = self.config["lower_bound_line_threshold"]
        # Assigning upper bound line threshold from configuration
        self.upper_bound_line_threshold = self.config["upper_bound_line_threshold"]
        # Assigning distance threshold for hands joined from configuration
        self.distance_threshold_hands_joined = self.config["distance_threshold_hands_joined"]
        # Initializing the Pose model with specified confidences
        self.pose_model = mp.solutions.pose.Pose(
            min_detection_confidence=self.min_detection_confidence, min_tracking_confidence=self.min_tracking_confidence)

    def get_initial_shoulder_coordinates(self, frame, results):
        """
        This function will return the initial shoulder coordinates.
        Args: 
            frame: image frame(bgr format)
        Returns:
            shoulder_coordinates : dictionary containing the right and left shoulder coordinates
        """
        try:
            # Check if pose landmarks are detected
            if results.pose_landmarks == None:
                # Log message if no pose landmarks detected
                logging.info(
                    "No pose detected for getting the initial shoulder coordinates")
                return None
            # Extract pose landmarks
            landmarks = results.pose_landmarks.landmark
            # Get frame dimensions
            frame_height, frame_width, _ = frame.shape
            # Calculate right shoulder coordinates
            right_shoulder = [int(landmarks[mp.solutions.pose.PoseLandmark.RIGHT_SHOULDER.value].x * frame_width),
                              int(landmarks[mp.solutions.pose.PoseLandmark.RIGHT_SHOULDER.value].y * frame_height)]
            # Calculate left shoulder coordinates
            left_shoulder = [int(landmarks[mp.solutions.pose.PoseLandmark.LEFT_SHOULDER.value].x * frame_width),
                             int(landmarks[mp.solutions.pose.PoseLandmark.LEFT_SHOULDER.value].y * frame_height)]
            # Store shoulder coordinates in dictionary
            coordinates = {"right_shoulder": right_shoulder,
                           "left_shoulder": left_shoulder}
            return coordinates
        except Exception as e:
            # Log error if encountered during shoulder coordinates extraction
            logging.error(f"Error getting initial shoulder coordinates: {e}")
            raise CustomException(
                "Error getting initial shoulder coordinates", sys)

    def draw_horizontal_and_vertical_lines(self, frame, center_y):
        """
        This function will draw horizontal and vertical lines on the frame.
        Args: 
            frame: image frame(bgr format)
            center_y: y coordinate of the center
        Returns:
            frame(bgr format): image frame with horizontal and vertical lines drawn
        """
        try:
            # Get frame dimensions
            frame_height, frame_width, _ = frame.shape
            # Set line thickness
            thickness = 2

            # Draw vertical line
            color = (255, 255, 255)  # White color
            start_point = (frame_width // 2, 0)
            end_point = (frame_width // 2, frame_height)
            cv2.line(frame, start_point, end_point, color, thickness)

            # Draw lower horizontal line
            color = (0, 0, 255)  # Red color
            lower_bound = center_y + self.lower_bound_line_threshold
            start_point = (0, lower_bound)
            end_point = (frame_width, lower_bound)
            cv2.line(frame, start_point, end_point, color, thickness)

            # Draw upper horizontal line
            color = (0, 0, 255)  # Red color
            upper_bound = center_y - self.upper_bound_line_threshold
            start_point = (0, upper_bound)
            end_point = (frame_width, upper_bound)
            cv2.line(frame, start_point, end_point, color, thickness)
            return frame
        except Exception as e:
            # Log error if encountered during line drawing
            logging.error(f"Error drawing lines: {e}")
            raise CustomException(
                "Error drawing lines", sys)

    def calculate_distance(self, landmark1, landmark2):
        """
        Calculate the Euclidean distance between two landmarks.
        Args:
            landmark1 (list): List containing the coordinates of the first landmark [x1, y1].
            landmark2 (list): List containing the coordinates of the second landmark [x2, y2].

        Returns:
            float: The Euclidean distance between the two landmarks.
        """
        try:
            # Extract x and y coordinates from landmark lists
            x1, y1 = landmark1
            x2, y2 = landmark2

            # Calculate Euclidean distance
            distance = math.sqrt((x2 - x1)**2 + (y2 - y1)**2)

            return distance
        except Exception as e:
            # Log error if encountered during distance calculation
            logging.error(f"Error calculating distance: {e}")
            raise CustomException(
                "Error calculating distance", sys)

    def check_hands_joined(self, frame, results):
        """
        This function will check if the hands are joined or not.
        Args: 
            frame: image frame(bgr format)
        Returns:
            status: string indicating if hands are joined or not("Hands Joined" or "Hands Not Joined")
        """
        try:
            # Extract frame dimensions
            frame_height, frame_width, _ = frame.shape
            # Check if pose landmarks are detected
            if results.pose_landmarks is None:
                # Log message if no pose landmarks detected
                logging.info("No pose detected for checking hands joined")
                return None
            # Extract pose landmarks
            landmarks = results.pose_landmarks.landmark
            # Get coordinates of left wrist
            left_wrist = [int(landmarks[mp.solutions.pose.PoseLandmark.LEFT_WRIST].x * frame_width),
                          int(landmarks[mp.solutions.pose.PoseLandmark.LEFT_WRIST].y * frame_height)]
            # Get coordinates of right wrist
            right_wrist = [int(landmarks[mp.solutions.pose.PoseLandmark.RIGHT_WRIST].x * frame_width),
                           int(landmarks[mp.solutions.pose.PoseLandmark.RIGHT_WRIST].y * frame_height)]
            # Calculate distance between left and right wrists
            distance = self.calculate_distance(left_wrist, right_wrist)
            # Check if distance is less than threshold for hands joined
            if distance < self.distance_threshold_hands_joined:
                return "Hands Joined"
            else:
                return "Hands Not Joined"
        except Exception as e:
            # Log error if encountered during hands joined check
            logging.error(f"Error checking hands joined: {e}")
            raise CustomException(
                "Error checking hands joined", sys)

    def check_left_right(self, frame, results):
        """
        This function will check if the person is moving left, right or center.
        Args: 
            frame: image frame(bgr format)
        Returns:
            position: string indicating the position of the person(left, right, center)
        """
        # Extract frame width
        _, frame_width, _ = frame.shape
        # Check if pose landmarks are detected
        if results.pose_landmarks is None:
            # Log message if no pose landmarks detected
            logging.info("No pose detected for checking horizontal movements")
            return None
        # Extract pose landmarks
        landmarks = results.pose_landmarks.landmark
        # Get x-coordinate of left shoulder
        left_shoulder_x = int(
            landmarks[mp.solutions.pose.PoseLandmark.LEFT_SHOULDER].x * frame_width)
        # Get x-coordinate of right shoulder
        right_shoulder_x = int(
            landmarks[mp.solutions.pose.PoseLandmark.RIGHT_SHOULDER].x * frame_width)
        # Determine the position based on the x-coordinates of shoulders
        if (right_shoulder_x < frame_width//2) and (left_shoulder_x > frame_width//2):
            return "Center"
        elif (right_shoulder_x > frame_width//2) and (left_shoulder_x > frame_width//2):
            return "Right"
        elif (right_shoulder_x < frame_width//2) and (left_shoulder_x < frame_width//2):
            return "Left"

    def check_up_down(self, frame, center_y, results):
        """
        This function will check if the person is moving up, down or standing.
        Args:
            frame: image frame(bgr format)
            center_y: y coordinate of the center
        Returns:
            movement: string indicating the movement of the person(up, down, standing)
        """
        # Extract frame height
        frame_height, _, _ = frame.shape
        # Check if pose landmarks are detected
        if results.pose_landmarks is None:
            # Log message if no pose landmarks detected
            logging.info("No pose detected for checking vertical movements")
            return None
        # Extract pose landmarks
        landmarks = results.pose_landmarks.landmark
        # Calculate y-coordinate of left shoulder
        left_shoulder_y = int(
            landmarks[mp.solutions.pose.PoseLandmark.LEFT_SHOULDER].y * frame_height)
        # Calculate y-coordinate of right shoulder
        right_shoulder_y = int(
            landmarks[mp.solutions.pose.PoseLandmark.RIGHT_SHOULDER].y * frame_height)
        # Calculate y-coordinate of midpoint between shoulders
        shoulder_mid_y = (left_shoulder_y + right_shoulder_y) // 2
        # Calculate upper and lower bounds for vertical movement
        lower_bound = center_y + self.lower_bound_line_threshold
        upper_bound = center_y - self.upper_bound_line_threshold
        # Determine if movement is up, down, or within bounds
        if shoulder_mid_y < upper_bound:
            return "Up"
        elif shoulder_mid_y > lower_bound:
            return "Down"
        else:
            return "Standing"