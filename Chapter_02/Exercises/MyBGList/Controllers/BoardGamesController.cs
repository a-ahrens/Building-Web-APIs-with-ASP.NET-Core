﻿using Microsoft.AspNetCore.Mvc;

namespace MyBGList.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BoardGamesController : ControllerBase
    {
        private readonly ILogger<BoardGamesController> _logger;
        public BoardGamesController(ILogger<BoardGamesController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name  = "GetBoardGames")]
        public IEnumerable<BoardGame> Get()
        {
            return new[]
            {
                new BoardGame
                {
                    Id = 1,
                    Name = "Axis and Allies",
                    Year = 1981,
                    MinPlayers = 1,
                    MaxPlayers = 1
                },
                new BoardGame
                {
                    Id = 2,
                    Name = "Citadels",
                    Year = 2000,
                    MinPlayers = 1,
                    MaxPlayers = 1
                },
                new BoardGame
                {
                    Id = 3,
                    Name = "Terraforming Mars",
                    Year = 2016,
                    MinPlayers = 1,
                    MaxPlayers = 1
                }
            };
        }
    }
}
