using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;

/// <summary>
/// <code>
/// 事前設定
/// # 可動範囲指定 =>Actionのこと
/// # 一回打ったらどこか当たらない限りもう一回打てない
/// Observation
///   See <see cref="CollectObservations(VectorSensor)"/> .
///  
/// Action <see cref="OnActionReceived(ActionBuffers)"/>.
/// # my moving pos and velocity
/// # my rotate
/// 
/// Reward 
/// # dropped: -0.5
/// # hit: 0.3
/// # netted: -0.4 (ネットかかったらやり直しだよ）
/// # time passing: little by little+
/// # 適正なところに跳ね返り overed net : 0.15
/// # 当たってからOpponentAreaにつくまで距離近い程 1 / Distance の報酬
/// 
/// -fifthで追加-
/// # 全く動かなかったら罰(静止防止) -0.05 Per Frame
/// 
/// -ninthで追加-
/// # 作り出した座標が範囲外なら罰 -0.02 Per Frame
/// 
/// episodeEndの時間制限はなし（落ちたらネットかかったらやり直し）
/// 
/// 
/// 変更点second -> third
/// secondの１００万step動画、この後進展せずRewardの値が飽和してしまったので改良てやり直しましたってする
/// ・打った後の報酬の与え方を改良した
/// ・＞＞＞ラケットの動きをRigidbodyにしないと球がそもそも力加えられなくてOpponentAreaに行かない、だからラケットをRigidrodyで[０１２３]学習させる
/// 　＞＞＞→transform.position += から rigidbody.Moveposition　へ　movepositionは物理挙動を接触した周りに与えることができる
/// ※学習IDはthirdTableTennis で実行
/// 
/// 変更点third -> fourth
/// ラケットが動こうとせず真上打ちして時間稼ぎに走ってしまったのでAIに任せてた移動Ratioを0.5で指定した　より動くよう促す
/// ※学習は ID　fourthTableTennis で実行
/// 
/// 変更点fourth -> fifth
/// Agentが全然動こうとしないのでフレーム間で動かなかったら罰を加える
/// ※学習は ID fifthTableTennis で実行
/// 
/// 変更点　fifth -> sixth
/// 撃った後の距離に応じた報酬をもっとたくさんあげる
/// 1 / dis  ===>  10 / dis へ
///　※学習は ID sixthTableTennis で実行
///　
/// 変更点 sixth -> seventh
/// 【大幅変更】、Rigidbodyの関数での移動を諦めTransformを作成した座標へと書き換えることにした
///　RacketRb.position = inAreaPosition; ってした。不具合懸念される、元に戻せるよう変更箇所明確化せよ。
///　変更コード位置は、
///　RacketRb.position = inAreaPosition;
///　この変更によりいらなくなったracketRbのRigidbody.MoveRigidbodyのコールチン実行箇所や付随物をコメントアウト、
///　コールチンは実行されなくて別にいいから放置
/// 以上
/// ※学習は ID seventhTableTennis で実行
/// 
/// 変更点 seventh -> eighth
/// ballをランダム位置生成　中央から±0.5のx横向き範囲で.
/// 観測するものを変更。相対座標から絶対座標に。Agentが自ら関係性気づけるようにした
/// Agent,ball,table,OpponentAreaの各々の絶対座標を観測＋ボールの速度なので 15 Observation -> 18 Observations　に
/// ※学習は ID eighthTableTennis で実行
/// 
/// 変更点　eighth -> ninth
/// Agentの座標生成の時点で、適正な位置が出てくるまでWhileではなく、作成して動かして範囲外だったらClampして抑えるという方法に変更
/// 処理の流れが変わるので移動するようになるかも
/// フレーム間で移動しなかったら罰にする処理を、Rigidbody.velocityで判断するのではなく、前後のフレーム間のpositionの変化を見るようにした(上記のせい)
/// なのでbeforeRacketPosというprivate Vector3を作成,<see cref="Initialize"/>で<see cref="defaultRacketPos"/>を割り当ててる
/// /// ※学習は ID ninthTableTennis で実行
/// 
/// 変更点 ninth -> tenth
/// Translateで動かして範囲外だったら逆方向にTranslateして留めさせるという方法に変更
/// ２回連続で打ったらreturnではなくEndEpisodeでぶちギル
/// 
/// ※うまくいったらコメントEnglish書けよ
/// ※学習は ID tenthTableTennis で実行
/// </code>
/// </summary>
public class TableTennisAgent : Agent
{
    /// <summary>
    /// The tennis ball
    /// </summary>
    [Tooltip("The tennis ball")]
    public Transform Ball;

