using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Branch : MonoBehaviour
{
    List<BranchNode> _branchNodes;
    private Mesh _mesh;
    private Material _material;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    private float _branchRadius;
    private int _meshFaces;


    private void Awake()
    {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();

    }

    public MeshFilter init(List<BranchNode> branchNodes, float branchRadius, int meshFaces, Material material)
    { 
        _branchNodes = branchNodes;
        _branchRadius = branchRadius;
        _meshFaces = meshFaces;
        _material = new Material(material);
       // _meshManager = meshManager;
       if(_meshRenderer!=null && _material!=null)
            _meshRenderer.sharedMaterial = _material;
        _mesh = CreateMesh(_branchNodes);
        if(_mesh!=null && _meshFilter !=null)
            _meshFilter.sharedMesh = _mesh;

        return _meshFilter;

    }


    private Mesh CreateMesh(List<BranchNode> nodes)
    {

        Mesh branchMesh = new Mesh();

        Vector3[] vertices = new Vector3[(nodes.Count) * _meshFaces * 4];
        Vector3[] normals = new Vector3[nodes.Count * _meshFaces * 4];
        Vector2[] uv = new Vector2[nodes.Count * _meshFaces * 4];
        int[] triangles = new int[(nodes.Count - 1) * _meshFaces * 6];

        for (int i = 0; i < nodes.Count; i++)
        {
            float vStep = (2f * Mathf.PI) / _meshFaces;

            var fw = Vector3.zero;
            if (i > 0)
            {
                fw = _branchNodes[i - 1].getPosition() - _branchNodes[i].getPosition();
            }

            if (i < _branchNodes.Count - 1)
            {
                fw += _branchNodes[i].getPosition() - _branchNodes[i + 1].getPosition();
            }

            if (fw == Vector3.zero)
            {
                fw = Vector3.forward;
            }

            fw.Normalize();

            

            var up = _branchNodes[i].getNormal();
            up.Normalize();

            for (int v = 0; v < _meshFaces; v++)
            {
                var orientation = Quaternion.LookRotation(fw, up);
                Vector3 xAxis = Vector3.up;
                Vector3 yAxis = Vector3.right;
                Vector3 pos = _branchNodes[i].getPosition();
                pos += orientation * xAxis * (_branchRadius * Mathf.Sin(v * vStep));
                pos += orientation * yAxis * (_branchRadius * Mathf.Cos(v * vStep));

                vertices[i * _meshFaces + v] = pos;

                var diff = pos - _branchNodes[i].getPosition();
                normals[i * _meshFaces + v] = diff / diff.magnitude;

                float uvID = Remap(i, 0, nodes.Count - 1, 0, 1);
                uv[i * _meshFaces + v] = new Vector2((float)v / _meshFaces, uvID);
            }

            if (i + 1 < nodes.Count)
            {
                for (int v = 0; v < _meshFaces; v++)
                {
                    triangles[i * _meshFaces * 6 + v * 6] = ((v + 1) % _meshFaces) + i * _meshFaces;
                    triangles[i * _meshFaces * 6 + v * 6 + 1] = triangles[i * _meshFaces * 6 + v * 6 + 4] = v + i * +_meshFaces;
                    triangles[i * _meshFaces * 6 + v * 6 + 2] = triangles[i * _meshFaces * 6 + v * 6 + 3] = ((v + 1) % _meshFaces + _meshFaces) + i * _meshFaces;
                    triangles[i * _meshFaces * 6 + v * 6 + 5] = (_meshFaces + v % _meshFaces) + i * _meshFaces;
                }
            }
        }

        branchMesh.vertices = vertices;
        branchMesh.triangles = triangles;
        branchMesh.normals = normals;
        branchMesh.uv = uv;
        return branchMesh;
    }

    float Remap(float input, float oldLow, float oldHigh, float newLow, float newHigh)
    {
        float t = Mathf.InverseLerp(oldLow, oldHigh, input);
        return Mathf.Lerp(newLow, newHigh, t);
    }

    private void OnDrawGizmos()
    {
        if (_branchNodes != null)
        {
            for (int i = 0; i < _branchNodes.Count; i++)
            {
               // Debug.Log($"Branch {i}: {_branchNodes[i].getPosition()}");
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_branchNodes[i].getPosition(), 0.002f);
                Gizmos.color = Color.blue;

                var fw = Vector3.zero;
                if (i > 0)
                {
                    fw = _branchNodes[i - 1].getPosition() - _branchNodes[i].getPosition();
                }

                if (i < _branchNodes.Count - 1)
                {
                    fw += _branchNodes[i].getPosition() - _branchNodes[i + 1].getPosition();
                }

                fw.Normalize();

                var up = _branchNodes[i].getNormal();
                up.Normalize();

                Vector3.OrthoNormalize(ref up, ref fw);

                float vStep = (2f * Mathf.PI) / _meshFaces;
                for (int v = 0; v < _meshFaces; v++)
                {

                    Gizmos.DrawLine(_branchNodes[i].getPosition(), _branchNodes[i].getPosition() + fw * .05f);

                    var orientation = Quaternion.LookRotation(fw, up);
                    Vector3 xAxis = Vector3.up;
                    Vector3 yAxis = Vector3.right;
                    Vector3 pos = _branchNodes[i].getPosition();
                    pos += orientation * xAxis * ((_branchRadius*1.5f) * Mathf.Sin(v * vStep));
                    pos += orientation * yAxis * ((_branchRadius*1.5f) * Mathf.Cos(v * vStep));

                    
                    Gizmos.DrawSphere(pos, .002f);
                }
            }
        }
    }



}
