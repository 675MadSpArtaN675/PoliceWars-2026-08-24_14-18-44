using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ScreenResolutionController : MonoBehaviour
{
    public static ScreenResolutionController Instance { get; private set; }


    public float scaleCoffsent = 1.15f;
    public Vector2 GlobalResolution = new Vector2(1920, 1080);
    public Vector2 CoffsentScale { get; private set; }
    public float GlobalSize = 6;

    public Vector2 NowResolution { get; set; }
    public GameObject CameraSlot;

    protected Vector2 _previous_scale_coffsent;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        NowResolution = new Vector2(Screen.width, Screen.height);

        CoffsentScale = CalculateResolutionCoffsent(NowResolution);
        _previous_scale_coffsent = CoffsentScale;

        Debug.Log($"Screen. Resolution: {NowResolution}. Coffsent: {CoffsentScale}. Previous coffsent: {_previous_scale_coffsent}.");
        TransformCamera();
    }

    private void FixedUpdate()
    {
        NowResolution = new Vector2(Screen.width, Screen.height);
        Vector2 resolution = CalculateResolutionCoffsent(NowResolution);

        Debug.Log($"Screen. Resolution: {NowResolution}. Coffsent: {resolution}. Previous coffsent: {_previous_scale_coffsent}.");
        if (resolution != _previous_scale_coffsent)
        {
            CoffsentScale = resolution;

            TransformCamera();

            _previous_scale_coffsent = CoffsentScale;
        }
    }

    protected void TransformCamera()
    {
        if (CameraSlot != null && CameraSlot.GetComponent<Camera>() is Camera camera)
        {
            if (CoffsentScale.x < 1.0f && CoffsentScale.y < 1.0f) {
                float coffsent = scaleCoffsent;

                Debug.Log($"Screen. Resolution: {NowResolution}. Result coffsent: {coffsent}");
                camera.orthographicSize = GlobalSize * coffsent;
            }
            else
            {
                camera.orthographicSize = GlobalSize;
            }
        }
    }

    protected Vector2 CalculateResolutionCoffsent(Vector2 resolution)
    {
        return new Vector2(resolution.x / GlobalResolution.x, resolution.y / GlobalResolution.y);
    }
}
