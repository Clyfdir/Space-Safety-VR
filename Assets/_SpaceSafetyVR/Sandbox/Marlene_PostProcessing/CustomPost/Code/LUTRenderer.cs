using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LUTBlendRenderer : ScriptableRendererFeature
{
    [SerializeField] 
    private Shader lutBlendShader;

    private Material lutBlendMaterial;
    private Pass pass;

    class Pass : ScriptableRenderPass
    {
        private readonly Material material;
        private RTHandle tempColor;
        private RTHandle source;
        private LUTBlend settings;

        public Pass(Material mat)
        {
            material = mat;
        }

        public void Setup(RTHandle src)
        {
            source = src;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            source = renderingData.cameraData.renderer.cameraColorTargetHandle;
            
            RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref tempColor, desc, name: "_LUTBlendTemp");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!material) return;

            var stack = VolumeManager.instance.stack;
            settings = stack.GetComponent<LUTBlend>();
            if (settings == null || !settings.IsActive()) return;

            var cmd = CommandBufferPool.Get("LUTBlend");

            // Pass parameters to shader
            material.SetTexture("_LUT_A", settings.lutA.value);
            material.SetTexture("_LUT_B", settings.lutB.value);
            material.SetFloat("_Blend", settings.blend.value);
            material.SetFloat("_ExposureOffset", settings.exposureOffset.value);
            material.SetFloat("_ContrastBoost", settings.contrastBoost.value);
            material.SetColor("_Tint", settings.tint.value);
            material.SetFloat("_LutSize", (float)settings.lutSize.value);

            // Bind source explicitly
            material.SetTexture("_SourceTex", source);

            // Proper RTHandle-safe blit
            Blitter.BlitCameraTexture(cmd, source, tempColor, material, 0);
            Blitter.BlitCameraTexture(cmd, tempColor, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // RTHandles cleaned up automatically
        }
    }

    public override void Create()
    {
        if (lutBlendShader == null)
            lutBlendShader = Shader.Find("Hidden/Custom/LUTBlend");

        if (lutBlendShader != null)
        {
            lutBlendMaterial = CoreUtils.CreateEngineMaterial(lutBlendShader);
            pass = new Pass(lutBlendMaterial)
            {
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
            };
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (pass != null)
        {
            renderer.EnqueuePass(pass);
        }
    }
}
