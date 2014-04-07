using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;

namespace MovieSchedulerParser
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static Dictionary<string, Movie> MovieList;
        [STAThread]
        static void Main()
        {
            MovieList = new Dictionary<string, Movie>();

            Logger log = Logger.GetInstance();
            log.WriteLog("************************PROGRAM INVOKE***************************");

            Lotte lotteParser = new Lotte();
            MovieList = lotteParser.Parsing(MovieList);

            Galaxy galaxyParser = new Galaxy();
            MovieList = galaxyParser.Parsing(MovieList);

            Cgv cgvParser = new Cgv();
            MovieList = cgvParser.Parsing(MovieList);

            log.WriteLog("=============Save To DB");
            CleanDB();
            foreach (var item in MovieList)
            {
                item.Value.SaveToDB();
            }
        }

        static void CleanDB()
        {
            string ConnectString = ConfigurationManager.ConnectionStrings["sql"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(ConnectString))
            {
                conn.Open();
                string cleanCommand = "TRUNCATE table Scheduler;Delete from Movie";
                SqlCommand cmd = new SqlCommand(cleanCommand, conn);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
