using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Custom SRP")]
public class CustomSrpAsset : RenderPipelineAsset
{
    public CustomSrpSettings Settings = new()
    {
        MsaaSamples = MsaaSamples.x1,
    };

    protected override RenderPipeline CreatePipeline() => new CustomSrp(Settings);
}