using MySql.Data.MySqlClient;

namespace kocsis_gabor_feleves_projekt
{
    internal class Program
    {
        public struct csapat
        {
            public string nev;
            public int lm; //lejatszott meccs
            public int lg; //lott golok
            public int kg; //kapott golok
            public int p; //pontok

        }

        public struct meccs
        {
            public string r1; //resztvevo1
            public string r2; //resztvevo2
            public int g1; //r1 goljai
            public int g2; //r2 goljai
        }

        public struct feladvany
        {
            public string kerdes;
            public string valasz;
            public int nehezseg;
        }
        static void Main(string[] args)
        {
            byte menupont=0;
            string[] menuLista = new string[] { "Játék kezdése", "Átlagosan szerzett gólok", "Lejátszott meccsek", "Tabella", "Gólkülönbség", "Kilépés" };
            tajekoztato();
            menuteljes(menupont, menuLista);
        }

        static void menuRajzol(string[] ml)
        {
            Console.WriteLine("Válassz az alábbi lehetőségek közül: ");
            for (byte m = 0; m < ml.Length; m++)
                Console.WriteLine($"{m + 1}. {ml[m]}");
        }
        static byte menuBekeres()
        {
            byte menupont;
            bool ok;
            string adat;
            menupont = 0;
            do
            {

                Console.Write("Írj be egy számot: ");
                adat = Console.ReadLine();
                ok = byte.TryParse(adat, out menupont);
                if (ok)
                {
                    if (menupont < 1 || menupont > 6)
                    {
                        ok = false;
                    }
                }
            } while (!ok);
            return menupont;
        }
        static void menuteljes(byte menupont, string[] menuLista)
        {
            do
            {
                menuRajzol(menuLista);
                menupont = menuBekeres();
                if (menupont == 1)
                {
                    Console.Clear();
                    jatek(feltolt(), folv());
                    Console.Clear();
                }
                else if (menupont == 2)
                {
                    Console.Clear();
                    atlag(meccsolvas(),feltolt());
                    Console.ReadLine();
                    Console.Clear();
                }
                else if (menupont == 3)
                {
                    Console.Clear();
                    osszesmeccs(meccsolvas(),feltolt());
                    Console.ReadLine();
                    Console.Clear();
                }
                else if (menupont == 4)
                {
                    Console.Clear();
                    tabella(feltolt());
                    Console.ReadLine();
                    Console.Clear();
                }
                else if (menupont == 5)
                {
                    Console.Clear();
                    golk(feltolt());
                    Console.ReadLine();
                    Console.Clear();
                }
            } while (menupont != menuLista.Length);
        }
        static void tajekoztato()
        {
            
            Console.WriteLine("Üdvözöllek!");
            Console.WriteLine("Ez egy focibajnokság formájában lebonyolított kvízbajnokság játék. A folytatáshoz nyomj meg egy gombot!");
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadLine();
            Console.Clear();
        }

