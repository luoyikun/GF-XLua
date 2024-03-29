﻿//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2020 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TweenScale))]
public class TweenScaleEditor : UITweenerEditor
{
	public override void OnInspectorGUI ()
	{
		GUILayout.Space(6f);
		NGUIEditorTools.SetLabelWidth(120f);

		TweenScale tw = target as TweenScale;
		GUI.changed = false;

		Vector3 from = EditorGUILayout.Vector3Field("From", tw.from);
		Vector3 to = EditorGUILayout.Vector3Field("To", tw.to);
		bool updateTable = EditorGUILayout.Toggle("Update Table", tw.updateTable);
		UITable table = null;
		if (updateTable)
        {
			table = (UITable)EditorGUILayout.ObjectField("Update Table", tw.mTable, typeof(UITable), true);
        }

		if (GUI.changed)
		{
			NGUIEditorTools.RegisterUndo("Tween Change", tw);
			tw.from = from;
			tw.to = to;
			tw.updateTable = updateTable;
			tw.mTable = table;
			NGUITools.SetDirty(tw);
		}

		DrawCommonProperties();
	}
}
