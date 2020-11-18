using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Configuration;
using UnityEngine.Rendering;

namespace CustomRP {
    public class LitShaderGUI : ShaderGUI {
        private MaterialEditor _editor;
        private Object[] _materials;
        private MaterialProperty[] _properties;

        #region GUI
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
            base.OnGUI(materialEditor, properties);

            _editor = materialEditor;
            _materials = materialEditor.targets;
            _properties = properties;

            OpaquePreset();
            ClipPreset();
            FadePreset();
            TransparentPreset();
        }

        private bool PresetButton(string name) {
            if(GUILayout.Button(name)) {
                _editor.RegisterPropertyChangeUndo(name);
                return true;
            }
            return false;
        }

        private void OpaquePreset() {
            if(PresetButton("Opaque")) {
                Cliping = false;
                PremultiplyAlpha = false;
                SrcBlend = BlendMode.One;
                DstBlend = BlendMode.Zero;
                ZWrite = true;
                RenderQueue = RenderQueue.Geometry;
            }
        }
        private void ClipPreset() {
            if (PresetButton("Clip")) {
                Cliping = true;
                PremultiplyAlpha = false;
                SrcBlend = BlendMode.One;
                DstBlend = BlendMode.Zero;
                ZWrite = true;
                RenderQueue = RenderQueue.AlphaTest;
            }
        }

        private void FadePreset() {
            if (PresetButton("Fade")) {
                Cliping = false;
                PremultiplyAlpha = false;
                SrcBlend = BlendMode.SrcAlpha;
                DstBlend = BlendMode.OneMinusSrcAlpha;
                ZWrite = false;
                RenderQueue = RenderQueue.Transparent;
            }
        }

        private void TransparentPreset() {
            if (PresetButton("Transparent")) {
                Cliping = false;
                PremultiplyAlpha = true;
                SrcBlend = BlendMode.One;
                DstBlend = BlendMode.OneMinusSrcAlpha;
                ZWrite = false;
                RenderQueue = RenderQueue.Transparent;
            }
        }
        #endregion

        #region properties
        private bool Cliping {
            set => SetProperty("_Cliping", "_CLIPING", value);
        }
        private bool PremultiplyAlpha {
            set => SetProperty("_PremultiplyAlpha", "_PREMULTIPLY_ALPHA", value);
        }
        private BlendMode SrcBlend {
            set => SetProperty("_SrcBlend", (float)value);
        }
        private BlendMode DstBlend {
            set => SetProperty("_DstBlend", (float)value);
        }
        private bool ZWrite {
            set => SetProperty("_ZWrite", value ? 1f : 0f);
        }
        private RenderQueue RenderQueue {
            set {
                foreach(Material m in _materials) {
                    m.renderQueue = (int)value;
                }
            }
        }

        private void SetProperty(string name, float value) {
            FindProperty(name, _properties).floatValue = value;
        }

        private void SetProperty(string name, string keyword, bool value) {
            SetProperty(name, value ? 1f : 0f);
            SetKeyword(keyword, value);
        }

        private void SetKeyword(string keyword, bool enabled) {
            if(enabled) {
                foreach(Material m in _materials) {
                    m.EnableKeyword(keyword);
                }
            }
            else {
                foreach(Material m in _materials) {
                    m.DisableKeyword(keyword);
                }
            }
        }
        #endregion
    }
}

