using System;
using System.Collections.Generic;
using System.IO;
using Gtk;

namespace Cyclone {
    class MainClass {
        public const int LINE_JUMP = 8;
        public const int MODEL_L_JUMP = 4;

        public const int MAKE_POSITION = 0;
        public const int MODEL_POSITION = 2;
        public const int TYPE_POSITION = 1;
        public const int YEAR_POSITION = 3;
        public const int WHEEL_S_POSITION = 4;
        public const int FORKS_POSITION = 5;
        public const int CODE_POSITION = 6;

        public const int ROW_T_P = 0;
        public const int STORE_MAKE_P = 1;
        public const int STORE_MODEL_P = 2;
        public const int STORE_TYPE_P = 4;
        public const int STORE_YEAR_P = 3;
        public const int STORE_W_S_P = 5;
        public const int STORE_FORKS_P = 6;
        public const int STORE_CODE_P = 7;

        public const int MAKE_DEPTH = 0;
        public const int MODEL_DEPTH = 1;
        public const int BIKE_DEPTH = 2;

        public const string baseInventory = "inventory.txt";
        public const string baseModels = "bikemake.txt";
        
        static TreeStore modelStore;
        static TreeStore bikeStore;
        static TreeStore bikeExpStore;
        
        static Dictionary<string, Dictionary<string, TreeIter>> bikeMap = new Dictionary<string, Dictionary<string, TreeIter>>();
        static Dictionary<string, TreeIter> modelMap = new Dictionary<string, TreeIter>();

        static Dictionary<int, Dictionary<string, TreeIter>> securityMap = new Dictionary<int, Dictionary<string, TreeIter>>();

        static int bikeCount;
        static int modelCount;


        public static void Main(string[] args) {
            Dictionary<string, Dictionary<string, List<Bike>>> bikeDict = parseInventoryFile(File.ReadAllLines(baseInventory));
            Dictionary<string, List<BikeModel>> modelsDict = parseModelFile(File.ReadAllLines(baseModels));
            
            populateModelStore(modelsDict);
            populateBikeStore(bikeDict);

            Application.Init();
            MainWindow mainWindow;
            mainWindow = new MainWindow();
            mainWindow.bikeTree.Model = bikeStore;

            mainWindow.importItem.Activated += (sender, e) => importFile(sender, e, mainWindow);
            mainWindow.exportItem.Activated += (sender, e) => exportFile(sender, e, mainWindow);
            mainWindow.modelsItem.Activated += (sender, e) => editModels(sender, e);
            mainWindow.changeView.Toggled += (sender, e) => changeTreeStore(sender, e, mainWindow);
            
            updateBikeCount(mainWindow);
            
            bikeStore.RowInserted += delegate {
                updateBikeCount(mainWindow);
            };
            
            bikeStore.RowDeleted += delegate {
                updateBikeCount(mainWindow);
            };
            
            mainWindow.bikeTree.RowActivated += (o, rowArgs) => rowActivate(o, rowArgs, mainWindow);
            mainWindow.addBikeButton.Clicked += (sender, e) => addBikeButton(sender, e, mainWindow);
            mainWindow.removeBikeButton.Clicked += (sender, e) => removeBikeButton(sender, e, mainWindow);
            mainWindow.bikeTree.KeyPressEvent += (o, keyArgs) => suprRow(o, keyArgs, mainWindow);
            mainWindow.ShowAll();
            mainWindow.Show();
            Application.Run();
        }

        private static void suprRow(object o, KeyPressEventArgs args, MainWindow mainWindow) {
            if (args.Event.Key == Gdk.Key.Delete) {
                deleteBikeTreeRow(mainWindow);
            }
        }

        static void changeTreeStore(object sender, EventArgs e, MainWindow mainWindow) {
            if (mainWindow.bikeTree.Model == bikeStore) {
                mainWindow.bikeTree.Model = bikeExpStore;
            } else {
                mainWindow.bikeTree.Model = bikeStore;
            }
        }

        static void updateBikeCount(MainWindow mainWindow) {
            string pluralNoun = "s";
            if (bikeCount == 1) {
                pluralNoun = "";
            }
            mainWindow.bikeAmount.Text = String.Format("{0} bike{1} in inventory", bikeCount, pluralNoun);
        }

