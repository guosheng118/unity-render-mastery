using System.Collections;
using UnityEngine;

namespace RenderLab
{
    public class CharacterSwitcher : MonoBehaviour
    {
        [System.Serializable]
        public class CharacterTheme
        {
            public Color tintA = new Color(0.25f, 0.75f, 1f, 1f);
            public Color tintB = new Color(1f, 0.54f, 0.17f, 1f);
            public Cubemap skyCube;
            public Color floorColor = new Color(0.14f, 0.20f, 0.38f, 1f);
            [ColorUsage(true, true)]
            public Color cloudColorA = new Color(0.32f, 0.68f, 1f, 0.02f);
            [ColorUsage(true, true)]
            public Color cloudColorB = new Color(0f, 0.66f, 1f, 0.02f);
            [ColorUsage(true, true)]
            public Color cloudBaseColor = new Color(0.36f, 0.61f, 1f, 0.79f);
        }

        static readonly int TintAId = Shader.PropertyToID("_TintA");
        static readonly int TintBId = Shader.PropertyToID("_TintB");
        static readonly int SkyTexId = Shader.PropertyToID("_Tex");
        static readonly int SkyTexBId = Shader.PropertyToID("_TexB");
        static readonly int SkyBlendId = Shader.PropertyToID("_SkyBlend");
        static readonly int FloorCubeId = Shader.PropertyToID("_SkyCube");
        static readonly int FloorCubeBId = Shader.PropertyToID("_SkyCubeB");
        static readonly int FloorColorId = Shader.PropertyToID("_FloorColor");
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");
        static readonly int FadeId = Shader.PropertyToID("_Fade");
        static readonly int GlitchId = Shader.PropertyToID("_Glitch");
        static readonly int GlitchHeightId = Shader.PropertyToID("_GlitchHeight");

        public Transform[] characters;
        public CharacterTheme[] themes;
        public int starIndex;
        public Camera worldCamera;
        public float slideDistance = 3f;
        [Min(0.01f)]
        public float duration = 0.5f;
        [Tooltip("角色开始滑动后，过多少秒才开始天空/地板/星星/云的过渡。")]
        [Min(0f)]
        public float themeDelay = 0.3f;
        [Tooltip("天空/地板/星星/云交叉过渡的时长（秒），和角色 Duration 独立。")]
        [Min(0.01f)]
        public float themeDuration = 0.25f;
        public Transform directionalLight;
        [Tooltip("按下 1 后，平行光绕世界 Y 轴逆时针转一圈的时长（秒）。")]
        [Min(0.01f)]
        public float lightOrbitDuration = 4f;
        [Tooltip("按住 A/D 时，当前角色绕世界 Y 轴旋转的速度（度/秒）。A 顺时针，D 逆时针。")]
        [Min(0f)]
        public float characterRotateSpeed = 90f;
        [Tooltip("按住 W/S 时，平行光绕世界 Y 轴旋转的速度（度/秒）。W 顺时针，S 逆时针。")]
        [Min(0f)]
        public float lightRotateSpeed = 90f;
        [Tooltip("角色滑入到位后，故障扫光从上到下走完的时长（秒）。")]
        [Min(0.01f)]
        public float glitchDuration = 0.7f;
        [Tooltip("滑入到位时的 Fade，停顿后再跟着扫光从该值过渡到 1。")]
        [Range(0f, 1f)]
        public float glitchArriveFade = 0.2f;
        [Tooltip("滑入停在 Arrive Fade 之后、开始扫光之前的停顿（秒）。")]
        [Min(0f)]
        public float glitchHold = 0.2f;
        [Tooltip("扫光高度的加速：1 匀速，越大开头越慢、后面越快。")]
        [Range(1f, 4f)]
        public float glitchEase = 2f;
        public Renderer starRenderer;
        public Renderer floorRenderer;
        public ParticleSystem cloudParticles;

