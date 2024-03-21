using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class Experiment : MonoBehaviour
{
	[SerializeField] UnityEngine.UI.Image _image;
	[SerializeField] List<Button> _allDifferenceButtons = new List<Button>();
	[SerializeField] Canvas _canvas;
	[SerializeField] float threshold = 1.5f;


	private Color[] _checkingColor = new Color[2];
	//private List<int> _circlePoints = new List<int,int>();	
	private void Start()
	{
		_checkingColor[0] = new Color(209/255f, 93/255f, 255/255f);
		_checkingColor[1] = new Color((float)0 / 255, (float)255 / 255, (float)43 / 255, (float)255 / 255); ;
		CreateButtons();
	}

	private void CreateButtons()
	{
		Texture2D imageTexture = _image.sprite.texture;
		Color[,] imagePixels = new Color[imageTexture.width, imageTexture.height];
		for(int i = 0; i < imageTexture.width; i++)
		{
			for(int j = 0; j < imageTexture.height; j++)
			{
				imagePixels[i, j] = imageTexture.GetPixel(i, j);	
			}
		}
		bool stop = false; 
		for (int i = 0; i < imageTexture.width; i++)
		{
			for (int j = 0; j < imageTexture.height; j++)
			{
				if (getColorDistance(imagePixels[i,j] , _checkingColor[0]) < threshold )
				{
					InstantiateButtons(imagePixels, i, j);
					stop = true;
					break;
				}
				
			}
			if (stop)
				break;
		}
	}
	private static double getColorDistance(Color c1, Color c2)
	{
		double redDiff = c1.r - c2.r;
		double greenDiff = c1.g - c2.g;
		double blueDiff = c1.b - c2.b;
		return Math.Sqrt(redDiff * redDiff + greenDiff * greenDiff + blueDiff * blueDiff);
	}
	private void InstantiateButtons(Color[,] imagePixels, int btnCenterX, int btnCenterY)
	{
		int btnWidth = 0, btnHeight = 0;
		for(int i = btnCenterY; i < imagePixels.GetLength(1); i++, btnHeight++)
		{
			if (imagePixels[btnCenterX,i] == _checkingColor[1])
			{
				break;
			}
		}
		for (int i = btnCenterY; i > 0; i--, btnHeight++)
		{
			if (imagePixels[btnCenterX, i] == _checkingColor[1])
			{
				break;
			}
		}
		for (int i = btnCenterX; i < imagePixels.GetLength(0); i++, btnWidth++)
		{
			if (imagePixels[i, btnCenterY] == _checkingColor[1])
			{
				break;
			}
		}
		for (int i = btnCenterX; i > 0; i--, btnWidth++)
		{
			if (imagePixels[i, btnCenterY] == _checkingColor[1])
			{
				break;
			}
		}

		GameObject obj = new GameObject();
		RectTransform rectTransfor  = obj.AddComponent<RectTransform>();
		rectTransfor.SetParent(_canvas.transform);

		Button btn = obj.AddComponent<Button>();
		btn.GetComponent<RectTransform>().sizeDelta = new Vector2(btnWidth, btnHeight);
		_allDifferenceButtons.Add(btn);
	}
}
