using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ArtEvaluatorAPI.Models;

public class ImageProcessingRequest
{
    [Required(ErrorMessage = "Image file is required.")]
    public required IFormFile ImageFile { get; set; }

    [MaxLength(100, ErrorMessage = "Media type must be 100 characters or less.")]
    public string? MediaType { get; set; }

    [MaxLength(500, ErrorMessage = "Properties must be 500 characters or less.")]
    public string? Properties { get; set; }

    [MaxLength(50, ErrorMessage = "Dimensions must be 50 characters or less.")]
    public string? Dimensions { get; set; }

    [MaxLength(100, ErrorMessage = "Artist name must be 100 characters or less.")]
    public string? Artist { get; set; }

    [Range(0, 9999, ErrorMessage = "Year must be a valid positive number.")]
    public int? Year { get; set; }

    [MaxLength(200, ErrorMessage = "Title must be 200 characters or less.")]
    public string? Title { get; set; }

    [MaxLength(100, ErrorMessage = "Location must be 100 characters or less.")]
    public string? Location { get; set; }
}
