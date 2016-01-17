using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ZoomSlider : MonoBehaviour
{

    private Slider m_slider;
    public CameraZoom m_camera;

    void Start()
    {
        m_slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        m_slider.value = m_camera.Zoom;
    }
}
