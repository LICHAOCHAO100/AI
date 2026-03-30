using System;
using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public static class CommonLogic
    {
        /// <summary>
        /// 世界坐标转屏幕坐标
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector2 WorldToScreenPoint(Vector3 position)
        {
            return RectTransformUtility.WorldToScreenPoint(Camera.main, position); 
        }

        /// <summary>
        /// 屏幕坐标转UI的position坐标
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        public static Vector3 ScreenPointToUiPoint(RectTransform rt, Vector2 screenPoint)
        {
            Vector3 uiPoint;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, screenPoint, GameModule.UI.UICamera, out uiPoint);
            return uiPoint;
        }

        /// <summary>
        /// 屏幕坐标转UI的anchoredPosition坐标
        /// </summary>
        /// <param name="parentRT"></param>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        public static Vector2 ScreenPointToUILocalPoint(RectTransform parentRT, Vector2 screenPoint)
        {
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, screenPoint, GameModule.UI.UICamera, out localPos);
            return localPos;
        }

        /// <summary>
        /// 获取归一化方向
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2Int GetBaseDirection(Vector2Int v)
        {
            if (v == Vector2Int.zero) return Vector2Int.zero;

            // 归一化到-1,0,1
            int x = v.x == 0 ? 0 : (v.x > 0 ? 1 : -1);
            int y = v.y == 0 ? 0 : (v.y > 0 ? 1 : -1);

            return new Vector2Int(x, y);
        }

        /// <summary>
        /// 获取一个画线的网格mesh
        /// </summary>
        /// <param name="gridSize"></param>
        /// <param name="cellSize"></param>
        /// <returns></returns>
        public static Mesh GetMeshGridLines(int gridSize, float cellSize)
        {
            var centerOffset = new Vector3(cellSize / 2, 0, cellSize / 2);
            // 计算总顶点数：
            // 横向线条：(gridHeight + 1) 条线，每条线2个顶点
            // 纵向线条：(gridWidth + 1) 条线，每条线2个顶点
            int horizontalLines = gridSize + 1;
            int verticalLines = gridSize + 1;
            int vertexCount = (horizontalLines + verticalLines) * 2;

            // 计算总索引数：每个线段由两个顶点构成，所以索引数等于顶点数
            int indexCount = vertexCount;

            // 创建顶点数组和索引数组
            Vector3[] vertices = new Vector3[vertexCount];
            int[] indices = new int[indexCount];
            Color[] colors = new Color[vertexCount]; // 用于顶点颜色

            // 计算偏移量使网格居中
            float totalWidth = gridSize * cellSize;
            float totalHeight = gridSize * cellSize;
            float offsetX = totalWidth * 0.5f;
            float offsetZ = totalHeight * 0.5f;

            int vertexIndex = 0;

            // ----- 生成横向线条的顶点 (沿X轴方向) -----
            for (int i = 1; i < gridSize; i++)
            {
                float zPos = i * cellSize - offsetZ;

                // 起点
                vertices[vertexIndex] = new Vector3(-offsetX, 0, zPos) + centerOffset;
                colors[vertexIndex] = Color.white;
                indices[vertexIndex] = vertexIndex;
                vertexIndex++;

                // 终点
                vertices[vertexIndex] = new Vector3(totalWidth - offsetX, 0, zPos) + centerOffset;
                colors[vertexIndex] = Color.white;
                indices[vertexIndex] = vertexIndex;
                vertexIndex++;
            }

            // ----- 生成纵向线条的顶点 (沿Z轴方向) -----
            for (int i = 1; i < gridSize; i++)
            {
                float xPos = i * cellSize - offsetX;

                // 起点
                vertices[vertexIndex] = new Vector3(xPos, 0, -offsetZ) + centerOffset;
                colors[vertexIndex] = Color.white;
                indices[vertexIndex] = vertexIndex;
                vertexIndex++;

                // 终点
                vertices[vertexIndex] = new Vector3(xPos, 0, totalHeight - offsetZ) + centerOffset;
                colors[vertexIndex] = Color.white;
                indices[vertexIndex] = vertexIndex;
                vertexIndex++;
            }

            // 创建Mesh并赋值
            Mesh mesh = new Mesh();
            mesh.name = $"Grid_{gridSize}x{gridSize}_Mesh";
            mesh.vertices = vertices;
            mesh.colors = colors;      // 设置顶点颜色，需要支持顶点颜色的shader
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
            return mesh;
        }
        public static Mesh GetMeshGridLines(int gridSize, float cellSize, Terrain terrain)
        {
            var centerOffset = new Vector3(cellSize / 2, 0, cellSize / 2);

            int horizontalLines = gridSize + 1;
            int verticalLines = gridSize + 1;
            int vertexCount = (horizontalLines + verticalLines) * 2;

            Vector3[] vertices = new Vector3[vertexCount];
            int[] indices = new int[vertexCount];
            Color[] colors = new Color[vertexCount];

            float totalWidth = gridSize * cellSize;
            float totalHeight = gridSize * cellSize;

            float offsetX = totalWidth * 0.5f;
            float offsetZ = totalHeight * 0.5f;

            int vertexIndex = 0;

            Vector3 terrainPos = terrain.transform.position;

            // 横线
            for (int i = 0; i <= gridSize; i++)
            {
                float zPos = i * cellSize - offsetZ;

                Vector3 start = new Vector3(-offsetX, 0, zPos) + centerOffset;
                Vector3 end = new Vector3(totalWidth - offsetX, 0, zPos) + centerOffset;

                start += terrainPos;
                end += terrainPos;

                start.y = terrain.SampleHeight(start) + 0.02f;
                end.y = terrain.SampleHeight(end) + 0.02f;

                vertices[vertexIndex] = start;
                colors[vertexIndex] = Color.white;
                indices[vertexIndex] = vertexIndex;
                vertexIndex++;

                vertices[vertexIndex] = end;
                colors[vertexIndex] = Color.white;
                indices[vertexIndex] = vertexIndex;
                vertexIndex++;
            }

            // 竖线
            for (int i = 0; i <= gridSize; i++)
            {
                float xPos = i * cellSize - offsetX;

                Vector3 start = new Vector3(xPos, 0, -offsetZ) + centerOffset;
                Vector3 end = new Vector3(xPos, 0, totalHeight - offsetZ) + centerOffset;

                start += terrainPos;
                end += terrainPos;

                start.y = terrain.SampleHeight(start) + 0.02f;
                end.y = terrain.SampleHeight(end) + 0.02f;

                vertices[vertexIndex] = start;
                colors[vertexIndex] = Color.white;
                indices[vertexIndex] = vertexIndex;
                vertexIndex++;

                vertices[vertexIndex] = end;
                colors[vertexIndex] = Color.white;
                indices[vertexIndex] = vertexIndex;
                vertexIndex++;
            }

            Mesh mesh = new Mesh();
            mesh.name = $"Grid_{gridSize}x{gridSize}_Mesh";

            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.SetIndices(indices, MeshTopology.Lines, 0);

            return mesh;
        }

        /// <summary>
        /// 显示建筑实际占用格子
        /// </summary>
        /// <param name="gridSizeX"></param>
        /// <param name="gridSizeZ"></param>
        /// <param name="cellSize"></param>
        /// <param name="mesh"></param>
        public static void GetMeshGridCell(int gridSizeX, int gridSizeZ, float cellSize, Mesh mesh)
        {
            //计算中心点 
            float centerX = (float)((int)(gridSizeX / 2) + cellSize / 2);
            float centerZ = (float)((int)(gridSizeZ / 2) + cellSize / 2);

            int totalCells = gridSizeX * gridSizeZ;
            int vertexCount = totalCells * 4;
            int triangleCount = totalCells * 2;
            mesh.Clear();
            Vector3[] vertices = new Vector3[vertexCount];
            int[] triangles = new int[triangleCount * 3];
            Vector2[] uv = new Vector2[vertexCount];
            Color[] colors = new Color[vertexCount];

            int vertexIndex = 0;
            int triangleIndex = 0;

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    // 计算格子位置（加上间隔偏移）
                    float xPos =  x * cellSize - centerX;
                    float zPos =  z * cellSize - centerZ;

                    // 创建单元格的4个顶点
                    vertices[vertexIndex] = new Vector3(xPos, 0.01f, zPos);
                    vertices[vertexIndex + 1] = new Vector3(xPos + cellSize, 0.01f, zPos);
                    vertices[vertexIndex + 2] = new Vector3(xPos, 0.01f, zPos + cellSize);
                    vertices[vertexIndex + 3] = new Vector3(xPos + cellSize, 0.01f, zPos + cellSize);
                    uv[vertexIndex] = new Vector2(0, 0);      // 左下
                    uv[vertexIndex + 1] = new Vector2(1, 0);  // 右下
                    uv[vertexIndex + 2] = new Vector2(0, 1);  // 左上
                    uv[vertexIndex + 3] = new Vector2(1, 1);  // 右上

                    // 设置默认颜色（蓝色半透明）
                    Color defaultColor = new Color(0, 0, 1, 0.5f);
                    colors[vertexIndex] = defaultColor;
                    colors[vertexIndex + 1] = defaultColor;
                    colors[vertexIndex + 2] = defaultColor;
                    colors[vertexIndex + 3] = defaultColor;
                    // 创建两个三角形
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + 2;
                    triangles[triangleIndex + 2] = vertexIndex + 1;
                    triangles[triangleIndex + 3] = vertexIndex + 2;
                    triangles[triangleIndex + 4] = vertexIndex + 3;
                    triangles[triangleIndex + 5] = vertexIndex + 1;

                    vertexIndex += 4;
                    triangleIndex += 6;
                }
            }
            // 赋值给Mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.colors = colors;
            // 重新计算法线
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        /// <summary>
        /// 显示建筑区域范围
        /// </summary>
        /// <param name="radiusCount"></param>
        /// <param name="cellSize"></param>
        /// <param name="mesh"></param>
        public static void GetMeshGrid(int radiusCount, float cellSize, Mesh mesh)
        {
            //计算中心点
            float offset = radiusCount + cellSize / 2;
            //半径乘2 再加1 凑成奇数对齐
            int count = radiusCount * 2 + 1;
            int totalCells = count * count;
            int vertexCount = totalCells * 4;
            int triangleCount = totalCells * 2;
            mesh.Clear();
            Vector3[] vertices = new Vector3[vertexCount];
            int[] triangles = new int[triangleCount * 3];
            Vector2[] uv = new Vector2[vertexCount];
            // 实际格子大小
            float actualCellSize = cellSize - 0.1f;
            float gapOffset = 0.1f * 0.5f;

            int vertexIndex = 0;
            int triangleIndex = 0;

            for (int x = 0; x < count; x++)
            {
                for (int z = 0; z < count; z++)
                {
                    // 计算格子位置（加上间隔偏移）
                    float xPos = x * cellSize + gapOffset - offset;//- centerOffset.x;
                    float zPos = z * cellSize + gapOffset - offset; //- centerOffset.z;

                    // 创建单元格的4个顶点
                    vertices[vertexIndex] = new Vector3(xPos, 0.01f, zPos);
                    vertices[vertexIndex + 1] = new Vector3(xPos + actualCellSize, 0.01f, zPos);
                    vertices[vertexIndex + 2] = new Vector3(xPos, 0.01f, zPos + actualCellSize);
                    vertices[vertexIndex + 3] = new Vector3(xPos + actualCellSize, 0.01f, zPos + actualCellSize);
                    uv[vertexIndex] = new Vector2(0, 0);      // 左下
                    uv[vertexIndex + 1] = new Vector2(1, 0);  // 右下
                    uv[vertexIndex + 2] = new Vector2(0, 1);  // 左上
                    uv[vertexIndex + 3] = new Vector2(1, 1);  // 右上

                    // 创建两个三角形
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + 2;
                    triangles[triangleIndex + 2] = vertexIndex + 1;
                    triangles[triangleIndex + 3] = vertexIndex + 2;
                    triangles[triangleIndex + 4] = vertexIndex + 3;
                    triangles[triangleIndex + 5] = vertexIndex + 1;

                    vertexIndex += 4;
                    triangleIndex += 6;
                }
            }
            // 赋值给Mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            //mesh.colors = colors;
            // 重新计算法线
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        public static void GetMeshCircle(int radius, Mesh mesh)
        {
            mesh.Clear();
            int segments = 100;
            Vector3[] vertices = new Vector3[segments + 1];
            Vector2[] uv = new Vector2[segments + 1];  // 添加UV坐标，方便使用渐变色
            int[] triangles = new int[segments * 3];

            // 中心点
            vertices[0] = Vector3.zero;
            uv[0] = new Vector2(0.5f, 0.5f);  // 中心点的UV

            float angleStep = 360f / segments;

            for (int i = 1; i <= segments; i++)
            {
                float angle = (i - 1) * angleStep * Mathf.Deg2Rad;
                float x = Mathf.Sin(angle) * radius;
                float z = Mathf.Cos(angle) * radius;

                vertices[i] = new Vector3(x, 0, z);  // Y轴为0，平贴地面

                // 计算UV坐标，用于边缘渐变效果
                float u = 0.5f + (x / radius) * 0.5f;
                float v = 0.5f + (z / radius) * 0.5f;
                uv[i] = new Vector2(u, v);
            }

            // 生成三角形索引
            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;

                int nextIndex = (i + 2 > segments) ? 1 : i + 2;
                triangles[i * 3 + 2] = nextIndex;
            }

            mesh.vertices = vertices;
            mesh.uv = uv;  // 赋值UV
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        #region Unity UI Extension
        public static void SetAnchoredPositionX(this RectTransform rectTransform, float anchoredPositionX)
        {
            var value = rectTransform.anchoredPosition;
            value.x = anchoredPositionX;
            rectTransform.anchoredPosition = value;
        }
        public static void SetAnchoredPositionY(this RectTransform rectTransform, float anchoredPositionY)
        {
            var value = rectTransform.anchoredPosition;
            value.y = anchoredPositionY;
            rectTransform.anchoredPosition = value;
        }
        public static void SetAnchoredPosition3DZ(this RectTransform rectTransform, float anchoredPositionZ)
        {
            var value = rectTransform.anchoredPosition3D;
            value.z = anchoredPositionZ;
            rectTransform.anchoredPosition3D = value;
        }

        public static void SetSizeDeltaX(this RectTransform rectTransform, float sizeDeltaX)
        {
            var value = rectTransform.sizeDelta;
            value.x = sizeDeltaX;
            rectTransform.sizeDelta = value;
        }

        public static void SetSizeDeltaY(this RectTransform rectTransform, float sizeDeltaY)
        {
            var value = rectTransform.sizeDelta;
            value.y = sizeDeltaY;
            rectTransform.sizeDelta = value;
        }

        public static void SetColorAlpha(this UnityEngine.UI.Graphic graphic, float alpha)
        {
            var value = graphic.color;
            value.a = alpha;
            graphic.color = value;
        }
        public static void SetFlexibleSize(this LayoutElement layoutElement, Vector2 flexibleSize)
        {
            layoutElement.flexibleWidth = flexibleSize.x;
            layoutElement.flexibleHeight = flexibleSize.y;
        }
        public static Vector2 GetFlexibleSize(this LayoutElement layoutElement)
        {
            return new Vector2(layoutElement.flexibleWidth, layoutElement.flexibleHeight);
        }
        public static void SetMinSize(this LayoutElement layoutElement, Vector2 size)
        {
            layoutElement.minWidth = size.x;
            layoutElement.minHeight = size.y;
        }
        public static Vector2 GetMinSize(this LayoutElement layoutElement)
        {
            return new Vector2(layoutElement.minWidth, layoutElement.minHeight);
        }
        public static void SetPreferredSize(this LayoutElement layoutElement, Vector2 size)
        {
            layoutElement.preferredWidth = size.x;
            layoutElement.preferredHeight = size.y;
        }
        public static Vector2 GetPreferredSize(this LayoutElement layoutElement)
        {
            return new Vector2(layoutElement.preferredWidth, layoutElement.preferredHeight);
        }
        #endregion

        #region Transform

        /// <summary>
        /// 设置绝对位置的 x 坐标。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="newValue">x 坐标值。</param>
        public static void SetPositionX(this Transform transform, float newValue)
        {
            Vector3 v = transform.position;
            v.x = newValue;
            transform.position = v;
        }

        /// <summary>
        /// 设置绝对位置的 y 坐标。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="newValue">y 坐标值。</param>
        public static void SetPositionY(this Transform transform, float newValue)
        {
            Vector3 v = transform.position;
            v.y = newValue;
            transform.position = v;
        }

        /// <summary>
        /// 设置绝对位置的 z 坐标。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="newValue">z 坐标值。</param>
        public static void SetPositionZ(this Transform transform, float newValue)
        {
            Vector3 v = transform.position;
            v.z = newValue;
            transform.position = v;
        }

        /// <summary>
        /// 增加绝对位置的 x 坐标。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="deltaValue">x 坐标值增量。</param>
        public static void AddPositionX(this Transform transform, float deltaValue)
        {
            Vector3 v = transform.position;
            v.x += deltaValue;
            transform.position = v;
        }

        /// <summary>
        /// 增加绝对位置的 y 坐标。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="deltaValue">y 坐标值增量。</param>
        public static void AddPositionY(this Transform transform, float deltaValue)
        {
            Vector3 v = transform.position;
            v.y += deltaValue;
            transform.position = v;
        }

        /// <summary>
        /// 增加绝对位置的 z 坐标。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="deltaValue">z 坐标值增量。</param>
        public static void AddPositionZ(this Transform transform, float deltaValue)
        {
            Vector3 v = transform.position;
            v.z += deltaValue;
            transform.position = v;
        }

        /// <summary>
        /// 设置相对位置的 x 坐标。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="newValue">x 坐标值。</param>
        public static void SetLocalPositionX(this Transform transform, float newValue)
        {
            Vector3 v = transform.localPosition;
            v.x = newValue;
            transform.localPosition = v;
        }

        /// <summary>
        /// 设置相对位置的 y 坐标。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="newValue">y 坐标值。</param>
        public static void SetLocalPositionY(this Transform transform, float newValue)
        {
            Vector3 v = transform.localPosition;
            v.y = newValue;
            transform.localPosition = v;
        }

        /// <summary>
        /// 设置相对位置的 z 坐标。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="newValue">z 坐标值。</param>
        public static void SetLocalPositionZ(this Transform transform, float newValue)
        {
            Vector3 v = transform.localPosition;
            v.z = newValue;
            transform.localPosition = v;
        }

        /// <summary>
        /// 增加相对位置的 x 坐标。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="deltaValue">x 坐标值。</param>
        public static void AddLocalPositionX(this Transform transform, float deltaValue)
        {
            Vector3 v = transform.localPosition;
            v.x += deltaValue;
            transform.localPosition = v;
        }

        /// <summary>
        /// 增加相对位置的 y 坐标。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="deltaValue">y 坐标值。</param>
        public static void AddLocalPositionY(this Transform transform, float deltaValue)
        {
            Vector3 v = transform.localPosition;
            v.y += deltaValue;
            transform.localPosition = v;
        }

        /// <summary>
        /// 增加相对位置的 z 坐标。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="deltaValue">z 坐标值。</param>
        public static void AddLocalPositionZ(this Transform transform, float deltaValue)
        {
            Vector3 v = transform.localPosition;
            v.z += deltaValue;
            transform.localPosition = v;
        }

        /// <summary>
        /// 设置相对尺寸的 x 分量。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="newValue">x 分量值。</param>
        public static void SetLocalScaleX(this Transform transform, float newValue)
        {
            Vector3 v = transform.localScale;
            v.x = newValue;
            transform.localScale = v;
        }

        /// <summary>
        /// 设置相对尺寸的 y 分量。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="newValue">y 分量值。</param>
        public static void SetLocalScaleY(this Transform transform, float newValue)
        {
            Vector3 v = transform.localScale;
            v.y = newValue;
            transform.localScale = v;
        }

        /// <summary>
        /// 设置相对尺寸的 z 分量。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="newValue">z 分量值。</param>
        public static void SetLocalScaleZ(this Transform transform, float newValue)
        {
            Vector3 v = transform.localScale;
            v.z = newValue;
            transform.localScale = v;
        }

        /// <summary>
        /// 增加相对尺寸的 x 分量。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="deltaValue">x 分量增量。</param>
        public static void AddLocalScaleX(this Transform transform, float deltaValue)
        {
            Vector3 v = transform.localScale;
            v.x += deltaValue;
            transform.localScale = v;
        }

        /// <summary>
        /// 增加相对尺寸的 y 分量。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="deltaValue">y 分量增量。</param>
        public static void AddLocalScaleY(this Transform transform, float deltaValue)
        {
            Vector3 v = transform.localScale;
            v.y += deltaValue;
            transform.localScale = v;
        }

        /// <summary>
        /// 增加相对尺寸的 z 分量。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="deltaValue">z 分量增量。</param>
        public static void AddLocalScaleZ(this Transform transform, float deltaValue)
        {
            Vector3 v = transform.localScale;
            v.z += deltaValue;
            transform.localScale = v;
        }
        #endregion Transform
    }
}
