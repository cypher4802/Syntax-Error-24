# Lords_of_motion: An online real-time motion based game.
Remember the X men character which can control all the things by his bare hands the code that we proposed here does the same thing for the Single player fighting game and gives you the Real life experience of how it will feel like to have the motions implemented and is very user friendly.

# Key Implementations
 * The code focuses on the accuracy of detecting the motion on the basis of hand and legs movement and provide the correct distance and posture on where to start in the first view.
 * The server is then rendering the commands to the player that has started the game and then it gets the action according to that.
 * The game is given a fixed time for the fight and is based on how much damage is caused to the opponent.

# Pose Based Motion Detection
  We here are using the Mediapipe library for pose detection and getting the angles for the estimation of motion, for the punch the angles of the joints are responsible for getting the punch, for getting the kick we are using the change in length between the hip bone and the knee to get the kick left and right, for jump we are using the refrence lines in the frame of the camera above which there is jump then stand then sit. 
  
  All this starts with getting to the good distance region as will be there on the computer camera popup then you have to get to the recommended position then the game will start and you can get the functionality you want.
# Recording the actions
When you want to record the moves then you can just press the key R and then the commands will get recorded which will be replayed when you want after a buffer time.

# MultiPurpose API
The api that we have produced here is not only used in our game but according to the commands in the keyboard for other games can work for them as well so we have in a way created a universal API.

# Future goals
We will implement the server using websocket and then the rendering and efficiency of the video will be fast.

Thank You
404 Not Found
Tanmay Sharma 
Vishesh Bhardwaj

  