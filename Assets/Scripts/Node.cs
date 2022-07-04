

using UnityEngine;

public class Node
{
        public Node Parent;
        public bool Walkable;
        public Vector3 WorldPosition;
        public int GridX;
        public int GridY;

        /// <summary>
        /// G Cost
        /// </summary>
        public int G;
        /// <summary>
        /// H Cost
        /// </summary>
        public int H;
        public int F => G + H;
        
        public Node(bool walkable, Vector3 worldPosition,int gridX, int gridY)
        {
                Walkable = walkable;
                WorldPosition = worldPosition;
                GridX = gridX;
                GridY = gridY;
        }
}
