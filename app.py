import os
import sys
import cv2
import pyautogui
import mediapipe as mp
import numpy as np
import math
import logging
from multiprocessing import shared_memory
import time

mp_drawing = mp.solutions.drawing_utils
mp_pose = mp.solutions.pose

# Baseline shoulder distance at 1 meter distance from the camera (calibrated value)
BASELINE_SHOULDER_DISTANCE = 0.25  # This value needs to be calibrated based on actual camera setup

SHARED_MEM_NAME = "ARFightingGameSharedMemory"
SHARED_MEM_SIZE = 256

class CustomException(Exception):
    pass

class SharedMemoryManager:
    def __init__(self):
        self.shared_memory = shared_memory.SharedMemory(name=SHARED_MEM_NAME, create=True, size=SHARED_MEM_SIZE)
        self.buffer = self.shared_memory.buf
        print(f"Shared memory created with name: {SHARED_MEM_NAME}")

    def write_data(self, data):
        self.buffer[0] = 0  # Set flag to 0 while writing
        data_bytes = data.encode('utf-8')
        data_bytes = data_bytes.ljust(SHARED_MEM_SIZE - 1)  # -1 for the flag byte
        self.buffer[1:] = data_bytes
        self.buffer[0] = 1  # Set flag to 1 when data is ready

    def close(self):
        self.shared_memory.close()
        self.shared_memory.unlink()

