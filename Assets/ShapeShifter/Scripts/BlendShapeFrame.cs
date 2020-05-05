using System;
using UnityEngine;

namespace ShapeShifter.Scripts
{
    [Serializable]
    public class BlendShapeFrame
    {
        [SerializeField] public string name;
        [SerializeField] public int shapeIndex;
        [SerializeField] public int frameIndex;
        [SerializeField] public Vector3[] vertices;
        [SerializeField] public Vector3[] normals;
        [SerializeField] public Vector3[] tangents;

        public BlendShapeFrame(int vertexCount)
        {
            InitVectors(vertexCount);
        }

        public BlendShapeFrame(Mesh mesh, int shapeIndex, int frameIndex)
        {
            GetVertices(mesh, shapeIndex, frameIndex);
        }

        private void InitVectors(int vertexCount)
        {
            vertices = new Vector3[vertexCount];
            normals = new Vector3[vertexCount];
            tangents = new Vector3[vertexCount];
        }

        public void GetVertices(Mesh mesh, int shapeIndex, int frameIndex)
        {
            InitVectors(mesh.vertexCount);
            name = mesh.GetBlendShapeName(shapeIndex);
            this.shapeIndex = shapeIndex;
            this.frameIndex = frameIndex;
            mesh.GetBlendShapeFrameVertices(shapeIndex, frameIndex, vertices, normals, tangents);
        }
    }
}
