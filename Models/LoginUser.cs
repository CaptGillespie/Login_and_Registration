using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class LoginUser
{
    [Key]
    public int UserId {get;set;}
    [Required]
    public string Email {get; set;}
    [Required]
    public string Password { get; set; }
}
