using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gtk;

namespace Cyclone
{
    /// <summary>
    /// Bike object.
    /// </summary>
    public class Bike
    {
        private string _make;
        private string _type;
        private string _model;
        private int _year;
        private string _wheelSize;
        private string _forks;
        private int _securityCode;
        private double _cost;
        private static int bikeTotal;

        /// <summary>
        /// Gets or sets the make of the bike.
        /// </summary>
        /// <value>The make.</value>
        public string Make
        {
            get
            {
                return _make;
            }

            set
            {
                _make = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the bike.
        /// </summary>
        /// <value>The type.</value>
        public string Type
        {
            get
            {
                return _type;
            }

            set
            {
                _type = value;
            }
        }

        /// <summary>
        /// Gets or sets the model of the bike.
        /// </summary>
        /// <value>The model.</value>
        public string Model
        {
            get
            {
                return _model;
            }

            set
            {
                _model = value;
            }
        }

        /// <summary>
        /// Gets or sets the year that the bike was made in.
        /// </summary>
        /// <value>The year.</value>
        public int Year
        {
            get
            {
                return _year;
            }

            set
            {
                _year = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the wheel of the bike.
        /// </summary>
        /// <value>The size of the wheel.</value>
        public string WheelSize
        {
            get
            {
                return _wheelSize;
            }

            set
            {
                _wheelSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the forks of the bike.
        /// </summary>
        /// <value>The forks.</value>
        public string Forks
        {
            get
            {
                return _forks;
            }

            set
            {
                _forks = value;
            }
        }

        /// <summary>
        /// Gets or sets the security code of the bike.
        /// </summary>
        /// <value>The security code.</value>
        public int SecurityCode
        {
            get
            {
                return _securityCode;
            }

            set
            {
                _securityCode = value;
            }
        }

        /// <summary>
        /// Gets or sets the cost of the bike.
        /// </summary>
        /// <value>The cost.</value>
        public double Cost {
            get {
                return _cost;
            }

            set {
                _cost = value;
            }
        }

        /// <summary>
        /// Gets the bike count.
        /// </summary>
        /// <value>The bike count.</value>
        public static int BikeCount {
            get {
                return bikeTotal;
            }
        }

        public Bike() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Cyclone.Bike"/> class.
        /// </summary>
        /// <param name="make">Make.</param>
        /// <param name="type">Type.</param>
        /// <param name="model">Model.</param>
        /// <param name="year">Year.</param>
        /// <param name="wheelSize">Wheel size.</param>
        /// <param name="forks">Forks.</param>
        /// <param name="securityCode">Security code.</param>
        public Bike(string make, string type, string model, int year, string wheelSize, string forks, int securityCode)
        {
            Make = make;
            Type = type;
            Model = model;
            Year = year;
            WheelSize = wheelSize;
            Forks = forks;
            SecurityCode = securityCode;

            bikeTotal++;
        }

        /// <summary>
        /// Removes a number of bikes from the count.
        /// </summary>
        /// <param name="amount">Amount.</param>
        public static void RemoveBike(int amount = 1) {
            bikeTotal -= amount;
        }
        
        /// <summary>
        /// Gets bike as inventory.
        /// </summary>
        /// <value>Bike as inventory.</value>
        public Dictionary<int, Bike> AsInventory {
            get {
                return new Dictionary<int, Bike> { 
                    [SecurityCode] = this
                };
            }
        }
    }
}