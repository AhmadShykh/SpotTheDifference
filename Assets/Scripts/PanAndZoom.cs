using UnityEngine;
using UnityEngine.EventSystems;

public class PanAndZoom : MonoBehaviour, IPointerDownHandler,IPointerUpHandler
{
    public RectTransform imageRect;  // The RectTransform of the image you want to manipulate
    public float zoomSpeed = 0.1f;   // How fast to zoom in/out
    public float panSpeed = 0.1f;    // How fast to pan the image
    public Camera cam;
    private Vector2 lastPanPosition; // For tracking pan movement
    private int panFingerId;         // Finger ID for panning
    private bool isPanning;
    private bool isTouch = false;
    
    void Update()
    {
        if (isTouch)
        {
            // Single touch for panning
            if (Input.touchCount >= 1)
            {
                Touch touch = Input.GetTouch(0);

                
                // Check if the touch point is over the image
                if (touch.phase == TouchPhase.Began)
                {
                    lastPanPosition = touch.position;
                    panFingerId = touch.fingerId;
                    isPanning = true;
                }
                else if (touch.phase == TouchPhase.Moved && isPanning && touch.fingerId == panFingerId)
                {
                    Pan(touch.position);
                }
                else if (touch.phase == TouchPhase.Ended && touch.fingerId == panFingerId)
                {
                    isPanning = false;
                }
                
            }

            // Two touches for pinch zooming
            if (Input.touchCount == 2)
            {
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);

                
                    if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
                    {
                        Zoom(touch1, touch2);
                    }
            }
            
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            // Simulate two touch points for panning effect
            Touch touch1 = new Touch();
            Touch touch2 = new Touch();

            // Manually set positions for touches
            Vector2 screenCenter = new Vector2(0, 0);
            touch1.position = new Vector2(Input.mousePosition.x,Input.mousePosition.y);  // Left of center
            touch2.position = new Vector2(Input.mousePosition.x,Input.mousePosition.y);   // Right of center
            Debug.Log(Input.mousePosition);
            
            // Simulate movement by setting the deltaPosition
            touch1.deltaPosition = new Vector2(-10, 0);  // Move left touch slightly left
            touch2.deltaPosition = new Vector2(10, 0);   // Move right touch slightly right

            // Simulate the "Moved" phase for both touches
            touch1.phase = TouchPhase.Moved;
            touch2.phase = TouchPhase.Moved;

            
                Zoom(touch1, touch2);
            
        }    
        
    }

    // Panning the image by touch drag
    private void Pan(Vector2 currentTouchPosition)
    {
        Vector2 panDelta = (currentTouchPosition - lastPanPosition) * panSpeed;

        imageRect.anchoredPosition += panDelta;
        lastPanPosition = currentTouchPosition;
    }


    private Vector2 Get01NormalPointWithinRect(Vector2 localPosition)
    {
        Vector2 parentSize = GetComponent<RectTransform>().rect.size;

        Vector2 normalizedPosition = new Vector2(
            (localPosition.x + parentSize.x / 2) / parentSize.x,  // Normalized X (0 to 1)
            (localPosition.y + parentSize.y / 2) / parentSize.y   // Normalized Y (0 to 1)
        );

        return normalizedPosition;

    }

    // Zooming in/out by pinching
    // Zooming in/out by pinching, with scaling between the two touch points
    private void Zoom(Touch touch1, Touch touch2)
    {
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
        Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

        // Previous distance between the touches
        float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
        // Current distance between the touches
        float currentTouchDeltaMag = (touch1.position - touch2.position).magnitude;

        // Difference in distances between the two touches
        float deltaMagnitudeDiff = currentTouchDeltaMag - prevTouchDeltaMag;

        // Calculate scale factor based on pinch movement and zoomSpeed
        float scaleFactor = 1 + deltaMagnitudeDiff * zoomSpeed;

        // Clamp the scale factor to avoid zooming too far or too close
        scaleFactor = Mathf.Clamp(scaleFactor, 0.5f, 3.0f); // Adjust min/max values as needed

        // Get the midpoint between the two touches
        //Vector2 zoomCenter = (touch1.position + touch2.position) / 2;

        // Convert the screen point (zoom center) to local point in the RectTransform
        //Vector2 localZoomCenter;
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(imageRect, zoomCenter, cam, out localZoomCenter);
       // Calculate the difference between the current anchored position and zoom center
        //Vector2 pivotOffset = Get01NormalPointWithinRect(localZoomCenter);
        
        //Debug.Log(localZoomCenter+" "+pivotOffset);
        // Apply the scale
        imageRect.localScale *= scaleFactor;
    
        // Adjust the position to ensure scaling happens between the touch points
        //imageRect.pivot = pivotOffset;
        //Debug.Log(touch1.position +" "+touch2.position);
    }
    
    public void OnPointerDown(PointerEventData data)
    {
        if (Input.touchCount >= 2 && imageRect.pivot ==new Vector2(0.5f, 0.5f))
        {
            // Get the midpoint between the two touches
            Vector2 zoomCenter = (Input.GetTouch(0).position + Input.GetTouch(1).position) / 2;

            
            // Convert the screen point (zoom center) to local point in the RectTransform
            Vector2 localZoomCenter;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(imageRect, zoomCenter, cam, out localZoomCenter);
            // Calculate the difference between the current anchored position and zoom center
            Vector2 pivotOffset = Get01NormalPointWithinRect(localZoomCenter);
            // Adjust the position to ensure scaling happens between the touch points
            //imageRect.anchoredPosition = new Vector2(0, 0);

            imageRect.pivot = pivotOffset;
            Debug.Log(localZoomCenter +" "+pivotOffset+" "+zoomCenter);
            Debug.Log("Down ");
            imageRect.anchoredPosition = localZoomCenter;
            //imageRect.anchoredPosition = new Vector2(GetComponent<RectTransform>().rect.width / 2, GetComponent<RectTransform>().rect.height / 2);
        }
        isTouch = true;

    }
    public void OnPointerUp(PointerEventData eventData)
    {
        isTouch = false;
        if (Input.touchCount <=2)
        {
            Debug.Log("Up");

            imageRect.pivot = new Vector2(0.5f,0.5f);
            //imageRect.anchoredPosition = new Vector2(-GetComponent<RectTransform>().rect.width/2,GetComponent<RectTransform>().rect.height/2 );
            imageRect.anchoredPosition = new Vector2(0,0 );
            //imageRect.anchoredPosition = new Vector2(0,GetComponent<RectTransform>().rect.height );
            imageRect.localScale = new Vector3(1,1,1);    
        }
    }
}
