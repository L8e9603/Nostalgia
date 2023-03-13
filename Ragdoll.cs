using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    public GameObject rootBone;
    private Rigidbody[] rigidbodies;
    private CapsuleCollider[] capsuleColliders;
    private BoxCollider[] boxColliders;
    private Animator animator;

    // Start is called before the first frame update
    private void OnEnable()
    {
        rigidbodies = rootBone.GetComponentsInChildren<Rigidbody>();
        capsuleColliders = rootBone.GetComponentsInChildren<CapsuleCollider>();
        boxColliders = rootBone.GetComponentsInChildren<BoxCollider>();
        animator = GetComponent<Animator>();

        DeactivateRagdoll();
    }

    public void DeactivateRagdoll()
    {
        foreach (var rigidbody in rigidbodies)
        {
            //rigidbody.isKinematic = true;
        }

        foreach(var capsuleCollider in capsuleColliders)
        {
            capsuleCollider.isTrigger = true;
        }

        foreach(var boxCollider in boxColliders)
        {
            boxCollider.isTrigger = true;
        }

        animator.enabled = true;
    }

    public void ActivateRagdoll()
    {
        foreach (var rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = false;
        }
        animator.enabled = false;
    }
}
