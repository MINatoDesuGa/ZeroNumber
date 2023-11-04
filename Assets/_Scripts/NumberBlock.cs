using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class NumberBlock : MonoBehaviour
{
    public TMP_Text numberText;

    public int number;

    private NumberBlock _collidedNumberBlock;

    [SerializeField] private float _blockMoveDuration = 1f;
    [SerializeField] private float _blockMoveDistance = 1f;

    [HideInInspector] public Vector2 initialPosition;
    [HideInInspector] public int defaultNumber;

    private void Awake()
    {
        initialPosition = transform.localPosition;
        defaultNumber = number;
        numberText.text = number.ToString();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!GameManager.isMovingBlock) return;
        
        //Debug.Log("collision");
        _collidedNumberBlock = collision.gameObject.GetComponent<NumberBlock>();

        if( _collidedNumberBlock.number != number)
        {
            _collidedNumberBlock.number += number;
            _collidedNumberBlock.numberText.text = _collidedNumberBlock.number.ToString();
        } else
        {
            collision.gameObject.SetActive(false);
        }
        transform.gameObject.SetActive(false);
        GameManager.isMovingBlock = false;

        GameManager.instance.CheckGameOver();

    }

    public void HandleBlockMove(GameManager.MoveDirection moveDirection)
    {
        if (GameManager.isMovingBlock) return;

        GameManager.isMovingBlock = true;

        var __undoPosition = transform.localPosition;

        switch(moveDirection)
        {
            case GameManager.MoveDirection.MoveLeft:
                transform.DOLocalMoveX(transform.localPosition.x - _blockMoveDistance, _blockMoveDuration).OnComplete(() => UndoMove(__undoPosition));
                break;
            case GameManager.MoveDirection.MoveRight:
                transform.DOLocalMoveX(transform.localPosition.x + _blockMoveDistance, _blockMoveDuration).OnComplete(() => UndoMove(__undoPosition));
                break;
            case GameManager.MoveDirection.MoveUp:
                transform.DOLocalMoveY(transform.localPosition.y + _blockMoveDistance, _blockMoveDuration).OnComplete(() => UndoMove(__undoPosition));
                break;
            case GameManager.MoveDirection.MoveDown:
                transform.DOLocalMoveY(transform.localPosition.y - _blockMoveDistance, _blockMoveDuration).OnComplete(() => UndoMove(__undoPosition));
                break;
        }
    }
    private void UndoMove(Vector2 undoPosition)
    {
        if (!GameManager.isMovingBlock) return;

        GameManager.isMovingBlock = false;
        transform.DOLocalMove(undoPosition, _blockMoveDuration/2);
    }
}
