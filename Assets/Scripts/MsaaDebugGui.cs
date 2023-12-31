using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MsaaDebugGui : MonoBehaviour
{
    private static readonly MsaaSamples[] ShownSamples =
    {
        MsaaSamples.x1,
        MsaaSamples.x2,
        MsaaSamples.x4,
        MsaaSamples.x8,
    };
    public Transform Layout;
    public Button Button;

    private readonly List<Button> _buttons = new();

    private void Start()
    {
        _buttons.Add(Button);

        while (_buttons.Count < ShownSamples.Length)
        {
            _buttons.Add(Instantiate(Button, Layout));
        }

        for (int i = 0; i < _buttons.Count; i++)
        {
            Button button = _buttons[i];
            int buttonIndex = i;
            button.GetComponentInChildren<Text>().text = $"MSAA {ShownSamples[i]}";
            button.onClick.AddListener(() =>
                {
                    RenderPipeline currentPipeline = RenderPipelineManager.currentPipeline;
                    if (currentPipeline is CustomSrp customSrp)
                    {
                        customSrp.Settings.MsaaSamples = ShownSamples[buttonIndex];
                    }

                    RefreshSelection();
                }
            );
        }

        RefreshSelection();
    }

    private void RefreshSelection()
    {
        MsaaSamples? msaaSamples = null;
        RenderPipeline currentPipeline = RenderPipelineManager.currentPipeline;
        if (currentPipeline is CustomSrp customSrp)
        {
            msaaSamples = customSrp.Settings.MsaaSamples;
        }

        for (int i = 0; i < _buttons.Count; i++)
        {
            Button button = _buttons[i];
            button.interactable = msaaSamples != ShownSamples[i];
        }
    }
}