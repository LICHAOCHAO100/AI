using System;
using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public interface LGroupTool
    {
        void Init(string name, Component temp);
        void HandTool(int index, Component obj);
    }

    public class LGroup<T> : IEnumerable<T> where T : Component
    {
        private struct GroupConfig
        {
            public LGroupTool toolData;
            public T tempT;
            public int frameNum;
            public string resName;
            public Transform parent;
        }
        private List<T> mAllGroupObj = new List<T>();
        private Action<int, T> mCallEvent;//所有物体
        private int mMaxNum = 0;//当前克隆显示的物体
        private Dictionary<string, int> mAlias = new Dictionary<string, int>();
        private GroupConfig mGroupConfig = new GroupConfig();
        public LGroup()
        {

        }
        public T Last { get { return mAllGroupObj[mMaxNum - 1]; } }
        public int Count { get { return mMaxNum; } }
        public List<T> Values { get { return mAllGroupObj.GetRange(0, Count); } }
        public T this[int index] { get { return mAllGroupObj[index]; } }
        public T this[string key] { get { return GetValueByAlias(key); } }
        public T GetValue(int id) { return mMaxNum > id ? mAllGroupObj[id] : null; }
        public void Replace(int index)
        {
            if (index < Count && mCallEvent != null)
            {
                mCallEvent(index, mAllGroupObj[index]);
            }
        }
        public void Replace(int index, int num)
        {
            for (int i = 0; i < num; i++)
            {
                Replace(index + i);
            }
        }
        public void ReplaceAll()
        {
            Replace(0, mMaxNum);
        }
        public void SetAlias(int index, string name)
        {
            mAlias[name] = index;
        }
        public int GetIndexByAlias(string name)
        {
            int index = 0;
            if (mAlias.TryGetValue(name, out index))
            {
                return index;
            }
            return -1;
        }
        public T GetValueByAlias(string name)
        {
            int index = GetIndexByAlias(name);
            return index == -1 ? null : GetValue(index);
        }
        public void ReplaceByAlias(string name)
        {
            int index = GetIndexByAlias(name);
            if (index != -1)
            {
                Replace(index);
            }
        }
        public void SetAction(Action<int, T> call)
        {
            mCallEvent = call;
        }
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return mAllGroupObj[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public void Init(T obj, Action<int, T> call, int num = 0)
        {
            _Init("", obj, null, call, null, num);
        }
        public void Init(T obj, LGroupTool tool, Action<int, T> call = null, int num = 0)
        {
            _Init("", obj, null, call, tool, num);
        }
        public void Init(string resName, GameObject parent, Action<int, T> call, int num = 0)
        {
            _Init(resName, null, parent.transform, call, null, num);
        }
        public void Init(string resName, GameObject parent, Action<int, T> call = null, LGroupTool tool = null, int num = 0)
        {
            _Init(resName, null, parent.transform, call, tool, num);
        }
        public void Init(GameObject parent, Action<int, T> call, bool isOnlyChild = true)
        {
            if (isOnlyChild)
            {
                int count = parent.transform.childCount;
                for (int i = 0; i < count; i++)
                {
                    T t = parent.transform.GetChild(i).GetComponent<T>();
                    if (t != null)
                    {
                        mAllGroupObj.Add(t);
                    }
                }
            }
            else
            {
                T[] temps = parent.GetComponentsInChildren<T>(true);
                if (temps.Length > 0)
                {
                    if (temps[0].gameObject == parent)
                    {
                        for (int i = 1; i < temps.Length; i++)
                        {
                            mAllGroupObj.Add(temps[i]);
                        }
                    }
                    else
                    {
                        mAllGroupObj.AddRange(temps);
                    }
                }
            }
            mMaxNum = mAllGroupObj.Count;
            mCallEvent = call;
            if (mCallEvent != null)
            {
                for (int i = 0; i < mMaxNum; i++)
                {
                    mCallEvent(i, mAllGroupObj[i]);
                }
            }
        }
        private void _Init(string resName, T obj, Transform parent, Action<int, T> call, LGroupTool tool, int num)
        {
            mGroupConfig.tempT = obj;
            mGroupConfig.toolData = tool;
            mGroupConfig.resName = resName;
            mGroupConfig.parent = parent;
            mCallEvent = call;
            if (obj)
            {
                obj.gameObject.SetActive(false);
            }
            if (tool != null)
            {
                tool.Init(resName, obj);
            }
            if (num != 0)
            {
                Refurbish(num);
            }
        }
        public void Refurbish(int count, Action<int, T> callBack = null)
        {
            mMaxNum = count;
            if (callBack != null)
            {
                mCallEvent = callBack;
            }
            CloneObjectList(mAllGroupObj, mMaxNum, mCallEvent, mGroupConfig);
        }
        private void CloneObjectList(List<T> list, int count, Action<int, T> callBack, GroupConfig groupConfig)
        {
            for (int i = 0; i < count; i++)
            {
                CloneOneObject(i, list, callBack, groupConfig);
            }
            for (int i = count; i < list.Count; i++)
            {
                list[i].gameObject.SetActive(false);
            }
        }
        private T CloneOneObject(int i, List<T> list, Action<int, T> callBack, GroupConfig groupConfig)
        {
            T t = null;
            if (i < list.Count)
            {
                t = list[i];
            }
            else
            {
                if (groupConfig.tempT == null)
                {
                    t = GameModule.Resource.LoadAsset<T>(groupConfig.resName);
                    t.transform.SetParent(groupConfig.parent);
                }
                else
                {
                    t = GameObject.Instantiate(groupConfig.tempT.gameObject).GetComponent<T>();
                    t.transform.SetParent(groupConfig.tempT.transform.parent);
                    t.transform.rotation = Quaternion.identity;
                    t.transform.localPosition = Vector3.zero;
                    t.transform.localScale = groupConfig.tempT.transform.localScale;
                }
                list.Add(t);
            }
            t.gameObject.SetActive(true);
            if (groupConfig.toolData != null)
            {
                groupConfig.toolData.HandTool(i, t);
            }
            if (callBack != null)
            {
                callBack(i, t);
            }
            return t;
        }
        public void Add(int count = 1, int startIndex = -1)
        {
            int lastNum = mMaxNum;
            mMaxNum += count;
            if (startIndex == -1)
            {
                startIndex = lastNum;
                for (int i = 0; i < mMaxNum; i++)
                {
                    CloneOneObject(i, mAllGroupObj, null, mGroupConfig);
                }
            }
            else
            {
                LGroupTool tool = mGroupConfig.toolData;
                mGroupConfig.toolData = null;
                CloneObjectList(mAllGroupObj, mMaxNum, null, mGroupConfig);
                mGroupConfig.toolData = tool;
                for (int i = 0; i < count; i++)
                {
                    T tempObject = mAllGroupObj[mMaxNum - 1];
                    mAllGroupObj.Remove(tempObject);
                    mAllGroupObj.Insert(startIndex + i, tempObject);
                    tempObject.transform.SetSiblingIndex(startIndex + i - 1);
                }
                CloneObjectList(mAllGroupObj, mMaxNum, null, mGroupConfig);
                Replace(startIndex, count);
            }
        }
        public void Remove(int count = 1, int startIndex = -1, bool isReallyClear = false)
        {
            if (startIndex == -1)
            {
                if (isReallyClear)
                {
                    for (int i = mMaxNum - 1; i > mMaxNum - count; i--)
                    {
                        T obj = mAllGroupObj[i];
                        mAllGroupObj.Remove(obj);
                        GameObject.Destroy(obj.gameObject);
                    }
                }
                else
                {
                    for (int i = mMaxNum - count; i < mMaxNum; i++)
                    {
                        mAllGroupObj[i].gameObject.SetActive(false);
                    }
                }
                mMaxNum -= count;
            }
            else
            {
                if (isReallyClear)
                {
                    for (int i = startIndex + count - 1; i >= startIndex; i--)
                    {
                        T tempObjcet = mAllGroupObj[i];
                        mAllGroupObj.Remove(tempObjcet);
                        GameObject.Destroy(tempObjcet.gameObject);
                    }
                }
                else
                {
                    for (int i = startIndex + count - 1; i > startIndex; i--)
                    {
                        T tempObject = mAllGroupObj[i];
                        mAllGroupObj.Remove(tempObject);
                        mAllGroupObj.Add(tempObject);
                        tempObject.transform.SetAsLastSibling();
                    }
                }
                mMaxNum -= count;
                CloneObjectList(mAllGroupObj, mMaxNum, null, mGroupConfig);
            }
        }
    }
    #region 扩展接口
    public class GroupPositionHandle : LGroupTool
    {
        private Vector3 mStartPos;
        private float mOffsetPosX;
        private float mOffsetPosY;
        private int mCountX = -1;
        public static GroupPositionHandle Creat(float x, float y = 0, int xCount = -1)
        {
            GroupPositionHandle gp = new GroupPositionHandle();
            gp.SetX(x).SetY(y).SetCount(xCount);
            return gp;
        }
        public GroupPositionHandle SetX(float x)
        {
            mOffsetPosX = x;
            return this;
        }
        public GroupPositionHandle SetY(float y)
        {
            mOffsetPosY = y;
            return this;
        }
        public GroupPositionHandle SetCount(int count)
        {
            mCountX = count;
            return this;
        }
        public GroupPositionHandle SetStartPosition(Vector3 startPos)
        {
            mStartPos = startPos;
            return this;
        }
        public void HandTool(int index, Component cloneObj)
        {
            if (mCountX <= 0)//表示只有一行
            {
                Vector3 pos = mStartPos;
                pos.x += index * mOffsetPosX;
                cloneObj.transform.localPosition = pos;
            }
            else
            {
                int posX = index % mCountX;
                int posY = index / mCountX;
                Vector3 pos = mStartPos;
                pos.x += posX * mOffsetPosX;
                pos.y += posY * mOffsetPosY;
                cloneObj.transform.localPosition = pos;
            }
        }

        public void Init(string name, Component matrix)
        {
            if (matrix)
            {
                mStartPos = matrix.transform.localPosition;
            }
            else
            {
                mStartPos = Vector3.zero;
            }
        }
    }
    #endregion
}
