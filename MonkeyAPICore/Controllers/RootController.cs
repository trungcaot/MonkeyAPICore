using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyAPICore.Controllers
{
    [Route("/")]
    [ApiVersion("1.0")]
    public class RootController : Controller
    {
        [HttpGet(Name = nameof(GetRoot))]
        public IActionResult GetRoot()
        {
            var response = new
            {
                href = Url.Link(nameof(GetRoot), null),
                rooms = new { href = Url.Link(nameof(RoomController.GetRooms), null) },
                hotelInfo = new { href = Url.Link(nameof(InfoController.GetInfo), null) }
            };
            return Ok(response);
        }
    }
}
