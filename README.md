# TableTennisAI

version: **Unity2019.4.20f1**

[日本語版](/README_JP.md)

A project that tried creating an AI that plays table tennis using MLAgents.
### ⚠️AI has not been completed in this project

---
目次
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
> #### <h4 id=Reward>Reward</h4>
> #### <h4 id=EpisodeEnd>Episode end condition</h4>
