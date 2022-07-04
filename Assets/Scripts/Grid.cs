using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    
    [SerializeField] private Vector2 _gridWordSize;
    [SerializeField] private LayerMask _unWalkableMask;
    [SerializeField] private float _nodeRadius = 0.5f;
    private Node[,] _grid;

    //node 直径
    private float _nodeDiameter;
    private int _gridSizeX, _gridSizeY;
    
    private void Start()
    {
        _nodeDiameter = _nodeRadius * 2;
        _gridSizeX =Mathf.RoundToInt (_gridWordSize.x / _nodeDiameter);//每行有几个Grid
        _gridSizeY =Mathf.RoundToInt (_gridWordSize.y / _nodeDiameter);//每列有几个Grid

        CreateGrid();

    }

    private void CreateGrid()
    {
        _grid = new Node[_gridSizeX, _gridSizeY];
        //获得左下角坐标  平面 中心点 - 右 x  _gridWordSize.x / 2　- 前　x _gridWordSize.y/2;
        Vector3 worldBottomLeft = transform.position - Vector3.right * _gridWordSize.x / 2 - Vector3.forward * _gridWordSize.y/2;

        for (int x = 0; x < _gridSizeX; x++)
        {
            for (int y = 0; y < _gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiameter + _nodeRadius)
                    + Vector3.forward * (y * _nodeDiameter + _nodeRadius); //grid中心点位置所以要加半径
                bool walkable = !(Physics.CheckSphere(worldPoint, _nodeRadius, _unWalkableMask));
                _grid[x, y] = new Node(walkable, worldPoint);
            }
        }
    }

    public Node GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        //当前位置 / _gridWordSize
        float percentX =  Mathf.Clamp01((worldPosition.x + _gridWordSize.x / 2) / _gridWordSize.x);
        float percentY =  Mathf.Clamp01((worldPosition.z + _gridWordSize.y / 2) / _gridWordSize.y);

        int x = Mathf.RoundToInt ((_gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt ((_gridSizeY - 1) * percentY);

        return _grid[x, y];
    }
    
#if UNITY_EDITOR
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3( _gridWordSize.x,1,_gridWordSize.y));

        if (_grid != null)
        {
            Node playerNode = GetNodeFromWorldPoint(_playerTransform.position);
            
            foreach (var node in _grid)
            {
                Gizmos.color = node.Walkable ? Color.white : Color.red;
                if (playerNode == node)
                {
                    Gizmos.color = Color.cyan;
                }
                Gizmos.DrawCube(node.WorldPosition,Vector3.one * (_nodeDiameter - 0.1f));                
            }
        }
    }
#endif
}