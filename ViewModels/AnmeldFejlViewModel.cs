using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Principal;
using System.Windows;
using System.Windows.Input;
using GlobalPopup.AnmeldFejl;
using Prism.Commands;
using InformationModel.Data;

namespace GlobalPopup.ViewModels
{
    public class AnmeldFejlViewModel : INotifyPropertyChanged
    {
        #region Globale variable
        private string _titel;
        private string _påvirkning;
        private string _vigtighed;
        private string _beskrivelse;
        private string _fil;
        private byte[] _data;
        private Område _selectedOmråde;
        private string _cpr;
        private Guid _guid = Guid.NewGuid();
        #endregion

        #region Observable Collections til dropdowns
        public ObservableCollection<string> PåvirkningData { get; internal set; }
        public ObservableCollection<string> VigtighedData { get; internal set; }
        #endregion

        #region Fields til cascading comboboxes
        public IList<Område> Områder { get; private set; }
        public ICollection<Kategori> Kategorier { get; private set; }
        public Kategori SelectedKategori { get; set; }
        #endregion

        #region Set Commands
        public DelegateCommand<Window>SendFejlCommand { get; internal set; }
        public DelegateCommand<Window> AnnullerCommand { get; internal set; }
        public ICommand VedhæftFilCommand { get; set; }
        public ICommand SletVedhæftCommand { get; set; }
        #endregion
        
        #region Constructor
        public AnmeldFejlViewModel()
        {
            this.SendFejlCommand = new DelegateCommand<Window>(this.SendFejlExecute);
            this.AnnullerCommand = new DelegateCommand<Window>(this.AnnullerExecute);
            InfoDataInstance = InformationModel.Data.InfoData.InfoDataInstance;
            CheckCprNr();
            SetPåvirkningData();
            SetVigtighedData();
            PopulateCascadingComboboxes();
            CreateVedhæftFilCommand();
            CreateSletVedhæftCommand();
        }
        #endregion
        
        #region Getters & Setters
        
        // Titel
        public string Titel
        {
            get { return _titel; }
            set
            {
                if (value == _titel)
                    return;

                _titel = value;
            }
        }

        // Påvirkning Combobox
        public string PåvirkningDropdown
        {
            get { return _påvirkning; }
            set
            {
                if (value == _påvirkning)
                    return;

                _påvirkning = value;
            }
        }

        // Vigtighed Combobox
        public string VigtighedDropdown
        {
            get { return _vigtighed; }
            set
            {
                if (value == _vigtighed)
                    return;

                _vigtighed = value;
            }
        }

        // Beskrivelse
        public string Beskrivelse
        {
            get { return _beskrivelse; }
            set
            {
                if (value == _beskrivelse)
                    return;

                _beskrivelse = value;
            }
        }

        // Filnavn
        public string FilNavn
        {
            get { return _fil; }
            set
            {
                if (value == _fil)
                    return;

                _fil = value;
                OnPropertyChanged("FilNavn");
            }
        }

        // Vedhæftede billede
        public byte[] Data
        {
            get { return _data; }
            set
            {
                if (value == _data)
                    return;

                _data = value;
            }
        }

        // CPR-nummer
        public string CprNr
        {
            get { return _cpr; }
            set
            {
                if (value == _cpr)
                    return;

                _cpr = value;
                OnPropertyChanged("CprNr");
            }
        }

        // Getter & setter til valgt Område i Kategori-combobox
        public Område SelectedOmråde
        {
            get
            {
                return _selectedOmråde;
            }
            set
            {
                _selectedOmråde = value;
                OnPropertyChanged("SelectedOmråde");
                this.Kategorier = _selectedOmråde.Kategorier;
                OnPropertyChanged("Kategorier");
            }
        }
        // InfoDataInstance til at hente CPR-nr
        public InfoData InfoDataInstance { get; set; }
        #endregion

        #region Commands

        #region "Send Fejl"-command

