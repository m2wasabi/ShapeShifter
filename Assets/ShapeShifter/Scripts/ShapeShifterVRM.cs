using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VRM;

namespace ShapeShifter.Scripts
{
    public class ShapeShifterVRM : ShapeShifter
    {
        public ShapeShifterVRM(SkinnedMeshRenderer smr) : base(smr){}

        public Mesh BuildMesh(List<BlendShapeClip> blendShapeClips)
        {
            var blendShapeFrames = GetBlendShapeFrames(_srcMesh.blendShapeCount);

            var newMesh = Object.Instantiate(_srcMesh);
            newMesh.name = newMesh.name.Substring(0, newMesh.name.IndexOf('('));
            newMesh.ClearBlendShapes();

            var clipIndex = 0;
            foreach (var blendShapeClip in blendShapeClips)
            {
                // フォームが空の場合は飛ばす
                if (blendShapeClip == null || blendShapeClip.Values == null) continue;

                var vCount = _srcMesh.vertexCount;
                var vertices = new Vector3[vCount];
                var normals = new Vector3[vCount];
                var tangents = new Vector3[vCount];
                
                var blendRatio = new List<BlendRatio>();
                var baseBindings = new List<BlendShapeBinding>();
                var relativePath = "";
                foreach (var value in blendShapeClip.Values)
                {
                    if (Path.GetFileName(value.RelativePath) != _smr.name)
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

            return newMesh;
        }
        
    }
}