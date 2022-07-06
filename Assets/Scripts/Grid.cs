using System;
using System.Collections;
using System.Collections.Generic;
using OpenCover.Framework.Model;
using UnityEngine;

namespace Astar
{
    public class Grid : MonoBehaviour
    {
        [SerializeField] private bool _disPlayGridGizmos;

        [SerializeField] private Vector2 _gridWordSize;
        [SerializeField] private LayerMask _unWalkableMask;
        [SerializeField] private float _nodeRadius = 0.5f;
        
        /// <summary>
        /// walk regions
        /// </summary>
        [SerializeField] private TerrainType[] _walkableRegions;

        [SerializeField] private int _obstacleProximityPenalty = 10;
        
        private LayerMask _walkableLayerMask;
        private Dictionary<int, int> _walkableRegionsDictionary = new Dictionary<int, int>();

        private Node[,] _grid;

        int _penaltyMin = Int32.MaxValue;
        int _penaltyMax = Int32.MinValue;


        //node 直径
        private float _nodeDiameter;
        private int _gridSizeX, _gridSizeY;

        public int MaxSize => _gridSizeX * _gridSizeY;

        private void Awake()
        {
            _nodeDiameter = _nodeRadius * 2;
            _gridSizeX = Mathf.RoundToInt(_gridWordSize.x / _nodeDiameter); //每行有几个Grid
            _gridSizeY = Mathf.RoundToInt(_gridWordSize.y / _nodeDiameter); //每列有几个Grid

            foreach (var walkRegion in _walkableRegions)
            {
                _walkableLayerMask.value |= walkRegion.TerrainMask;
                _walkableRegionsDictionary.Add((int)Mathf.Log(walkRegion.TerrainMask.value,2) ,walkRegion.TerrainPenalty);
            }
            
            CreateGrid();

        }

        /// <summary>
        /// Create Grid
        /// </summary>
        private void CreateGrid()
        {
            _grid = new Node[_gridSizeX, _gridSizeY];
            //获得左下角坐标  平面 中心点 - 右 x  _gridWordSize.x / 2　- 前　x _gridWordSize.y/2;
            Vector3 worldBottomLeft = transform.position - Vector3.right * _gridWordSize.x / 2 -
                                      Vector3.forward * _gridWordSize.y / 2;

            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiameter + _nodeRadius)
                                                         + Vector3.forward *
                                                         (y * _nodeDiameter + _nodeRadius); //grid中心点位置所以要加半径
                    //检测区域是否可以行走
                    bool walkable = !(Physics.CheckSphere(worldPoint, _nodeRadius, _unWalkableMask));

                    int movementPenalty = 0;

                    //Raycast
                    Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                    RaycastHit hit;
                    if(Physics.Raycast(ray,out hit,100,_walkableLayerMask))
                    {
                        _walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                    }

                    if (!walkable)
                    {
                        movementPenalty += _obstacleProximityPenalty;
                    }
                    
