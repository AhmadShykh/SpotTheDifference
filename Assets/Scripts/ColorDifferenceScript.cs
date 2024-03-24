using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Security.Cryptography;
using static UnityEngine.EventSystems.EventTrigger;
using JetBrains.Annotations;
using System.Threading.Tasks;
using UnityEditor.Tilemaps;
using System.Net.NetworkInformation;

public class ColorDifferenceScript : MonoBehaviour
{

	[Header("Images Settings")]
	[SerializeField] Image originalImage;
	[SerializeField] Image differenceImage;


	private Button _diffBtn;
	
	private string _folderPath;

	private int _shownImgCount = 0;
	private int _maxDiffCount = 8;
	private int _currentDiffCount = 3;
	private int _incDiffAfterImgCount = 10;
	private int _diffRadiusPer = 5;

	private int _maxTriesCount = 5;
	private int _tries = 0;
	private float _timeToRemoveHint = 4;
	private List<Vector2> _diffCenterPoints = new List<Vector2>();

	private int _maxThreads = 4;
	private int _differenceValue = 500;

	private bool _differenceClicked;
	private int _differenceRadius;

	void Start()
	{
		_folderPath = "D:\\Unity Projects\\SpotTheDifference\\Assets\\Images\\All Images";
		GameManager.OnStateChangeAction += StartGameSequence;

		_diffBtn = differenceImage.gameObject.GetComponent<Button>();
		_diffBtn.onClick.AddListener(DiffBtnClickSequence);
		_differenceClicked = false;

	}
	private async void DiffBtnClickSequence()
	{
		if (!_differenceClicked)
		{
			// Getting the Mouse Clicked Position on the image
			Vector2 pixelClickedPos = MouseWorldPosToDiffImgPos(Input.mousePosition);
			
			
			foreach (Vector2 point in _diffCenterPoints)
			{
				if (PointWithinCircle(pixelClickedPos, point))
				{
					_differenceClicked = true;
					await RemovePointDifference(point);
					_differenceClicked = false;

					if (_diffCenterPoints.Count <= 0)
						GameManager.instance.UpdateGameState(State.GameScreen);

					return;

				}
			}
		}

		if (_tries >= _maxTriesCount)
		{
			_tries = 0;
			GameManager.instance.UpdateGameState(State.InterstitialAd);
		}
		_tries++;
		
	}
	Vector2 MouseWorldPosToDiffImgPos(Vector2 pixelClickedPos)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(differenceImage.rectTransform, Input.mousePosition, GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>(), out pixelClickedPos);

