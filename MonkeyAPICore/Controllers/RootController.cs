using Microsoft.AspNetCore.Mvc;
using MonkeyAPICore.Models;
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
            var response = new RootResponse
            {
                // don't forget update mappiing 
                Self = Link.To(nameof(GetRoot)),
                Info = Link.To(nameof(InfoController.GetInfo)),
                Rooms = Link.To(nameof(RoomsController.GetRoomsAsync))
            };
            return Ok(response);
        }
    }
}
