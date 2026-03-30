using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public sealed class LoopScrollGroup : IDisposable
{
    #region Config

    private readonly ScrollRect scrollRect;
    private readonly RectTransform viewport;
    private readonly RectTransform content;
    private readonly RectTransform template;

    private readonly Vector2 spacing;
    private readonly RectOffset padding;
    private readonly float scrollThreshold;

    private readonly bool isVertical;

    #endregion

    #region Layout Data

    private readonly Vector2 itemSize;

    private int constraintCount;
    private int visibleCount;
    private int poolCount;

    #endregion

    #region Runtime State

    private readonly List<RectTransform> items = new List<RectTransform>();
    private Action<int, RectTransform> onRefresh;

    private int totalCount;
    private int startIndex;
    private int lastGroupIndex = -1;

    private bool disposed;

    #endregion

    #region Constructor

    public LoopScrollGroup(
        ScrollRect scrollRect,
        Vector2 spacing,
        RectOffset padding,
        float scrollThreshold = 0.1f)
    {
        this.scrollRect = scrollRect;
        this.spacing = spacing;
        this.padding = padding ?? new RectOffset();
        this.scrollThreshold = scrollThreshold;

        viewport = scrollRect.viewport;
        content = scrollRect.content;
        isVertical = scrollRect.vertical;

        if (content.childCount == 0)
            throw new Exception("Content 下必须至少有一个子节点作为模板");

        template = content.GetChild(0) as RectTransform;
        itemSize = template.rect.size;

        template.gameObject.SetActive(false);

        CalculateLayout();
        CreatePool();

        scrollRect.onValueChanged.AddListener(OnScroll);
    }

    #endregion

    #region Public API

    public void SetTotalCount(int count)
    {
        totalCount = Mathf.Max(0, count);
        UpdateContentSize();
        ForceRefresh();
    }

    public void SetOnRefresh(Action<int, RectTransform> refresh)
    {
        onRefresh = refresh;
        //ForceRefresh();
    }

    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        scrollRect.onValueChanged.RemoveListener(OnScroll);
    }

    #endregion

    #region Init & Layout

    private void CalculateLayout()
    {
        if (isVertical)
        {
            float usableWidth =
                viewport.rect.width - padding.left - padding.right;

            constraintCount = Mathf.Max(1,
                Mathf.FloorToInt(
                    (usableWidth + spacing.x) /
                    (itemSize.x + spacing.x)));

            int visibleRows = Mathf.CeilToInt(
                (viewport.rect.height - padding.top - padding.bottom) /
                (itemSize.y + spacing.y));

            visibleCount = visibleRows * constraintCount;
        }
        else
        {
            float usableHeight =
                viewport.rect.height - padding.top - padding.bottom;

            constraintCount = Mathf.Max(1,
                Mathf.FloorToInt(
                    (usableHeight + spacing.y) /
                    (itemSize.y + spacing.y)));

            int visibleCols = Mathf.CeilToInt(
                (viewport.rect.width - padding.left - padding.right) /
                (itemSize.x + spacing.x));

            visibleCount = visibleCols * constraintCount;
        }

        poolCount = visibleCount + constraintCount * 2;
    }

    private void CreatePool()
    {
        for (int i = 0; i < poolCount; i++)
        {
            RectTransform item = UnityEngine.Object.Instantiate(template, content);
            item.gameObject.SetActive(true);
            items.Add(item);
        }
    }

    #endregion

    #region Scroll Logic

    private void OnScroll(Vector2 _)
    {
        int groupIndex = CalculateGroupIndex();
        if (groupIndex == lastGroupIndex)
            return;

        lastGroupIndex = groupIndex;
        startIndex = groupIndex * constraintCount;

        Refresh();
    }

    private int CalculateGroupIndex()
    {
        if (isVertical)
        {
            float y = content.anchoredPosition.y - padding.top;
            if (Mathf.Abs(y) < scrollThreshold) y = 0;

            return Mathf.Max(0,
                Mathf.FloorToInt(y / (itemSize.y + spacing.y)));
        }
        else
        {
            float x = -content.anchoredPosition.x - padding.left;
            if (Mathf.Abs(x) < scrollThreshold) x = 0;

            return Mathf.Max(0,
                Mathf.FloorToInt(x / (itemSize.x + spacing.x)));
        }
    }

    private void ForceRefresh()
    {
        lastGroupIndex = -1;
        OnScroll(Vector2.zero);
    }

    private void Refresh()
    {
        for (int i = 0; i < items.Count; i++)
        {
            int dataIndex = startIndex + i;
            RectTransform item = items[i];

            if (dataIndex < 0 || dataIndex >= totalCount)
            {
                item.gameObject.SetActive(false);
                continue;
            }

            item.gameObject.SetActive(true);
            SetItemPosition(item, dataIndex);
            onRefresh?.Invoke(dataIndex, item);
        }
    }

    #endregion

    #region Position & Content Size

    private void SetItemPosition(RectTransform item, int index)
    {
        int col, row;

        if (isVertical)
        {
            col = index % constraintCount;
            row = index / constraintCount;
        }
        else
        {
            row = index % constraintCount;
            col = index / constraintCount;
        }

        float x =
            padding.left +
            col * (itemSize.x + spacing.x);

        float y =
            -padding.top -
            row * (itemSize.y + spacing.y);

        item.anchoredPosition = new Vector2(x, y);
    }

    private void UpdateContentSize()
    {
        if (totalCount <= 0)
            return;

        if (isVertical)
        {
            int rows = Mathf.CeilToInt((float)totalCount / constraintCount);
            float height =
                padding.top +
                padding.bottom +
                rows * itemSize.y +
                (rows - 1) * spacing.y;

            content.sizeDelta = new Vector2(content.sizeDelta.x, height);
        }
        else
        {
            int cols = Mathf.CeilToInt((float)totalCount / constraintCount);
            float width =
                padding.left +
                padding.right +
                cols * itemSize.x +
                (cols - 1) * spacing.x;

            content.sizeDelta = new Vector2(width, content.sizeDelta.y);
        }
    }
    #endregion
}