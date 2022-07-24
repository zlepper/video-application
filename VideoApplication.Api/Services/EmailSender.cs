using VideoApplication.Api.Models;

namespace VideoApplication.Api.Services;

public class EmailSender
{
    private SmtpSettings _smtpSettings;
    private ILogger<EmailSender> _logger;

    public EmailSender(SmtpSettings smtpSettings, ILogger<EmailSender> logger)
    {
        _smtpSettings = smtpSettings;
        _logger = logger;
    }
    
    
}