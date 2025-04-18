using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Service.Plugins
{
    public class OpenAIPlugin
    {
        [KernelFunction]
        [Description("Returns the current date and time of the user.")]
        public DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }


        [KernelFunction]
        [Description("Returns the current user information Async.")]
        public async Task<UserInfo> GetCurrentUserInformation()
        {
            return await this.GetUserInfoAsync() ;
        }
        [KernelFunction]
        [Description("Creates an appointment for the current user. Automatically uses GetCurrentUserInformation to get the username. Does not require a date or time.")]
        public bool CreateAppointmentByUserName(string username)
        {
            var d = username.ToUpper();
            return true;
        }
        public struct UserInfo
        {
            public string UserName { get; set; }
            public string UserEmail { get; set; }
            public string UserRole { get; set; }
        }

        public async Task<UserInfo> GetUserInfoAsync()
        {
            var info = new UserInfo
            {
                UserEmail = "Omerfaruk2695@gmail.com",
                UserName = "burack18",
                UserRole = "Admin"
            };
            return await Task.FromResult(info);
        }
    }
}
