# Simple-Platformer

Framework for a platformer made in Unity based off Little Big Planet for learning purposes.

## Features

- Player movement and controls using a rigidbody and box collider.
    - Shifting between three lanes.
    - Running, jumping, and grabbing.
    - Slope movement.
- Auto shift lanes.
    - Shifts the player to a lane with ground when falling for smoother gameplay.
- Level functionality.
    - Points.
    - Checkpoint system.
    - Deaths and respawns.
    - Level completed or failed.
- Animations
    - Running.
    - Idle.
    - Grabbing.
- Pause/restart/game over menu.
- Player input through Unity Input System

## Project Notes

### Raycasting

Raycasting, along with boxcasts and spherecasts are used for grounding, slopes, lane shifting, auto shifting, and grab checks. Learning how to properly take advantage of these proved to be extremely useful. The grounded check is done through a short boxcast from the center of the player collider to right below the player collider. Lane shifting has boxcasts the size of the player collider on both sides of the colliders x-axis to make sure there is space to shift. Auto shifting lanes uses raycasts in each lane to check if there is ground the player should be moved to while they are falling if they have none below them. Lastly the grab uses a spherecast to check for any grabbable objects in its range so the player can latch onto it. Slope ray casts are explained below in slopes section. All of these can be seen in the screenshots below.

### Events and Delegates

Events and delegates are used to help control the game. They are used to handle things such as deaths, pauses, the checkpoint system, etc. They make it easy to reference and call a method within another class/script to help handle game logic associated with certain events.

### Prefabs

Prefabs are extremely useful in creating the framework for a platformer like this. The player, score bubbles, finish, game controller, and checkpoint system are all prefabs. With these existing, all that needs to be done to create more levels is design the terrain for the player to traverse.

### Rigidbody Movement/Physics

The player movement is done by applying force to the rigidbody based on the user's input direction. The base implementation is straightforward but became more complicated as I decided how I wanted my player to move. I had to decide on restrictions, such as should the player stick to the ground when transitioning from a slope to a non-slope, should the player slow down when over the max speed, what kind of friction should there be, etc. This made using a rigidbody and the physics system for movement a challenge and even more so when I introduced slopes. Eventually the correct forces were added for each type of movement/scenario through a lot of testing.

### Slopes

Slopes are handled using two raycasts, one at the forward edge and one at the backwards edge of the players collider. Slope movement turned out to be the most difficult part of dealing with the player movement. I went through a lot of iterations determining how I wanted the player to move on slopes and each iteration had its own problems to be tackled. Some of the issues that had to be taken care of in the final iteration were transitions between a slope and non-slope (or vice-versa), keeping grounded while moving on the slope or during a transition, getting the correct movement value to keep a consistent speed on the slope, and needing to determine/deal with uphill/downhill movement. The two raycasts along with a bunch of logic ultimately helped solve all these issues.

### Modeling and Animation

I tried my hand at modeling and animating the character to be used for the project but ended up going with an already existing model with my own running and idle animations (other animations were made but not used). Attempting to model my own character helped me to learn a lot about using blender which eventually allowed me to create a level for the project to be used with. Creating animations lead me to learning about the animator systems in blender and unity as well as the states necessary to play the animation you want in specific scenarios.


## Screenshots

![Player Front](/Screenshots/FrontSacboyText.png?raw=true)

![Player Side](/Screenshots/SideSacboy.PNG?raw=true)


## Demo

https://youtu.be/eKQHRevVIUs
