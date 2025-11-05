using System.ComponentModel.DataAnnotations;

namespace CrimsonBackend.Models;

public class Player
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Nickname { get; set; } = string.Empty;
    
    // Navigation property
    public GameData? GameData { get; set; }
}