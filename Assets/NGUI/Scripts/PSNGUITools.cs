using System;
using System.Collections.Generic;
using UnityEngine;

public class PSNGUITools
{
    public static Func<string, Shader> GetShaderHelper = null;

    public static Shader GetShader(string name)
    {
        if (GetShaderHelper != null)
        {
            return GetShaderHelper(name);
        }

        return Shader.Find(name);
    }

    static PSNGUITools()
    {
        m_StackBetterListUIRect = new Stack<BetterList<UIRect>>();
        m_StackListUIDrawCall = new Stack<List<UIDrawCall>>();
        m_StackListUIWidget = new Stack<List<UIWidget>>();
        m_StackListTransform = new Stack<List<Transform>>();
        m_GeometryStack = new Stack<UIGeometry>();
        m_V3Stack = new NguiListPool<Vector3>();
        m_ColorStack = new NguiListPool<Color>();
        m_V2Stack = new NguiListPool<Vector2>();
        UIGridAndUITableTransformBuffer = new List<Transform>();
#if UNITY_EDITOR
        m_HashGeometry = new HashSet<UIGeometry>();
#endif
    }
    static public List<Transform> UIGridAndUITableTransformBuffer;
    static public int WhichPowerOfTwo(int input)
    {
        if (input == 0) return 0;
        else if (input <= 1) return 0;
        else if (input <= 2) return 1;
        else if (input <= 4) return 2;
        else if (input <= 8) return 3;
        else if (input <= 16) return 4;
        else if (input <= 32) return 5;
        else if (input <= 64) return 6;
        else if (input <= 128) return 7;
        else if (input <= 256) return 9;
        else if (input <= 512) return 9;
        else if (input <= 1024) return 10;
        else if (input <= 2048) return 11;
        else if (input <= 4096) return 12;
        else if (input <= 8192) return 13;
        else if (input <= 16384) return 14;
        else if (input <= int.MaxValue) return 15;
        else return 0;
    }
    internal const int StackBufferConstCount = 16;
    public class NguiListPool<T>
    {
        public NguiListPool()
        {
            m_ListStackBuffer = new Stack<List<T>>[StackBufferConstCount];
            for (int i = 0; i < StackBufferConstCount; i++)
            {
                m_ListStackBuffer[i] = new Stack<List<T>>();
            }
#if UNITY_EDITOR
            m_ActiveListBuffer = new HashSet<List<T>>[StackBufferConstCount];
            for (int i = 0; i < StackBufferConstCount; i++)
            {
                m_ActiveListBuffer[i] = new HashSet<List<T>>();
            }
#endif
        }
#if UNITY_EDITOR
        public HashSet<List<T>>[] m_ActiveListBuffer;
#endif
        public Stack<List<T>>[] m_ListStackBuffer;

        public List<T> Pop(int index = 0)
        {
            var stack = index >= 0 && index < m_ListStackBuffer.Length?m_ListStackBuffer[index]:null;
            if (stack != null && stack.Count > 0)
            {
#if UNITY_EDITOR
                var item = stack.Pop();
                m_ActiveListBuffer[index].Add(item);
                return item;
#else
                return stack.Pop();
#endif
            }
            else
            {
                int capatic = 1 << index;
#if UNITY_EDITOR
                 var item = new List<T>(capatic);
                 m_ActiveListBuffer[index].Add(item);
                 return item;
#else
                 return new List<T>(capatic);
#endif
            }
        }
#if UNITY_EDITOR
        public void GetCount(out int stackCount,out int activeCount,out int totalCount)
        {
            int sum = 0;
            for (int i = 0; i < m_ListStackBuffer.Length; i++)
            {
                sum += m_ListStackBuffer[i].Count;
            }
            stackCount = sum;
            sum = 0;
            for (int i = 0; i < m_ActiveListBuffer.Length; i++)
            {
                sum += m_ActiveListBuffer[i].Count;
            }
            activeCount = sum;
            totalCount = stackCount + activeCount;
        }
#endif
        public void Push(List<T> list)
        {
            var index = PSNGUITools.WhichPowerOfTwo(list.Capacity);
            if (index > 0 && index < m_ListStackBuffer.Length)
            {
                m_ListStackBuffer[index].Push(list);
            }
            else
            {
                m_ListStackBuffer[0].Push(list);
            }
#if UNITY_EDITOR
            m_ActiveListBuffer[index].Remove(list);
#endif
        }
    }