        static void addBikeButton(object sender, EventArgs e, MainWindow mainWindow) {
            BikeEditor createBikeWin;
            createBikeWin = new BikeEditor();
    
            createBikeWin.ShowAll();
            createBikeWin.Show();
            
            createBikeWin.saveBike.Clicked += (saveSender, saveE) => createBike(saveSender, saveE, mainWindow, createBikeWin);
        }
        
        static void removeBikeButton(object sender, EventArgs e, MainWindow mainWindow) {
            deleteBikeTreeRow(mainWindow);
        }
        
        private static void deleteBikeTreeRow(MainWindow mainWindow) {
            // Work with currently used store
            TreeStore activeStore = (TreeStore) mainWindow.bikeTree.Model;

            // Get selected rows
            TreeSelection selectedChildren = mainWindow.bikeTree.Selection;
            TreePath[] selected = selectedChildren.GetSelectedRows();
            
            // Confirm deletion
            string pluralNoun = "";
            if (selectedChildren.CountSelectedRows() > 1) {
                pluralNoun = "s";
            }

            string confMsg = string.Format( "Are you sure you want to delete {0} row{1}?", 
                selectedChildren.CountSelectedRows(), pluralNoun);
            
            MessageDialog md = new MessageDialog (mainWindow, 
                DialogFlags.DestroyWithParent,
                MessageType.Question, 
                ButtonsType.YesNo, confMsg);
    
            ResponseType result = (ResponseType) md.Run ();

            if (result == ResponseType.Yes) {
                // Get each element from last to first, to preserve iter order
                Array.Reverse(selected);
                
                foreach (TreePath bikeRow in selected) {
                    TreeIter rowIter;
    
                    if (activeStore.GetIter(out rowIter, bikeRow)) {
                        int rowType =  (int) activeStore.GetValue(rowIter, 0);

                        if (rowType == BIKE_DEPTH) {
                            // Get reference code and delete from both stores
                            int secCode = (int) activeStore.GetValue(rowIter, STORE_CODE_P);
                            deleteElement(secCode);

                        } else if (rowType == MODEL_DEPTH) {
                            // Recover in grouped store and delete every child in both stores
                            deleteBranch(rowIter);
                        } else {
                            // Loop model branch deletion
                            TreeIter model;
                            bool pointingModel = activeStore.IterChildren(out model, rowIter);

                            TreeIter advPointer = model;
                
                            while (pointingModel) {
                                pointingModel = activeStore.IterNext(ref advPointer);
                                deleteBranch(model);
                                model = advPointer;
                            }                            
                        }
                    }
                }
            }
            
            md.Destroy();
        }

        

        private static void rowActivate(object o, RowActivatedArgs args, MainWindow mainWindow) {
            TreeIter rowActive;
            TreeStore activeStore = (TreeStore) mainWindow.bikeTree.Model;
            activeStore.GetIter(out rowActive, args.Path);

            int rowType = activeStore.IterDepth(rowActive);

            if (rowType == BIKE_DEPTH) {
                string make = (string) activeStore.GetValue(rowActive, STORE_MAKE_P);
                string model = (string) activeStore.GetValue(rowActive, STORE_MODEL_P);
                string type = (string) activeStore.GetValue(rowActive, STORE_TYPE_P);
                
                int year;
                int.TryParse((string) activeStore.GetValue(rowActive, STORE_YEAR_P), out year);
                
                string wheelSize = (string) activeStore.GetValue(rowActive, STORE_W_S_P);
                string forks = (string) activeStore.GetValue(rowActive, STORE_FORKS_P);
                int code = (int) activeStore.GetValue(rowActive, STORE_CODE_P);

                Bike activatedBike = new Bike(make, type, model, year, wheelSize, forks, code);
                
                BikeEditor editBikeWin;
                editBikeWin = new BikeEditor(activatedBike);
        
                editBikeWin.ShowAll();
                editBikeWin.Show();
                editBikeWin.saveBike.Clicked += (sender, e) => editBike(sender, e, mainWindow, editBikeWin, rowActive);

            }
        }

