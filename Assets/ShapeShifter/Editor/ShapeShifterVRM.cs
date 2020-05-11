using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShapeShifter.Scripts;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VRM;

namespace ShapeShifter.Editor
{
    public class ShapeShifterVRM : EditorWindow
    {
        [MenuItem("VRM/ShapeShifter VRM", priority = 36000)]
        private static void ShowWindow()
        {
            var window = GetWindow<ShapeShifterVRM>();
            window.titleContent = new GUIContent("ShapeShifterVRM");
            window.Show();
        }

        #region Properties
        #pragma warning disable 0649
        [SerializeField] private GameObject _modelPrefab;
        [SerializeField] private SkinnedMeshRenderer _TargetMesh;
        [SerializeField] private List<BlendShapeClip> _blendShapeClips;
        #pragma warning restore 0649
        #endregion
        
        private Vector2 scrollPos = Vector2.zero;
        private string modelSavePath;

        #region ReordableList
        ReorderableList m_clipList;
        private SerializedObject _so;

        protected void OnEnable()
        {
            _so = new SerializedObject(this);

            var prop = _so.FindProperty("_blendShapeClips");
            m_clipList = new ReorderableList(_so, prop);

            m_clipList.drawHeaderCallback = (rect) =>
                EditorGUI.LabelField(rect, "BlendShapeClips");

            m_clipList.elementHeight = BlendShapeClipDrawer.Height;
            m_clipList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = prop.GetArrayElementAtIndex(index);
                rect.height -= 4;
                rect.y += 2;
                EditorGUI.PropertyField(rect, element);
            };

            m_clipList.onAddCallback += (list) =>
            {
                // Add slot
                prop.arraySize++;
                // select last item
                list.index = prop.arraySize - 1;
                // get last item
                var element = prop.GetArrayElementAtIndex(list.index);
                element.objectReferenceValue = null;
            };
        }
        #endregion

        private void OnGUI()
        {
            _so.Update();

            EditorGUILayout.PropertyField(_so.FindProperty("_modelPrefab"), true);
            EditorGUILayout.PropertyField(_so.FindProperty("_TargetMesh"), true);
            
            // BlendShapeClipは ScrollViewで囲む
            scrollPos = EditorGUILayout.BeginScrollView( scrollPos,GUI.skin.box );
            m_clipList.DoLayoutList();
            EditorGUILayout.EndScrollView();
            
            GUILayout.Label( "WARNING! Replaceを実行すると VRM出力後にBlendShapeClipの再定義が必要です。" );
            if( GUILayout.Button( "ReplaceBlendShapes" , GUILayout.Height(50)) ) ReplaceBlendShapes();
            
            _so.ApplyModifiedProperties();

        }

        private void ReplaceBlendShapes()
        {
            // SerializeObjectからPropertyに反映させる
            _so.ApplyModifiedProperties();

            var prefab = PrefabUtility.GetCorrespondingObjectFromSource (_modelPrefab);
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
            var clipIndex = 0;
            foreach (var blendShapeClip in _blendShapeClips.Distinct())
            {
                // フォームが空の場合は飛ばす
                if (blendShapeClip == null || blendShapeClip.Values == null) continue;

                var vCount = mesh.vertexCount;
                var vertices = new Vector3[vCount];
                var normals = new Vector3[vCount];
                var tangents = new Vector3[vCount];
                
                var blendRatio = new List<BlendRatio>();
                var baseBindings = new List<BlendShapeBinding>();
                var relativePath = "";
                foreach (var value in blendShapeClip.Values)
                {
                    if (Path.GetFileName(value.RelativePath) != _TargetMesh.name)
                    {
                        baseBindings.Add(value);
                        continue;
                    }

                    relativePath = value.RelativePath;
                    blendRatio.Add(new BlendRatio(){Index = value.Index, Weight = value.Weight / 100});
                }
                // 関係するシェイプが無い場合は飛ばす
                if (blendRatio.Count == 0) continue;
                for (int i = 0; i < vCount; i++)
                {
                    var v = Vector3.zero;
                    var n = Vector3.zero;
                    var t = Vector3.zero;
                    foreach (var mix in blendRatio)
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
                var mergedBinding = new BlendShapeBinding
                {
                    RelativePath = relativePath, Index = clipIndex, Weight = 100.0f
                };
                baseBindings.Add(mergedBinding);
                blendShapeClip.Values = baseBindings.ToArray();
                clipIndex++;
            }
            
            _TargetMesh.sharedMesh = newMesh;
            AssetDatabase.SaveAssets();
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
    }
}
