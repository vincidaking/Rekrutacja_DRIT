using Soneta.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soneta.Kadry;
using Soneta.KadryPlace;
using Soneta.Types;
using Rekrutacja.Workers.Template;

//Rejetracja Workera - Pierwszy TypeOf określa jakiego typu ma być wyświetlany Worker, Drugi parametr wskazuje na jakim Typie obiektów będzie wyświetlany Worker
[assembly: Worker(typeof(TemplateWorker), typeof(Pracownicy))]
namespace Rekrutacja.Workers.Template
{
    public class TemplateWorker
    {
        //Aby parametry działały prawidłowo dziedziczymy po klasie ContextBase
        public class TemplateWorkerParametry : ContextBase
        {
            [Caption("A")]
            public double X { get; set; }
            [Caption("B")]
            public double Y { get; set; }
            [Caption("Data obliczeń")]
            public Date DataObliczen { get; set; }
            [Caption("Operacja")]
            public char Operacja { get; set; }
          
            public TemplateWorkerParametry(Context context) : base(context)
            {
                this.X = 0;
                this.DataObliczen = Date.Today;
                this.Operacja = '+';
                this.Y = 0;

            }
        }
        //Obiekt Context jest to pudełko które przechowuje Typy danych, aktualnie załadowane w aplikacji
        //Atrybut Context pobiera z "Contextu" obiekty które aktualnie widzimy na ekranie
        [Context]
        public Context Cx { get; set; }
        //Pobieramy z Contextu parametry, jeżeli nie ma w Context Parametrów mechanizm sam utworzy nowy obiekt oraz wyświetli jego formatkę

        [Context]
        public TemplateWorkerParametry Parametry { get; set; }
        //Atrybut Action - Wywołuje nam metodę która znajduje się poniżej
        [Action("Kalkulator",
           Description = "Prosty kalkulator ",
           Priority = 10,
           Mode = ActionMode.ReadOnlySession,
           Icon = ActionIcon.Accept,
           Target = ActionTarget.ToolbarWithText)]

        public async void WykonajAkcje()
        {
            //Włączenie Debug, aby działał należy wygenerować DLL w trybie DEBUG
            DebuggerSession.MarkLineAsBreakPoint();
            //Pobieranie danych z Contextu

            Pracownik[] pracownik = null;

            if (this.Cx.Contains(typeof(Pracownik[])))
            {
                pracownik = (Pracownik[])this.Cx[typeof(Pracownik[])];
            }


            //Modyfikacja danych
            //Aby modyfikować dane musimy mieć otwartą sesję, któa nie jest read only
            using (Session nowaSesja = this.Cx.Login.CreateSession(false, false, "ModyfikacjaPracownika"))
            {
                //Otwieramy Transaction aby można było edytować obiekt z sesji
                using (ITransaction trans = nowaSesja.Logout(true))
                {
                    //Pobieramy obiekt z Nowo utworzonej sesji

                    foreach (var item in pracownik)
                    {
                        var pracownikZSesja = nowaSesja.Get(item);
                        //Features - są to pola rozszerzające obiekty w bazie danych, dzięki czemu nie jestesmy ogarniczeni to kolumn jakie zostały utworzone przez producenta

                        pracownikZSesja.Features["DataObliczen"] = this.Parametry.DataObliczen;
                        pracownikZSesja.Features["Wynik"] = await ArytmetykaAsync(this.Parametry.X, this.Parametry.Y, this.Parametry.Operacja);
                    }

                    //Zatwierdzamy zmiany wykonane w sesji
                    trans.CommitUI();
                }
                //Zapisujemy zmiany
                nowaSesja.Save();
            }
        }

        async Task<double> ArytmetykaAsync(double a, double b, char x)
        {

            double temp = 0.0;

            switch (x)
            {
                case '+':
                    temp = a + b;
                    break;
                case '-':
                    temp = a - b;
                    break;
                case '*':
                    temp = a * b;
                    break;
                case '/':
                    {
                        if (a == 0.0 || b == 0.0)
                            break;

                        temp = a / b;
                        break;
                    }
            };

            return await Task.FromResult<double>(temp);
        }
    }
}