        private static void createBike(object sender, EventArgs e, MainWindow mainWindow, BikeEditor bikeWin) {
            try {
                Bike newBike = bikeWin.getBike();
                
                Dictionary<string, Dictionary<string, List<Bike>>> newBikeDict = new Dictionary<string, Dictionary<string, List<Bike>>> { 
                    [newBike.Make] = {
                        [newBike.Model] = new List<Bike> {newBike}
                    }
                };

                populateBikeStore(newBikeDict);

            } catch (FormatException) {
                int bikeError = bikeWin.errorType;

                if (bikeWin.errorType == BikeEditor.ERROR_YEAR) {
                    sendError("Please enter a valid year", mainWindow);
                } else if (bikeError == BikeEditor.ERROR_EMPTY) {
                    sendError("Please fill in all mandatory fields", mainWindow);
                }
            }

            bikeWin.Destroy();
        }
        
        private static void editBike(object sender, EventArgs e, MainWindow mainWindow, BikeEditor editBikeWin, TreeIter rowActive) {
            try {
                Bike newBike = editBikeWin.getBike();
                bikeStore.SetValues(rowActive, false, newBike.Make,
                    newBike.Model, newBike.Year, newBike.Type, newBike.WheelSize, newBike.Forks, null);
            } catch (FormatException) {
                int bikeError = editBikeWin.errorType;

                if (bikeError == BikeEditor.ERROR_YEAR) {
                    sendError("Please enter a valid year", mainWindow);
                } else if (bikeError == BikeEditor.ERROR_EMPTY) {
                    sendError("Please fill in all mandatory fields", mainWindow);
                }
            }
              
                editBikeWin.Destroy();
        }
        
        private static void sendError(string errorMsg, MainWindow mainWindow) {
            MessageDialog yearError = new MessageDialog(mainWindow,
                DialogFlags.DestroyWithParent,
                MessageType.Error,
                ButtonsType.Close, errorMsg);

            ResponseType yearRes = (ResponseType) yearError.Run();

            if (yearRes == ResponseType.Close) {
                yearError.Destroy();
            }
        }

        private static void importFile(object sender, EventArgs args, MainWindow mainWindow) {
            FileChooserDialog importFileDialog = new FileChooserDialog("Import an existing inventory file", 
                mainWindow, FileChooserAction.Open, 
                "Cancel",ResponseType.Cancel, "Open",ResponseType.Accept);
            
            if (importFileDialog.Run() == (int) ResponseType.Accept) {
                string importFilename = importFileDialog.Filename;
                
                if (File.Exists(importFilename)) {
                    int oldBikeCount = bikeCount;
                    
                    Dictionary<string, Dictionary<string, List<Bike>>> newImportedInv = parseInventoryFile(File.ReadAllLines(importFilename));
                    populateBikeStore(newImportedInv);
                    int elementsAdded = bikeCount - oldBikeCount;

                    Console.WriteLine(String.Format("Read {0} elements from file {1}", elementsAdded, importFileDialog.Name));
                } else {
                    throw new NotImplementedException();
                }
            }

            importFileDialog.Destroy();    
        }

        private static void exportFile(object sender, EventArgs e, MainWindow mainWindow) {
            FileChooserDialog exportFile = new FileChooserDialog("Create a report", 
                mainWindow, FileChooserAction.Save, 
                "Cancel",ResponseType.Cancel, "Save",ResponseType.Accept);
            
            if (exportFile.Run() == (int) ResponseType.Accept) {
                Console.WriteLine(exportFile.Filename);
            }

            exportFile.Destroy();    
        }

        private static void editModels(object sender, EventArgs e) {
            ModelsEditor modelsWin;
            modelsWin = new ModelsEditor();
            modelsWin.modelTree.Model = modelStore;
            modelsWin.ShowAll();
            modelsWin.Show();
        }

