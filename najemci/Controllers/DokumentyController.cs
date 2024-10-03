using Microsoft.AspNetCore.Mvc;
using najemci.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Microsoft.EntityFrameworkCore;
using najemci.Models;
using iTextSharp.text.pdf.draw;
using Xceed.Words.NET;


namespace najemci.Controllers
{
    public class DokumentyController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DokumentyController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult DPNPdf(int bytId)
        {
            var byt = _context.Byty.Include(b => b.Nemovitost).Include(b => b.Najemnici).FirstOrDefault(b => b.Id == bytId);

            if (byt == null)
            {
                return NotFound();
            }

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
            string nazevSouboru = $"DodatekSmlouvy_{najemce.Jmeno.Replace(" ", "_").Replace(",", "")}.pdf";

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document();
                try
                {
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    document.Open();

                    BaseFont baseFont = BaseFont.CreateFont("C:/Windows/Fonts/times.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    Font font = new Font(baseFont, 12, Font.NORMAL);

                    document.Add(new Paragraph("Dodatek k nájemní smlouvě", font) { Alignment = Element.ALIGN_CENTER });
                    document.Add(new Paragraph("podle § 2235 a násl. zákona č. 89/2012 Sb., občanský zákoník, v platném znění", font) { Alignment = Element.ALIGN_CENTER });
                    document.Add(new Paragraph("(dále jen „Občanský zákoník“)", font) { Alignment = Element.ALIGN_CENTER });
                    document.Add(new Paragraph("(dále jen „Dodatek“)", font) { Alignment = Element.ALIGN_CENTER });
                    document.Add(new Paragraph("Michal Bradáč", font));
                    document.Add(new Paragraph("datum narození: 25.1.1996", font));
                    document.Add(new Paragraph("trvale bytem: Javorová 241, 252 44 Dolní Jirčany", font));
                    document.Add(new Paragraph("a", font));
                    document.Add(new Paragraph("Jan Bradáč", font));
                    document.Add(new Paragraph("datum narození: 18.8.1998", font));
                    document.Add(new Paragraph("trvale bytem: Javorová 241, 252 44 Dolní Jirčany", font));

                    if (nemovId <= 3)
                    {
                        document.Add(new Paragraph("Lenka Bradáčová", font));
                        document.Add(new Paragraph("datum narození: 22.05.1968", font));
                        document.Add(new Paragraph("trvale bytem: Javorová 241, 252 44 Dolní Jirčany", font));
                    }

                    document.Add(new Paragraph(" "));

                    if (nemovId >= 4)
                    {
                        document.Add(new Paragraph("tel.: 720 389 260", font));
                        document.Add(new Paragraph("e-mail: nadpahorkem24@gmail.com", font));
                        document.Add(new Paragraph($"č. účtu: 816846033/0800, vedený u České spořitelny, variabilní symbol: {cisloBytu}", font));
                        document.Add(new Paragraph(" "));
                    }
                    else if (nemovId == 3)
                    {
                        document.Add(new Paragraph("e-mail: nadpahorkem24@gmail.com", font));
                        document.Add(new Paragraph($"č. účtu: 229263108/0300, vedený u ČSOB, variabilní symbol: {cisloBytu}", font));
                        document.Add(new Paragraph(" "));
                    }
                    else if (nemovId == 2)
                    {
                        document.Add(new Paragraph("e-mail: pripotocni31@gmail.com", font));
                        document.Add(new Paragraph($"č. účtu: 133936558/0300, vedený u ČSOB, variabilní symbol: {cisloBytu}", font));
                        document.Add(new Paragraph(" "));
                    }

                    document.Add(new Paragraph("(dále též „Pronajímatel“)", font));
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph("a", font));
                    document.Add(new Paragraph(" "));

                    foreach (var n in byt.Najemnici)
                    {
                        if (n.RoleNajemnika == Models.Role.Najemnik)
                        {
                            if (!string.IsNullOrWhiteSpace(n.Jmeno))
                            {
                                document.Add(new Paragraph($"{n.Jmeno}", font));
                            }
                            if (n.DatumNarozeni.HasValue)
                            {
                                document.Add(new Paragraph($"Datum narození: {n.DatumNarozeni?.ToString("dd.MM.yyyy")}", font));
                            }
                            if (!string.IsNullOrWhiteSpace(n.RodneCislo))
                            {
                                document.Add(new Paragraph($"Rodné číslo: {n.RodneCislo}", font));
                            }
                            if (!string.IsNullOrWhiteSpace(n.Email))
                            {
                                document.Add(new Paragraph($"Email: {n.Email}", font));
                            }
                            if (!string.IsNullOrWhiteSpace(n.Telefon))
                            {
                                document.Add(new Paragraph($"Tel.: {n.Telefon}", font));
                            }
                            if (n.CisloOP.HasValue)
                            {
                                document.Add(new Paragraph($"Číslo občanského průkazu: {n.CisloOP}", font));
                            }
                            if (!string.IsNullOrWhiteSpace(n.CisloUctu))
                            {
                                document.Add(new Paragraph($"Číslo účtu: {n.CisloUctu}", font));
                            }

                            document.Add(new Paragraph(" "));
                        }
                    }

                    document.Add(new Paragraph("(dále též „Nájemce“)", font));
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph("(Pronajímatel a Nájemce dále též společně jako „smluvní strany“)", font));
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph("uzavírají níže uvedeného dne, měsíce a roku tento Dodatek:", font));
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph("1. Úvodní ustanovení", font));
                    document.Add(new Paragraph($"1.1 Smluvní strany uzavřely dne {puvodniNajem?.ToString("dd.MM.yyyy")} nájemní smlouvu, na základě které Pronajímatel pronajal Nájemci byt č. {byt.Cislo}, na adrese {byt.Nemovitost.Adresa} (dále též „Nájemní smlouva“).", font));
                    document.Add(new Paragraph($"1.2 Doba trvání nájmu byla v Nájemní smlouvě sjednána na dobu určitou, a to do {konecNajmu?.ToString("dd.MM.yyyy")}.", font));
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph("2. Dohoda o změně Nájemní smlouvy", font));
                    document.Add(new Paragraph($"2.1 Smluvní strany se dohodly, že doba trvání nájmu sjednaná v Nájemní smlouvě se na základě tohoto Dodatku prodlužuje o další rok, a to do {datumProdlouzeni?.ToString("dd.MM.yyyy")}.", font));
                    document.Add(new Paragraph("2.2 Ostatní ujednání Nájemní smlouvy nejsou tímto Dodatkem dotčena.", font));
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph("3. Závěrečná ustanovení", font));
                    document.Add(new Paragraph("3.1 Tento Dodatek nabývá platnosti a účinnosti dnem jeho uzavření.", font));
                    document.Add(new Paragraph("3.2 Tento Dodatek je sepsán ve dvou stejnopisech, přičemž Pronajímatel a Nájemce obdrží po jednom stejnopisu.", font));
                    document.Add(new Paragraph("3.3 Každá ze smluvních stran prohlašuje, že tento Dodatek uzavírá svobodně a vážně, že považuje obsah tohoto Dodatku za určitý a srozumitelný a že jsou jí známy všechny skutečnosti, jež jsou pro uzavření tohoto Dodatku rozhodující.", font));
                    document.Add(new Paragraph(" ", font));
                    document.Add(new Paragraph($"V Praze dne {datumPodpisu.ToString("dd.MM.yyyy")}", font));

                    PdfPTable signatureTable = new PdfPTable(2);
                    signatureTable.WidthPercentage = 100;
                    signatureTable.SpacingBefore = 20f;
                    signatureTable.SetWidths(new float[] { 1f, 1f });

                    PdfPCell leftCell = new PdfPCell();
                    leftCell.Border = Rectangle.NO_BORDER;
                    leftCell.HorizontalAlignment = Element.ALIGN_CENTER;

                    LineSeparator lineSeparator = new LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, -2);
                    leftCell.AddElement(new Chunk(lineSeparator));

                    if (nemovId == 4 || nemovId == 5)
                    {
                        leftCell.AddElement(new Paragraph("Michal Bradáč a Jan Bradáč", font));
                    }
                    else
                    {
                        leftCell.AddElement(new Paragraph("Lenka Bradáčová, Michal Bradáč a Jan Bradáč", font));
                    }

                    PdfPCell rightCell = new PdfPCell();
                    rightCell.Border = Rectangle.NO_BORDER;
                    rightCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    rightCell.AddElement(new Chunk(lineSeparator));
                    rightCell.AddElement(new Paragraph($"{jmenaNajemcu}", font));

                    signatureTable.AddCell(leftCell);
                    signatureTable.AddCell(rightCell);

                    document.Add(signatureTable);

                    document.Close();

                    byte[] fileContent = ms.ToArray();
                    return File(fileContent, "application/pdf", nazevSouboru);
                }
                catch (Exception e)
                {
                    throw new ApplicationException("chyba při generování PDF", e);
                }
            }
        }
    }
}
