

using UnityEngine;

public class Node : IHeapItem<Node>
{
        public Node Parent;
        public bool Walkable;
        public Vector3 WorldPosition;
        public int GridX;
        public int GridY;
        /// <summary>
        /// 移动惩罚值
        /// </summary>
        public int MovementPenalty;

        /// <summary>
        /// G Cost
        /// </summary>
        public int G;
        /// <summary>
        /// H Cost
        /// </summary>
        public int H;
        public int F => G + H;

        private int heapIndex;
        
        public Node(bool walkable, Vector3 worldPosition,int gridX, int gridY)
        {
                Walkable = walkable;
                WorldPosition = worldPosition;
                GridX = gridX;
                GridY = gridY;
        }

        public int CompareTo(Node other)
        {
                int compare = F.CompareTo(other.F);
                if (compare == 0)
                {
                        compare = H.CompareTo(other.H);
                }

                return -compare;
        }

        public int HeapIndex
        {
                get { return heapIndex; }
                set { heapIndex = value; }
        }
}

