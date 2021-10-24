using System;
using System.Collections;
using System.Threading;
using Constdef;
using DG.Tweening;
using Managers.MapManager;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;


public class Player : MonoBehaviour
{
    // attribute
    public float moveSpeed = 3f;            // 水平移动速度
    private StarA.Point _resultPoint;
    private StarA.Point _playerPoint;

    private Animator _animator;
    // attribute
    public int IsTurnAround =-1;
    public int IsBack = -1;
    public int IsFront = -1;
    public int IsSide = -1;

    public bool Left = true;
    private void Start()
    {
        EventCenter.GetInstance().AddEventListener<StarA.Point>(Common.PlayerMoveEvent, MoveTo);
        _animator = GetComponentInChildren<Animator>();
        _animator.speed = 0;
        IsTurnAround = Animator.StringToHash ("IsTurnAround");
        IsBack = Animator.StringToHash ("IsBack");
        IsFront = Animator.StringToHash ("IsFront");
        IsSide = Animator.StringToHash("IsSide");
    }

    private Vector3 ConvertScreenToWorldPoint(Vector3 screenPoint)
    {
        if (Camera.main == null)
        {
            utils.LogUtil.LogError(Constdef.ConstDefine.CameraNullError);
            return transform.position;  // 出错返回自身坐标 即不移动
        }
        var transformScreen = Camera.main.WorldToScreenPoint(transform.position);
        screenPoint.z = transformScreen.z;
        var resultPoint = Camera.main.ScreenToWorldPoint(screenPoint);
        return resultPoint;
    }
        
    private void MoveTo(StarA.Point point)
    {
        if (transform == null || Camera.main == null)
        {
            return;
        }

        if (_playerPoint == null)
        {
            Debug.LogWarning("playerPoint is null");
            return;
        }
        MapManager.GetInstance().ReInit();
        MapManager.GetInstance().FindResult(_playerPoint, point);
        _resultPoint = point;
    }
    
    private void Update()
    {
        if (_playerPoint == null)
        {
            _playerPoint = MapManager.GetInstance().GetPoint(0,  0);
        }
        if (_resultPoint != null && _playerPoint != _resultPoint && _playerPoint.child != null)
        {
            _animator.SetBool(IsFront, true);
            var step = moveSpeed * Time.deltaTime; 
            var pointPosition = new Vector3(_playerPoint.child.Layout.x, transform.position.y, _playerPoint.child.Layout.z);
            // Debug.Log("move to " + pointPosition + "   index = "+_playerPoint.child.x+_playerPoint.child.y);
            bool left = pointPosition.x < transform.localPosition.x;
            
            if (pointPosition.z > transform.localPosition.z && Math.Abs(pointPosition.z - transform.localPosition.z) > 0.1f)
            {
                _animator.SetBool(IsTurnAround, true);
                _animator.SetBool(IsBack, true);
                _animator.SetBool(IsFront, false);
                _animator.SetBool(IsSide, false);
            }
            else if (pointPosition.z < transform.localPosition.z && Math.Abs(pointPosition.z - transform.localPosition.z) > 0.1f)
            {
                _animator.SetBool(IsTurnAround, true);
                _animator.SetBool(IsFront, true);
                _animator.SetBool(IsBack, false);
                _animator.SetBool(IsSide, false);

            }

            Debug.Log(Math.Abs(pointPosition.z - transform.localPosition.z));
            if (Math.Abs(pointPosition.z - transform.localPosition.z) < 0.1f)
            {
                _animator.SetBool(IsTurnAround, true);
                _animator.SetBool(IsBack, false);
                _animator.SetBool(IsFront, false);
                _animator.SetBool(IsSide, true);

                if (left && !Left)
                {
                    Left = true;
                    transform.DOFlip();
                }
                else if (!left && Left)
                {
                    Left = false;
                    transform.DOFlip();
                }
            }
            gameObject.transform.localPosition = Vector3.MoveTowards(gameObject.transform.localPosition, pointPosition, step);
            _animator.speed = 1;
            if (transform.position == pointPosition)
            {
                _playerPoint = _playerPoint.child;
            }
        }
        else
        {
            _animator.speed = 0;
            // _animator.SetBool(IsMove, false);
        }
    }

}