        public static void populateModelStore(Dictionary<String, List<BikeModel>> modelDict) {
            if (modelStore == null) {
                modelStore = new TreeStore(typeof(string), typeof(string), typeof(string));
            }

            foreach (string make in modelDict.Keys) {
                if (!modelMap.ContainsKey(make)) {
                    TreeIter makeParent = modelStore.AppendValues(make);
                    modelMap[make] = makeParent;
                }

                TreeIter makeIter = modelMap[make];

                foreach (BikeModel model in modelDict[make]) {
                    TreeIter modelChild = modelStore.AppendValues(makeIter, model.Make, model.Model, model.Type);
                    modelCount++;
                }
            }
        }

        public static void populateBikeStore(Dictionary<string, Dictionary<string, List<Bike>>> bikeDict ) {
            // Create bikeStore if not yet done
            if (bikeStore == null) {
                bikeStore = new TreeStore(typeof(int), typeof(string), typeof(string),
                    typeof(string), typeof(string), typeof(string), typeof(string), typeof(int));
                
                // Last attribute is an int specifying the type of row (make, model, bike) (0, 1, 2)
            }
            
            // Create expanded store if not yet created            
            if (bikeExpStore == null) {
                bikeExpStore = new TreeStore(typeof(int), typeof(string), typeof(string),
                    typeof(string), typeof(string), typeof(string), typeof(string), typeof(int));
            }
            
            // For every make on new bikes
            foreach (string make in bikeDict.Keys) {
                // If bike store does not have that make yet, create it
                if (!bikeMap.ContainsKey(make)) {
                    TreeIter makeParent = bikeStore.AppendValues(0, make);
                    bikeMap[make] = new Dictionary<string, TreeIter>{
                        ["_ownIter"] = makeParent
                    };
                }

                TreeIter makeIter = bikeMap[make]["_ownIter"];

                foreach (string model in bikeDict[make].Keys) {
                    if (!bikeMap[make].ContainsKey(model)) {
                        TreeIter modelParent = bikeStore.AppendValues(makeIter, 1, make, model);
                        bikeMap[make][model] = modelParent;                    
                    }

                    TreeIter modelIter = bikeMap[make][model];

                    foreach (Bike bike in bikeDict[make][model]) {
                        bikeCount++;

                        TreeIter bikeChild = bikeStore.AppendValues(modelIter, BIKE_DEPTH, bike.Make, 
                            bike.Model, bike.Year, bike.Type, bike.WheelSize, bike.Forks, bike.SecurityCode);

                        TreeIter expChild = bikeExpStore.AppendValues(BIKE_DEPTH, bike.Make, 
                            bike.Model, bike.Year, bike.Type, bike.WheelSize, bike.Forks, bike.SecurityCode);

                        securityMap[bike.SecurityCode] = new Dictionary<string, TreeIter> { 
                            ["_grouped"] = bikeChild,
                            ["_expanded"] = expChild
                        };
                    }
                }
            }
        }
        
        private static void deleteElement(int secCode) {
            // Recover references to the bike in both TreeStores
            TreeIter groupedElement = securityMap[secCode]["_grouped"];
            TreeIter expElement = securityMap[secCode]["_expanded"];
            
            // Console feedback
            Console.WriteLine(string.Format("About to delete {0}", secCode));

            // Recover parent before deletion
            TreeIter modelParent;
            bikeStore.IterParent(out modelParent, groupedElement);
            string parentMake = (string) bikeStore.GetValue(modelParent, 1);
            string parentModel = (string) bikeStore.GetValue(modelParent, 2);

            // Decrease count now so that deleted event can update indicator accordingly
            bikeCount--;
            
            // Remove from both Stores using TreeIter reference recovered earlier
            bikeStore.Remove(ref groupedElement);
            bikeExpStore.Remove(ref expElement);
            
            // Remove saved reference
            securityMap.Remove(secCode);
            
            // If last element of parent, delete parent too
            if (bikeStore.IterNChildren(modelParent) == 0) {
                bikeStore.Remove(ref modelParent);
                bikeMap[parentMake].Remove(parentModel);
            }
        }
        
