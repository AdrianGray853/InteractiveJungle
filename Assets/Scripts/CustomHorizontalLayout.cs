using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomHorizontalLayout : MonoBehaviour
{
    [SerializeField] private float spacing = 10.0f; // Adjust the horizontal spacing between sprite renderers
    
    // private void Start()
    // {
    //     ArrangeSpriteRenderersHorizontally();
    // }

    private void Update()
    {
        ArrangeSpriteRenderersHorizontally();
    }

    private void ArrangeSpriteRenderersHorizontally()
    {
        float totalWidth = 0.0f;

        // Iterate through the active child sprite renderers to calculate the total width
        foreach (Transform child in transform)
        {
            if (!child.gameObject.activeSelf)
                continue; // Ignore disabled children

            SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                totalWidth += spriteRenderer.bounds.size.x;
            }
        }

        // Calculate the starting X position to center the sprites
        float xOffset = -totalWidth / 2.0f;

        // Iterate through the active child sprite renderers again to set their positions
        foreach (Transform child in transform)
        {
            if (!child.gameObject.activeSelf)
                continue; // Ignore disabled children

            SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                // Adjust the sprite renderer's position
                Vector3 newPosition = child.localPosition;
                newPosition.x = xOffset;
                child.localPosition = newPosition;

                // Update the xOffset for the next sprite renderer
                xOffset += spriteRenderer.bounds.size.x + spacing;
            }
        }
    }
}
