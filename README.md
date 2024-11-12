# Lords_of_motion: An online real-time motion based game.
Remember the X men character which can control all the things by his bare hands the code that we proposed here does the same thing for the Single player fighting game and gives you the Real life experience of how it will feel like to have the motions implemented and is very user friendly.

# Key Implementations
 * The code focuses on the accuracy of detecting the motion on the basis of hand and legs movement and provide the correct distance and posture on where to start in the first view.
 * The server is then rendering the commands to the player that has started the game and then it gets the action according to that.
 * The game is given a fixed time for the fight and is based on how much damage is caused to the opponent.

# Pose Based Motion Detection
  We here are using the Mediapipe library for pose detection and getting the angles for the estimation of motion, for the punch the angles of the joints are responsible for getting the punch, for getting the kick we are using the change in length between the hip bone and the knee to get the kick left and right, for jump we are using the refrence lines in the frame of the camera above which there is jump then stand then sit. 
  
  All this starts with getting to the good distance region as will be there on the computer camera popup then you have to get to the recommended position then the game will start and you can get the functionality you want.

  ![Pose_detection](https://github.com/cypher4802/Syntax-Error-24/blob/main/Images/Screenshot%202024-10-20%20104158.png)
  ![Pose_detection](https://github.com/cypher4802/Syntax-Error-24/blob/main/Images/Screenshot%202024-10-20%20104221.png)

# Recording the actions
When you want to record the moves then you can just press the key R and then the commands will get recorded which will be replayed when you want after a buffer time.
![Replay_actions](https://github.com/cypher4802/Syntax-Error-24/blob/main/Images/Screenshot%202024-10-20%20104212.png)


# MultiPurpose API
The api that we have produced here is not only used in our game but according to the commands in the keyboard for other games can work for them as well so we have in a way created a universal API.

# To Start the game
* Clone the "merged" branch of the repo
* Open the unity project Tekken-main on unity version 2021.3.26f1
* Run the app.py file
* Run the server.py file
* Start the game and enjoy



# Future goals
Our next steps involve significantly advancing the server architecture by integrating WebSockets for real-time communication between various components of the system. This will enable smoother, low-latency interactions between the client and server, crucial for the gameplay experience.

We also plan to implement a shared memory system to streamline data transfer between the AI model and the game engine. This integration will not only make the replay feature more efficient but also allow the AI to analyze gameplay in real time, providing actionable insights into player performance, decision-making, and strategies.

To elevate the user experience, we aim to incorporate AI-driven animations, making character movements and in-game responses more dynamic and lifelike. By leveraging AI for enhanced animations and smarter replays, players can enjoy a more immersive and visually engaging gaming experience. Additionally, these improvements will serve as powerful tools for gameplay analysis, debugging, and creating advanced tutorials.

## Thank You
## 404 Not Found
## Tanmay Sharma 
## Vishesh Bhardwaj


  
