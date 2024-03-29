﻿//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2020 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;

/// <summary>
/// Makes it possible to animate a color of the widget.
/// </summary>

[ExecuteInEditMode]
[RequireComponent(typeof(UIWidget))]
public class AnimatedColor : MonoBehaviour
{
	public Color color = Color.white;
	
	UIWidget mWidget;

	void OnEnable () { mWidget = GetComponent<UIWidget>(); color = mWidget.color; LateUpdate(); }
	void LateUpdate () { mWidget.color = color; }
}