    /// <summary>
    /// The table
    /// </summary>
    [Tooltip("The table")]
    public Transform Table;

    /// <summary>
    /// The opponent to play ball with
    /// </summary>
    [Tooltip("The opponent to play ball with")]
    public TableTennisAgent Opponent;
    
    /// <summary>
    /// Collider in a zone separated by a net on a table.
    /// </summary>
    [Tooltip("Collider in a zone separated by a net on a table.")]
    public Collider TableCollider_1;
    /// <summary>
    /// Collider in a zone separated by a net on a table.
    /// </summary>
    [Tooltip("Collider in a zone separated by a net on a table.")]
    public Collider TableCollider_2;

    /// <summary>
    /// Like a bounding box that determines the area where the Agent can move.
    /// </summary>
    [Tooltip("Like a bounding box that determines the area where the Agent can move.")]
    public Collider moveArea_1;
    /// <summary>
    /// Like a bounding box that determines the area where the Agent can move.
    /// </summary>
    [Tooltip("Like a bounding box that determines the area where the Agent can move.")]
    public Collider moveArea_2;

    /// <summary>
    /// For debugging to check that the opposing OpponentArea is correctly recognized in the editor. Never be got.
    /// </summary>
    [Tooltip("For debugging to check that the opposing OpponentArea is correctly recognized in the editor. Never be got.")]
    public Collider MyOpponentArea;

    /// <summary>
    /// For debugging to check that you have correctly identified the area the Agent can move in the editor. Never be got.
    /// </summary>
    [Tooltip("For debugging to check that you have correctly identified the area the Agent can move in the editor. Never be got.")]
    public Collider MyMoveArea;

    /// <summary>
    /// My transform. Raket is the Agent.
    /// </summary>
    private Transform Racket;

    /// <summary>
    /// Vector3(Position) of the first racket before the episode starts, different for each individual Agent.
    /// </summary>
    private Vector3 defaultRacketPos;

    /// <summary>
    /// Quaternion(Rotation) of the first racket before the episode starts.
    /// </summary>
    private Quaternion defaultRacketRot;

    /// <summary>
    /// Vector3 of the first ball before the episode starts, different for each individual Group.
    /// </summary>
    private Vector3 defaultBallPos;

    /// <summary>
    /// Vector3 to save the position of the Agent one frame ago.
    /// </summary>
    private Vector3 beforeRacketPos;

    /// <summary>
    /// The rigidbody of the Agent (myself)
    /// </summary>
    private Rigidbody racketRb;

    /// <summary>
    /// The rigidbody of the ball
    /// </summary>
    private Rigidbody ballRb;

    /// <summary>
    /// Collider of the other side's table zone calculated by <see cref="Initialize()"/>
    /// </summary>
    private Collider opponentArea;

    /// <summary>
    /// Collider that defines the area within which the Agent can move.
    /// Agent moves only within this area. Caculated by <see cref="Initialize()"/>
    /// </summary>
    private Collider moveArea;

    /// <summary>
    /// Once the Agent hits a ball, it cannot be hit again until it bounces off the table,
    /// so we need to flag the ball to see if it is in a state to be hit.
    /// </summary>
    private bool isHitable;

    /// <summary>
    /// A flag for giving a reward the closer the ball is to the OpponentArea 
    /// until the Agent hits the ball and collides with the OpponentArea.
    /// </summary>
    private bool isHitPeriod;

    /// <summary>
    /// <para> Agentが自分で生成した座標への移動中に、新しく座標が生成され、前の移動が止まってしまわないよう移動中かどうかを管理するフラグ</para>
    /// A flag that manages whether the Agent is moving or not to prevent the previous movement
    /// from stopping due to the creation of a new position while the Agent is moving to the position it has created.
    /// </summary>
    //private bool moving;


