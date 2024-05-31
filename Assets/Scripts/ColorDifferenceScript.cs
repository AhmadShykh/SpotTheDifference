using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Unity.VisualScripting;
using UnityEngine.Purchasing.MiniJSON;
using Newtonsoft.Json;
using UnityEngine.Purchasing.Extension;
using JetBrains.Annotations;
using GoogleMobileAds.Ump.Api;
using System;
using TMPro;


[System.Serializable]
public class Difference
{
	public float x_percent;
	public float y_percent;
	public float width_percent;
	public float height_percent;
}


[System.Serializable]
public class Differences
{
	public string name;
	public Difference[] difference;
}


[System.Serializable] 
public class DifferenceImages
{
	public Differences[] differences;
}


public class ColorDifferenceScript : MonoBehaviour
{


	//================================================ Image Diff Script 1.0 ================================================

	[Header("Images Settings")]
	[SerializeField] Image originalImage;
	[SerializeField] Image differenceImage;
	[SerializeField] AssetLabelReference _assetsLabelReference;

	[Header("Differences Settings")]
	[SerializeField] TextAsset _optionsFile;
	[SerializeField] TextMeshProUGUI _triesText;
	[SerializeField] TextMeshProUGUI _differenceText;
	[SerializeField] GameObject _differenceSquare;

	private List<GameObject> _differenceSquareList;

	[Header("Audio Settings")]
	[SerializeField] AudioSource _soundEffectAudioSource;
	[SerializeField] AudioClip _correctSound;
	[SerializeField] AudioClip _wrongSound;
	

	private Button _diffBtn;

	//private string _folderPath;

	private int _shownImgCount = 0;
	//private int _maxDiffCount = 8;
	//private int _incDiffAfterImgCount = 10;
	//private int _diffRadiusPer = 5;

	private int _maxTriesCount = 5;
	private int _tries = 0;
	private int _showBannerAfter = 4;
	private Differences _selImgDifferences;

	private int _maxThreads = 4;
	//private int _differenceValue = 500;

	private bool _differenceClicked;
	private int _diffClickCount;
	//private int _differenceRadius;
	//private int _currentDiffCount;

	// Addressables data
	//IList<IResourceLocation> files;

	//Experiment 



	private void Awake()
	{
		// Events and listeners
		GameManager.OnStateChangeAction += StartGameSequence;
		GameManager.OnStateChangeAction += MainScreenSequence;
		_diffBtn = differenceImage.gameObject.GetComponent<Button>();
		_diffBtn.onClick.AddListener(DiffBtnClickSequence);
	}

	private void MainScreenSequence(State state)
	{
		if (state == State.MainScreen)
		{
			//_currentDiffCount = 3;
			_shownImgCount = 0;
		}
	}

	private void OnDestroy()
	{
		GameManager.OnStateChangeAction -= StartGameSequence;
	}
	void Start()
	{
		// Initial Variable Set Values
		//_folderPath = "D:\\Unity Projects\\SpotTheDifference\\Assets\\Images\\All Images";
		_differenceSquareList = new List<GameObject>();
		_differenceClicked = false;

		//LoadAllImagesLocations();

	}
	//private void LoadAllImagesLocations()
	//{
	//	AsyncOperationHandle<IList<IResourceLocation>> locationsHandler = Addressables.LoadResourceLocationsAsync(_assetsLabelReference);
	//	locationsHandler.WaitForCompletion();

	//	if (locationsHandler.Status == AsyncOperationStatus.Succeeded)
	//		files = locationsHandler.Result;
	//	else
	//		Debug.LogError("Could not load addressable");

	//	Addressables.Release(locationsHandler);

