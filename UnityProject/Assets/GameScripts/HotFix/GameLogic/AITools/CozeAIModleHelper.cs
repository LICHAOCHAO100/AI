using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TEngine;
using UnityEngine;
using UnityEngine.Networking;

namespace GameLogic
{
    public class CozeAIModleHelper : AIModleHelperBase
    {
        string apiToken = "pat_LAzdKVPWPcNyBcE70L9xpIN0pBq9niyiGZjABPPqNRp1XDlz1xZe4Qbs2ZhFCdLL";
        public override async UniTask<string> GetConversationId(string botId)
        {
            string chatUrl = "https://api.coze.cn/v1/conversations";
            chatUrl += $"?bot_id={botId}&";
            UnityWebRequest chatRequest = UnityWebRequest.Get(chatUrl);
            chatRequest.SetRequestHeader("Authorization", "Bearer " + apiToken);
            chatRequest.SetRequestHeader("Content-Type", "application/json");
            await chatRequest.SendWebRequest().ToUniTask();
            if (chatRequest.result == UnityWebRequest.Result.Success)
            {
                string response = chatRequest.downloadHandler.text;
                var parameters = JObject.Parse(response);
                var code = (int)parameters["code"];
                if (code != 0) return "";
                var _data = (JObject)parameters["data"];
                var conversations = (JArray)_data["conversations"];
                if (conversations.Count > 0)
                {
                    return conversations[0]["id"].ToString();
                }
                else
                {
                    return await PostCreateConversation(botId);
                }
            }
            else
            {
                Debug.LogError("Error: " + chatRequest.error);
                return "";
            }
        }