        static csapat[] feltolt()
        {
            string sor;
            string[] darabok;
            int k = 0;
            csapat[] cs=new csapat[100];
            FileStream fs = new FileStream("csapatok.txt", FileMode.Open);
            StreamReader o = new StreamReader(fs);
            while (!o.EndOfStream)
            {
                sor= o.ReadLine();
                darabok = sor.Split("#");
                cs[k].nev=darabok[0];
                cs[k].lm=int.Parse(darabok[1]);
                cs[k].lg = int.Parse(darabok[2]);
                cs[k].kg = int.Parse(darabok[3]);
                cs[k++].p = int.Parse(darabok[4]);

            }
            o.Close();
            fs.Close();
            Array.Resize(ref cs, k);
            return cs;
        }
        static void tabella(csapat[] cs)
        {
            string kapcsString = @"server=localhost;userid=tabellakezelo;password=asd123;database=tabella";
            MySqlConnection kapcs = new MySqlConnection(kapcsString);
            try
            {
                kapcs.Open();
                //tabla kiuritese
                var torol = "DELETE FROM csapat";
                var torles = new MySqlCommand(torol, kapcs);
                torles.Prepare();
                torles.ExecuteNonQuery();
                //feltoltes a friss adatokkal

                for (int i = 0; i < cs.Length; i++)
                {
                    var sqlUtasitas = "INSERT INTO csapat(nev, lejatszott, lottgol, kapottgol, pontok) VALUES(@adat1, @adat2, @adat3, @adat4, @adat5)";
                    var sqlParancs = new MySqlCommand(sqlUtasitas, kapcs);

                    sqlParancs.Parameters.AddWithValue("@adat1", cs[i].nev);
                    sqlParancs.Parameters.AddWithValue("@adat2", cs[i].lm);
                    sqlParancs.Parameters.AddWithValue("@adat3", cs[i].lg);
                    sqlParancs.Parameters.AddWithValue("@adat4", cs[i].kg);
                    sqlParancs.Parameters.AddWithValue("@adat5", cs[i].p);
                    sqlParancs.Prepare();
                    sqlParancs.ExecuteNonQuery();
                }

                string olvas = "SELECT nev, lejatszott, lottgol, kapottgol, pontok FROM csapat ORDER BY pontok DESC";
                MySqlCommand kiolvas = new MySqlCommand(olvas, kapcs);
                MySqlDataReader lekerdezesAdatok = kiolvas.ExecuteReader();

                while (lekerdezesAdatok.Read())
                {
                    Console.WriteLine($"{lekerdezesAdatok.GetString(0)} {lekerdezesAdatok.GetInt32(1)} {lekerdezesAdatok.GetInt32(2)} {lekerdezesAdatok.GetInt32(3)} {lekerdezesAdatok.GetInt32(4)}");
                }
            }
            catch(MySql.Data.MySqlClient.MySqlException)
            {
                Console.WriteLine("A kapcsolat megnyitásakor hiba történt!");
            }
           
        }

        static void jatek(csapat[] cs, feladvany[] f)
        {
            //elvegezendo feladatok a kesobbi vegleges verziohoz: forulok szamontartasa, (minden csapat csak ketszer jatszhat egymassal, ha mindenki jatszott minenkivel ketszer, akkor vege a bajnoksagnak)
            //uj, focival kapcsolatos kerdesek keresese (kész)
            //kerdesek nehezsegi fokozata (kész)

            string j1; //jatekos 1 csapata
            string j2; //jatekos 2 csapata
            string v1; //jatekos 1 valasza a kerdesre
            string v2; //jatekos 2 valasza a kerdesre
            int ford=0; // forduloszam
            int p1=0; //jatekos 1 szerzett pontjai a meccsen
            int p2=0; //jatekos 2 szerzett pontjai a meccsen
            int r = 999;
            bool ok;
            string adat;
            int[] felhasznaltKerdesek;
            int x = 0;
            Console.WriteLine("Elérhető csapatok:");
            //összes csapat kiírása
            elerheto(cs);
            
            //csapatvalasztas
            Console.WriteLine("Játékos 1 csapata: ");
            j1 =csell(cs);

            Console.WriteLine("Játékos 2 csapata: ");
            j2 = csell(cs);

            do { 
                Console.Write("Hány kérdéssel szeretnétek játszani:");
                adat = Console.ReadLine();
                ok = int.TryParse(adat, out ford);
                if (ok)
                {
                    if (ford <= 0 || ford > 20)
                    {
                        ok = false;
                    }
                }
            } while (!ok);

            felhasznaltKerdesek = new int[ford * 2];

            //fordulószámszor kérdés j1, kérdés j2
            Random vsz= new Random();
            for (int u=0; u<ford; u++)
            {
                while (Array.IndexOf(felhasznaltKerdesek, r)!=-1||r==999)
                {
                    r = vsz.Next(0, f.Length);
                }
                felhasznaltKerdesek[x] += r;
                x++;
                Console.WriteLine(f[r].kerdes);
                v1=Console.ReadLine();
                if (v1 == f[r].valasz)
                {
                    Console.WriteLine($"Helyes valasz! Szerzett pont: {f[r].nehezseg}");
                    p1 += f[r].nehezseg;
                }
                else
                {
                    Console.WriteLine("Rossz valasz!");
                }

                while (Array.IndexOf(felhasznaltKerdesek, r) != -1)
                {
                    r = vsz.Next(0, f.Length);
                }
                felhasznaltKerdesek[x] += r;
                Console.WriteLine(f[r].kerdes);
                x++;
                v2 = Console.ReadLine();
                if (v2 == f[r].valasz)
                {
                    Console.WriteLine($"Helyes valasz! Szerzett pont: {f[r].nehezseg}");
                    p2 += f[r].nehezseg;
                }
                else
                {
                    Console.WriteLine("Rossz valasz!");
                }
            }


            //pontok hozzaadasa lott golokhoz és ellenfel pontjainak hozzaadasa kapott golokhoz mindkét játékosnál
            //a csapat pontjainak modositasa (tabellan)
            //a lejatszott meccsek szamanak novelese 1-gyel mindket csapatnal
            ment(cs, j1, j2, p1, p2);
            //meccs es eredmeny beirasa a meccsek.txt fajlba
            FileStream fs = new FileStream("meccsek.txt", FileMode.Append);
            StreamWriter i = new StreamWriter(fs);
            i.WriteLine($"{j1}-{j2}#{p1}-{p2}");
            i.Close();
            fs.Close();
        }

