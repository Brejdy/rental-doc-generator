using Microsoft.AspNetCore.Mvc;
using najemci.Data;
using Xceed.Words.NET;
using Xceed.Document.NET;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Text;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Identity.Client;

namespace najemci.Controllers
{
    public class WordController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        public WordController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult DPNWord(int bytId)
        {
            var byt = _context.Byty.Include(b => b.Nemovitost).Include(b => b.Najemnici).FirstOrDefault(b => b.Id == bytId);

            var nemovId = byt.NemovitostId;
            var cisloBytu = byt.Cislo;

            var najemce = byt.Najemnici.FirstOrDefault();
            var puvodniNajem = najemce?.NajemOd;
            var puvNajem = najemce?.NajemOd ?? DateTime.Now;
            var konecNajmu = puvodniNajem.HasValue ? puvodniNajem.Value.AddYears(1) : (DateTime?)null;
            DateTime datumPodpisu = DateTime.Now;
            DateTime? datumProdlouzeni = new DateTime(DateTime.Now.Year, puvNajem.Month, puvNajem.Day);
            datumProdlouzeni = datumProdlouzeni.Value.AddYears(1);
            string jmenaNajemcu = string.Join(", ", byt.Najemnici.Where(n => n.RoleNajemnika == Models.Role.Najemnik).Select(n => n.Jmeno));
            string nazevSouboru = $"DodatekSmlouvyProdlouzeniNajmu_{najemce.Jmeno.Replace(" ", "_").Replace(",", "")}.docx";

            string templatePath = Path.Combine(_environment.WebRootPath, "Templates", "Prodlouzeni.docx");         

            StringBuilder mamka = new StringBuilder();
            mamka.AppendLine("Lenka Bradáčová");
            mamka.AppendLine("datum narození: 22.5.1968");
            mamka.AppendLine("trvale bytem: Javorová 241, 252 44 Dolní Jirčany ");
            mamka.AppendLine("a");

