using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOverworld : MonoBehaviour
{
    CharacterController controller;
    Animator animator;
    public float speed;
    float turnSmoothVelocity;
    public float turnSmoothTime;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 inputVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        animator.SetFloat("Blend", inputVector.magnitude);
        controller.Move(inputVector * speed * Time.deltaTime);

        //Rotate
        if (inputVector.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputVector.x, inputVector.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }
    }
}
