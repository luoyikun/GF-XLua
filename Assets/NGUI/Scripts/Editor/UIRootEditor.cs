//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2020 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(UIRoot))]
public class UIRootEditor : Editor
{
    public int[] MaxBuffer = new int[16];
    public void Awake()
    {
       
    }
    public override void OnInspectorGUI ()
	{
		serializedObject.Update();
		NGUIEditorTools.SetLabelWidth(110f);

		SerializedProperty sp = NGUIEditorTools.DrawProperty("Scaling Style", serializedObject, "scalingStyle");

		UIRoot.Scaling scaling = (UIRoot.Scaling)sp.intValue;

		if (scaling == UIRoot.Scaling.Flexible)
		{
			NGUIEditorTools.DrawProperty("Minimum Height", serializedObject, "minimumHeight");
			NGUIEditorTools.DrawProperty("Maximum Height", serializedObject, "maximumHeight");
			NGUIEditorTools.DrawProperty("Shrink Portrait UI", serializedObject, "shrinkPortraitUI");
			NGUIEditorTools.DrawProperty("Adjust by DPI", serializedObject, "adjustByDPI");

			EditorGUILayout.HelpBox("Also known as the 'Pixel-Perfect' mode, this setting makes NGUI's virtual pixels match the screen. This means that your UI will look smaller on high resolution devices and bigger on lower resolution devices, but it will always be as crisp as it can be." +
				"\n\nIdeal usage: PC games with a modular user interface that takes advantage of widget anchoring.", MessageType.Info);
		}
		else
		{
			GUILayout.BeginHorizontal();
			NGUIEditorTools.DrawProperty("Content Width", serializedObject, "manualWidth", GUILayout.Width(160f));
			NGUIEditorTools.SetLabelWidth(26f);
			bool fitWidth = NGUIEditorTools.DrawProperty("Fit", serializedObject, "fitWidth").boolValue;
			NGUIEditorTools.SetLabelWidth(110f);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			NGUIEditorTools.DrawProperty("Content Height", serializedObject, "manualHeight", GUILayout.Width(160f));
			NGUIEditorTools.SetLabelWidth(26f);
			bool fitHeight = NGUIEditorTools.DrawProperty("Fit", serializedObject, "fitHeight").boolValue;
			NGUIEditorTools.SetLabelWidth(110f);
			GUILayout.EndHorizontal();

			if (fitHeight)
			{
				if (fitWidth)
				{
					EditorGUILayout.HelpBox("Whatever you create within the content frame (blue outline) will always be visible, regardless of the screen's aspect ratio." +
						"\n\nThink of it as choosing the 'Fit' desktop background style.", MessageType.Info);
				}
				else
				{
					EditorGUILayout.HelpBox("This setting will keep your UI look the same on all screen sizes relative to the height of the screen." +
						"\n\nYou will still be able to see more or less on the left and right sides of the screen as the aspect ratio changes.", MessageType.Info);
				}
			}
			else if (fitWidth)
			{
				EditorGUILayout.HelpBox("This setting will keep your UI look the same on all screen sizes relative to the width of the screen." +
					"\n\nYou will still be able to see more or less on the top and bottom sides sides of the screen as the aspect ratio changes.", MessageType.Info);
			}
			else
			{
				EditorGUILayout.HelpBox("Your UI within the content frame (blue outline) will always fill the screen, and edges will be cropped based on the aspect ratio." +
					"\n\nThink of it as choosing the 'Fill' desktop background style.", MessageType.Info);
			}
		}
        EditorGUILayout.LabelField(string.Format("UI Buffer GeometryCount:{0} Active:{1} Total:{2}", PSNGUITools.m_GeometryStack.Count,PSNGUITools.m_HashGeometry.Count,(PSNGUITools.m_HashGeometry.Count+ PSNGUITools.m_GeometryStack.Count)));
        int stackCount,activeCount,totalCount;
        PSNGUITools.m_ColorStack.GetCount(out stackCount, out activeCount, out totalCount);

        EditorGUILayout.LabelField(string.Format("UI Buffer StackCount {0} ActiveCount:{1} TotalCount:{2}", stackCount,activeCount,totalCount));
        
        EditorGUILayout.LabelField(string.Format("UI Rect BetterList StackCount {0}", PSNGUITools.m_StackBetterListUIRect.Count));
        for (int i = 0; i < PSNGUITools.m_ColorStack.m_ListStackBuffer.Length; i++)
        {
            var u = PSNGUITools.m_ColorStack.m_ListStackBuffer[i].Count;
            var a = PSNGUITools.m_ColorStack.m_ActiveListBuffer[i].Count;
            var total = a + u;
            var m = MaxBuffer[i];
            if (total > m)
            {
                m = total;
                MaxBuffer[i] = m;
            }
            EditorGUILayout.LabelField(string.Format("index {0} Unactive:{1}  Activie:{2} Total:{3} Max:{4}",i,u ,a,total,m));
        }
        serializedObject.ApplyModifiedProperties();
	}
   
}
