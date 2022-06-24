//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2019 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// If you don't have or don't wish to create an atlas, you can simply use this script to draw a texture.
/// Keep in mind though that this will create an extra draw call with each UITexture present, so it's
/// best to use it only for backgrounds or temporary visible widgets.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Texture")]
public class UITexture : UIBasicSprite
{
	[HideInInspector][SerializeField] Rect mRect = new Rect(0f, 0f, 1f, 1f);
	[HideInInspector][SerializeField] Texture mTexture;
	[HideInInspector][SerializeField] Shader mShader;
	[HideInInspector][SerializeField] Vector4 mBorder = Vector4.zero;
    [HideInInspector][SerializeField] Vector4 mDoubleBorder = Vector4.zero;
    [HideInInspector][SerializeField] bool mFixedAspect = false;

	[System.NonSerialized] int mPMA = -1;

	/// <summary>
	/// Texture used by the UITexture. You can set it directly, without the need to specify a material.
	/// </summary>

	public override Texture mainTexture
	{
		get
		{
			if (mTexture != null) return mTexture;
			if (mMat != null) return mMat.mainTexture;
			return null;
		}
		set
		{
			if (mTexture != value)
			{
				if (drawCall != null && drawCall.widgetCount == 1 && mMat == null)
				{
					mTexture = value;
					drawCall.mainTexture = value;
				}
				else
				{
					RemoveFromPanel();
					mTexture = value;
					mPMA = -1;
					MarkAsChanged();
				}
			}
		}
	}

	/// <summary>
	/// Material used by the widget.
	/// </summary>

	public override Material material
	{
		get
		{
			return mMat;
		}
		set
		{
			if (mMat != value)
			{
				RemoveFromPanel();
				mShader = null;
				mMat = value;
				mPMA = -1;
				MarkAsChanged();
			}
		}
	}

	/// <summary>
	/// Shader used by the texture when creating a dynamic material (when the texture was specified, but the material was not).
	/// </summary>

	public override Shader shader
	{
		get
		{
			if (mMat != null) return mMat.shader;
			if (mShader == null) mShader = PSNGUITools.GetShader("Unlit/Transparent Colored");
			return mShader;
		}
		set
		{
			if (mShader != value)
			{
				if (drawCall != null && drawCall.widgetCount == 1 && mMat == null)
				{
					mShader = value;
					drawCall.shader = value;
				}
				else
				{
					RemoveFromPanel();
					mShader = value;
					mPMA = -1;
					mMat = null;
					MarkAsChanged();
				}
			}
		}
	}

	/// <summary>
	/// Whether the texture is using a premultiplied alpha material.
	/// </summary>

	public override bool premultipliedAlpha
	{
		get
		{
			if (mPMA == -1)
			{
				Material mat = material;
				mPMA = (mat != null && mat.shader != null && mat.shader.name.Contains("Premultiplied")) ? 1 : 0;
			}
			return (mPMA == 1);
		}
	}


	/// <summary>
	/// Sprite's border. X = left, Y = bottom, Z = right, W = top.
	/// </summary>

	public override Vector4 border
	{
		get
		{
			return mBorder;
		}
		set
		{
			if (mBorder != value)
			{
				mBorder = value;
				MarkAsChanged();
			}
		}
	}
    public Vector4 doubleBorder
    {
        get
        {
            return mDoubleBorder;
        }
        set
        {
            if (mDoubleBorder != value)
            {
                mDoubleBorder = value;
                MarkAsChanged();
            }
        }
    }
	/// <summary>
	/// UV rectangle used by the texture.
	/// </summary>

	public Rect uvRect
	{
		get
		{
			return mRect;
		}
		set
		{
			if (mRect != value)
			{
				mRect = value;
				MarkAsChanged();
			}
		}
	}

	/// <summary>
	/// Widget's dimensions used for drawing. X = left, Y = bottom, Z = right, W = top.
	/// This function automatically adds 1 pixel on the edge if the texture's dimensions are not even.
	/// It's used to achieve pixel-perfect sprites even when an odd dimension widget happens to be centered.
	/// </summary>

