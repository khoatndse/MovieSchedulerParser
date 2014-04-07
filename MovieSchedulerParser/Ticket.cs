using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieSchedulerParser
{
    class Ticket
    {
        public string CumRap { get; set; }
        public string TenRap { get; set; }
        public string Ngaychieu { get; set; }
        public string SuatChieu { get; set; }

        public Ticket()
        {

        }

        public Ticket(string cumRap,string tenRap, string ngayChieu, string suatChieu)
        {
            this.CumRap = cumRap;
            this.TenRap = tenRap;
            this.Ngaychieu = ngayChieu;
            this.SuatChieu = suatChieu;
        }
    }
}
