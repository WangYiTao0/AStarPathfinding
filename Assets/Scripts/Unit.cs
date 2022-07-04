using System;
using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
      [SerializeField] private Transform _target;
      [SerializeField]private float _speed = 5;
      private Vector3[] _path;
      /// <summary>
      /// 目标索引
      /// </summary>
      private int _targetIndex = 0;

      private void Start()
      {
          PathRequestManager.RequestPath(transform.position,_target.position,OnPathFound);
      }

      private void OnPathFound(Vector3[] newPath, bool pathSuccessful)
      {
          if (pathSuccessful)
          {
              _path = newPath;
              StopCoroutine("FollowPath");
              StartCoroutine("FollowPath");
          }
      }

      IEnumerator FollowPath()
      {
          Vector3 currentWayPoint = _path[0];
          while (true)
          {
              if (transform.position == currentWayPoint)
              {
                  //到达目标后 索引++ _path的索引
                  _targetIndex++;
                  if (_targetIndex >= _path.Length)
                  {
                      yield break;
                  }
                  //下一个Path
                  currentWayPoint = _path[_targetIndex];
              }
              // 移动
              transform.position = Vector3.MoveTowards(transform.position, currentWayPoint, _speed * Time.deltaTime);
              yield return null;
          }
      }


      private void OnDrawGizmos()
      {
          if (_path != null)
          {
              for (int i = _targetIndex; i < _path.Length; i++)
              {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(_path[1],Vector3.one);
                if (i == _targetIndex)
                {
                    Gizmos.DrawLine(transform.position,_path[i]);
                }
                else
                {
                    Gizmos.DrawLine(_path[i-1],_path[i]);
                }
              }
          }
      }
}