        //golkulonbseg
        static void golk(csapat[] cs)
        {
            byte valasz = 0;
            string[] menupontok = new string[] { "Összes csapat gólkülönbsége", "Egy csapat gólkülönbsége" };

            menuRajzol(menupontok);
            valasz = menuBekeres2();
            Console.Clear();

            if (valasz == 1)
            {
                for (int i = 0; i < cs.Length; i++)
                {
                    Console.WriteLine($"A(z) {cs[i].nev} golkulonbsege {cs[i].lg - cs[i].kg}");
                }
            }
            else
            {
                elerheto(cs);
                string csapat = csell(cs);
                for (int i = 0; i < cs.Length; i++)
                {
                    if (cs[i].nev == csapat)
                    {
                        Console.WriteLine($"A(z) {cs[i].nev} golkulonbsege {cs[i].lg - cs[i].kg}");
                    }
                }
            }
        }

        //feladvanyok olvasasa
        static feladvany[] folv()
        {
            string sor;
            string[] darabok;
            int k = 0;
            feladvany[] f = new feladvany[100];
            FileStream fs = new FileStream("feladvanyok.txt", FileMode.Open);
            StreamReader o = new StreamReader(fs);
            while (!o.EndOfStream)
            {
                sor = o.ReadLine();
                darabok = sor.Split(";");
                f[k].kerdes = darabok[0];
                f[k].valasz = darabok[1];
                f[k++].nehezseg = int.Parse(darabok[2]);

            }
            o.Close();
            fs.Close();
            Array.Resize(ref f, k);
            return f;
        }

        //beirt csapatnevek ellenorzese
        static string csell(csapat[] cs)
        {
            string val;
            HashSet<string> csnev = new HashSet<string>();
            Console.Write("Válassz egy csapatot az elérhető csapatokból: ");
            val = Console.ReadLine();
            for (int t=0; t<cs.Length; t++)
            {
                csnev.Add(cs[t].nev);
            }
            while (!csnev.Contains(val))
            {
                Console.Write("Válassz egy csapatot az elérhető csapatokból: ");
                val = Console.ReadLine();
            }
            return val;
        }

