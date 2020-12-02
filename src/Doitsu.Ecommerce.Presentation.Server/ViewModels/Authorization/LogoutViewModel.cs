using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Doitsu.Ecommerce.Presentation.Server.ViewModels.Authorization 
{
    public class LogoutViewModel
    {
        [BindNever]
        public string RequestId { get; set; }
    }
}