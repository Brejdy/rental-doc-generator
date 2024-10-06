using Microsoft.AspNetCore.Mvc;
using najemci.Data;
using Xceed.Words.NET;
using Xceed.Document.NET;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Text;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace najemci.Controllers
{
    public class WordController : Controller
    {
        private readonly ApplicationDbContext _context;
        public WordController(ApplicationDbContext context)
        {
            _context = context;
        }

        private readonly IWebHostEnvironment _environment;

        public WordController(IWebHostEnvironment environment)
        {
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
            datumProdlouzeni?.AddYears(1);
            string jmenaNajemcu = string.Join(", ", byt.Najemnici.Where(n => n.RoleNajemnika == Models.Role.Najemnik).Select(n => n.Jmeno));
            string nazevSouboru = $"DodatekSmlouvy_{najemce.Jmeno.Replace(" ", "_").Replace(",", "")}.docx";

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
                using (var document = DocX.Create(ms))
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
                    document.ReplaceText("[NAJEMNIK]", najemnikInfo.ToString());
                    document.ReplaceText("[ADRESA]", byt.Nemovitost.Adresa);
                    document.ReplaceText("[KONECNAJMU]", konecNajmu?.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[DATUMPRODLOUZENI]", datumProdlouzeni?.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[NAJEMOD]", puvNajem.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[DATUMPODPISU]", datumPodpisu.ToString("dd.MM.yyyy"));
                    document.ReplaceText("[JMENANAJEMCU]", jmenaNajemcu);
                    document.ReplaceText("[TEL]", "720 389 260");

                    if (nemovId == 2 || nemovId == 3)
                    {
                        document.ReplaceText("<!- PRIPO/PAHO ->", mamka.ToString());
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
            datumProdlouzeni?.AddYears(1);
            string jmenaNajemcu = string.Join(", ", byt.Najemnici.Where(n => n.RoleNajemnika == Models.Role.Najemnik).Select(n => n.Jmeno));
            string nazevSouboru = $"DodatekSmlouvy_{najemce.Jmeno.Replace(" ", "_").Replace(",", "")}.docx";
            string najem = CisloNaText.CisloNaSlova(byt.Najem);
            string sluzby = CisloNaText.CisloNaSlova(byt.Sluzby);
            string kauce = CisloNaText.CisloNaSlova(byt.Kauce);
            int pulNajem = (byt.NajemSluzby / 2) + byt.NajemSluzby;
            string jedenPulNajem = CisloNaText.CisloNaSlova(pulNajem);

            using (MemoryStream ms = new MemoryStream())
            {
                using (var document = DocX.Create(ms))
                {
                    document.InsertParagraph("Dodatek k nájemní smlouvě").Font("Times New Roman").FontSize(12).Alignment = Alignment.center;
                    document.InsertParagraph("podle § 2235 a násl. zákona č. 89/2012 Sb., občanský zákoník, v platném znění").Font("Times New Roman").FontSize(12).Alignment = Alignment.center;
                    document.InsertParagraph("(dále jen „Občanský zákoník“)").Font("Times New Roman").FontSize(12).Alignment = Alignment.center;
                    document.InsertParagraph("(dále jen „Dodatek“)").Font("Times New Roman").FontSize(12).Alignment = Alignment.center;
                    document.InsertParagraph("Michal Bradáč").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("datum narození: 25.1.1996").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("trvale bytem: Javorová 241, 252 44 Dolní Jirčany").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("a").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("Jan Bradáč").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("datum narození: 18.8.1998").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("trvale bytem: Javorová 241, 252 44 Dolní Jirčany").Font("Times New Roman").FontSize(12);

                    if (nemovId <= 3)
                    {
                        document.InsertParagraph("Lenka Bradáčová").Font("Times New Roman").FontSize(12);
                        document.InsertParagraph("datum narození: 22.05.1968").Font("Times New Roman").FontSize(12);
                        document.InsertParagraph("trvale bytem: Javorová 241, 252 44 Dolní Jirčany").Font("Times New Roman").FontSize(12);
                    }

                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);

                    if (nemovId >= 4)
                    {
                        document.InsertParagraph("tel.: 720 389 260").Font("Times New Roman").FontSize(12);
                        document.InsertParagraph("e-mail: nadpahorkem24@gmail.com").Font("Times New Roman").FontSize(12);
                        document.InsertParagraph($"č. účtu: 816846033/0800, vedený u České spořitelny, variabilní symbol: {cisloBytu}").Font("Times New Roman").FontSize(12);
                        document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    }
                    else if (nemovId == 3)
                    {
                        document.InsertParagraph("e-mail: nadpahorkem24@gmail.com").Font("Times New Roman").FontSize(12);
                        document.InsertParagraph($"č. účtu: 229263108/0300, vedený u ČSOB, variabilní symbol: {cisloBytu}").Font("Times New Roman").FontSize(12);
                        document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    }
                    else if (nemovId == 2)
                    {
                        document.InsertParagraph("e-mail: pripotocni31@gmail.com").Font("Times New Roman").FontSize(12);
                        document.InsertParagraph($"č. účtu: 133936558/0300, vedený u ČSOB, variabilní symbol: {cisloBytu}").Font("Times New Roman").FontSize(12);
                        document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    }

                    document.InsertParagraph("(dále též „Pronajímatel“)").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("a").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);

                    foreach (var n in byt.Najemnici)
                    {
                        if (n.RoleNajemnika == Models.Role.Najemnik)
                        {
                            if (!string.IsNullOrWhiteSpace(n.Jmeno))
                            {
                                document.InsertParagraph($"{n.Jmeno}").Font("Times New Roman").FontSize(12);
                            }
                            if (n.DatumNarozeni.HasValue)
                            {
                                document.InsertParagraph($"Datum narození: {n.DatumNarozeni?.ToString("dd.MM.yyyy")}").Font("Times New Roman").FontSize(12);
                            }
                            if (!string.IsNullOrWhiteSpace(n.RodneCislo))
                            {
                                document.InsertParagraph($"Rodné číslo: {n.RodneCislo}").Font("Times New Roman").FontSize(12);
                            }
                            if (!string.IsNullOrWhiteSpace(n.Email))
                            {
                                document.InsertParagraph($"Email: {n.Email}").Font("Times New Roman").FontSize(12);
                            }
                            if (!string.IsNullOrWhiteSpace(n.Telefon))
                            {
                                document.InsertParagraph($"Tel.: {n.Telefon}").Font("Times New Roman").FontSize(12);
                            }
                            if (n.CisloOP.HasValue)
                            {
                                document.InsertParagraph($"Číslo občanského průkazu: {n.CisloOP}").Font("Times New Roman").FontSize(12);
                            }
                            if (!string.IsNullOrWhiteSpace(n.CisloUctu))
                            {
                                document.InsertParagraph($"Číslo účtu: {n.CisloUctu}").Font("Times New Roman").FontSize(12);
                            }

                            document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                        }
                    }
                    document.InsertParagraph("PREAMBULE").Font("Times New Roman").FontSize(12).Alignment = Alignment.center;
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph($"A. Pronajímatel má ve výlučném vlastnictví byt č. {byt.Cislo}, na adrese {byt.Nemovitost.Adresa}, umístěný v {byt.Patro} v budově č.p. {byt.Nemovitost.CisloPopisne}, postavené na pozemku parc. č. {byt.Nemovitost.Parcela}, část obce {byt.Nemovitost.Obec}, zapsané v katastru nemovitostí vedeném Katastrálním úřadem pro Praha, Katastrální pracoviště Praha, pro katastrální území {byt.Nemovitost.Obec}, obec Praha na LV č. {byt.Nemovitost.LV} (dále jen „Byt“).").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    switch (byt.Mistnosti)
                    {
                        case ("1+kk"):
                            document.InsertParagraph($"B. Byt je dispozičně řešen jako {byt.Mistnosti} o podlahové ploše {byt.Rozloha} m2 a sestává z předsíně, pokoje s kuchyňskou linkou, koupelny a WC. Byt je dále označen také jako „Předmět nájmu“.").Font("Times New Roman").FontSize(12);
                            break;
                        case ("1+1"):
                            document.InsertParagraph($"B. Byt je dispozičně řešen jako {byt.Mistnosti} o podlahové ploše {byt.Rozloha} m2 a sestává z předsíně, pokoje, kuchyě, koupelny a WC. Byt je dále označen také jako „Předmět nájmu“.").Font("Times New Roman").FontSize(12);
                            break;
                        case ("2+kk"):
                            document.InsertParagraph($"B. Byt je dispozičně řešen jako {byt.Mistnosti} o podlahové ploše {byt.Rozloha} m2 a sestává z předsíně, ložnice, pokoje s kuchyňskou linkou, koupelny a WC. Byt je dále označen také jako „Předmět nájmu“.").Font("Times New Roman").FontSize(12);
                            break;
                        case ("2+1"):
                            document.InsertParagraph($"B. Byt je dispozičně řešen jako {byt.Mistnosti} o podlahové ploše {byt.Rozloha} m2 a sestává z předsíně, ložnice, obývacího pokoje, kuchyně, koupelny a WC. Byt je dále označen také jako „Předmět nájmu“.").Font("Times New Roman").FontSize(12);
                            break;
                        case ("2+0"):
                            document.InsertParagraph($"B. Byt je dispozičně řešen jako {byt.Mistnosti} o podlahové ploše {byt.Rozloha} m2 a sestává z předsíně, ložnice, obývacího pokoje, koupelny a dvou WC. Byt je dále označen také jako „Předmět nájmu“.").Font("Times New Roman").FontSize(12);
                            break;
                        case ("3+kk"):
                            document.InsertParagraph($"B. Byt je dispozičně řešen jako {byt.Mistnosti} o podlahové ploše {byt.Rozloha} m2 a sestává z předsíně, ložnice, obývacího pokoje, pokoje s kuchyňskou linkou, koupelny a WC. Byt je dále označen také jako „Předmět nájmu“.").Font("Times New Roman").FontSize(12);
                            break;
                        case ("3+1"):
                            document.InsertParagraph($"B. Byt je dispozičně řešen jako {byt.Mistnosti} o podlahové ploše {byt.Rozloha} m2 a sestává z předsíně, ložnice, dvou pokojů, kuchyně, koupelny a WC. Byt je dále označen také jako „Předmět nájmu“.").Font("Times New Roman").FontSize(12);
                            break;
                    }
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("C. Je-li na straně nájemce více osob, za závazky z této smlouvy odpovídají společně a nerozdílně.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("ČL. 1. PŘENECHÁNÍ PŘEDMĚTU NÁJMU").Font("Times New Roman").FontSize(12).Alignment = Alignment.center;
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("1.1 Pronajímatel tímto přenechává Nájemci do užívání Předmět nájmu a Nájemce do užívání Předmět nájmu přijímá. Nájemce se zavazuje, že bude Byt využívat výhradně za účelem bydlení Nájemce. Nájemce bere na vědomí, že není oprávněn Byt využívat k podnikatelské činnosti a dále není oprávněn v Bytě zřídit zejména sídla podnikatelských subjektů, místa podnikání, provozovny a podobně.\r\n").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    if (byt.Najemnici.Any(n=>n.RoleNajemnika == Models.Role.ClenDomacnosti))
                    {
                        document.InsertParagraph("1.2 Společně s nájemcem sdílí domácnost následující člen domácnosti:").Font("Times New Roman").FontSize(12);
                        document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                        foreach (var n in byt.Najemnici)
                        {
                            if (n.RoleNajemnika == Models.Role.ClenDomacnosti)
                            {
                                document.InsertParagraph($"Jméno: {n.Jmeno}").Font("Times New Roman").FontSize(12);
                                document.InsertParagraph($"Datum narození: {n.DatumNarozeni}").Font("Times New Roman").FontSize(12);
                                document.InsertParagraph($"Rodné číslo: {n.RodneCislo}").Font("Times New Roman").FontSize(12);
                                document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                                document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                            }
                        }
                        document.InsertParagraph("1.3 Pronajímatel předal Předmět nájmu Nájemci v den podpisu této Smlouvy. Předávací protokol ohledně předání Předmětu nájmu včetně soupisu zařízení a vybavení Předmětu nájmu tvoří Přílohu č. 1 této Smlouvy.").Font("Times New Roman").FontSize(12);
                        document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                        document.InsertParagraph("1.4 Nájemce tímto prohlašuje, že si před podpisem této Smlouvy podrobně prohlédl Předmět nájmu a prověřil jeho zařízení a vybavení a je mu tak znám stavebně-technický stav Předmětu nájmu. Nájemce prohlašuje, že Předmět nájmu splňuje účel nájmu podle této Smlouvy a že jej Nájemce přebírá do užívání bez výhrad.").Font("Times New Roman").FontSize(12);
                        document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    }
                    else
                    {
                        document.InsertParagraph("1.2 Pronajímatel předal Předmět nájmu Nájemci v den podpisu této Smlouvy. Předávací protokol ohledně předání Předmětu nájmu včetně soupisu zařízení a vybavení Předmětu nájmu tvoří Přílohu č. 1 této Smlouvy.").Font("Times New Roman").FontSize(12);
                        document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                        document.InsertParagraph("1.3 Nájemce tímto prohlašuje, že si před podpisem této Smlouvy podrobně prohlédl Předmět nájmu a prověřil jeho zařízení a vybavení a je mu tak znám stavebně-technický stav Předmětu nájmu. Nájemce prohlašuje, že Předmět nájmu splňuje účel nájmu podle této Smlouvy a že jej Nájemce přebírá do užívání bez výhrad.").Font("Times New Roman").FontSize(12);
                        document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    }
                    document.InsertParagraph("ČL. 2 DOBA NÁJMU").Font("Times New Roman").FontSize(12).Alignment = Alignment.center;
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph($"1. Nájem se uzavírá na dobu určitou od {najemce.NajemOd?.ToString("dd.MM.yyyy")} do {konecNajmu?.ToString("dd.MM.yyyy")}. Po uplynutí této doby se nájemce zavazuje byt bez náhrady vyklidit, nebude-li dohodou smluvních stran stanoveno jinak.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    if (nemovId == 2)
                    {
                        document.InsertParagraph($"2. Smluvní strany sjednávají, že prodloužení nájmu bytu je možné jen za podmínek dále stanovených v tomto odstavci smlouvy. Smluvní strany se dohodly, že nájem bytu se automaticky prodlužuje vždy o šest měsíců do dne 28. února a 31. srpna příslušného kalendářního roku, a to i opakovaně, pokud smluvní strana nesdělí druhé smluvní straně, svým oznámením písemně nebo prostřednictvím mailu ({najemce.Email}, pripotocni31@gmail.com) došlým druhé smluvní straně nejpozději v dvouměsíčním předstihu, resp. do dne 31. prosince nebo 30. června příslušného kalendářního roku, že na dalším pokračování nájmu bytu nemá zájem. Ustanovením tohoto odstavce smlouvy smluvní strany výslovně vylučují zákonnou úpravu prolongace nájmu bytu zakotvenou v ustanovení § 2285 občanského zákoníku.").Font("Times New Roman").FontSize(12);
                    }
                    else
                    {
                        document.InsertParagraph($"2. Smluvní strany sjednávají, že prodloužení nájmu bytu je možné jen za podmínek dále stanovených v tomto odstavci smlouvy. Smluvní strany se dohodly, že nájem bytu se automaticky prodlužuje vždy o šest měsíců do dne 28. února a 31. srpna příslušného kalendářního roku, a to i opakovaně, pokud smluvní strana nesdělí druhé smluvní straně, svým oznámením písemně nebo prostřednictvím mailu ({najemce.Email}, nadpahorkem24@gmail.com) došlým druhé smluvní straně nejpozději v dvouměsíčním předstihu, resp. do dne 31. prosince nebo 30. června příslušného kalendářního roku, že na dalším pokračování nájmu bytu nemá zájem. Ustanovením tohoto odstavce smlouvy smluvní strany výslovně vylučují zákonnou úpravu prolongace nájmu bytu zakotvenou v ustanovení § 2285 občanského zákoníku.").Font("Times New Roman").FontSize(12);
                    }
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("Před uplynutím Doby nájmu může nájem skončit:").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("a) na základě písemné dohody Smluvních stran; nebo").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("b) na základě výpovědi podané Pronajímatelem uplynutím tříměsíční výpovědní lhůty nebo, v zákonem předvídaných případech, i bez výpovědní lhůty; nebo").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("c) na základě výpovědi podané Nájemcem uplynutím tříměsíční výpovědní lhůty nebo, v zákonem předvídaných případech, i bez výpovědní lhůty; nebo").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("d) v jiných případech stanovených zákonem.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("Výpovědní doba běží od prvního dne kalendářního měsíce následujícího poté, co byla výpověď doručena druhé Smluvní straně. Vypoví-li nájem Pronajímatel, poučí Nájemce o jeho právu vznést proti výpovědi námitky a o možnosti přezkumu oprávněnosti výpovědi.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("2.2 Smluvní strany sjednávají, že za hrubé porušení povinnosti Nájemce vyplývající z nájmu bytu, pro které může Pronajímatel tuto Smlouvu vypovědět, Smluvní strany budou považovat mj. i to, že Nájemce nebo ti, kdo s ním bydlí, přes písemnou výstrahu hrubě porušuje dobré mravy, klid a pořádek v domě, nebo že je Nájemce o více než 1 měsíc v prodlení s jakoukoli platbou dle této Smlouvy a dlužnou částku neuhradí ani ve lhůtě 10 dnů od obdržení výzvy Pronajímatele k nápravě, nebo že Nájemce dá neoprávněně Předmět nájmu do podnájmu třetí osobě nebo jí jinak neoprávněně užívání Předmětu nájmu umožní, nebo že Nájemce nebo ti, kdo s ním bydlí, užívá Předmět nájmu, resp. jeho zařízení a vybavení, v rozporu s touto Smlouvou nebo způsobem, který vede ke vzniku škody (Pronajímateli nebo třetí osobě) nebo k objektivnímu riziku vzniku škody (Pronajímateli nebo třetí osobě)  v hodnotě přesahující Kauci definovanou níže, nebo že Nájemce bez předchozího písemného souhlasu Pronajímatele provádí stavební nebo jiné změny v Předmětu nájmu.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("2.3 Nájemce se zavazuje Předmět nájmu vyklidit a vyklizený předat Pronajímateli nejpozději v den skončení nájmu dle této Smlouvy, tj. k poslednímu dni výpovědní lhůty, ke dni stanovenému dohodou Smluvních stran, příp. k poslednímu dni Doby nájmu. Nájemce je povinen předat Předmět nájmu Pronajímateli ve stavu, v jakém jej převzal (čistý, uklizený a vymalovaný) s přihlédnutím k obvyklému opotřebení při řádném užívání a údržbě. O předání Předmětu nájmu se Smluvní strany zavazují sepsat protokol, ve kterém zejména uvedou případné škody způsobené Nájemcem. V případě, že Pronajímatel udělí souhlas s přihlášením trvalého pobytu Nájemce nebo dalších osob v Předmětu nájmu, je Nájemce povinen Pronajímateli doložit, že všechny osoby odhlásily svůj trvalý pobyt z Předmětu nájmu nejpozději v den skončení nájmu.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("2.4 Pronajímatel má právo na náhradu ve výši ujednaného Nájemného, neodevzdá-li Nájemce Předmět nájmu Pronajímateli v den skončení nájmu až do dne, kdy Nájemce Pronajímateli Předmět nájmu skutečně odevzdá. Tím není dotčeno případné právo Pronajímatele na náhradu škody. V případě, že Nájemce nesplní svou povinnost Pronajímateli doložit, že všechny osoby odhlásily svůj trvalý pobyt z Předmětu nájmu nejpozději v den skončení nájmu, nepovažuje se Předmět nájmu za řádně odevzdaný Pronajímateli, i kdyby byl Pronajímateli Předmět nájmu předán.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("2.5 Po skončení nájmu je Pronajímatel oprávněn Předmět nájmu vyklidit na náklady Nájemce.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("ČL. 3 NÁJEMNÉ A ÚHRADA ZA PLNĚNÍ POSKYTOVANÁ S UŽÍVÁNÍM PŘEDMĚTU NÁJMU").Font("Times New Roman").FontSize(12).Alignment = Alignment.center;
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph($"3.1 Nájemné za užívání Předmětu nájmu činí {byt.Najem},- Kč (slovy: {najem} korun českých) měsíčně (dále jen „Nájemné“). Nájemné je splatné měsíčně vždy nejpozději do každého 5. dne měsíce, za který se Nájemné platí, a to na bankovní účet Pronajímatele uvedený v záhlaví této Smlouvy pod variabilním symbolem platby {byt.Cislo}. Nájemné se považuje za uhrazené v den, kdy je částka Nájemného připsána účet Pronajímatele.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("3.2 Pronajímatel je oprávněn zvýšit nájemné jednostranně písemným oznámením adresovaným nájemci, avšak nejvýše o míru inflace vyhlášenou Českým statistickým úřadem nebo jeho nástupnickou organizací. Zvýšení nájemného je účinné počínaje prvním nájemným splatným po doručení oznámení podle předchozí věty nájemci. Toto ustanovení platí pouze v případě, že doba nájmu je sjednaná na delší časové období než jeden rok, nebo dojde-li k prodloužení doby nájmu, kde může, ale nemusí zvýšit výši nájemného.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph($"3.3 Smluvní strany sjednávají, že Pronajímatel nebude Nájemci zajištovat žádná plnění ani služby, s výjimkami uvedenými v tomto odstavci. Pronajímatel bude zajištovat pouze služby dodané prostřednictvím SVJ: provoz, elektřina společných prostor, elektřina výtah, úklid, odměny orgánů SVJ, správní činnost, správa domu, pojištění nemovitosti, odvoz domovního odpadu, studená voda, ústřední topení, teplá voda, teplo pro teplou vodu, ZOBF, přičemž služby: poplatek za správu, ZOBF a vybavení bytu se platí paušálem, nezúčtovávají se. Zálohy na poplatky za uvedené služby budou hrazeny spolu s Nájemným na účet Pronajímatele uvedený v záhlaví této Smlouvy, měsíční záloha činí {byt.Sluzby},- Kč (slovy: {sluzby} Korun českých). Smlouvy s dodavateli/poskytovateli ostatních služeb, zejména elektřiny, datových služeb a rozhlasového a televizního vysílání, uzavře Nájemce svým jménem a platby za tyto služby bude Nájemce hradit přímo jednotlivým dodavatelům/poskytovatelům. Pronajímatel se zavazuje poskytnout Nájemci veškerou rozumně požadovanou součinnost k uzavření příslušných smluv s takovými dodavateli/poskytovateli.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("3.4 Pronajímatel vyúčtuje Nájemci poskytované služby za uplynulý kalendářní rok nejpozději do 30.06. Případné nedoplatky je Nájemce povinen uhradit Pronajímateli do 14 dnů ode dne doručení vyúčtování na účet Pronajímatele uvedený v záhlaví této Smlouvy. Případné přeplatky vyplatí Pronajímatel ve stejné lhůtě na účet Nájemce uvedený v záhlaví této Smlouvy. Povinnost Smluvních stran uhradit rozdíl dle vyúčtování ukončením účinnosti této Smlouvy nezaniká. Pronajímatel provede vyúčtování podle skutečné výše nákladů na služby účtované jemu třetími osobami bez jakékoliv přirážky (přitom náklady na teplo, vodné a stočné se vypočítávají dle skutečné spotřeby a náklady na správu, odvoz odpadu, úklid společných prostor domu a osvětlení veřejných prostor domu se rozpočítávají dle počtu osob v budově). Pronajímatel je, na základě provedeného vyúčtování a nebo zvýšení cen služeb zajišťovaných Pronajímatelem, oprávněn adekvátně zvýšit zálohy za služby na stávající kalendářní rok. Nájemce je v takovém případě povinen hradit zvýšené zálohy ode dne, kdy mu Pronajímatel oznámil novou výši záloh za služby.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph($"3.5 Před podpisem této Smlouvy Nájemce složil k rukám Pronajímatele částku ve výši částku {byt.Kauce} Kč (slovy: {kauce} korun českých) jako peněžitou jistotu (dále jen „Kauce“). Dále složil částku ve výši jeden a půl nájemného spolu se službami, tj. částku {pulNajem} Kč (slovy: {jedenPulNajem} korun českých). Kauce je určena k úhradě případných škod zaviněných Nájemcem na Předmětu nájmu či jeho vybavení a zařízení, nebo k úhradě případných nedoplatků ze strany Nájemce vůči Pronajímateli v souvislosti s Předmětem nájmu a touto Smlouvou. Pronajímatel je oprávněn čerpat částku odpovídající jeho pohledávce z Kauce, pokud tato částka nebyla Nájemcem uhrazena do 5 dnů od výzvy Pronajímatele k úhradě případného dluhu. Pokud bude z Jistoty v průběhu trvání nájemního vztahu čerpáno, je Nájemce na výzvu Pronajímatele povinen doplnit peněžní prostředky do původní výše, a to ve lhůtě do jednoho týdne od dojití písemné výzvy. Po skončení nájemního vztahu bude Jistota nebo její nevyčerpaná část vrácena Nájemci ve lhůtě do jednoho měsíce ode dne vyklizení předmětu nájmu a jeho protokolárního předání zpět Pronajímateli, jakož i splnění všech podmínek. Nájemce po ukončení nájmu odevzdá Předmět nájmu Pronajímateli řádně a včas, bez poškození Předmětu nájmu a jeho vybavení a zařízení, a zároveň Nájemce nebude mít z této Smlouvy žádné nedoplatky, bude Kauce Nájemci vrácení v plné výši, a to nejpozději do 1 měsíce po skončení nájmu. Strany prohlašují, že úročení účtu úschov bylo zohledněno při sjednané výši nájemného").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("ČL. 4 PRÁVA A POVINNOSTI PRONAJÍMATELE").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("4.1 Pronajímatel je povinen předat Nájemci Byt ve stavu způsobilém k řádnému užívání a zajistit Nájemci plný a nerušený výkon práv spojených s užíváním Bytu, a to po celou dobu trvání nájmu podle této Smlouvy. Pronajímatel však neodpovídá za nedostatky a výpadky při poskytování služeb poskytovaných Pronajímatelem, pokud nebyly způsobeny z důvodů na straně Pronajímatele.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("4.2 Za účelem kontroly dodržování povinností Nájemce podle této Smlouvy a/nebo kontroly stavu Předmětu nájmu a/nebo za účelem provedení odečtů vody nebo jiných médií je Pronajímatel nebo jím pověřená osoba oprávněn vstoupit do Bytu, a to po předchozím oznámení Pronajímatelem alespoň 3 dny předem, anebo v termínu a čase stanoveném Pronajímatelem po projednání s Nájemcem. Předchozí oznámení se nevyžaduje, je-li nezbytné zabránit škodě nebo hrozí-li nebezpečí z prodlení.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("4.3 Pronajímatel nebo jeho zástupce mají právo navštívit Byt společně s potencionálními budoucími nájemníky během posledních 3 měsíců trvání Doby nájmu. Pronajímatel je povinen oznámit Nájemci takovouto návštěvu s přiměřeným předstihem.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("4.4 Opravy Předmětu nájmu (vyjma těch, ke kterým je povinen Nájemce) bude zařizovat Pronajímatel na vlastní náklady v přiměřené lhůtě, pod podmínkou, že tyto závady nebyly způsobeny nevhodným užíváním nebo úmyslným poškozením Nájemcem, resp. osob, kterým Nájemce užívání Předmětu nájmu umožnil. V takovémto případě bude oprava provedena na náklady Nájemce.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("4.5 Pronajímatel neodpovídá za ztráty nebo škody na majetku vneseném do Předmětu nájmu Nájemcem a osobami užívajícími Předmět nájmu, pokud ztrátu nebo vznik škody nezavinil.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("4.6 Pronajímatel si vyhrazuje právo mít klíče od Předmětu nájmu pro případ nutného vstupu do Předmětu nájmu, tj. například v případě havárie.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("ČL. 5 PRÁVA A POVINNOSTI NÁJEMCE").Font("Times New Roman").FontSize(12).Alignment = Alignment.center;
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("5.1 Nájemce se zavazuje řádně užívat Předmět nájmu včetně převzatého vybavení pouze pro svou vlastní potřebu pro účely bydlení, tj. obvyklým způsobem, přiměřeně charakteru Předmětu nájmu a tak, aby neomezoval ostatní obyvatele domu ve výkonu jejich práv (zejména hlukem, zápachem, vibracemi, prachem apod.). Nájemce se zavazuje dodržovat domovní řád a pravidla obvyklá pro chování v domě a dále bezpečnostní, hygienické a protipožární předpisy a ostatní relevantní právní předpisy a zajistit dodržování výše uvedených povinností též ze strany všech osob, kterým Nájemce umožní do Předmětu nájmu a domu přístup.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("5.2 Nájemce je povinen udržovat Předmět nájmu v čistotě, v dobrém a uživatelném stavu, dbát na to, aby veškeré jím užívané instalace a zařízení včetně připojených spotřebičů byly v naprostém pořádku a provozuschopném stavu podle platných předpisů a aby na majetku Pronajímatele nevznikla škoda.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("5.3 Pokud se Nájemce zdržuje delší dobu mimo Byt je povinen postarat se o to, aby Byt zajistil proti poškození, zejména vypnul hlavní přívod elektřiny do Bytu a uzavřel přívod vody. Ví-li Nájemce předem o své nepřítomnosti v Bytě, která má být delší než 2 měsíce, i o tom, že Byt mu bude po tuto dobu obtížně dostupný, oznámí to včas Pronajímateli a určí osobu, která v případě potřeby umožní přístup do Bytu.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("5.4 Nájemce bude zajišťovat na svůj náklad úklid, drobné opravy a běžnou údržbu Předmětu nájmu a jeho zařízení a vnitřního vybavení; drobné opravy, běžná údržba a jejich rozsah jsou definovány v nařízení vlády České republiky č. 308/2015 Sb.  v platném znění.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("5.5 Nájemce je povinen bezodkladně písemně oznámit Pronajímateli jakékoli poškození Předmětu nájmu a/nebo potřebu oprav Předmětu nájmu a umožnit jejich provedení. Tato povinnost se netýká drobných oprav a běžné údržby Bytu, ke kterým je povinen Nájemce. Nájemce je povinen snášet omezení v užívání Předmětu nájmu v rozsahu nutném pro provedení oprav Předmětu nájmu.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("5.6 Nájemce je povinen bezodkladně, nejpozději do 10 dnů ode dne, kdy jej k tomu Pronajímatel vyzve, odstranit závady a poškození, které způsobil na Předmětu nájmu sám nebo ti, kdo s ním v Předmětu nájmu bydlí, či osoby, kterým Nájemce umožní do Předmětu nájmu přístup. Nestane-li se tak, má právo Pronajímatel po předchozím upozornění Nájemce závady a poškození odstranit na svůj náklad sám a požadovat od Nájemce náhradu skutečně vynaložených nákladů.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("5.7 Nájemce se zavazuje, že bez písemného souhlasu Pronajímatele nebude provádět žádné stavební nebo jiné změny v Předmětu nájmu a společných prostorách domu.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("5.8 Nájemce se zavazuje písemně oznámit Pronajímateli bez zbytečného odkladu jakoukoli změnu v počtu osob, které s ním budou Byt užívat. Nájemce je vždy povinen zajistit, aby počet osob, které s ním budou Byt užívat, byl přiměřený velikosti Bytu a nebránil tomu, aby všechny tyto osoby mohly Byt řádně užívat za obvyklých a hygienicky vyhovujících podmínek. Bez ohledu na výše uvedené, Nájemce není oprávněn přenechat Předmět nájmu nebo jeho část do podnájmu třetí osobě nebo jí jinak umožnit užívání Předmětu nájmu ani takovou osobu přijmout za člena své domácnosti bez předchozího písemného souhlasu Pronajímatele.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("5.9 Nájemce není oprávněn provádět jakékoliv změny v Předmětu nájmu a na jeho vybavení a zařízení bez předchozího písemného souhlasu Pronajímatele.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("5.10 Nájemce je povinen zřídit si a po celou Dobu nájmu udržovat pojištění domácnosti ve vztahu k Předmětu nájmu a jeho vybavení jakož i pojištění odpovědnosti za škodu, a to do výše 1.000.000,-- Kč (slovy: jeden milion korun českých).").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("5.11 Nájemce je povinen umožnit Pronajímateli nebo jím určeným osobám nezbytný přístup do Předmětu nájmu, a to zejména za účelem kontroly stavu Předmětu nájmu, dodržování povinností Nájemce podle této Smlouvy, a/nebo za účelem provedení odečtů vody nebo jiných médií, či v dalších obdobných případech.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("5.12 Nájemce je povinen platit Nájemné, zálohy na služby poskytované Pronajímatelem i veškeré další platby dle této Smlouvy řádně a včas. Nezaplatí-li Nájemce řádně a včas Nájemné a/nebo zálohu na služby poskytované Pronajímatelem a/nebo nedoplatek z vyúčtování na služby poskytované Pronajímatelem či jakoukoli jejich část a/nebo jakoukoli jinou platbu, ke které je dle této Smlouvy povinen, je povinen zaplatit Pronajímateli úrok z prodlení ve výši stanovené nařízením vlády České republiky č. 351/2013 Sb.; tím není dotčen nárok Pronajímatele na případnou náhradu škody.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("5.13 Nájemce se zavazuje v plném rozsahu odškodnit Pronajímatele za jakoukoli újmu vzniklou Pronajímateli v důsledku zaviněného jednání či opomenutí Nájemce, osob žijících s Nájemcem v Bytě či jakýchkoli jiných osob, kterým Nájemce do Bytu či domu umožnil přístup.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("5.14 Nájemce není bez předchozího písemného souhlasu Pronajímatele oprávněn započíst jakékoli své pohledávky za Pronajímatelem vůči pohledávkám Pronajímatele podle této Smlouvy.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("ČL. 6 ZÁVĚREČNÁ USTANOVENÍ").Font("Times New Roman").FontSize(12).Alignment = Alignment.center;
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("6.1 Smlouva nabývá platnosti a účinnosti dnem jejího podpisu oběma Smluvními stranami.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("6.2 Veškeré právní vztahy vzniklé na základě této Smlouvy se řídí příslušnými ustanoveními Občanského zákoníku v platném znění, a ostatními obecně platnými právními předpisy České republiky a českým právním řádem.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("6.3 Změny a doplňky této Smlouvy lze provést pouze formou písemných dodatků učiněných na základě dohody Smluvních stran.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("6.4 Tato Smlouva byla sepsána ve dvou vyhotoveních v českém jazyce, z nichž každá Smluvní strana obdrží po jednom.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("6.5 Smluvní strany prohlašují, že si Smlouvu řádně přečetly, seznámily se s jejím obsahem, a že Smlouva vyjadřuje jejich pravou a svobodnou vůli, je uzavírána určitě a vážně, a nikoliv v tísni za nápadně nevýhodných podmínek, na důkaz čehož připojují své podpisy.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("6.6 Tato Smlouva má následující přílohy:").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph("Příloha č. 1 - předávací protokol včetně soupisu zařízení a vybavení Předmětu nájmu.").Font("Times New Roman").FontSize(12);
                    document.InsertParagraph(" ").Font("Times New Roman").FontSize(12);

                    var podpisovaTabulka = document.AddTable(2, 2);
                    podpisovaTabulka.Alignment = Alignment.center;
                    podpisovaTabulka.SetWidths(new float[] { 300f, 300f });

                    podpisovaTabulka.SetBorder(TableBorderType.InsideH, new Border(BorderStyle.Tcbs_none, 0, 0, Color.White));
                    podpisovaTabulka.SetBorder(TableBorderType.InsideV, new Border(BorderStyle.Tcbs_none, 0, 0, Color.White));
                    podpisovaTabulka.SetBorder(TableBorderType.Top, new Border(BorderStyle.Tcbs_none, 0, 0, Color.White));
                    podpisovaTabulka.SetBorder(TableBorderType.Bottom, new Border(BorderStyle.Tcbs_none, 0, 0, Color.White));
                    podpisovaTabulka.SetBorder(TableBorderType.Left, new Border(BorderStyle.Tcbs_none, 0, 0, Color.White));
                    podpisovaTabulka.SetBorder(TableBorderType.Right, new Border(BorderStyle.Tcbs_none, 0, 0, Color.White));

                    podpisovaTabulka.Rows[0].Cells[0].Paragraphs[0].Append("__________________________________").Alignment = Alignment.center;

                    podpisovaTabulka.Rows[0].Cells[1].Paragraphs[0].Append("__________________________________").Alignment = Alignment.center;

                    if (nemovId == 4 || nemovId == 5)
                    {
                        podpisovaTabulka.Rows[1].Cells[0].Paragraphs[0].Append("Michal Bradáč a Jan Bradáč").Font("Times New Roman").FontSize(12).Alignment = Alignment.center;
                    }
                    else
                    {
                        podpisovaTabulka.Rows[1].Cells[0].Paragraphs[0].Append("Lenka Bradáčová, Michal Bradáč a Jan Bradáč").Font("Times New Roman").FontSize(12).Alignment = Alignment.center;
                    }

                    podpisovaTabulka.Rows[1].Cells[1].Paragraphs[0].Append(jmenaNajemcu).Font("Times New Roman").FontSize(12).Alignment = Alignment.center;

                    document.InsertParagraph(" ").SpacingAfter(20);
                    document.InsertTable(podpisovaTabulka);

                    document.Save();
                }

                byte[] fileContent = ms.ToArray();
                return File(fileContent, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", nazevSouboru);
            }
        }
    }
}