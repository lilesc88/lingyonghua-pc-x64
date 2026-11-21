using System;
using System.Collections.Generic;
using WinFrmTalk;

/// <summary>
/// v23.12 (2026-09-22): avatar cache version.
///
/// Root cause it fixes: avatar URL is a fixed path (/res/avatar/t/&lt;n&gt;/&lt;userId&gt;.jpg),
/// so after a user changes the avatar the URL stays the same and the client keeps
/// serving the old image from memory cache / disk cache (list shows old avatar while
/// the detail page, which forces a refresh, shows the new one).
///
/// Solution: every avatar URL gets a "?v=&lt;token&gt;" suffix, so a changed avatar maps to a
/// new cache key and is re-downloaded.
///  - SessionToken changes on every app start =&gt; all avatars are re-fetched once per launch.
///  - Bump(userId) forces an immediate re-fetch for one user (avatar just changed).
/// </summary>
internal static class AvatarVersion
{
    // token of the current process (new value on every launch)
    private static readonly string SessionToken = TimeUtils.CurrentTimeMillis().ToString();

    private static readonly Dictionary<string, string> UserTokens = new Dictionary<string, string>();
    private static readonly object _lock = new object();

    /// <summary>
    /// current version token for one user
    /// </summary>
    internal static string Get(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return SessionToken;
        }

        lock (_lock)
        {
            string token;
            if (UserTokens.TryGetValue(userId, out token) && !string.IsNullOrEmpty(token))
            {
                return token;
            }
        }

        return SessionToken;
    }

    /// <summary>
    /// mark one user avatar as changed (next render will re-download it)
    /// </summary>
    internal static void Bump(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        lock (_lock)
        {
            UserTokens[userId] = TimeUtils.CurrentTimeMillis().ToString();
        }
    }

    /// <summary>
    /// append the version parameter to an avatar url
    /// </summary>
    internal static string Append(string url, string userId)
    {
        if (string.IsNullOrEmpty(url))
        {
            return url;
        }

        return url + (url.IndexOf('?') >= 0 ? "&" : "?") + "v=" + Get(userId);
    }
}