	//	// Unloading all unused item
	//	Resources.UnloadUnusedAssets();
	//}
	private void DiffBtnClickSequence()
	{
		if (!_differenceClicked)
		{
			_differenceClicked = true;
			// Getting the Mouse Clicked Position on the image
			Vector2 pixelClickedPos = MouseWorldPosToDiffImgPos(Input.mousePosition);


			for (int i = 0; i < _selImgDifferences.difference.Length; i++)
			{
				Difference pointPercents = _selImgDifferences.difference[i];

				float width = differenceImage.sprite.texture.width/(float)100.0;
				float height = differenceImage.sprite.texture.height/(float)100.0;

				
				Vector2 sqPoint = new Vector2(pointPercents.x_percent * width, pointPercents.y_percent * height);
				Vector2 perimeter = new Vector2(pointPercents.width_percent * width ,pointPercents.height_percent * height);

				pixelClickedPos = new Vector2(MathF.Abs(pixelClickedPos.x), MathF.Abs(pixelClickedPos.y));

				if (PointWithinCircle(pixelClickedPos, sqPoint, perimeter))
				{
					Debug.Log($"Difference Clicked: {sqPoint} {pixelClickedPos}");

					Vector2 percentPoint = new Vector2(pointPercents.x_percent, pointPercents.y_percent);
					Vector2 percentPerimeter = new Vector2(pointPercents.width_percent, pointPercents.height_percent);


					_soundEffectAudioSource.clip = _correctSound;
					_soundEffectAudioSource.Play();

					Vector2 imageSize = new Vector2(differenceImage.rectTransform.rect.width, differenceImage.rectTransform.rect.height);

					Vector2 diffSqPoint = DiffImgPosToLocalPos(percentPoint, percentPerimeter, imageSize);

					GameObject diffObj = Instantiate(_differenceSquare, differenceImage.transform);
					diffObj.transform.localPosition = diffSqPoint;
					RectTransform diffSqRect = diffObj.GetComponent<RectTransform>();
					Vector2 newSize = new Vector2(percentPerimeter.x * imageSize.x / 100f, percentPerimeter.y * imageSize.y / 100f);
					diffSqRect.sizeDelta = newSize;

					_differenceSquareList.Add(diffObj);

					List<Difference> diffList = new List<Difference>(_selImgDifferences.difference);
					diffList.RemoveAt(i);
					_selImgDifferences.difference = diffList.ToArray();

					Scoring._score += 1;

					_diffClickCount--;
					_differenceText.text = _diffClickCount.ToString();

					//removeDiffFromImg(sqPoint, perimeter);

					_differenceClicked = false;

					if (_selImgDifferences.difference.Length <= 0)
					{
						_diffBtn.interactable = false;
						SpawnNewImg();
					}

					return;
				}
			}

			_differenceClicked = false;

		}

		_soundEffectAudioSource.clip = _wrongSound;
		_soundEffectAudioSource.Play();

		_tries--;
		_triesText.text = _tries.ToString();

		if(_tries <=0 )
		{
			_tries = _maxTriesCount;
			GameManager.instance.UpdateGameState(State.InterstitialAd);
			GameManager.instance.UpdateGameState(State.MainScreen); 
		}

	}

	private async Task SpawnNewImg()
	{
		await Task.Delay(1030);
		GameManager.instance.UpdateGameState(State.GameScreen);
	}

	void DestroyDiffSquares()
	{
		foreach(GameObject obj in _differenceSquareList)
		{
			Destroy(obj);
		}
		_differenceSquareList.Clear();
	}

	private void removeDiffFromImg(Vector2 sqPoint, Vector2 perimeter)
	{
		int textureWidth = differenceImage.sprite.texture.width, textureHeight = differenceImage.sprite.texture.height;


		Color[] diffPixels = differenceImage.sprite.texture.GetPixels();
		Color[] origPixels = originalImage.sprite.texture.GetPixels();

		for(int i = (int)sqPoint.x; i < sqPoint.x + perimeter.x; i++) {
			for (int j = (int)sqPoint.y; j < sqPoint.y + perimeter.y; j++)
			{
				diffPixels[j + textureWidth + i] = origPixels[j + textureWidth + i];
			}
		}


		differenceImage.sprite.texture.SetPixels(diffPixels);
		originalImage.sprite.texture.SetPixels(origPixels);
		differenceImage.sprite.texture.Apply();
		originalImage.sprite.texture.Apply();

	}

