# TableTennisAI

バージョン: **Unity2019.4.20f1**

MLAgentsを使って、卓球をするAIを作ってみたプロジェクト
### ⚠️AI (期待されたモデル) は完成していません
---
目次
> - [学習について](#AboutLearning)
>   - [概要](#Description)
>   - [Observation(観測)](#Observation)
>   - [Action(行動)](#Action)
>   - [Reward(報酬)](#Reward)
>   - [Episode終了条件](#EpisodeEnd)
> - [モデルについて](#AboutModel)
>   - [yamlファイル設定](#Yaml)
>   - [学習実行時のコマンド](#LearnCommand)
---
##

### <h3 id=AboutLearning>学習について</h3>


> #### <h4 id=Description> 概要</h4>
卓球台を挟んだ2人のAgentで１グループとして構成しています。学習の方向性としては、攻撃を仕掛けるというよりも、より長くラリーが続くように学習させます。
以下の [**Observation**](#Observation), [**Action**](#Action), [**Reward**](#Reward)の設定は、すべて[Agentのスクリプト](/Assets/Scripts/TableTennisAgent.cs)にXMLコメントとして明記されています。
それに加え、学習の改善履歴やID別の変更点が書かれています。(かなり長いので、障るなら削除して構いません。)

##
> #### <h4 id=Observation> Observation(観測)</h4>
**観測しているものは主に６つ　座標は絶対座標**
- 自分(ラケット)の座標 *(Vector3型,XYZ3つの数値)*

- 球の座標 *(Vector3型,XYZ3つの数値)*

- テーブルの座標 *(Vector3型,XYZ3つの数値)*

- 反対側のバウンドエリアの座標 *(Vector3型,XYZ3つの数値)*
  - 卓球のルールとして、球を打って相手の陣地の机にバウンドさせる必要があるので、そのバウンドさせるエリアがどこにあるかを観測
- 自分(ラケット)の角度 *(Vector3型,XYZ3つの数値)*

- ボールの速度 *(Vector3型,XYZ3つの数値)*

観測の種類は6つ、Observationは**float数値で測るので計18つ**

##
> #### <h4 id=Action> Action(行動)</h4>
**Agentが取れる行動のパターンは主に２つ**
- 移動する
  - `Rigidbody.position`で移動  
   自分が動ける範囲を`moveArea`で指定しており、その範囲を超えられないよう調整される
- 角度を変える
  - `Transform.Rotate(Vector axis, float angle)` で角度変更。X,Y,Zの3軸 

行動の種類は6つ、すべてfloat値

##
> #### <h4 id=Reward> Reward(報酬)</h4>
#### 正の報酬
|条件|報酬値|補足|
|:---:|:---|:---|
|球がラケットに当たる|0.3|当たったことをほめる|
|球が相手の陣地でバウンドする|0.15|向こうまで届かせたことをほめる|
|時間経過(Frame毎)|1 / 最大ステップ(5000)|なるべく長い間ラリーが続くようにする|
|球を打ってから相手の陣地につくまで(Frame毎)|球とその陣地との距離が近いほど大きい報酬|届かせるよう仕向ける|

#### 負の報酬
|条件|報酬値|備考|
|:---:|:---|:---|
|地面に落ちる|-0.5|[エピソードを終了する](#EpisodeEnd)|
|ネットに掛かる|-0.4|[エピソードを終了する](#EpisodeEnd)|
|2Frame間で動いていない(Frame毎)|-0.05|動くよう仕向ける|
|範囲外のところに移動(フレーム毎)|-0.02|範囲外に出ないよう仕向ける|

##
> #### <h4 id=EpisodeEnd> Episode終了条件</h4>
**Episodeが終了する条件**
- 打てない状態で打ったとき(2回連続で打ったとき)

- 自分の陣地で球をバウンドさせたとき

- 球が落ちた時

- 球がネットに掛かったとき


### <h3 id=AboutModel>モデルについて</h3>
> #### <h4 id=Yaml> yamlファイル設定</h4>
学習のステップ数や処理方法などを記した[`TableTennis.yaml`](/TableTennis.yaml)を見れば分かりますが、
- 余裕を持たせて`maxstep: 1000000`
にしてあります。

> #### <h4 id=LearnCommand>[学習実行時のコマンド]</h4>
学習を実行させる際は、
- `cd`で[`TableTennis.yaml`](/TableTennis.yaml)があるパスに以下のコマンドで移動。
```
cd (リポジトリがあるパス)~~/TabletennisAI
```
- mlagentsのパスを通したうえで、以下のコマンドで学習開始
```
mlagents-learn ./TableTennis.yaml --run-id=(自分で作成したID) --torch-device cuda
```
🚩
`run-id`は自分で作成し、実際に学習する本体があるMLAgentsのパスのconfig以下に、その名でファイルが生成されます。
（このプロジェクトにある[results](/results)は、そのファイルを引っ張ってきた）

❗ 各自でMLAgentsを実機に導入し、MLAgentsのパスを通しておかないと学習できません

**ML-Agents導入方法については以前に[こちらでツイート](https://twitter.com/mimisukeMaster/status/1461321187858944004)したことがあるので、参考にしてください**

🚩
`--trorch-device cuda` は、GPUで学習するときに必要なものなので、なくても構いません