    /// <summary>
    /// Only called once, regardless of the episode.
    /// </summary>
    public override void Initialize()
    {
        Debug.Log("initlaize called");
        // Refer to own Transform
        Racket = this.transform;

        // Set transforms 
        float posX = Racket.position.x;
        float posY = Racket.position.y;
        float posZ = Racket.position.z;
        defaultRacketPos = new Vector3(posX, posY, posZ);
        float rotX = Racket.rotation.x;
        float rotY = Racket.rotation.y;
        float rotZ = Racket.rotation.z;
        float rotW = Racket.rotation.w;
        defaultRacketRot = new Quaternion(rotX, rotY, rotZ, rotW);
        posX = Ball.position.x;
        posY = Ball.position.y;
        posZ = Ball.position.z;
        defaultBallPos = new Vector3(posX, posY, posZ);

        beforeRacketPos = defaultRacketPos;

        // Refer to my(Agent) rigidbody
        racketRb = GetComponent<Rigidbody>();

        // Refer to ball's rigidbody
        ballRb = Ball.GetComponent<Rigidbody>();

        // Calculate which table's zone is farther from the Agent,
        // and specify the farther one as the other's zone. ("o" means opponent)
        float oDistance_1 = Vector3.Distance(Racket.position, TableCollider_1.transform.position);
        float oDistance_2 = Vector3.Distance(Racket.position, TableCollider_2.transform.position);

        if(oDistance_1 < oDistance_2)
        {
            opponentArea = TableCollider_2;
        }
        if(oDistance_2 < oDistance_1)
        {
            opponentArea = TableCollider_1;
        }

        MyOpponentArea = opponentArea;

        //Calculate which moveArea is closer to the Agent,
        //and assign the closer one to the moveArea as the Collider (bounding box) within which the Agent can move.
        float mDistance_1 = Vector3.Distance(Racket.position, moveArea_1.transform.position);
        float mDistance_2 = Vector3.Distance(Racket.position, moveArea_2.transform.position);

        if (mDistance_1 < mDistance_2)
        {   
            moveArea = moveArea_1;
        }   
        if (mDistance_2 < mDistance_1)
        {
            moveArea = moveArea_2;
        }

        MyMoveArea = moveArea;
    }



    public override void OnEpisodeBegin()
    {
        // Reset the Agent's position
        Racket.position = defaultRacketPos;

        // Reset the Agent's rotation
        Racket.rotation = defaultRacketRot;

        // Set the ball in a fixed position
        Ball.position = defaultBallPos;
        Ball.position = new Vector3(Random.Range(defaultBallPos.x - 0.5f, defaultBallPos.x + 0.5f), defaultBallPos.y, defaultBallPos.z); 

        // Reset the rigidbody
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        // Make it ready to hit
        isHitable = true;

        // Set the flag back to false
        isHitPeriod = false;

        // Set the flag back to false
        //moving = false;
    }

    /// <summary>
    /// Collect vector observations from the environment
    /// </summary>
    /// <param name="sensor">The vector sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe Agent's position (3 observations)
        sensor.AddObservation(Racket.position);

        // Observe ball's position (3 observations)
        sensor.AddObservation(Ball.position);

        // Observe Table's position (3 observations)
        sensor.AddObservation(Table.position);

        // Observe the position of opponent's area (3 observations)
        sensor.AddObservation(opponentArea.transform.position);


        // Observe the Agent's rotation (3 Observations)
        sensor.AddObservation(Racket.localEulerAngles);

        // Observe ball's velocity (3 Observations)
        sensor.AddObservation(ballRb.velocity);

