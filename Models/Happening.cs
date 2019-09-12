using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
    
public class Happening
{
    [Key]
    public int HappeningId {get;set;}

    [Required]
    public string Title {get;set;}

    [Required]
    public TimeSpan Time {get;set;}

    [Required]
    public DateTime Date {get;set;}

    [Required]
    public int Duration {get;set;}


    [Required]
    public string Description {get;set;}
    
    

    public DateTime CreatedAt {get;set;} = DateTime.Now;

    public DateTime UpdatedAt {get;set;} = DateTime.Now;

    public int RegisterUserId { get; set; }
    // These are Navigation Properties (doesnt actually get added to the DB)
    // You need to use an Include statement to populate this


    public List<Association> Attendee { get; set; }
    public RegisterUser Creator { get; set; }





}