	Vector2 MouseWorldPosToDiffImgPos(Vector2 pixelClickedPos)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(differenceImage.rectTransform, Input.mousePosition, GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>(), out pixelClickedPos);

		pixelClickedPos.x = pixelClickedPos.x * (differenceImage.sprite.texture.width / differenceImage.rectTransform.rect.width);
		pixelClickedPos.y = pixelClickedPos.y * (differenceImage.sprite.texture.height / differenceImage.rectTransform.rect.height);
		return pixelClickedPos;
	}


	//private async Task RemovePointDifference(Vector2 point)
	//{
	//	_diffCenterPoints.Remove(point);
	//	Color[] orgPixles = originalImage.sprite.texture.GetPixels();
	//	Color[] diffPixles = differenceImage.sprite.texture.GetPixels();

	//	for (int i = (int)point.x - _differenceRadius; i < point.x + _differenceRadius; i++)
	//	{
	//		for (int j = (int)point.y - _differenceRadius; j < point.y + _differenceRadius; j++)
	//		{
	//			if (PointWithinCircle(new Vector2(i, j), point))
	//			{
	//				diffPixles[j * originalImage.sprite.texture.width + i] = orgPixles[j * originalImage.sprite.texture.width + i];
	//			}
	//		}
	//	}
	//	_diffCenterPoints.Remove(point);
	//	differenceImage.sprite.texture.SetPixels(diffPixles);
	//	differenceImage.sprite.texture.Apply();
	//}
	private void StartGameSequence(State state)
	{
		
		if (state == State.GameScreen)
		{
			_diffBtn.interactable = true;
			DestroyDiffSquares();
			_tries = _maxTriesCount;
			_triesText.text = _tries.ToString();
			_shownImgCount++;

			if (originalImage.sprite != null)
				Destroy(originalImage.sprite.texture);
			if (differenceImage.sprite != null)
				Destroy(differenceImage.sprite.texture);

			// Set sprites to null to clear references
			originalImage.sprite = null;
			differenceImage.sprite = null;

			diffImgSelector();

			//SelectRandomPicture();
			//ImageDifferenceCreator();

			_diffClickCount = _selImgDifferences.difference.Length;
			_differenceText.text = _diffClickCount.ToString();

			if (_shownImgCount % _showBannerAfter == 0)
				GameManager.instance.ShowBanner();

			
		}
	}

	private void diffImgSelector()
	{
		DifferenceImages differenceImages = JsonUtility.FromJson<DifferenceImages>(_optionsFile.text);
		_selImgDifferences = differenceImages.differences[UnityEngine.Random.Range(0, differenceImages.differences.Length)];

		Debug.Log("Image Name: "+ _selImgDifferences.name);

		string origImgPath = "Assets/Images/Images/Orig/" + _selImgDifferences.name + ".jpg";

		// Search for the asset by name synchronously
		Texture2D origTexture = Addressables.LoadAssetAsync<Texture2D>(origImgPath).WaitForCompletion();
		Texture2D diffTexture = Addressables.LoadAssetAsync<Texture2D>("Assets/Images/Images/Diff/" + _selImgDifferences.name + " diff.jpg").WaitForCompletion();

		AddFileToLibrary(origImgPath);



		// Applying it to a new texture
		Texture2D originalTexture = Instantiate(origTexture) as Texture2D;
		Texture2D differenceTexture = Instantiate(diffTexture) as Texture2D;

		// Creating Sprites from those Texture
		Sprite orgImgSprite = Sprite.Create(originalTexture, new Rect(0, 0, originalTexture.width, originalTexture.height), Vector2.one * 0.5f);
		Sprite diffImgSprite = Sprite.Create(differenceTexture, new Rect(0, 0, differenceTexture.width, differenceTexture.height), Vector2.one * 0.5f);

		originalImage.sprite = orgImgSprite;
		differenceImage.sprite = diffImgSprite;

		// Unloading all unused item
		Resources.UnloadUnusedAssets();
	}

	//private void SelectRandomPicture()
	//{
	//	IResourceLocation fileResource = files[Random.Range(0, files.Count)];
	//	AsyncOperationHandle<Texture2D> textureAsync = Addressables.LoadAssetAsync<Texture2D>(fileResource.PrimaryKey);
	//	textureAsync.WaitForCompletion();
	//	Texture2D imageTexture = textureAsync.Result;

	//	AddFileToLibrary(fileResource.PrimaryKey);


	//	// Applying it to a new texture
	//	Texture2D originalTexture = Instantiate(imageTexture) as Texture2D;
	//	Texture2D differenceTexture = Instantiate(imageTexture) as Texture2D;

	//	// Creating Sprites from those Texture
	//	Sprite orgImgSprite = Sprite.Create(originalTexture, new Rect(0, 0, originalTexture.width, originalTexture.height), Vector2.one * 0.5f);
	//	Sprite diffImgSprite = Sprite.Create(differenceTexture, new Rect(0, 0, differenceTexture.width, differenceTexture.height), Vector2.one * 0.5f);

	//	originalImage.sprite = orgImgSprite;
	//	differenceImage.sprite = diffImgSprite;

	//	Addressables.Release(textureAsync);

	//}

	private void AddFileToLibrary(string filePath)
	{
		string libraryFiles = PlayerPrefs.GetString("Library", "");
		string[] allFiles = libraryFiles.Split(';');
		if (libraryFiles == "")
			PlayerPrefs.SetString("Library", filePath);
		else if (!allFiles.Contains(filePath))
			PlayerPrefs.SetString("Library", libraryFiles + ";" + filePath);
	}



	//void ImageDifferenceCreator()
	//{

	//	int textureWidth = differenceImage.sprite.texture.width, textureHeight = differenceImage.sprite.texture.height;

	//	Color[] pixels = differenceImage.sprite.texture.GetPixels();




	//	// Calculating Difference based on the texture width
	//	_differenceRadius = (_diffRadiusPer * textureWidth) / 100;


	//	_currentDiffCount += _shownImgCount / _incDiffAfterImgCount;
	//	_currentDiffCount = _currentDiffCount <= _maxDiffCount ? _currentDiffCount : _maxDiffCount;

	//	for (int j = 0; j < _currentDiffCount; j++)
	//	{
	//		// Randomly select a center point within the image bounds
	//		int centerX = Random.Range(_differenceRadius, textureWidth - _differenceRadius);
	//		int centerY = Random.Range(_differenceRadius, textureHeight - _differenceRadius);
	//		int tries = 0;
	//		while (CheckPointWithInDiffPoints(new Vector2(centerX, centerY)) != new Vector2(-1, -1))
	//		{
	//			centerX = _differenceRadius + ((centerX + Random.Range(0, _differenceValue)) % (textureWidth - 2 * _differenceRadius));
	//			centerY = _differenceRadius + ((centerY + Random.Range(0, _differenceValue)) % (textureHeight - 2 * _differenceRadius));
	//			tries++;
	//		}

	//		_diffCenterPoints.Add(new Vector2(centerX, centerY));

	//		tries = 0;
	//		// Finding Center Point From where we will get the difference
	//		int diffCenterX = Random.Range(_differenceRadius, textureWidth - _differenceRadius);
	//		int diffCenterY = Random.Range(_differenceRadius, textureHeight - _differenceRadius);
	//		while (CheckPointWithInDiffPoints(new Vector2(diffCenterX, diffCenterY)) != new Vector2(-1, -1))
	//		{
	//			diffCenterX = _differenceRadius + ((diffCenterX + Random.Range(0, _differenceValue)) % (textureWidth - 2 * _differenceRadius));
	//			diffCenterY = _differenceRadius + ((diffCenterY + Random.Range(0, _differenceValue)) % (textureHeight - 2 * _differenceRadius));
	//			tries++;
	//		}

	//		int orgStrX, orgEndX, diffStrX;

	//		for (int i = 0; i < _maxThreads; i++)
	//		{
	//			orgStrX = (centerX - _differenceRadius) + i * (_differenceRadius / _maxThreads);
	//			orgEndX = i + 1 == _maxThreads ? centerX + _differenceRadius : (centerX - _differenceRadius) + (i + 1) * (_differenceRadius / _maxThreads);
	//			diffStrX = (diffCenterX - _differenceRadius) + i * (_differenceRadius / _maxThreads);

	//			ExecuteMatrixFillInParellel(orgStrX, orgEndX, diffStrX, pixels, textureWidth, centerX, centerY, diffCenterY);
	//		}
	//	}

	//	differenceImage.sprite.texture.SetPixels(pixels);// = diffImgSprite;
	//													 // Apply changes to the modified image
	//	differenceImage.sprite.texture.Apply();
	//}
	//private async void ExecuteMatrixFillInParellel(int orgStrX, int orgEndX, int diffStrX, Color[] pixels, int textureWidth, int centerX, int centerY, int diffCenterY)
	//{
	//	// Fill the erased region with any other radius of the circle
	//	for (int x = orgStrX, diffX = diffStrX; x <= orgEndX; x++, diffX++)
	//	{
	//		for (int y = centerY - _differenceRadius, diffY = diffCenterY - _differenceRadius; y <= centerY + _differenceRadius; y++, diffY++)
	//		{
	//			// Check if the pixel is within the circle defined by the eraser radius
	//			if (PointWithinCircle(new Vector2(x, y), new Vector2(centerX, centerY)))
	//			{
	//				// Set the color of the pixel in the modified image to the difference radius
	//				pixels[y * textureWidth + x] = pixels[diffY * textureWidth + diffX];
	//			}
	//		}
	//	}
	//}
	bool PointWithinCircle(Vector2 point, Vector2 sqPoint,Vector2 perimeter)
	{
		if (point.x >= sqPoint.x && point.y >= sqPoint.y && point.x <= sqPoint.x + perimeter.x && point.y <= sqPoint.y + perimeter.y)
			return true;
		else
			return false;
	}
	//private void ExecuteMatrixFillInParellel()
	//{
	//	throw new System.NotImplementedException();
	//}
	//private Vector2 CheckPointWithInDiffPoints(Vector2 centerPoint)
	//{
	//	foreach (Vector2 point in _diffCenterPoints)
	//	{
	//		if (Vector2.Distance(centerPoint, point) < 2 * _differenceRadius)
	//		{
	//			return point;
	//		}
	//	}
	//	return new Vector2(-1, -1);



	//}



	public GameObject InitiateHintSequence(GameObject hintPrefab)
	{
		Vector2 pointPercent = new Vector2(_selImgDifferences.difference[0].x_percent, _selImgDifferences.difference[0].y_percent);
		Vector2 perimeterPercent = new Vector2(_selImgDifferences.difference[0].width_percent, _selImgDifferences.difference[0].height_percent);

		Vector2 imageSize = new Vector2(differenceImage.rectTransform.rect.width, differenceImage.rectTransform.rect.height);

		Vector2 hintPoint = DiffImgPosToLocalPos(pointPercent,perimeterPercent,imageSize);

		// Calculate the local position
		hintPoint.x += ((perimeterPercent.x / 200f) * imageSize.x);
		hintPoint.y -= ((perimeterPercent.y / 200f) * imageSize.y);

		Debug.Log($"Hint Point: {hintPoint} Diff Point: {pointPercent}");
		
		GameObject hintObj = Instantiate(hintPrefab, differenceImage.transform);
		hintObj.transform.localPosition = hintPoint;



		return hintObj;
	}

	// Function to convert pixel coordinates to local position of differenceImage
	private Vector2 DiffImgPosToLocalPos(Vector2 pixelCoordinates,Vector2 perimeterCoordinates,Vector2 imageSize)
	{
		// Get the size of the differenceImage in pixels
		
		// Calculate the local position
		Vector2 localPosition = new Vector2(
			(pixelCoordinates.x / 100f) * imageSize.x ,
			-(pixelCoordinates.y / 100f) * imageSize.y 
		);

		return localPosition;
	}


	//================================================ Image Diff Script 1.0 ================================================
}
