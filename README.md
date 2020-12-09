# ShapeShifter
Unity BlendShape remixer

## これは何

UnityのMeshに付いているBlendShapeを再構築するツールです

## 応用例

+ 用途に応じて最小限のBlendShapeで利用する(ファイナライズ)
+ パーツごとにモーフ設定された無限の組み合わせの中から、性格や設定、表情パターンに基づいて出力する(フェイシャルクリエイション)

## 使い方

### ShapeShifterVRM

[ダウンロード 0.1.1](https://github.com/m2wasabi/ShapeShifter/releases/download/0.1.1/ShapeShifterVRM_0.1.1.unitypackage)

1. バックアップは取りましょう！！
1. Unityプロジェクトを作る
1. [UniVRM 0.62.0](https://github.com/vrm-c/UniVRM/releases) をインポートする
1. ShapeShifterVRM をインポートする
1. vrmモデルをインポートする
1. シーンのHierarchyにvrmモデルを配置する
1. Window -> ShapeShifter -> ShapeShifter VRM で窓を呼び出す
1. Prefab, Mesh, BlendShapeClip を放り込んで `ReplacePlendShapes` ボタンを押す
1. シーン上のモデルをvrmとして書き出す
1. BlendShapeを再設定する

### As a Library

Unityプロジェクトからコードで使う場合

`using ShapeShifter.Scripts;`

```csharp
            SkinnedMeshRenderer smr;  // Skinned Mesh Renderer to Edit
            List<BlendShapeClip> blendShapeClips;  // Blend Shape Clips to Import

            var shifter = new ShapeShifterVRM(smr);
            smr.sharedMesh = shifter.BuildMesh(blendShapeClips); // in case of OverWrite
```

## 既知の問題

+ VRM の BlendShapeの紐づけが壊れるので、vrmファイルへエクスポート⇒再構築が必要
