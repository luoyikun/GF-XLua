//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2020 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// All children added to the game object with this script will be arranged into a table
/// with rows and columns automatically adjusting their size to fit their content
/// (think "table" tag in HTML).
/// </summary>

[AddComponentMenu("NGUI/Interaction/Table")]
public class UITable : UIWidgetContainer
{
    public delegate void OnReposition();

    [DoNotObfuscateNGUI]
    public enum Direction
    {
        Down,
        Up,
    }

    [DoNotObfuscateNGUI]
    public enum Sorting
    {
        None,
        Alphabetic,
        Horizontal,
        Vertical,
        Custom,
    }

    /// <summary>
    /// How many columns there will be before a new line is started. 0 means unlimited.
    /// </summary>

    public int columns = 0;

    /// <summary>
    /// Which way the new lines will be added.
    /// </summary>

    public Direction direction = Direction.Down;

    /// <summary>
    /// How to sort the grid's elements.
    /// </summary>

    public Sorting sorting = Sorting.None;

    [Tooltip("Whether the sort order will be inverted")]
    public bool inverted = false;
    /// <summary>
    /// Final pivot point for the table itself.
    /// </summary>

    public UIWidget.Pivot pivot = UIWidget.Pivot.TopLeft;

    /// <summary>
    /// Final pivot point for the table's content.
    /// </summary>

    public UIWidget.Pivot cellAlignment = UIWidget.Pivot.TopLeft;

    /// <summary>
    /// Whether inactive children will be discarded from the table's calculations.
    /// </summary>

    public bool hideInactive = true;

    /// <summary>
    /// Whether the parent container will be notified of the table's changes.
    /// </summary>

    public bool keepWithinPanel = false;

    /// <summary>
    /// Padding around each entry, in pixels.
    /// </summary>

    public Vector2 padding = Vector2.zero;

    /// <summary>
    /// Delegate function that will be called when the table repositions its content.
    /// </summary>

    public OnReposition onReposition;

    /// <summary>
    /// Custom sort delegate, used when the sorting method is set to 'custom'.
    /// </summary>

    public System.Comparison<Transform> onCustomSort;

    protected UIPanel mPanel;
    protected bool mInitDone = false;
    protected bool mReposition = false;

    /// <summary>
    /// Reposition the children on the next Update().
    /// </summary>

    public bool repositionNow { set { if (value) { mReposition = true; enabled = true; } } }

    /// <summary>
    /// Get the current list of the grid's children.
    /// </summary>
    public List<Transform> GetChildList()
    {
        childList.Clear();
        return GetChildListInternal(childList, transform);
    }
    private List<Transform> childList
    {
        get
        {
            if (mChildList == null)
            {
                mChildList = PSNGUITools.GetTransformList();
            }
            return mChildList;
        }
    }
    private List<Transform> mChildList;
    private List<Transform> GetChildListInternal(List<Transform> list,Transform myTrans)
    {
        for (int i = 0; i < myTrans.childCount; ++i)
        {
            Transform t = myTrans.GetChild(i);
            if (!hideInactive || (t && NGUITools.GetActive(t.gameObject)))
                list.Add(t);
        }

        // Sort the list using the desired sorting logic
        if (sorting != Sorting.None)
        {
            if (sorting == Sorting.Alphabetic) { if (inverted) list.Sort(UIGrid.m_SortByNameInv); else list.Sort(UIGrid.m_SortByName); }
            else if (sorting == Sorting.Horizontal) { if (inverted) list.Sort(UIGrid.m_SortHorizontalInv); else list.Sort(UIGrid.m_SortHorizontal); }
            else if (sorting == Sorting.Vertical) { if (inverted) list.Sort(UIGrid.m_SortVerticalInv); else list.Sort(UIGrid.m_SortVertical); }
            else if (onCustomSort != null) list.Sort(onCustomSort);
            else Sort(list);
        }
        return list;
    }
    private void OnDestroy()
    {
        if (mChildList != null)
        {
            PSNGUITools.ReleaseTransformList(mChildList);
            mChildList = null;
        }
    }
    private List<Transform> GetChildListPrivate()
    {
        PSNGUITools.UIGridAndUITableTransformBuffer.Clear();
        return GetChildListInternal(PSNGUITools.UIGridAndUITableTransformBuffer, transform);
    }
    /// <summary>
    /// Want your own custom sorting logic? Override this function.
    /// </summary>

