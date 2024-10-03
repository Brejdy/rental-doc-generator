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
        [Display(Name = "Číslo popisné")]
        public int CisloPopisne { get; set; }
        [Display(Name = "Část obce")]
        public string Obec { get; set; } = string.Empty;
        [Display(Name = "LV")]
        public string LV { get; set; }
        [Display(Name = "Parcela")]
        public string Parcela {  get; set; }

        //navigace k bytům
        public virtual ICollection<Byt> Byty { get; set; } = new HashSet<Byt>();
    }
}
