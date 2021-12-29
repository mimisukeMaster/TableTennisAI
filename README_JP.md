# TableTennisAI

バージョン: **Unity2019.4.20f1**

MLAgentsを使って、卓球をするAIを作ってみたプロジェクト
### ⚠️AI (期待されたモデル) は完成していません

目次
> - [学習について](#AboutLearning)
>   - [概要](#Description)
>   - [Observation](#Observation)
>   - [Action](#Action)
>   - [Reward](#Reward)
> - [モデルについて](#AboutModel)
>   - [yamlファイル設定](#Yaml)
>   - [学習実行時のコマンド](#LearnCommand)



### <h3 id=AboutLearning>学習について</h3>
> #### <h4 id=Description> 概要</h4>
卓球台を挟んだ2人のAgentで１グループとして構成しています。学習の方向性としては、攻撃を仕掛けるというよりも、より長くラリーが続くように学習させます。
以下の [**Observation**](#Observation), [**Action**](#Action), [**Reward**](#Reward)の設定は、すべて[Agentのスクリプト](/TableTennisAI/Assets/Scripts/TableTennisAgent.cs)にXMLコメントとして明記されています。
それに加え、学習の改善履歴やID別の変更点が書かれています。(かなり長いので、障るなら削除して構いません。)

> #### <h4 id=Observation> Observation</h4>
**観測しているものは主に６つ　座標は絶対座標**
- 自分(ラケット)の座標 *(Vector3型,XYZ3つの数値)*

- 球の座標 *(Vector3型,XYZ3つの数値)*

- テーブルの座標 *(Vector3型,XYZ3つの数値)*

- 反対側のバウンドエリアの座標 *(Vector3型,XYZ3つの数値)*
> 卓球のルールとして、球を打って相手の陣地の机にバウンドさせる必要があるので、そのバウンドさせるエリアがどこにあるかを観測
- 自分(ラケット)の角度 *(Vector3型,XYZ3つの数値)*

- ボールの速度 *(Vector3型,XYZ3つの数値)*

観測の種類は6つ、Observationは**float数値で測るので計18つ**

> #### <h4 id=Action> Action</h4>
**Agentが取れる行動のパターンは主に２つ**
いどう
かくど

> #### <h4 id=Reward> Reward</h4>
