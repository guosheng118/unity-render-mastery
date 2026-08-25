using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RenderingLab
{
    public static class LabVolumeFactory
    {
        public static Volume Ensure(LabModule module)
        {
            var existing = Object.FindFirstObjectByType<Volume>();
            Volume volume;
            if (existing == null)
            {
                var go = new GameObject("GlobalVolume");
                volume = go.AddComponent<Volume>();
            }
            else
            {
                volume = existing;
            }

            volume.isGlobal = true;
            volume.priority = 0;
            volume.weight = 1f;
            if (volume.profile == null)
                volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();

            var bloom = GetOrAdd<Bloom>(volume.profile);
            bloom.active = module is LabModule.PostProcess or LabModule.NeonShowcase or LabModule.QualityTiers or LabModule.Hub;
            bloom.intensity.Override(module == LabModule.PostProcess ? 0.35f : 0.22f);
            bloom.threshold.Override(0.85f);
            bloom.scatter.Override(0.7f);
            bloom.tint.Override(new Color(1f, 0.75f, 0.9f));

            var tonemap = GetOrAdd<Tonemapping>(volume.profile);
            tonemap.mode.Override(TonemappingMode.Neutral);

            var color = GetOrAdd<ColorAdjustments>(volume.profile);
            color.postExposure.Override(0.15f);
            color.contrast.Override(12f);
            color.saturation.Override(18f);

            var vignette = GetOrAdd<Vignette>(volume.profile);
            vignette.intensity.Override(module == LabModule.PostProcess ? 0.28f : 0.18f);
            vignette.color.Override(new Color(0.08f, 0.02f, 0.12f));

            var ca = GetOrAdd<ChromaticAberration>(volume.profile);
            ca.intensity.Override(module == LabModule.PostProcess ? 0.12f : 0.04f);

            var film = GetOrAdd<FilmGrain>(volume.profile);
            film.intensity.Override(module == LabModule.PostProcess ? 0.25f : 0.08f);
            film.type.Override(FilmGrainLookup.Thin1);

            if (module == LabModule.PostProcess)
            {
                var lift = GetOrAdd<LiftGammaGain>(volume.profile);
                lift.gain.Override(new Vector4(1.02f, 1.0f, 1.08f, 0.05f));
            }

            return volume;
        }

        static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (!profile.TryGet(out T component))
                component = profile.Add<T>(true);
            component.active = true;
            return component;
        }
    }
}
