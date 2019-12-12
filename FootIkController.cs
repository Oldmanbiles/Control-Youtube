using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootIkController : MonoBehaviour
{
    public bool IKEnabled;

    public float IKSpeed;
    public float HeightFromGroundToCast;
    Animator m_Animator;

    Vector3 m_LeftFootPosition;
    Vector3 m_LeftFootIKPosition;
    Quaternion m_LeftFootIKRotation;
    float m_LastLeftFootPositionY;

    Vector3 m_RightFootPosition;
    Vector3 m_RightFootIKPosition;
    Quaternion m_RightFootIKRotation;
    float m_LastRightFootPositionY;

    public LayerMask FootIKLayer;

    private void Awake() {
        m_Animator = GetComponent<Animator>();
    }


    private void FixedUpdate()
    {

        SetFootTarget(ref m_RightFootPosition, HumanBodyBones.RightFoot);
        SetFootTarget(ref m_LeftFootPosition, HumanBodyBones.LeftFoot);
        GetFootPositionandRotation(m_LeftFootPosition, ref m_LeftFootIKPosition, ref m_LeftFootIKRotation);
        GetFootPositionandRotation(m_RightFootPosition, ref m_RightFootIKPosition, ref m_RightFootIKRotation);


    }

    void GetFootPositionandRotation(Vector3 skypos, ref Vector3 feetIKPosition, ref Quaternion feetIKRotation)
    {
        RaycastHit feethitout;
        if(Physics.Raycast(skypos,Vector3.down, out feethitout, 2+HeightFromGroundToCast,FootIKLayer))
        {
            feetIKPosition = skypos;
            feetIKPosition.y = feethitout.point.y;
            feetIKRotation = Quaternion.FromToRotation(Vector3.up,feethitout.normal) * transform.rotation;

            return;
        }

        feetIKPosition = Vector3.zero;
    }

    private void SetFootTarget(ref Vector3 footPosition, HumanBodyBones foot)
    {
        footPosition = m_Animator.GetBoneTransform(foot).position;
        footPosition.y = transform.position.y + HeightFromGroundToCast;
    }


    private void OnAnimatorIK()
    {
        if(!IKEnabled)
            return;
        
        m_Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, m_Animator.GetFloat("RightFootCurve"));
        MoveFootToIK(AvatarIKGoal.RightFoot, m_RightFootIKPosition, m_RightFootIKRotation, ref m_LastRightFootPositionY);

        m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, m_Animator.GetFloat("LeftFootCurve"));
        MoveFootToIK(AvatarIKGoal.LeftFoot, m_LeftFootIKPosition, m_LeftFootIKRotation, ref m_LastLeftFootPositionY);

    }

    private void MoveFootToIK(AvatarIKGoal foot, Vector3 IKPosition, Quaternion IKRotation, ref float lastFootY)
    {
        Vector3 targetIKPosition = m_Animator.GetIKPosition(foot);

        if(IKPosition != Vector3.zero)
        {
            targetIKPosition = transform.InverseTransformPoint(targetIKPosition);
            IKPosition = transform.InverseTransformPoint(IKPosition);

            float y = Mathf.Lerp(lastFootY, IKPosition.y, IKSpeed);
            targetIKPosition.y += y;
            lastFootY = y;
            targetIKPosition = transform.TransformPoint(targetIKPosition);
            m_Animator.SetIKRotation(foot,IKRotation);

        }

        m_Animator.SetIKPosition(foot,targetIKPosition);
    }

}
