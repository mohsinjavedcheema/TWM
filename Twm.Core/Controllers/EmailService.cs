using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using Twm.Core.Classes;

namespace Twm.Core.Controllers
{
    public class EmailService
    {
        
        private static EmailService _mInstance;

        public static EmailService Instance
        {
            get { return _mInstance ?? (_mInstance = new EmailService()); }
        }

        private SmtpClient _smtpClient;

        private SemaphoreSlim _emailSemaphore = new SemaphoreSlim(1,1);

        protected EmailService()
        {
            var host = SystemOptions.Instance.EmailHost;
            var port = SystemOptions.Instance.EmailPort;
            var username = SystemOptions.Instance.EmailUsername;
            var password = SystemOptions.Instance.EmailPassword;
            _smtpClient = new SmtpClient()
            {
                UseDefaultCredentials = false,
                Host = host,
                Port = port,
                Credentials = new NetworkCredential(
                    username,
                    password
                ),
                EnableSsl = true,
                TargetName = "STARTTLS/" + host
            };

        }

        public async Task SendEmail(string toAddress, string subject, string body)
        {
            await _emailSemaphore.WaitAsync(new CancellationToken());
            try
            {
                var username = SystemOptions.Instance.EmailUsername;
                var mailMsg = new MailMessage(username, toAddress, subject, body);
                await _smtpClient.SendMailAsync(mailMsg);
            }
            finally
            {
                _emailSemaphore.Release(1);
            }

        }

       
    }
}