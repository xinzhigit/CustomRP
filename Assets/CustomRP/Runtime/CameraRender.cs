using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP {
    public partial class CameraRender {
        private ScriptableRenderContext _context;
        private Camera _camera;

        private const string commandBufferName = "Render Camera";
        private CommandBuffer _commandBuffer = new CommandBuffer() { name = commandBufferName };

        private CullingResults _cullingResults;

        private static ShaderTagId _unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        private static ShaderTagId _litShaderTagId = new ShaderTagId("CustomLit");

        private Lighting lighting = new Lighting();

        partial void DrawUnsupportedShaders();
        partial void DrawGizmos();
        partial void PrepareCommandBuffer();
        partial void PrepareForSceneWindow();

        public void Render(ScriptableRenderContext context, 
                            Camera camera, 
                            bool useDynamicBatching, 
                            bool useGPUInstancing,
                            ShadowSettings shadowSettings) {
            _context = context;
            _camera = camera;

            PrepareCommandBuffer();
            PrepareForSceneWindow();
            if (!Cull(shadowSettings.maxDistance)) {
                return;
            }

            lighting.Setup(context, _cullingResults, shadowSettings);
            Setup();

            DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
            DrawUnsupportedShaders();
            DrawGizmos();

            lighting.Cleanup();
            Submit();
        }

        private void Setup() {
            _context.SetupCameraProperties(_camera);
            CameraClearFlags flags = _camera.clearFlags;
            bool clearDepth = flags <= CameraClearFlags.Depth;
            bool clearColor = flags == CameraClearFlags.Color;
            Color backColor = flags == CameraClearFlags.Color ? _camera.backgroundColor.linear : Color.clear;
            _commandBuffer.ClearRenderTarget(clearDepth, clearColor, backColor);
            _commandBuffer.BeginSample(SampleName);
            ExecuteCommandBuffer();
        }

        private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing) {
            var sortingSettings = new SortingSettings(_camera) {
                criteria = SortingCriteria.CommonOpaque
            };
            var drawingSettings = new DrawingSettings(_unlitShaderTagId, sortingSettings) {
                enableDynamicBatching = useDynamicBatching,
                enableInstancing = useGPUInstancing
            };
            drawingSettings.SetShaderPassName(1, _litShaderTagId);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);

            _context.DrawSkybox(_camera);

            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;
            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        }

        private void Submit() {
            _commandBuffer.EndSample(SampleName);
            ExecuteCommandBuffer();
            _context.Submit();
        }

        private void ExecuteCommandBuffer() {
            _context.ExecuteCommandBuffer(_commandBuffer);
            _commandBuffer.Clear();
        }

        private bool Cull(float maxShadowDistance) {
            if (_camera.TryGetCullingParameters(out ScriptableCullingParameters p)) {
                p.shadowDistance = Mathf.Min(maxShadowDistance, _camera.farClipPlane);
                _cullingResults = _context.Cull(ref p);
                return true;
            }

            return false;
        }
    }

}
