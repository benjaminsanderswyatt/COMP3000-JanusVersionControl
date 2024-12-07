using backend.DataTransferObjects;
using backend.Helpers;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CLIPolicy")]
    [Authorize(Policy = "CLIPolicy")]
    public class CLIController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;
        private readonly CLIHelper _cliHelper;

        public CLIController(JanusDbContext janusDbContext, CLIHelper cliHelper)
        {
            _janusDbContext = janusDbContext;
            _cliHelper = cliHelper;
        }



        // POST: api/CLI/Push
        [HttpPost("Push")]
        public async Task<IActionResult> Push([FromForm] string commitJson)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // BranchId TODO
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId;
            if(!Int32.TryParse(userIdClaim, out userId))
            {
                return BadRequest(ModelState);
            }

            using (var transaction = await _janusDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    List<CommitDto> commitDtos = JsonConvert.DeserializeObject<List<CommitDto>>(commitJson);

                    foreach(var commitDto in commitDtos)
                    {
                        // var BranchId

                        var parentCommitId = await _cliHelper.GetParentCommitIdAsync(commitDto.ParentCommitHash);

                        var commit = new Commit
                        {
                            BranchId = 0,
                            UserId = userId,
                            CommitHash = commitDto.CommitHash,
                            Message = commitDto.Message,
                            ParentCommitId = parentCommitId,
                            CommittedAt = commitDto.CommittedAt,
                            Files = commitDto.Files.Select(fileDto => new Models.File
                            {
                                FilePath = fileDto.FilePath,
                                FileHash = fileDto.FileHash,
                                FileContents = new FileContent
                                {
                                    Content = fileDto.FileContent
                                }
                            }).ToList()
                        };

                        // Add to database
                        _janusDbContext.Commits.Add(commit);
                    }



                    await _janusDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { error = ex.Message });
                }
            }

            
        }


    }
}
