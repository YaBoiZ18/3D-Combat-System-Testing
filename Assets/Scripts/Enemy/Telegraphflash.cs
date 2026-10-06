using System.Collections.Generic;
using UnityEngine;

// NEW: makes an enemy glow toward a warning color right before it attacks.
// You don't have to add this yourself: EnemyController adds it automatically.
// Add it manually to the enemy if you want to change the color or strength in the Inspector.
public class TelegraphFlash : MonoBehaviour
{
    [SerializeField] private Color flashColor = new Color(1f, 0.2f, 0.15f);
    [Range(0f, 1f)]
    [SerializeField] private float strength = 0.75f; // how far toward flashColor it goes at full charge

    // Most shaders store their main color in one of these two properties
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor"); // URP / HDRP
    private static readonly int ColorId = Shader.PropertyToID("_Color");         // Built-in pipeline

    // One entry per material on every renderer, remembering its original color
    private struct Slot
    {
        public Renderer renderer;
        public int materialIndex;
        public int propertyId;
        public Color original;
    }

    private Slot[] slots;
    private MaterialPropertyBlock block;
    private bool isOn;

    private void Awake()
    {
        block = new MaterialPropertyBlock();

        List<Slot> found = new List<Slot>();

        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
        {
            if (r is ParticleSystemRenderer)
                continue;

            Material[] materials = r.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                Material m = materials[i];
                if (m == null)
                    continue;

                int id;
                if (m.HasProperty(BaseColorId)) id = BaseColorId;
                else if (m.HasProperty(ColorId)) id = ColorId;
                else continue; // this shader has no tintable color

                found.Add(new Slot
                {
                    renderer = r,
                    materialIndex = i,
                    propertyId = id,
                    original = m.GetColor(id)
                });
            }
        }

        slots = found.ToArray();
    }

    // amount: 0 = normal color, 1 = fully charged
    public void SetIntensity(float amount)
    {
        float t = Mathf.Clamp01(amount) * strength;

        for (int i = 0; i < slots.Length; i++)
        {
            Slot s = slots[i];
            if (s.renderer == null)
                continue;

            block.Clear();
            block.SetColor(s.propertyId, Color.Lerp(s.original, flashColor, t));
            s.renderer.SetPropertyBlock(block, s.materialIndex);
        }

        isOn = true;
    }

    // Back to the normal colors
    public void Clear()
    {
        if (!isOn)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].renderer != null)
            {
                slots[i].renderer.SetPropertyBlock(null, slots[i].materialIndex);
            }
        }

        isOn = false;
    }

    private void OnDisable()
    {
        Clear();
    }
}