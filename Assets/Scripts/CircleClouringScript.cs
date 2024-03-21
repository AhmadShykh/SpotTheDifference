using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.EventSystems;
using System.Threading.Tasks;
using System.Linq;

public class CircleClouringScript : MonoBehaviour, IPointerClickHandler
{
	//Threshold for Checking Green Color
	[SerializeField] float threshold = 3;
	[SerializeField] Color checkingColor = new Color((float)0 / 255, (float)255 / 255, (float)43 / 255, (float)255 / 255);
	[SerializeField] Image myImg;


	public Texture2D loadedTexture; 
	string folderPath;
	
	Color[,] SolImgPixels;
	Vector2 buttonLocationOffset;
	float rectToTextureHeightRatio;
	float rectToTextureWidthtRatio;
	string[] solutFiles;
	
	int index = 34;
	void Start()
	{
		folderPath = "D:/Unity Projects/SpotTheDifference/SpotTheDifferenceGame/Assets/fl/all folders";
		LoadTextures();
		
		SolImgPixels = GetPixelMatrix(myImg.sprite.texture);
		CalculateBoundaries();
	}

	private void CalculateBoundaries()
	{
		float btnHeightValue = GetComponentInParent<RectTransform>().rect.height;
		float btnWidthValue = GetComponentInParent<RectTransform>().rect.width;
		buttonLocationOffset = new Vector2(btnWidthValue / 2, btnHeightValue / 2);
		rectToTextureHeightRatio = myImg.sprite.texture.height / btnHeightValue;
		rectToTextureWidthtRatio = myImg.sprite.texture.width / btnWidthValue;
	}

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
	// Function to extract the numeric part of a string
	int GetNumericValue(string str)
	{
		string numericPart = new string(str.Where(char.IsDigit).ToArray());
		return int.Parse(numericPart);
	}
	void LoadTextures()
	{ 
		// Get all files in the folder
		string[] files = Directory.GetFiles(folderPath);

		// Filter files containing "solut" in their names and exclude ".meta" files
		solutFiles = Array.FindAll(files, name => name.Contains("solut") && !name.EndsWith(".meta"));
		Array.Sort(solutFiles, (x, y) => GetNumericValue(x).CompareTo(GetNumericValue(y)));

		byte[] fileData = File.ReadAllBytes(solutFiles[index]);
		Texture2D texture = new Texture2D(2, 2); // Modify texture size as needed
		if (texture.LoadImage(fileData))
		{
			// Image loaded successfully
			loadedTexture = texture;
			Sprite newSprite = Sprite.Create(loadedTexture, new Rect(0, 0, loadedTexture.width, loadedTexture.height), new Vector2(0.5f, 0.5f)); // Create a new sprite from the texture
			myImg.sprite = newSprite; // Assign the new sprite to the Image component
		}
		else
		{
			// Failed to load image
			Debug.LogError("Failed to load image from byte array.");
		}
	}
	public async void OnPointerClick(PointerEventData eventData)
	{
		// Get the position of the click relative to the image
		Vector2 localPoint;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out localPoint);
		localPoint += buttonLocationOffset;
		// Convert localPoint to pixel coordinates
		Vector2Int pixelCoord = new Vector2Int((int)(localPoint.x * rectToTextureWidthtRatio), (int)(localPoint.y * rectToTextureHeightRatio));
		Debug.Log(pixelCoord);

