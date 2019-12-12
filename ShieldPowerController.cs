using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Cameras;
using UnityEngine.Rendering.PostProcessing;
public class ShieldPowerController : MonoBehaviour
{
    bool m_IsShielding;
    public int MaxObjectsInShield;
    public float StartCheckRadius;
    public float MaxCheckRadius;

    float m_CurrentCheckRadius;

    List<Rigidbody> m_BodiesInShield;
    List<Vector3> m_TargetPositions;

    public LayerMask GrabbableObjectsLayer;

    public Vector3 ShieldOffset;

    public float N;
    public float Alpha;

    PlayerController m_Player;
    Animator m_Animator;
    public FreeLookCam PlayerCamera;

    bool m_FireShield;
    public ParticleSystem GrabParticle;

    //post process
    public PostProcessVolume volume;
    ChromaticAberration m_Chromatic;
    float m_TargetChroma;

    private void Awake() 
    {
        m_Animator = GetComponent<Animator>();
        m_Player = GetComponent<PlayerController>();
        m_BodiesInShield = new List<Rigidbody>();
        m_TargetPositions = new List<Vector3>();

        volume.profile.TryGetSettings(out m_Chromatic);
    }

    private void Update() 
    {
        HandleInput();
        HandlePlayer();
        HandleAnimator();
        HandleCamera();
        HandleParticles();

        HandlePostProcess();

    }

    void HandlePostProcess()
    {
        if(m_Chromatic == null)
            return;

        if(m_IsShielding)
            m_TargetChroma = 1;
        else
            m_TargetChroma = 0;

        m_Chromatic.intensity.value = Mathf.Lerp(m_Chromatic.intensity.value,m_TargetChroma,Time.deltaTime*2);
    }

    void HandleInput()
    {

        if(!m_FireShield)
        {
            if(Input.GetMouseButtonDown(1))
            {
                m_CurrentCheckRadius = StartCheckRadius;
                m_IsShielding = !m_IsShielding;
            }

            if(Input.GetMouseButtonUp(1))
            {
                m_CurrentCheckRadius = 0;
                foreach (Rigidbody rb in m_BodiesInShield)
                    rb.drag = 1;

                m_BodiesInShield.Clear();
            }

        }


        if(Input.GetMouseButtonDown(0))
        {
            if(m_IsShielding)
                m_FireShield = true;
        }
    }

    void HandlePlayer()
    {
        m_Player.IsWalking = m_IsShielding;
    }

    void HandleParticles()
    {
        if(m_IsShielding)
        {
            GrabParticle.Emit(1);
            GrabParticle.gravityModifier = 0;
        }else{
            GrabParticle.gravityModifier = 1;
        }
    }

    void HandleCamera()
    {
        if(m_IsShielding)
        {
            PlayerCamera.SetZoom(2);
        }else{
            PlayerCamera.SetZoom(5);
        }
    }


    void HandleAnimator()
    {
        m_Animator.SetBool("IsFiring",m_FireShield);
        m_Animator.SetBool("IsShielding",m_IsShielding);
    }

    private void FixedUpdate()
    {
        if(m_IsShielding)
        {
            GetShieldObjects();
            HandleRadius();

            if(m_BodiesInShield.Count > 0)
            {
                GetPositionOfObjectInCircle();
                MoveObjectToPositions();
            }
        }
    }

    void MoveObjectToPositions()
    {
        for (int rb = 0; rb < m_BodiesInShield.Count; rb++)
        {
            Rigidbody target = m_BodiesInShield[rb];
            Vector3 targetPosition = transform.forward + transform.position + ShieldOffset + m_TargetPositions[rb];
            target.drag = 10;

            Vector3 diff = targetPosition - target.position;

            target.AddForce(diff * 100, ForceMode.Force);
        }
    }

    void GetPositionOfObjectInCircle()
    {
        m_TargetPositions.Clear();
        float b = Mathf.Round(Alpha * Mathf.Sqrt(N));
        float phi = (Mathf.Sqrt(5)+1)/2;

        for (int i = 0; i < m_BodiesInShield.Count; i++)
        {
            float r = radius(i,N,b);
            float theta = 2 * Mathf.PI * i / Mathf.Pow(phi,2);
            float x = r * Mathf.Cos(theta);
            float y = r * Mathf.Sin(theta);

            Vector3 pos = transform.TransformVector(x,y,0);
            m_TargetPositions.Add(pos);
        }
    }

    float radius(float k, float n, float b)
    {
        float r;
        if(k>n-b)
            r=1;
        else
            r = Mathf.Sqrt(k-1/2)/Mathf.Sqrt(n-(b+1)/2);
        return r;
    }

    void GetShieldObjects()
    {
        if(m_BodiesInShield.Count < MaxObjectsInShield)
        {
            Collider[] c = Physics.OverlapSphere(transform.position,m_CurrentCheckRadius, GrabbableObjectsLayer);

            foreach (Collider col in c)
            {
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if(rb != null)
                {
                    if(!m_BodiesInShield.Contains(rb))
                        m_BodiesInShield.Add(rb);
                }
            }
        }

        //clamp radius

        m_CurrentCheckRadius = Mathf.Clamp(m_CurrentCheckRadius,StartCheckRadius,MaxCheckRadius);
    }

    void HandleRadius()
    {
        m_CurrentCheckRadius += Time.deltaTime * 2;

        m_CurrentCheckRadius = Mathf.Clamp(m_CurrentCheckRadius, StartCheckRadius,MaxCheckRadius);
    }

    public void FireShield()
    {
        m_FireShield = false;
        m_IsShielding = false;

        //release the objects;

        foreach (Rigidbody rb in m_BodiesInShield)
        {
            rb.drag = 1;
            rb.AddForce(PlayerCamera.transform.forward * 1000, ForceMode.Force);
        }
        m_BodiesInShield.Clear();
    }
}
