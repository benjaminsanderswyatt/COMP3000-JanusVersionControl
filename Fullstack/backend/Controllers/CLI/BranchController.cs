using backend.DataTransferObjects;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers.CLI
{
    [Route("api/cli/[controller]")]
    [ApiController]
    [EnableCors("CLIPolicy")]
    [Authorize(Policy = "CLIPolicy")]
    public class BranchController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;

        public BranchController(JanusDbContext janusDbContext)
        {
            _janusDbContext = janusDbContext;
        }





    }
}
