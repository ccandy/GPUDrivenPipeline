using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace AndyRenderingPipeline
{
    public struct LightData
    {
        public int MainLightIndex;
        public int AdditionLightsCount;
        public int MaxPerObjectAdditionalLightCount;
        
        public NativeArray<VisibleLight> VisibleLights;
    }

    public struct RenderingData
    {
        public CullingResults CullResults;
        public CameraData RenderCameraData;
        public LightData RenderLightData;

        public bool SupportDynamicBatching;


        public static RenderingData CreateRenderingData(PipelineAdditionalCameraData additionalCameraData)
        {
            RenderingData renderingData = new RenderingData();

            return renderingData;
        }

    }


    public struct CameraData
    {
        Matrix4x4 _viewMatrix;
        public Matrix4x4 ViewMatrix
        {
            set
            {
                _viewMatrix = value;
            }

            get
            {
                return _viewMatrix;
            }
        }

        Matrix4x4 _projMatrix;
        public Matrix4x4 ProjectionMatrix
        {
            set
            {
                _projMatrix = value;
            }
             
            get
            {
                return _projMatrix;
            }
        }

        public void SetViewAndProjMatrix(Matrix4x4 viewMat, Matrix4x4 projMat)
        {
            _viewMatrix = viewMat;
            _projMatrix = projMat;
        }

        public Camera RenderCamera;
        public CameraRenderType RenderType;
        public RenderTexture TargetTexture;

        public Rect PixelRect;
        public int PixelWidth;
        public int PixelHeight;

        public float RenderScale;
        public float AspectRatio;

        public bool ClearDepth;
        public CameraType RenderCameraType;

        public bool RequireDepthTexture;
        public bool RequireOpaqueTexture;
        public bool PostProcessingRequiresDepthTexture;

        public bool IsClearDepth;
        public bool IsOverlay;

        public Color ClearColor;
        

        public bool IsSenceViewCamera()
        {
            return RenderCameraType == CameraType.SceneView;
        }

        public bool IsPreviewCamera()
        {
            return RenderCameraType == CameraType.Preview;
        }

        public bool IsRenderPassSupportedCamera()
        {
            return (RenderCameraType == CameraType.Game || RenderCameraType == CameraType.Preview); 
        }

        public bool CameraProjectionMatrixFlipped()
        {
            return false;
        }

        public SortingCriteria DefaultOpaqueSortFlags;

        public float MaxShadowDistance;

        public bool EnablePostProcess;

        public AndyScriptRender AndyRender;

        public Vector3 WorldSpaceCameraPos;
    
        public static CameraData CreateCameraData(PipelineAdditionalCameraData additiondata, Camera cam)
        {
            CameraData cameraData = new CameraData();

            
            cameraData.ViewMatrix = cam.worldToCameraMatrix;
            cameraData.ProjectionMatrix = cam.projectionMatrix;
            cameraData.TargetTexture = cam.targetTexture;
            cameraData.RenderCameraType = cam.cameraType;
            cameraData.IsClearDepth = !(!additiondata.IsOverlayClearDepth && additiondata.IsOverlayCamera);
            cameraData.IsOverlay = additiondata.IsOverlayCamera;
            cameraData.ClearColor = CoreUtils.ConvertSRGBToActiveColorSpace(cam.backgroundColor);

            Rect cameraRect = cam.rect;
            cameraData.PixelRect = cam.pixelRect;
            cameraData.PixelHeight = cam.pixelHeight;
            cameraData.PixelWidth = cam.pixelWidth;

            cameraData.AspectRatio = (float)cam.pixelWidth / (float)cam.pixelHeight;
            const float kRenderScaleThreshold = 0.05f;

            AndyRenderPipelineAsset asset = AndyRenderPipeline.Asset;
            cameraData.RenderScale = (Mathf.Abs(1.0f - asset.RenderScale) < kRenderScaleThreshold) ? 1.0f : asset.RenderScale;


            return cameraData;
        }
    
    }



    public struct ShadowData
    {
        public bool SupportsMainLightShadows;

        public int MainLightShadowmapWidth;
        public int MainLightShadowmapHeight;
        public int MainLightShadowCascadeCount;

        public Vector3 MainLightShadowCascadeSplit;
        public bool SupportSoftShadows;

        public List<int> Resolution;
        public List<Vector4> Bias;
    }

    internal static class ShaderPropertyId
    {
        public static readonly int sourceTex = Shader.PropertyToID("_SourceTex");
        public static readonly int scaleBias = Shader.PropertyToID("_ScaleBias");
    }

}
    
