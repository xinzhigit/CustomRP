using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

namespace CustomRP {
    public class Lighting {
        private const string _bufferName = "Lighting";
        private CommandBuffer _buffer = new CommandBuffer {
            name = _bufferName
        };

        /// <summary>
        /// 最大平行光数量
        /// </summary>
        private const int _maxDirLightCount = 4;

        private static int _dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
        private static int _dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
        private static int _dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");

        private static Vector4[] dirLightColors = new Vector4[_maxDirLightCount];
        private static Vector4[] dirLightDirections = new Vector4[_maxDirLightCount];

        private CullingResults _cullingResults;

        private Shadows _shadows = new Shadows();

        public void Setup(ScriptableRenderContext context, 
                            CullingResults cullingResults,
                            ShadowSettings shadowSettings) {
            _cullingResults = cullingResults;

            _buffer.BeginSample(_bufferName);
            _shadows.Setup(context, cullingResults, shadowSettings);
            SetupLights();
            _shadows.Render();
            _buffer.EndSample(_bufferName);
            context.ExecuteCommandBuffer(_buffer);
            _buffer.Clear();
        }

        public void Cleanup() {
            _shadows.Cleanup();
        }

        private void SetupDirectionalLight(int index, ref VisibleLight visibleLight) {
            dirLightColors[index] = visibleLight.finalColor;
            dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);

            _shadows.ReserveDirectionalShadows(visibleLight.light, index);
        }

        private void SetupLights() {
            NativeArray<VisibleLight> visibleLights = _cullingResults.visibleLights;
            int dirLightCount = 0;
            for(int n = 0; n < visibleLights.Length; ++n) {
                VisibleLight visibleLight = visibleLights[n];
                if(visibleLight.lightType == LightType.Directional) {
                    SetupDirectionalLight(dirLightCount++, ref visibleLight);
                    if (dirLightCount >= _maxDirLightCount) {
                        break;
                    }
                }
            }

            _buffer.SetGlobalInt(_dirLightCountId, visibleLights.Length);
            _buffer.SetGlobalVectorArray(_dirLightColorsId, dirLightColors);
            _buffer.SetGlobalVectorArray(_dirLightDirectionsId, dirLightDirections);
        }
    }
}