class App:
    def __init__(self):
        self.start_game = False
        self.lower_bound_line_threshold = 30  # Adjust as needed
        self.upper_bound_line_threshold = 30  # Adjust as needed
        self.shared_memory_manager = SharedMemoryManager()

    def run(self):
        try:
            cap = cv2.VideoCapture(0)

            # Curl counter variables for both arms
            left_counter = 0
            right_counter = 0
            left_stage = None
            right_stage = None
            
            # Kick counter variables
            left_kick_counter = 0
            right_kick_counter = 0

            # Initialize heights for knee tracking
            initial_left_knee_height = None
            initial_right_knee_height = None
            kick_threshold = 0.25  # Height change threshold for detecting kicks
            kick_type = ""  # Variable to store the type of kick

            with mp_pose.Pose(min_detection_confidence=0.5, min_tracking_confidence=0.5) as pose:
                if not cap.isOpened():
                    print("Error opening video capture device")
                    exit()

                cap.set(cv2.CAP_PROP_FRAME_WIDTH, 700)
                cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 1280)

                # Variables for game and movement
                initiate_game = False
                game_stop = False
                prev_vertical_status = None

                while True:
                    ret, frame = cap.read()
                    if not ret:
                        break

                    frame = cv2.flip(frame, 1)
                    frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                    frame_height, frame_width, _ = frame.shape

                    results = pose.process(frame_rgb)

                    if results.pose_landmarks:
                        landmarks = results.pose_landmarks.landmark
                        left_shoulder = [landmarks[mp_pose.PoseLandmark.LEFT_SHOULDER.value].x,
                                         landmarks[mp_pose.PoseLandmark.LEFT_SHOULDER.value].y]
                        right_shoulder = [landmarks[mp_pose.PoseLandmark.RIGHT_SHOULDER.value].x,
                                          landmarks[mp_pose.PoseLandmark.RIGHT_SHOULDER.value].y]
                        
                        left_elbow = [landmarks[mp_pose.PoseLandmark.LEFT_ELBOW.value].x,
                                      landmarks[mp_pose.PoseLandmark.LEFT_ELBOW.value].y]
                        left_wrist = [landmarks[mp_pose.PoseLandmark.LEFT_WRIST.value].x,
                                      landmarks[mp_pose.PoseLandmark.LEFT_WRIST.value].y]
                        right_elbow = [landmarks[mp_pose.PoseLandmark.RIGHT_ELBOW.value].x,
                                       landmarks[mp_pose.PoseLandmark.RIGHT_ELBOW.value].y]
                        right_wrist = [landmarks[mp_pose.PoseLandmark.RIGHT_WRIST.value].x,
                                       landmarks[mp_pose.PoseLandmark.RIGHT_WRIST.value].y]
                        left_knee = landmarks[mp_pose.PoseLandmark.LEFT_KNEE.value]
                        right_knee = landmarks[mp_pose.PoseLandmark.RIGHT_KNEE.value]
                        left_knee_height = left_knee.y
                        right_knee_height = right_knee.y

                        if initial_left_knee_height is None:
                            initial_left_knee_height = left_knee_height
                        if initial_right_knee_height is None:
                            initial_right_knee_height = right_knee_height
                        
                        left_height_change = initial_left_knee_height - left_knee_height
                        right_height_change = initial_right_knee_height - right_knee_height

                        # Detect left kick
                        if left_height_change > kick_threshold:
                            if kick_type != "Left Kick":
                                left_kick_counter += 1
                                self.shared_memory_manager.write_data("left_kick")
                            kick_type = "Left Kick"

                        # Detect right kick
                        elif right_height_change > kick_threshold:
                            if kick_type != "Right Kick":
                                right_kick_counter += 1
                                self.shared_memory_manager.write_data("right_kick")
                            kick_type = "Right Kick"

                        else:
                            kick_type = ""  # Reset if no kick detected
                        
                        left_angle = self.calculate_angle(left_shoulder, left_elbow, left_wrist)
                        right_angle = self.calculate_angle(right_shoulder, right_elbow, right_wrist)

                        if not self.start_game:
                            if left_angle > 25 or right_angle > 25:
                                warning_message = "Both elbows should be below 25 degrees to start!"
                                cv2.putText(frame, warning_message, (frame_width // 2 - 250, frame_height // 2),
                                            cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 2)
                            else:
                                self.start_game = True
                                cv2.putText(frame, "Game Started! Keep going!", (frame_width // 2 - 200, frame_height // 2),
                                            cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)

                        # Curl logic for left arm
                        if left_angle > 70:
                            left_stage = "down"
                        if left_angle < 30 and left_stage == "down":
                            left_stage = "up"
                            left_counter += 1
                            self.shared_memory_manager.write_data("left_punch")

                        # Curl logic for right arm
                        if right_angle > 70:
                            right_stage = "down"
                        if right_angle < 30 and right_stage == "down":
                            right_stage = "up"
                            right_counter += 1
                            self.shared_memory_manager.write_data("right_punch")

                        shoulder_distance = np.linalg.norm(np.array(left_shoulder) - np.array(right_shoulder))
                        scale_factor = shoulder_distance / BASELINE_SHOULDER_DISTANCE

                        if scale_factor > 1.1:
                            distance_feedback = "Too Close"
                        elif scale_factor < 1:
                            distance_feedback = "Too Far"
                        else:
                            distance_feedback = "Good Distance"

                        # Show distance feedback at the top center of the frame
                        distance_feedback_text = f"Distance: {distance_feedback}"
                        text_size_feedback = cv2.getTextSize(distance_feedback_text, cv2.FONT_HERSHEY_SIMPLEX, 1, 2)[0]
                        text_x_feedback = (frame_width - text_size_feedback[0]) // 2
                        cv2.putText(frame, distance_feedback_text, (text_x_feedback, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 2)

                        # Update thresholds for Jump and Sit detection
                        y_nose = landmarks[mp_pose.PoseLandmark.NOSE.value].y * frame_height

                        # Detect vertical movement
                        if y_nose < (frame_height / 3):  # Upper third of the frame
                            vertical_status = "Jump"
                            self.shared_memory_manager.write_data("jump")
                        elif y_nose > (2 * frame_height / 3):  # Lower third of the frame
                            vertical_status = "Sit"
                            self.shared_memory_manager.write_data("sit")
                        else:
                            vertical_status = "Stand"

                        # Draw the vertical status at the bottom center of the frame
                        text_size = cv2.getTextSize(vertical_status, cv2.FONT_HERSHEY_PLAIN, 2, 3)[0]
                        text_x = (frame_width - text_size[0]) // 2
                        text_y = frame_height - 10
                        cv2.putText(frame, vertical_status, (text_x, text_y), cv2.FONT_HERSHEY_PLAIN, 2, (255, 255, 255), 3)

                        # Draw rep counters
                        cv2.rectangle(frame, (0, 0), (225, 73), (245, 117, 16), -1)
                        cv2.putText(frame, "LEFT REPS", (15, 12), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 0), 1)
                        cv2.putText(frame, str(left_counter), (10, 60), cv2.FONT_HERSHEY_SIMPLEX, 2, (255, 255, 255), 2)

                        cv2.rectangle(frame, (frame_width - 225, 0), (frame_width, 73), (245, 117, 16), -1)
                        cv2.putText(frame, "RIGHT REPS", (frame_width - 210, 12), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 0), 1)
                        cv2.putText(frame, str(right_counter), (frame_width - 220, 60), cv2.FONT_HERSHEY_SIMPLEX, 2, (255, 255, 255), 2)

                        # Draw kick counters
                        cv2.rectangle(frame, (0, frame_height - 73), (225, frame_height), (245, 117, 16), -1)
                        cv2.putText(frame, "LEFT KICKS", (15, frame_height - 62), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 0), 1)
                        cv2.putText(frame, str(left_kick_counter), (10, frame_height - 10), cv2.FONT_HERSHEY_SIMPLEX, 2, (255, 255, 255), 2)

                        cv2.rectangle(frame, (frame_width - 225, frame_height - 73), (frame_width, frame_height), (245, 117, 16), -1)
                        cv2.putText(frame, "RIGHT KICKS", (frame_width - 210, frame_height - 62), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 0), 1)
                        cv2.putText(frame, str(right_kick_counter), (frame_width - 220, frame_height - 10), cv2.FONT_HERSHEY_SIMPLEX, 2, (255, 255, 255), 2)

                    # Show the frame
                    cv2.imshow('MediaPipe Pose', frame)

                    # Exit on 'q' key
                    if cv2.waitKey(10) & 0xFF == ord('q'):
                        break

                    time.sleep(0.01)  # Small delay to prevent busy-waiting

                cap.release()
                cv2.destroyAllWindows()

        except CustomException as e:
            logging.error("Custom exception occurred: %s", e)
            print("An error occurred in the application.")
        except Exception as e:
            logging.error("An unexpected error occurred: %s", e)
            print("An unexpected error occurred.")
        finally:
            # Clean up shared memory
            self.shared_memory_manager.close()

    def draw_horizontal_and_vertical_lines(self, frame, y_position):
        frame_height, frame_width, _ = frame.shape
        cv2.line(frame, (0, y_position), (frame_width, y_position), (255, 0, 0), 2)  # Horizontal line
        cv2.line(frame, (frame_width // 2, 0), (frame_width // 2, frame_height), (255, 0, 0), 2)  # Vertical line

    def calculate_angle(self, a, b, c):
        a = np.array(a)  # First point
        b = np.array(b)  # Second point (vertex)
        c = np.array(c)  # Third point
        radians = np.arctan2(c[1] - b[1], c[0] - b[0]) - np.arctan2(a[1] - b[1], a[0] - b[0])
        angle = np.abs(radians * 180.0 / np.pi)
        if angle > 180.0:
            angle = 360 - angle
        return angle

if __name__ == "__main__":
    app = App()
    app.run()