                    _grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
                }
            }
            
            BlurPenaltyMap(3);
        }

        void BlurPenaltyMap(int blurSize)
        {
            int kernelSize = blurSize * 2 - 1;
            int kernelExtents = (kernelSize - 1) /2;

            int[,] penaltyHorizontalPass = new int[_gridSizeX, _gridSizeY];
            int[,] penaltyVerticalPass = new int[_gridSizeX, _gridSizeY];
            
                //横向计算
            for (int y = 0; y < _gridSizeY; y++)
            {
                for (int x =-kernelExtents; x <= kernelExtents; x++)
                {
                    // 3格的kernel -1到1 sampleX(0,1)
                    int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                    //每一行 第一格的 三个Kernel 和值
                    penaltyHorizontalPass[0, y] += _grid[sampleX, y].MovementPenalty;
                }

                //通过每一行第一格的和值 计算 之后所有的和值
                // 旧值 - 去除掉的格子的值 + 新加的格子的值
                for (int x = 1; x < _gridSizeX; x++)
                {
                    // 防止超出索引
                    int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, _gridSizeX);
                    int addIndex =  Mathf.Clamp(x + kernelExtents,0,_gridSizeX -1);
                    //kernelSize = 3的情况 和的新值[x,y] = 和的旧值[x-1,y]- 前两格[x-2.y] + 后一个[x+1,y] //
                    penaltyHorizontalPass[x, y] =
                        penaltyHorizontalPass[x- 1, y] - _grid[removeIndex, y].MovementPenalty + _grid[addIndex,y].MovementPenalty;
                }
            }
            //横向计算
            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = -kernelExtents; y <= kernelExtents; y++)
                {
                    int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                    //每一列 第一格的 三个Kernel 和值
                    //从 横向 采样
                    penaltyVerticalPass[x, 0] += penaltyHorizontalPass[x, sampleY];
                }
                
                int blurredPenalty =
                    Mathf.RoundToInt((float)penaltyVerticalPass[x, 0] / (kernelSize * kernelSize));
                _grid[x, 0].MovementPenalty = blurredPenalty;
                //通过每一列 第一格的和值 计算 之后所有的和值
                // 旧值 - 去除掉的格子的值 + 新加的格子的值
                for (int y = 1; y < _gridSizeY; y++)
                {
                    // 防止超出索引
                    int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, _gridSizeY);
                    int addIndex = Mathf.Clamp(y + kernelExtents, 0, _gridSizeY - 1);
                    // kernelSize = 3的情况 和的新值[x,y] = 喝的旧值[x,y-1] - 前两格[x,y-2] + 后一个[x,y+1] //
                    penaltyVerticalPass[x, y] =
                        penaltyVerticalPass[x, y-1] - penaltyHorizontalPass[x, removeIndex] +
                        penaltyHorizontalPass[x, addIndex];

                    blurredPenalty =
                        Mathf.RoundToInt((float)penaltyVerticalPass[x, y] / (kernelSize * kernelSize));
                    _grid[x, y].MovementPenalty = blurredPenalty;

                    if (blurredPenalty > _penaltyMax)
                    {
                        _penaltyMax = blurredPenalty;
                    }

                    if (blurredPenalty < _penaltyMin)
                    {
                        _penaltyMin = blurredPenalty;
                    }
                }
            }
        }

        public List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.GridX + x;
                    int checkY = node.GridY + y;

                    if (checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY < _gridSizeY)
                    {
                        neighbours.Add(_grid[checkX, checkY]);
                    }
                }
            }

            return neighbours;
        }


        /// <summary>
        /// 根据世界坐标返回Node
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public Node GetNodeFromWorldPoint(Vector3 worldPosition)
        {
            //当前位置 在_gridWordSize里的百分比
            float percentX = Mathf.Clamp01((worldPosition.x + _gridWordSize.x / 2) / _gridWordSize.x);
            float percentY = Mathf.Clamp01((worldPosition.z + _gridWordSize.y / 2) / _gridWordSize.y);

            // Cast to Index
            int x = Mathf.RoundToInt((_gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((_gridSizeY - 1) * percentY);

            return _grid[x, y];
        }



#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(_gridWordSize.x, 1, _gridWordSize.y));

            if (_grid != null && _disPlayGridGizmos)
            {
                foreach (var node in _grid)
                {
                    Gizmos.color = Color.Lerp(Color.white, Color.black,
                        Mathf.InverseLerp(_penaltyMin, _penaltyMax, node.MovementPenalty));
                    
                    Gizmos.color = node.Walkable ? Gizmos.color : Color.red;
                    Gizmos.DrawCube(node.WorldPosition, Vector3.one * (_nodeDiameter));
                }
            }
        }
#endif

        [Serializable]
        public class TerrainType
        {
            public LayerMask TerrainMask;
            public int TerrainPenalty;
        }

    }
}
