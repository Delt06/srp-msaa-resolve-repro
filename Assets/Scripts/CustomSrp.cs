using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomSrp : RenderPipeline
{
    public CustomSrpSettings Settings;

    public CustomSrp(CustomSrpSettings settings) => Settings = settings;

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        QualitySettings.antiAliasing = 0;

        foreach (Camera camera in cameras)
        {
            if (!camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters))
            {
                continue;
            }

            if (camera.cameraType != CameraType.Game)
            {
                continue;
            }

            RenderCamera(camera, ref context, ref cullingParameters);
        }
    }

    private void RenderCamera(Camera camera,
        ref ScriptableRenderContext context,
        ref ScriptableCullingParameters cullingParameters)
    {
        int cameraPixelWidth = camera.pixelWidth;
        int cameraPixelHeight = camera.pixelHeight;

        var renderTextureDescriptor =
            new RenderTextureDescriptor(cameraPixelWidth, cameraPixelHeight, RenderTextureFormat.Default)
            {
                msaaSamples = (int) Settings.MsaaSamples,
            };
        int msaaSamples = SystemInfo.GetRenderTextureSupportedMSAASampleCount(renderTextureDescriptor);

        CullingResults cullingResults = context.Cull(ref cullingParameters);

        context.SetupCameraProperties(camera);

        {
            CommandBuffer cmd = CommandBufferPool.Get();

            cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget, RenderBufferLoadAction.DontCare,
                RenderBufferStoreAction.Store
            );
            cmd.ClearRenderTarget(false, true, Color.magenta);
            context.ExecuteCommandBuffer(cmd);

            CommandBufferPool.Release(cmd);
        }


        var attachmentDescriptors = new NativeArray<AttachmentDescriptor>(2, Allocator.Temp);
        var colorAttachmentDescriptor = new AttachmentDescriptor(RenderTextureFormat.Default)
        {
            loadAction = RenderBufferLoadAction.DontCare,
        };

        if (msaaSamples > 1)
        {
            colorAttachmentDescriptor.storeAction = RenderBufferStoreAction.Resolve;
            colorAttachmentDescriptor.resolveTarget = BuiltinRenderTextureType.CameraTarget;
        }
        else
        {
            colorAttachmentDescriptor.storeAction = RenderBufferStoreAction.Store;
            colorAttachmentDescriptor.loadStoreTarget = BuiltinRenderTextureType.CameraTarget;
        }

        var depthAttachmentDescriptor = new AttachmentDescriptor(RenderTextureFormat.Depth)
        {
            loadAction = RenderBufferLoadAction.Clear,
            storeAction = RenderBufferStoreAction.DontCare,
        };

        attachmentDescriptors[0] = colorAttachmentDescriptor;
        attachmentDescriptors[1] = depthAttachmentDescriptor;
        context.BeginRenderPass(cameraPixelWidth, cameraPixelHeight, msaaSamples, attachmentDescriptors, 1);

        var colorIndices = new NativeArray<int>(1, Allocator.Temp);
        colorIndices[0] = 0;
        context.BeginSubPass(colorIndices, false, false);

        context.DrawSkybox(camera);

        var drawingSettings = new DrawingSettings(new ShaderTagId("SRPDefaultUnlit"), new SortingSettings(camera));
        FilteringSettings filteringSettings = FilteringSettings.defaultValue;
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

        context.EndSubPass();
        context.EndRenderPass();

        context.Submit();
    }
}