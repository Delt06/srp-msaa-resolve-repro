using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomSrp : RenderPipeline
{
	protected override void Render(ScriptableRenderContext context, Camera[] cameras)
	{
		const int msaaSamples = 4;
		QualitySettings.antiAliasing = 0;

		foreach (Camera camera in cameras)
		{
			if (!camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters))
			{
				continue;
			}

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
				storeAction = RenderBufferStoreAction.Resolve,
				resolveTarget = BuiltinRenderTextureType.CameraTarget,
			};
			var depthAttachmentDescriptor = new AttachmentDescriptor(RenderTextureFormat.Depth)
			{
				loadAction = RenderBufferLoadAction.Clear,
				storeAction = RenderBufferStoreAction.DontCare,
			};

			attachmentDescriptors[0] = colorAttachmentDescriptor;
			attachmentDescriptors[1] = depthAttachmentDescriptor;
			context.BeginRenderPass(camera.pixelWidth, camera.pixelHeight, msaaSamples, attachmentDescriptors, 1);

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
}