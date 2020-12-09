using System.Collections.Generic;
using UnityEngine;

namespace ShapeShifter.Scripts
{
    public class ShapeShifter
    {
        protected SkinnedMeshRenderer _smr;
        protected readonly Mesh _srcMesh;

        public ShapeShifter(SkinnedMeshRenderer smr)
        {
            _smr = smr;
            _srcMesh = smr.sharedMesh;
        }

        protected List<BlendShapeFrame> GetBlendShapeFrames(int blendShapeCount)
        {
            var blendShapeFrames = new List<BlendShapeFrame>();
            for (int i = 0; i < blendShapeCount; i++)
            {
                // blendShapeFrameCount を取得する (大抵1である)
                var bsc = _srcMesh.GetBlendShapeFrameCount(i);
                for (int j = 0; j < bsc; j++)
                {
                    var frame = new BlendShapeFrame(_srcMesh, i, j);
                    blendShapeFrames.Add(frame);
                }
            }
            return blendShapeFrames;
        }
        public void ReplaceMeshes()
        {
            /*
            var blendShapeFrames = GetBlendShapeFrames(_srcMesh.blendShapeCount);

            var newMesh = Object.Instantiate(_srcMesh);
            newMesh.name = newMesh.name.Substring(0, newMesh.name.IndexOf('('));
            newMesh.ClearBlendShapes();
            */

            // ToDo: Imprement Replace Meshes
        }
        
    }

    public class TargetBlendShape
    {
        public string Name;
        public string MeshName;
        public List<BlendRatio> BlendRatios;
    }

    public class BlendRatio
    {
        public int Index;
        public float Weight;
    }
}