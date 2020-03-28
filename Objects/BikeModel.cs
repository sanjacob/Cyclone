using System;
namespace Cyclone {
    /// <summary>
    /// Bike model class.
    /// </summary>
    public class BikeModel {
        private string _make;
        private string _type;
        private string _model;
        private static int modelTotal;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Cyclone.BikeModel"/> class.
        /// </summary>
        /// <param name="make">Make.</param>
        /// <param name="type">Type.</param>
        /// <param name="model">Model.</param>
        public BikeModel(string make, string type, string model, bool temp = false) {
            Make = make;
            Type = type;
            Model = model;

            if (!temp) {
                modelTotal++;
            }
        }

        /// <summary>
        /// Gets or sets the make of the model.
        /// </summary>
        /// <value>The make.</value>
        public string Make {
            get {
                return _make;
            }

            set {
                _make = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of bike of the model.
        /// </summary>
        /// <value>The type.</value>
        public string Type {
            get {
                return _type;
            }

            set {
                _type = value;
            }
        }

        /// <summary>
        /// Gets or sets the model name.
        /// </summary>
        /// <value>The model.</value>
        public string Model {
            get {
                return _model;
            }

            set {
                _model = value;
            }
        }

        /// <summary>
        /// Gets the model count.
        /// </summary>
        /// <value>The model count.</value>
        public static int ModelCount {
            get {
                return modelTotal;
            }
        }

        /// <summary>
        /// Removes a model from the count.
        /// </summary>
        public static void RemoveModel() {
            modelTotal--;
        }
    }
}
