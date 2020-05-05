using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using ShapeShifter.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using VRM;

namespace ShapeShifter.Editor
{
    public class ShapeShifterVRM : EditorWindow
    {
        [MenuItem("Window/ShapeShifter/ShapeShifter VRM")]
        private static void ShowWindow()
        {
            var window = GetWindow<ShapeShifterVRM>();
            window.titleContent = new GUIContent("ShapeShifterVRM");
            window.Show();
        }

        #region Properties

        [SerializeField] private GameObject _modelPrefab;
        [SerializeField] private SkinnedMeshRenderer _TargetMesh;
        [SerializeField] private List<BlendShapeClip> _blendShapeClips;

        #endregion
        private Vector2 scrollPos = Vector2.zero;
        private string modelSavePath;
        private void OnGUI()
        {
            // 自身のSerializedObjectを取得
            var so = new SerializedObject(this);

            so.Update();
        
            // 第二引数をtrueにしたPropertyFieldで描画
            EditorGUILayout.PropertyField(so.FindProperty("_modelPrefab"), true);
            EditorGUILayout.PropertyField(so.FindProperty("_TargetMesh"), true);
            
            
            scrollPos = EditorGUILayout.BeginScrollView( scrollPos,GUI.skin.box );
            EditorGUILayout.PropertyField(so.FindProperty("_blendShapeClips"), true);
            EditorGUILayout.EndScrollView();
            
            GUILayout.Label( "WARNING! Replaceを実行すると BlendShapeClipは壊れます！！" );
            if( GUILayout.Button( "ReplaceBlendShapes" ) ) ReplaceBlendShapes();
            
            so.ApplyModifiedProperties();

        }

        private void ReplaceBlendShapes()
        {
            var prefab = PrefabUtility.GetPrefabParent (_modelPrefab);
            modelSavePath = AssetDatabase.GetAssetPath (prefab);
            modelSavePath = modelSavePath.Substring(0, modelSavePath.Length - 7);
            
            var mesh = Instantiate(_TargetMesh.sharedMesh);

            var blendCount = mesh.blendShapeCount;
            var blendShapeFrames = new List<BlendShapeFrame>();
            for (int i = 0; i < blendCount; i++)
            {
                // blendShapeFrameCount を取得する (大抵1である)
                var bsc = mesh.GetBlendShapeFrameCount(i);
                for (int j = 0; j < bsc; j++)
                {
                    var frame = new BlendShapeFrame(mesh, i, j);
                    blendShapeFrames.Add(frame);
                }
            }

            var newMesh = Instantiate(mesh);
            newMesh.name = newMesh.name.Substring(0, newMesh.name.IndexOf('('));
            newMesh.ClearBlendShapes();
            foreach (var blendShapeClip in _blendShapeClips)
            {
                var vCount = mesh.vertexCount;
                var vertices = new Vector3[vCount];
                var normals = new Vector3[vCount];
                var tangents = new Vector3[vCount];
                
                var mixRatio = new List<MixRatio>();
                foreach (var value in blendShapeClip.Values)
                {
                    //if (value.RelativePath != _TargetMesh.name) continue;
                    mixRatio.Add(new MixRatio(value.Index, value.Weight / 100));
                }
                for (int i = 0; i < vCount; i++)
                {
                    var v = Vector3.zero;
                    var n = Vector3.zero;
                    var t = Vector3.zero;
                    foreach (var mix in mixRatio)
                    {
                        v += blendShapeFrames[mix.Index].vertices[i] * mix.Weight;
                        n += blendShapeFrames[mix.Index].normals[i] * mix.Weight;
                        t += blendShapeFrames[mix.Index].tangents[i] * mix.Weight;
                    }

                    vertices[i] = v;
                    normals[i] = n.normalized;
                    tangents[i] = t.normalized;
                }
                newMesh.AddBlendShapeFrame(blendShapeClip.BlendShapeName, 1.0f , vertices, normals, tangents);
            }
            
            _TargetMesh.sharedMesh = newMesh;
        }

        public Mesh StoreMesh(Mesh mesh)
        {
            var meshSavePath = modelSavePath + ".Meshes/";
            if (!Directory.Exists(meshSavePath)) Directory.CreateDirectory(meshSavePath);

            var storedMeshPath = meshSavePath + mesh.name + ".asset";
            if (!File.Exists(storedMeshPath))
            {
                AssetDatabase.CreateAsset(mesh, storedMeshPath);
                AssetDatabase.SaveAssets ();
            }
            return AssetDatabase.LoadAssetAtPath<Mesh>(storedMeshPath);
        }

        private class MixRatio
        {
            public int Index;
            public float Weight;
            public MixRatio(int index, float weight)
            {
                Index = index;
                Weight = weight;
            }
        }
    }
}
