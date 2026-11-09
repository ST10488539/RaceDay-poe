using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Filters
{
    /// <summary>
    /// Blocks the request unless the user is logged in (session exists).
    /// Optionally restricts access to specific roles.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public SessionAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserID");
            var role = context.HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
            {
                context.Result = new UnauthorizedObjectResult(
                    new { message = "You must log in first." });
                return;
            }

            if (_roles.Length > 0 && !_roles.Contains(role))
            {
                context.Result = new ObjectResult(
                    new { message = "You do not have permission to access this resource." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}