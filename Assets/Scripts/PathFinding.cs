using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;

public class PathFinding : MonoBehaviour
{

    private Grid _grid;
    private PathRequestManager _requestManager;

        private void Awake()
        {
            _requestManager = GetComponent<PathRequestManager>();
                _grid = GetComponent<Grid>();
        }


        public void StartFindPath(Vector3 starPos, Vector3 targetPos)
        {
            StartCoroutine(FindPath(starPos,targetPos));
        }

        IEnumerator FindPath(Vector3 starPos, Vector3 targetPos)
        {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                Vector3[] wayPoints = new Vector3[0];
                bool pathSuccessl = false;
            
                Node startNode = _grid.GetNodeFromWorldPoint(starPos);
                Node targetNode = _grid.GetNodeFromWorldPoint(targetPos);

                //如果可以走才执行下面的操作
                if (startNode.Walkable && targetNode.Walkable)
                {
                    //要评估的节点集
                    Heap<Node> openSet = new Heap<Node>(_grid.MaxSize);
                    // 已经被评估的节点集
                    HashSet<Node> closedSet = new HashSet<Node>(); //HashSet no order 不允许重复 
                    // add the start node to Open
                    openSet.Add(startNode);

                    //Loop
                    while (openSet.Count > 0)
                    {
                        Node currentNode = openSet.RemoveFirst();
                        // Node currentNode = openSet[0];
                        // for (int i = 1; i < openSet.Count; i++)
                        // {
                        //     //如果检查的F值小于当前NodeF值 替换它  等于的话 取 H最小
                        //     if (openSet[i].F < currentNode.F　|| openSet[i].F  == currentNode.F &&openSet[i].H < currentNode.H)
                        //     {
                        //         //current = node in Open with lowest f_cost lowest h_cost 
                        //         currentNode = openSet[i];
                        //     }
                        // }
                        // //remove current from Open
                        // openSet.Remove(currentNode);
                        // add current to Closed
                        closedSet.Add(currentNode);
                        //  if current is target node // 找到路径了
                        if (currentNode == targetNode)
                        {
                            stopwatch.Stop();
                            print($"Path Found: {stopwatch.ElapsedMilliseconds}  ms");
                            //Found Path
                            pathSuccessl = true;
                            //跳出循环
                            break;
                        }

                        foreach (var neighbour in _grid.GetNeighbours(currentNode))
                        {
                            //if neighbour is not traversable or neighbour is in Closed
                            if (!neighbour.Walkable || closedSet.Contains(neighbour))
                            {
                                continue;
                            }

                            //新的节点G Cost
                            int newMovementCostToNeighbour = currentNode.G + GetDistance(currentNode, neighbour);
                            // if new path to neighbour is shorter OR neighbour is not in Open
                            // 如果现在的NeighbourG cost <  neighbour.G(初始是0) Or  neighbour 不在 要评估节点集里面

                            if (newMovementCostToNeighbour < neighbour.G || !openSet.Contains(neighbour))
                            {
                                neighbour.G = newMovementCostToNeighbour;
                                neighbour.H = GetDistance(neighbour, targetNode);
                                //设置父节点
                                neighbour.Parent = currentNode;
                                if (!openSet.Contains(neighbour))
                                {
                                    //如果要检测的Node里没有这个邻居 添加进去
                                    openSet.Add(neighbour);
                                }
                                else
                                {
                                    //如果有要更新数值
                                    openSet.UpdateItem(neighbour);
                                }
                            }
                        }
                    }
                }
                //等待下一帧
                yield return null;

                if (pathSuccessl)
                {
                   wayPoints = RetracePath(startNode, targetNode);
                }
                _requestManager.FinishedProcessingPath(wayPoints,pathSuccessl);
        }
        
        Vector3[] RetracePath(Node startNode, Node endNote)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNote;
            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }
            Vector3[] waypoints = SimplifyPath(path);
            Array.Reverse(waypoints); 
            return waypoints;
        }

        /// <summary>
        /// 检测Node之间路径方向
        /// 相同方向不会添加到waypoint
        /// </summary>
        /// <param name="path">从终点到起点的Node集合</param>
        /// <returns></returns>
        Vector3[] SimplifyPath(List<Node> path)
        {
            List<Vector3> wayPoints = new List<Vector3>();
             
            Vector2 directionOld = Vector2.zero;
            for (int i = 1; i < path.Count; i++)
            {
                Vector2 directionNew = new Vector2(path[i - 1].GridX - path[i].GridX,
                    path[i - 1].GridY - path[i].GridY);
                if (directionNew != directionOld)
                {
                    //如果方向不同 添加到wayPoints列表里
                    wayPoints.Add(path[i].WorldPosition);
                }
                directionOld = directionNew;
            }

            return wayPoints.ToArray();
        }

        int GetDistance(Node nodeA, Node nodeB)
        {
            //14 low + 10(high - low)
            int distanceX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
            int distanceY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

            if (distanceX > distanceY)
            {
                return 14 * distanceY + 10 * (distanceX - distanceY);
            }

            return 14 * distanceX + 10 * (distanceY - distanceX);
        }

      
}