using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private ProceduralVineManager _proceduralVineManager;
    private void Awake()
    {
        _proceduralVineManager = GetComponent<ProceduralVineManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        { 
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                _proceduralVineManager.CreateVines(hit);
            }
        }
    }
}
