using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDD_FilRouge3emePartie
{
    class Logement
    {
        private int host_id;
        private int room_id;
        private string room_type;
        private string borough;
        private string neighborhood;
        private int reviews;
        private double overall_satisfaction;
        private int accommodates;
        private int bedrooms;
        private double price;
        private int minstay;
        private double latitude;
        private double longitude;
        private string week;
        private string availability;


        public Logement(int host_id, int room_id, string room_type, string borough, string neighborhood, int reviews, double overall_satisfaction, int accommodates, int bedrooms, double price, int minstay, double latitude, double longitude, string week, string availability)
        {
            this.host_id = host_id;
            this.room_id = room_id;
            this.room_type = room_type;
            this.borough = borough;
            this.neighborhood = neighborhood;
            this.reviews = reviews;
            this.overall_satisfaction = overall_satisfaction;
            this.accommodates = accommodates;
            this.bedrooms = bedrooms;
            this.price = price;
            this.minstay = minstay;
            this.latitude = latitude;
            this.longitude = longitude;
            this.week = week;
            this.availability = availability;
        }

        public int Host_id
        {
            get { return host_id; }
        }

        public int Room_id
        {
            get { return room_id; }
        }

        public string Room_type
        {
            get { return room_type; }
        }

        public string Borough
        {
            get { return borough; }
        }

        public string Neighborhood
        {
            get { return neighborhood; }
        }
        public int Reviews
        {
            get { return reviews; }
        }
        public double Overall_satisfaction
        {
            get { return overall_satisfaction; }
        }
        public int Accommodates
        {
            get { return accommodates; }
        }
        public int Bedrooms
        {
            get { return bedrooms; }
        }
        public double Price
        {
            get { return price; }
        }
        public int Minstay
        {
            get { return minstay; }
        }
        public double Latitude
        {
            get { return latitude; }
        }
        public double Longitude
        {
            get { return longitude; }
        }
        public string Week
        {
            get { return week;}
        }
        public string Availability
        {
            get { return availability; }
        }

        public override string ToString()
        {
            return (host_id.ToString());
        }

    }
}











