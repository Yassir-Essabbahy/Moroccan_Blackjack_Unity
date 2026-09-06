using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif

public class VHSRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material material;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public Settings settings = new Settings();
    private VHSRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new VHSRenderPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null)
        {
            Debug.LogWarning("VHSRendererFeature: Material is missing.");
            return;
        }
        m_ScriptablePass.renderPassEvent = settings.renderPassEvent;
        renderer.EnqueuePass(m_ScriptablePass);
    }

    private class VHSRenderPass : ScriptableRenderPass
    {
        private Material m_Material;
        private int m_TempRTid;
        private static Material s_CopyMaterial;

        public VHSRenderPass(Settings settings)
        {
            m_Material = settings.material;
            renderPassEvent = settings.renderPassEvent;
            ConfigureInput(ScriptableRenderPassInput.Color);
            m_TempRTid = Shader.PropertyToID("_VHSTempRT");

            if (s_CopyMaterial == null)
            {
                s_CopyMaterial = Blitter.GetBlitMaterial(TextureDimension.Tex2D);
                if (s_CopyMaterial == null)
                {
                    Shader blitShader = Shader.Find("Hidden/Universal Render Pipeline/Blit") ?? Shader.Find("Hidden/BlitCopy");
                    if (blitShader != null)
                    {
                        s_CopyMaterial = new Material(blitShader) { hideFlags = HideFlags.HideAndDontSave };
                    }
                }
            }
        }

#if UNITY_6000_0_OR_NEWER
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (m_Material == null)
                return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            TextureHandle cameraColor = resourceData.activeColorTexture;

            if (!cameraColor.IsValid())
                return;

            TextureDesc descriptor = cameraColor.GetDescriptor(renderGraph);
            descriptor.depthBufferBits = 0;
            descriptor.name = "_VHSTempRT";
            descriptor.clearBuffer = false;
            descriptor.useDynamicScale = true;

            TextureHandle tempTex = renderGraph.CreateTexture(descriptor);

            if (s_CopyMaterial == null)
            {
                s_CopyMaterial = Blitter.GetBlitMaterial(TextureDimension.Tex2D);
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("VHS Copy", out var passData))
            {
                passData.source = cameraColor;
                passData.copyMaterial = s_CopyMaterial;
                builder.UseTexture(cameraColor, AccessFlags.Read);
                builder.SetRenderAttachment(tempTex, 0, AccessFlags.Write);
                builder.SetRenderFunc(static (PassData data, RasterGraphContext ctx) =>
                {
                    if (data.copyMaterial != null)
                    {
                        Blitter.BlitTexture(ctx.cmd, data.source, Vector2.one, data.copyMaterial, 0);
                    }
                    else
                    {
                        Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1f, 1f, 0f, 0f), 0f, false);
                    }
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("VHS Effect", out var passData))
            {
                passData.source = tempTex;
                passData.copyMaterial = s_CopyMaterial;
                passData.material = m_Material;
                builder.UseTexture(tempTex, AccessFlags.Read);
                builder.SetRenderAttachment(cameraColor, 0, AccessFlags.Write);
                builder.SetRenderFunc(static (PassData data, RasterGraphContext ctx) =>
                {
                    if (data.material != null)
                    {
                        Blitter.BlitTexture(ctx.cmd, data.source, Vector2.one, data.material, 0);
                    }
                    else if (data.copyMaterial != null)
                    {
                        Blitter.BlitTexture(ctx.cmd, data.source, Vector2.one, data.copyMaterial, 0);
                    }
                    else
                    {
                        Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1f, 1f, 0f, 0f), 0f, false);
                    }
                });
            }
        }

        class PassData
        {
            public TextureHandle source;
            public Material material;
            public Material copyMaterial;
        }
#else
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_Material == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("VHS Effect");

            RenderTargetIdentifier cameraColorTarget =
                renderingData.cameraData.renderer.cameraColorTarget;

            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(m_TempRTid, descriptor);

            cmd.SetGlobalTexture("_BlitTexture", cameraColorTarget);
            cmd.Blit(cameraColorTarget, m_TempRTid);

            cmd.SetGlobalTexture("_BlitTexture", m_TempRTid);
            cmd.Blit(m_TempRTid, cameraColorTarget, m_Material);

            cmd.ReleaseTemporaryRT(m_TempRTid);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
#endif
    }
}