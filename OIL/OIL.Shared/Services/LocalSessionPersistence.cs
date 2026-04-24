using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using Blazored.LocalStorage; // Add Blazored.LocalStorage via NuGet

namespace OIL.Shared.Services
{
    // Add Blazored.LocalStorage via NuGet

    public class LocalSessionPersistence : IGotrueSessionPersistence<Session>
    {
        private readonly ILocalStorageService _storage;
        private const string SessionKey = "supabase_session";

        public LocalSessionPersistence(ILocalStorageService storage) => _storage = storage;

        public void SaveSession(Session session) =>
            _storage.SetItemAsync(SessionKey, session).AsTask().Wait();

        public void DestroySession() =>
            _storage.RemoveItemAsync(SessionKey).AsTask().Wait();

        public Session LoadSession() =>
            _storage.GetItemAsync<Session>(SessionKey).AsTask().Result;
    }
}


