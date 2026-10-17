using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ObliqueCamera2D : MonoBehaviour
{
    [Header("Pengaturan Sudut Perspektif 2D")]
    [Range(-85f, 85f)] 
    public float angle = -45.0f; // Sudut rebah kemiringan peta
    
    [Range(0.1f, 2f)]
    public float zScale = 0.5f;  // Seberapa ekstrem efek distorsi perspektifnya

    private Camera cam;

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        CalculateOblique();
    }

    void Update()
    {
        // Dipanggil setiap frame agar Anda bisa langsung melihat perubahannya saat menggeser slider
        CalculateOblique();
    }

    void OnDisable()
    {
        if (cam != null)
        {
            cam.ResetProjectionMatrix();
        }
    }

    void CalculateOblique()
    {
        if (cam == null) return;

        // Paksa kamera ke Orthographic agar piksel tetap tajam
        cam.orthographic = true;

        float orthoHeight = cam.orthographicSize;
        float orthoWidth = cam.aspect * orthoHeight;

        // 1. Buat matriks Orthographic dasar secara manual
        Matrix4x4 m = Matrix4x4.Ortho(
            -orthoWidth, orthoWidth, 
            -orthoHeight, orthoHeight, 
            cam.nearClipPlane, 
            cam.farClipPlane
        );

        // 2. Lakukan injeksi kemiringan sudut (Oblique Projection) pada koordinat Z matriks
        float s = zScale / orthoHeight;
        m[0, 2] = +s * Mathf.Sin(Mathf.Deg2Rad * -angle);
        m[1, 2] = -s * Mathf.Cos(Mathf.Deg2Rad * -angle);

        // 3. Terapkan matriks modifikasi penuh ke kamera
        cam.projectionMatrix = m;
    }
}
