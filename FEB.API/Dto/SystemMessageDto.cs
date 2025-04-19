using Newtonsoft.Json;

namespace FEB.API.Dto
{
    public class SystemMessageDto
    {
        public string Message { get; set; }
        public string UserID { get; set; } = string.Empty;
    }
}
