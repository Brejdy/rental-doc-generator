using System.Text;

namespace najemci
{
    public class CisloNaText
    {
        private static readonly string[] jednotky = { "", "jeden", "dva", "tři", "čtyři", "pět", "šest", "sedm", "osm", "devět" };
        private static readonly string[] desitky = { "", "deset", "dvacet", "třicet", "čtyřicet", "padesát", "šedesát", "sedmdesát", "osmdesát", "devadesát" };
        private static readonly string[] teens = { "deset", "jedenáct", "dvanáct", "třináct", "čtrnáct", "patnáct", "šestnáct", "sedmnáct", "osmnáct", "devatenáct" };
        private static readonly string[] stovky = { "", "sto", "dvě stě", "tři sta", "čtyři sta", "pět set", "šest set", "sedm set", "osm set", "devět set" };

        public static string CisloNaSlova(int cislo)
        {
            if (cislo == 0) return "nula";

            var sb = new StringBuilder();

            if (cislo >= 100000)
            {
                int stovkyTisic = cislo / 100000;
                sb.Append(stovky[stovkyTisic] + " tisíc ");
                cislo %= 100000;
            }

            if (cislo >= 20000)
            {
                int desitkyTisic = cislo / 10000;
                sb.Append(desitky[desitkyTisic] + " ");
                cislo %= 10000;
            }
            else if (cislo >= 10000)
            {
                int teensTisic = (cislo / 1000) % 10;
                sb.Append(teens[teensTisic] + " tisíc ");
                cislo %= 1000;
            }

            if (cislo >= 1000)
            {
                int tisice = cislo / 1000;
                if (tisice == 1)
                {
                    sb.Append("jeden tisíc ");
                }
                else if (tisice >= 2 && tisice <= 4)
                {
                    sb.Append(jednotky[tisice] + " tisíce ");
                }
                else
                {
                    sb.Append(jednotky[tisice] + " tisíc ");
                }
                cislo %= 1000;
            }

            if (cislo >= 100)
            {
                int sto = cislo / 100;
                sb.Append(stovky[sto] + " ");
                cislo %= 100;
            }

            if (cislo >= 20)
            {
                int deset = cislo / 10;
                sb.Append(desitky[deset] + " ");
                cislo %= 10;
            }
            else if (cislo >= 10)
            {
                sb.Append(teens[cislo - 10] + " ");
                cislo = 0;
            }

            if (cislo > 0)
            {
                sb.Append(jednotky[cislo] + " ");
            }

            return sb.ToString().Trim();
        }
    }
}
