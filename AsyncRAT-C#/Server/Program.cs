using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

/* 
       │ Author       : Light Yagami
       │ Name         : AsyncRatSharp
       │ Contact Me   : https://github.com/LightYagami

       This program is distributed for educational purposes only.
*/

namespace Server
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Enables visual styles for the application.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Define the path for the Fixer.bat file.
                string batPath = Path.Combine(Application.StartupPath, "Fixer.bat");
                
                // If Fixer.bat does not exist, create it with default content.
                if (!File.Exists(batPath))
                {
                    File.WriteAllText(batPath, Properties.Resources.Fixer);
                }
            }
            catch (Exception ex)
            {
                // Catch any exceptions that may arise and silently handle them (e.g., logging the error could be added).
                Console.WriteLine("Error creating Fixer.bat: " + ex.Message);
            }

            // Create and display the main application form.
            form1 = new Form1();
            Application.Run(form1);
        }

        // Static field to store the main form instance.
        public static Form1 form1;
    }
}
