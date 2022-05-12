#define MAX_LIGHTS 8

struct lightSource
{
    int enabled;
    float3 color;
    float3 position;
    float _padding0;
    float3 direction;
    float _padding1;
    float spotAngle;
    float3 attenuation;
};

struct lightParts
{
    float3 diffusePart;
    float3 specularPart;
};

interface ILightSource
{
    float3 computeDiffusePart(lightSource light, float3 toLight, float3 normal);
    float3 computeSpecularPart(lightSource light, float3 toLight, float3 normal, float3 toEye, float specularPower);
    lightParts computeLightParts(lightSource light, float3 toLight, float3 normal, float3 toEye, float specularPower);
    lightParts computeLight(lightSource light, float3 position, float3 normal, float3 eye, float specularPower);
};

class CBaseLight : ILightSource
{
    float4 color;

    float3 computeDiffusePart(lightSource light, float3 toLight, float3 normal)
    {
        float normalDotLight = max(0, dot(normal, toLight));

        return light.color * normalDotLight;
    }

    float3 computeSpecularPart(lightSource light, float3 toLight, float3 normal, float3 toEye, float specularPower)
    {
        float3 reflection = normalize(reflect(-toLight, normal));
        if (dot(toLight, normal) <= 0.0f)
            reflection = (float3) (0);
        float reflectDotEye = max(0, dot(reflection, toEye));

        return light.color * pow(reflectDotEye, specularPower);
    }

    lightParts computeLightParts(lightSource light, float3 toLight, float3 normal, float3 toEye, float specularPower)
    {
        lightParts result;

        result.diffusePart = computeDiffusePart(light, toLight, normal);
        result.specularPart = computeSpecularPart(light, toLight, normal, toEye, specularPower);

        return result;
    }

    lightParts computeLight(lightSource light, float3 position, float3 normal, float3 eye, float specularPower)
    {
        return (lightParts)0;
    }
};

class CDirectionalLight : CBaseLight
{
    lightParts computeLight(lightSource light, float3 position, float3 normal, float3 eye, float specularPower)
    {
        float3 toLight = -light.direction;
        float3 toEye = normalize(eye - position);

        lightParts result = computeLightParts(light, toLight, normal, toEye, specularPower);

        return result;
    }
};

class CFadedLight : CBaseLight
{
    float computeAttenuation(lightSource light, float distance)
    {
        return 1.0f / (light.attenuation.x + light.attenuation.y * distance + light.attenuation.z * distance * distance);
    }
};

class CPointLight : CFadedLight
{
    lightParts computeLight(lightSource light, float3 position, float3 normal, float3 eye, float specularPower)
    {
        float3 toLight = light.position - position;
        float attenuation = computeAttenuation(light, length(toLight));
        toLight = normalize(toLight);
        float3 toEye = normalize(eye - position);

        lightParts result = computeLightParts(light, toLight, normal, toEye, specularPower);
        result.diffusePart *= attenuation;
        result.specularPart *= attenuation;

        return result;
    }
};

class CSpotLight : CPointLight
{
    lightParts computeLight(lightSource light, float3 position, float3 normal, float3 eye, float specularPower)
    {
        float3 toLight = normalize(light.position - position);

        float minCos = cos(light.spotAngle);
        float maxCos = cos(minCos + 1.0f) / 2.0f;
        float cosAngle = dot(light.direction, -toLight);
        float spotIntensity = smoothstep(minCos, maxCos, cosAngle);

        lightParts result = CPointLight::computeLight(light, position, normal, eye, specularPower);

        result.diffusePart *= spotIntensity;
        result.specularPart *= spotIntensity;

        return result;
    }
};

struct pixelData
{
    float4 positionScreenSpace : SV_POSITION;
    float4 positionWorldSpace : POSITION0;
    float4 normalWorldSpace : POSITION1;
    float4 color : COLOR;
    float2 texCoord : TEXCOORD0;
};

cbuffer materialProperties : register(b0)
{
    float3 emmisiveK;
    float _paddingMP0;
    float3 ambientK;
    float _paddingMP1;
    float3 diffuseK;
    float _paddingMP2;
    float3 specularK;
    float specularPower;
};


Texture2D meshTexture : register(t0);
sampler meshSampler : register(s0);


cbuffer illumination : register(b1)
{
    float4 eyePosition;
    float3 globalAmbient;
    float _paddingI0;
    lightSource lightSources[MAX_LIGHTS];
}

cbuffer lightClasses : register(b2)
{
    CBaseLight baseLight;
    CDirectionalLight directinalLight;
    CPointLight pointLight;
    CSpotLight spotLight;
}

ILightSource lights[MAX_LIGHTS];

float4 GetTexturedColor(float4 color, float2 texCoord)
{
    float mipLevel = meshTexture.CalculateLevelOfDetail(meshSampler, texCoord);
    return meshTexture.SampleLevel(meshSampler, texCoord, mipLevel) * color;
}

float4 pixelShader(pixelData input) : SV_Target
{
    float4 materialColor = GetTexturedColor(input.color, input.texCoord);
    float4 color;
    color.xyz = (emmisiveK + ambientK * globalAmbient) * materialColor.rgb;

    lightParts parts = (lightParts)0;

    for (int i = 0; i <= MAX_LIGHTS - 1; i++)
    {
        lightSource light = lightSources[i];
        if (light.enabled > 0)
        {
            lightParts p = lights[i].computeLight(light, input.positionWorldSpace.xyz, input.normalWorldSpace.xyz, eyePosition.xyz, specularPower);
            parts.diffusePart += p.diffusePart;
            parts.specularPart += p.specularPart;
        }
    }

    float3 light = parts.diffusePart * diffuseK;
    light += parts.specularPart * specularK;

    color.xyz += light * materialColor.rgb;
    color.a = materialColor.a;

    return color;
}