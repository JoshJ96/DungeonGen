using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageIndicator : MonoBehaviour
{
    public TextMeshPro text;

    void Start()
    {
        GetComponent<Animator>().SetTrigger("DamagePopUp");
        text = GetComponent<TextMeshPro>();
    }

    void HidePopup()
    {
        Destroy(this.gameObject);
    }
}
