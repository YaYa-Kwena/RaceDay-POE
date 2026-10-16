using Microsoft.AspNetCore.Http;

namespace RaceDay.API.Helpers
{
    public static class SessionAuthHelper
    {
        public const string SessionUserIdKey = "UserID";
        public const string SessionUserRoleKey = "UserRole";
        public const string SessionUserEmailKey = "UserEmail";

        public static void SetUserSession(ISession session, int userId, string role, string email)
        {
            session.SetInt32(SessionUserIdKey, userId);
            session.SetString(SessionUserRoleKey, role);
            session.SetString(SessionUserEmailKey, email);
        }

        public static int? GetUserId(ISession session)
        {
            return session.GetInt32(SessionUserIdKey);
        }

        public static string? GetUserRole(ISession session)
        {
            return session.GetString(SessionUserRoleKey);
        }

        public static bool IsAuthenticated(ISession session)
        {
            return GetUserId(session).HasValue && !string.IsNullOrEmpty(GetUserRole(session));
        }

        public static bool IsOrganiser(ISession session)
        {
            return IsAuthenticated(session) &&
                   string.Equals(GetUserRole(session), "Organiser", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsParticipant(ISession session)
        {
            return IsAuthenticated(session) &&
                   string.Equals(GetUserRole(session), "Participant", StringComparison.OrdinalIgnoreCase);
        }
    }
}