using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class KawaseBlur : ScriptableRendererFeature
{
    [System.Serializable]
    public class KawaseBlurSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material blurMaterial = null;

        [Range(2, 15)] public int blurPasses = 1;

        [Range(1, 4)] public int downsample = 1;
        public bool copyToFramebuffer;
        public string targetName = "_blurTexture";
    }

    public KawaseBlurSettings settings = new KawaseBlurSettings();

    public class KawasePass : ScriptableRenderPass
    {
        public static bool enabled;
        public Material blurMaterial;
        public int passes;
        public int downsample;
        public bool copyToFramebuffer;
        public string targetName;
        private string _profilerTag;

        private int _tmpId1;
        private int _tmpId2;
        private RenderTargetIdentifier _tmpRT1;
        private RenderTargetIdentifier _tmpRT2;

        public KawasePass(string profilerTag)
        {
            _profilerTag = profilerTag;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            var width = cameraTextureDescriptor.width / downsample;
            var height = cameraTextureDescriptor.height / downsample;
            var format = cameraTextureDescriptor.graphicsFormat;

            _tmpId1 = Shader.PropertyToID("tmpBlurRT1");
            _tmpId2 = Shader.PropertyToID("tmpBlurRT2");
            cmd.GetTemporaryRT(_tmpId1, width, height, 0, FilterMode.Bilinear, format);
            cmd.GetTemporaryRT(_tmpId2, width, height, 0, FilterMode.Bilinear, format);
            _tmpRT1 = new RenderTargetIdentifier(_tmpId1);
            _tmpRT2 = new RenderTargetIdentifier(_tmpId2);

            ConfigureTarget(_tmpRT1);
            ConfigureTarget(_tmpRT2);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cam = renderingData.cameraData.camera;
            if (cam.cameraType != CameraType.Game || !enabled) return;

            CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            // first pass
            cmd.GetTemporaryRT(_tmpId1, opaqueDesc, FilterMode.Bilinear);
            cmd.SetGlobalFloat("_offset", 1.5f);
            cmd.Blit(renderingData.cameraData.renderer.cameraColorTarget, _tmpRT1, blurMaterial);

            for (var i = 1; i < passes - 1; i++)
            {
                cmd.SetGlobalFloat("_offset", 0.5f + i);
                cmd.Blit(_tmpRT1, _tmpRT2, blurMaterial);
                // pingpong
                var rttmp = _tmpRT1;
                _tmpRT1 = _tmpRT2;
                _tmpRT2 = rttmp;
            }

            // final pass
            cmd.SetGlobalFloat("_offset", 0.5f + passes - 1f);
            if (copyToFramebuffer)
            {
                cmd.Blit(_tmpRT1, renderingData.cameraData.renderer.cameraColorTarget, blurMaterial);
            }
            else
            {
                cmd.Blit(_tmpRT1, _tmpRT2, blurMaterial);
                cmd.SetGlobalTexture(targetName, _tmpRT2);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }
    }

    private KawasePass _scriptablePass;

    public override void Create()
    {
        _scriptablePass = new KawasePass("KawaseBlur");
        _scriptablePass.blurMaterial = settings.blurMaterial;
        _scriptablePass.passes = settings.blurPasses;
        _scriptablePass.downsample = settings.downsample;
        _scriptablePass.copyToFramebuffer = settings.copyToFramebuffer;
        _scriptablePass.targetName = settings.targetName;
        _scriptablePass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_scriptablePass);
    }
}