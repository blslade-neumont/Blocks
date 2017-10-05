using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FlashInstructions : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI m_textMesh;

    void Start()
    {
        if (this.m_textMesh == null) this.m_textMesh = this.GetComponent<TextMeshProUGUI>();
    }

    private float m_timer = .4f;
    void Update()
    {
        this.m_timer -= Time.deltaTime;
        if (this.m_timer <= 0)
        {
            this.m_textMesh.enabled = !this.m_textMesh.enabled;
            this.m_timer += .4f;
        }
    }
}