    protected virtual void Sort(List<Transform> list) { list.Sort(UIGrid.m_SortByName); }
    protected virtual void OnEnable() { mReposition = true; }

    /// <summary>
    /// Position the grid's contents when the script starts.
    /// </summary>

    protected virtual void Start()
    {
        Init();
        Reposition();
        enabled = false;
    }

    /// <summary>
    /// Find the necessary components.
    /// </summary>

    protected virtual void Init()
    {
        mInitDone = true;
        mPanel = NGUITools.FindInParents<UIPanel>(gameObject);
    }
    /// <summary>
    /// Is it time to reposition? Do so now.
    /// </summary>

    protected virtual void LateUpdate()
    {
        if (mReposition)
        {
            Reposition();
        }
        enabled = false;
    }

    /// <summary>
    /// Reposition the content on inspector validation.
    /// </summary>

    void OnValidate() { if (!Application.isPlaying && NGUITools.GetActive(this)) Reposition(); }
    private void GenerateCachedIndexBuffer(Nordeus.DataStructures.VaryingBoundsList boundsList, int indexCount)
    {
        if (boundsList.size < indexCount)
        {
            boundsList.MakeLargerThan(indexCount);
            for (int i = 0; i < boundsList.size; i++)
            {
                boundsList.buffer[i] = default;
            }
            for (int i = boundsList.size; i < indexCount; i++)
            {
                boundsList.Add(default);
            }
        }
        else
        {
            for (int i = 0; i < indexCount; i++)
            {
                boundsList.buffer[i] = default;
            }
        }
    }

