using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP {
    public class CustomRP : RenderPipeline {
        private CameraRender _renderer = new CameraRender();

        private bool _useDynamicBatching;
        private bool _useGPUInstancing;
        private bool _useSRPBatcher;
        private ShadowSettings _shadowSettings;

        public CustomRP(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher, ShadowSettings shadowSettings) {
            _useDynamicBatching = useDynamicBatching;
            _useGPUInstancing = useGPUInstancing;
            _useSRPBatcher = useSRPBatcher;
            _shadowSettings = shadowSettings;

            GraphicsSettings.useScriptableRenderPipelineBatching = _useSRPBatcher;
            GraphicsSettings.lightsUseLinearIntensity = true;
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
            for (int n = 0; n < cameras.Length; ++n) {
                _renderer.Render(context, cameras[n], _useDynamicBatching, _useGPUInstancing, _shadowSettings);
            }
        }
    }

}
