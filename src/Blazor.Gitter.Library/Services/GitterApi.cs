﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Blazor.Gitter.Library
{
    public class GitterApi : IChatApi
    {
        private const string APIBASE = "https://api.gitter.im/v1/";
        private const string APIUSERPATH = "user";
        private const string APIROOMS = "rooms";

        private string Token { get; set; }
        private HttpClient HttpClient { get; set; }
        public GitterApi(HttpClient httpClient = null)
        {
            HttpClient = httpClient ?? throw new Exception("Make sure you have added an HttpClient to your DI Container");
        }

        public void SetAccessToken(string token)
        {
            Token = token;
            PrepareHttpClient();
        }

        private void PrepareHttpClient()
        {
            if (!(HttpClient.BaseAddress is object))
            {
                HttpClient.BaseAddress = new Uri(APIBASE);
                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Token}");
            }
        }

        public async Task<IChatUser> GetCurrentUser()
        {
            return (await HttpClient.GetJsonAsync<GitterUser[]>(APIUSERPATH)).First();
        }

        public Task<IChatUser> GetChatUser(string UserId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IChatUser>> GetChatRoomUsers(string RoomId)
        {
            throw new NotImplementedException();
        }

        public async Task<IChatRoom> GetChatRoom(string UserId, string RoomId)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"id", RoomId}
            });

            return (await HttpClient.PostJsonAsync<GitterRoom[]>($"{APIUSERPATH}/{UserId}/{APIROOMS}", content)).First();
        }

        public async Task<IEnumerable<IChatRoom>> GetChatRooms(string UserId)
        {
            return await HttpClient.GetJsonAsync<GitterRoom[]>($"{APIUSERPATH}/{UserId}/{APIROOMS}");
        }

        public Task<IChatMessage> GetChatMessage(string RoomId, string MessageId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IChatMessage>> GetChatMessages(string RoomId, IChatMessageOptions Options)
        {
            return await HttpClient.GetJsonAsync<GitterMessage[]>($"{APIROOMS}/{RoomId}/chatMessages{Options}");
        }

        public async Task<IEnumerable<IChatMessage>> SearchChatMessages(string RoomId, IChatMessageOptions Options)
        {
            if (string.IsNullOrWhiteSpace(Options.Query))
            {
                return default;
            }
            return await GetChatMessages(RoomId, Options);
        }

        public async Task<IChatMessage> SendChatMessage(string RoomId, string Message)
        {
            var content = new NewMessage() { text = Message };

            var result = await HttpClient.PostJsonAsync<GitterMessage>($"{APIROOMS}/{RoomId}/chatMessages", content);

            return result;
        }

        public async Task<IChatMessage> EditChatMessage(string RoomId, string MessageId, string Message)
        {
            var content = new NewMessage() { text = Message };

            return (await HttpClient.PutJsonAsync<GitterMessage>($"{APIROOMS}/{RoomId}/chatMessages/{MessageId}", content));
        }

        public async Task<bool> MarkChatMessageAsRead(string UserId, string RoomId, string MessageId)
        {
            var content = new MarkUnread { chat = new string[] { MessageId } };
            try
            {
                var result = await HttpClient.PostJsonAsync<SimpleSuccess>($"{APIUSERPATH}/{UserId}/{APIROOMS}/{RoomId}/unreadItems", content);
                return result.success;
            }
            catch { }
            return false;
        }

        public IChatMessageOptions GetNewOptions()
        {
            return new GitterMessageOptions();
        }
    }
    public class NewMessage
    {
        public string text { get; set; }
    }
    public class MarkUnread
    {
        public string[] chat { get; set; }
    }

    public class SimpleSuccess
    {
        public bool success { get; set; }
    }
}
