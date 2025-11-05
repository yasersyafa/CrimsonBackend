using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrimsonBackend.Data;
using CrimsonBackend.Models;
using CrimsonBackend.DTOs;

namespace CrimsonBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController(ApplicationDbContext context, ILogger<PlayerController> logger) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<PlayerController> _logger = logger;

    /// <summary>
    /// Creates a new player with default game data
    /// Nickname must be unique
    /// </summary>
    /// <param name="request">Player creation request containing nickname</param>
    /// <returns>Newly created player with game data</returns>
    [HttpPost("new")]
    public async Task<ActionResult<PlayerResponse>> CreateNewPlayer([FromBody] CreatePlayerRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if nickname already exists - TIDAK BOLEH DUPLIKASI
            var existingPlayer = await _context.Players
                .FirstOrDefaultAsync(p => p.Nickname == request.Nickname);

            if (existingPlayer != null)
            {
                _logger.LogWarning($"Failed to create player: Nickname '{request.Nickname}' already exists");
                return Conflict(new { 
                    success = false,
                    message = $"Nickname '{request.Nickname}' sudah digunakan. Silakan gunakan nickname lain." 
                });
            }

            // Create new player
            var player = new Player
            {
                Id = Guid.NewGuid(),
                Nickname = request.Nickname
            };

            // Create default game data for the player
            var gameData = new GameData
            {
                Id = Guid.NewGuid(),
                PlayerId = player.Id,
                Score = 0,
                Coin = 0
            };

            _context.Players.Add(player);
            _context.GameDatas.Add(gameData);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"New player created: {player.Id} - {player.Nickname}");

            return Ok(new PlayerResponse
            {
                PlayerId = player.Id,
                Nickname = player.Nickname,
                Score = gameData.Score,
                Coin = gameData.Coin
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new player");
            return StatusCode(500, new { message = "An error occurred while creating the player" });
        }
    }

    /// <summary>
    /// Loads player data by nickname
    /// Player can enter the game if nickname exists
    /// </summary>
    /// <param name="request">Load player request containing nickname</param>
    /// <returns>Player data including playerId, nickname, score, and coins if found</returns>
    [HttpPost("load")]
    public async Task<ActionResult<PlayerResponse>> LoadPlayer([FromBody] LoadPlayerRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Cari player berdasarkan nickname
            var player = await _context.Players
                .Include(p => p.GameData)
                .FirstOrDefaultAsync(p => p.Nickname == request.Nickname);

            if (player == null)
            {
                _logger.LogWarning($"Failed to load player: Nickname '{request.Nickname}' not found");
                return NotFound(new { 
                    success = false,
                    message = $"Nickname '{request.Nickname}' tidak ditemukan. Silakan buat player baru terlebih dahulu." 
                });
            }

            if (player.GameData == null)
            {
                // Create default game data if it doesn't exist
                var gameData = new GameData
                {
                    Id = Guid.NewGuid(),
                    PlayerId = player.Id,
                    Score = 0,
                    Coin = 0
                };
                _context.GameDatas.Add(gameData);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Player loaded successfully: {player.Nickname} (created default game data)");

                return Ok(new PlayerResponse
                {
                    PlayerId = player.Id,
                    Nickname = player.Nickname,
                    Score = 0,
                    Coin = 0
                });
            }

            _logger.LogInformation($"Player loaded successfully: {player.Nickname} - Score: {player.GameData.Score}, Coin: {player.GameData.Coin}");

            return Ok(new PlayerResponse
            {
                PlayerId = player.Id,
                Nickname = player.Nickname,
                Score = player.GameData.Score,
                Coin = player.GameData.Coin
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error loading player: {request.Nickname}");
            return StatusCode(500, new { message = "An error occurred while loading the player" });
        }
    }

    /// <summary>
    /// Saves/updates player game data
    /// </summary>
    /// <param name="request">Save player data request containing player ID, score, and coins</param>
    /// <returns>Updated player data</returns>
    [HttpPost("save")]
    public async Task<ActionResult<PlayerResponse>> SavePlayerData([FromBody] SavePlayerDataRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var player = await _context.Players
                .Include(p => p.GameData)
                .FirstOrDefaultAsync(p => p.Id == request.PlayerId);

            if (player == null)
            {
                return NotFound(new { message = "Player not found" });
            }

            if (player.GameData == null)
            {
                // Create new game data if it doesn't exist
                var gameData = new GameData
                {
                    Id = Guid.NewGuid(),
                    PlayerId = player.Id,
                    Score = request.Score,
                    Coin = request.Coin
                };
                _context.GameDatas.Add(gameData);
            }
            else
            {
                // Update existing game data
                player.GameData.Score = request.Score;
                player.GameData.Coin = request.Coin;
                _context.GameDatas.Update(player.GameData);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Player data saved: {player.Id} - Score: {request.Score}, Coin: {request.Coin}");

            return Ok(new PlayerResponse
            {
                PlayerId = player.Id,
                Nickname = player.Nickname,
                Score = request.Score,
                Coin = request.Coin
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error saving player data: {request.PlayerId}");
            return StatusCode(500, new { message = "An error occurred while saving player data" });
        }
    }

    /// <summary>
    /// Gets the leaderboard sorted by score
    /// </summary>
    /// <param name="top">Optional parameter to limit the number of results (default: 10)</param>
    /// <returns>List of leaderboard entries with rank, nickname, score, and coins</returns>
    [HttpGet("leaderboard")]
    public async Task<ActionResult<List<LeaderboardEntry>>> GetLeaderboard([FromQuery] int? top = 10)
    {
        try
        {
            if (top.HasValue && top.Value <= 0)
            {
                return BadRequest(new { message = "Top parameter must be greater than 0" });
            }

            var limit = top ?? 10;

            // First, query the database and bring data to memory
            var gameDatas = await _context.GameDatas
                .Include(gd => gd.Player)
                .OrderByDescending(gd => gd.Score)
                .ThenByDescending(gd => gd.Coin)
                .Take(limit)
                .ToListAsync();

            // Then, create leaderboard entries with rank in memory (client-side)
            var leaderboard = gameDatas
                .Select((gd, index) => new LeaderboardEntry
                {
                    Rank = index + 1,
                    Nickname = gd.Player!.Nickname,
                    Score = gd.Score,
                    Coin = gd.Coin
                })
                .ToList();

            _logger.LogInformation($"Leaderboard retrieved: Top {limit}, Total entries: {leaderboard.Count}");

            return Ok(leaderboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leaderboard");
            return StatusCode(500, new { message = "An error occurred while retrieving the leaderboard" });
        }
    }
}

