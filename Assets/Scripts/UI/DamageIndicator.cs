using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageIndicator : MonoBehaviour
{
    Animator animator;
    public TextMeshPro text;

    void Start()
    {
        GameEvents.instance.doDamage += DoDamage;
        animator = GetComponent<Animator>();
        text = GetComponent<TextMeshPro>();
    }

    private void Update()
    {
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    private void DoDamage(Unit attacking, Unit takingDamage, int damage)
    {
        text.text = $"{damage}";
        animator.SetTrigger("DamagePopUp");
    }

    void HidePopup()
    {
        animator.SetTrigger("DamagePopUpDisable");
    }
}
