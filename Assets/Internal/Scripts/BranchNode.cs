using UnityEngine;

public class BranchNode 
{

    private Vector3 _position;
    private Vector3 _normal;

    public BranchNode(Vector3 position, Vector3 normal)
    {
        _position = position;
        _normal = normal;
    }

    public Vector3 getPosition()=> _position;
    public Vector3 getNormal()=> _normal;
}
