using System;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class PathFinding : MonoBehaviour
{
        public Transform Seeker, Target;
        
        private Grid _grid;

        private void Awake()
        {
                _grid = GetComponent<Grid>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                FindPath(Seeker.position, Target.position);
            }
        }

        void FindPath(Vector3 starPos, Vector3 targetPos)
        {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
            
                Node startNode = _grid.GetNodeFromWorldPoint(starPos);
                Node targetNode = _grid.GetNodeFromWorldPoint(targetPos);
                //要评估的节点集
                List<Node> openSet = new List<Node>();
                // 已经被评估的节点集
                HashSet<Node> closedSet = new HashSet<Node>();//HashSet no order 不允许重复 
                // add the start node to Open
                openSet.Add(startNode);
                
                //TODO Heap Optimization 二叉树 父节点 小于 两个子节点
         
                //Loop
                while (openSet.Count > 0)
                {
                    Node currentNode = openSet[0];
                    for (int i = 1; i < openSet.Count; i++)
                    {
                        //如果检查的F值小于当前NodeF值 替换它  等于的话 取 H最小
                        if (openSet[i].F < currentNode.F　|| openSet[i].F  == currentNode.F &&openSet[i].H < currentNode.H)
                        {
                            //current = node in Open with lowest f_cost lowest h_cost 
                            currentNode = openSet[i];
                        }
                    }

                    //remove current from Open
                    openSet.Remove(currentNode);
                    //add current to Closed
                    closedSet.Add(currentNode);
                    //  if current is target node // 找到路径了
                    if (currentNode == targetNode)
                    {
                        stopwatch.Stop();
                        print($"Path Found: {stopwatch.ElapsedMilliseconds}  ms") ;
                        //Find Path
                        RetracePath(startNode, targetNode);
                        return;
                    }
                    
                    foreach (var neighbour in _grid.GetNeighbours(currentNode))
                    {
                        //if neighbour is not traversable or neighboour is in Closed
                        if (!neighbour.Walkable || closedSet.Contains(neighbour))
                        {
                            continue;
                        }

                        //新的节点G Cost
                        int newMovementCostToNeighbour = currentNode.G　+ GetDistance(currentNode,neighbour);
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
                                openSet.Add(neighbour);
                            }
                        }
                    }
                }

                void RetracePath(Node startNode, Node endNote)
                {
                    List<Node> path = new List<Node>();
                    Node currentNode = endNote;
                    while (currentNode != startNode)
                    {
                        path.Add(currentNode);
                        currentNode = currentNode.Parent;
                    }
                    
                    path.Reverse();

                    _grid.Path = path;
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
                
                /*
 
                foreach neighbour of the current node
               
                        skip to next neighbour
              
                        set f_cost of neighbour
                        set parent of neighbour to current
                        if neighbour is not in Open
                            add neighbour to Open
                 */

        }
}