		//Filling the hollow circle
		await MarkClickedArea(pixelCoord);
		SetTextureFromMatrix(SolImgPixels);
	}
	// Method to calculate the Euclidean distance between two colors
	private static double getColorDistance(Color c1, Color c2)
	{
		double redDiff = c1.r - c2.r;
		double greenDiff = c1.g - c2.g;
		double blueDiff = c1.b - c2.b;
		return Math.Sqrt(redDiff * redDiff + greenDiff * greenDiff + blueDiff * blueDiff);
	}
	private async Task MarkClickedArea(Vector2 ClickedPos)
	{
		float val = (float)getColorDistance(SolImgPixels[(int)ClickedPos.x, (int)ClickedPos.y] , checkingColor);
		while (ClickedPos.x >= 0 && getColorDistance(SolImgPixels[(int)ClickedPos.x, (int)ClickedPos.y] , checkingColor) > threshold)
				ClickedPos.x--;

		ClickedPos.x++;
		while (ClickedPos.x < loadedTexture.width && getColorDistance(SolImgPixels[(int)ClickedPos.x, (int)ClickedPos.y] , checkingColor) > threshold)
		{
			ColorTopSide(ClickedPos);
			ColorBottomSide(ClickedPos);
			ClickedPos.x++;
		}
	}

	private async void ColorBottomSide(Vector2 ClickedPos)
	{
		for (int i = (int)ClickedPos.y - 1; i >= 0 && getColorDistance(SolImgPixels[(int)ClickedPos.x, i], checkingColor) > threshold; --i)
		{
			SolImgPixels[(int)ClickedPos.x, i] = checkingColor;
		}
	}

	private async void ColorTopSide(Vector2 ClickedPos)
	{
		for (int i = (int)ClickedPos.y; i < loadedTexture.height && getColorDistance(SolImgPixels[(int)ClickedPos.x, i], checkingColor) > threshold; ++i)
		{
			SolImgPixels[(int)ClickedPos.x, i] = checkingColor;
		}
	}

	async void SetTextureFromMatrix(Color[,] colorMatrix)
	{
		//Texture2D newTexture = myImg.sprite.texture;//new Texture2D(SolImg.sprite.texture.width, SolImg.sprite.texture.height);

		await SetLeftHalfTexture(colorMatrix);
		await SetRightHalfTexture(colorMatrix);
		loadedTexture.Apply();
	}

	private async Task SetRightHalfTexture(Color[,] colorMatrix)
	{
		for (int x = loadedTexture.width / 2; x < loadedTexture.width; x++)
		{
			for (int y = 0; y < loadedTexture.height; y++)
			{
				loadedTexture.SetPixel(x, y, colorMatrix[x, y]);
			}
		}
	}

	private async Task SetLeftHalfTexture(Color[,] colorMatrix)
	{
		for (int x = 0; x < loadedTexture.width / 2; x++)
		{
			for (int y = 0; y < loadedTexture.height; y++)
			{
				loadedTexture.SetPixel(x, y, colorMatrix[x, y]);
			}
		}
	}

	public void Move(string dir)
	{
		// Check if the down arrow key is pressed
		if (dir == "left")
		{
			
			byte[] fileData = File.ReadAllBytes(solutFiles[--index]);
			Texture2D texture = new Texture2D(2, 2); // Modify texture size as needed
			if (texture.LoadImage(fileData))
			{
				// Image loaded successfully
				loadedTexture = texture;
				SolImgPixels = GetPixelMatrix(loadedTexture);
				Sprite newSprite = Sprite.Create(loadedTexture, new Rect(0, 0, loadedTexture.width, loadedTexture.height), new Vector2(0.5f, 0.5f)); // Create a new sprite from the texture
				myImg.sprite = newSprite; // Assign the new sprite to the Image component
				CalculateBoundaries();
			}
		}
		else if (dir == "right")
		{
			
			// Get the texture bytes
			byte[] textureBytes = loadedTexture.EncodeToJPG(); // Or use EncodeToJPG for JPG format

			// Create a file path for the new texture in the destination folder
			string newTexturePath = "D:/Unity Projects/SpotTheDifference/SpotTheDifferenceGame/Assets/fl/spot " + (index+1) + " solution.jpg";

			// Write the texture bytes to the new file
			File.WriteAllBytes(newTexturePath, textureBytes);
			
			byte[] fileData = File.ReadAllBytes(solutFiles[++index]);
			Texture2D texture = new Texture2D(2, 2); // Modify texture size as needed
			if (texture.LoadImage(fileData))
			{
				// Image loaded successfully
				loadedTexture = texture;
				SolImgPixels = GetPixelMatrix(loadedTexture);
				Sprite newSprite = Sprite.Create(loadedTexture, new Rect(0, 0, loadedTexture.width, loadedTexture.height), new Vector2(0.5f, 0.5f)); // Create a new sprite from the texture
				myImg.sprite = newSprite; // Assign the new sprite to the Image component
				CalculateBoundaries();
			}

		}
		
	}
}
