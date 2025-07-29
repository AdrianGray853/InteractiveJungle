using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallexBg : MonoBehaviour
{
    public float Speed;
    private MeshRenderer _meshRenderer;

    public void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        if (StateManager.IsGamePlaying())
        {
            _meshRenderer.material.mainTextureOffset += new Vector2(Speed * Time.deltaTime, 0);   
        }
    }
}
