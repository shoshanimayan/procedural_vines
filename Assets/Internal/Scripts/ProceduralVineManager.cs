using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;


public static class ListExtension
{

    public static List<T> join<T>(this List<T> first, List<T> second)
    {
        if (first == null)
        {
            return second;
        }
        if (second == null)
        {
            return first;
        }
        return first.Concat(second).ToList();
    }

}

public class ProceduralVineManager : MonoBehaviour
{

    [SerializeField] private int _branches = 3;
    [SerializeField] private int _maxBranchNodes = 10;
    [SerializeField] private float _segmentLength = 2f;
    [SerializeField] private float _branchRadius = 0.02f;
    [SerializeField] private int _branchFaces = 7;
    [Space]
    [SerializeField] private Material _branchMaterial;
    [SerializeField] private bool _combinedMeshes = true;



    private int vineCount = 0;
    private List<MeshFilter> _meshFilters = new List<MeshFilter>();


    public void CreateVines(RaycastHit rayhit)
    {
        Vector3 tangent = FindTangentFromNormal(rayhit.normal);

        GameObject vine = new GameObject("VinePlant" + vineCount.ToString());   
        vine.transform.SetParent(transform);

        for (int i = 0; i < _branches; i++)
        {
            int angle = (360 / _branches) ;
            Vector3 dir = Quaternion.AngleAxis((angle*i)+Random.Range(-angle,angle ), rayhit.normal)*tangent;
            List<BranchNode> nodes = CreateBranch(_maxBranchNodes, rayhit.point, rayhit.normal, dir);
            GameObject branchObj = new GameObject("Branch" + i.ToString());
            Branch branch = branchObj.AddComponent<Branch>();
            
            MeshFilter tempMesh = branch.init(nodes, _branchRadius, _branchFaces, _branchMaterial);
            branchObj.transform.SetParent(vine.transform);

            if (tempMesh != null)
            { 
                _meshFilters.Add(tempMesh);
            }
        
        }
        if (_combinedMeshes)
        {
            CombineMesh(_meshFilters, vine.transform, _branchMaterial);
        }
        _meshFilters.Clear();
        vineCount++;
    }

    private void CombineMesh(List<MeshFilter> meshes, Transform parentTransform, Material material)
    {
        Mesh mesh = new Mesh();
        CombineInstance[] combine = new CombineInstance[meshes.Count];

        for (int i = 0; i < meshes.Count; i++)
        {
            combine[i].mesh = meshes[i].sharedMesh;
            combine[i].transform = meshes[i].transform.localToWorldMatrix;
            _meshFilters[i].gameObject.SetActive(false);
        }

        mesh.CombineMeshes(combine);

        MeshFilter meshFilter = parentTransform.gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = parentTransform.gameObject.AddComponent<MeshRenderer>();


        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial = material;

        for (int i = 0; i < meshes.Count; i++)
        {
            Destroy(meshes[i].gameObject);
        }

    }

