using FEB.Infrastructure.Dto;
using FEB.Infrastructure.Repositories.Abstract;
using FEB.Service.Abstract;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Service.Plugins
{
    public class OpenAIPlugin:IAIPlugin
    {
        private readonly IUserService _userService;

        public OpenAIPlugin(IUserService userService)
        {
            _userService = userService;
        }

        [KernelFunction]
        [Description("Returns the current date and time of the user.")]
        public DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }


        [KernelFunction]
        [Description("Returns the current user information Async.")]
        public async Task<UserDto?> GetCurrentUserInformation()
        {
            return await _userService.GetCurrentUser();
        }
        [KernelFunction]
        [Description("Creates an appointment for the current user. Automatically uses GetCurrentUserInformation to get the username. Does not require a date or time.")]
        public bool CreateAppointmentByUserName(string username)
        {
            var d = username.ToUpper();
            return true;
        }

        public async Task<UserDto?> GetUserInfoAsync(string username)
        {
            return await _userService.GetUserAsync(username);
        }
    }
}
