using System;
namespace Cyclone {
    public class BikeModel {
        private string _make;
        private string _type;
        private string _model;
        private static int modelTotal;
        
        public BikeModel(string make, string type, string model) {
            Make = make;
            Type = type;
            Model = model;

            modelTotal++;
        }
        
        public string Make {
            get {
                return _make;
            }

            set {
                _make = value;
            }
        }

        public string Type {
            get {
                return _type;
            }

            set {
                _type = value;
            }
        }

        public string Model {
            get {
                return _model;
            }

            set {
                _model = value;
            }
        }
        
        public static int ModelCount {
            get {
                return modelTotal;
            }
        }
        
        public static void removeBike() {
            modelTotal--;
        }
    }
}
