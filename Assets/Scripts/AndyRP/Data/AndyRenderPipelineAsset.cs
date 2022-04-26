using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEngine.Rendering;

namespace AndyRenderingPipeline
{
    public enum ShadowQuality
    {
        Disabled,
        HardShadows,
        SoftShadows,
    }


    public enum ShadowResolution
    {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096
    }



    [CreateAssetMenu(menuName = "Rendering/Andy Render Pipeline")]
    public class AndyRenderPipelineAsset : RenderPipelineAsset
    {

        public bool UseSRPBatch;
        public bool UseDynamicBatch;

        public float RenderScale = 1;

        protected override RenderPipeline CreatePipeline()
        {
            return new AndyRenderPipeline(this);
        }
    }

}
    
