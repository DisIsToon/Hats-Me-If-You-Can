
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine;


public class OutlineEffectRenderGraph : FullscreenEffectBaseRenderGraph<OutlineEffectPassRenderGraph>
{
}

public class OutlineEffectPassRenderGraph : FullscreenPassBase<FullscreenPassDataBase>
{

    bool isEnabled()
    {
        var volumeComponent = VolumeManager.instance.stack.GetComponent<OutlineVolumeComponent>();

        return volumeComponent.Enabled.value;
    }
    void UpdateProperties()
    {
        var volumeComponent = VolumeManager.instance.stack.GetComponent<OutlineVolumeComponent>();

        bool enabled = volumeComponent.Enabled.value;
        Color lineCol = volumeComponent.LineColor.value;

        material.SetColor("_Color", lineCol);
        if (!enabled)
        {
            return;
        }
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (isEnabled())
        {
            UpdateProperties();
            base.RecordRenderGraph(renderGraph, frameData);
        }

    }
}