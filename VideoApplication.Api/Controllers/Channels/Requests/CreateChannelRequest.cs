using System.ComponentModel.DataAnnotations;

namespace VideoApplication.Api.Controllers.Channels.Requests;

public record CreateChannelRequest(
    [MaxLength(50)]
    [MinLength(5)]
    [RegularExpression( @"[a-zA-Z0-9][a-zA-Z0-9 \-]+[a-zA-Z0-9]")]
    string IdentifierName, 
    
    [MaxLength(100)]
    [MinLength(5)]
    [RegularExpression(@"\S.+\S")]
    string DisplayName, 
    
    string Description
);