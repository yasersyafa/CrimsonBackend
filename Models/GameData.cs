using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrimsonBackend.Models;

public class GameData
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public int Score { get; set; } = 0;
    
    public int Coin { get; set; } = 0;
    
    [Required]
    [ForeignKey("Player")]
    public Guid PlayerId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Player? Player { get; set; }
}