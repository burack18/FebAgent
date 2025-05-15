using FEB.Infrastructure.Dto;
using FEB.Infrastructure.Repositories.Abstract;
using FEB.Service.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xceed.Words.NET;

namespace FEB.Service.Plugins
{
    public class OpenAIPlugin : IAIPlugin
    {
        private readonly IUserService _userService;
        private readonly Lazy<IDocumentService> _documentService;

        public OpenAIPlugin(IUserService userService, Lazy<IDocumentService> documentService)
        {
            _userService = userService;
            _documentService = documentService;
        }
        //public OpenAIPlugin(IUserService userService)
        //{
        //    _userService = userService;

        //}

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


        [KernelFunction]
        [Description("Creates a document for the current user Use GetCurrentUserInformation to get UserInformation. 'fileContent' is the content of the document, ask from the user filename")]
        public async Task<bool> CreateDocument(string userID, string fileContent,string fileName)
        {
            var file = CreateDocxFormFile(fileContent, fileName);
            await _documentService.Value.SaveDocument(userID, file);
            return true;
        }

        public async Task<UserDto?> GetUserInfoAsync(string username)
        {
            return await _userService.GetUserAsync(username);
        }
        public IFormFile CreateDocxFormFile(string content, string fileName)
        {
            var ms = new MemoryStream(); // ✅ no 'using' here

            // Create DOCX document in memory using DocX
            using (var doc = DocX.Create(ms))
            {
                doc.InsertParagraph(content);
                doc.Save(); // Save to memory stream
            }

            ms.Position = 0; // Reset stream position for reading

            // Create IFormFile from memory stream
            var formFile = new FormFile(ms, 0, ms.Length, fileName, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            };

            return formFile;
        }
    }
}