	public override Vector4 drawingDimensions
	{
		get
		{
			Vector2 offset = pivotOffset;

			float x0 = -offset.x * mWidth;
			float y0 = -offset.y * mHeight;
			float x1 = x0 + mWidth;
			float y1 = y0 + mHeight;

			if (mTexture != null && mType != UISprite.Type.Tiled)
			{
				int w = mTexture.width;
				int h = mTexture.height;
				int padRight = 0;
				int padTop = 0;

				float px = 1f;
				float py = 1f;

				if (w > 0 && h > 0 && (mType == UISprite.Type.Simple || mType == UISprite.Type.Filled))
				{
					if ((w & 1) != 0) ++padRight;
					if ((h & 1) != 0) ++padTop;

					px = (1f / w) * mWidth;
					py = (1f / h) * mHeight;
				}

				if (mFlip == UISprite.Flip.Horizontally || mFlip == UISprite.Flip.Both)
				{
					x0 += padRight * px;
				}
				else x1 -= padRight * px;

				if (mFlip == UISprite.Flip.Vertically || mFlip == UISprite.Flip.Both)
				{
					y0 += padTop * py;
				}
				else y1 -= padTop * py;
			}

			float fw, fh;

			if (mFixedAspect)
			{
				fw = 0f;
				fh = 0f;
			}
			else
			{
				Vector4 br = border;
				fw = br.x + br.z;
				fh = br.y + br.w;
			}

			float vx = Mathf.Lerp(x0, x1 - fw, mDrawRegion.x);
			float vy = Mathf.Lerp(y0, y1 - fh, mDrawRegion.y);
			float vz = Mathf.Lerp(x0 + fw, x1, mDrawRegion.z);
			float vw = Mathf.Lerp(y0 + fh, y1, mDrawRegion.w);

			return new Vector4(vx, vy, vz, vw);
		}
	}

    [HideInInspector] [SerializeField] protected Vector2Int mInsideSize = Vector2Int.zero;
    public Vector2Int insideSize
    {
        get
        {
            return mInsideSize;
        }
        set
        {
            if (mInsideSize != value)
            {
                mInsideSize = value;
                MarkAsChanged();
            }
        }
    }
    public virtual Vector4 drawingDimensionsInside
    {
        get
        {
            Vector2 offset = pivotOffset;

            float x0 = -offset.x * insideSize.x;
            float y0 = -offset.y * insideSize.y;
            float x1 = x0 + insideSize.x;
            float y1 = y0 + insideSize.y;
            if (mTexture != null && mType != UISprite.Type.Tiled)
            {
                int w = mTexture.width;
                int h = mTexture.height;
                int padRight = 0;
                int padTop = 0;

                float px = 1f;
                float py = 1f;

                if (w > 0 && h > 0 && (mType == UISprite.Type.Simple || mType == UISprite.Type.Filled))
                {
                    if ((w & 1) != 0) ++padRight;
                    if ((h & 1) != 0) ++padTop;

                    px = (1f / w) * mWidth;
                    py = (1f / h) * mHeight;
                }

                if (mFlip == UISprite.Flip.Horizontally || mFlip == UISprite.Flip.Both)
                {
                    x0 += padRight * px;
                }
                else x1 -= padRight * px;

                if (mFlip == UISprite.Flip.Vertically || mFlip == UISprite.Flip.Both)
                {
                    y0 += padTop * py;
                }
                else y1 -= padTop * py;
            }

            float fw, fh;

            if (mFixedAspect)
            {
                fw = 0f;
                fh = 0f;
            }
            else
            {
                Vector4 br = doubleBorder;
                fw = br.x + br.z;
                fh = br.y + br.w;
            }

            float vx = Mathf.Lerp(x0, x1 - fw, mDrawRegion.x);
            float vy = Mathf.Lerp(y0, y1 - fh, mDrawRegion.y);
            float vz = Mathf.Lerp(x0 + fw, x1, mDrawRegion.z);
            float vw = Mathf.Lerp(y0 + fh, y1, mDrawRegion.w);

            return new Vector4(vx, vy, vz, vw);
        }
    }
    /// <summary>
    /// Whether the drawn texture will always maintain a fixed aspect ratio.
    /// This setting is not compatible with drawRegion adjustments (sliders, progress bars, etc).
    /// </summary>