        //eredmenyek modositasa a meccsek.txt fajlban
        static void ment(csapat[] cs, string j1, string j2, int p1, int p2)
        {
            for (int t=0; t<cs.Length; t++)
            {
                if (cs[t].nev== j1)
                {
                    cs[t].lm++;
                    cs[t].lg += p1;
                    cs[t].kg += p2;
                    if(p1>p2)
                    {
                        cs[t].p += 3;
                    }
                    else if (p1 == p2)
                    {
                        cs[t].p += 1;
                    }
                }
            }

            for (int t = 0; t < cs.Length; t++)
            {
                if (cs[t].nev == j2)
                {
                    cs[t].lm++;
                    cs[t].lg += p2;
                    cs[t].kg += p1;
                    if (p2 > p1)
                    {
                        cs[t].p += 3;
                    }
                    else if (p2 == p1)
                    {
                        cs[t].p += 1;
                    }
                }
            }

            FileStream fs = new FileStream("csapatok.txt",FileMode.Create);
            StreamWriter i = new StreamWriter(fs);
            for (int t=0; t<cs.Length; t++)
            {
                i.WriteLine($"{cs[t].nev}#{cs[t].lm}#{cs[t].lg}#{cs[t].kg}#{cs[t].p}");
            }
            i.Close();
            fs.Close();
        }

        static void atlag(meccs[] m, csapat[] cs)
        {
            string csnev;
            int lejatszott = 0;
            int golok = 0;
            elerheto(cs);
            csnev =csell(cs);
            for (int i=0; i<m.Length; i++)
            {
               if (m[i].r1 == csnev)
                {
                    lejatszott++;
                    golok += m[i].g1;
                }
                 if (m[i].r2 == csnev)
                {
                    lejatszott++;
                    golok += m[i].g2;
                }
            }

            if (lejatszott > 0)
            {
                Console.WriteLine($"A(z) {csnev} átlagosan szerzett góljai: {golok / lejatszott}");
            }
            else
            {
                Console.WriteLine("Ez a csapaat még nem játszott meccset!");
            }
        }

        static meccs[] meccsolvas()
        {
            string sor;
            string[] darabok;
            string[] resztvevok;
            string[] golok;
            int k = 0;
            meccs[] m=new meccs[200];
            FileStream fs = new FileStream("meccsek.txt",FileMode.Open);
            StreamReader o = new StreamReader(fs);
            while(!o.EndOfStream)
            {
                sor = o.ReadLine();
                darabok = sor.Split("#");
                resztvevok = darabok[0].Split("-");
                golok= darabok[1].Split("-");
                m[k].r1=resztvevok[0];
                m[k].r2=resztvevok[1];
                m[k].g1=int.Parse(golok[0]);
                m[k++].g2=int.Parse(golok[1]);
            }
            Array.Resize(ref m,k);
            o.Close();
            fs.Close();
            return m;
        }

        static void osszesmeccs(meccs[] m, csapat[] cs)
        {
            byte valasz=0;
            string[] menupontok = new string[] { "Összes meccs", "Egy csapat meccsei"};

                menuRajzol(menupontok);
                valasz = menuBekeres2();
                Console.Clear();

            if (valasz==1)
            {
                for (int i = 0; i < m.Length; i++)
                {
                    Console.WriteLine($"{m[i].r1}-{m[i].r2} {m[i].g1}-{m[i].g2}");
                }
            }
            else
            {
                elerheto(cs);
                string csapat = csell(cs);
                for (int i = 0; i < m.Length; i++)
                {
                    if (m[i].r1==csapat|| m[i].r2 == csapat)
                    Console.WriteLine($"{m[i].r1}-{m[i].r2} {m[i].g1}-{m[i].g2}");
                }
            }
        }

        static byte menuBekeres2()
        {
            byte menupont;
            bool ok;
            string adat;
            menupont = 0;
            do
            {

                Console.Write("Írj be egy számot: ");
                adat = Console.ReadLine();
                ok = byte.TryParse(adat, out menupont);
                if (ok)
                {
                    if (menupont < 1 || menupont > 2)
                    {
                        ok = false;
                    }
                }
            } while (!ok);
            return menupont;
        }

        static void elerheto(csapat[] cs)
        {
            for (int q = 0; q < cs.Length; q++)
            {
                Console.Write($"{cs[q].nev}, ");

            }
            Console.WriteLine();
        }
    }


}