    /// <summary>
    /// Positions the grid items, taking their own size into consideration.
    /// </summary>
    private static Nordeus.DataStructures.VaryingBoundsList boundsBuffer = new Nordeus.DataStructures.VaryingBoundsList();
    private static Nordeus.DataStructures.VaryingBoundsList boundsRowsBuffer = new Nordeus.DataStructures.VaryingBoundsList();
    private static Nordeus.DataStructures.VaryingBoundsList boundsColsBuffer = new Nordeus.DataStructures.VaryingBoundsList();
    protected void RepositionVariableSize(List<Transform> children)
    {
        float xOffset = 0;
        float yOffset = 0;

        int cols = columns > 0 ? children.Count / columns + 1 : 1;
        int rows = columns > 0 ? columns : children.Count;
        int boundsBufferLength = cols * rows;

        GenerateCachedIndexBuffer(boundsBuffer,boundsBufferLength);
        var boundsBufferOriginalLength = boundsBuffer.OriginalLength();
        boundsBuffer.AsArrayOfLength((ulong)boundsBufferLength);

        GenerateCachedIndexBuffer(boundsRowsBuffer, rows);
        var boundsRowsBufferOriginalLenght = boundsRowsBuffer.OriginalLength();
        boundsRowsBuffer.AsArrayOfLength((ulong)rows);

        GenerateCachedIndexBuffer(boundsColsBuffer, cols);
        var boundsColsBufferOriginalLength = boundsColsBuffer.OriginalLength();
        boundsColsBuffer.AsArrayOfLength((ulong)cols);

        Bounds[] bounds = boundsBuffer.buffer;

        Bounds[] boundsRows = boundsRowsBuffer.buffer;
        Bounds[] boundsCols = boundsColsBuffer.buffer;
        //Bounds
        int x = 0;
        int y = 0;

        for (int i = 0, imax = children.Count; i < imax; ++i)
        {
            Transform t = children[i];
            Bounds b = NGUIMath.CalculateRelativeWidgetBounds(t, !hideInactive);

            Vector3 scale = t.localScale;
            b.min = Vector3.Scale(b.min, scale);
            b.max = Vector3.Scale(b.max, scale);
            bounds[(y) + (x * cols)] = b;
            boundsRows[x].Encapsulate(b);
            boundsCols[y].Encapsulate(b);

            if (++x >= columns && columns > 0)
            {
                x = 0;
                ++y;
            }
        }

        x = 0;
        y = 0;

        Vector2 po = NGUIMath.GetPivotOffset(cellAlignment);

        for (int i = 0, imax = children.Count; i < imax; ++i)
        {
            Transform t = children[i];
            Bounds b = bounds[(y) + (x * cols)];
            Bounds br = boundsRows[x];
            Bounds bc = boundsCols[y];

            Vector3 pos = t.localPosition;
            pos.x = xOffset + b.extents.x - b.center.x;
            pos.x -= Mathf.Lerp(0f, b.max.x - b.min.x - br.max.x + br.min.x, po.x) - padding.x;

            if (direction == Direction.Down)
            {
                pos.y = -yOffset - b.extents.y - b.center.y;
                pos.y += Mathf.Lerp(b.max.y - b.min.y - bc.max.y + bc.min.y, 0f, po.y) - padding.y;
            }
            else
            {
                pos.y = yOffset + b.extents.y - b.center.y;
                pos.y -= Mathf.Lerp(0f, b.max.y - b.min.y - bc.max.y + bc.min.y, po.y) - padding.y;
            }

            xOffset += br.size.x + padding.x * 2f;

            t.localPosition = pos;

            if (++x >= columns && columns > 0)
            {
                x = 0;
                ++y;

                xOffset = 0f;
                yOffset += bc.size.y + padding.y * 2f;
            }
        }
        boundsBuffer.AsArrayOfLength(boundsBufferOriginalLength);
        boundsColsBuffer.AsArrayOfLength(boundsColsBufferOriginalLength);
        boundsRowsBuffer.AsArrayOfLength(boundsRowsBufferOriginalLenght);
        
        // Apply the origin offset
        if (pivot != UIWidget.Pivot.TopLeft)
        {
            po = NGUIMath.GetPivotOffset(pivot);

            float fx, fy;

            Bounds b = NGUIMath.CalculateRelativeWidgetBounds(transform);

            fx = Mathf.Lerp(0f, b.size.x, po.x);
            fy = Mathf.Lerp(-b.size.y, 0f, po.y);

            Transform myTrans = transform;

            for (int i = 0; i < myTrans.childCount; ++i)
            {
                Transform t = myTrans.GetChild(i);
                SpringPosition sp = t.GetComponent<SpringPosition>();

                if (sp != null)
                {
                    sp.enabled = false;
                    sp.target.x -= fx;
                    sp.target.y -= fy;
                    sp.enabled = true;
                }
                else
                {
                    Vector3 pos = t.localPosition;
                    pos.x -= fx;
                    pos.y -= fy;
                    t.localPosition = pos;
                }
            }
        }
    }

    /// <summary>
    /// Recalculate the position of all elements within the table, sorting them alphabetically if necessary.
    /// </summary>

    [ContextMenu("Execute")]
    public virtual void Reposition()
    {
        if (Application.isPlaying && !mInitDone && NGUITools.GetActive(this)) Init();

        mReposition = false;
        var myTrans = transform;
        var ch = GetChildListPrivate();
        if (ch.Count > 0) RepositionVariableSize(ch);

        if (keepWithinPanel && mPanel != null)
        {
            //UnityEditor.MaterialEditor materialEditor;
            mPanel.ConstrainTargetToBounds(myTrans, true);
            var sv = mPanel.GetComponent<UIScrollView>();
            if (sv != null) sv.UpdateScrollbars(true);
        }

        if (onReposition != null) onReposition();
    }
}
