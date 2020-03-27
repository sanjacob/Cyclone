using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gtk;

namespace Cyclone {
    class MainClass {
        private const int ROW_T_P = 0;
        private const int STORE_MAKE_P = 1;
        private const int STORE_MODEL_P = 2;
        private const int STORE_TYPE_P = 4;
        private const int STORE_YEAR_P = 3;
        private const int STORE_W_S_P = 5;
        private const int STORE_FORKS_P = 6;
        private const int STORE_CODE_P = 7;

        private const int MODEL_MAKE_P = 0;
        private const int MODEL_MODEL_P = 1;
        private const int MODEL_TYPE_P = 2;

        private const int MAKE_DEPTH = 0;
        private const int MODEL_DEPTH = 1;
        private const int BIKE_DEPTH = 2;

        private const string baseInventory = "inventory.txt";
        private const string baseModels = "bikemake.txt";

        static TreeStore modelStore;
        static TreeStore bikeStore;
        static TreeStore bikeExpStore;

        // NEW MODEL
        static Dictionary<int, Bike> inventory;
        static List<BikeModel> validModels;

        static Dictionary<string, List<string>> modelsDict = new Dictionary<string, List<string>> ();
        static Dictionary<string, string> modelTypeDict = new Dictionary<string, string>();

        public static void Main(string[] args) {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string cycloneLocal = Path.Combine(localAppData, "Cyclone");
            string inventorySave = Path.Combine(cycloneLocal, "inventory.json");
            string modelsSave = Path.Combine(cycloneLocal, "models.json");

            Directory.CreateDirectory(cycloneLocal);

            if (File.Exists(inventorySave)) { 
                
            }

            Application.Init();
            MainWindow mainWindow;
            mainWindow = new MainWindow();

            try {
                inventory = Parser.ParseInventoryFile(File.ReadAllLines(baseInventory));
            } catch (FormatException) {
                mainWindow.SendError("Inventory is not complete");
            } catch (FileNotFoundException) {
                mainWindow.SendError("Inventory was not found");
            }
            
            try {
                validModels = Parser.ParseModelsFile(File.ReadAllLines(baseModels));
            } catch (FormatException) {
                mainWindow.SendError("Models list is not complete");
            } catch (FileNotFoundException) {
                mainWindow.SendError("Models list was not found");
            }

            RepopulateModelTree(validModels);
            RepopulateBikeTree(inventory);
          
            mainWindow.bikeTree.Model = bikeStore;

            mainWindow.mainMenu.importItem.Activated += (sender, e) => importFile(sender, e, mainWindow);
            mainWindow.mainMenu.exportItem.Activated += (sender, e) => exportFile(sender, e, mainWindow);
            mainWindow.mainMenu.modelsItem.Activated += (sender, e) => editModels(sender, e);
            mainWindow.mainMenu.clearItem.Activated += (sender, e) => clearInventory(sender, e, mainWindow);

            mainWindow.mainMenu.changeView.Toggled += (sender, e) => changeTreeStore(sender, e, mainWindow);
            
            mainWindow.BikeCount();
            
            bikeStore.RowInserted += delegate {
                mainWindow.BikeCount();
            };
            
            bikeStore.RowDeleted += delegate {
                mainWindow.BikeCount();
            };
            
            mainWindow.bikeTree.RowActivated += (o, rowArgs) => rowActivate(o, rowArgs, mainWindow);
            mainWindow.bikeToolbar.addBikeButton.Clicked += (sender, e) => addBikeButton(sender, e, mainWindow);
            mainWindow.bikeToolbar.removeBikeButton.Clicked += (sender, e) => removeBikeButton(sender, e, mainWindow);
            mainWindow.bikeTree.KeyPressEvent += (o, keyArgs) => SuprRow(o, keyArgs, mainWindow);
            mainWindow.ShowAll();
            mainWindow.Show();
            Application.Run();
        }

