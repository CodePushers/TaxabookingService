using System;
namespace taxaBookingAPI
{
    public class BookingDTO
    {

        public int BookingId { get; set; }
        public DateTime BookingTidspunkt { get; set; }
        public string KundeNavn { get; set; }
        public string TelefonNr { get; set; }
        public DateTime StartTidspunkt { get; set; }
        public string StartSted { get; set; }
        public string SlutSted { get; set; }

        public BookingDTO(int bookingid, DateTime bookingtidspunkt, string kundenavn, string telefonnr, DateTime starttidspunkt, string startsted, string slutsted)
        {
           
            this.BookingId = bookingid;
            this.BookingTidspunkt = bookingtidspunkt;
            this.KundeNavn = kundenavn;
            this.TelefonNr = telefonnr;
            this.StartTidspunkt = starttidspunkt;
            this.StartSted = startsted;
            this.SlutSted = slutsted;
        }

        public BookingDTO()
        {
        }
    }
}

