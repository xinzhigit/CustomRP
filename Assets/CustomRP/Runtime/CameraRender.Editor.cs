using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

namespace CustomRP {
    public partial class CameraRender {
#if UNITY_EDITOR
        private static ShaderTagId[] _legencyShaderTagId = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM"),
    };
        private static Material _errorMaterial;

        private string SampleName { get; set; }

        partial void DrawUnsupportedShaders() {
            if (_errorMaterial == null) {
                _errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
            }

            var sortingSettings = new SortingSettings(_camera);
            var drawSettings = new DrawingSettings(_legencyShaderTagId[0], sortingSettings);
            drawSettings.overrideMaterial = _errorMaterial;
            for (int n = 0; n < _legencyShaderTagId.Length; ++n) {
                drawSettings.SetShaderPassName(n, _legencyShaderTagId[n]);
            }

            var filteringSettings = FilteringSettings.defaultValue;

            _context.DrawRenderers(_cullingResults, ref drawSettings, ref filteringSettings);
        }

        partial void DrawGizmos() {
            if (Handles.ShouldRenderGizmos()) {
                _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
                _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
            }
        }

        partial void PrepareCommandBuffer() {
            Profiler.BeginSample("Editor Only");
            _commandBuffer.name = SampleName = _camera.name;
            Profiler.EndSample();
        }

        partial void PrepareForSceneWindow() {
            if (_camera.cameraType == CameraType.SceneView) {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
            }
        }
#else
    const string SampleName = commandBufferName;
#endif
    }

}
