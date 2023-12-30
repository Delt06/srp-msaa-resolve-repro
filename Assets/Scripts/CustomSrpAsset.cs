using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Custom SRP")]
public class CustomSrpAsset : RenderPipelineAsset
{
	protected override RenderPipeline CreatePipeline() => new CustomSrp();
}