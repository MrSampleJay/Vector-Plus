using Core._Common;
using DG.Tweening;
using Nekki.Vector.Core.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class ItemGO : MonoBehaviour
{
    [SerializeField]
    private string _atlas;

    [SerializeField]
    private string _atlasEnd;

    private List<Sprite> atlasSequence;

    private List<Sprite> endSequence;

    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private AnimationSprite _animationSprite;

    [SerializeField]
    private TextMesh _textMesh;

    private Vector3 _defaultTextPos;

    public string AtlasIdle {
        get => _atlas;
        set => _atlas = value;
    }

    public string AtalasOff
    {
        get => _atlasEnd;
        set => _atlasEnd = value;
    }

    public Color color
    {
        set => _spriteRenderer.color = value;
    }

    public void Init()
    {
        atlasSequence = AnimationSprite.GetFramesSequence(VectorPaths.AnimatedTextures + "/" + _atlas, 0.5f, 0.5f);
        endSequence = AnimationSprite.GetFramesSequence(VectorPaths.AnimatedTextures + "/" + _atlasEnd, 0.5f, 0.5f);

        _defaultTextPos = _textMesh.transform.localPosition;
        Reset();
    }

    public void PlayEnd(int scope)
    {
        _textMesh.text = "+" + scope;
        _animationSprite.Init(endSequence, _spriteRenderer);
        _animationSprite.Iterations = 1;
        _textMesh.gameObject.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        var tween = _textMesh.transform.DOLocalMoveY(_textMesh.transform.localPosition.y + -200, 1);
        sequence.Append(tween);
        var callback = new TweenCallback(() => _textMesh.gameObject.SetActive(false));
        sequence.AppendCallback(callback);
        sequence.Play();
    }

    public void Reset()
    {
        _animationSprite.Init(atlasSequence, _spriteRenderer);
        _animationSprite.IsWork = true;
        _animationSprite.Iterations = -1;
        _textMesh.gameObject.SetActive(false);
        _textMesh.transform.localPosition = _defaultTextPos;
    }
}
