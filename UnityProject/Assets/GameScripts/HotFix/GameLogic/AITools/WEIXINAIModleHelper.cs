using Cysharp.Threading.Tasks;

using Newtonsoft.Json.Linq;
using System;
using System.Text;

using UnityEngine;
using UnityEngine.Networking;

namespace GameLogic
{
    public class WEIXINAIModleHelper : AIModleHelperBase
    {
        public WEIXINAIModleHelper()
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
            string apiKey = "sk-147zv4InTWevwJvIho5hg2wDKSN5nXDNMvDSht3V1c5SQDUd";//"tYyBAugMtLg3LTP8haLtGdrrhpy9Bzd4";
            string url = "https://api.lkeap.cloud.tencent.com/v1";

            JObject payload = new JObject
            {
                ["model"] = "deepseek-v3",
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
                DeepSeekMessageResponse messageResponse = JsonUtility.FromJson<DeepSeekMessageResponse>(response);
                action?.Invoke(messageResponse.choices[0].message.content);
                return messageResponse.choices[0].message.content;
            }
            else
            {
                Debug.LogError("DeepSeek Error: " + request.error + "\n" + request.downloadHandler.text);
                action?.Invoke("");
                return "";
            }
        }
    }

    [System.Serializable]
    public class DeepSeekMessageResponse
    {
        public DeepSeekChoices[] choices;
    }
    [System.Serializable]
    public class DeepSeekChoices
    {
        public DeepSeekMessage message;
    }
    [System.Serializable]
    public class DeepSeekMessage
    {
        public string content;
        public string role;
    }
}