        /// <summary>
        /// Det kode, der skal eksekveres, når end-useren har trykket "Send Fejl"
        /// </summary>
        private void SendFejlExecute(Window window)
        {
            using (var client = new TfsOpretBugProxy.OpretBugItemSoapClient())
            {
                // Opretter instans af bug
                var bug = new TfsOpretBugProxy.AnmeldFejlDto();

                // Tjekker om vi får attachment med eller ej
                CheckVedhæft();

                try
                {
                    // Laver variable, som sættes i bug nedenfor
                    var titel = Titel.ToString();
                    var procesInstansId = _guid;
                    var beskrivelse = Beskrivelse.ToString();
                    var vigtighed = VigtighedDropdown.ToString();
                    var påvirkning = PåvirkningDropdown.ToString();
                    var område = SelectedOmråde.OmrådeNavn.ToString();
                    var kategori = SelectedKategori.KategoriNavn.ToString();
                    var brugernavn = CheckUserName(WindowsIdentity.GetCurrent().Name);
                    var cprNr = SetSubStringCpr(CprNr.ToString());

                    // Tilegner variable til bug
                    bug.Title = titel;
                    bug.ProcesInstansId = procesInstansId;
                    bug.Beskrivelse = beskrivelse;
                    bug.Urgency = vigtighed;
                    bug.Impact = påvirkning;
                    bug.RootCause = "Ukendt";
                    bug.CprNummer = cprNr;
                    bug.AreaType = område;
                    bug.IterationType = kategori;
                    bug.CurrentUserFullName = brugernavn;
                    bug.CurrentProcessNavn = ""; // Bliver ikke brugt i univers
                    bug.ProcesTrin = ""; // Bliver ikke brugt i univers
                    bug.File = FilNavn;
                    bug.Data = _data;

                    // Sender bug til TFS
                    try
                    {
                        // Her sendes buggen
                        var result = client.AnmeldEnfejl(bug);
                        // Succes Messagebox
                        var message = "Du har anmeldt en fejl til DSA IT & Support. \n \nTak for hjælpen :-)";
                        var caption = "Fejl anmeldt succesfuldt";
                        var btn = MessageBoxButton.OK;
                        var img = MessageBoxImage.Information;
                        System.Windows.MessageBox.Show(message, caption, btn, img);
                        CloseWindow(window);
                    }
                    catch (IOException e)
                    {
                        // Fejl Messagebox
                        var message = "Ups, noget gik galt, prøv igen. \n \n" + e.StackTrace;
                        var caption = "Fejl";
                        var btn = MessageBoxButton.OK;
                        var img = MessageBoxImage.Error;
                        System.Windows.MessageBox.Show(message, caption, btn, img);
                        CloseWindow(window);
                    }
                }
                catch(NullReferenceException e)
                {
                    // Fejl Messagebox
                    var message = "Ups, noget gik galt, prøv igen. \n \n" + e.StackTrace;
                    var caption = "Fejl";
                    var btn = MessageBoxButton.OK;
                    var img = MessageBoxImage.Error;
                    System.Windows.MessageBox.Show(message, caption, btn, img);
                    CloseWindow(window);
                }
            }
        }
        #endregion

        #region "Vedhæft billede"-command
        /// <summary>
        /// Command til vedhæft-knap
        /// </summary>
        private void CreateVedhæftFilCommand()
        {
            VedhæftFilCommand = new DelegateCommand(VedhæftFilExecute);
        }

        /// <summary>
        /// Metode som opretter en OpenFileDialog, hvori brugeren kan vælge et billede at vedhæfte.
        /// Derefter konverteres billedet til at byte array som kan sendes med buggen og dens andre parametre.
        /// </summary>
        private void VedhæftFilExecute()
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.Multiselect = false; // Ved flere billeder, sæt til true (skal rettes i WebService også da)
            fileDialog.Filter = "Alle billedfiler|*.png; *.jpg; *.jpeg; *.bmp; *.gif;|" + 
                                "PNG Filer(*.png)|*.png;|" +
                                "JPEG Filer(*.jpg *.jpeg)|*.jpg; *.jpeg;|" +
                                "BMP Filer(*.bmp)|*.bmp;|" +
                                "GIF Filer(*.gif)|*.gif;";
            fileDialog.DefaultExt = ".png";
            fileDialog.Title = "Vedhæft billede";