        private static void importFile(object sender, EventArgs args, MainWindow mainWindow) {
            FileChooserDialog importFileDialog = new FileChooserDialog("Import an existing inventory file", 
                mainWindow, FileChooserAction.Open, 
                "Cancel",ResponseType.Cancel, "Open",ResponseType.Accept);

            if (importFileDialog.Run() == (int) ResponseType.Accept) {
                string importFilename = importFileDialog.Filename;

                if (File.Exists(importFilename)) {
                    int oldBikeCount = Bike.BikeCount;

                    Dictionary<int, Bike> newImportedInv = Parser.ParseInventoryFile(File.ReadAllLines(importFilename));
                    AddToInventory(newImportedInv);
                    RepopulateBikeTree(inventory);
                    int elementsAdded = Bike.BikeCount - oldBikeCount;

                    Console.WriteLine(String.Format("Imported {0} bikes from file {1}", elementsAdded, importFilename));
                } else {
                    mainWindow.SendError("File does not exist");
                }
            }

            importFileDialog.Destroy();
        }

        private static void exportFile(object sender, EventArgs e, MainWindow mainWindow) {
            FileChooserDialog exportFileDiag = new FileChooserDialog("Create a report", 
                mainWindow, FileChooserAction.Save, 
                "Cancel",ResponseType.Cancel, "Save",ResponseType.Accept);
            
            if (exportFileDiag.Run() == (int) ResponseType.Accept) {
                Console.WriteLine(exportFileDiag.Filename);
            }

            exportFileDiag.Destroy();    
        }

        private static void clearInventory(object sender, EventArgs e, MainWindow mainWindow) {
            string confMsg = string.Format( "Are you sure you want to delete ALL {0} bikes in the inventory?", Bike.BikeCount);

             MessageDialog clearDialog = new MessageDialog (mainWindow,
                DialogFlags.DestroyWithParent,
                MessageType.Warning, 
                ButtonsType.YesNo, confMsg);
    
            ResponseType result = (ResponseType) clearDialog.Run ();

            if (result == ResponseType.Yes) {
                Bike.RemoveBike(Bike.BikeCount);
                inventory.Clear();
                RepopulateBikeTree(inventory);

                Console.WriteLine("Cleared Inventory");
            }

            clearDialog.Destroy();
        }

        private static void editModels(object sender, EventArgs e) {
            ModelsViewer modelsWin;
            modelsWin = new ModelsViewer();
            modelsWin.modelTree.Model = modelStore;

            modelsWin.modelsToolbar.removeModelButton.Clicked += (removeSender, removeE) => removeModelButton(removeSender, removeE, modelsWin);
            modelsWin.modelsToolbar.addModelButton.Clicked += (addSender, addE) => addModelButton(addSender, addE, modelsWin);
            modelsWin.modelsToolbar.editModelButton.Clicked += (editSender, editE) => editModelButton(editSender,editE, modelsWin);

            modelsWin.modelTree.KeyPressEvent += (o, keyArgs) => SuprModel(o, keyArgs, modelsWin);
            modelsWin.modelTree.RowActivated += (o, rowArgs) => modelActivate(o, rowArgs, modelsWin);

            modelsWin.modelsMenu.importItem.Activated += (importSender, importE) => importModelsFile(importSender, importE, modelsWin);
            modelsWin.ModelCount();
            
            modelStore.RowInserted += delegate {
                modelsWin.ModelCount();
            };
            
            modelStore.RowDeleted += delegate {
                modelsWin.ModelCount();
            };

            modelsWin.ShowAll();
            modelsWin.Show();
        }
        
