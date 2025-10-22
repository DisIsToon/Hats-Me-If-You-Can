using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[VolumeComponentMenu("Custom/GradientFog")]
public class GradientFogVolumeComponent : VolumeComponent
{
    public BoolParameter Enabled = new BoolParameter(false);
    public BoolParameter OverwriteSkybox = new BoolParameter(false);
    public FloatParameter FogAmount = new FloatParameter(0);
    public FloatParameter FogOffset = new FloatParameter(0);
    public TextureParameter GradientTexture = new TextureParameter(null);
    public FloatParameter FogIntensity = new FloatParameter(0);
}

