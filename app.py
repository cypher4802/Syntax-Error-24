import os
import sys
import cv2
import pyautogui
import mediapipe as mp
import numpy as np

# Set up MediaPipe pose model and drawing utilities
mp_drawing = mp.solutions.drawing_utils
mp_pose = mp.solutions.pose

# Baseline shoulder distance at 1 meter distance from the camera (calibrated value)
BASELINE_SHOULDER_DISTANCE = 0.25  # This value needs to be calibrated based on actual camera setup

class App:
    def __init__(self):
        self.start_game = False

    def run(self):
        try:
            # Initialize video capture
            cap = cv2.VideoCapture(0)

            # Curl counter variables for both arms
            left_counter = 0
            right_counter = 0
            left_stage = None
            right_stage = None

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

                # Loop for video feed
                while True:
                    ret, frame = cap.read()
                    if not ret:
                        break

                    # Flip and convert frame to RGB for processing
                    frame = cv2.flip(frame, 1)
                    frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                    frame_height, frame_width, _ = frame.shape

                    # Process the frame with MediaPipe pose model
                    results = pose.process(frame_rgb)

                    # Helper function to calculate the angle between joints
                    def calculate_angle(a, b, c):
                        a = np.array(a)  # First point
                        b = np.array(b)  # Second point (vertex)
                        c = np.array(c)  # Third point
                        radians = np.arctan2(c[1] - b[1], c[0] - b[0]) - np.arctan2(a[1] - b[1], a[0] - b[0])
                        angle = np.abs(radians * 180.0 / np.pi)
                        if angle > 180.0:
                            angle = 360 - angle
                        return angle

                    if results.pose_landmarks:
                        # Extract landmarks for left and right arms
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

                        # Calculate angles for left and right arms
                        left_angle = calculate_angle(left_shoulder, left_elbow, left_wrist)
                        right_angle = calculate_angle(right_shoulder, right_elbow, right_wrist)

                        # Warning for starting the game
                        if not self.start_game:
                            if left_angle > 25 or right_angle > 25:
                                warning_message = "Both elbows should be below 25 degrees to start!"
                                cv2.putText(frame, warning_message, (frame_width // 2 - 250, frame_height // 2),
                                            cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 2)  # Red color for warning
                            else:
                                self.start_game = True  # Start the game if both angles are below 25
                                cv2.putText(frame, "Game Started! Keep going!", (frame_width // 2 - 200, frame_height // 2),
                                            cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)  # Green color for confirmation

                        # Curl logic for left arm
                        if left_angle > 70:
                            left_stage = "down"
                        if left_angle < 30 and left_stage == "down":
                            left_stage = "up"
                            left_counter += 1

                        # Curl logic for right arm
                        if right_angle > 70:
                            right_stage = "down"
                        if right_angle < 25 and right_stage == "down":
                            right_stage = "up"
                            right_counter += 1

                        # Calculate the distance between left and right shoulders
                        shoulder_distance = np.linalg.norm(np.array(left_shoulder) - np.array(right_shoulder))

                        # Calculate the scaling factor to compare with the baseline shoulder distance
                        scale_factor = shoulder_distance / BASELINE_SHOULDER_DISTANCE

                        # Provide feedback if the user is too close or too far from the camera
                        if scale_factor > 1.1:
                            distance_feedback = "Too Close"
                        elif scale_factor < 1:
                            distance_feedback = "Too Far"
                        else:
                            distance_feedback = "Good Distance"

                        # Display the distance value at the center-bottom
                        distance_value_text = f"Distance: {shoulder_distance:.2f}m"
                        cv2.putText(frame, distance_value_text, (frame_width // 2 - 150, frame_height - 30),
                                    cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 2)  # White text

                        # Draw the distance feedback on the bottom-left corner
                        cv2.putText(frame, distance_feedback, (10, frame_height - 30),
                                    cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 2)  # Red color for warnings

                        # Draw the lines for jump and sit detection
                        two_thirds_height = int(2 * frame_height / 3) - 30  # Raise the line for jumping
                        two_fifths_height = int(2 * frame_height / 5) - 30   # Raise the line for sitting
                        cv2.line(frame, (0, two_thirds_height), (frame_width, two_thirds_height), (0, 255, 0), 2)  # Jump line
                        cv2.line(frame, (0, two_fifths_height), (frame_width, two_fifths_height), (255, 0, 0), 2)  # Sit line

                        # Detect vertical movement based on nose position
                        y_nose = landmarks[mp_pose.PoseLandmark.NOSE.value].y * frame_height
                        if y_nose < two_fifths_height:
                            vertical_status = "Jump"
                        elif y_nose > two_thirds_height:
                            vertical_status = "Sit"
                        else:
                            vertical_status = "Stand"

                        # Show the detected movement status
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

                        # Draw left and right rep counters
                        cv2.rectangle(frame, (0, 0), (225, 73), (245, 117, 16), -1)
                        cv2.putText(frame, "LEFT REPS", (15, 12), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 0), 1)
                        cv2.putText(frame, str(left_counter), (10, 60), cv2.FONT_HERSHEY_SIMPLEX, 2, (255, 255, 255), 2)

                        cv2.rectangle(frame, (frame_width - 225, 0), (frame_width, 73), (245, 117, 16), -1)
                        cv2.putText(frame, "RIGHT REPS", (frame_width - 210, 12), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 0), 1)
                        cv2.putText(frame, str(right_counter), (frame_width - 210, 60), cv2.FONT_HERSHEY_SIMPLEX, 2, (255, 255, 255), 2)

                    # Display the processed frame
                    cv2.imshow("Exercise Detection", frame)

                    # Break loop on 'q' key press
                    if cv2.waitKey(10) & 0xFF == ord('q'):
                        break

                # Release video capture
                cap.release()
                cv2.destroyAllWindows()
        except Exception as e:
            print(f"Error: {e}")

if __name__ == "__main__":
    app = App()
    app.run()