        private static void importModelsFile(object sender, EventArgs args, ModelsViewer editorWindow) {
            FileChooserDialog importFileDialog = new FileChooserDialog("Import an existing inventory file", 
                editorWindow, FileChooserAction.Open, 
                "Cancel",ResponseType.Cancel, "Open",ResponseType.Accept);
            
            if (importFileDialog.Run() == (int) ResponseType.Accept) {
                string importFilename = importFileDialog.Filename;
                
                if (File.Exists(importFilename)) {
                    List<BikeModel> importedModels = new List<BikeModel>();
                    int oldModelCount = BikeModel.ModelCount;

                    try {
                        importedModels = Parser.ParseModelsFile(File.ReadAllLines(importFilename));
                        AddToModels(importedModels);
                        RepopulateModelTree(validModels);

                        int elementsAdded = BikeModel.ModelCount - oldModelCount;
                        Console.WriteLine(String.Format("Imported {0} models from file {1}", elementsAdded, importFilename));

                    } catch (FormatException) {
                        editorWindow.SendError("Models were not complete, none imported");
                    }
                } else {
                    editorWindow.SendError("File does not exist");
                }
            }

            importFileDialog.Destroy();    
        }
        
        private static void changeTreeStore(object sender, EventArgs e, MainWindow mainWindow) {
            if (mainWindow.bikeTree.Model == bikeStore) {
                mainWindow.bikeTree.Model = bikeExpStore;
            } else {
                mainWindow.bikeTree.Model = bikeStore;
            }
        }

        private static void addBikeButton(object sender, EventArgs e, MainWindow mainWindow) {
            BikeEditor createBikeWin = new BikeEditor();
            string[] makeArray = modelsDict.Keys.ToArray();
            string[] modelArray = { };
            createBikeWin.makeCombo(makeArray);

            createBikeWin.bikeMakeC.Changed += delegate {
                int makeIndex = createBikeWin.bikeMakeC.Active;
                string make = makeArray[makeIndex];
                modelArray = modelsDict[make].ToArray();
                createBikeWin.modelCombo(modelArray);
            };

            createBikeWin.bikeModelC.Changed += delegate {
                int modelIndex = createBikeWin.bikeModelC.Active;
                if (modelIndex != -1) {
                    string model = modelArray[modelIndex];
                    createBikeWin.type = modelTypeDict[model];
                } else {
                    createBikeWin.type = "";
                }
            };

            createBikeWin.ShowAll();
            createBikeWin.Show();

            createBikeWin.saveBike.Clicked += (saveSender, saveE) => createBike(saveSender, saveE, mainWindow, createBikeWin);
        }
        
        private static void addModelButton(object sender, EventArgs e, ModelsViewer editorWindow) {
            ModelEditor createModelWin;
            createModelWin = new ModelEditor();
    
            createModelWin.ShowAll();
            createModelWin.Show();
            
            createModelWin.saveBike.Clicked += (saveSender, saveE) => createModel(saveSender, saveE, editorWindow, createModelWin);
        }

        private static void createBike(object sender, EventArgs e, MainWindow mainWindow, BikeEditor bikeWin) {
            try {
                Bike newBike = bikeWin.getBike(modelsDict);
                Console.WriteLine(string.Format("Adding bike of make {0} and model {1}", newBike.Make, newBike.Model));

                AddToInventory(newBike.AsInventory);
                RepopulateBikeTree(inventory);

            } catch (FormatException) {
                int bikeError = bikeWin.errorType;

                if (bikeError == BikeEditor.ERROR_YEAR) {
                    mainWindow.SendError("Please enter a valid year");
                } else if (bikeError == ModelEditor.ERROR_EMPTY) {
                    mainWindow.SendError("Please fill in all mandatory fields");
                } else if (bikeError == BikeEditor.ERROR_CODE) {
                    mainWindow.SendError("Please enter a valid code");
                }
            }

            bikeWin.Destroy();
        }

        private static void createModel(object sender, EventArgs e, ModelsViewer mainWindow, ModelEditor modelWin) {
            try {
                BikeModel newBike = modelWin.getModel();
                Console.WriteLine(string.Format("Adding model {0} of make {1}", newBike.Model, newBike.Make));

                AddToModels(new List<BikeModel> { newBike });
                RepopulateModelTree(validModels);
            } catch (FormatException) {
                int modelError = modelWin.errorType;

                if (modelError == ModelEditor.ERROR_EMPTY) {
                    mainWindow.SendError("Please fill in all mandatory fields");
                }
            }

            modelWin.Destroy();
        }