    private List<BranchNode> CreateBranch(int count, Vector3 pos, Vector3 normal, Vector3 direction)
    {

        if (count == _maxBranchNodes)
        {
            BranchNode rootNode = new BranchNode(pos, normal);
            return new List<BranchNode>() { rootNode }.join(CreateBranch(count-1,pos,normal,direction));
        }
        else if (count < _maxBranchNodes && count > 0)
        {

            if (count % 2 == 0)
            {
                direction = Quaternion.AngleAxis(Random.Range(-20.0f, 20.0f), normal) * direction;
            }

            RaycastHit hit;
            Ray ray = new Ray(pos, normal);
            Vector3 p1 = pos + normal * _segmentLength;

            if (Physics.Raycast(ray, out hit, _segmentLength))
            {
                p1 = hit.point;
            }
            ray = new Ray(p1, direction);

            if (Physics.Raycast(ray, out hit, _segmentLength))
            {
                Vector3 p2 = hit.point;
                BranchNode p2Node = new BranchNode(p2, -direction);
                return new List<BranchNode> { p2Node }.join(CreateBranch(count - 1, p2, -direction, normal));
            }
            else
            {
                Vector3 p2 = p1 + direction * _segmentLength;
                ray = new Ray(AdjustVector(p2, normal), -normal);
                if (Physics.Raycast(ray, out hit, _segmentLength))
                {
                    Vector3 p3 = hit.point;
                    BranchNode p3Node = new BranchNode(p3, normal);

                    if (IsOccluded(p3, pos, normal))
                    {
                        Vector3 middle = calculateMiddlePoint(p3, pos, (normal + direction) / 2);

                        Vector3 m0 = (pos + middle) / 2;
                        Vector3 m1 = (p3 + middle) / 2;

                        BranchNode m0Node = new BranchNode(m0, normal);
                        BranchNode m1Node = new BranchNode(m1, normal);

                        return new List<BranchNode> { m0Node, m1Node, p3Node }.join(CreateBranch(count - 3, p3, normal, direction));
                    }

                    return new List<BranchNode> { p3Node }.join(CreateBranch(count - 1, p3, normal, direction));
                }
                else
                {
                    Vector3 p3 = p2 - normal * _segmentLength;
                    ray = new Ray(AdjustVector(p3, normal), -normal);

                    if (Physics.Raycast(ray, out hit, _segmentLength))
                    {
                        Vector3 p4 = hit.point;
                        BranchNode p4Node = new BranchNode(p4, normal);

                        if (IsOccluded(p4, pos, normal))
                        {
                            Vector3 middle = calculateMiddlePoint(p4, pos, (normal + direction) / 2);
                            Vector3 m0 = (pos + middle) / 2;
                            Vector3 m1 = (p4 + middle) / 2;

                            BranchNode m0Node = new BranchNode(m0, normal);
                            BranchNode m1Node = new BranchNode(m1, normal);

                            return new List<BranchNode> { m0Node, m1Node, p4Node }.join(CreateBranch(count - 3, p4, normal, direction));
                        }

                        return new List<BranchNode> { p4Node }.join(CreateBranch(count - 1, p4, normal, direction));
                    }
                    else
                    {
                        Vector3 p4 = p3 - normal * _segmentLength;
                        BranchNode p4Node = new BranchNode(p4, direction);

                        if (IsOccluded(p4, pos, normal))
                        {
                            Vector3 middle = calculateMiddlePoint(p4, pos, (normal + direction) / 2);

                            Vector3 m0 = (pos + middle) / 2;
                            Vector3 m1 = (p4 + middle) / 2;

                            BranchNode m0Node = new BranchNode(m0, direction);
                            BranchNode m1Node = new BranchNode(m1, direction);

                            return new List<BranchNode> { m0Node, m1Node, p4Node }.join(CreateBranch(count - 3, p4, direction, -normal));
                        }
                        return new List<BranchNode> { p4Node }.join(CreateBranch(count - 1, p4, direction, -normal));
                    }
                }
            }

        }
        return null;

    }

    Vector3 AdjustVector(Vector3 pos, Vector3 normal)
    {
        return pos + normal * 0.01f;
    }

    private bool IsOccluded(Vector3 origin, Vector3 target, Vector3 normal) 
    { 
        Vector3 adjustedOrigin = AdjustVector(origin,normal);
        Vector3 adjustedTarget = AdjustVector(target,normal);

        Ray ray = new Ray(adjustedOrigin,(adjustedTarget-adjustedOrigin)/(adjustedTarget-adjustedOrigin).magnitude);
        return Physics.Raycast(ray, (adjustedTarget - adjustedOrigin).magnitude);
    }

    private Vector3 calculateMiddlePoint(Vector3 origin, Vector3 target, Vector3 normal)
    {
        Vector3 middle = (origin + target) / 2;
        Vector3 h = origin - target;
        float distance =h.magnitude;
        return middle + normal*distance;
    }

    private Vector3 FindTangentFromNormal(Vector3 normal)
    {
        Vector3 t1 = Vector3.Cross(normal, Vector3.forward);
        Vector3 t2 = Vector3.Cross(normal, Vector3.up);

        if (t1.magnitude > t2.magnitude)
        {
            return t1;
        }
        return t2;
    }

}
