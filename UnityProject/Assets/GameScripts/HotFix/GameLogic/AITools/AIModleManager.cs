using Cysharp.Threading.Tasks;

using GameBase;

using System;
using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public class AIModleManager : Singleton<AIModleManager>
    {
        private IAIModleHelper m_AIModleHtlper;
        protected override void Init()
        {
            m_AIModleHtlper = new WEIXINAIModleHelper();
        }

        /// <summary>
        /// 获取会话ID
        /// </summary>
        /// <param name="botId"></param>
        /// <returns></returns>
        public async UniTask<string> GetConversationId(string botId)
        {
            if (m_AIModleHtlper == null)
            {
                throw new GameFrameworkException("AIModle helper is invalid.");
            }
            return await m_AIModleHtlper.GetConversationId(botId);
        }

        /// <summary>
        /// 给Ai发送消息
        /// </summary>
        /// <param name="content"></param>
        /// <param name="botId"></param>
        /// <param name="conversationid"></param>
        /// <returns></returns>
        private async UniTask<string> SendAIMessages(string content, Action<string> action = null)
        {
            if (m_AIModleHtlper == null)
            {
                throw new GameFrameworkException("AIModle helper is invalid.");
            }
            Debug.LogError("请求AI：" + content);
            return await m_AIModleHtlper.SendAIMessages(content, null, null, action);
        }

        /// <summary>
        /// 给Ai发送消息
        /// </summary>
        /// <param name="content"></param>
        /// <param name="botId"></param>
        /// <param name="conversationid"></param>
        /// <returns></returns>
        private async UniTask<string> SendAIMessages(string content, string botId, string conversationid, Action<string> action = null)
        {
            if (m_AIModleHtlper == null)
            {
                throw new GameFrameworkException("AIModle helper is invalid.");
            }
            return await m_AIModleHtlper.SendAIMessages(content, botId, conversationid, action);
        }

        /// <summary>
        /// 获取AI返回的Json数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <returns></returns>
        public async UniTask<T> GetAIMessageData<T>(string content)
        {
            string actionJson = await SendAIMessages(content);
            int lastIdx = actionJson.LastIndexOf("```");
            if (lastIdx > 0)
            {
                actionJson = actionJson.Substring(0, lastIdx);
                actionJson = actionJson.Replace("```json", "");
            }
            actionJson = RemoveWhite(actionJson);
            Debug.LogError("<color=#00FF00>收到AI回包：</color>" + actionJson);
            T data = Utility.Json.ToObject<T>(actionJson);
            return data;
        }

        /// <summary>
        /// 获取AI返回的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <returns></returns>
        public async UniTask<string> GetAIMessageStr(string content, Action<string> action = null)
        {
            string _action = await SendAIMessages(content, action);
            Debug.LogError("<color=#00FF00>收到AI回包：</color>" + _action);
            return _action;
        }
        public string RemoveWhiteSpace(string input)
        {
            // 移除字符串首尾的空白字符
            string trimmed = input.Trim();
            // 移除字符串中的所有空格（包括换行符）
            return trimmed.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        }

        public string RemoveWhite(string input)
        {
            // 移除字符串首尾的空白字符
            string trimmed = input.Trim();
            // 移除字符串中的所有空格（包括换行符）
            return trimmed.Replace("\n", "").Replace("\r", "");
        }
        public override void Release()
        {

        }
    }
}