    internal static List<UIWidget> GetUIWidgetsList()
    {
        if (m_StackListUIWidget.Count > 0)
        {
            return m_StackListUIWidget.Pop();
        }
        return new List<UIWidget>();
    }

    internal static List<Transform> GetTransformList()
    {
        if (m_StackListTransform.Count>0)
        {
            return m_StackListTransform.Pop();
        }
        return new List<Transform>();
    }

    internal static List<UIDrawCall> GetUIDrawCallList()
    {
        if (m_StackListUIDrawCall.Count>0)
        {
            return m_StackListUIDrawCall.Pop();
        }
        return new List<UIDrawCall>();
    }

    internal static BetterList<UIRect> GetBetterListUIRect()
    {
        if (m_StackBetterListUIRect.Count>0)
        {
            return m_StackBetterListUIRect.Pop();
        }
        return new BetterList<UIRect>();
       
    }

    internal static void ReleaseUIWidgetList(List<UIWidget> list)
    {
        if (list != null)
        {
            list.Clear();
            m_StackListUIWidget.Push(list);
        }
    }
    public static Stack<List<UIWidget>> m_StackListUIWidget;
    internal static void ReleaseUIDrawCallList(List<UIDrawCall> list)
    {
        if (list != null)
        {
            list.Clear();
            m_StackListUIDrawCall.Push(list);
        }
    }
    public static Stack<List<UIDrawCall>> m_StackListUIDrawCall;

    public static void ReleaseTransformList(List<Transform> list)
    {
        if (list != null )
        {
            list.Clear();
            m_StackListTransform.Push(list);
        }
    }
    public static Stack<List<Transform>> m_StackListTransform;
    public static Stack<UIGeometry> m_GeometryStack;
    public static NguiListPool<Vector3> m_V3Stack;
    public static NguiListPool<Color> m_ColorStack;
    public static NguiListPool<Vector2> m_V2Stack;
#if UNITY_EDITOR
    public static HashSet<UIGeometry> m_HashGeometry;
#endif
    internal static UIGeometry GetGeometry()
    {
#if UNITY_EDITOR
        if (m_GeometryStack.Count > 0)
        {
            var g = m_GeometryStack.Pop();
            m_HashGeometry.Add(g);
            return g;
        }
        else
        {
            var g = new UIGeometry();
            m_HashGeometry.Add(g);
            return g;
        }
#else
        if (m_GeometryStack.Count > 0)
        {
            return m_GeometryStack.Pop();
        }
        return new UIGeometry();
#endif
    }

    internal static void ReleaseGeometry(UIGeometry geometry)
    {
        geometry.Release();
        m_GeometryStack.Push(geometry);
#if UNITY_EDITOR
        m_HashGeometry.Remove(geometry);
#endif
    }
    internal static int GetBufferStackIndex(int capacity)
    {
        return WhichPowerOfTwo(capacity);
    }
    internal static List<Vector3> GetV3List(int capacity = 0)
    {
        return m_V3Stack.Pop(GetBufferStackIndex(capacity));
    }

    internal static List<Vector2> GetV2List(int capacity = 0)
    {
        return m_V2Stack.Pop(GetBufferStackIndex(capacity));
    }

    internal static List<Color> GetColorList(int capacity = 0)
    {
        return m_ColorStack.Pop(GetBufferStackIndex(capacity));
    }

    internal static void ReleaseV3List(List<Vector3> verts)
    {
        verts.Clear();
        m_V3Stack.Push(verts);
    }

    internal static void ReleaseColorList(List<Color> cols)
    {
        cols.Clear();
        m_ColorStack.Push(cols);
    }

    internal static void ReleaseV2List(List<Vector2> uvs)
    {
        uvs.Clear();
        m_V2Stack.Push(uvs);
    }
    public static Stack<BetterList<UIRect>> m_StackBetterListUIRect;
    internal static void ReleaseBetterListUIRect(BetterList<UIRect> mChildrenBuffer)
    {
        mChildrenBuffer.Release();
        m_StackBetterListUIRect.Push(mChildrenBuffer);
    }
}
