import os
import sys
import cv2
import pyautogui
import mediapipe as mp
import numpy as np

mp_drawing = mp.solutions.drawing_utils
mp_pose = mp.solutions.pose

# Baseline shoulder distance at 1 meter distance from the camera (calibrated value)
BASELINE_SHOULDER_DISTANCE = 0.25  # This value needs to be calibrated based on actual camera setup

class App:
    def __init__(self):
        self.start_game = False

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

                    def calculate_angle(a, b, c):
                        a = np.array(a)
                        b = np.array(b)
                        c = np.array(c)
                        radians = np.arctan2(c[1] - b[1], c[0] - b[0]) - np.arctan2(a[1] - b[1], a[0] - b[0])
                        angle = np.abs(radians * 180.0 / np.pi)
                        if angle > 180.0:
                            angle = 360 - angle
                        return angle

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
                        if left_height_change > 0.2:
                            if kick_type != "Left Kick":
                                left_kick_counter += 1
                            kick_type = "Left Kick"

                        # Detect right kick
                        elif right_height_change > kick_threshold:
                            if kick_type != "Right Kick":
                                right_kick_counter += 1
                            kick_type = "Right Kick"

                        else:
                            kick_type = ""  # Reset if no kick detected
                        
                        left_angle = calculate_angle(left_shoulder, left_elbow, left_wrist)
                        right_angle = calculate_angle(right_shoulder, right_elbow, right_wrist)

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

                        # Curl logic for right arm
                        if right_angle > 70:
                            right_stage = "down"
                        if right_angle < 30 and right_stage == "down":
                            right_stage = "up"
                            right_counter += 1

                        shoulder_distance = np.linalg.norm(np.array(left_shoulder) - np.array(right_shoulder))
                        scale_factor = shoulder_distance / BASELINE_SHOULDER_DISTANCE

                        if scale_factor > 1.1:
                            distance_feedback = "Too Close"
                        elif scale_factor < 1:
                            distance_feedback = "Too Far"
                        else:
                            distance_feedback = "Good Distance"

                        distance_value_text = f"Distance: {shoulder_distance:.2f}m"
                        cv2.putText(frame, distance_value_text, (frame_width // 2 - 150, frame_height - 30),
                                    cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 2)

                        cv2.putText(frame, distance_feedback, (10, frame_height - 30),
                                    cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 2)

                        # Detect vertical movement
                        y_nose = landmarks[mp_pose.PoseLandmark.NOSE.value].y * frame_height

                        # Update thresholds for Jump and Sit detection
                        if y_nose < (frame_height / 3):  # Upper half of the frame
                            vertical_status = "Sit"
                        elif y_nose > (3 * frame_height / 5):  # Lower quarter of the frame
                            vertical_status = "Jump"
                        else:
                            vertical_status = "Stand"

                        cv2.putText(frame, vertical_status, (frame_width - 150, frame_height - 10),
                                    cv2.FONT_HERSHEY_PLAIN, 2, (255, 255, 255), 3)

                        # Trigger actions based on movement status change
                        if vertical_status != prev_vertical_status:
                            if vertical_status == "Jump":
                                print("Move Up")
                                if self.start_game and not game_stop:
                                    pyautogui.press("up")
                            elif vertical_status == "Sit":
                                print("Move Down")
                                if self.start_game and not game_stop:
                                    pyautogui.press("down")
                            prev_vertical_status = vertical_status

                        # Draw horizontal lines for Jump and Sit thresholds
                        cv2.line(frame, (0, frame_height // 2), (frame_width, frame_height // 2), (0, 0, 255), 2)  # Line for Sit
                        cv2.line(frame, (0, 3 * frame_height // 4), (frame_width, 3 * frame_height // 4), (0, 255, 0), 2)  # Line for Jump

                        # Draw left and right rep counters
                        cv2.rectangle(frame, (0, 0), (225, 73), (245, 117, 16), -1)
                        cv2.putText(frame, "LEFT REPS", (15, 12), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 0), 1)
                        cv2.putText(frame, str(left_counter), (10, 60), cv2.FONT_HERSHEY_SIMPLEX, 2, (255, 255, 255), 2)

                        cv2.rectangle(frame, (frame_width - 225, 0), (frame_width, 73), (245, 117, 16), -1)
                        cv2.putText(frame, "RIGHT REPS", (frame_width - 215, 12), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 0), 1)
                        cv2.putText(frame, str(right_counter), (frame_width - 215, 60), cv2.FONT_HERSHEY_SIMPLEX, 2, (255, 255, 255), 2)

                        # Draw kick counters below the rep counters
                        cv2.rectangle(frame, (0, 73), (225, 100), (245, 117, 16), -1)  # Adjust height as needed
                        cv2.putText(frame, "LEFT KICKS", (15, 90), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 0), 1)
                        cv2.putText(frame, str(left_kick_counter), (10, 130), cv2.FONT_HERSHEY_SIMPLEX, 1.5, (255, 255, 255), 2)

                        cv2.rectangle(frame, (frame_width - 225, 73), (frame_width, 100), (245, 117, 16), -1)  # Adjust height as needed
                        cv2.putText(frame, "RIGHT KICKS", (frame_width - 215, 90), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 0), 1)
                        cv2.putText(frame, str(right_kick_counter), (frame_width - 215, 130), cv2.FONT_HERSHEY_SIMPLEX, 1.5, (255, 255, 255), 2)

                        mp_drawing.draw_landmarks(frame, results.pose_landmarks, mp_pose.POSE_CONNECTIONS)

                    cv2.imshow("Push Up Counter", frame)
                    if cv2.waitKey(10) & 0xFF == 27:  # Press 'ESC' to quit
                        break

            cap.release()
            cv2.destroyAllWindows()
        except Exception as e:
            print(f"Error: {str(e)}")
            sys.exit(1)

if __name__ == "__main__":
    app = App()
    app.run()
