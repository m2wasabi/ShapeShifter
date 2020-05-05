using System;
using System.Collections.Generic;
using ShapeShifter.Scripts;
using UnityEngine;

public class exam : MonoBehaviour
{
    public GameObject avatar;
    private GameObject _distGo;
    private SkinnedMeshRenderer[] _smrs;
    private Mesh[] _meshes;
    private static readonly string errMessageMeshReadable
        = "Meshが読み込めません。fbxファイルの[ImportSettings] > [Model] > [Read/Write Enabled] を確認してください。";
    void Start()
    {
        if(avatar == null) return;
        _distGo = Instantiate(avatar);
        _smrs = _distGo.GetComponentsInChildren<SkinnedMeshRenderer>();
        _meshes = new Mesh[_smrs.Length];
        var smrsCount = _smrs.Length;
        for (int i = 0; i < smrsCount; i++)
        {
            if(!_smrs[i].sharedMesh.isReadable) throw new Exception(errMessageMeshReadable);
            _meshes[i] = Instantiate(_smrs[i].sharedMesh);
        }

        var mesh = _meshes[5];
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
        Debug.Log("See");

        var newMesh = Instantiate(mesh);
        newMesh.name = newMesh.name.Substring(0, newMesh.name.IndexOf('(') - 1);
        newMesh.ClearBlendShapes();
        for (int i = 0; i < 8; i++)
        {
            newMesh.AddBlendShapeFrame(
                blendShapeFrames[i].name,
                1.0f,
                blendShapeFrames[i].vertices,
                blendShapeFrames[i].normals,
                blendShapeFrames[i].tangents
                );
        }

        _smrs[5].sharedMesh = newMesh;
        Debug.Log("You");
    }
}
