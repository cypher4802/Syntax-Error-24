import os
import sys
import cv2
import pyautogui
import mediapipe as mp
import numpy as np
import logging
import time

mp_drawing = mp.solutions.drawing_utils
mp_pose = mp.solutions.pose

# Baseline shoulder distance at 1 meter distance from the camera (calibrated value)
BASELINE_SHOULDER_DISTANCE = 0.25  # This value needs to be calibrated based on actual camera setup

def calculate_angle(a, b, c):
    a = np.array(a)
    b = np.array(b)
    c = np.array(c)
    radians = np.arctan2(c[1] - b[1], c[0] - b[0]) - np.arctan2(a[1] - b[1], a[0] - b[0])
    angle = np.abs(radians * 180.0 / np.pi)
    if angle > 180.0:
        angle = 360 - angle
    return angle

def main():
    try:
        cap = cv2.VideoCapture(0)
        start_game = False
        
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
        vertical_status = "Stand"  # Default vertical status
        last_action = ""  # Track last action to debounce

        with mp_pose.Pose(min_detection_confidence=0.5, min_tracking_confidence=0.5) as pose:
            if not cap.isOpened():
                print("Error opening video capture device")
                exit()

            cap.set(cv2.CAP_PROP_FRAME_WIDTH, 700)
            cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 1280)

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
                        if last_action != "Left Kick":
                            pyautogui.keyDown('j')  # Simulate left kick
                            time.sleep(0.1)  # Duration of the kick
                            pyautogui.keyUp('j')
                            last_action = "Left Kick"
                        left_kick_counter += 1

                    # Detect right kick
                    elif right_height_change > kick_threshold:
                        if last_action != "Right Kick":
                            pyautogui.keyDown('k')  # Simulate right kick
                            time.sleep(0.1)  # Duration of the kick
                            pyautogui.keyUp('k')
                            last_action = "Right Kick"
                        right_kick_counter += 1

                    else:
                        last_action = ""  # Reset if no kick detected

                    left_angle = calculate_angle(left_shoulder, left_elbow, left_wrist)
                    right_angle = calculate_angle(right_shoulder, right_elbow, right_wrist)

                    # Game start logic
                    if not start_game:
                        if left_angle > 25 or right_angle > 25:
                            warning_message = "Both elbows should be below 25 degrees to start!"
                            cv2.putText(frame, warning_message, (frame_width // 2 - 250, frame_height // 2),
                                        cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 2)
                        else:
                            start_game = True
                            cv2.putText(frame, "Game Started! Keep going!", (frame_width // 2 - 200, frame_height // 2),
                                        cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)

                    # Curl logic for left arm
                    if left_angle > 70:
                        left_stage = "down"
                    if left_angle < 30 and left_stage == "down":
                        left_stage = "up"
                        if last_action != "Left Punch":
                            pyautogui.keyDown('u')  # Simulate left punch
                            time.sleep(0.1)  # Duration of the punch
                            pyautogui.keyUp('u')
                            last_action = "Left Punch"
                        left_counter += 1

                    # Curl logic for right arm
                    if right_angle > 70:
                        right_stage = "down"
                    if right_angle < 30 and right_stage == "down":
                        right_stage = "up"
                        if last_action != "Right Punch":
                            pyautogui.keyDown('i')  # Simulate right punch
                            time.sleep(0.1)  # Duration of the punch
                            pyautogui.keyUp('i')
                            last_action = "Right Punch"
                        right_counter += 1

                    # Detect vertical movement
                    y_nose = landmarks[mp_pose.PoseLandmark.NOSE.value].y * frame_height

                    if y_nose < (frame_height / 3):  # Upper third of the frame
                        vertical_status = "Jump"
                        if last_action != "Jump":
                            pyautogui.keyDown('w')  # Simulate jump
                            time.sleep(0.1)  # Duration of the jump
                            pyautogui.keyUp('w')
                            last_action = "Jump"
                    elif y_nose > (2 * frame_height / 3):  # Lower third of the frame
                        vertical_status = "Sit"
                        if last_action != "Sit":
                            pyautogui.keyDown('s')  # Simulate sit
                            time.sleep(0.1)  # Duration of the sit
                            pyautogui.keyUp('s')
                            last_action = "Sit"
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
                    cv2.putText(frame, str(right_counter), (frame_width - 225 + 10, 60), cv2.FONT_HERSHEY_SIMPLEX, 2, (255, 255, 255), 2)

                    # Display the frame with landmarks
                    mp_drawing.draw_landmarks(frame, results.pose_landmarks, mp_pose.POSE_CONNECTIONS)

                cv2.imshow('MediaPipe Pose', frame)
                if cv2.waitKey(10) & 0xFF == 27:  # Press 'ESC' to exit
                    break

        cap.release()
        cv2.destroyAllWindows()

    except Exception as e:
        logging.error(f"Error in main loop: {str(e)}")
        os._exit(1)

if __name__ == "__main__":
    main()
