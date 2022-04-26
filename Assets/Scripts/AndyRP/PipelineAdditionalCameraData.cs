using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AndyRenderingPipeline
{
    public static class CameraExtensions 
    {
        public static PipelineAdditionalCameraData GetPipelineAdditionalCameraData(this Camera camera)
        {
            GameObject gameObject = camera.gameObject;
            bool componentExist = gameObject.TryGetComponent<PipelineAdditionalCameraData>(out var cameraData);
            if (!componentExist)
            {
                cameraData = gameObject.AddComponent<PipelineAdditionalCameraData>();
            }
            return cameraData;       
        }
    }

    [RequireComponent(typeof(Camera))]

    public class PipelineAdditionalCameraData : MonoBehaviour
    {
        public bool IsOverlayCamera = false;

        public bool IsOverlayClearDepth = true;

        public List<Camera> StackCameras = new List<Camera>();

        

    }





}

