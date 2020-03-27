using System;
using System.Collections.Generic;

namespace Cyclone {
    public class Parser {
        public const int INVENTORY_JUMP = 8;
        public const int MODEL_JUMP = 4;

        private const int MAKE_LINE = 0;
        private const int TYPE_LINE = 1;
        private const int MODEL_LINE = 2;
        private const int YEAR_LINE = 3;
        private const int WHEEL_SIZE_LINE = 4;
        private const int FORKS_LINE = 5;
        private const int CODE_LINE = 6;

        public Parser() {
        }
        
        /// <summary>
        /// Parses the base 'inventory.txt' file into a list of Bike objects.
        /// </summary>
        /// <returns>The parsed file as a Bike list.</returns>
        /// <param name="inventoryLines">Base inventory file by lines.</param>
        public static Dictionary<int, Bike> ParseInventoryFile(string[] inventoryLines) {
            Dictionary<int, Bike> bikeInventory = new Dictionary<int, Bike>();
            
            // For as many bikes are in the inventory
            for (var i = 0; (i + INVENTORY_JUMP - 2) < inventoryLines.Length; i += INVENTORY_JUMP) {
                // Recover bike properties        
                string make = inventoryLines[i + MAKE_LINE];
                string type = inventoryLines[i + TYPE_LINE];
                string model = inventoryLines[i + MODEL_LINE];
                string wheelSize = inventoryLines[i + WHEEL_SIZE_LINE];
                string forks = inventoryLines[i + FORKS_LINE];

                // Parse fields of Year and Security Code as integers
                int bikeYear, securityCode;
                bool yearOK = Int32.TryParse(inventoryLines[i + YEAR_LINE], out bikeYear);
                bool codeOK = Int32.TryParse(inventoryLines[i + CODE_LINE], out securityCode);

                // Validate fields
                bool fieldsEmpty = string.IsNullOrWhiteSpace(make)
                    || string.IsNullOrWhiteSpace(model) || string.IsNullOrWhiteSpace(type)
                    || string.IsNullOrWhiteSpace(wheelSize) || string.IsNullOrWhiteSpace(forks);

                if (yearOK && codeOK && !fieldsEmpty) {
                    // Create bike and add to inventory
                    Bike importedBike = new Bike(make, type, model, bikeYear, 
                        wheelSize, forks, securityCode);
                    bikeInventory[securityCode] = importedBike;
                }
            }

            return bikeInventory;
        }
        
        public static List<BikeModel> ParseModelsFile(string[] modelLines) {
            List<BikeModel> newValidModels = new List<BikeModel>();

            for (var i = 0; (i + MODEL_JUMP - 2) < modelLines.Length; i += MODEL_JUMP) {
                // Recover model properties
                string make = modelLines[i + MAKE_LINE];
                string model = modelLines[i + MODEL_LINE];
                string type = modelLines[i + TYPE_LINE];
                
                // Validate fields
                bool fieldsEmpty = string.IsNullOrWhiteSpace(make) 
                    || string.IsNullOrWhiteSpace(model) || string.IsNullOrWhiteSpace(type);

                if (!fieldsEmpty) {
                    // Parse bike data
                    BikeModel importModel = new BikeModel(make, type, model);
                    newValidModels.Add(importModel);
                } else {
                    throw new FormatException();
                }
            }

            return newValidModels;
        } 
    }
}
