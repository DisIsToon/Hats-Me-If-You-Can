using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine;


public class GradientFogEffectRenderGraph : FullscreenEffectBaseRenderGraph<GradientFogPassRenderGraph>
{
}

public class GradientFogPassRenderGraph : FullscreenPassBase<FullscreenPassDataBase>
{
    bool isEnabled()
    {
        var volumeComponent = VolumeManager.instance.stack.GetComponent<GradientFogVolumeComponent>();

        return volumeComponent.Enabled.value;
    }
    void UpdateProperties()
    {
        var volumeComponent = VolumeManager.instance.stack.GetComponent<GradientFogVolumeComponent>();


        bool overwrite = volumeComponent.OverwriteSkybox.value;
        float FogIntensity = volumeComponent.FogIntensity.value;
        float FogOffset = volumeComponent.FogOffset.value;
        float FogAmount = volumeComponent.FogAmount.value;
        Texture gradientTex = volumeComponent.GradientTexture.value;

        if (overwrite)
        {
            material.EnableKeyword("_OVERWRITESKYBOX");
        }
        else
        {
            material.DisableKeyword("_OVERWRITESKYBOX");
        }

        material.SetFloat("_Fog_Intensity", FogIntensity);
        material.SetFloat("_Fog_Offset", FogOffset);
        material.SetFloat("_Fog_Amount", FogAmount);
        material.SetTexture("_Gradient_Texture", gradientTex);
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