using System;
using System.Collections.Generic;
using UnityEngine;


namespace Astar
{
    public class PathRequestManager : MonoBehaviour
    {
        /// <summary>
        /// 请求Queue 先入先出
        /// </summary>
        private Queue<PathRequest> _pathRequestQueue = new Queue<PathRequest>();

        private PathRequest _currentPathRequest;

        private static PathRequestManager _instance;
        private PathFinding _pathFinding;
        private bool _isProcessingPath;

        private void Awake()
        {
            _instance = this;
            _pathFinding = GetComponent<PathFinding>();
        }

        public static void RequestPath(Vector3 start, Vector3 end, Action<Vector3[], bool> callback)
        {
            //当新的 寻路请求来的时候
            PathRequest newRequest = new PathRequest(start, end, callback);
            // 添加到Queue里
            _instance._pathRequestQueue.Enqueue(newRequest);
            _instance.TryProcessNext();
        }

        private void TryProcessNext()
        {
            //如果没有在处理路径，且queue里由需要处理的路径
            if (!_isProcessingPath && _pathRequestQueue.Count > 0)
            {
                _currentPathRequest = _pathRequestQueue.Dequeue();
                _isProcessingPath = true;
                _pathFinding.StartFindPath(_currentPathRequest.PathStart, _currentPathRequest.PathEnd);
            }
            //如果在处理路径就等待路径处理完 路径处理完调用TryProcessNext() 对下一个请求进行处理
        }

        public void FinishedProcessingPath(Vector3[] path, bool success)
        {
            _currentPathRequest.Callback?.Invoke(path, success);
            _isProcessingPath = false;
            _instance.TryProcessNext();

        }

        struct PathRequest
        {
            public Vector3 PathStart;
            public Vector3 PathEnd;

            public Action<Vector3[], bool> Callback;

            public PathRequest(Vector3 start, Vector3 end, Action<Vector3[], bool> callback)
            {
                PathStart = start;
                PathEnd = end;
                Callback = callback;
            }
        }
    }
}