		pixelClickedPos.x = pixelClickedPos.x * (differenceImage.sprite.texture.width / differenceImage.rectTransform.rect.width);
		pixelClickedPos.y = pixelClickedPos.y * (differenceImage.sprite.texture.height / differenceImage.rectTransform.rect.height);
		Debug.Log(pixelClickedPos);
		return pixelClickedPos;
	}
	private async Task RemovePointDifference(Vector2 point)
	{
		_diffCenterPoints.Remove(point);
		Color[] orgPixles = originalImage.sprite.texture.GetPixels();
		Color[] diffPixles = differenceImage.sprite.texture.GetPixels();

		for (int i = (int)point.x - _differenceRadius; i < point.x + _differenceRadius; i++)
		{
			for (int j = (int)point.y - _differenceRadius; j < point.y + _differenceRadius; j++)
			{
				if (PointWithinCircle(new Vector2(i, j), point))
				{
					diffPixles[j * originalImage.sprite.texture.width + i] = orgPixles[j * originalImage.sprite.texture.width + i];
				}
			}
		}
		_diffCenterPoints.Remove(point);
		differenceImage.sprite.texture.SetPixels(diffPixles);
		differenceImage.sprite.texture.Apply();
	}
	private void StartGameSequence(State state)
	{
		if(state == State.GameScreen)
		{
			if(originalImage.sprite != null)
				Destroy(originalImage.sprite.texture);
			if (differenceImage.sprite != null)
				Destroy(differenceImage.sprite.texture);

			// Set sprites to null to clear references
			originalImage.sprite = null;
			differenceImage.sprite = null;
			_diffCenterPoints.Clear();

			SelectRandomPicture();
			ImageDifferenceCreator(); 
		}
	}
	private void SelectRandomPicture()
	{
		if (Directory.Exists(_folderPath))
		{
			// Taking The image
			string[] files = Directory.GetFiles(_folderPath,"*.jpg");
			byte[] fileBytes = File.ReadAllBytes(files[Random.Range(0, files.Length-1)]);
			
			// Applying it to a new texture
			Texture2D originalTexture = new Texture2D(2, 2);
			Texture2D differenceTexture = new Texture2D(2, 2);
			originalTexture.LoadImage(fileBytes);
			differenceTexture.LoadImage(fileBytes);
			
			// Creating Sprites from those Texture
			Sprite orgImgSprite = Sprite.Create(originalTexture, new Rect(0, 0, originalTexture.width, originalTexture.height), Vector2.one * 0.5f);
			Sprite diffImgSprite = Sprite.Create(differenceTexture, new Rect(0, 0, differenceTexture.width, differenceTexture.height), Vector2.one * 0.5f);

			originalImage.sprite = orgImgSprite;
			differenceImage.sprite = diffImgSprite;

		}
		else
			Debug.LogError("Path Does Not Exists");
	}
	void ImageDifferenceCreator()
	{
		
		int textureWidth = originalImage.sprite.texture.width, textureHeight = originalImage.sprite.texture.height;
		Color[] pixels = originalImage.sprite.texture.GetPixels();

		// Calculating Difference based on the texture width
		_differenceRadius = (_diffRadiusPer * textureWidth) / 100;


		_currentDiffCount += _shownImgCount / _incDiffAfterImgCount;
		_currentDiffCount = _currentDiffCount <= _maxDiffCount ? _currentDiffCount : _maxDiffCount;

		for (int j = 0; j< _currentDiffCount; j++)
		{
			// Randomly select a center point within the image bounds
			int centerX = Random.Range(_differenceRadius, textureWidth - _differenceRadius);
			int centerY = Random.Range(_differenceRadius, textureHeight - _differenceRadius);
			int tries = 0;
			while (CheckPointWithInDiffPoints(new Vector2(centerX, centerY)) != new Vector2(-1, -1))
			{
				centerX = _differenceRadius + ((centerX + Random.Range(0, _differenceValue)) % (textureWidth - 2 * _differenceRadius));
				centerY = _differenceRadius + ((centerY + Random.Range(0, _differenceValue)) % (textureHeight - 2 * _differenceRadius));
				tries++;
			}

			_diffCenterPoints.Add(new Vector2(centerX, centerY));

			tries = 0;
			// Finding Center Point From where we will get the difference
			int diffCenterX = Random.Range(_differenceRadius, textureWidth - _differenceRadius);
			int diffCenterY = Random.Range(_differenceRadius, textureHeight - _differenceRadius);
			while (CheckPointWithInDiffPoints(new Vector2(diffCenterX, diffCenterY)) != new Vector2(-1, -1))
			{
				diffCenterX = _differenceRadius + ((diffCenterX + Random.Range(0, _differenceValue)) % (textureWidth - 2 * _differenceRadius));
				diffCenterY = _differenceRadius + ((diffCenterY + Random.Range(0, _differenceValue)) % (textureHeight - 2 * _differenceRadius));
				tries++;
			}

			int orgStrX, orgEndX, diffStrX;

			for (int i = 0; i < _maxThreads; i++)
			{
				orgStrX = (centerX - _differenceRadius) + i * (_differenceRadius / _maxThreads);
				orgEndX = i + 1 == _maxThreads ? centerX + _differenceRadius : (centerX - _differenceRadius) + (i + 1) * (_differenceRadius / _maxThreads);
				diffStrX = (diffCenterX - _differenceRadius) + i * (_differenceRadius / _maxThreads);

				ExecuteMatrixFillInParellel(orgStrX, orgEndX, diffStrX, pixels, textureWidth, centerX, centerY, diffCenterY);
			}
		}
		
		differenceImage.sprite.texture.SetPixels(pixels);
		// Apply changes to the modified image
		differenceImage.sprite.texture.Apply();
	}
	private async void ExecuteMatrixFillInParellel(int orgStrX, int orgEndX, int diffStrX, Color[] pixels, int textureWidth,int centerX,int centerY, int diffCenterY)
	{
		// Fill the erased region with any other radius of the circle
		for (int x = orgStrX, diffX = diffStrX; x <= orgEndX; x++, diffX++)
		{
			for (int y = centerY - _differenceRadius, diffY = diffCenterY - _differenceRadius; y <= centerY + _differenceRadius; y++, diffY++)
			{
				// Check if the pixel is within the circle defined by the eraser radius
				if (PointWithinCircle(new Vector2(x,y) , new Vector2(centerX,centerY)))
				{
					// Set the color of the pixel in the modified image to the difference radius
					pixels[y * textureWidth + x] = pixels[diffY * textureWidth + diffX];
				}
			}
		}
	}
	bool PointWithinCircle(Vector2 point, Vector2 circleCenter)
	{
		bool returnVal = (Mathf.Pow(point.x - circleCenter.x,2) + Mathf.Pow(point.y - circleCenter.y,2) <= _differenceRadius * _differenceRadius) ? true : false;
		return returnVal;
	}
	private void ExecuteMatrixFillInParellel()
	{
		throw new System.NotImplementedException();
	}
	private Vector2 CheckPointWithInDiffPoints(Vector2 centerPoint)
	{
		foreach(Vector2 point in _diffCenterPoints)
		{
			if (Vector2.Distance(centerPoint , point) < 2*_differenceRadius){
				return point;
			}
		}
		return new Vector2(-1,-1);
	}
	public GameObject InitiateHintSequence(GameObject hintPrefab)
	{
		Vector2 hintPoint = DiffImgPosToLocalPos(_diffCenterPoints[0]);
		Debug.Log(hintPoint);
		Debug.Log(_diffCenterPoints[0]);
		GameObject hintObj = Instantiate(hintPrefab,differenceImage.transform);
		hintObj.transform.localPosition = hintPoint;
		return hintObj;
	}
	private Vector2 DiffImgPosToLocalPos(Vector2 centerPoint)
	{
		centerPoint.x = centerPoint.x * ( differenceImage.rectTransform.rect.width/differenceImage.sprite.texture.width) ;
		centerPoint.y = centerPoint.y * ( differenceImage.rectTransform.rect.height/ differenceImage.sprite.texture.height) ;
		Debug.Log(differenceImage.rectTransform.rect.height + " " +differenceImage.sprite.texture.height);
		return centerPoint;
	}
}
