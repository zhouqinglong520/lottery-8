﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;

using Colin.Lottery.Models;


namespace Colin.Lottery.WebApp.Hubs
{
    public abstract class BaseHub<T>
    : Hub
    where T : BaseHub<T>
    {
        /// <summary>
        /// 用户统计
        /// </summary>
        protected static int _usersCount = 0;

        /// <summary>
        /// 在线用户
        /// </summary>
        protected static ConcurrentDictionary<string, object> _users = new ConcurrentDictionary<string, object>();

        public async override Task OnConnectedAsync()
        {
            Interlocked.Increment(ref _usersCount);
            _users[Context.ConnectionId] = new UserData(Context.ConnectionId, $"user{_usersCount}");

            await Groups.AddAsync(Context.ConnectionId, typeof(T).Name);

            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            Interlocked.Decrement(ref _usersCount);
            _users.TryRemove(Context.ConnectionId, out object user);

            await Groups.RemoveAsync(Context.ConnectionId, typeof(T).Name);

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 向所有客户端推送广播数据
        /// </summary>
        /// <returns>The broadcast.</returns>
        /// <param name="data">Data.</param>
        public async Task Broadcast(object data)
        {
            await Clients.All.SendAsync("Broadcast", data);
        }
    }
}