        private static void SuprRow(object o, KeyPressEventArgs args, MainWindow mainWindow) {
            if (args.Event.Key == Gdk.Key.Delete) {
                deleteBikeTreeRow(mainWindow);
            }
        }

        private static void SuprModel(object o, KeyPressEventArgs args, ModelsViewer editorWindow) {
            if (args.Event.Key == Gdk.Key.Delete) {
                deleteModelTreeRow(editorWindow);
            }
        }

        private static void removeBikeButton(object sender, EventArgs e, MainWindow mainWindow) {
            deleteBikeTreeRow(mainWindow);
        }

        private static void removeModelButton(object sender, EventArgs e, ModelsViewer editorWindow) {
            deleteModelTreeRow(editorWindow);
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
                        int rowType =  (int) activeStore.GetValue(rowIter, ROW_T_P);

                        if (rowType == BIKE_DEPTH) {
                            // Get reference code and delete from both stores
                            int secCode = (int) activeStore.GetValue(rowIter, STORE_CODE_P);
                            DeleteBike(secCode);
                            RepopulateBikeTree(inventory);

                        } else if (rowType == MODEL_DEPTH) {
                            // Recover in grouped store and delete every child in both stores
                            TreeIter bikeChild;
                            bool nextBike = bikeStore.IterChildren(out bikeChild, rowIter);
                            
                            while (nextBike) {
                                int securityCode = (int) bikeStore.GetValue(bikeChild, STORE_CODE_P);
                                DeleteBike(securityCode);

                                nextBike = activeStore.IterNext(ref bikeChild);
                            }

                            RepopulateBikeTree(inventory);
                        } else {
                            Console.WriteLine("Not implemented yet");
                        }
                    }
                }
            }
            
            md.Destroy();
        }

        private static void deleteModelTreeRow(ModelsViewer editorWindow) {
            // Get selected rows
            TreeSelection selectedChildren = editorWindow.modelTree.Selection;
            TreePath[] selected = selectedChildren.GetSelectedRows();
            
            // Confirm deletion
            string pluralNoun = "";
            if (selectedChildren.CountSelectedRows() > 1) {
                pluralNoun = "s";
            }

            string confMsg = string.Format( "Are you sure you want to delete {0} row{1}?", 
                selectedChildren.CountSelectedRows(), pluralNoun);
            
            MessageDialog md = new MessageDialog (editorWindow, 
                DialogFlags.DestroyWithParent,
                MessageType.Question, 
                ButtonsType.YesNo, confMsg);
    
            ResponseType result = (ResponseType) md.Run ();

            if (result == ResponseType.Yes) {
                // Get each element from last to first, to preserve iter order
                Array.Reverse(selected);
                
                foreach (TreePath modelRow in selected) {
                    TreeIter rowIter;
    
                    if (modelStore.GetIter(out rowIter, modelRow)) {
                        int rowType =  modelStore.IterDepth(rowIter);

                        if (rowType == MODEL_DEPTH) {
                            // Get reference code and delete from both stores
                            //DeleteModel(rowIter);
                        } else if (rowType == MAKE_DEPTH) {
                            editorWindow.SendError("Can't delete make branch, instead select all models");
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

            int rowType = (int) activeStore.GetValue(rowActive, ROW_T_P);

            if (rowType == BIKE_DEPTH) {
                string make = (string) activeStore.GetValue(rowActive, STORE_MAKE_P);
                string model = (string) activeStore.GetValue(rowActive, STORE_MODEL_P);
                string type = (string) activeStore.GetValue(rowActive, STORE_TYPE_P);

                int year;
                int.TryParse((string) activeStore.GetValue(rowActive, STORE_YEAR_P), out year);

                string wheelSize = (string) activeStore.GetValue(rowActive, STORE_W_S_P);
                string forks = (string) activeStore.GetValue(rowActive, STORE_FORKS_P);
                int code = (int) activeStore.GetValue(rowActive, STORE_CODE_P);

                BikeEditor editBikeWin;
                editBikeWin = new BikeEditor(make, model, year, type, wheelSize, forks, code);
                string[] makeArray = modelsDict.Keys.ToArray();
                string[] modelArray = { };
                editBikeWin.makeCombo(makeArray);
                
                int selectMake = Array.IndexOf(makeArray, make);
                editBikeWin.bikeMakeC.Active = selectMake;

                modelArray = modelsDict[make].ToArray();
                int selectModel = Array.IndexOf(modelArray, model);
                editBikeWin.modelCombo(modelArray);
                editBikeWin.bikeModelC.Active = selectModel;
                editBikeWin.type = modelTypeDict[model];
                          
                editBikeWin.bikeMakeC.Changed += delegate {
                    int makeIndex = editBikeWin.bikeMakeC.Active;
                    string makeParsed = makeArray[makeIndex];
                    modelArray = modelsDict[makeParsed].ToArray();
                    editBikeWin.modelCombo(modelArray);
                };
    
                editBikeWin.bikeModelC.Changed += delegate {
                    int modelIndex = editBikeWin.bikeModelC.Active;
                    if (modelIndex != -1) {
                        string modelParsed = modelArray[modelIndex];
                        editBikeWin.type = modelTypeDict[modelParsed];
                    } else {
                        editBikeWin.type = "";
                    }
                };

                editBikeWin.ShowAll();
                editBikeWin.Show();
                editBikeWin.saveBike.Clicked += (sender, e) => EditBike(sender, e, mainWindow, editBikeWin);
            }
        }

        private static void editModelButton(object sender, EventArgs e, ModelsViewer editorWindow) {
            // Get selected rows
            TreeSelection selectedChildren = editorWindow.modelTree.Selection;
            TreePath[] selected = selectedChildren.GetSelectedRows();
            
            if (selectedChildren.CountSelectedRows() == 1) {
                TreeIter editIter;

                if (modelStore.GetIter(out editIter, selected[0])) {
                    editModel(editIter, editorWindow);
                }
            } else {
                editorWindow.SendError("Can't edit more than one model at a time");
            }
        }

        private static void modelActivate(object o, RowActivatedArgs args, ModelsViewer modelsWindow) {
            TreeIter rowActive;
            modelStore.GetIter(out rowActive, args.Path);
            editModel(rowActive, modelsWindow);
        }

        private static void editModel(TreeIter rowActive, ModelsViewer modelsWindow) {
            int rowType = modelStore.IterDepth(rowActive);

            if (rowType == MODEL_DEPTH) {
                string make = (string) modelStore.GetValue(rowActive, MODEL_MAKE_P);
                string model = (string) modelStore.GetValue(rowActive, MODEL_MODEL_P);
                string type = (string) modelStore.GetValue(rowActive, MODEL_TYPE_P);

                ModelEditor editModelWin = new ModelEditor(make, model, type);

                editModelWin.ShowAll();
                editModelWin.Show();
                editModelWin.saveBike.Clicked += (sender, e) => EditModel(sender, e, modelsWindow, editModelWin);
            }
        }

        /// <summary>
        /// Re-inserts the edited bike.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        /// <param name="mainWindow">Main window.</param>
        /// <param name="editBikeWin">Edit bike window.</param>
        private static void EditBike(object sender, EventArgs e, MainWindow mainWindow, BikeEditor editBikeWin) {
            try {
                Bike newBike = editBikeWin.getBike(modelsDict);
                DeleteBike(newBike.SecurityCode);

                AddToInventory(newBike.AsInventory);
                RepopulateBikeTree(inventory);

            } catch (FormatException) {
                int bikeError = editBikeWin.errorType;

                if (bikeError == BikeEditor.ERROR_YEAR) {
                    mainWindow.SendError("Please enter a valid year");
                } else if (bikeError == ModelEditor.ERROR_EMPTY) {
                    mainWindow.SendError("Please fill in all mandatory fields");
                }
            } finally {
                editBikeWin.Destroy();
            }
        }
        
        /// <summary>
        /// Re-insert the edited model.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        /// <param name="modelsWindow">Models window.</param>
        /// <param name="editModelWin">Edit model window.</param>
        private static void EditModel(object sender, EventArgs e, ModelsViewer modelsWindow, ModelEditor editModelWin) {
            try {
                BikeModel newModel = editModelWin.getModel();
                DeleteModel(newModel);

                AddToModels(new List<BikeModel> { newModel });
                RepopulateModelTree(validModels);

            } catch (FormatException) {
                int bikeError = editModelWin.errorType;

                if (bikeError == ModelEditor.ERROR_EMPTY) {
                    modelsWindow.SendError("Please fill in all mandatory fields");
                }
            } finally {
                editModelWin.Destroy();
            }
        }

        /// <summary>
        /// Repopulates the bike Trees after a change in the inventory.
        /// </summary>
        /// <param name="bikeInventory">Bike inventory list.</param>
        private static void RepopulateBikeTree(Dictionary<int, Bike> bikeInventory) {
            Dictionary<string, Dictionary<string, List<Bike>>> inventoryMap = new Dictionary<string, Dictionary<string, List<Bike>>>();

            // For every bike in the inventory parameter
            foreach (Bike storedBike in bikeInventory.Values) {

                // Check if make is already in map
                if (inventoryMap.ContainsKey(storedBike.Make)) {
                    // Check if model is in map
                    if (inventoryMap[storedBike.Make].ContainsKey(storedBike.Model)) {
                        // Make and model were already present, add bike to model
                        inventoryMap[storedBike.Make][storedBike.Model].Add(storedBike);
                    } else {
                        // Model was not present, create it as list and insert bike
                        inventoryMap[storedBike.Make][storedBike.Model] = new List<Bike> {
                            storedBike
                        };
                    }
                } else {
                    // Make was not present, create make dictionary and model list, insert bike
                    inventoryMap[storedBike.Make] = new Dictionary<string, List<Bike>> {
                        [storedBike.Model] = new List<Bike> { 
                            storedBike 
                        }
                    };
                }
            }
            
            // Create bikeStore if not yet done
            if (bikeStore == null) {
                bikeStore = new TreeStore(typeof(int), typeof(string), typeof(string),
                    typeof(string), typeof(string), typeof(string), typeof(string), typeof(int));
                
                // First attribute is an int specifying the type of row
            }
            
            // Create expanded store if not yet created            
            if (bikeExpStore == null) {
                bikeExpStore = new TreeStore(typeof(int), typeof(string), typeof(string),
                    typeof(string), typeof(string), typeof(string), typeof(string), typeof(int));
            }

            // Clear both bike stores to re-add contents
            bikeExpStore.Clear();
            bikeStore.Clear();

            // For each bike make
            foreach (string inventoryMake in inventoryMap.Keys) {
                // Append make parent node to TreeStore
                TreeIter storeMake = bikeStore.AppendValues(MAKE_DEPTH, inventoryMake);
                
                // For each bike model of that make
                foreach (string inventoryModel in inventoryMap[inventoryMake].Keys) {
                    // Append model node to TreeStore inside make
                    TreeIter storeModel = bikeStore.AppendValues(storeMake, MODEL_DEPTH, inventoryMake, inventoryModel);
                    
                    // For each bike of that model of that make
                    foreach (Bike inventoryBike in inventoryMap[inventoryMake][inventoryModel]) {
                        // Append bike child node to TreeStore inside model node, inside make parent node
                        TreeIter storeBike = bikeStore.AppendValues(storeModel, BIKE_DEPTH, 
                            inventoryBike.Make, inventoryBike.Model, inventoryBike.Year, 
                            inventoryBike.Type, inventoryBike.WheelSize, inventoryBike.Forks,
                            inventoryBike.SecurityCode);

                        // Append bike to ungrouped store, as parent node
                        TreeIter storeExpBike = bikeExpStore.AppendValues(BIKE_DEPTH, 
                            inventoryBike.Make, inventoryBike.Model, inventoryBike.Year, 
                            inventoryBike.Type, inventoryBike.WheelSize, inventoryBike.Forks,
                            inventoryBike.SecurityCode);

                        // TODO: Add Cost attribute
                    }
                }
            }
        }
        
        /// <summary>
        /// Repopulates the model tree.
        /// </summary>
        /// <param name="modelRegistry">Model list.</param>
        private static void RepopulateModelTree(List<BikeModel> modelRegistry) {
            Dictionary<string, List<BikeModel>> modelMap = new Dictionary<string, List<BikeModel>>();

            // For every model in the model list parameter
            foreach (BikeModel storedModel in modelRegistry) {
                // Check if make is already in map
                if (modelMap.ContainsKey(storedModel.Make)) {
                    modelMap[storedModel.Make].Add(storedModel);
                } else {
                    // Make was not present, create it as list and insert model
                    modelMap[storedModel.Make] = new List<BikeModel> {
                        storedModel
                    };
                }
            }

            // Create modelStore if not yet done
            if (modelStore == null) {
                modelStore = new TreeStore(typeof(string), typeof(string), typeof(string));
            }

            // For each model make
            foreach (string modelMake in modelMap.Keys) {
                // Append make parent node to TreeStore
                TreeIter storeMake = modelStore.AppendValues(modelMake);
                
                // For each bike of that model of that make
                foreach (BikeModel validModel in modelMap[modelMake]) {
                    // Append model to store, inside make
                    TreeIter storeModel = modelStore.AppendValues(storeMake,
                        validModel.Make, validModel.Model, validModel.Type);
                }
            }
        }

        /// <summary>
        /// Deletes a bike from the inventory.
        /// </summary>
        /// <param name="secCode">Security code.</param>
        private static void DeleteBike(int secCode) {
            // Decrease count now so that deleted event can update indicator accordingly
            Bike.RemoveBike();
            inventory.Remove(secCode);
            Console.WriteLine(string.Format("Deleted bike with code {0}", secCode));
        }

        /// <summary>
        /// Deletes the model from the list of valid models.
        /// </summary>
        /// <param name="modelToDelete">Model to delete.</param>
        private static void DeleteModel(BikeModel modelToDelete) {
            // Decrease count now so that deleted event can update indicator accordingly            
            BikeModel.RemoveModel();
            validModels.Remove(modelToDelete);
            Console.WriteLine(string.Format("Deleted {0}", modelToDelete.Model));
        }

        /// <summary>
        /// Merges a new inventory with the current one.
        /// </summary>
        /// <param name="newInventory">Inventory to be added.</param>
        private static void AddToInventory(Dictionary<int, Bike> newInventory) {
            // For each new bike, check if the code is new
            foreach (int securityCode in newInventory.Keys) {
                if (inventory.ContainsKey(securityCode)) {
                    // Log duplicate code error
                    Console.WriteLine(
                        string.Format("Could not add bike with code {0} as it already exists", 
                        securityCode));
                } else {
                    // Add to current inventory
                    inventory[securityCode] = newInventory[securityCode];
                }
            }
        }

        /// <summary>
        /// Adds to valid models.
        /// </summary>
        /// <param name="newModels">List of  new models.</param>
        private static void AddToModels(List<BikeModel> newModels) {
            // For each new model
            foreach(BikeModel newModel in newModels) {
                // Check for duplicates
                if (validModels.Contains(newModel)) {
                    // Log duplicate model error
                    Console.WriteLine(
                        string.Format("Could not add model {0} as it already exists", 
                        newModel.Model));
                } else {
                    validModels.Add(newModel);
                }
            }
        }
    }
}