            // Hvis vinduet til at vælge en fil er åben, fortsæt
            if ((bool)fileDialog.ShowDialog())
            {
                // Får fat i hele filnavnet fra path
                var fil = System.IO.Path.GetFileName(fileDialog.FileName);

                // Åbner Fil op, og streamer til byte array
                using (System.IO.Stream stream = fileDialog.OpenFile())
                    // Tjekker længden på filen (4mb), hvis ja, smid i byte-array
                    if (stream.Length < 4000000)
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(fileDialog.FileName);
                        this._data = bytes;
                        FilNavn = fil;
                    }
                    // Ellers giv fejlbesked og åben FileDialog-vindue igen
                    else
                    {
                        var message = "Den valgte fil er for stor! \n \nVælg venligst en mindre fil (max 4MB)";
                        var caption = "Vedhæft billede";
                        var btn = MessageBoxButton.OK;
                        var img = MessageBoxImage.Exclamation;
                        System.Windows.MessageBox.Show(message, caption, btn, img);
                        VedhæftFilExecute();
                    }
            }
        }
        #endregion

        #region "Slet Vedhæft"-command
        /// <summary>
        /// Command til slet-vedhæft knap
        /// </summary>
        private void CreateSletVedhæftCommand()
        {
            SletVedhæftCommand = new DelegateCommand(SletVedhæftExecute);
        }

        /// <summary>
        /// Metode som sætter et tomt byte-array og et tomt filnavn ind i Data og FilNavn 
        /// </summary>
        private void SletVedhæftExecute()
        {
            var tomtArray = new byte[] { };
            if(Data != null)
            {
                FilNavn = "";
                _data = tomtArray;
                _fil = "INTET_BILLEDE";
            }
        }

        #endregion

        #region "Annuller"-command
        /// <summary>
        /// Metode der køres når brugeren trykker "Annuller"
        /// </summary>
        /// <param name="window">AnmeldFejlPopup-vinduet, som skal lukkes</param>
        private void AnnullerExecute(Window window)
        {
            var message = "Er du sikker på, at du vil lukke?";
            var caption = "Annullér";
            var btn = MessageBoxButton.YesNo;
            var img = MessageBoxImage.Question;
            if (MessageBox.Show(message, caption, btn, img) == MessageBoxResult.No)
            {
                return;
            }
            else
            {
                CloseWindow(window);
            }
        }

        #endregion

        #endregion

        #region Helpers

        /// <summary>
        /// Metode der opretter en ObservableCollection til Påvirkning dropdown
        /// </summary>
        private void SetPåvirkningData()
        {
            PåvirkningData = new ObservableCollection<string>
            {
                "Mange medlemmer påvirkes direkte økonomisk",
                "Få medlemmer påvirkes økonomisk",
                "Medlemmer påvirkes på anden måde en økonomisk"
            };
        }

        /// <summary>
        /// Metode der opretter en ObservableCollection til Vigtighed dropdown
        /// </summary>
        private void SetVigtighedData()
        {
            VigtighedData = new ObservableCollection<string>
            {
                "Væsentlig sagsbehandling eller medlemsservice kan ikke gennemføres",
                "Sagsbehandling forsinkes væsentligt",
                "Sagsbehandling forsinkes i mindre grad"
            };
        }

        /// <summary>
        /// Tilegner pågældende CPR-nummer en bindestreg,
        /// så det ser lækkert ud i TFS
        /// </summary>
        /// <param name="cpr">Pågældende CPR-nummer</param>
        /// <returns>Pågælende CPR-nummer med bindestreg</returns>
        private string SetSubStringCpr(string cpr)
        {
            var subString = "-";
            var index = 6;

            if (!cpr.Contains(subString) && cpr.Length >= 9)
            {
                return cpr.Insert(index, subString);
            }
            else
            {
                return cpr;
            }
        }

        /// <summary>
        /// Metode som checker for om InfoDataInstance er null, hvis den er sættes _cpr til en tom string ellers sættes _cpr til at
        /// være InfoDataInstance.CprNr, derefter kører den metoden SetSubStringCpr.
        /// </summary>
        private void CheckCprNr()
        {
            if (InfoDataInstance == null)
            {
                 _cpr = "";
            }
            else if (InfoDataInstance != null)
            {
                _cpr = InfoDataInstance.CprNr;
                var showCpr = SetSubStringCpr(_cpr);
                _cpr = showCpr;               
            }
        }

        /// <summary>
        /// Hjælpemetode som tjekker om brugernavnet på den pågældende bruger som anmelder en fejl, indeholder "dsa-".
        /// Metoden sætter derefter "dsa-" ind i brugernavnet hvis dette ikke er der i forvejen.
        /// </summary>
        /// <param name="userName">Brugernavnet på den pågældende bruger af medlemsuniverset</param>
        /// <returns>Brugernavnet med substring</returns>
        private string CheckUserName(string userName)
        {
            var subString = "dsa-";

            if (!userName.Contains(subString))
            {
                var index = userName.LastIndexOf("\\") + 1;
                return userName.Insert(index, subString);
            }
            else
            {
                return userName;
            }
        }

        /// <summary>
        /// Hjælpemetode som tjekker for om _data og_fil er null, hvis dette er tilfældet sættes disse til et tomt byte array
        /// og et standard filnavn
        /// Ellers sættes de til det valgte billede og filnavn.
        /// </summary>
        private void CheckVedhæft()
        {
            if (_data == null && _fil == null)
            {
                _data = new byte[] { };
                _fil = "INTET_BILLEDE";
            }
            else if (_data != null && _fil != null)
            {
                _data = Data;
                _fil = FilNavn;
            }
        }
        /// <summary>
        /// Lukker vinduet, som den får med fra "Send Fejl"-knappen
        /// </summary>
        /// <param name="window"></param>
        private void CloseWindow(Window window)
        {
            if(window != null)
            {
                window.Close();
            }
        }

        #region PopulateCascadingComboboxes
        /// <summary>
        /// Populerer Område/Kategori-comboboxes
        /// i forhold til hvad brugeren har valgt
        /// </summary>
        private void PopulateCascadingComboboxes()
        {
            this.Områder = new List<Område>()
            {
                #region ARMY
                new Område()
                {
                    OmrådeNavn = "ARMY", Kategorier = new List<Kategori>()
                    {
                        new Kategori() {KategoriNavn = "Feriepenge ARMY"},
                        new Kategori() {KategoriNavn = "G-dage"},
                        new Kategori() {KategoriNavn = "Indplacering"},
                        new Kategori() {KategoriNavn = "Karantæne"},
                        new Kategori() {KategoriNavn = "Overgang til ARMY"},
                        new Kategori() {KategoriNavn = "Satser"},
                        new Kategori() {KategoriNavn = "Teknisk belægning"},
                        new Kategori() {KategoriNavn = "Udbetaling"},
                        new Kategori() {KategoriNavn = "Ydelseskort"},
                        new Kategori() {KategoriNavn = "Andet"}
                    }
                },
                #endregion

                #region Beskedservice
                new Område()
                {
                    OmrådeNavn = "Beskedservice", Kategorier = new List<Kategori>()
                    {
                        new Kategori() {KategoriNavn = "Dokumenter"},
                        new Kategori() {KategoriNavn = "SMS/Email"},
                        new Kategori() {KategoriNavn = "Uregistreret post"},
                        new Kategori() {KategoriNavn = "Andet"}
                    }
                },
                #endregion

                #region Dagpenge
                new Område()
                {
                    OmrådeNavn = "Dagpenge", Kategorier = new List<Kategori>()
                    {
                        new Kategori() {KategoriNavn = "AUS - Arbejdsløshedsdagpenge under Sygdom"},
                        new Kategori() {KategoriNavn = "Dokumenter"},
                        new Kategori() {KategoriNavn = "G-dage"},
                        new Kategori() {KategoriNavn = "Indplacering"},
                        new Kategori() {KategoriNavn = "Karantæne"},
                        new Kategori() {KategoriNavn = "Ledighedserklæring"},
                        new Kategori() {KategoriNavn = "Pension"},
                        new Kategori() {KategoriNavn = "Skattefri præmie"},
                        new Kategori() {KategoriNavn = "Teknisk belægning"},
                        new Kategori() {KategoriNavn = "Udbetaling"},
                        new Kategori() {KategoriNavn = "UPH"},
                        new Kategori() {KategoriNavn = "Ydelseskort"},
                        new Kategori() {KategoriNavn = "Andet"}
                    }
                },
                #endregion

                #region Efterløn
                new Område()
                {
                    OmrådeNavn = "Efterløn", Kategorier = new List<Kategori>()
                    {
                        new Kategori() {KategoriNavn = "AUS - Arbejdsløshedsdagpenge under Sygdom"},
                        new Kategori() {KategoriNavn = "Dokumenter"},
                        new Kategori() {KategoriNavn = "Efterlønsbevis"},
                        new Kategori() {KategoriNavn = "G-dage"},
                        new Kategori() {KategoriNavn = "Halvårserklæring"},
                        new Kategori() {KategoriNavn = "Karantæne"},
                        new Kategori() {KategoriNavn = "Overgang til efterløn"},
                        new Kategori() {KategoriNavn = "Pension"},
                        new Kategori() {KategoriNavn = "Satser"},
                        new Kategori() {KategoriNavn = "Skattefri præmie"},
                        new Kategori() {KategoriNavn = "Teknisk belægning"},
                        new Kategori() {KategoriNavn = "Timebank"},
                        new Kategori() {KategoriNavn = "Udbetaling"},
                        new Kategori() {KategoriNavn = "Ydelseskort"},
                        new Kategori() {KategoriNavn = "Andet"}
                    }
                },
                #endregion

                #region Feriedagpenge
                new Område()
                {
                    OmrådeNavn = "Feriedagpenge", Kategorier = new List<Kategori>()
                    {
                        new Kategori() {KategoriNavn = "Dokumenter"},
                        new Kategori() {KategoriNavn = "G-dage"},
                        new Kategori() {KategoriNavn = "Indplacering"},
                        new Kategori() {KategoriNavn = "Karantæne"},
                        new Kategori() {KategoriNavn = "Satser"},
                        new Kategori() {KategoriNavn = "Teknisk belægning"},
                        new Kategori() {KategoriNavn = "Udbetaling"},
                        new Kategori() {KategoriNavn = "Ydelseskort"},
                        new Kategori() {KategoriNavn = "Andet"}
                    }
                },
                #endregion

                #region Insite
                new Område()
                {
                    OmrådeNavn = "Insite", Kategorier = new List<Kategori>()
                    {
                        new Kategori() {KategoriNavn = "Andet"}
                    }
                },
                #endregion

                #region Jobformidling/Socialrådgivning
                new Område()
                {
                    OmrådeNavn = "Jobformidling/Socialrådgivning", Kategorier = new List<Kategori>()
                    {
                        new Kategori() {KategoriNavn = "Aftaler"},
                        new Kategori() {KategoriNavn = "Aktiviteter"},
                        new Kategori() {KategoriNavn = "CV Information"},
                        new Kategori() {KategoriNavn = "Dokumenter"},
                        new Kategori() {KategoriNavn = "Joblog"},
                        new Kategori() {KategoriNavn = "Jobmål"},
                        new Kategori() {KategoriNavn = "Krav til jobsøgning"},
                        new Kategori() {KategoriNavn = "Samtaler"},
                        new Kategori() {KategoriNavn = "Udmeldelse"},
                        new Kategori() {KategoriNavn = "Andet"}
                    }
                },
                #endregion

                #region Medlemsservice
                new Område()
                {
                    OmrådeNavn = "Medlemsservice", Kategorier = new List<Kategori>()
                    {
                        new Kategori() {KategoriNavn = "SMS/Email"},
                        new Kategori() {KategoriNavn = "Andet"}
                    }
                },
                #endregion

                #region Medlemskab
                new Område()
                {
                    OmrådeNavn = "Medlemskab", Kategorier = new List<Kategori>()
                    {
                        new Kategori() {KategoriNavn = "Dimittend"},
                        new Kategori() {KategoriNavn = "Dokumenter"},
                        new Kategori() {KategoriNavn = "EØS"},
                        new Kategori() {KategoriNavn = "Fraflytning"},
                        new Kategori() {KategoriNavn = "Halvårserklæring"},
                        new Kategori() {KategoriNavn = "Inkasso"},
                        new Kategori() {KategoriNavn = "Karantæne"},
                        new Kategori() {KategoriNavn = "Kontingent/Efterlønsbidrag"},
                        new Kategori() {KategoriNavn = "Medlemskabsdata"},
                        new Kategori() {KategoriNavn = "Optagelse"},
                        new Kategori() {KategoriNavn = "Tilflytning"},
                        new Kategori() {KategoriNavn = "Timebank"},
                        new Kategori() {KategoriNavn = "Udmeldelse"},
                        new Kategori() {KategoriNavn = "Andet"}
                    }
                },
                #endregion

                #region Mit DSA
                new Område()
                {
                    OmrådeNavn = "Mit DSA", Kategorier = new List<Kategori>()
                    {
                        new Kategori() {KategoriNavn = "Dokumenter"},
                        new Kategori() {KategoriNavn = "SMS/Email"},
                        new Kategori() {KategoriNavn = "Andet"}
                    }
                },
                #endregion

                #region Sekretariat
                new Område()
                {
                    OmrådeNavn = "Sekretariat", Kategorier = new List<Kategori>()
                    {
                        new Kategori() {KategoriNavn = "Dokumenter"},
                        new Kategori() {KategoriNavn = "Kontingent/Efterlønsbidrag"},
                        new Kategori() {KategoriNavn = "Medlemskabsdata"},
                        new Kategori() {KategoriNavn = "SMS/Email"},
                        new Kategori() {KategoriNavn = "Udbetaling"},
                        new Kategori() {KategoriNavn = "Udmeldelse"},
                        new Kategori() {KategoriNavn = "Andet"}
                    }
                },
                #endregion

                #region Supplerende dagpenge
                new Område()
                {
                    OmrådeNavn = "Supplerende dagpenge", Kategorier = new List<Kategori>()
                    {
                        new Kategori() {KategoriNavn = "Dokumenter"},
                        new Kategori() {KategoriNavn = "G-dage"},
                        new Kategori() {KategoriNavn = "Indplacering"},
                        new Kategori() {KategoriNavn = "Karantæne"},
                        new Kategori() {KategoriNavn = "Satser"},
                        new Kategori() {KategoriNavn = "Teknisk belægning"},
                        new Kategori() {KategoriNavn = "Udbetaling"},
                        new Kategori() {KategoriNavn = "Ydelseskort"},
                        new Kategori() {KategoriNavn = "Andet"}
                    }
                },
                #endregion

                #region Andet
                new Område()
                {
                    OmrådeNavn = "Andet", Kategorier = new List<Kategori>()
                    {
                        new Kategori() {KategoriNavn = "Dokumenter"},
                        new Kategori() {KategoriNavn = "Gældsbillede"},
                        new Kategori() {KategoriNavn = "SMS/Email"},
                        new Kategori() {KategoriNavn = "Uregistreret post"},
                        new Kategori() {KategoriNavn = "Andet"}
                    }
                },
                #endregion
            };
        }
        #endregion

        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}