        /// <summary>
        /// 创建会话
        /// </summary>
        /// <param name="botId"></param>
        /// <returns></returns>
        async UniTask<string> PostCreateConversation(string botId)
        {
            string chatUrl = "https://api.coze.cn/v1/conversation/create";
            JObject data = new JObject();
            data["bot_id"] = botId;
            UnityWebRequest chatRequest = new UnityWebRequest(chatUrl, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(data.ToString());
            chatRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            chatRequest.downloadHandler = new DownloadHandlerBuffer();
            chatRequest.SetRequestHeader("Authorization", "Bearer " + apiToken);
            chatRequest.SetRequestHeader("Content-Type", "application/json");
            await chatRequest.SendWebRequest().ToUniTask();
            if (chatRequest.result == UnityWebRequest.Result.Success)
            {
                string response = chatRequest.downloadHandler.text;
                var parameters = JObject.Parse(response);
                var code = (int)parameters["code"];
                if (code != 0) return "";
                var _data = (JObject)parameters["data"];
                return _data["id"].ToString();
            }
            else
            {
                return "";
            }
        }


        public override async UniTask<string> SendAIMessages(string content, string botId, string conversationid, Action<string> action)
        {
            Log.Info("创建对话:" + content);
            Log.Info("conversationId:" + conversationid);
            // Step 1: 发起对话请求
            string chatUrl = "https://api.coze.cn/v3/chat";
            chatUrl += $"?conversation_id={conversationid}&";
            JObject data = new JObject();
            data["bot_id"] = botId;
            data["user_id"] = "123456789";
            data["stream"] = false;
            data["auto_save_history"] = true;
            JObject mess = new JObject();
            mess["role"] = "user";
            mess["content"] = content;
            mess["content_type"] = "text";
            JArray _mess = new JArray { mess };
            data["additional_messages"] = _mess;
            UnityWebRequest chatRequest = new UnityWebRequest(chatUrl, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(data.ToString());
            chatRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            chatRequest.downloadHandler = new DownloadHandlerBuffer();
            chatRequest.SetRequestHeader("Authorization", "Bearer " + apiToken);
            chatRequest.SetRequestHeader("Content-Type", "application/json");

            await chatRequest.SendWebRequest().ToUniTask();
            Log.Info("创建对话完成");
            if (chatRequest.result == UnityWebRequest.Result.Success)
            {
                string response = chatRequest.downloadHandler.text;
                //Debug.LogError(response);
                var dataResponse = JsonUtility.FromJson<ChatDataResponse>(response);
                if (dataResponse.code == 0)
                {
                    // 继续执行后续代码
                    await CheckChatStatus(dataResponse.data.id, conversationid);
                    return await GetChatMessages(dataResponse.data.id, conversationid, action);
                }
                else
                {
                    Log.Error("错误码:" + dataResponse.code + "聊天内容：" + content + botId + "重新发送。");
                    if (dataResponse.code == 4016)
                    {
                        await UniTask.Delay(2000);
                        return await SendAIMessages(content, botId, conversationid, action);
                    }
                    action?.Invoke("");
                    return "";
                }
            }
            else
            {
                Debug.LogError("网络问题：" + chatRequest.result);
                return await SendAIMessages(content, botId, conversationid, action);
                //Debug.LogError("Error: botId:" + botId +":"+ chatRequest.error);
                //return "";
            }
        }

        /// <summary>
        /// 查看对话详情
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="conversationId"></param>
        /// <returns></returns>
         async UniTask CheckChatStatus(string chatId, string conversationId)
        {
            bool isCompleted = false;
            int time = 0;
            int timeOut = 999;
            while (!isCompleted && time < timeOut)
            {
                string statusUrl = $"https://api.coze.cn/v3/chat/retrieve?chat_id={chatId}&conversation_id={conversationId}";
                UnityWebRequest statusRequest = UnityWebRequest.Get(statusUrl);
                statusRequest.SetRequestHeader("Authorization", "Bearer " + apiToken);
                statusRequest.SetRequestHeader("Content-Type", "application/json");
                await statusRequest.SendWebRequest();
                if (statusRequest.result == UnityWebRequest.Result.Success)
                {
                    string response = statusRequest.downloadHandler.text;
                    ChatResponse statusResponse = JsonUtility.FromJson<ChatResponse>(response);

                    // 检查status是否为completed
                    if (statusResponse.data.status == "completed")
                    {
                        isCompleted = true;
                        //Debug.Log("Chat completed!");
                    }
                    else
                    {
                        //Debug.Log("Chat still in progress...");
                    }
                }
                else
                {
                    Debug.LogError("Status check failed: " + statusRequest.error);
                }
                await UniTask.Delay(1000);
                time++;
            }
        }

        /// <summary>
        /// 查看对话消息
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="conversationId"></param>
        /// <returns></returns>
         async UniTask<string> GetChatMessages(string chatId, string conversationId, Action<string> action)
        {
            string messageUrl = $"https://api.coze.cn/v3/chat/message/list?chat_id={chatId}&conversation_id={conversationId}";
            UnityWebRequest messageRequest = UnityWebRequest.Get(messageUrl);
            messageRequest.SetRequestHeader("Authorization", "Bearer " + apiToken);
            messageRequest.SetRequestHeader("Content-Type", "application/json");

            await messageRequest.SendWebRequest();

            if (messageRequest.result == UnityWebRequest.Result.Success)
            {
                string response = messageRequest.downloadHandler.text;

                MessageResponse messageResponse = JsonUtility.FromJson<MessageResponse>(response);

                // 查找类型为"answer"的消息
                string content = "";
                foreach (MessageData message in messageResponse.data)
                {
                    if (message.type == "answer")
                    {
                        content = message.content;
                        break;
                    }
                }
                //Debug.LogError(content);
                action?.Invoke(content);
                return content;
            }
            else
            {
                Debug.LogError("Failed to get messages: " + messageRequest.error);
                action?.Invoke("");
                return "";
            }
        }
    }

    [System.Serializable]
    public class MessageResponse
    {
        public int code;
        public MessageData[] data;
        public string msg;
    }

    [System.Serializable]
    public class MessageData
    {
        public string bot_id;
        public string content;
        public string content_type;
        public string conversation_id;
        public string id;
        public string role;
        public string type;
        public string created_at;
    }

    [System.Serializable]
    public class ChatResponse
    {
        public ChatData data;
        public int code;
        public string msg;
    }

    [System.Serializable]
    public class ChatData
    {
        public string id;
        public string conversation_id;
        public string bot_id;
        public long created_at;
        public long completed_at;
        public string last_error;
        public object meta_data;
        public string status;
        public UsageData usage;
    }

    [System.Serializable]
    public class UsageData
    {
        public int token_count;
        public int output_count;
        public int input_count;
    }

    [System.Serializable]
    public class SSEChatResponse
    {
        public string id;
        public string conversation_id;
        public string bot_id;
        public long created_at;
        public LastError last_error;
        public string status;
        //public UsageData usage;
        //public string section_id;
    }

    [System.Serializable]
    public class LastError
    {
        public int code;
        public string msg;
    }

    [System.Serializable]
    public class ChatDataResponse
    {
        public SSEChatResponse data;
        public int code;
        public string msg;
    }
}