            StringBuilder najemnikInfo = new StringBuilder();
            foreach (var n in byt.Najemnici)
            {
                if (n.RoleNajemnika == Models.Role.Najemnik)
                {
                    if (!string.IsNullOrWhiteSpace(n.Jmeno))
                    {
                        najemnikInfo.AppendLine($"{n.Jmeno}");
                    }
                    if (n.DatumNarozeni.HasValue)
                    {
                        najemnikInfo.AppendLine($"Datum narození: {n.DatumNarozeni?.ToString("dd.MM.yyyy")}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.RodneCislo))
                    {
                        najemnikInfo.AppendLine($"Rodné číslo: {n.RodneCislo}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.Email))
                    {
                        najemnikInfo.AppendLine($"Email: {n.Email}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.Telefon))
                    {
                        najemnikInfo.AppendLine($"Tel.: {n.Telefon}");
                    }
                    if (n.CisloOP.HasValue)
                    {
                        najemnikInfo.AppendLine($"Číslo občanského průkazu: {n.CisloOP}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.CisloUctu))
                    {
                        najemnikInfo.AppendLine($"Číslo účtu: {n.CisloUctu}");
                    }

                    najemnikInfo.AppendLine($"");
                }
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (var document = DocX.Load(templatePath))
                {               

                    if (nemovId == 2)
                    {
                        document.ReplaceText("[EMAIL]", "pripotocni31@gmail.com");
                    }
                    else
                    {
                        document.ReplaceText("[EMAIL]", "nadpahorkem24@gmail.com");
                    }

                    if (nemovId == 3)
                    {
                        document.ReplaceText("[KONTO]", "229263108/0300, vedený u ČSOB");
                    }
                    else if (nemovId == 2)
                    {
                        document.ReplaceText("[KONTO]", "133936558/0300, vedený u ČSOB");
                    }
                    else
                    {
                        document.ReplaceText("[KONTO]", "816846033/0800, vedený u České spořitelny");
                    }
                    document.ReplaceText("[CISLOBYTU]", byt.Cislo.ToString());
                    document.ReplaceText("[ADRESA]", byt.Nemovitost.Adresa);
                    document.ReplaceText("[KONECNAJMU]", konecNajmu?.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[DATUMPRODLOUZENI]", datumProdlouzeni?.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[NAJEMOD]", puvNajem.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[DATUMPODPISU]", datumPodpisu.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[JMENANAJEMCU]", jmenaNajemcu);

                    if (nemovId == 2 || nemovId >= 4)
                        document.ReplaceText("[TEL]", "720 389 260");
                    else
                        document.ReplaceText("[TEL]", "720 389 259");

                    var najemnikPlaceholder = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[NAJEMNIK]"));
                    if (najemnikPlaceholder != null)
                    {
                        
                        var najemnikParagraph = najemnikPlaceholder.InsertParagraphAfterSelf(najemnikInfo.ToString())
                                                                   .Font("Times New Roman")
                                                                   .FontSize(12);
                        najemnikParagraph.Alignment = Alignment.left;

                        
                        najemnikPlaceholder.Remove(false);
                    }

                    if (nemovId == 2 || nemovId == 3)
                    {
                        var mamkaPlaceholder = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("<!- PRIPO/PAHO ->"));
                        if (mamkaPlaceholder != null)
                        {
                            
                            var mamkaParagraph = mamkaPlaceholder.InsertParagraphAfterSelf(mamka.ToString())
                                                                 .Font("Times New Roman")
                                                                 .FontSize(12);
                            mamkaParagraph.Alignment = Alignment.left;

                            
                            mamkaPlaceholder.Remove(false);
                        }


                        document.ReplaceText("<!-PODPIS->", "Lenka Bradáčová a ");
                    }
                    else
                    {
                        document.ReplaceText("<!- PRIPO/PAHO ->", "");
                        document.ReplaceText("<!-PODPIS->", "");
                    }

                    document.SaveAs(ms);
                }

                byte[] fileContent = ms.ToArray();
                return File(fileContent, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", nazevSouboru);
            }
        }

        public IActionResult NSWord(int bytId)
        {
            var byt = _context.Byty.Include(b => b.Nemovitost).Include(b => b.Najemnici).FirstOrDefault(b => b.Id == bytId);

            var nemovId = byt.NemovitostId;
            var cisloBytu = byt.Cislo;

            var najemce = byt.Najemnici.FirstOrDefault();
            var puvodniNajem = najemce?.NajemOd;
            var puvNajem = najemce?.NajemOd ?? DateTime.Now;
            var konecNajmu = puvodniNajem.HasValue ? puvodniNajem.Value.AddYears(1) : (DateTime?)null;
            DateTime datumPodpisu = DateTime.Now;
            DateTime? datumProdlouzeni = new DateTime(DateTime.Now.Year, puvNajem.Month, puvNajem.Day);
            datumProdlouzeni = datumProdlouzeni.Value.AddYears(1);
            string jmenaNajemcu = string.Join(", ", byt.Najemnici.Where(n => n.RoleNajemnika == Models.Role.Najemnik).Select(n => n.Jmeno));
            string nazevSouboru = $"NajemniSmlouva_{najemce.Jmeno.Replace(" ", "_").Replace(",", "")}.docx";

            int pulNajem = byt.Najem / 2;
            int jedenPulNajem = byt.Najem + pulNajem;
            string najemSlovy = CisloNaText.CisloNaSlova(byt.Najem);
            string kauceSlovy = CisloNaText.CisloNaSlova(byt.Kauce);
            string sluzbySlovy = CisloNaText.CisloNaSlova(byt.Sluzby);
            string jedenPulNajemSlovy = CisloNaText.CisloNaSlova(jedenPulNajem);
           
            string templatePath = Path.Combine(_environment.WebRootPath, "Templates", "NajemniSmlouva.docx");

            StringBuilder mamka = new StringBuilder();
            mamka.AppendLine("Lenka Bradáčová");
            mamka.AppendLine("datum narození: 22.5.1968");
            mamka.AppendLine("trvale bytem: Javorová 241, 252 44 Dolní Jirčany ");

            StringBuilder najemnikInfo = new StringBuilder();
            foreach (var n in byt.Najemnici)
            {
                if (n.RoleNajemnika == Models.Role.Najemnik)
                {
                    if (!string.IsNullOrWhiteSpace(n.Jmeno))
                    {
                        najemnikInfo.AppendLine($"{n.Jmeno}");
                    }
                    if (n.DatumNarozeni.HasValue)
                    {
                        najemnikInfo.AppendLine($"Datum narození: {n.DatumNarozeni?.ToString("dd.MM.yyyy")}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.RodneCislo))
                    {
                        najemnikInfo.AppendLine($"Rodné číslo: {n.RodneCislo}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.Email))
                    {
                        najemnikInfo.AppendLine($"Email: {n.Email}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.Telefon))
                    {
                        najemnikInfo.AppendLine($"Tel.: {n.Telefon}");
                    }
                    if (n.CisloOP.HasValue)
                    {
                        najemnikInfo.AppendLine($"Číslo občanského průkazu: {n.CisloOP}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.CisloUctu))
                    {
                        najemnikInfo.AppendLine($"Číslo účtu: {n.CisloUctu}");
                    }

                    najemnikInfo.AppendLine($" ");
                }
            }

            StringBuilder clen = new StringBuilder();
            foreach (var n in byt.Najemnici)
            {
                if (n.RoleNajemnika == Models.Role.ClenDomacnosti)
                {
                    if (!string.IsNullOrWhiteSpace(n.Jmeno))
                    {
                        clen.AppendLine($"{n.Jmeno}");
                    }
                    if (n.DatumNarozeni.HasValue)
                    {
                        clen.AppendLine($"Datum narození: {n.DatumNarozeni?.ToString("dd.MM.yyyy")}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.RodneCislo))
                    {
                        clen.AppendLine($"Rodné číslo: {n.RodneCislo}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.Email))
                    {
                        clen.AppendLine($"Email: {n.Email}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.Telefon))
                    {
                        clen.AppendLine($"Tel.: {n.Telefon}");
                    }
                    if (n.CisloOP.HasValue)
                    {
                        clen.AppendLine($"Číslo občanského průkazu: {n.CisloOP}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.CisloUctu))
                    {
                        clen.AppendLine($"Číslo účtu: {n.CisloUctu}");
                    }

                    clen.AppendLine($"");
                }
            }

            StringBuilder podpisy = new StringBuilder();
            foreach (var n in byt.Najemnici)
            {
                podpisy.AppendLine($"{n.Jmeno}");
                podpisy.AppendLine(" ");
                podpisy.AppendLine(" ");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (var document = DocX.Load(templatePath))
                {

                    if (nemovId == 2)
                    {
                        document.ReplaceText("[EMAIL]", "pripotocni31@gmail.com");
                    }
                    else
                    {
                        document.ReplaceText("[EMAIL]", "nadpahorkem24@gmail.com");
                    }

                    if (nemovId == 3)
                    {
                        document.ReplaceText("[KONTO]", "229263108/0300, vedený u ČSOB");
                    }
                    else if (nemovId == 2)
                    {
                        document.ReplaceText("[KONTO]", "133936558/0300, vedený u ČSOB");
                    }
                    else
                    {
                        document.ReplaceText("[KONTO]", "816846033/0800, vedený u České spořitelny");
                    }
                    document.ReplaceText("[CISLOBYTU]", byt.Cislo.ToString());
                    document.ReplaceText("[ADRESA]", byt.Nemovitost.Adresa);
                    document.ReplaceText("[KONECNAJMU]", konecNajmu?.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[DATUMPRODLOUZENI]", datumProdlouzeni?.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[NAJEMOD]", puvNajem.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[DATUMPODPISU]", datumPodpisu.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[JMENANAJEMCU]", jmenaNajemcu);
                    document.ReplaceText("[CISLOPOPISNE]", byt.Nemovitost.CisloPopisne.ToString());
                    document.ReplaceText("[PARCELA]", byt.Nemovitost.Parcela.ToString());
                    document.ReplaceText("[OBEC]", byt.Nemovitost.Obec.ToString());
                    document.ReplaceText("[LV]", byt.Nemovitost.LV.ToString());
                    document.ReplaceText("[DISPOZICE]", byt.Mistnosti.ToString());
                    document.ReplaceText("[ROZLOHA]", byt.Rozloha.ToString());
                    document.ReplaceText("[NAJEMNIKEMAIL]", najemce.Email.ToString());
                    document.ReplaceText("[NAJEMNE]", byt.Najem.ToString());
                    document.ReplaceText("[NAJEMNESLOVY]", najemSlovy);
                    document.ReplaceText("[KAUCE]", byt.Kauce.ToString());
                    document.ReplaceText("[KAUCESLOVY]", kauceSlovy);
                    document.ReplaceText("[SLUZBY]", byt.Sluzby.ToString());
                    document.ReplaceText("[SLUZBYSLOVY]", sluzbySlovy);
                    document.ReplaceText("[JEDENPULNAJEMNE]", jedenPulNajem.ToString());
                    document.ReplaceText("[JEDENPULNAJEMNESLOVY]", jedenPulNajemSlovy);
                    document.ReplaceText("[VARIABILNI]", byt.Cislo.ToString());
                    document.ReplaceText("[NAJEMNIKPODPIS]", podpisy.ToString());

                    switch (byt.Mistnosti)
                    {
                        case ("1+1"):
                            document.ReplaceText("[MISTNOSTIVBYTU]", "předsíně, pokoje, kuchyně, koupelny a WC");
                            break;
                        case ("1+kk"):
                            document.ReplaceText("[MISTNOSTIVBYTU]", "předsíně, pokoje s kuchyňskou linkou, koupelny a WC");
                            break;
                        case ("2+0"):
                            document.ReplaceText("[MISTNOSTIVBYTU]", "předsíně, dvou pokojů, koupelny a dvou WC");
                            break;
                        case ("2+1"):
                            document.ReplaceText("[MISTNOSTIVBYTU]", "předsíně, pokoje, kuchyně, ložnice, koupelny a WC");
                            break;
                        case ("2+kk"):
                            document.ReplaceText("[MISTNOSTIVBYTU]", "předsíně, pokoje s kuchyňskou linkou, ložnice, koupelny a WC");
                            break;
                        case ("3+1"):
                            document.ReplaceText("[MISTNOSTIVBYTU]", "předsíně, dvou pokojů, kuchyně, ložnice, koupelny a WC");
                            break;
                        case ("3+kk"):
                            document.ReplaceText("[MISTNOSTIVBYTU]", "předsíně, pokoje s kuchyňskou linkou, dvou pokojů, koupelny a WC");
                            break;
                        case ("8+2kk"):
                            document.ReplaceText("[MISTNOSTIVBYTU]", "chodby, šesti pokojů, dvou pokojů s kuchyňskou linkou, tří koupelen a tří WC");
                            break;
                    }

                    if (nemovId == 2 || nemovId >= 4)
                        document.ReplaceText("[TEL]", "720 389 260");
                    else
                        document.ReplaceText("[TEL]", "720 389 259");

                    var najemnikPlaceholder = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[NAJEMNIK]"));
                    if (najemnikPlaceholder != null)
                    {
                        var najemnikParagraph = najemnikPlaceholder.InsertParagraphAfterSelf(najemnikInfo.ToString()).Font("Aptos").FontSize(12);
                        najemnikParagraph.Alignment = Alignment.left;

                        najemnikPlaceholder.Remove(false);
                    }

                    var clenDoma = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[CLENDOMACNOSTI]"));
                    if (clenDoma != null)
                    {
                        var clenDomacnosti = clenDoma.InsertParagraphAfterSelf(clen.ToString()).Font("Aptos").FontSize(12);
                        clenDomacnosti.Alignment = Alignment.left;

                        clenDoma.Remove(false);
                    }

                    if (nemovId == 2 || nemovId == 3)
                    {
                        var mamkaPlaceholder = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("<!- PRIPO/PAHO ->"));
                        if (mamkaPlaceholder != null)
                        {
                            var mamkaParagraph = mamkaPlaceholder.InsertParagraphAfterSelf(mamka.ToString()).Font("Aptos").FontSize(12);
                            mamkaParagraph.Alignment = Alignment.left;

                            mamkaPlaceholder.Remove(false);
                        }

                        document.ReplaceText("<!-PODPIS->", "Lenka Bradáčová");
                    }
                    else
                    {
                        document.ReplaceText("<!- PRIPO/PAHO ->", "");
                        document.ReplaceText("<!-PODPIS->", "");
                    }

                    document.SaveAs(ms);
                }

                byte[] fileContent = ms.ToArray();
                return File(fileContent, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", nazevSouboru);
            }
        }
        public IActionResult PPWord(int bytId)
        {
            var byt = _context.Byty.Include(b => b.Nemovitost).Include(b => b.Najemnici).FirstOrDefault(b => b.Id == bytId);

            var nemovId = byt.NemovitostId;
            var cisloBytu = byt.Cislo;

            var najemce = byt.Najemnici.FirstOrDefault();
            var puvodniNajem = najemce?.NajemOd;
            var puvNajem = najemce?.NajemOd ?? DateTime.Now;
            var konecNajmu = puvodniNajem.HasValue ? puvodniNajem.Value.AddYears(1) : (DateTime?)null;
            DateTime datumPodpisu = DateTime.Now;
            DateTime? datumProdlouzeni = new DateTime(DateTime.Now.Year, puvNajem.Month, puvNajem.Day);
            datumProdlouzeni = datumProdlouzeni.Value.AddYears(1);
            string jmenaNajemcu = string.Join(", ", byt.Najemnici.Where(n => n.RoleNajemnika == Models.Role.Najemnik).Select(n => n.Jmeno));
            string nazevSouboru = $"PredavaciProtokol_{najemce.Jmeno.Replace(" ", "_").Replace(",", "")}.docx";
            string templatePath = Path.Combine(_environment.WebRootPath, "Templates", "PredavaciProtokol.docx");

            StringBuilder najemnikInfo = new StringBuilder();
            int pocetOsob = 0;
            foreach (var n in byt.Najemnici)
            {
                pocetOsob++;
                if (n.RoleNajemnika == Models.Role.Najemnik)
                {
                    najemnikInfo.AppendLine(n.Jmeno);
                    if (n.DatumNarozeni.HasValue)
                    {
                        najemnikInfo.AppendLine($"Datum narození: {n.DatumNarozeni?.ToString("dd.MM.yyyy")}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.RodneCislo))
                    {
                        najemnikInfo.AppendLine($"Rodné číslo: {n.RodneCislo}");
                    }
                    najemnikInfo.AppendLine(" ");
                }
            }


            using (MemoryStream ms = new MemoryStream())
            {
                using (var document = DocX.Load(templatePath))
                {
                    if (nemovId == 2)
                    {
                        document.ReplaceText("[PREDAVAJICI]", "Michal Bradáč");
                        document.ReplaceText("[NAROZENI]", "25.1.1996");
                    }
                    else
                    {
                        document.ReplaceText("[PREDAVAJICI]", "Jan Bradáč");
                        document.ReplaceText("[NAROZENI]", "19.8.1998");
                    }

                    var najemnikPlaceholder = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[NAJEMNIK]"));
                    if (najemnikPlaceholder != null)
                    {
                        var najemnikParagraph = najemnikPlaceholder.InsertParagraphAfterSelf(najemnikInfo.ToString()).Font("Aptos").FontSize(12);
                        najemnikParagraph.Alignment = Alignment.left;

                        najemnikPlaceholder.Remove(false);
                    }

                    document.ReplaceText("[CISLOBYTU]", byt.Cislo.ToString());
                    document.ReplaceText("[PATRO]", byt.Patro.ToString());
                    document.ReplaceText("[ADRESA]", byt.Nemovitost.Adresa.ToString());
                    document.ReplaceText("[ROZLOHA]", byt.Rozloha.ToString());
                    document.ReplaceText("[DISPOZICE]", byt.Mistnosti.ToString());
                    document.ReplaceText("[POCETOSOB]", pocetOsob.ToString());
                    document.ReplaceText("[DATUMPODPISU]", datumPodpisu.ToString("dd.MM.yyyy"));


                    document.SaveAs(ms);
                }
                byte[] fileContent = ms.ToArray();
                return File(fileContent, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", nazevSouboru);
            }
        }
        public IActionResult DUNWord(int bytId)
        {
            var byt = _context.Byty.Include(b => b.Nemovitost).Include(b => b.Najemnici).FirstOrDefault(b => b.Id == bytId);

            var nemovId = byt.NemovitostId;
            var cisloBytu = byt.Cislo;

            var najemce = byt.Najemnici.FirstOrDefault();
            var puvodniNajem = najemce?.NajemOd;
            var puvNajem = najemce?.NajemOd ?? DateTime.Now;
            var konecNajmu = puvodniNajem.HasValue ? puvodniNajem.Value.AddYears(1) : (DateTime?)null;
            DateTime datumPodpisu = DateTime.Now;
            DateTime? datumProdlouzeni = new DateTime(DateTime.Now.Year, puvNajem.Month, puvNajem.Day);
            datumProdlouzeni = datumProdlouzeni.Value.AddYears(1);
            string jmenaNajemcu = string.Join(", ", byt.Najemnici.Where(n => n.RoleNajemnika == Models.Role.Najemnik).Select(n => n.Jmeno));
            string nazevSouboru = $"DohodaUkonceniNajmu_{najemce.Jmeno.Replace(" ", "_").Replace(",", "")}.docx";
            string templatePath = Path.Combine(_environment.WebRootPath, "Templates", "DohodaUkonceni.docx");
            int lastDayMonth = DateTime.DaysInMonth(datumPodpisu.Year, datumPodpisu.Month);
            DateTime konecMesice = new DateTime(datumPodpisu.Year, datumPodpisu.Month, lastDayMonth);
            int snizKauce = byt.Kauce - 5000;

            StringBuilder najemnikInfo = new StringBuilder();
            foreach (var n in byt.Najemnici)
            {
                if (n.RoleNajemnika == Models.Role.Najemnik)
                {
                    najemnikInfo.AppendLine(n.Jmeno);
                    if (n.DatumNarozeni.HasValue)
                    {
                        najemnikInfo.AppendLine($"Datum narození: {n.DatumNarozeni?.ToString("dd.MM.yyyy")}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.RodneCislo))
                    {
                        najemnikInfo.AppendLine($"Rodné číslo: {n.RodneCislo}");
                    }
                    najemnikInfo.AppendLine(" ");
                }
            }


            using (MemoryStream ms = new MemoryStream())
            {
                using (var document = DocX.Load(templatePath))
                {
                    if (nemovId == 2 || 3 == nemovId)
                    {
                        document.ReplaceText("[PRONAJIMATEL]", "Lenka Bradáčová, Michal Bradáč a Jan Bradáč");
                    }
                    else
                    {
                        document.ReplaceText("[PRONAJIMATEL]", "Michal Bradáč a Jan Bradáč");
                    }

                    var najemnikPlaceholder = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[NAJEMNIK]"));
                    if (najemnikPlaceholder != null)
                    {
                        var najemnikParagraph = najemnikPlaceholder.InsertParagraphAfterSelf(najemnikInfo.ToString()).Font("Aptos").FontSize(12);
                        najemnikParagraph.Alignment = Alignment.left;

                        najemnikPlaceholder.Remove(false);
                    }

                    document.ReplaceText("[NAJEMOD]", najemce.NajemOd?.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[CISLOBYTU]", byt.Cislo.ToString());
                    document.ReplaceText("[ADRESA]", byt.Nemovitost.Adresa.ToString());
                    document.ReplaceText("[PATRO]", byt.Patro.ToString());
                    document.ReplaceText("[CISLOPOPISNE]", byt.Nemovitost.CisloPopisne.ToString());
                    document.ReplaceText("[PARCELA]", byt.Nemovitost.Parcela.ToString());
                    document.ReplaceText("[OBEC]", byt.Nemovitost.Obec.ToString());
                    document.ReplaceText("[LV]", byt.Nemovitost.LV.ToString());
                    document.ReplaceText("[DATUMPODPISU]", datumPodpisu.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[KONECNAJMU]", konecMesice.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[KAUCE]", byt.Kauce.ToString());
                    document.ReplaceText("[KAUCEMIN]", snizKauce.ToString());


                    document.SaveAs(ms);
                }
                byte[] fileContent = ms.ToArray();
                return File(fileContent, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", nazevSouboru);
            }
        }
        public IActionResult UDWord(int bytId)
        {
            var byt = _context.Byty.Include(b => b.Nemovitost).Include(b => b.Najemnici).FirstOrDefault(b => b.Id == bytId);

            var nemovId = byt.NemovitostId;
            var cisloBytu = byt.Cislo;

            var najemce = byt.Najemnici.FirstOrDefault();
            var puvodniNajem = najemce?.NajemOd;
            var puvNajem = najemce?.NajemOd ?? DateTime.Now;
            var konecNajmu = puvodniNajem.HasValue ? puvodniNajem.Value.AddYears(1) : (DateTime?)null;
            DateTime datumPodpisu = DateTime.Now;
            DateTime? datumProdlouzeni = new DateTime(DateTime.Now.Year, puvNajem.Month, puvNajem.Day);
            datumProdlouzeni = datumProdlouzeni.Value.AddYears(1);
            string jmenaNajemcu = string.Join(", ", byt.Najemnici.Where(n => n.RoleNajemnika == Models.Role.Najemnik).Select(n => n.Jmeno));
            string nazevSouboru = $"UznaniDluhu_{najemce.Jmeno.Replace(" ", "_").Replace(",", "")}.docx";
            string templatePath = Path.Combine(_environment.WebRootPath, "Templates", "UznaniDluhu.docx");
            int lastDayMonth = DateTime.DaysInMonth(datumPodpisu.Year, datumPodpisu.Month);
            DateTime konecMesice = new DateTime(datumPodpisu.Year, datumPodpisu.Month, lastDayMonth);
            int snizKauce = byt.Kauce - 5000;

            StringBuilder najemnikInfo = new StringBuilder();
            foreach (var n in byt.Najemnici)
            {
                if (n.RoleNajemnika == Models.Role.Najemnik)
                {
                    najemnikInfo.AppendLine(n.Jmeno);
                    if (n.DatumNarozeni.HasValue)
                    {
                        najemnikInfo.AppendLine($"Datum narození: {n.DatumNarozeni?.ToString("dd.MM.yyyy")}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.RodneCislo))
                    {
                        najemnikInfo.AppendLine($"Rodné číslo: {n.RodneCislo}");
                    }
                    najemnikInfo.AppendLine(" ");
                }
            }


            using (MemoryStream ms = new MemoryStream())
            {
                using (var document = DocX.Load(templatePath))
                {
                    if (nemovId == 2 || 3 == nemovId)
                    {
                        document.ReplaceText("[PRONAJIMATEL]", "Lenka Bradáčová, Michal Bradáč a Jan Bradáč");
                    }
                    else
                    {
                        document.ReplaceText("[PRONAJIMATEL]", "Michal Bradáč a Jan Bradáč");
                    }

                    var najemnikPlaceholder = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[NAJEMNIK]"));
                    if (najemnikPlaceholder != null)
                    {
                        var najemnikParagraph = najemnikPlaceholder.InsertParagraphAfterSelf(najemnikInfo.ToString()).Font("Aptos").FontSize(12);
                        najemnikParagraph.Alignment = Alignment.left;

                        najemnikPlaceholder.Remove(false);
                    }

                    if (nemovId == 3)
                    {
                        document.ReplaceText("[KONTO]", "229263108/0300, vedený u ČSOB");
                    }
                    else if (nemovId == 2)
                    {
                        document.ReplaceText("[KONTO]", "133936558/0300, vedený u ČSOB");
                    }
                    else
                    {
                        document.ReplaceText("[KONTO]", "816846033/0800, vedený u České spořitelny");
                    }

                    document.ReplaceText("[NAJEMOD]", najemce.NajemOd?.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[CISLOBYTU]", byt.Cislo.ToString());
                    document.ReplaceText("[ADRESA]", byt.Nemovitost.Adresa.ToString());
                    document.ReplaceText("[PATRO]", byt.Patro.ToString());
                    document.ReplaceText("[CISLOPOPISNE]", byt.Nemovitost.CisloPopisne.ToString());
                    document.ReplaceText("[PARCELA]", byt.Nemovitost.Parcela.ToString());
                    document.ReplaceText("[OBEC]", byt.Nemovitost.Obec.ToString());
                    document.ReplaceText("[LV]", byt.Nemovitost.LV.ToString());
                    document.ReplaceText("[DATUMPODPISU]", datumPodpisu.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[KONECNAJMU]", konecMesice.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[KAUCE]", byt.Kauce.ToString());
                    document.ReplaceText("[ROZLOHA]", byt.Rozloha.ToString());
                    document.ReplaceText("[DISPOZICE]", byt.Mistnosti.ToString());
                    document.ReplaceText("[NAJEM]", byt.Najem.ToString());
                    document.ReplaceText("[SLUZBY]", byt.Sluzby.ToString());
                    document.ReplaceText("[NAJEMSLUZBY]", byt.NajemSluzby.ToString());
                    document.ReplaceText("[JMENO]", najemce.Jmeno.ToString());



                    document.SaveAs(ms);
                }
                byte[] fileContent = ms.ToArray();
                return File(fileContent, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", nazevSouboru);
            }
        }
        public IActionResult VNWord(int bytId)
        {
            var byt = _context.Byty.Include(b => b.Nemovitost).Include(b => b.Najemnici).FirstOrDefault(b => b.Id == bytId);

            var nemovId = byt.NemovitostId;
            var cisloBytu = byt.Cislo;

            var najemce = byt.Najemnici.FirstOrDefault();
            var puvodniNajem = najemce?.NajemOd;
            var puvNajem = najemce?.NajemOd ?? DateTime.Now;
            var konecNajmu = puvodniNajem.HasValue ? puvodniNajem.Value.AddYears(1) : (DateTime?)null;
            DateTime datumPodpisu = DateTime.Now;
            DateTime? datumProdlouzeni = new DateTime(DateTime.Now.Year, puvNajem.Month, puvNajem.Day);
            datumProdlouzeni = datumProdlouzeni.Value.AddYears(1);
            string jmenaNajemcu = string.Join(", ", byt.Najemnici.Where(n => n.RoleNajemnika == Models.Role.Najemnik).Select(n => n.Jmeno));
            string nazevSouboru = $"Vypoved_z_najmu_{najemce.Jmeno.Replace(" ", "_").Replace(",", "")}.docx";
            string templatePath = Path.Combine(_environment.WebRootPath, "Templates", "Vypoved.docx");
            int lastDayMonth = DateTime.DaysInMonth(datumPodpisu.Year, datumPodpisu.Month);
            DateTime konecMesice = new DateTime(datumPodpisu.Year, datumPodpisu.Month, lastDayMonth);
            int snizKauce = byt.Kauce - 5000;

            StringBuilder najemnikInfo = new StringBuilder();
            foreach (var n in byt.Najemnici)
            {
                if (n.RoleNajemnika == Models.Role.Najemnik)
                {
                    najemnikInfo.AppendLine(n.Jmeno);
                    if (n.DatumNarozeni.HasValue)
                    {
                        najemnikInfo.AppendLine($"Datum narození: {n.DatumNarozeni?.ToString("dd.MM.yyyy")}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.RodneCislo))
                    {
                        najemnikInfo.AppendLine($"Rodné číslo: {n.RodneCislo}");
                    }
                    najemnikInfo.AppendLine(" ");
                }
            }


            using (MemoryStream ms = new MemoryStream())
            {
                using (var document = DocX.Load(templatePath))
                {
                    if (nemovId == 2 || 3 == nemovId)
                    {
                        document.ReplaceText("[PRONAJIMATEL]", "Lenka Bradáčová, Michal Bradáč a Jan Bradáč");
                    }
                    else
                    {
                        document.ReplaceText("[PRONAJIMATEL]", "Michal Bradáč a Jan Bradáč");
                    }

                    var najemnikPlaceholder = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[NAJEMNIK]"));
                    if (najemnikPlaceholder != null)
                    {
                        var najemnikParagraph = najemnikPlaceholder.InsertParagraphAfterSelf(najemnikInfo.ToString()).Font("Aptos").FontSize(12);
                        najemnikParagraph.Alignment = Alignment.left;

                        najemnikPlaceholder.Remove(false);
                    }

                    document.ReplaceText("[NAJEMOD]", najemce.NajemOd?.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[CISLOBYTU]", byt.Cislo.ToString());
                    document.ReplaceText("[ADRESA]", byt.Nemovitost.Adresa.ToString());
                    document.ReplaceText("[PATRO]", byt.Patro.ToString());
                    document.ReplaceText("[CISLOPOPISNE]", byt.Nemovitost.CisloPopisne.ToString());
                    document.ReplaceText("[PARCELA]", byt.Nemovitost.Parcela.ToString());
                    document.ReplaceText("[OBEC]", byt.Nemovitost.Obec.ToString());
                    document.ReplaceText("[LV]", byt.Nemovitost.LV.ToString());
                    document.ReplaceText("[DATUM]", datumPodpisu.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[KONECNAJMU]", konecMesice.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[KAUCE]", byt.Kauce.ToString());
                    document.ReplaceText("[ROZLOHA]", byt.Rozloha.ToString());
                    document.ReplaceText("[DISPOZICE]", byt.Mistnosti.ToString());
                    document.ReplaceText("[NAJEM]", byt.Najem.ToString());
                    document.ReplaceText("[SLUZBY]", byt.Sluzby.ToString());
                    document.ReplaceText("[NAJEMSLUZBY]", byt.NajemSluzby.ToString());
                    document.ReplaceText("[JMENO]", najemce.Jmeno.ToString());
                    document.ReplaceText("[TEL]", najemce.Telefon.ToString());

                    if(nemovId == 2 ||  nemovId == 3)
                        document.ReplaceText("<!-PODPIS->", "Mgr. Lenka Bradáčová");
                    else
                        document.ReplaceText("<!-PODPIS->", " ");


                    document.SaveAs(ms);
                }
                byte[] fileContent = ms.ToArray();
                return File(fileContent, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", nazevSouboru);
            }
        }

        public IActionResult OSNWord(int bytId)
        {
            var byt = _context.Byty.Include(b => b.Nemovitost).Include(b => b.Najemnici).FirstOrDefault(b => b.Id == bytId);

            var nemovId = byt.NemovitostId;
            var cisloBytu = byt.Cislo;

            var najemce = byt.Najemnici.FirstOrDefault();
            var puvodniNajem = najemce?.NajemOd;
            var puvNajem = najemce?.NajemOd ?? DateTime.Now;
            var konecNajmu = puvodniNajem.HasValue ? puvodniNajem.Value.AddYears(1) : (DateTime?)null;
            DateTime datumPodpisu = DateTime.Now;
            DateTime? datumProdlouzeni = new DateTime(DateTime.Now.Year, puvNajem.Month, puvNajem.Day);
            datumProdlouzeni = datumProdlouzeni.Value.AddYears(1);
            string jmenaNajemcu = string.Join(", ", byt.Najemnici.Where(n => n.RoleNajemnika == Models.Role.Najemnik).Select(n => n.Jmeno));
            string nazevSouboru = $"OznameniSkonceniNajmu_{najemce.Jmeno.Replace(" ", "_").Replace(",", "")}.docx";
            string templatePath = Path.Combine(_environment.WebRootPath, "Templates", "OznameniSkonceniNajmu.docx");
            int lastDayMonth = DateTime.DaysInMonth(datumPodpisu.Year, datumPodpisu.Month);
            DateTime konecMesice = new DateTime(datumPodpisu.Year, datumPodpisu.Month, lastDayMonth);
            int snizKauce = byt.Kauce - 5000;

            StringBuilder najemnikInfo = new StringBuilder();
            foreach (var n in byt.Najemnici)
            {
                if (n.RoleNajemnika == Models.Role.Najemnik)
                {
                    najemnikInfo.AppendLine(n.Jmeno);
                    if (n.DatumNarozeni.HasValue)
                    {
                        najemnikInfo.AppendLine($"Datum narození: {n.DatumNarozeni?.ToString("dd.MM.yyyy")}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.RodneCislo))
                    {
                        najemnikInfo.AppendLine($"Rodné číslo: {n.RodneCislo}");
                    }
                    najemnikInfo.AppendLine(" ");
                }
            }


            using (MemoryStream ms = new MemoryStream())
            {
                using (var document = DocX.Load(templatePath))
                {
                    if (nemovId == 2 || 3 == nemovId)
                    {
                        document.ReplaceText("[MAJITELE]", "Mgr. Lenka Bradáčová, Michal Bradáč a Jan Bradáč");
                    }
                    else
                    {
                        document.ReplaceText("[MAJITELE]", "Michal Bradáč a Jan Bradáč");
                    }

                    var najemnikPlaceholder = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[NAJEMNIK]"));
                    if (najemnikPlaceholder != null)
                    {
                        var najemnikParagraph = najemnikPlaceholder.InsertParagraphAfterSelf(najemnikInfo.ToString()).Font("Aptos").FontSize(12);
                        najemnikParagraph.Alignment = Alignment.left;

                        najemnikPlaceholder.Remove(false);
                    }

                    document.ReplaceText("[NAJEMOD]", najemce.NajemOd?.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[CISLOBYTU]", byt.Cislo.ToString());
                    document.ReplaceText("[ADRESA]", byt.Nemovitost.Adresa.ToString());
                    document.ReplaceText("[PATRO]", byt.Patro.ToString());
                    document.ReplaceText("[CISLOPOPISNE]", byt.Nemovitost.CisloPopisne.ToString());
                    document.ReplaceText("[PARCELA]", byt.Nemovitost.Parcela.ToString());
                    document.ReplaceText("[OBEC]", byt.Nemovitost.Obec.ToString());
                    document.ReplaceText("[LV]", byt.Nemovitost.LV.ToString());
                    document.ReplaceText("[DATUM]", datumPodpisu.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[KONECNAJMU]", konecMesice.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[KAUCE]", byt.Kauce.ToString());
                    document.ReplaceText("[ROZLOHA]", byt.Rozloha.ToString());
                    document.ReplaceText("[DISPOZICE]", byt.Mistnosti.ToString());
                    document.ReplaceText("[NAJEM]", byt.Najem.ToString());
                    document.ReplaceText("[SLUZBY]", byt.Sluzby.ToString());
                    document.ReplaceText("[NAJEMSLUZBY]", byt.NajemSluzby.ToString());


                    document.SaveAs(ms);
                }
                byte[] fileContent = ms.ToArray();
                return File(fileContent, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", nazevSouboru);
            }
        }

        public IActionResult ZNWord(int bytId)
        {
            var byt = _context.Byty.Include(b => b.Nemovitost).Include(b => b.Najemnici).FirstOrDefault(b => b.Id == bytId);

            var nemovId = byt.NemovitostId;
            var cisloBytu = byt.Cislo;

            var najemce = byt.Najemnici.FirstOrDefault();
            var puvodniNajem = najemce?.NajemOd;
            var puvNajem = najemce?.NajemOd ?? DateTime.Now;
            var konecNajmu = puvodniNajem.HasValue ? puvodniNajem.Value.AddYears(1) : (DateTime?)null;
            DateTime datumPodpisu = DateTime.Now;
            DateTime? datumProdlouzeni = new DateTime(DateTime.Now.Year, puvNajem.Month, puvNajem.Day);
            datumProdlouzeni = datumProdlouzeni.Value.AddYears(1);
            string jmenaNajemcu = string.Join(", ", byt.Najemnici.Where(n => n.RoleNajemnika == Models.Role.Najemnik).Select(n => n.Jmeno));
            string nazevSouboru = $"ZvyseniNajmu_{najemce.Jmeno.Replace(" ", "_").Replace(",", "")}.docx";
            string templatePath = Path.Combine(_environment.WebRootPath, "Templates", "ZvyseniNajmu.docx");
            int lastDayMonth = DateTime.DaysInMonth(datumPodpisu.Year, datumPodpisu.Month);
            DateTime konecMesice = new DateTime(datumPodpisu.Year, datumPodpisu.Month, lastDayMonth);
            int snizKauce = byt.Kauce - 5000;

            StringBuilder najemnikInfo = new StringBuilder();
            foreach (var n in byt.Najemnici)
            {
                if (n.RoleNajemnika == Models.Role.Najemnik)
                {
                    najemnikInfo.AppendLine(n.Jmeno);
                    if (n.DatumNarozeni.HasValue)
                    {
                        najemnikInfo.AppendLine($"Datum narození: {n.DatumNarozeni?.ToString("dd.MM.yyyy")}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.RodneCislo))
                    {
                        najemnikInfo.AppendLine($"Rodné číslo: {n.RodneCislo}");
                    }
                    najemnikInfo.AppendLine(" ");
                }
            }

            int prirazka = byt.Najem / 10;
            int novyNajem = byt.Najem + prirazka;
            int novyCelkem = novyNajem + byt.Sluzby;


            using (MemoryStream ms = new MemoryStream())
            {
                using (var document = DocX.Load(templatePath))
                {
                    if (nemovId == 2 || 3 == nemovId)
                    {
                        document.ReplaceText("[MAJITELE]", "Mgr. Lenka Bradáčová, Michal Bradáč a Jan Bradáč");
                    }
                    else
                    {
                        document.ReplaceText("[MAJITELE]", "Michal Bradáč a Jan Bradáč");
                    }

                    var najemnikPlaceholder = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[NAJEMNIK]"));
                    if (najemnikPlaceholder != null)
                    {
                        var najemnikParagraph = najemnikPlaceholder.InsertParagraphAfterSelf(najemnikInfo.ToString()).Font("Aptos").FontSize(12);
                        najemnikParagraph.Alignment = Alignment.left;

                        najemnikPlaceholder.Remove(false);
                    }

                    document.ReplaceText("[NAJEMOD]", najemce.NajemOd?.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[CISLOBYTU]", byt.Cislo.ToString());
                    document.ReplaceText("[ADRESA]", byt.Nemovitost.Adresa.ToString());
                    document.ReplaceText("[PATRO]", byt.Patro.ToString());
                    document.ReplaceText("[CISLOPOPISNE]", byt.Nemovitost.CisloPopisne.ToString());
                    document.ReplaceText("[PARCELA]", byt.Nemovitost.Parcela.ToString());
                    document.ReplaceText("[OBEC]", byt.Nemovitost.Obec.ToString());
                    document.ReplaceText("[LV]", byt.Nemovitost.LV.ToString());
                    document.ReplaceText("[DATUM]", datumPodpisu.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[KONECNAJMU]", konecMesice.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[KAUCE]", byt.Kauce.ToString());
                    document.ReplaceText("[ROZLOHA]", byt.Rozloha.ToString());
                    document.ReplaceText("[DISPOZICE]", byt.Mistnosti.ToString());
                    document.ReplaceText("[NAJEM]", byt.Najem.ToString());
                    document.ReplaceText("[SLUZBY]", byt.Sluzby.ToString());
                    document.ReplaceText("[NAJEMSLUZBY]", byt.NajemSluzby.ToString());
                    document.ReplaceText("[JMENO]", najemce.Jmeno.ToString());
                    document.ReplaceText("[NOVYNAJ]", novyNajem.ToString());
                    document.ReplaceText("[NOVYCELKEM]", novyCelkem.ToString());


                    document.SaveAs(ms);
                }
                byte[] fileContent = ms.ToArray();
                return File(fileContent, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", nazevSouboru);
            }
        }
        public IActionResult DVWord(int bytId)
        {
            var byt = _context.Byty.Include(b => b.Nemovitost).Include(b => b.Najemnici).FirstOrDefault(b => b.Id == bytId);

            var nemovId = byt.NemovitostId;
            var cisloBytu = byt.Cislo;

            var najemce = byt.Najemnici.FirstOrDefault();
            var puvodniNajem = najemce?.NajemOd;
            var puvNajem = najemce?.NajemOd ?? DateTime.Now;
            var konecNajmu = puvodniNajem.HasValue ? puvodniNajem.Value.AddYears(1) : (DateTime?)null;
            DateTime datumPodpisu = DateTime.Now;
            DateTime? datumProdlouzeni = new DateTime(DateTime.Now.Year, puvNajem.Month, puvNajem.Day);
            datumProdlouzeni = datumProdlouzeni.Value.AddYears(1);
            string jmenaNajemcu = string.Join(", ", byt.Najemnici.Where(n => n.RoleNajemnika == Models.Role.Najemnik).Select(n => n.Jmeno));
            string nazevSouboru = $"Dohoda_o_Vyklizeni_{najemce.Jmeno.Replace(" ", "_").Replace(",", "")}.docx";
            string templatePath = Path.Combine(_environment.WebRootPath, "Templates", "DohodaVyklizeni.docx");
            int lastDayMonth = DateTime.DaysInMonth(datumPodpisu.Year, datumPodpisu.Month);
            DateTime konecMesice = new DateTime(datumPodpisu.Year, datumPodpisu.Month, lastDayMonth);
            int snizKauce = byt.Kauce - 5000;

            StringBuilder najemnikInfo = new StringBuilder();
            foreach (var n in byt.Najemnici)
            {
                if (n.RoleNajemnika == Models.Role.Najemnik)
                {
                    najemnikInfo.AppendLine(n.Jmeno);
                    if (n.DatumNarozeni.HasValue)
                    {
                        najemnikInfo.AppendLine($"Datum narození: {n.DatumNarozeni?.ToString("dd.MM.yyyy")}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.RodneCislo))
                    {
                        najemnikInfo.AppendLine($"Rodné číslo: {n.RodneCislo}");
                    }
                    najemnikInfo.AppendLine(" ");
                }
            }

            int prirazka = byt.Najem / 10;
            int novyNajem = byt.Najem + prirazka;
            int novyCelkem = novyNajem + byt.Sluzby;


            using (MemoryStream ms = new MemoryStream())
            {
                using (var document = DocX.Load(templatePath))
                {
                    if (nemovId == 2 || 3 == nemovId)
                    {
                        document.ReplaceText("[MAJITELE]", "Mgr. Lenka Bradáčová, Michal Bradáč a Jan Bradáč");
                    }
                    else
                    {
                        document.ReplaceText("[MAJITELE]", "Michal Bradáč a Jan Bradáč");
                    }

                    var najemnikPlaceholder = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[NAJEMNIK]"));
                    if (najemnikPlaceholder != null)
                    {
                        var najemnikParagraph = najemnikPlaceholder.InsertParagraphAfterSelf(najemnikInfo.ToString()).Font("Aptos").FontSize(12);
                        najemnikParagraph.Alignment = Alignment.left;

                        najemnikPlaceholder.Remove(false);
                    }

                    document.ReplaceText("[NAJEMOD]", najemce.NajemOd?.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[CISLOBYTU]", byt.Cislo.ToString());
                    document.ReplaceText("[ADRESA]", byt.Nemovitost.Adresa.ToString());
                    document.ReplaceText("[PATRO]", byt.Patro.ToString());
                    document.ReplaceText("[CISLOPOPISNE]", byt.Nemovitost.CisloPopisne.ToString());
                    document.ReplaceText("[PARCELA]", byt.Nemovitost.Parcela.ToString());
                    document.ReplaceText("[OBEC]", byt.Nemovitost.Obec.ToString());
                    document.ReplaceText("[LV]", byt.Nemovitost.LV.ToString());
                    document.ReplaceText("[DATUM]", datumPodpisu.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[KONECNAJMU]", konecMesice.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[KAUCE]", byt.Kauce.ToString());
                    document.ReplaceText("[ROZLOHA]", byt.Rozloha.ToString());
                    document.ReplaceText("[DISPOZICE]", byt.Mistnosti.ToString());
                    document.ReplaceText("[NAJEM]", byt.Najem.ToString());
                    document.ReplaceText("[SLUZBY]", byt.Sluzby.ToString());
                    document.ReplaceText("[NAJEMSLUZBY]", byt.NajemSluzby.ToString());
                    document.ReplaceText("[JMENO]", najemce.Jmeno.ToString());
                    document.ReplaceText("[NOVYNAJ]", novyNajem.ToString());
                    document.ReplaceText("[NOVYCELKEM]", novyCelkem.ToString());


                    document.SaveAs(ms);
                }
                byte[] fileContent = ms.ToArray();
                return File(fileContent, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", nazevSouboru);
            }
        }

        public IActionResult VDWord(int bytId)
        {
            var byt = _context.Byty.Include(b => b.Nemovitost).Include(b => b.Najemnici).FirstOrDefault(b => b.Id == bytId);

            var nemovId = byt.NemovitostId;
            var cisloBytu = byt.Cislo;

            var najemce = byt.Najemnici.FirstOrDefault();
            var puvodniNajem = najemce?.NajemOd;
            var puvNajem = najemce?.NajemOd ?? DateTime.Now;
            var konecNajmu = puvodniNajem.HasValue ? puvodniNajem.Value.AddYears(1) : (DateTime?)null;
            DateTime datumPodpisu = DateTime.Now;
            DateTime? datumProdlouzeni = new DateTime(DateTime.Now.Year, puvNajem.Month, puvNajem.Day);
            datumProdlouzeni = datumProdlouzeni.Value.AddYears(1);
            string jmenaNajemcu = string.Join(", ", byt.Najemnici.Where(n => n.RoleNajemnika == Models.Role.Najemnik).Select(n => n.Jmeno));
            string nazevSouboru = $"Vystraha_{najemce.Jmeno.Replace(" ", "_").Replace(",", "")}.docx";
            string templatePath = Path.Combine(_environment.WebRootPath, "Templates", "Vystraha.docx");
            int lastDayMonth = DateTime.DaysInMonth(datumPodpisu.Year, datumPodpisu.Month);
            DateTime konecMesice = new DateTime(datumPodpisu.Year, datumPodpisu.Month, lastDayMonth);
            int snizKauce = byt.Kauce - 5000;

            StringBuilder najemnikInfo = new StringBuilder();
            foreach (var n in byt.Najemnici)
            {
                if (n.RoleNajemnika == Models.Role.Najemnik)
                {
                    najemnikInfo.AppendLine(n.Jmeno);
                    if (n.DatumNarozeni.HasValue)
                    {
                        najemnikInfo.AppendLine($"Datum narození: {n.DatumNarozeni?.ToString("dd.MM.yyyy")}");
                    }
                    if (!string.IsNullOrWhiteSpace(n.RodneCislo))
                    {
                        najemnikInfo.AppendLine($"Rodné číslo: {n.RodneCislo}");
                    }
                    najemnikInfo.AppendLine(" ");
                }
            }

            int prirazka = byt.Najem / 10;
            int novyNajem = byt.Najem + prirazka;
            int novyCelkem = novyNajem + byt.Sluzby;


            using (MemoryStream ms = new MemoryStream())
            {
                using (var document = DocX.Load(templatePath))
                {
                    if (nemovId == 2 || 3 == nemovId)
                    {
                        document.ReplaceText("[MAJITELE]", "Mgr. Lenka Bradáčová, Michal Bradáč a Jan Bradáč");
                    }
                    else
                    {
                        document.ReplaceText("[MAJITELE]", "Michal Bradáč a Jan Bradáč");
                    }

                    var najemnikPlaceholder = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[NAJEMNIK]"));
                    if (najemnikPlaceholder != null)
                    {
                        var najemnikParagraph = najemnikPlaceholder.InsertParagraphAfterSelf(najemnikInfo.ToString()).Font("Aptos").FontSize(12);
                        najemnikParagraph.Alignment = Alignment.left;

                        najemnikPlaceholder.Remove(false);
                    }

                    if (nemovId == 3)
                    {
                        document.ReplaceText("[KONTO]", "229263108/0300, vedený u ČSOB");
                    }
                    else if (nemovId == 2)
                    {
                        document.ReplaceText("[KONTO]", "133936558/0300, vedený u ČSOB");
                    }
                    else
                    {
                        document.ReplaceText("[KONTO]", "816846033/0800, vedený u České spořitelny");
                    }

                    document.ReplaceText("[NAJEMOD]", najemce.NajemOd?.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[CISLOBYTU]", byt.Cislo.ToString());
                    document.ReplaceText("[ADRESA]", byt.Nemovitost.Adresa.ToString());
                    document.ReplaceText("[PATRO]", byt.Patro.ToString());
                    document.ReplaceText("[CISLOPOPISNE]", byt.Nemovitost.CisloPopisne.ToString());
                    document.ReplaceText("[PARCELA]", byt.Nemovitost.Parcela.ToString());
                    document.ReplaceText("[OBEC]", byt.Nemovitost.Obec.ToString());
                    document.ReplaceText("[LV]", byt.Nemovitost.LV.ToString());
                    document.ReplaceText("[DATUM]", datumPodpisu.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[KONECNAJMU]", konecMesice.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[KAUCE]", byt.Kauce.ToString());
                    document.ReplaceText("[ROZLOHA]", byt.Rozloha.ToString());
                    document.ReplaceText("[DISPOZICE]", byt.Mistnosti.ToString());
                    document.ReplaceText("[NAJEM]", byt.Najem.ToString());
                    document.ReplaceText("[SLUZBY]", byt.Sluzby.ToString());
                    document.ReplaceText("[NAJEMSLUZBY]", byt.NajemSluzby.ToString());
                    document.ReplaceText("[JMENO]", najemce.Jmeno.ToString());
                    document.ReplaceText("[NOVYNAJ]", novyNajem.ToString());
                    document.ReplaceText("[NOVYCELKEM]", novyCelkem.ToString());


                    document.SaveAs(ms);
                }
                byte[] fileContent = ms.ToArray();
                return File(fileContent, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", nazevSouboru);
            }
        }
    }
}




