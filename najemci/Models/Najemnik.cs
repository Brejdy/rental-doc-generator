using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace najemci.Models
{
    public class Najemnik
    {
        public int Id { get; set; }
        [Display(Name = "Celé jméno")]
        public string? Jmeno { get; set; }
        [EmailAddress]
        [Display(Name = "Emailová adresa")]
        public string? Email { get; set; }
        [Phone]
        [Display(Name = "Telefonní číslo")]
        public string? Telefon { get; set; }
        [Display(Name = "Datum Narození")]
        [DataType(DataType.Date)]
        public DateTime? DatumNarozeni { get; set; }
        [Display(Name = "Nájem od data")]
        [DataType(DataType.Date)]
        public DateTime? NajemOd { get; set; }
        [Display(Name = "Rodné číslo")]
        public string? RodneCislo { get; set; }
        [Display(Name ="Číslo občanského průkazu")]
        public int? CisloOP { get; set; }
        [Display(Name = "Číslo účtu")]
        public string? CisloUctu { get; set; }
        public int BytId { get; set; }
        [Display(Name = "Nájemník / Člen domácnosti")]
        public Role? RoleNajemnika { get; set; }
        [ValidateNever]
        public Byt Byt { get; set; }
    }

    public enum Role
    {
        Najemnik,
        ClenDomacnosti
    }
        
}
