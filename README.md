# TableTennisAI

version: **Unity2019.4.20f1**

[日本語版](/README_JP.md)

A project that tried creating an AI that plays table tennis using MLAgents.
### ⚠️AI is still not perfectly created. in this project



https://user-images.githubusercontent.com/81568941/147680661-54ec2a3a-b351-4daf-96b5-d5753d98db52.mp4




---
Contents
> - [About Learning](#AboutLearning)
>   - [Overview](#Description)
>   - [Observation](#Observation)
>   - [Action](#Action)
>   - [Reward](#Reward)
>   - [Episode end condition](#EpisodeEnd)
> - [About Models](#AboutModel)
>   - [.yaml file setting](#Yaml)
>   - [Commands for learning](#LearnCommand)
---
##

### <h3 id=AboutLearning>About Learning</h3>


> #### <h4 id=Description>Overview</h4>
Two Agents across a ping-pong table make up one group. The direction of learning is not to launch an attack, but to make the rally last longer. The following [**Observation**](#Observation), [**Action**](#Action), [**Reward**](#Reward), settings are all specified as XML comments in the [Agent's script](/Assets/Scripts/TableTennisAgent.cs). In addition to that, there is a history of learning improvements and changes for each ID. (It's quite long, so feel free to delete it if it hinders you.)

##
> #### <h4 id=Observation>Observation</h4>
There are six main things we're observing. The coordinates are absolute.

- Transform of myself (racket) (Vector3, XYZ 3 values)

- Transform of the ball (Vector3, XYZ 3 values)

- Transform of the table (Vector3, XYZ 3 values)

- Transform of the opposite bouncing area (Vector3,XYZ 3 values)

  - As a rule of table tennis, the Agent need to hit the ball and make it bounce on the table of the opponent's position, so it needs to observe where the bouncing area is.

- Angle of your (racket) (Vector3 type,XYZ 3 values)

- Velocity of the ball (Vector3 type, XYZ 3 values)

There are 6 types of observations and Observation is counted by float numbers, so there are 18 in total.


> #### <h4 id=Action>Action</h4>
**There are two main patterns of behavior that Agent can take**
- Moving
  - Move by `Rigidbody.position`  
  The area where you can move is specified by moveArea, and it will be adjusted so that you can't exceed that area.
- Changing the angle
  - `Transform.Rotate(Vector axis, float angle)` to change the angle. X,Y,Z axis.

Six types of actions, all handled by float values.


> #### <h4 id=Reward>Reward</h4>
#### positive rewards
|Conditions|Value|Note|
|:---:|:---|:---|
|The ball hits the racket|0.3|Praise for hitting the ball.|
|The ball bounces on the opponent's area|0.15|Give the Agent credit for reaching over there.|
|Time lapse (per Frame)|1 / max steps(5000)|Try to keep the rally going for as long as possible.|
|From the time the ball is hit until it bounces the opponent's area (per Frame)|The closer the distance between the ball and its area, the more rewards Agents gets.|make sure the Agent gets it.|

#### negative rewards
|Conditions|Value|Note|
|:---:|:---|:---|
|Fall to the ground|-0.4|End the episode|
|Be caught in a net|-0.5|End the episode|
|Not moving between 2 frames (per Frame)|-0.05|get the Agent to move.|
|Move out of range (per frame)|-0.02|make sure the Agent doesn't go out of range.|

##
> #### <h4 id=EpisodeEnd>Episode end condition</h4>
**Conditions for Episode end**
- When the Agent hits the ball when you can't hit it (twice in a row)

- When the Agent bounces a ball in your own territory

- When the ball falls

- When the ball is caught in a net


### <h3 id=AboutModel>About Models</h3>
> #### <h4 id=Yaml> .yaml file setting</h4>
If you look at [`TableTennis.yaml`](/TableTennis.yaml), which describes the number of learning steps and how to handle them, you will see..
- We have set the `maxstep: 1000000` to allow for more room.

> #### <h4 id=LearnCommand>Commands for learning</h4>
When you run the learning, 
- Go to the path where [`TableTennis.yaml`](/TableTennis.yaml) is located with the following command.
```
cd (The path where the repository is located)~~/TabletennisAI
```
- Start learning with the following command after passing the Path of mlagents.
```
mlagents-learn ./TableTennis.yaml --run-id=(ID created by you) --torch-device cuda
```
🚩
You can create your own `run-id` and files with that name will be created under config of Path of ML-Agents with actual learning body
（The [results](/results) in this project are the files that were pulled from there）

❗ You need to install MLAgents on your machine and go through the path of MLAgents to learn.

**I've tweeted [here](https://twitter.com/mimisukeMaster/status/1461321187858944004) about how to install ML-Agents before, so please refer to it!**

🚩
`--torch-device cuda`is needed for learning on GPU, so you don't have to write.
