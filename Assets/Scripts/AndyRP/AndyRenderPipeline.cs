using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
namespace AndyRenderingPipeline
{
    public class AndyRenderPipeline : RenderPipeline
    {
        // Start is called before the first frame update

        public static AndyRenderPipelineAsset Asset;
        


        const string bufferName = "Render Camera";
        ScriptableRenderContext _context;
        CommandBuffer _buffer;
        CullingResults _cullingResults;

        Material _errMat;

        static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

        static ShaderTagId[] legacyShaderTagIds = {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM")
    };

        SortingSettings _sortingSettings;
        DrawingSettings _drawingSettings;
        FilteringSettings _filteringSettings;
        public AndyRenderPipeline(AndyRenderPipelineAsset asset)
        {
            //this. = asset;
            Asset = asset;
        }
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {

            GraphicsSettings.useScriptableRenderPipelineBatching = Asset.UseSRPBatch;
            GraphicsSettings.lightsUseLinearIntensity = (QualitySettings.activeColorSpace == ColorSpace.Linear);
            this._context = context;

            _buffer = new CommandBuffer();
            _buffer.name = bufferName;


            SortCamera(cameras);

            for(int n = 0; n < cameras.Length; n++)
            {
                Camera cam = cameras[n];

                if (IsGameCamera(cam))
                {
                    RenderStackCamera(cam);
                }
                else
                {
                    RenderSceneViewCamera(cam);
                }

                
            }
        }




        private void RenderSingleCamera(Camera camera)
        {

            Setup(camera);
            PrepareForSceneWindow(camera);
            if (!Culling(camera))
            {
                return;
            }

            DrawOpaqueGeometry(camera);
            DrawTransparentGeometry(camera);
            DrawUnsupportedShaders(camera);

#if UNITY_EDITOR
            DrawGizmo(camera);
            
#endif
            Submit();
        }

        private void DrawUnsupportedShaders(Camera camera)
        {
            _sortingSettings = new SortingSettings(camera);
            _sortingSettings.criteria = SortingCriteria.CommonOpaque;

            _drawingSettings = new DrawingSettings(legacyShaderTagIds[0], _sortingSettings);
            _filteringSettings = new FilteringSettings(RenderQueueRange.all);

            if (_errMat == null)
            {
                _errMat =
                    new Material(Shader.Find("Hidden/InternalErrorShader"));
            }
            _drawingSettings = new DrawingSettings(
                legacyShaderTagIds[0], new SortingSettings(camera)
            )
            {
                overrideMaterial = _errMat
            };

            for (int n = 1; n< legacyShaderTagIds.Length; n++)
            {
                _drawingSettings.SetShaderPassName(n, legacyShaderTagIds[n]);
            }


            _context.DrawRenderers(_cullingResults, ref _drawingSettings, ref _filteringSettings);
           
        }

        private void DrawOpaqueGeometry(Camera camera)
        {

            _sortingSettings = new SortingSettings(camera);
            _sortingSettings.criteria = SortingCriteria.CommonOpaque;

            _drawingSettings = new DrawingSettings(unlitShaderTagId, _sortingSettings);
            _filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            


            _context.DrawRenderers(_cullingResults, ref _drawingSettings, ref _filteringSettings);
            _context.DrawSkybox(camera);

        }

        private void DrawTransparentGeometry(Camera camera)
        {

            _sortingSettings.criteria = SortingCriteria.CommonTransparent;
            _drawingSettings.sortingSettings = _sortingSettings;
            _filteringSettings.renderQueueRange = RenderQueueRange.transparent;

            
            _context.DrawRenderers(_cullingResults, ref _drawingSettings, ref _filteringSettings);
        }



        private void Setup(Camera cam)
        {
            _context.SetupCameraProperties(cam);
            _buffer.ClearRenderTarget(true, true, Color.clear);
            _buffer.BeginSample(bufferName);
            ExecuteBuffer(_buffer);
        }

        private void RenderSceneViewCamera(Camera camera)
        {
            
        }

        private void RenderStackCamera(Camera camera)
        {
            PipelineAdditionalCameraData addtionData = camera.GetPipelineAdditionalCameraData();
            if (addtionData.IsOverlayCamera)
            {
                return;
            }
            
            RenderSingleCamera(camera);
        }

        private void SortCamera(Camera[] cameras)
        {
            if(cameras.Length > 1)
            {
                Array.Sort(cameras, cameraComparison);
            }
        }

        private void Submit()
        {
            _buffer.EndSample(bufferName);
            ExecuteBuffer(_buffer);
            _context.Submit();
        }

        private bool IsGameCamera(Camera cam)
        {
            return cam.cameraType == CameraType.Game;
        }
                

        private void ExecuteBuffer(CommandBuffer buffer)
        {
            _context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        private bool Culling(Camera cam)
        {
            if(cam.TryGetCullingParameters(out ScriptableCullingParameters sc))
            {
                _cullingResults = _context.Cull(ref sc);
                return true;
            }

            return false;
        }


        private void DrawGizmo(Camera cam)
        {
            if (Handles.ShouldRenderGizmos())
            {
                _context.DrawGizmos(cam, GizmoSubset.PreImageEffects);
                _context.DrawGizmos(cam, GizmoSubset.PostImageEffects);
            }
        }
        private void PrepareForSceneWindow(Camera cam)
        {
            if (cam.cameraType == CameraType.SceneView)
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(cam);
            }
        }
        Comparison<Camera> cameraComparison = (camera1, camera2) => { return (int)camera1.depth - (int)camera2.depth; };
    }
}
    