    public bool fixedAspect
	{
		get
		{
			return mFixedAspect;
		}
		set
		{
			if (mFixedAspect != value)
			{
				mFixedAspect = value;
				mDrawRegion = new Vector4(0f, 0f, 1f, 1f);
				MarkAsChanged();
			}
		}
	}

	/// <summary>
	/// Adjust the scale of the widget to make it pixel-perfect.
	/// </summary>

	public override void MakePixelPerfect ()
	{
		base.MakePixelPerfect();
		if (mType == Type.Tiled) return;

		Texture tex = mainTexture;
		if (tex == null) return;

		if (mType == Type.Simple || mType == Type.Filled || !hasBorder)
		{
			if (tex != null)
			{
				int w = tex.width;
				int h = tex.height;

				if ((w & 1) == 1) ++w;
				if ((h & 1) == 1) ++h;

				width = w;
				height = h;
			}
		}
	}

	/// <summary>
	/// Adjust the draw region if the texture is using a fixed aspect ratio.
	/// </summary>

	protected override void OnUpdate ()
	{
		base.OnUpdate();
		
		if (mFixedAspect)
		{
			Texture tex = mainTexture;

			if (tex != null)
			{
				int w = tex.width;
				int h = tex.height;
				if ((w & 1) == 1) ++w;
				if ((h & 1) == 1) ++h;
				float widgetWidth = mWidth;
				float widgetHeight = mHeight;
				float widgetAspect = widgetWidth / widgetHeight;
				float textureAspect = (float)w / h;

				if (textureAspect < widgetAspect)
				{
					float x = (widgetWidth - widgetHeight * textureAspect) / widgetWidth * 0.5f;
					drawRegion = new Vector4(x, 0f, 1f - x, 1f);
				}
				else
				{
					float y = (widgetHeight - widgetWidth / textureAspect) / widgetHeight * 0.5f;
					drawRegion = new Vector4(0f, y, 1f, 1f - y);
				}
			}
		}
	}
    public override int GetVertexCount()
    {
        var tex = mainTexture;
        if (tex == null) return 0;
        return base.GetVertexCount();
    }
    /// <summary>
    /// Virtual function called by the UIPanel that fills the buffers.
    /// </summary>
    public override void OnFill(List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
    {
        Texture tex = mainTexture;
        if (tex == null) return;

        Rect outer = new Rect(mRect.x * tex.width, mRect.y * tex.height, tex.width * mRect.width, tex.height * mRect.height);

        float w = 1f / tex.width;
        float h = 1f / tex.height;


        int offset = verts.Count;
        if (type == Type.DoubleSliced)
        {
            Rect inner2 = outer;
            Vector4 br2 = doubleBorder;
            inner2.xMin += br2.x;
            inner2.yMin += br2.y;
            inner2.xMax -= br2.z;
            inner2.yMax -= br2.w;


            Rect inner = inner2;

            Vector4 br = border;
            inner.xMin += (br.x - br2.x);
            inner.yMin += (br.y - br2.y);
            inner.xMax -= (br.z - br2.z);
            inner.yMax -= (br.w - br2.w);

            inner.xMin *= w;
            inner.xMax *= w;
            inner.yMin *= h;
            inner.yMax *= h;
            inner2.xMin *= w;
            inner2.xMax *= w;
            inner2.yMin *= h;
            inner2.yMax *= h;
            outer.xMin *= w;
            outer.xMax *= w;
            outer.yMin *= h;
            outer.yMax *= h;
            mOuterUV = outer;
            mInnerUV = inner;
            var v = drawingDimensions;
            var u = drawingUVs;
            var c = drawingColor;
            var v2 = drawingDimensionsInside;
            DoubleSlicedFill(verts, uvs, cols, inner2, v2, ref v, ref u, ref c);
        }
        else
        {
            Rect inner = outer;

            Vector4 br = border;
            inner.xMin += br.x;
            inner.yMin += br.y;
            inner.xMax -= br.z;
            inner.yMax -= br.w;
            inner.xMin *= w;
            inner.xMax *= w;
            inner.yMin *= h;
            inner.yMax *= h;
            outer.xMin *= w;
            outer.xMax *= w;
            outer.yMin *= h;
            outer.yMax *= h;
            Fill(verts, uvs, cols, outer, inner);
        }

        if (onPostFill != null)
            onPostFill(this, offset, verts, uvs, cols);
    }
    public override void GetOuterAndInner(out Rect outer, out Rect inner, Texture tex)
    {
        outer = new Rect(mRect.x * tex.width, mRect.y * tex.height, tex.width * mRect.width, tex.height * mRect.height);

        float w = 1f / tex.width;
        float h = 1f / tex.height;
        inner = outer;

        Vector4 br = border;
        inner.xMin += br.x;
        inner.yMin += br.y;
        inner.xMax -= br.z;
        inner.yMax -= br.w;
        inner.xMin *= w;
        inner.xMax *= w;
        inner.yMin *= h;
        inner.yMax *= h;
        outer.xMin *= w;
        outer.xMax *= w;
        outer.yMin *= h;
        outer.yMax *= h;
    }

    protected void DoubleSlicedFill(List<Vector3> verts, List<Vector2> uvs, List<Color> cols, Rect inner2, Vector4 v2, ref Vector4 v, ref Vector4 u, ref Color gc)
    {

        //Vector4 br = border * pixelSize;
        //Vector4 br2 = doubleBorder * pixelSize;
        Vector4 br2 = border * pixelSize;
        Vector4 br = doubleBorder * pixelSize;
        //if (br.x == 0f && br.y == 0f && br.z == 0f && br.w == 0f)
        //{
        //    SimpleFill(verts, uvs, cols, ref v, ref u, ref gc);
        //    return;
        //}

        mTempPos[0].x = v.x;
        mTempPos[0].y = v.y;
        mTempPos[3].x = v.z;
        mTempPos[3].y = v.w;
        mTempPos2[0].x = v2.x;
        mTempPos2[0].y = v2.y;
        mTempPos2[3].x = v2.z;
        mTempPos2[3].y = v2.w;
        if (mFlip == Flip.Horizontally || mFlip == Flip.Both)
        {
            mTempPos[1].x = mTempPos[0].x + br.z;
            mTempPos[2].x = mTempPos[3].x - br.x;

            mTempPos2[1].x = mTempPos2[0].x + br2.z;
            mTempPos2[2].x = mTempPos2[3].x - br2.x;

            mTempUVs[3].x = inner2.xMin;
            mTempUVs[2].x = mInnerUV.xMin;
            mTempUVs[1].x = mInnerUV.xMax;
            mTempUVs[0].x = inner2.xMax;

            mTempUVs2[3].x = mOuterUV.xMin;
            mTempUVs2[2].x = inner2.xMin;
            mTempUVs2[1].x = inner2.xMax;
            mTempUVs2[0].x = mOuterUV.xMax;
        }
        else
        {
            mTempPos[1].x = mTempPos[0].x + br.x;
            mTempPos[2].x = mTempPos[3].x - br.z;

            mTempPos2[1].x = mTempPos2[0].x + br2.x;
            mTempPos2[2].x = mTempPos2[3].x - br2.z;

            mTempUVs[0].x = inner2.xMin;
            mTempUVs[1].x = mInnerUV.xMin;
            mTempUVs[2].x = mInnerUV.xMax;
            mTempUVs[3].x = inner2.xMax;

            mTempUVs2[0].x = inner2.xMin;
            mTempUVs2[1].x = mOuterUV.xMin;
            mTempUVs2[2].x = mOuterUV.xMax;
            mTempUVs2[3].x = inner2.xMax;
        }

        if (mFlip == Flip.Vertically || mFlip == Flip.Both)
        {
            mTempPos[1].y = mTempPos[0].y + br.w;
            mTempPos[2].y = mTempPos[3].y - br.y;

            mTempPos2[1].y = mTempPos2[0].y + br2.w;
            mTempPos2[2].y = mTempPos2[3].y - br2.y;

            mTempUVs[3].y = inner2.yMin;
            mTempUVs[2].y = mInnerUV.yMin;
            mTempUVs[1].y = mInnerUV.yMax;
            mTempUVs[0].y = inner2.yMax;

            mTempUVs2[3].y = mOuterUV.yMin;
            mTempUVs2[2].y = inner2.yMin;
            mTempUVs2[1].y = inner2.yMax;
            mTempUVs2[0].y = mOuterUV.yMax;
        }
        else
        {
            mTempPos[1].y = mTempPos[0].y + br.y;
            mTempPos[2].y = mTempPos[3].y - br.w;

            mTempPos2[1].y = mTempPos2[0].y + br2.y;
            mTempPos2[2].y = mTempPos2[3].y - br2.w;

            mTempUVs[0].y = inner2.yMin;
            mTempUVs[1].y = mInnerUV.yMin;
            mTempUVs[2].y = mInnerUV.yMax;
            mTempUVs[3].y = inner2.yMax;

            mTempUVs2[0].y = mOuterUV.yMin;
            mTempUVs2[1].y = inner2.yMin;
            mTempUVs2[2].y = inner2.yMax;
            mTempUVs2[3].y = mOuterUV.yMax;
        }
        for (int x = 0; x < 3; x++)
        {
            int x2 = x + 1;

            for (int y = 0; y < 3; y++)
            {
                int y2 = y + 1;
                Fill(verts, uvs, cols, mTempPos2[x].x, mTempPos2[x2].x, mTempPos2[y].y, mTempPos2[y2].y,
                    mTempUVs[x].x, mTempUVs[x2].x, mTempUVs[y].y, mTempUVs[y2].y, gc);

                switch (y)
                {
                    case 0:
                        switch (x)
                        {
                            case 0:
                                Fill(verts, uvs, cols, mTempPos[x].x, mTempPos2[x].x, mTempPos[y].y, mTempPos2[y].y,
mTempUVs[x].x, mTempUVs2[x].x, mTempUVs[y].y, mTempUVs2[y].y, gc);
                                break;
                            case 1:
                                Fill(verts, uvs, cols,  mTempPos2[x - 1].x, mTempPos2[x2 + 1].x, mTempPos[y].y, mTempPos2[y].y,
mTempUVs2[x - 1].x, mTempUVs2[x2 + 1].x, mTempUVs[y].y, mTempUVs2[y].y, gc);
                                break;
                            case 2:
                                Fill(verts, uvs, cols, mTempPos2[x2].x, mTempPos[x2].x, mTempPos[y].y, mTempPos2[y].y,
mTempUVs2[x2].x, mTempUVs[x2].x, mTempUVs[y].y, mTempUVs2[y].y, gc);
                                break;

                            default:
                                break;
                        }
                        break;
                    case 1:
                        switch (x)
                        {
                            case 0:
                                Fill(verts, uvs, cols, mTempPos[x].x, mTempPos2[x].x, mTempPos2[y - 1].y, mTempPos2[y2 + 1].y,
mTempUVs[x].x, mTempUVs2[x].x, mTempUVs2[y - 1].y, mTempUVs2[y2 + 1].y, gc);
                                break;
                            case 2:
                                Fill(verts, uvs, cols, mTempPos2[x2].x, mTempPos[x2].x, mTempPos2[y - 1].y, mTempPos2[y2 + 1].y,
mTempUVs2[x2].x, mTempUVs[x2].x, mTempUVs2[y - 1].y, mTempUVs2[y2 + 1].y, gc);
                                break;
                            default:
                                break;
                        }
                        break;
                    case 2:
                        switch (x)
                        {
                            case 0:
                                Fill(verts, uvs, cols, mTempPos[x].x, mTempPos2[x].x, mTempPos2[y2].y, mTempPos[y2].y,
mTempUVs[x].x, mTempUVs2[x].x, mTempUVs2[y2].y, mTempUVs[y2].y, gc);
                                break;
                            case 1:
                                Fill(verts, uvs, cols,mTempPos2[x - 1].x, mTempPos2[x2 + 1].x, mTempPos2[y2].y, mTempPos[y2].y,
mTempUVs2[x - 1].x, mTempUVs2[x2 + 1].x, mTempUVs2[y].y, mTempUVs2[y2].y, gc);
                                break;
                            case 2:
                                Fill(verts, uvs, cols, mTempPos2[x2].x, mTempPos[x2].x, mTempPos2[y2].y, mTempPos[y2].y,
mTempUVs2[x2].x, mTempUVs[x2].x, mTempUVs2[y2].y, mTempUVs[y2].y, gc);
                                break;

                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
