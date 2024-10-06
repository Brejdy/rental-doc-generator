using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace najemci.Models
{
    public class Byt
    {
        public int Id { get; set; }
        public int NemovitostId { get; set; }

        [Display(Name = "Číslo bytu")]
        public int Cislo { get; set; }
        
        [Display(Name = "Výše nájmu")]
        public int Najem { get; set; }

        private int _kauce;
        public int Kauce { get => _kauce; set => _kauce = value; }
        
        [Display(Name = "Služby, vybavení, režie")]
        public int Sluzby { get; set; }

        private int _najemSluzby;
        public int NajemSluzby { get => _najemSluzby; set => _najemSluzby = value; }

        [Required(AllowEmptyStrings = true)]
        [Display(Name = "Rozloha")]
        public string? Rozloha { get; set; }  

        [Required(AllowEmptyStrings = true)]
        [Display(Name = "Patro + PP/NP")]
        public string? Patro { get; set; }

        [Required(AllowEmptyStrings = true)]
        [Display(Name = "Dispozice / Rozložení místností")]
        public string? Mistnosti { get; set; }
        
        //navigační vlastnost
        [ValidateNever]
        public Nemovitost Nemovitost { get; set; } 

        public virtual ICollection<Najemnik> Najemnici { get; set; } = new HashSet<Najemnik>();

        public void SetDefault()
        {
            try
            {
                if (Kauce == 0)
                {
                    Kauce = 2 * Najem;
                }
                NajemSluzby = Najem + Sluzby;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
