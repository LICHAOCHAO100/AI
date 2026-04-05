using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace GameLogic
{
    public class BaiLianAIModleHelper : AIModleHelperBase
    {
        /// <summary>
        /// 获取会话ID
        /// </summary>
        /// <returns></returns
        public override async UniTask<string> GetConversationId(string botId)
        {
            return "";
        }
        /// <summary>
        /// 给AI发送消息
        /// </summary>
        /// <param name="content"></param>
        /// <param name="botId"></param>
        /// <param name="conversationid"></param>
        /// <returns></returns>
        public override async UniTask<string> SendAIMessages(string content, string botId, string conversationid, Action<string> action)
        {
            string key = "sk-e031e0bff56a412282a78ba9fb07cb27";
            string Url = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions";
            JObject data = new JObject();
            data["model"] = "deepseek-v3";
            data["stream"] = false;
            JObject mess = new JObject();
            mess["role"] = "user";
            mess["content"] = content;
            JArray _mess = new JArray { mess };
            data["messages"] = _mess;
            UnityWebRequest chatRequest = new UnityWebRequest(Url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(data.ToString());
            chatRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            chatRequest.downloadHandler = new DownloadHandlerBuffer();
            chatRequest.SetRequestHeader("Authorization", "Bearer " + key);
            chatRequest.SetRequestHeader("Content-Type", "application/json");
            await chatRequest.SendWebRequest().ToUniTask();
            if (chatRequest.result == UnityWebRequest.Result.Success)
            {
                string response = chatRequest.downloadHandler.text;
                BaiLianMessageResponse messageResponse = JsonUtility.FromJson<BaiLianMessageResponse>(response);
                action?.Invoke(messageResponse.choices[0].message.content);
                return messageResponse.choices[0].message.content;
            }
            else
            {
                Debug.LogError("Error: " + chatRequest.error);
                action?.Invoke("");
                return "";
            }
        }
    }

    [System.Serializable]
    public class BaiLianMessageResponse
    {
        public BaiLianChoices[] choices;
    }
    [System.Serializable]
    public class BaiLianChoices 
    {
       public BaiLianMessage message;
    }
    [System.Serializable]
    public class BaiLianMessage
    {
        public string content;
        public string role;
    }
}
