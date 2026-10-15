using UnityEngine;

public class EnemyAnimationTest : MonoBehaviour
{
    public Animator anim;
    public SpriteRenderer spriteRenderer;
    public float moveSpeed = 5f;

    // Variabel internal untuk menyimpan arah terakhir agar Attack tidak salah arah
    private float lastH = 0;
    private float lastV = -1; // Default menghadap bawah

    void Update()
    {
        // 1. Ambil Input WASD
        float h = Input.GetAxisRaw("Horizontal"); 
        float v = Input.GetAxisRaw("Vertical");   
        Vector2 input = new Vector2(h, v).normalized;

        // 2. Update Parameter & Mirroring
        if (input.sqrMagnitude > 0.1f)
        {
            lastH = h;
            lastV = v;

            anim.SetFloat("Horizontal", h);
            anim.SetFloat("Vertical", v);

            // Logika Mirroring: Jika ke kiri (h < 0), flip aktif.
            // Gunakan else if agar tidak terjadi flip balik saat h = 0 (hanya atas/bawah)
            if (h < 0) spriteRenderer.flipX = true;
            else if (h > 0) spriteRenderer.flipX = false;
        }
        
        anim.SetFloat("Speed", input.sqrMagnitude);

        // 3. Testing Serangan (Tekan Spasi)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Sebelum trigger, pastikan arah terakhir sudah benar di Animator
            anim.SetFloat("Horizontal", lastH);
            anim.SetFloat("Vertical", lastV);
            anim.SetTrigger("doAttack");
        }

        // 4. Gerakan
        transform.Translate(input * moveSpeed * Time.deltaTime);
    }
}