using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.Common
{
    public class AppSettings
    {
        AppSettings()
        {
            GcRanges = new List<GcRange>();
            PriceListAssignmentList = new List<PriceListAssignment>();
        }

        public static readonly string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Andreas Schultz Software", "Order2VPos");

        static readonly string settingsPath = Path.Combine(AppDataFolder, "AppSettings.xml");

        class SingletonCreator
        {
            static SingletonCreator() { }
            internal static readonly AppSettings instance = LoadSettings();
        }

        public static AppSettings Default
        {
            get { return SingletonCreator.instance; }
        }

        public void SaveSettings()
        {
            string path = Path.GetDirectoryName(settingsPath);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            Serializer.Save(this, settingsPath);
        }

        public static AppSettings GetSettingsFromFile()
        {
            return LoadSettings();
        }

        static AppSettings LoadSettings()
        {
            if (File.Exists(settingsPath))
            {
                var settings = Serializer.Load<AppSettings>(settingsPath);

                return settings;
            }
            else
                return new AppSettings();
        }

        [Category("Vectron POS"), DisplayName("Bediener Geheimcode")]
        [PasswordPropertyText(true)]
        public int OperatorCode { get; set; }

        [Category("IONE API"), DisplayName("API Basis-Url")]
        public string ApiBaseAddress { get; set; } = "https://qa-gatewayapi.ione.de.com/api/RESTAPI";

        [Category("Vectron POS"), DisplayName("Artikeleigenschaften Nr. (für Webshop)")]
        public int AttributeNoForWebShop { get; set; }

        [Category("IONE API"), DisplayName("Filial-Nr.")]
        public int BranchAddressId { get; set; }

        [Category("IONE API"), DisplayName("API Identifier")]
        public string IoneApiIdentifier { get; set; }

        [Category("IONE API"), DisplayName("API Token")]
        [PasswordPropertyText(true)]
        public string IoneApiToken { get; set; }

        [Category("Vectron POS"), DisplayName("Bediener-Nr.")]
        public int Operator { get; set; }

        [Category("Vectron POS"), DisplayName("Finanzweg-Nr.")]
        public int ReceiptMediaNo { get; set; }

        [Category("Vectron POS"), DisplayName("Rabatt/Aufschlag-Nr. (für Trinkgeld)")]
        public int TipDiscountNumber { get; set; }

        [Category("Vectron POS - Verbindung"), DisplayName("IP-Adresse")]
        public string VPosIPAddress { get; set; }

        [Category("Vectron POS"), DisplayName("Tischbereiche")]
        public List<GcRange> GcRanges { get; set; }

        [Category("Allgemein"), DisplayName("Timer aktiv")]
        public bool TimerActive { get; set; }

        [Category("Vectron POS - Verbindung"), DisplayName("IP-Port")]
        public int VPosIpPort { get; set; } = 1050;

        [Category("Vectron POS"), DisplayName("Verwendeter Name")]
        public NameNr NameNr { get; set; }

        [Category("IONE API"), DisplayName("Preislistenzuordnung")]
        public List<PriceListAssignment> PriceListAssignmentList { get; set; }

        [Category("Webservice"), DisplayName("Url-Prefix")]
        public string WebServiceUrlPrefix { get; set; } = "http://*:1080/";

    }
}
