using System.ComponentModel.DataAnnotations;

namespace ArtEvaluatorAPI.Models;

public class PostRequest
{
    [Key]
    public int Id { get; set; }
    public string? AdditionalInfo { get; set; }
    public string? ImageFileHash { get; set; }
    public string? UserKey { get; set; }
    public string? APIResponse { get; set; }
}
