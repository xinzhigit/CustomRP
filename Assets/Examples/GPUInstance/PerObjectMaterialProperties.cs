using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour {
    private static int _baseColorId = Shader.PropertyToID("_BaseColor");
    private static int _cutoffId = Shader.PropertyToID("_Cutoff");
    private static int _metallicId = Shader.PropertyToID("_Metallic");
    private static int _smoothnessId = Shader.PropertyToID("_Smoothness");
    private static MaterialPropertyBlock _block;

    [SerializeField]
    private Color _baseColor = Color.white;

    [SerializeField, Range(0f, 1f)]
    private float _cutoff = 0.5f;

    [SerializeField, Range(0f, 1f)]
    private float _metalic = 0.5f;

    [SerializeField, Range(0f, 1f)]
    private float _smoothness = 0.5f;

    private void Awake() {
        OnValidate();
    }

    private void OnValidate() {
        if(_block == null) {
            _block = new MaterialPropertyBlock();
        }

        _block.SetColor(_baseColorId, _baseColor);
        _block.SetFloat(_cutoffId, _cutoff);
        _block.SetFloat(_metallicId, _metalic);
        _block.SetFloat(_smoothnessId, _smoothness);

        GetComponent<Renderer>().SetPropertyBlock(_block);
    }
}
