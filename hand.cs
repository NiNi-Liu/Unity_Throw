using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class hand : MonoBehaviour
{
    public SteamVR_Action_Boolean Grab;
    public Collider Hand_mesh_collider;
    SteamVR_Behaviour_Pose pose;
    FixedJoint m_joint;
    Interactable m_correntInteractable;
    public List<Interactable> m_contactInteractable = new List<Interactable>();

    private void Awake()
    {
        pose = GetComponent<SteamVR_Behaviour_Pose>();
        m_joint = GetComponent<FixedJoint>();
    }

    // Update is called once per frame
    void Update()
    {
        //state down
        if (Grab.GetStateDown(pose.inputSource))
        {
            print(pose.inputSource + " Get State Down");
            pickUp();
        }
        //state up
        if (Grab.GetStateUp(pose.inputSource))
        {
            print(pose.inputSource + " Get State Up");
            Drop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Interactable"))
            return;

        if(other.transform.root == null)
            m_contactInteractable.Add(other.gameObject.GetComponent<Interactable>());
        else
        {
            if(other.transform.root.gameObject.GetComponent<Interactable>() == null)
            {
                m_contactInteractable.Add(other.gameObject.GetComponent<Interactable>());
            }
            else
            {
                m_contactInteractable.Add(other.transform.root.gameObject.GetComponent<Interactable>());
            }
        }

        print("on");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("Interactable"))
            return;
        if (other.transform.root == null)
            m_contactInteractable.Remove(other.gameObject.GetComponent<Interactable>());
        else
        {
            if (other.transform.root.gameObject.GetComponent<Interactable>() == null)
            {
                m_contactInteractable.Remove(other.gameObject.GetComponent<Interactable>());
            }
            else
            {
                m_contactInteractable.Remove(other.transform.root.gameObject.GetComponent<Interactable>());
            }
        }
        print("out");
    }
    //========================================================================================================
    //private void OnCollisionEnter(Collision other)
    //{
    //    if (!other.gameObject.CompareTag("Interactable"))
    //        return;
    //    m_contactInteractable.Add(other.transform.parent.gameObject.GetComponent<Interactable>());
    //    print("on");
    //}

    //private void OnCollisionExit(Collision other)
    //{
    //    if (!other.gameObject.CompareTag("Interactable"))
    //        return;
    //    m_contactInteractable.Remove(other.transform.parent.gameObject.GetComponent<Interactable>());
    //    print("out");
    //}
    //========================================================================================================
    void pickUp()
    {
        Hand_mesh_collider.enabled = false;
        //Get nearest
        m_correntInteractable = getnearInteractable();
        //Null check
        if (!m_correntInteractable)
            return;
        //Already held,check
        if (m_correntInteractable.m_activeHand)
            m_correntInteractable.m_activeHand.Drop();
        //position
        m_correntInteractable.transform.position = transform.position;
        m_correntInteractable.transform.forward = transform.forward;
        //Attach
        Rigidbody targetBody = m_correntInteractable.GetComponent<Rigidbody>();
        m_joint.connectedBody = targetBody;
        //Set active hand
        m_correntInteractable.m_activeHand = this;
    }


    void Drop()
    {
        StartCoroutine(waitCollider());
        //null check
        if (!m_correntInteractable)
            return;
        //apply velocity
        Rigidbody targetBody = m_correntInteractable.GetComponent<Rigidbody>();
        targetBody.velocity = pose.GetVelocity();
        targetBody.angularVelocity = pose.GetAngularVelocity();

        //detach
        m_joint.connectedBody = null;
        //clear
        m_correntInteractable.m_activeHand = null;
        m_correntInteractable = null;
    }

    IEnumerator waitCollider()
    {
        yield return new WaitForSeconds(0.1f);
        Hand_mesh_collider.enabled = true;
    }

    Interactable getnearInteractable()
    {
        Interactable nearest = null;
        float minDistance = float.MaxValue;
        float distance = 0.0f;

        foreach(Interactable interactable in m_contactInteractable)
        {
            distance = (interactable.transform.position - transform.position).sqrMagnitude;

            if(distance < minDistance)
            {
                minDistance = distance;
                nearest = interactable;
            }
        }
        return nearest;
    }
}
