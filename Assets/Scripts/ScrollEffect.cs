using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ScrollEffect : MonoBehaviour
{
    public enum EffectAxis { X, Y, Z };

    public Cubemap EffectTexture = null;
    public Texture2D GradientTexture = null;
    public Color EffectColor = Color.magenta;
    public EffectAxis LocalAxis = EffectAxis.X;
    public bool InvertAxis = false;
    [Range(0.1f, 5.0f)]
    public float Speed = 1.0f;
    [Range(0.0f, 1.0f)]
    public float BlendAmount = 0.5f;


    private static class Uniforms
    {
        internal static readonly int _EffectTex = Shader.PropertyToID("_EffectTex");
        internal static readonly int _GradientTex = Shader.PropertyToID("_GradientTex");
        internal static readonly int _EffectColor = Shader.PropertyToID("_EffectColor");
        internal static readonly int _LocalAxis = Shader.PropertyToID("_LocalAxis");
        internal static readonly int _InvertAxis = Shader.PropertyToID("_InvertAxis");
        internal static readonly int _EffectOffset = Shader.PropertyToID("_EffectOffset");
        internal static readonly int _Scale = Shader.PropertyToID("_Scale");
        internal static readonly int _BlendAmount = Shader.PropertyToID("_BlendAmount");
    }

    private Renderer m_meshRenderer = null;
    private Material m_material = null;

    private readonly HashSet<Camera> m_cameras = new HashSet<Camera>();

    private float m_currentEffectOffest = 0.0f;
    private CommandBuffer m_commandBuffer = null;


    private void Awake()
    {
        m_meshRenderer = GetComponent<MeshRenderer>();

        m_material = new Material(Shader.Find("ScrollEffect"));
        m_material.SetTexture(Uniforms._EffectTex, EffectTexture);
        m_material.SetTexture(Uniforms._GradientTex, GradientTexture);
        m_material.SetColor(Uniforms._EffectColor, EffectColor);
        m_material.SetVector(Uniforms._LocalAxis, AxisToVector(LocalAxis));
        m_material.SetInt(Uniforms._InvertAxis, InvertAxis ? 1 : 0);
        m_material.SetFloat(Uniforms._BlendAmount, BlendAmount);

        var size = GetComponent<MeshFilter>().sharedMesh.bounds.size;
        m_material.SetVector(Uniforms._Scale, new Vector3(1.0f / size.x, 1.0f / size.y, 1.0f / size.z));

        m_commandBuffer = new CommandBuffer()
        {
            name = "ScrollEffect"
        };
        for (int i = 0; i < m_meshRenderer.materials.Length; ++i)
        {
            m_commandBuffer.DrawRenderer(m_meshRenderer, m_material, i);
        }
    }

    private void OnWillRenderObject()
    {
        var camera = Camera.current;
        if (!camera)
            return;
        if (m_cameras.Contains(camera))
            return;

        m_cameras.Add(camera);
        camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, m_commandBuffer);
    }

    private void OnEnable()
    {
        m_currentEffectOffest = 0.0f;
    }

    private void Update()
    {
        m_currentEffectOffest += Speed * Time.deltaTime;
        //m_currentEffectOffest = m_currentEffectOffest - Mathf.Floor(m_currentEffectOffest);
        if (m_currentEffectOffest >= 1.0f)
            m_currentEffectOffest -= 1.0f;

        m_material.SetFloat(Uniforms._EffectOffset, m_currentEffectOffest);
    }

    private static Vector3 AxisToVector(EffectAxis axis)
    {
        if (axis == EffectAxis.X) return Vector3.right;
        if (axis == EffectAxis.Y) return Vector3.up;
        return Vector3.forward;
    }
}
