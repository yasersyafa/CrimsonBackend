using System.ComponentModel.DataAnnotations;

namespace CrimsonBackend.DTOs;

public class CreatePlayerRequest
{
    [Required(ErrorMessage = "Nickname is required")]
    [MaxLength(100, ErrorMessage = "Nickname cannot exceed 100 characters")]
    public string Nickname { get; set; } = string.Empty;
}

public class LoadPlayerRequest
{
    [Required(ErrorMessage = "Nickname is required")]
    [MaxLength(100, ErrorMessage = "Nickname cannot exceed 100 characters")]
    public string Nickname { get; set; } = string.Empty;
}

public class PlayerResponse
{
    public Guid PlayerId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public int Score { get; set; }
    public int Coin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class LeaderboardEntry
{
    public int Rank { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public int Score { get; set; }
    public int Coin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SavePlayerDataRequest
{
    [Required]
    public Guid PlayerId { get; set; }
    
    public int Score { get; set; }
    
    public int Coin { get; set; }
}

