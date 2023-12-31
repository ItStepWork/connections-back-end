﻿using backend.Models;
using backend.Models.Enums;
using Firebase.Database;
using Firebase.Database.Query;

namespace backend.Services
{
    public class NotificationService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
        public static async Task AddNotificationAsync(string senderId, string recipientId, NotificationType type)
        {
            Notification notification = new();
            notification.SenderId = senderId;
            notification.Type = type;
            notification.Id = Guid.NewGuid().ToString("N");
            notification.DateTime = DateTime.UtcNow;

            await firebaseDatabase.Child("Notifications").Child(recipientId).Child(notification.Id).PutAsync(notification);
        }
        public static async Task AddNotificationAsync(string senderId, string recipientId, Group group, NotificationType type)
        {
            Notification notification = new();
            notification.SenderId = senderId;
            notification.Type = type;
            notification.Id = Guid.NewGuid().ToString("N");
            notification.DateTime = DateTime.UtcNow;
            notification.Url = $"{General.SiteUrl}group/{group.Id}";
            notification.GroupName = group.Name;

            await firebaseDatabase.Child("Notifications").Child(recipientId).Child(notification.Id).PutAsync(notification);
        }
        public static async Task<Notification?> GetNotificationAsync(string userId, string notificationId)
        {
            var notification = await firebaseDatabase
              .Child("Notifications")
              .Child(userId)
              .Child(notificationId)
              .OnceSingleAsync<Notification>();
            return notification;
        }
        public static async Task<IEnumerable<NotificationResponse>?> GetNotificationsAsync(string userId)
        {
            var result = await firebaseDatabase
                .Child("Notifications")
                .Child(userId)
                .OnceAsync<Notification>();

            var users = await UserService.GetUsersAsync();
            var notifications = result.Select(n => n.Object).ToList();
            notifications.Sort((x, y) => DateTime.Compare(y.DateTime, x.DateTime));
            var response = notifications?.Select(x => new NotificationResponse() { Notification = x, User = users?.FirstOrDefault(user => user.Id == x.SenderId) }).ToList();

            var friends = await FriendService.GetFriendsAsync(userId);
            if (friends != null)
            {
                var dateTime = DateTime.UtcNow.AddYears(-150);
                var dateTimeNow = DateTime.UtcNow;
                var confirmed = friends.Where(friend => friend.IsConfirmed);
                var birthDays = confirmed.Where(x =>
                {
                    var user = users?.FirstOrDefault(user => user.Id == x.UserId);
                    if (user == null || user?.BirthDay < dateTime) return false;
                    else
                    {
                        if (user?.BirthDay.Day == dateTimeNow.Day && user.BirthDay.Month == dateTimeNow.Month) return true;
                        else return false;
                    }
                })
                .Select(x => new NotificationResponse()
                {
                    Notification = new() { DateTime = DateTime.UtcNow, Type = NotificationType.BirthDay }
                    , User = users?.FirstOrDefault(user => user.Id == x.UserId)
                });
                if (birthDays?.Count() > 0)
                {
                    if (response == null) response = new List<NotificationResponse>();
                    response.InsertRange(0, birthDays);
                }
            }
            return response;
        }
    }
}
