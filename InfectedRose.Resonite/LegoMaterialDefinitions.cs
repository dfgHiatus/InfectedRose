using System.Collections.Generic;

namespace InfectedRose.Resonite;

internal class LegoMaterialDefinitions
{
    internal static readonly HashSet<string> PBS_VertexColorMetallic = new HashSet<string>
    {
        "LEGO_FaceCreate",
        "LEGO-FadeUp",
        "Experimental Stub",
        "VC_NL_NoTex_D",
        "Model Footprint",
        "LEGO Masked NonDecal",
        "Basic",
        "Drop Shadow",
        "Multishader",
        "Basic NL NT",
        "Opaque NL VC NT NoFog",
        "Opaque NL NoFog",
        "Opaque NL VC NoFog",
        "Opaque VC NT NoFog",
        "Opaque VC NoFog",
        "LEGO NoAmbient",
        "Pet Taming Imagination Cloud",
        "Pet Taming LEGO In Cloud",
        "Two Textures Added NL VC AnimUV",
        "Darkling",
        "Fixed Function",
        "LEGO",
        "Basic NL Material",
        "Basic NL VC NT",
        "Basic NL",
        "Basic NL VC",
        "Basic NL UVAnim",
        "Basic VC NT",
        "Basic VC",
        "Basic VC UVAnim",
        "LEGO(No LOD)",
        "Head Icon",
        "LEGO_FrontEnd",
        "Powerups",
        "LEGO-Reveal",
        "LEGO-AnimUV",
        "LEGO-Item",
        "OneSidedAlpha VC",
        "OneSidedAlpha NL",
        "OneSidedAlpha NL VC",
        "OneSidedAlpha NL VC NT",
        "OneSidedAlpha AnimUV V Skinned",
        "OneSidedAlpha VC Skinned",
        "OneSidedAlpha NL Skinned",
        "OneSidedAlpha NL VC Skinned",
        "OneSidedAlpha NL VC NT Skinned",
        "OneSidedAlpha AnimUV V",
        "OneSidedAlpha NL AnimAlpha"
    };

    internal static readonly HashSet<string> PBS_VertexColorMetallic_Emissive = new HashSet<string>
    {
        "LEGO-SuperEmissive",
        "VC_NoLighting_D",
        "LEGO-Glow",
        "LEGO-Glow-IgnoreVertAlpha",
        "LEGO-ItemGlow",
        "LEGO-Emissive",
        "VC_Texture_Emissive"
    };

    internal static readonly HashSet<string> PBS_VertexColorMetallic_Transparent = new HashSet<string>
    {
        "VertColor_Alpha",
        "VertColorTex_NoLight_AlphaBlend",
        "VertColor_Alpha_Fade",
        "VertColor_NoLight_NoTex_AnimAlpha",
        "Clear Plastic"
    };

    internal static readonly HashSet<string> PBS_VertexColorMetallic_Metallic = new HashSet<string>
    {
        "ShinyGlint",
        "Polished Metal",
        "Brushed Steel",
        "Brushed Steel Item",
        "Darking Shiny Glint"
    };

    internal static readonly HashSet<string> PBS_VertexColorSpecular_Specular = new HashSet<string>
    {
        "Darkling /w Specular Shiny Glint",
        "Darkling Structure Shiny Glint"
    };

    internal static readonly HashSet<string> PBS_VertexColorSpecular = new HashSet<string>
    {
        "Darkling /w Specular",
        "Darkling Structure"
    };

    internal static readonly HashSet<string> PBS_RimMetallic = new HashSet<string>
    {
        "Terrain Mesh Rim Light"
    };

    internal static readonly HashSet<string> GrayscaleMaterial = new HashSet<string>
    {
        "LEGO-Grayscale",
        "Post Process Gray Bubble Interior Ghost",
        "PostProcess Gray Bubble",
        "Fogless GrayBubble"
    };

    internal static readonly HashSet<string> PBS_DisplaceMetallic = new HashSet<string>
    {
        "Terrain Diffuse Map Only",
        "Terrain Mesh",
        "Distortion Directional(Ocean)",
        "Distortion FX(Ocean)",
        "Distortion NoDepth(Ocean) (Alpha)",
        "Distortion(Ocean)",
        "ScrollingUV NL AnimAlpha NoFog",
        "ScrollingUV_NoLight_AimAlpha_Post",
        "ScrollingUV_NoLight_AnimAlpha",
        "ScrollingUV",
        "Two Layers Blended NL VC AnimUV",
        "Two Layers Blended VC AnimUV",
        "Two Layers Added VC AnimUV",
        "BrickWater"
    };

    internal static readonly HashSet<string> UnlitDistanceLerp = new HashSet<string>
    {
        "Distortion(Ocean) Unlit"
    };

    internal static readonly HashSet<string> Unlit = new HashSet<string>
    {
        "Orb",
        "TV Screen",
        "Flat Surf"
    };

    internal static readonly HashSet<string> Unlit_VertexColorAndAlpha = new HashSet<string>
    {
        "VertColor_NoLighting_Alpha",
        "LEGO-No Light",
        "VertColorTex_NoLight_AlphaTest",
        "Additive NoLight VertColor"
    };

    internal static readonly HashSet<string> Unlit_Overlay = new HashSet<string>
    {
        "Over Everything(Unlit)",
        "Over Everything Material Unlit"
    };
}
