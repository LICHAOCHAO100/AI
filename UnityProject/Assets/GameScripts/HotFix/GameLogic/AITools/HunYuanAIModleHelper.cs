using Cysharp.Threading.Tasks;

using Newtonsoft.Json.Linq;
using System;
using System.Text;

using UnityEngine;
using UnityEngine.Networking;

namespace GameLogic
{
    public class HunYuanAIModleHelper : AIModleHelperBase
    {
        public HunYuanAIModleHelper()
        {
        }
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
            string apiKey = "sk-3zLbIX1F296Kq1KebcfgzyyfEGmZu2SNLd5o4amFOrmq5ujd";//"tYyBAugMtLg3LTP8haLtGdrrhpy9Bzd4";
            string url = "https://api.hunyuan.cloud.tencent.com/v1/chat/completions";

            JObject payload = new JObject
            {
                ["model"] = "hunyuan-lite",
                ["stream"] = false,
                ["messages"] = new JArray
                {
                new JObject
                    {
                        ["role"] = "user",
                        ["content"] = content
                    }
                }
            };

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(payload.ToString());
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest().ToUniTask();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                HunYuanMessageResponse messageResponse = JsonUtility.FromJson<HunYuanMessageResponse>(response);
                action?.Invoke(messageResponse.choices[0].message.content);
                return messageResponse.choices[0].message.content;
            }
            else
            {
                Debug.LogError("HunYuan Error: " + request.error + "\n" + request.downloadHandler.text);
                action?.Invoke("");
                return "";
            }
        }
    }

    [System.Serializable]
    public class HunYuanMessageResponse
    {
        public HunYuanChoices[] choices;
    }
    [System.Serializable]
    public class HunYuanChoices
    {
        public HunYuanMessage message;
    }
    [System.Serializable]
    public class HunYuanMessage
    {
        public string content;
        public string role;
    }
}
