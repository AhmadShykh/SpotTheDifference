using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Experiment2 : MonoBehaviour
{
	public Texture2D originalImage;
	public RawImage imageDisplay;
	public int eraserRadius = 20;

	void Start()
	{
		ApplyMagicEraser();
	}

	void ApplyMagicEraser()
    {
        // Create a copy of the original image
        Texture2D modifiedImage = new Texture2D(originalImage.width, originalImage.height);
        Color[] pixels = originalImage.GetPixels();

        // Randomly select a center point within the image bounds
        int centerX = Random.Range(eraserRadius, originalImage.width - eraserRadius);
        int centerY = Random.Range(eraserRadius, originalImage.height - eraserRadius);

        // Sample colors from outside the eraser radius
        Color[] sampledColors = SampleColors(centerX, centerY, pixels);

        // Fill the erased region with the sampled colors
        for (int x = centerX - eraserRadius; x <= centerX + eraserRadius; x++)
        {
            for (int y = centerY - eraserRadius; y <= centerY + eraserRadius; y++)
            {
                // Check if the pixel is within the circle defined by the eraser radius
                if ((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY) <= eraserRadius * eraserRadius)
                {
                    // Calculate the coordinates of the corresponding pixel in the original image
                    int originalX = Mathf.Clamp(x, 0, originalImage.width - 1);
                    int originalY = Mathf.Clamp(y, 0, originalImage.height - 1);

					// Set the color of the pixel in the modified image to the sampled color
					pixels[originalY * originalImage.width + originalX] = pixels[originalY * originalImage.width + (originalX+2*eraserRadius + 3)];
                }
            }
        }
        modifiedImage.SetPixels(pixels);
        // Apply changes to the modified image
        modifiedImage.Apply();

        // Display the modified image
        imageDisplay.texture = modifiedImage;
    }

    Color[] SampleColors(int centerX, int centerY, Color[] pixels)
    {
        // Sample colors from outside the eraser radius
        List<Color> sampledColors = new List<Color>();

        for (int x = 0; x < originalImage.width; x++)
        {
            for (int y = 0; y < originalImage.height; y++)
            {
                // Check if the pixel is outside the circle defined by the eraser radius
                if ((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY) > eraserRadius * eraserRadius)
                {
                    // Calculate the coordinates of the corresponding pixel in the original image
                    int originalX = Mathf.Clamp(x, 0, originalImage.width - 1);
                    int originalY = Mathf.Clamp(y, 0, originalImage.height - 1);

                    // Add the color of the pixel to the list of sampled colors
                    sampledColors.Add(pixels[originalY * originalImage.width + originalX]);
                }
            }
        }

        return sampledColors.ToArray();
    }
}
