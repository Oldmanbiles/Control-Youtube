using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour 
{
    public float MoveSpeed;
    Rigidbody m_RigidBody;
    Animator m_Animator;

    [SerializeField]float m_AnimSpeedMultiplier;
    float m_HInput;
    float m_VInput;
    public bool IsWalking {get;set;}
    [SerializeField]Vector3 m_Move;
    float m_ForwardSpeed;
    Transform m_Camera;

    private void Awake() {
        m_RigidBody = GetComponent<Rigidbody>();
        m_Animator = GetComponent<Animator>();
        m_Camera = Camera.main.transform;
    }

    private void Update() {
        HandleInput();
        UpdateAnimator();
    }

    private void HandleInput()
    {
        m_HInput = CrossPlatformInputManager.GetAxis("Horizontal");
        m_VInput = CrossPlatformInputManager.GetAxis("Vertical");
        IsWalking = Input.GetKey(KeyCode.LeftShift);
    }

    private void FixedUpdate()
    {
        Move();
    }


    private void Move()
    {
        Vector3 camForward = Vector3.Scale(m_Camera.forward, new Vector3(1,0,1)).normalized;
        m_Move = m_VInput * camForward + m_HInput * m_Camera.right;

        
        m_Move.Normalize();

        if(IsWalking)
            m_Move *= 0.5f;

        m_Move = transform.InverseTransformDirection(m_Move);
        m_Move = Vector3.ProjectOnPlane(m_Move, GetGroundNormal());    
        m_ForwardSpeed = m_Move.z;
        
        float turnAmount = Mathf.Atan2(m_Move.x,m_Move.z);
        float turnSpeed = Mathf.Lerp(180,360,m_ForwardSpeed);
        transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime,0);

    }


    Vector3 GetGroundNormal()
    {
        RaycastHit hit;

        if(Physics.Raycast(transform.position +(Vector3.up * 0.1f),Vector3.down, out hit, 3))
        {
            return hit.normal;
        }else{
            return Vector3.up;
        }
    }

    private void UpdateAnimator()
    {
        m_Animator.SetFloat("Forward",m_ForwardSpeed,0.5f,Time.deltaTime);

        if(m_Move.magnitude > 0)
            m_Animator.speed = m_AnimSpeedMultiplier;
        
    }

    private void OnAnimatorMove() {
        if(Time.deltaTime > 0)
        {
            Vector3 v = (m_Animator.deltaPosition * MoveSpeed)/Time.deltaTime;
            v.y = m_RigidBody.velocity.y;
            m_RigidBody.velocity = v;
        }
    }
}