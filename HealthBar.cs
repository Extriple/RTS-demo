using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    const string HpCanvas = "HpCanvas";

    [SerializeField]
    Vector3 offset;

    Slider slider;
    Unit unit;
    Transform cameraTransform;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        unit = GetComponentInParent<Unit>();
        var canvas = GameObject.FindGameObjectWithTag(HpCanvas);
        if (canvas) transform.SetParent(canvas.transform);
        //Zawracamy kompoment zawierający nazwę  "main camera"
        cameraTransform = Camera.main.transform; 
    }
    private void Update()
    {
        if (!unit)
        {
            Destroy(gameObject); 
            return;
        }
        //Pokazuje ilość życia naszej jednostki
        slider.value = unit.HealthPercent;
        //ustawianie pozycji healthbara zgodnie z pozycja naszej jednostki
        transform.position = unit.transform.position + offset;
        transform.LookAt(cameraTransform);
        //Quoterion 
        var rotation = transform.localEulerAngles;
        rotation.y = 180;
        transform.localEulerAngles = rotation;

        
    }

}   
