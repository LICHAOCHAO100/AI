using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public interface IAIModleHelper 
    {
        /// <summary>
        /// 获取会话ID
        /// </summary>
        /// <returns></returns>
        UniTask<string> GetConversationId(string botId);
        /// <summary>
        /// 给AI发送消息
        /// </summary>
        /// <param name="content"></param>
        /// <param name="botId"></param>
        /// <param name="conversationid"></param>
        /// <returns></returns>
        UniTask<string> SendAIMessages(string content, string botId, string conversationid, Action<string> action);
    }
}
