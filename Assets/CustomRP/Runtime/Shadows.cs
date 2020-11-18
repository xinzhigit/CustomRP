using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP {
    public class Shadows {
        private static int _dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");

        private const string _bufferName = "Shadows";
        private CommandBuffer _buffer = new CommandBuffer {
            name = _bufferName
        };

        private ScriptableRenderContext _context;
        private CullingResults _cullingResults;
        private ShadowSettings _shadowSettings;

        /// <summary>
        /// 最大可投影平行光数量
        /// </summary>
        private const int _maxShadowedDirectionalLightCount = 1;
        private int _shadowDirectionalLightCount = 0;

        private struct ShadowDirectionalLight {
            public int visibleLightIndex;
        }

        private ShadowDirectionalLight[] _shadowDirectionalLights = new ShadowDirectionalLight[_maxShadowedDirectionalLightCount];

        public void Setup(ScriptableRenderContext context, 
                            CullingResults cullingResults, 
                            ShadowSettings shadowSettings) {
            _context = context;
            _cullingResults = cullingResults;
            _shadowSettings = shadowSettings;

            _shadowDirectionalLightCount = 0;
        }

        public void ReserveDirectionalShadows(Light light, int visibleLightIndex) {
            if(_shadowDirectionalLightCount < _maxShadowedDirectionalLightCount && 
                light.shadows != LightShadows.None &&
                light.shadowStrength > 0f &&
                _cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b)) {
                _shadowDirectionalLights[_shadowDirectionalLightCount++] = new ShadowDirectionalLight {
                    visibleLightIndex = visibleLightIndex
                };
            }
        }

        public void Render() {
            if(_shadowDirectionalLightCount > 0) {
                RenderDirectionalShadows();   
            }
            else {
                _buffer.GetTemporaryRT(_dirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            }
        }

        private void RenderDirectionalShadows() {
            int atlasSize = (int)_shadowSettings.directional.atlasSize;
            _buffer.GetTemporaryRT(_dirShadowAtlasId, atlasSize, atlasSize,
                                    32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            _buffer.SetRenderTarget(_dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            _buffer.ClearRenderTarget(true, false, Color.clear);
            _buffer.BeginSample(_bufferName);
            ExecuteBuffer();

            for(int n = 0; n < _shadowDirectionalLightCount; ++n) {
                RenderDirectionalShadows(n, atlasSize);
            }
            _buffer.EndSample(_bufferName);
            ExecuteBuffer();
        }

        private void RenderDirectionalShadows(int index, int tileSize) {
            ShadowDirectionalLight light = _shadowDirectionalLights[index];
            var shadowDrawSettings = new ShadowDrawingSettings(_cullingResults, light.visibleLightIndex);
            _cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.visibleLightIndex, 0, 1,
                Vector3.zero, tileSize, 0f, out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
                out ShadowSplitData splitData);
            shadowDrawSettings.splitData = splitData;
            _buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            ExecuteBuffer();
            _context.DrawShadows(ref shadowDrawSettings);
        }

        public void Cleanup() {
            if(_shadowDirectionalLightCount > 0) {
                _buffer.ReleaseTemporaryRT(_dirShadowAtlasId);
                ExecuteBuffer();
            }
        }

        private void ExecuteBuffer() {
            _context.ExecuteCommandBuffer(_buffer);
            _buffer.Clear();
        }
    }
}
