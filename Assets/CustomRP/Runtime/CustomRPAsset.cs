using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP {
    [CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]
    public class CustomRPAsset : RenderPipelineAsset {
        [SerializeField]
        private bool _useDynamicBatching = true;
        [SerializeField]
        private bool _useGPUInstancing = true;
        [SerializeField]
        private bool _useSRPBatcher = true;

        [SerializeField]
        private ShadowSettings _shadows = default;

        protected override RenderPipeline CreatePipeline() {
            return new CustomRP(_useDynamicBatching, _useGPUInstancing, _useSRPBatcher, _shadows);
        }
    }
}
