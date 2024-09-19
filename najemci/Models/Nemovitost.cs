using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace najemci.Models
{
    public class Nemovitost
    {
        public int Id { get; set; }
        [Display(Name = "Jméno nemovitosti")]
        [Required(ErrorMessage = "Uveď jméno nemovitosti")]
        public string Jmeno { get; set; } = string.Empty;
        [Display(Name = "Adresa nemovitosti")]
        [Required(ErrorMessage = "Uveď adresu nemovitosti")]
        public string Adresa { get; set; } = string.Empty;

        //navigace k bytům
        public virtual ICollection<Byt> Byty { get; set; } = new HashSet<Byt>();
    }
}
