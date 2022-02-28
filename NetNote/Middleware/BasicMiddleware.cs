using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetNote.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class BasicMiddleware
    {
        private readonly RequestDelegate _next;

        private BasicUser _user;
        public BasicMiddleware(RequestDelegate next,BasicUser user)
        {
            _next = next;
            _user = user;
        }

        public Task Invoke(HttpContext httpContext)
        {
            var request = httpContext.Request;
            string auth = request.Headers["Authorization"];
            if (string.IsNullOrEmpty(auth))
                return BasicResult(httpContext);

            string[] authParts = auth.Split(' ');
            if (authParts.Length != 2)
                return BasicResult(httpContext);
            string base64 = authParts[1];
            string authValue;
            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                authValue = Encoding.ASCII.GetString(bytes);
            }
            catch
            {
                authValue = null;
            }
            if(string.IsNullOrEmpty(authValue))
                return BasicResult(httpContext);

            
            string[] strs = authValue.Split(new string[] { ":"},StringSplitOptions.RemoveEmptyEntries);
            if(strs.Length!=2)
                return BasicResult(httpContext);
            string userName = strs[0];
            string password = strs[1];
            if(_user.UserName.Equals(userName) && _user.Password.Equals(password))
                return _next(httpContext);
            else
                return BasicResult(httpContext);
        }

        /// <summary>
        /// 返回需Basic认证输出
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        private static Task BasicResult(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = 401;
            httpContext.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"localhost\"");
            return Task.FromResult(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class BasicMiddlewareExtensions
    {
        public static IApplicationBuilder UseBasicMiddleware(this IApplicationBuilder builder, BasicUser user)
        {
            if (user == null)
                throw new ArgumentException("需设置Basic用户");
            return builder.UseMiddleware<BasicMiddleware>(user);
        }
    }
    public class BasicUser
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
