// using System.Net.Http.Json;
// using Microsoft.Extensions.Configuration;
//
// namespace TechStoreEll.Core.Services;
//
// public interface IEmailService
// {
//     Task<bool> SendPasswordResetEmailAsync(string email, string resetLink);
// }
//
// public class EmailJsService(HttpClient httpClient, IConfiguration configuration) : IEmailService
// {
//     private const string ServiceId = "service_TechStoreEll";
//     private const string TemplateId = "template_a9ice16";
//     private const string PublicKey = "iy6f9MZAEZbnI3x2y";
//     
//     public async Task<bool> SendPasswordResetEmailAsync(string email, string resetLink)
//     {
//         try
//         {
//             var emailData = new
//             {
//                 service_id = ServiceId,
//                 template_id = TemplateId,
//                 user_id = PublicKey,
//                 template_params = new
//                 {
//                     to_email = email,
//                     reset_link = resetLink,
//                     subject = "Восстановление пароля - TechStoreEll",
//                     from_name = "TechStoreEll Support",
//                     to_name = email
//                 }
//             };
//
//             var response = await httpClient.PostAsJsonAsync(
//                 "https://api.emailjs.com/api/v1.0/email/send", 
//                 emailData);
//
//             return response.IsSuccessStatusCode;
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Ошибка отправки email: {ex.Message}");
//             return false;
//         }
//     }
// }