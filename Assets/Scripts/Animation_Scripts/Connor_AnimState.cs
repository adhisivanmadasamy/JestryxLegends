using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connor_AnimState : MonoBehaviour
{
    public PlayerController controller;
    Animator animator;

    Vector3 gotMoveAmount;

    
    void Start()
    {
        animator = GetComponent<Animator>();
        
    }

    // Update is called once per frame
    void Update()
    {
        gotMoveAmount =  controller.moveAmount;
        Debug.Log(gotMoveAmount);

        MoveFront();
        MoveSide();

        

    }

    public void MoveFront()
    {
        if (gotMoveAmount.z > 1f && gotMoveAmount.z <= 3f)
        {
            animator.SetInteger("VertSpeed", 1);
        }
        else if (gotMoveAmount.z > 3f)
        {
            animator.SetInteger("VertSpeed", 2);
        }
        else if (gotMoveAmount.z < 1f && gotMoveAmount.z > -1f)
        {
            animator.SetInteger("VertSpeed", 0);
        }
        else if (gotMoveAmount.z < -1f && gotMoveAmount.z >= -3f)
        {
            animator.SetInteger("VertSpeed", -1);
        }
        else if (gotMoveAmount.z < -3f)
        {
            animator.SetInteger("VertSpeed", -2);
        }
    }

    public void MoveSide()
    {
        if (gotMoveAmount.x > 1f && gotMoveAmount.x <= 3f)
        {
            animator.SetInteger("HorizSpeed", 1);
        }
        else if (gotMoveAmount.x > 3f)
        {
            animator.SetInteger("HorizSpeed", 2);
        }
        else if (gotMoveAmount.x < 1f && gotMoveAmount.x > -1f)
        {
            animator.SetInteger("HorizSpeed", 0);
        }
        else if (gotMoveAmount.x < -1f && gotMoveAmount.x >= -3f)
        {
            animator.SetInteger("HorizSpeed", -1);
        }
        else if (gotMoveAmount.x < -3f)
        {
            animator.SetInteger("HorizSpeed", -2);
        }
    }
}