        // 18 observations (15->18 changed)
    }

    /// <summary>
    /// Called when and action is received from either the player input or the neural network
    /// </summary>
    /// <param name="actions">The actions to take</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Uses MaxStep as the denominator and gives a small reward for each step
        AddReward(1f / MaxStep);

        // Find out if the Agent is in area of the action,
        // and make sure it doesn't go out of area
      
        // Make sure the Agent is not in a move
        

        racketRb.transform.Translate(
            new Vector3(actions.ContinuousActions[0], actions.ContinuousActions[1],actions.ContinuousActions[2]) * Time.deltaTime);

        Collider[] colliders = Physics.OverlapSphere(racketRb.position, 0.002f);

        if (!colliders.Contains(moveArea))
        {
            AddReward(-0.02f);
            // If the Agent is out of range, it will move in the opposite direction just out of range and remain stationary.
            racketRb.transform.Translate(
                new Vector3(actions.ContinuousActions[0], actions.ContinuousActions[1], actions.ContinuousActions[2]) * Time.deltaTime * -1);

        }

        // the Agent can move freely about each axis.
        Racket.Rotate(new Vector3(1, 0, 0), Mathf.Clamp(actions.ContinuousActions[3] * 20, 0, 360));
        Racket.Rotate(new Vector3(0, 1, 0), Mathf.Clamp(actions.ContinuousActions[4] * 20, 0, 360));
        Racket.Rotate(new Vector3(0, 0, 1), Mathf.Clamp(actions.ContinuousActions[5] * 20, 0, 360));

    }

    /// <summary>
    /// For user operation
    /// </summary>
    /// <param name="actionsOut"></param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var DiscreteActionsOut = actionsOut.DiscreteActions;

        // Convert keybord inputs to movement and turning
        // All values should be between -1 and 1

        DiscreteActionsOut[0] = 10;
        // Forward/backward
        if (Input.GetKey(KeyCode.W)) racketRb.position += transform.forward * Time.deltaTime;
        if (Input.GetKey(KeyCode.S)) racketRb.position += -transform.forward* Time.deltaTime;

        // left/right
        if (Input.GetKey(KeyCode.A)) racketRb.position += -transform.right * Time.deltaTime;
        else if (Input.GetKey(KeyCode.D)) racketRb.position += transform.right * Time.deltaTime;

        // Up/down
        if (Input.GetKey(KeyCode.E)) racketRb.position += transform.up * Time.deltaTime;
        else if (Input.GetKey(KeyCode.C)) racketRb.position += -transform.up * Time.deltaTime;

        // Pitch up/down
        if (Input.GetKey(KeyCode.UpArrow)) Racket.Rotate(transform.forward,Time.deltaTime);
        else if (Input.GetKey(KeyCode.DownArrow)) Racket.Rotate(-transform.forward, Time.deltaTime);

        // Turn left/right
        if (Input.GetKey(KeyCode.LeftArrow)) Racket.Rotate(transform.right, Time.deltaTime);
        else if (Input.GetKey(KeyCode.RightArrow)) Racket.Rotate(-transform.right, Time.deltaTime);
    }

    /// <summary>
    /// Called when the ball dropped to the floor in game.
    /// </summary>
    public void BallDropped()
    {
        // Collided with the floor, give a negative reward
        AddReward(-0.5f);

        // End this episode, the opponent's episode too.
        EndEpisode();
        Opponent.EndEpisode();

    }

    /// <summary>
    /// Called when the ball hits the racket.
    /// </summary>
    public void BallHit()
    {
        // If the Agent hits the ball when it is not allowed to be hit, it is invalid.
        if (!isHitable) EndEpisode();

        // Give a reward for hitting it
        AddReward(0.3f);

        // Can't hit it again until it hits the table
        isHitable = false;

        // Start a period where the closer the ball moves to OpponentArea, the more rewards it will be given.
        isHitPeriod = true;
    }

    /// <summary>
    /// Called when the ball collided with the table.
    /// </summary>
    public void BallBounced(Collider collidedZone)
    {
        // When the ball bounces on the opponent's collide zone (success)
        if (collidedZone == opponentArea)
        {
            // Give a reward for bouncing the ball in the correct place
            AddReward(0.15f);

            // Make the ball ready to hit
            // The Agent will only move within its own area, so it will never go to the opponent's one
            isHitable = true;

            //End a period where the closer the ball moves to OpponentArea, the more rewards it will be given.
            isHitPeriod = false;
        }

        // When the ball bounces on own collide zone (failure), or on the leg of the table, or the back of the table
        if (collidedZone != opponentArea)
        {
            // Give a negative reward for bouncing the ball in the wrong place
            AddReward(-0.1f);

            // End this episode, the opponent's episode too
            EndEpisode();
            Opponent.EndEpisode();
        }
    }

    /// <summary>
    /// Called when the ball netted in a game.
    /// </summary>
    public void BallNetted()
    {
        // Give a negative reward for netted
        AddReward(-0.4f);

        // End this episode, the opponent's episode too
        EndEpisode();
        Opponent.EndEpisode();
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        // If the Agent hit the ball and before it collide with an OpponentArea
        if (isHitPeriod)
        {
            // the closer the ball moves to OpponentArea, the more rewards it will be given.
            float dis = Vector3.Distance(Ball.position, opponentArea.transform.position);
            AddReward(10 / dis);
        }

        // If the Agent does not move at all between the two frames, give a negative reward
        // ==> Urge the Agent to move
        if (racketRb.position == beforeRacketPos) 
        {
            Debug.DrawRay(racketRb.position, new Vector3(0, 0.5f, 0), Color.green);
            AddReward(-0.05f);
            
        }
        else
        {
            beforeRacketPos = racketRb.position;
        }

    }
}