        int _index;
        bool _busy;
        bool _lightBusy;
        Coroutine _glitchCo;
        Vector3[] _home;
        Renderer[][] _charRenderers;
        MaterialPropertyBlock _starBlock;
        MaterialPropertyBlock _floorBlock;
        MaterialPropertyBlock _cloudBlock;
        MaterialPropertyBlock _fadeBlock;
        ParticleSystemRenderer _cloudRenderer;
        ParticleSystem.Particle[] _cloudParticleBuffer;
        Material _skyboxShared;
        Material _skyboxRuntime;

        void Start()
        {
            if (worldCamera == null)
                worldCamera = FindFirstObjectByType<Camera>();

            if (starRenderer == null)
            {
                GameObject star = GameObject.Find("Sky_Star_Mesh");
                if (star != null)
                    starRenderer = star.GetComponent<Renderer>();
            }

            if (cloudParticles == null)
            {
                GameObject cloud = GameObject.Find("Cloud");
                if (cloud != null)
                    cloudParticles = cloud.GetComponent<ParticleSystem>();
            }

            if (cloudParticles != null)
                _cloudRenderer = cloudParticles.GetComponent<ParticleSystemRenderer>();

            if (directionalLight == null)
            {
                GameObject lightGo = GameObject.Find("Directional Light");
                if (lightGo != null)
                    directionalLight = lightGo.transform;
            }

            _index = characters != null && characters.Length > 0
                ? Mathf.Clamp(starIndex, 0, characters.Length - 1)
                : 0;

            _home = new Vector3[characters.Length];
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i] == null)
                    continue;
                _home[i] = characters[i].position;
            }

            CacheCharacterRenderers();
            ApplyInstant();
        }

        void OnDestroy()
        {
            if (_skyboxShared != null)
                RenderSettings.skybox = _skyboxShared;
            if (_skyboxRuntime != null)
                Destroy(_skyboxRuntime);
        }

        void Update()
        {
            if (!_lightBusy && (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)))
                StartCoroutine(OrbitLight());

            RotateHeld();

            if (_busy || characters == null || characters.Length < 2)
                return;

            if (Input.GetKeyDown(KeyCode.RightArrow))
                StartCoroutine(Slide(1));
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                StartCoroutine(Slide(-1));
        }

        void RotateHeld()
        {
            float charSign = 0f;
            if (Input.GetKey(KeyCode.A))
                charSign -= 1f;
            if (Input.GetKey(KeyCode.D))
                charSign += 1f;
            if (charSign != 0f && !_busy && characters != null && _index >= 0 && _index < characters.Length)
            {
                Transform t = characters[_index];
                if (t != null)
                    t.Rotate(Vector3.up, charSign * characterRotateSpeed * Time.deltaTime, Space.World);
            }

            if (_lightBusy || directionalLight == null)
                return;

            float lightSign = 0f;
            if (Input.GetKey(KeyCode.W))
                lightSign -= 1f;
            if (Input.GetKey(KeyCode.S))
                lightSign += 1f;
            if (lightSign != 0f)
                directionalLight.Rotate(Vector3.up, lightSign * lightRotateSpeed * Time.deltaTime, Space.World);
        }

        IEnumerator OrbitLight()
        {
            if (directionalLight == null)
                yield break;

            _lightBusy = true;
            Quaternion start = directionalLight.rotation;
            float dur = Mathf.Max(lightOrbitDuration, 0.01f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / dur;
                float u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
                directionalLight.rotation = Quaternion.AngleAxis(360f * u, Vector3.up) * start;
                yield return null;
            }

            directionalLight.rotation = start;
            _lightBusy = false;
        }

        void ApplyInstant()
        {
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i] == null)
                    continue;
                characters[i].gameObject.SetActive(i == _index);
                characters[i].position = _home[i];
                SetCharacterFade(i, i == _index ? 1f : 0f);
                SetCharacterGlitch(i, 0f, 0f);
            }

            ApplyTheme(_index, true);
        }

        bool TryGetTheme(int index, out CharacterTheme theme)
        {
            theme = null;
            if (themes == null || index < 0 || index >= themes.Length)
                return false;
            theme = themes[index];
            return theme != null;
        }

        void ApplyTheme(int index, bool updateEnvironment)
        {
            if (!TryGetTheme(index, out CharacterTheme theme))
                return;

            ApplyVisualTheme(theme, theme, 1f);
            ApplyCloudTheme(theme);

            if (updateEnvironment)
                DynamicGI.UpdateEnvironment();
        }

        void ApplyVisualTheme(CharacterTheme from, CharacterTheme to, float u)
        {
            if (from == null || to == null)
                return;

            u = Mathf.Clamp01(u);

            if (starRenderer != null)
            {
                if (_starBlock == null)
                    _starBlock = new MaterialPropertyBlock();
                starRenderer.GetPropertyBlock(_starBlock);
                _starBlock.SetColor(TintAId, Color.Lerp(from.tintA, to.tintA, u));
                _starBlock.SetColor(TintBId, Color.Lerp(from.tintB, to.tintB, u));
                starRenderer.SetPropertyBlock(_starBlock);
            }

            Cubemap cubeA = from.skyCube != null ? from.skyCube : to.skyCube;
            Cubemap cubeB = to.skyCube != null ? to.skyCube : from.skyCube;
            if (cubeA != null || cubeB != null)
            {
                EnsureSkyboxInstance();
                if (_skyboxRuntime != null)
                {
                    if (cubeA != null)
                        _skyboxRuntime.SetTexture(SkyTexId, cubeA);
                    if (cubeB != null)
                        _skyboxRuntime.SetTexture(SkyTexBId, cubeB);
                    _skyboxRuntime.SetFloat(SkyBlendId, from.skyCube == to.skyCube ? 0f : u);
                }
            }

            if (floorRenderer != null)
            {
                if (_floorBlock == null)
                    _floorBlock = new MaterialPropertyBlock();
                floorRenderer.GetPropertyBlock(_floorBlock);
                _floorBlock.SetColor(FloorColorId, Color.Lerp(from.floorColor, to.floorColor, u));
                if (cubeA != null)
                    _floorBlock.SetTexture(FloorCubeId, cubeA);
                if (cubeB != null)
                    _floorBlock.SetTexture(FloorCubeBId, cubeB);
                _floorBlock.SetFloat(SkyBlendId, from.skyCube == to.skyCube ? 0f : u);
                floorRenderer.SetPropertyBlock(_floorBlock);
            }

            if (cloudParticles != null)
            {
                Color cloudA = Color.Lerp(from.cloudColorA, to.cloudColorA, u);
                Color cloudB = Color.Lerp(from.cloudColorB, to.cloudColorB, u);
                ApplyCloudColors(cloudA, cloudB, Color.Lerp(from.cloudBaseColor, to.cloudBaseColor, u));
            }
        }

        void ApplyCloudTheme(CharacterTheme theme)
        {
            if (theme == null)
                return;
            ApplyCloudColors(theme.cloudColorA, theme.cloudColorB, theme.cloudBaseColor);
        }

        void ApplyCloudColors(Color startA, Color startB, Color baseColor)
        {
            if (cloudParticles == null)
                return;

            ParticleSystem.MainModule main = cloudParticles.main;
            main.startColor = new ParticleSystem.MinMaxGradient(startA, startB);

            if (_cloudRenderer == null)
                _cloudRenderer = cloudParticles.GetComponent<ParticleSystemRenderer>();

            if (_cloudRenderer != null)
            {
                if (_cloudBlock == null)
                    _cloudBlock = new MaterialPropertyBlock();
                _cloudRenderer.GetPropertyBlock(_cloudBlock);
                _cloudBlock.SetColor(BaseColorId, baseColor);
                _cloudBlock.SetColor(ColorId, baseColor);
                _cloudRenderer.SetPropertyBlock(_cloudBlock);
            }

            int max = main.maxParticles;
            if (max <= 0)
                return;
            if (_cloudParticleBuffer == null || _cloudParticleBuffer.Length < max)
                _cloudParticleBuffer = new ParticleSystem.Particle[max];

            int count = cloudParticles.GetParticles(_cloudParticleBuffer);
            for (int i = 0; i < count; i++)
            {
                float k = (_cloudParticleBuffer[i].randomSeed & 0xFFFFu) / 65535f;
                _cloudParticleBuffer[i].startColor = Color.Lerp(startA, startB, k);
            }
            cloudParticles.SetParticles(_cloudParticleBuffer, count);
        }

        void CacheCharacterRenderers()
        {
            _charRenderers = new Renderer[characters.Length][];
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i] == null)
                    continue;
                _charRenderers[i] = characters[i].GetComponentsInChildren<Renderer>(true);
            }
        }

        void SetCharacterFade(int index, float fade)
        {
            if (_charRenderers == null || index < 0 || index >= _charRenderers.Length)
                return;

            Renderer[] renderers = _charRenderers[index];
            if (renderers == null)
                return;

            if (_fadeBlock == null)
                _fadeBlock = new MaterialPropertyBlock();

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (r == null)
                    continue;
                r.GetPropertyBlock(_fadeBlock);
                _fadeBlock.SetFloat(FadeId, fade);
                r.SetPropertyBlock(_fadeBlock);
            }
        }

        void SetCharacterGlitch(int index, float glitch, float height)
        {
            if (_charRenderers == null || index < 0 || index >= _charRenderers.Length)
                return;

            Renderer[] renderers = _charRenderers[index];
            if (renderers == null)
                return;

            if (_fadeBlock == null)
                _fadeBlock = new MaterialPropertyBlock();

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (r == null)
                    continue;
                r.GetPropertyBlock(_fadeBlock);
                _fadeBlock.SetFloat(GlitchId, glitch);
                _fadeBlock.SetFloat(GlitchHeightId, height);
                r.SetPropertyBlock(_fadeBlock);
            }
        }

        bool TryGetCharacterYRange(int index, out float yMin, out float yMax)
        {
            yMin = 0f;
            yMax = 2f;
            if (_charRenderers == null || index < 0 || index >= _charRenderers.Length)
                return false;

            Renderer[] renderers = _charRenderers[index];
            if (renderers == null)
                return false;

            bool any = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (r == null || !r.gameObject.activeInHierarchy)
                    continue;
                Bounds b = r.bounds;
                if (!any)
                {
                    yMin = b.min.y;
                    yMax = b.max.y;
                    any = true;
                }
                else
                {
                    yMin = Mathf.Min(yMin, b.min.y);
                    yMax = Mathf.Max(yMax, b.max.y);
                }
            }
            return any;
        }

        void StopGlitch()
        {
            if (_glitchCo != null)
            {
                StopCoroutine(_glitchCo);
                _glitchCo = null;
            }
        }

        IEnumerator PlayGlitch(int index)
        {
            if (!TryGetCharacterYRange(index, out float yMin, out float yMax))
            {
                _glitchCo = null;
                yield break;
            }

            const float pad = 0.2f;
            float y0 = yMax + pad;
            float y1 = yMin - pad;
            float fadeFrom = Mathf.Clamp01(glitchArriveFade);

            SetCharacterFade(index, fadeFrom);
            SetCharacterGlitch(index, 0f, y0);

            float hold = Mathf.Max(glitchHold, 0f);
            if (hold > 0f)
                yield return new WaitForSeconds(hold);

            float dur = Mathf.Max(glitchDuration, 0.01f);
            float t = 0f;

            while (t < dur)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / dur);
                float sweep = Mathf.Pow(u, glitchEase);
                float height = Mathf.Lerp(y0, y1, sweep);
                float glitchIn = Mathf.SmoothStep(0f, 1f, u / 0.12f);
                float glitchOut = 1f - Mathf.SmoothStep(0f, 1f, (u - 0.82f) / 0.18f);
                SetCharacterFade(index, Mathf.Lerp(fadeFrom, 1f, Mathf.SmoothStep(0f, 1f, sweep)));
                SetCharacterGlitch(index, glitchIn * glitchOut, height);
                yield return null;
            }

            SetCharacterFade(index, 1f);
            SetCharacterGlitch(index, 0f, y1);
            _glitchCo = null;
        }

        void EnsureSkyboxInstance()
        {
            if (_skyboxRuntime != null)
                return;

            _skyboxShared = RenderSettings.skybox;
            if (_skyboxShared == null)
                return;

            _skyboxRuntime = new Material(_skyboxShared);
            Shader blendShader = Shader.Find("RenderingLab/Genshin/SkyboxBlend");
            if (blendShader != null)
            {
                Texture current = _skyboxRuntime.GetTexture(SkyTexId);
                _skyboxRuntime.shader = blendShader;
                if (current != null)
                {
                    _skyboxRuntime.SetTexture(SkyTexId, current);
                    _skyboxRuntime.SetTexture(SkyTexBId, current);
                }
                _skyboxRuntime.SetFloat(SkyBlendId, 0f);
            }
            RenderSettings.skybox = _skyboxRuntime;
        }

        Vector3 SlideAxis()
        {
            Vector3 r = worldCamera != null ? worldCamera.transform.right : Vector3.right;
            r.y = 0f;
            return r.sqrMagnitude > 1e-6f ? r.normalized : Vector3.right;
        }

        IEnumerator Slide(int sign)
        {
            int next = (_index + sign + characters.Length) % characters.Length;
            Transform a = characters[_index];
            Transform b = characters[next];
            if (a == null || b == null)
                yield break;

            _busy = true;
            StopGlitch();
            SetCharacterGlitch(_index, 0f, 0f);
            SetCharacterGlitch(next, 0f, 0f);

            Vector3 axis = SlideAxis();
            Vector3 aHome = _home[_index];
            Vector3 bHome = _home[next];
            Vector3 aEnd = aHome - axis * sign * slideDistance;
            Vector3 bStart = bHome + axis * sign * slideDistance;

            b.position = bStart;
            SetCharacterFade(next, 0f);
            b.gameObject.SetActive(true);

            TryGetTheme(_index, out CharacterTheme fromTheme);
            TryGetTheme(next, out CharacterTheme toTheme);
            if (fromTheme != null && toTheme != null)
                ApplyVisualTheme(fromTheme, toTheme, 0f);

            float slideDur = Mathf.Max(duration, 0.01f);
            float delay = Mathf.Max(themeDelay, 0f);
            float themeDur = Mathf.Max(themeDuration, 0.01f);
            float total = Mathf.Max(slideDur, delay + themeDur);
            float elapsed = 0f;
            bool characterSettled = false;

            while (elapsed < total)
            {
                elapsed += Time.deltaTime;

                float slideT = Mathf.Clamp01(elapsed / slideDur);
                float u = Mathf.SmoothStep(0f, 1f, slideT);

                if (!characterSettled)
                {
                    a.position = Vector3.Lerp(aHome, aEnd, u);
                    b.position = Vector3.Lerp(bStart, bHome, u);
                    SetCharacterFade(_index, 1f - u);
                    SetCharacterFade(next, Mathf.Lerp(0f, glitchArriveFade, u));

                    if (slideT >= 1f)
                    {
                        a.gameObject.SetActive(false);
                        a.position = aHome;
                        b.position = bHome;
                        SetCharacterFade(_index, 0f);
                        SetCharacterFade(next, glitchArriveFade);
                        SetCharacterGlitch(_index, 0f, 0f);
                        _index = next;
                        characterSettled = true;
                        StopGlitch();
                        _glitchCo = StartCoroutine(PlayGlitch(_index));
                    }
                }

                if (fromTheme != null && toTheme != null)
                {
                    float themeT = Mathf.Clamp01((elapsed - delay) / themeDur);
                    float themeU = Mathf.SmoothStep(0f, 1f, themeT);
                    ApplyVisualTheme(fromTheme, toTheme, themeU);
                }

                yield return null;
            }

            ApplyTheme(_index, true);
            _busy = false;
        }
    }
}
