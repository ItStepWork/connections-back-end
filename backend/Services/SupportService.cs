﻿using backend.Models;
using backend.Models.Enums;
using Firebase.Database;
using Firebase.Database.Query;

namespace backend.Services
{
    public static class SupportService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");

        public static async Task SendMessageAsync(string senderId, Request request)
        {
            Message message = new Message();
            message.Id = Guid.NewGuid().ToString("N");
            message.Text = request.Text;
            message.CreateTime = DateTime.UtcNow;
            message.SenderId = senderId;
            message.Status = MessageStatus.Unread;

            if (request.File != null)
            {
                string? link = await UserService.SaveFileAsync(request.File, "Support", message.Id);
                message.Link = link;
            }

            await firebaseDatabase
             .Child($"Support/Messages/{senderId}/{message.Id}")
             .PutAsync(message);
        }
        public static async Task<IEnumerable<Message>?> GetMessagesAsync(string userId)
        {
            var result = await firebaseDatabase.Child($"Support/Messages/{userId}")
                .OnceAsync<Message>();

            return result?.Select(x => x.Object);
        }
        public static async Task<IEnumerable<Dialog>?> GetDialogsAsync()
        {
            var dialogs = await firebaseDatabase.Child($"Support/Messages")
                .OnceAsync<IDictionary<string, Message>>();

            var users = await UserService.GetUsersAsync();
            var result = dialogs.Select(x => new Dialog() { User = users?.FirstOrDefault(u => u.Id == x.Key), LastMessage = x.Object.LastOrDefault().Value });
            return result;
        }
    }
}