        private static void deleteBranch(TreeIter modelIter) {
            // Set pointer to first child of model branch (depth 1)
            TreeIter bikeRow;
            bool pointingChild = bikeStore.IterChildren(out bikeRow, modelIter);
            
            // As long as we have successfully assigned pointer to next element...
            while (pointingChild) {
                // Save reference code to later delete
                int secCode = (int) bikeStore.GetValue(bikeRow, STORE_CODE_P);
                // Advance pointer, before base is deleted
                pointingChild = bikeStore.IterNext(ref bikeRow);
                
                // Send code to recover reference and so delete
                deleteElement(secCode);
            }
            
            // Save parent iter reference
            TreeIter makeParent;
            bikeStore.IterParent(out makeParent, modelIter);
            string parentMake = (string) bikeStore.GetValue(makeParent, 1);
            
            // Remove model branch
            bikeStore.Remove(ref modelIter);
            
            // Remove make parent if it is empty now
            if (bikeStore.IterNChildren(makeParent) == 0) { 
                bikeStore.Remove(ref makeParent);
                bikeMap.Remove(parentMake);
            }
        }
        
        public static Dictionary<string, Dictionary<string, List<Bike>>> parseInventoryFile(String[] invLines) {
            Dictionary<string, Dictionary<string, List<Bike>>> bikeInventory = new Dictionary<string, Dictionary<string, List<Bike>>>();

             for (var i = 0; (i + LINE_JUMP - 2) < invLines.Length; i += LINE_JUMP) {
                // string make, string type, string model, int year, string wheelSize, string forks, int securityCode
                // Parse fields of Year and Security Code as integers
                string make = invLines[i + MAKE_POSITION];
                string type = invLines[i + TYPE_POSITION];
                string model = invLines[i + MODEL_POSITION];
                string wheelSize = invLines[i + WHEEL_S_POSITION];
                string forks = invLines[i + FORKS_POSITION];

                int bikeYear, securityCode;
                
                // Validate fields
                bool yearOK = Int32.TryParse(invLines[i + YEAR_POSITION], out bikeYear);
                bool codeOK = Int32.TryParse(invLines[i + CODE_POSITION], out securityCode);
                bool fieldsEmpty = string.IsNullOrWhiteSpace(make)
                    || string.IsNullOrWhiteSpace(model) || string.IsNullOrWhiteSpace(type)
                    || string.IsNullOrWhiteSpace(wheelSize) || string.IsNullOrWhiteSpace(forks);

                if (yearOK && codeOK && !fieldsEmpty) {

                    // Parse bike data
                    Bike importBike = new Bike(make, type, model, bikeYear, wheelSize,
                        forks, securityCode);

                    // Group into makes
                    if (bikeInventory.ContainsKey(make)) {
                        // Group into models
                        if (bikeInventory[make].ContainsKey(model)) {
                            bikeInventory[make][model].Add(importBike);
                        } else {
                            bikeInventory[make][model] = new List<Bike> {
                                importBike
                            };
                        }
                    } else {
                        bikeInventory[make] = new Dictionary<string, List<Bike>> {
                            [model] = new List<Bike>{ importBike }
                        }; 
                    }
                } else {
                    throw new NotImplementedException();
                }
            }

            return bikeInventory;
        }
        
        public static Dictionary<String, List<BikeModel>> parseModelFile(string[] modelFile) {
            Dictionary<String, List<BikeModel>> modelDict = new Dictionary<string, List<BikeModel>>();

            for (var i = 0; (i + MODEL_L_JUMP - 2) < modelFile.Length; i += MODEL_L_JUMP) {
                // string make, string type, string model
                string make = modelFile[i + MAKE_POSITION];
                string model = modelFile[i + MODEL_POSITION];
                string type = modelFile[i + TYPE_POSITION];
                
                // Validate fields
                bool fieldsEmpty = string.IsNullOrWhiteSpace(make) 
                    || string.IsNullOrWhiteSpace(model) || string.IsNullOrWhiteSpace(type);

                if (!fieldsEmpty) {
                    // Parse bike data
                    BikeModel importModel = new BikeModel(make, type, model);
                    
                    // Group into makes
                    if (modelDict.ContainsKey(make)) {
                        modelDict[make].Add(importModel);
                    } else {
                        modelDict[make] = new List<BikeModel> { 
                            importModel
                        };
                    }
                } else {
                    throw new NotImplementedException();
                }
            }

            return modelDict;
        } 
    }
}
