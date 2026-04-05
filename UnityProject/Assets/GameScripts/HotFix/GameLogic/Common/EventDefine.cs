using System;
using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// Game事件定义
    /// </summary>
    public static class GameEventDefine
    {
        public static readonly int GameStart = RuntimeId.ToRuntimeId("GameEventDefine.GameStart");
        public static readonly int GameOver = RuntimeId.ToRuntimeId("GameEventDefine.GameOver");

    }

    /// <summary>
    /// UI事件定义
    /// </summary>
    public static class UIEventDefine
    {
        public static readonly int Test = RuntimeId.ToRuntimeId("UIEventDefine.Test");
        public static readonly int EnterNextLevel = RuntimeId.ToRuntimeId("UIEventDefine.EnterNextLevel");//通知下一关
    }

    public class ConstData
    {
        public const string C_SaveLevel = "SaveLevel";
        public const string C_SaveGold = "SaveGold";
        public const string C_SavePower = "SavePower";
    }

    public class CellData
    {
        public int RegionId;
        public bool IsIsAnswer;
        public Vector2Int Pos;
    }

    [Serializable]
    public class QueenLevelData
    {
        public int grid;
        public string board;
    }
}