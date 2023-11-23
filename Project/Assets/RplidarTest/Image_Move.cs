using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Image_Move : MonoBehaviour
{
    public float moveSpeed = 5f; // �̹����� �̵� �ӵ�

    public Image imageA; // A �̹���

    private GameObject UI_Canvas;
    private Camera UI_Camera;

    private GraphicRaycaster GR;
    private PointerEventData PED;
    private Vector3 Temp_position;

    void Start()
    {
        UI_Canvas = Manager_Sensor.instance.Get_UIcanvas();
        UI_Camera = Manager_Sensor.instance.Get_UIcamera();

        GR = UI_Canvas.GetComponent<GraphicRaycaster>();
        PED = new PointerEventData(null);

    }
    void Update()
    {
        // ���� �� ���� �Է��� �޾� �̵� ������ ���
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 moveDirection = new Vector3(horizontalInput, verticalInput, 0f).normalized;

        // �̹��� �̵�
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShootRay();
        }
    }

    void ShootRay()
    {
        Temp_position = UI_Camera.WorldToScreenPoint(this.transform.position);

        //���� ī�޶� ���� ĳ��Ʈ
        Ray ray = Camera.main.ScreenPointToRay(Temp_position);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log(hit.transform.name);
        }

        //UI ī�޶� ���� ĳ��Ʈ
        PED.position = Temp_position;
        List<RaycastResult> results = new List<RaycastResult>();
        GR.Raycast(PED, results);

        if (results.Count > 0)
        {
            Debug.Log(results[0].gameObject.name);
        }
    }
}
