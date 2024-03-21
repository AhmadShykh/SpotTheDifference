
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class DifferenceDetection : MonoBehaviour, IPointerClickHandler
{
	[SerializeField] Image DiffImg;
	[SerializeField] Image SolImg;
	[SerializeField] Image OrgImg;
	[SerializeField] Image NewImg;

	[SerializeField] float checkingCircleWidth;
	[SerializeField] float checkingCircleHeight;
	[SerializeField] [Range(1, 100)] float chkThreshold;
	[SerializeField] Color checkingColor = new Color((float)1 / 255, (float)255 / 255, (float)43 / 255, (float)255 / 255);
	Color[,] DiffImgPixels;
	Color[,] SolImgPixels;
	Color[,] OrigImgPixels;
	Vector2 buttonLocationOffset;
	float rectToTextureHeightRatio;
	float rectToTextureWidthtRatio;
	void Start()
	{
		DiffImgPixels = GetPixelMatrix(DiffImg.sprite.texture);
		SolImgPixels = GetPixelMatrix(SolImg.sprite.texture);
		OrigImgPixels = GetPixelMatrix(OrgImg.sprite.texture);

		float btnHeightValue = GetComponentInParent<RectTransform>().rect.height;
		float btnWidthValue = GetComponentInParent<RectTransform>().rect.width;
		buttonLocationOffset = new Vector2(btnWidthValue / 2, btnHeightValue / 2);
		rectToTextureHeightRatio = DiffImg.sprite.texture.height / btnHeightValue;
		rectToTextureWidthtRatio = DiffImg.sprite.texture.width / btnWidthValue;
		convertToBlackAndWhite(NewImg.sprite.texture);
		ThinTexture(NewImg.sprite.texture);
	}

	private void convertToBlackAndWhite(Texture2D txt)
	{
		for (int x = 0; x < txt.width; x++)
		{
			for (int y = 0; y < txt.height; y++)
			{
				if( txt.GetPixel(x, y) == checkingColor)
				{
					txt.SetPixel(x, y, new Color(0, 0, 0));
				}
				else
				{
					txt.SetPixel(x, y, new Color(1, 1, 1));
				}
			}
		}
		txt.Apply();
	}


	//public void OnPointerClick(PointerEventData eventData)
	//{
	//	// Get the position of the click relative to the image
	//	Vector2 localPoint;
	//	RectTransformUtility.ScreenPointToLocalPointInRectangle(
	//		GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out localPoint);
	//	localPoint += buttonLocationOffset;
	//	// Convert localPoint to pixel coordinates
	//	Vector2Int pixelCoord = new Vector2Int((int)(localPoint.x * rectToTextureWidthtRatio), (int)(localPoint.y * rectToTextureHeightRatio));
	//	Debug.Log(pixelCoord);
	//	if (SolImgPixels[pixelCoord.x, pixelCoord.y] != checkingColor  )
	//	{
	//		if (GiveSimilarityPercentage(pixelCoord) > chkThreshold / 100)
	//		{
	//			Debug.Log("Difference Found Hurray");
	//			MarkClickedArea(pixelCoord);
	//			SetTextureFromMatrix(SolImgPixels);
	//		}
	//		else 
	//			Debug.Log("Difference Not Found");
	//	}
	//	else
	//		Debug.Log("Pressed Green");
	//}

	private void MarkClickedArea(Vector2 ClickedPos)
	{
		//Move to the center left most position of the circle 
		while (SolImgPixels[(int)ClickedPos.x, (int)ClickedPos.y-1] != checkingColor&& SolImgPixels[(int)ClickedPos.x - 1, (int)ClickedPos.y ] != checkingColor)
		{
			while (SolImgPixels[(int)ClickedPos.x, (int)ClickedPos.y] != checkingColor && ClickedPos.x >= 0)
				ClickedPos.x--;
			while (SolImgPixels[(int)ClickedPos.x, (int)ClickedPos.y] != checkingColor && ClickedPos.y >= 0)
				ClickedPos.y--;
		}
		
		ClickedPos.x++;
		ClickedPos.y++;
		while(SolImgPixels[(int)ClickedPos.x, (int)ClickedPos.y] != checkingColor)
		{
			for(int i = (int)ClickedPos.y;  SolImgPixels[(int)ClickedPos.x, i] != checkingColor; i++)
			{
				SolImgPixels[(int)ClickedPos.x, i] = checkingColor;
			}
			for (int i = (int)ClickedPos.y-1; SolImgPixels[(int)ClickedPos.x, i] != checkingColor; i--)
			{
				SolImgPixels[(int)ClickedPos.x, i] = checkingColor;
			}
			ClickedPos.x++;
		}
	}

	private float GiveSimilarityPercentage(Vector2 pixelCords)
	{
		int checkRectStart = (int)(pixelCords.x - checkingCircleWidth / 2);
		int checkRectEnd = (int)(pixelCords.y - checkingCircleHeight / 2);
		int differenceCount = 0;
		for (int i = checkRectStart; i < checkRectStart + checkingCircleWidth; i++ )
		{
			for(int j = checkRectEnd; j < checkRectEnd + checkingCircleHeight; j++)
			{
				if(SolImgPixels[i,j] != checkingColor && DiffImgPixels[i,j] != OrigImgPixels[i, j])
				{
					differenceCount++;
				}
			}
		}
		return (float)(differenceCount/ (checkingCircleWidth * checkingCircleHeight));

	}




	// Convert the image into a matrix of pixel colors
	Color[,] GetPixelMatrix(Texture2D tex)
	{
		Color[,] pixels = new Color[tex.width, tex.height];
		for (int x = 0; x < tex.width; x++)
		{
			for (int y = 0; y < tex.height; y++)
			{
				pixels[x, y] = tex.GetPixel(x, y);
			}
		}
		return pixels;
	}
	void SetTextureFromMatrix(Color[,] colorMatrix)
	{
		Texture2D newTexture = NewImg.sprite.texture;//new Texture2D(SolImg.sprite.texture.width, SolImg.sprite.texture.height);
		
		for (int x = 0; x < newTexture.width; x++)
		{
			for (int y = 0; y < newTexture.height; y++)
			{
				newTexture.SetPixel(x, y, colorMatrix[x, y]);
			}
		}
		//Sprite newSprite = Sprite.Create(newTexture, new Rect(0,0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f)); // Create a new sprite from the texture
		//NewImg.sprite = newSprite; // Assign the new sprite to the Image component
		newTexture.Apply();
	}
	void ThinTexture(Texture2D inputTexture)
	{
		//// Convert input texture to Mat
		//Mat inputMat = Unity.TextureToMat(inputTexture);

		//// Convert Mat to grayscale
		//Mat grayMat = new Mat();
		//OpenCVInterop.CvtColor(inputMat, grayMat, ColorConversionCodes.BGR2GRAY);

		//// Perform thresholding to create binary image
		//Mat binaryMat = new Mat();
		//OpenCVInterop.Threshold(grayMat, binaryMat, 127, 255, ThresholdTypes.Binary);

		//// Perform morphological thinning
		//Mat thinMat = new Mat();
		//OpenCVInterop.MorphologyEx(binaryMat, thinMat, MorphTypes.Thinning, null);

		//// Convert Mat to Texture2D
		//outputTexture = Unity.MatToTexture(thinMat);

		//// Display the result
		//GetComponent<Renderer>().material.mainTexture = outputTexture;
	}
	public void OnPointerClick(PointerEventData eventData)
	{
		Debug.Log("Hello");
	}
}
