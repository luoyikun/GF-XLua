using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class PSNGUIRootHelper : MonoBehaviour
{
    public UIRoot NGUIRoot { get; private set; }

    public void Awake()
    {
        NGUIRoot = GetComponent<UIRoot>();
    }
    void LateUpdate()
    {
        UIPanel.RefreshAllNGUIPanel();
    }
}
