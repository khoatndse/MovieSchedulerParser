using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;

namespace MovieSchedulerParser
{
    class Movie
    {
        public string Name { get; set; }
        public string Producer { get; set; }
        public string Length { get; set; }
        public string Type { get; set; }
        public string Image { get; set; }
        public string DetailLink { get; set; }
        public string TrailerLink { get; set; }
        public string Info { get; set; }
        public string MvSource { get; set; }
        public List<Ticket> Tickets { get; set; }

        public Movie()
        {
        }

        public Movie(string name, string producer, string length, string type, string image)
        {
            this.Name = name;
            this.Producer = producer;
            this.Length = length;
            this.Type = type;
            this.Image = image;
        }

        public override string ToString()
        {
            // for test only
            string s = "";
            for (int i = 0; i < Tickets.Count; i++)
            {
                s += Tickets[i].CumRap + " ";
                s += Tickets[i].TenRap + " ";
                s += Tickets[i].Ngaychieu + " ";
                s += Tickets[i].SuatChieu + " ";
                s += "\n";
            }
            return "" + this.DetailLink + "\n"
                + this.TrailerLink + "\n"
                + this.Name + "\n"
                + this.Producer + "\n"
                + this.Length + "\n"
                + this.Type + "\n"
                + s;
        }

        public void SaveToDB()
        {
            string ConnectString = ConfigurationManager.ConnectionStrings["sql"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(ConnectString))
            {
                conn.Open();
                string insertMovieCommand = "INSERT INTO Movie(Name,Producer,Length,Type,Image,DetailLink,TrailerLink,Info,MvSource) VALUES (@Name,@Producer,@Length,@Type,@Image,@DetailLink,@TrailerLink,@Info,@MvSource);SELECT SCOPE_IDENTITY()";
                SqlCommand cmd = new SqlCommand(insertMovieCommand, conn);
                cmd.Parameters.Add(new SqlParameter("Name", this.Name));
                cmd.Parameters.Add(new SqlParameter("Producer", this.Producer));
                cmd.Parameters.Add(new SqlParameter("Length", this.Length));
                cmd.Parameters.Add(new SqlParameter("Type", this.Type));
                cmd.Parameters.Add(new SqlParameter("Image", this.Image));
                cmd.Parameters.Add(new SqlParameter("DetailLink", this.DetailLink));
                cmd.Parameters.Add(new SqlParameter("TrailerLink", this.TrailerLink));
                cmd.Parameters.Add(new SqlParameter("Info", this.Info));
                cmd.Parameters.Add(new SqlParameter("MvSource", this.MvSource));

                var newID = cmd.ExecuteScalar();

                // insert mapping ticket
                for (int i = 0; i < this.Tickets.Count; i++)
                {
                    string insertTicketCommand = "INSERT INTO Scheduler(MovieID,CumRap,TenRap,NgayChieu,SuatChieu) VALUES(@MovieID,@CumRap,@TenRap,@NgayChieu,@SuatChieu)";
                    cmd.CommandText = insertTicketCommand;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter("MovieID",newID));
                    cmd.Parameters.Add(new SqlParameter("CumRap", Tickets[i].CumRap));
                    cmd.Parameters.Add(new SqlParameter("TenRap", Tickets[i].TenRap));
                    cmd.Parameters.Add(new SqlParameter("NgayChieu", Tickets[i].Ngaychieu));
                    cmd.Parameters.Add(new SqlParameter("SuatChieu", Tickets[i].SuatChieu));
                    cmd.ExecuteNonQuery();
                }

                
            }
        }
    }
}
