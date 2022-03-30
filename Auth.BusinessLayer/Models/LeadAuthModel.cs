using Marvelous.Contracts.Enums;

namespace Auth.BusinessLayer.Models;

public struct LeadAuthModel
{
    public int Id { get; set; }
    public Role Role { get; set; }
    public string HashPassword { get; set; }
}