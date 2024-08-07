using ArtEvaluatorAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ArtEvaluatorAPI.Controllers;
public interface IChatController
{
    Task<IActionResult> ProcessImage([FromForm] ImageProcessingRequest request);
}