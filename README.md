# ShapeShifter
Unity BlendShape remixer

## これは何

UnityのMeshに付いているBlendShapeを再構築するツールです

## 応用例

+ 用途に応じて最小限のBlendShapeで利用する(ファイナライズ)
+ パーツごとにモーフ設定された無限の組み合わせの中から、性格や設定、表情パターンに基づいて出力する(フェイシャルクリエイション)

## 使い方

### ShapeShifterVRM

[ダウンロード 0.0.1](./releases/download/0.0.1/ShapeShifterVRM_0.0.1.unitypackage)

0. バックアップは取りましょう！！
1. シーンのHierarchyにvrmモデルを配置する
2. Window -> ShapeShifter -> ShapeShifter VRM で窓を呼び出す
3. Prefab, Mesh, BlendShapeClip を放り込んで `ReplacePlendShapes` ボタンを押す
4. シーンのモデルをvrmとして書き出す
5. BlendShapeを再設定する

## 既知の問題

+ バリデーションが雑なので余計なBlendShapeを入れるとダメ
+ PrefabとModel、BlendShapeが関係ないモデルの時にダメ
+ VRM の BlendShapeの紐づけが壊れるので、vrmファイルへエクスポート⇒再構築が必要
