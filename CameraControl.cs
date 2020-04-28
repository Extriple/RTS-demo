using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour
{


    public float cameraSpeed, zoomSpeed, groundHeight;
    public Vector2 cameraHeightMinMax;
    public Vector2 cameraRotationMinMax;

    //Atrybut pola -- ograniczamy zmienne w odniesieniu tylko do inspektora
    [Range(0, 1)]
    public float zoomLerp = 0.1f;
    //W jakim odległosci będziemy przewijać kamerę

    [Range(0, 0.2f)]
    public float cursorTreshold;
    Vector2 mousePos, mousePosScreen, keyboardInput, mouseScroll;
    bool isCursorinGameScreen;
    Rect selectionRect, boxRect;
    //Dodawanie do kamery podglądu na zazczone jednostki
    List<Unit> selectedUnits = new List<Unit>();


    RectTransform SelectionBox;
    new Camera camera;

    private void Awake()
    {
        SelectionBox = GetComponentInChildren<Image>(true).transform as RectTransform;     //true zaznaczamy wtedy gdy chcemy aby były wyłączone obiekty
        SelectionBox.gameObject.SetActive(false);
        camera = GetComponent<Camera>();
    }
    private void Update()
    {
        UpdateMovement();
        UpdateZoom();
        UpdateClicks();
        
    }


    void UpdateMovement()
    {
        keyboardInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        mousePos = Input.mousePosition;
        //Przekazujemy infomracje, czy kursor znajduje sie na ekranie gry 
        mousePosScreen = camera.ScreenToViewportPoint(mousePos);
        isCursorinGameScreen = mousePosScreen.x >= 0 && mousePosScreen.x < 1 && mousePosScreen.y >= 0 && mousePosScreen.y < 1;
        Vector2 movementDirection = keyboardInput;

        if (isCursorinGameScreen)
        {
            //Jeżeli x jest miejszy od naszeho Tresholdu to znaczy, że poruszamy się w lewo i na odwrót
            // x = 0.1
            //tr = 0.2 ==? 0.1/0.2= ?? bardzo małe liczby dlatego musimy przez podzielamy postawić jedynkę

            if (mousePosScreen.x < cursorTreshold) movementDirection.x -= 1 - mousePosScreen.x / cursorTreshold;
            if (mousePosScreen.x > 1 - cursorTreshold) movementDirection.x += 1 - (1 - mousePosScreen.x) / (cursorTreshold);
            if (mousePosScreen.y < cursorTreshold) movementDirection.y -= 1 - mousePosScreen.y / cursorTreshold;
            if (mousePosScreen.y > 1 - cursorTreshold) movementDirection.y += 1 - (1 - mousePosScreen.y) / (cursorTreshold);

        }

        //Kontrolowanie kamery, jeżeli tylko nadamy w Unity predkość przesuwania się
        var deltaPosition = new Vector3(movementDirection.x, 0, movementDirection.y);
        deltaPosition *= cameraSpeed * Time.deltaTime;
        transform.position += deltaPosition;

    }

    void UpdateZoom()
    {
        mouseScroll = Input.mouseScrollDelta;
        //Zakładamy, że jest to x
        float zoomDelta = mouseScroll.y * zoomSpeed * Time.deltaTime;
        //Wartość zooma
        zoomLerp = Mathf.Clamp01(zoomLerp + zoomDelta); //Jeżeli ta operacja przekraczy 1 to do samego Zoomlerp i tak przypisze się się 1 i odwrotnie. 

        //Aktualna pozycja
        var position = transform.localPosition;
        position.y = Mathf.Lerp(cameraHeightMinMax.y, cameraHeightMinMax.x, zoomLerp) + groundHeight; //Interpolacja liniowa
        transform.localPosition = position;

        var rotation = transform.localEulerAngles;
        rotation.x = Mathf.Lerp(cameraRotationMinMax.y, cameraRotationMinMax.x, zoomLerp);
        transform.localEulerAngles = rotation;
    }

    void UpdateClicks()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectionBox.gameObject.SetActive(true);
            selectionRect.position = mousePos;
                
        }
        else if (Input.GetMouseButtonUp(0))
        {
            SelectionBox.gameObject.SetActive(false);
        }

        if (Input.GetMouseButton(0))
        {
            //Tworzenie siatki zaznaczania jednostek
            selectionRect.size = mousePos - selectionRect.position;
            //Pozycja kotwicy 
            boxRect = AbsRect(selectionRect);
            SelectionBox.anchoredPosition = boxRect.position;
            SelectionBox.sizeDelta = boxRect.size;
            UpdateSelecting();

        }
        if (Input.GetMouseButtonDown(1))
        {
            GiveCommands();
           //SelectionBox.gameObject.SetActive(true);
           //selectionRect.position = mousePos;

        }
    }

    void UpdateSelecting()
    {
        //Czyścimy całą listę
        selectedUnits.Clear();
        //Musimy przejść przez wszystkie elementy, które są do zaznaczenia
        foreach (Unit unit in Unit.SelectableUnits)
        {
            Debug.Log(unit.ToString());
            //Jeżeli jednoskti nie są naszymi wybranymi jednostkami, to przejdź do kolejnego
            if (!unit) continue;
            //Pobieramy pozycję
            var pos = unit.transform.position;
            //Przekonwertujemy pozycję jednostki na pozycję na ekranie 
            var posScreen = camera.WorldToScreenPoint(pos);
            bool inRect = IsPointInRect(boxRect, posScreen);
            //Rzutowanie na interface
            (unit as ISelectable).SetSelected(inRect);
            if (inRect)
            {
                selectedUnits.Add(unit);
            }

        }
    }
    //Funkcja pomocnicza
    bool IsPointInRect(Rect rect, Vector2 point)
    {
        return point.x >= rect.position.x && point.x <= (rect.position.x + rect.size.x) &&
            point.y >= rect.position.y && point.y <= (rect.position.y + rect.size.y);
    }

    // Zwracamy przetransportowany prostokąt czyt. siatkę do zaznaczania postaci aby, móc zazaczać jednostki, poruszająć sie w góre i w prawo( wtedy wartości są ujmmne)
    Rect AbsRect(Rect rect)
    {
        if (rect.width < 0)
        {
            rect.x += rect.width;
            rect.width *= -1;
        }
        if(rect.height < 0)
        {
            rect.y += rect.height;
            rect.height *= -1;
        }
        return rect;

    }

    Ray ray;
    RaycastHit rayHit;
    [SerializeField]
    LayerMask commandLayerMask = -1;

    void GiveCommands()
    {
        
        ray = camera.ViewportPointToRay(mousePosScreen);
        if(Physics.Raycast(ray, out rayHit, 1000f, commandLayerMask))
        {
            object commandData = null;
            //Sprawdzamy czym jest collider, w który trafiliśmy
            if(rayHit.collider is TerrainCollider)
            {
                 Debug.Log("Terrain"+ rayHit.point.ToString());   
                commandData = rayHit.point;
            }
            else
            {
                Debug.Log(rayHit.collider);   
                commandData = rayHit.collider.gameObject.GetComponent<Unit>();
            }
            GiveCommands(commandData);

        }

    }
    void GiveCommands(object dataCommnad)
    {
        foreach (Unit unit in selectedUnits)
            unit.SendMessage("Command", dataCommnad, SendMessageOptions.DontRequireReceiver);
    }
    

}























// ViewportPointToRay --> procentowa wielkość  ekranu w przypadku kliknięcia
// Anchored --> Pozycja zakotwiczona to pozycja osi RectTransform z uwzględnieniem punktu odniesienia kotwicy. Punktem odniesienia dla kotwicy jest pozycja kotwic.
//dodanie przedrostka out oznacza, że to bedzie kolejny parametr jaki zwraca nasza funkcja
