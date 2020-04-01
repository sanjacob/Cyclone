using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cyclone.Objects;
using Gtk;
using Newtonsoft.Json;

namespace Cyclone {
    class MainClass {
        private const int ROW_T_P = 0;
        private const int STORE_CODE_P = 5;

        private const int MODEL_MAKE_P = 0;
        private const int MODEL_MODEL_P = 1;
        private const int MODEL_TYPE_P = 2;

        private const int MAKE_DEPTH = 0;
        private const int MODEL_DEPTH = 1;
        private const int BIKE_DEPTH = 2;

        private const int messageID = 1;
        private static bool changesSaved = true;

        private const string baseInventory = "inventory.txt";
        private const string baseModels = "bikemake.txt";
        private static string inventorySave;
        private static string modelsSave;

        static TreeStore modelStore;
        static TreeStore bikeStore;
        static TreeStore bikeExpStore;

        // NEW MODEL
        static Dictionary<int, Bike> inventory = new Dictionary<int, Bike>();
        static List<BikeModel> validModels = new List<BikeModel>();

        static List<Sale> bikeSales = new List<Sale>();
        static List<Bike> bikePurchases = new List<Bike>();

        public static void Main(string[] args) {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string cycloneLocal = Path.Combine(localAppData, "Cyclone");
            inventorySave = Path.Combine(cycloneLocal, "inventory.json");
            modelsSave = Path.Combine(cycloneLocal, "models.json");

            Application.Init();
            MainWindow mainWindow;
            mainWindow = new MainWindow();
            mainWindow.DeleteEvent += (deleteO, deleteArgs) => OnWindowClosed(deleteO, deleteArgs, mainWindow);

            Directory.CreateDirectory(cycloneLocal);

            if (File.Exists(inventorySave)) {
                inventory = JsonConvert.DeserializeObject<Dictionary<int, Bike>>(File.ReadAllText(inventorySave));
                mainWindow.mainStatus.Push(messageID, "Recovered saved inventory");
            } else { 
                try {
                    inventory = Parser.ParseInventoryFile(File.ReadAllLines(baseInventory));
                    mainWindow.mainStatus.Push(messageID, "Recovered base inventory");
                } catch (FormatException) {
                    mainWindow.SendError("Inventory is not complete");
                } catch (FileNotFoundException) {
                    mainWindow.SendError("Inventory was not found");
                }
            }

            if (File.Exists(modelsSave)) {
                validModels = JsonConvert.DeserializeObject<List<BikeModel>>(File.ReadAllText(modelsSave));
                mainWindow.mainStatus.Push(messageID, "Recovered saved bike models");
            } else {
                try {
                    validModels = Parser.ParseModelsFile(File.ReadAllLines(baseModels));
                    mainWindow.mainStatus.Push(messageID, "Recovered base bike models");
                } catch (FormatException) {
                    mainWindow.SendError("Models list is not complete");
                } catch (FileNotFoundException) {
                    mainWindow.SendError("Models list was not found");
                }
            }

            RepopulateModelTree(validModels);
            RepopulateBikeTree(inventory);

            if (File.Exists(inventorySave)) {
                changesSaved = true;
            }
            // TODO REMOVE
            //changesSaved = true;

            mainWindow.mainStatus.Push(messageID, "Populated bike and models tree");

            mainWindow.bikeTree.Model = bikeStore;

            mainWindow.mainMenu.saveItem.Activated += (sender, e) => saveFile(sender, e, mainWindow);
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
            mainWindow.bikeToolbar.sellBikeButton.Clicked += (sender, e) => sellBikeButton(sender, e, mainWindow);
            mainWindow.bikeTree.KeyPressEvent += (o, keyArgs) => SuprRow(o, keyArgs, mainWindow);
            mainWindow.ShowAll();
            mainWindow.Show();
            Application.Run();
        }

        /// <summary>
        /// Handles the delete event.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="a">The alpha component.</param>
        private static void OnWindowClosed(object sender, DeleteEventArgs a, MainWindow mainWindow) {
            if (!changesSaved) {
                string confMsg = string.Format("Do you wish to save the inventory?");

                MessageDialog saveConfirm = new MessageDialog(mainWindow,
                    DialogFlags.DestroyWithParent,
                    MessageType.Question,
                    ButtonsType.YesNo, confMsg);

                ResponseType result = (ResponseType)saveConfirm.Run();

                if (result == ResponseType.Yes) {
                    saveAppData(mainWindow);
                }
            }
    
            Application.Quit();
            a.RetVal = true;
        }

        /// <summary>
        /// Saves to app folder.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        /// <param name="mainWindow">Main window.</param>
        private static void saveFile(object sender, EventArgs e, MainWindow mainWindow) {
            saveAppData(mainWindow);
        }

        private static void saveAppData(MainWindow mainWindow) {
            SaveInventoryToJSON();
            SaveModelsToJSON();
            changesSaved = true;

            if (mainWindow != null) {
                mainWindow.mainStatus.Push(messageID, "Inventory and valid models saved");
            }
        }

        /// <summary>
        /// Imports a correctly formatted file containing bikes.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="args">Arguments.</param>
        /// <param name="mainWindow">Main window.</param>
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
                    mainWindow.mainStatus.Push(messageID, "Imported file");
                } else {
                    mainWindow.SendError("File does not exist");
                }
            }

            importFileDialog.Destroy();
        }

        /// <summary>
        /// Exports a report.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        /// <param name="mainWindow">Main window.</param>
        private static void exportFile(object sender, EventArgs e, MainWindow mainWindow) {
            FileChooserDialog exportFileDiag = new FileChooserDialog("Create a report", 
                mainWindow, FileChooserAction.Save, 
                "Cancel",ResponseType.Cancel, "Save",ResponseType.Accept);
            
            if (exportFileDiag.Run() == (int) ResponseType.Accept) {
                Console.WriteLine(exportFileDiag.Filename);
            }

            exportFileDiag.Destroy();    
        }

        /// <summary>
        /// Clears the inventory.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        /// <param name="mainWindow">Main window.</param>
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
                mainWindow.mainStatus.Push(messageID, "Cleared Inventory");
            }

            clearDialog.Destroy();
        }

        /// <summary>
        /// Launches window to edit valid bike models.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
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

        /// <summary>
        /// Imports a correctly formatted file containing valid models.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="args">Arguments.</param>
        /// <param name="editorWindow">Editor window.</param>
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

        /// <summary>
        /// Changes the visible tree store.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        /// <param name="mainWindow">Main window.</param>
        private static void changeTreeStore(object sender, EventArgs e, MainWindow mainWindow) {
            mainWindow.mainStatus.Push(messageID, "Changed view mode");

            if (mainWindow.bikeTree.Model == bikeStore) {
                mainWindow.bikeTree.Model = bikeExpStore;
            } else {
                mainWindow.bikeTree.Model = bikeStore;
            }
        }

        private static void addBikeButton(object sender, EventArgs e, MainWindow mainWindow) {
            BikeEditor createBikeWin = new BikeEditor(GroupModels(validModels));

            Dictionary<string, List<BikeModel>> modelsGrouped = GroupModels(validModels);

            string[] makeArray = modelsGrouped.Keys.ToArray();
            List<BikeModel> makeModels = new List<BikeModel>();
            List<string> modelNames = new List<string>();

            createBikeWin.ShowAll();
            createBikeWin.Show();

            createBikeWin.saveEdit.Clicked += (saveSender, saveE) => createBike(saveSender, saveE, mainWindow, createBikeWin);
        }
        
        private static void addModelButton(object sender, EventArgs e, ModelsViewer editorWindow) {
            ModelEditor createModelWin;
            createModelWin = new ModelEditor();
    
            createModelWin.ShowAll();
            createModelWin.Show();
            
            createModelWin.saveEdit.Clicked += (saveSender, saveE) => createModel(saveSender, saveE, editorWindow, createModelWin);
        }

        private static void createBike(object sender, EventArgs e, MainWindow mainWindow, BikeEditor bikeWin) {
            try {
                Bike newBike = bikeWin.ParseBike();
                Console.WriteLine(string.Format("Adding bike of make {0} and model {1}", newBike.Make, newBike.Model));

                AddToInventory(newBike.AsInventory);
                RepopulateBikeTree(inventory);
                mainWindow.Balance();

                if (newBike.WasBought) {
                    bikePurchases.Add(newBike);
                }

                mainWindow.mainStatus.Push(messageID, "Added a new bike");
            } catch (FormatException) {
                int bikeError = bikeWin.errorType;

                if (bikeError == BikeEditor.ERROR_YEAR) {
                    mainWindow.SendError("Please enter a valid year");
                } else if (bikeError == BikeEditor.ERROR_CODE) {
                    mainWindow.SendError("Please enter a valid code");
                } else if (bikeError == BikeEditor.ERROR_COST) {
                    mainWindow.SendError("Please enter a valid price for the bike");
                } else {
                    mainWindow.SendError("Please fill in all mandatory fields");
                } 
            } catch (IndexOutOfRangeException) { 
                mainWindow.SendError("Please select both a make and model");
            } finally {
                bikeWin.Destroy();
            }
        }

        private static void createModel(object sender, EventArgs e, ModelsViewer mainWindow, ModelEditor modelWin) {
            try {
                BikeModel newBike = modelWin.ParseModel();
                Console.WriteLine(string.Format("Adding model {0} of make {1}", newBike.Model, newBike.Make));

                AddToModels(new List<BikeModel> { newBike });
                RepopulateModelTree(validModels);
            } catch (FormatException) {
                int modelError = modelWin.errorType;
                mainWindow.SendError("Please fill in all mandatory fields");
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

                        } else if (rowType == MODEL_DEPTH) {
                            // Recover in grouped store and delete every child in both stores
                            TreeIter bikeChild;
                            bool nextBike = bikeStore.IterChildren(out bikeChild, rowIter);
                            
                            while (nextBike) {
                                int securityCode = (int) bikeStore.GetValue(bikeChild, STORE_CODE_P);
                                DeleteBike(securityCode);

                                nextBike = activeStore.IterNext(ref bikeChild);
                            }

                        } else {
                            Console.WriteLine("Not implemented yet");
                        }
                    }
                }

                RepopulateBikeTree(inventory);
                mainWindow.mainStatus.Push(messageID, "Deleted some rows");
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
                            string modelMake = (string) modelStore.GetValue(rowIter, MODEL_MAKE_P);
                            string modelModel = (string) modelStore.GetValue(rowIter, MODEL_MODEL_P);
                            string modelType = (string) modelStore.GetValue(rowIter, MODEL_TYPE_P);

                            BikeModel bikeModel = new BikeModel(modelMake, modelType, modelModel, true);
                            DeleteModel(bikeModel);
                        } else if (rowType == MAKE_DEPTH) {
                            editorWindow.SendError("Can't delete make branch, instead select all models");
                        }
                    }
                }

                RepopulateModelTree(validModels);
            }
            
            md.Destroy();
        }
        
        private static void sellBikeButton(object sender, EventArgs e, MainWindow mainWindow) {
            List<Bike> soldBikes = new List<Bike>();

            // Work with currently used store
            TreeStore activeStore = (TreeStore) mainWindow.bikeTree.Model;

            // Get selected rows
            TreeSelection selectedChildren = mainWindow.bikeTree.Selection;
            TreePath[] selected = selectedChildren.GetSelectedRows();

            foreach (TreePath bikeRow in selected) {
                TreeIter rowIter;

                if (activeStore.GetIter(out rowIter, bikeRow)) {
                    int rowType = (int)activeStore.GetValue(rowIter, ROW_T_P);

                    if (rowType == BIKE_DEPTH) {
                        // Get reference code and delete from both stores
                        int secCode = (int)activeStore.GetValue(rowIter, STORE_CODE_P);
                        soldBikes.Add(inventory[secCode]);
                    }
                }
            }
            
            if (soldBikes.Count > 0 && soldBikes.Count < 11) {
                SaleWindow newSaleWin = new SaleWindow(soldBikes);
                newSaleWin.ShowAll();
                newSaleWin.Show();
                newSaleWin.Apply += (applySender, applyE) => NewBikeSale(sender, e, newSaleWin, mainWindow);
            } else {
                mainWindow.SendError("Select between 1 and 10 bikes to sell");
            }
        }

        private static void NewBikeSale(object sender, EventArgs e, SaleWindow saleWindow, MainWindow mainWindow) {
            try {
                Sale newSale = saleWindow.ParseSale();

                foreach (Bike bike in newSale.soldBikes) {
                    DeleteBike(bike.SecurityCode);
                }

                mainWindow.Balance();
                bikeSales.Add(newSale);
                RepopulateBikeTree(inventory);
            } catch (FormatException) {
                throw new NotImplementedException();
            } catch (IndexOutOfRangeException) {
                throw new NotImplementedException();
            }
        }

        private static void rowActivate(object o, RowActivatedArgs args, MainWindow mainWindow) {
            TreeIter rowActive;
            TreeStore activeStore = (TreeStore) mainWindow.bikeTree.Model;
            activeStore.GetIter(out rowActive, args.Path);

            int rowType = (int) activeStore.GetValue(rowActive, ROW_T_P);

            if (rowType == BIKE_DEPTH) {
                int code = (int) activeStore.GetValue(rowActive, STORE_CODE_P);

                BikeEditor editBikeWin = new BikeEditor(GroupModels(validModels), inventory[code]);

                editBikeWin.ShowAll();
                editBikeWin.Show();
                editBikeWin.saveEdit.Clicked += (sender, e) => OverwriteBike(sender, e, mainWindow, editBikeWin);
            }
        }

        private static void editModelButton(object sender, EventArgs e, ModelsViewer editorWindow) {
            // Get selected rows
            TreeSelection selectedChildren = editorWindow.modelTree.Selection;
            TreePath[] selected = selectedChildren.GetSelectedRows();
            
            if (selectedChildren.CountSelectedRows() == 1) {
                TreeIter editIter;

                if (modelStore.GetIter(out editIter, selected[0])) {
                    EditModel(editIter, editorWindow);
                }
            } else {
                editorWindow.SendError("Can't edit more than one model at a time");
            }
        }

        private static void modelActivate(object o, RowActivatedArgs args, ModelsViewer modelsWindow) {
            TreeIter rowActive;
            modelStore.GetIter(out rowActive, args.Path);
            EditModel(rowActive, modelsWindow);
        }

        private static void EditModel(TreeIter rowActive, ModelsViewer modelsWindow) {
            int rowType = modelStore.IterDepth(rowActive);

            if (rowType == MODEL_DEPTH) {
                string make = (string) modelStore.GetValue(rowActive, MODEL_MAKE_P);
                string model = (string) modelStore.GetValue(rowActive, MODEL_MODEL_P);
                string type = (string) modelStore.GetValue(rowActive, MODEL_TYPE_P);

                BikeModel oldModel = new BikeModel(make, type, model, true);
                ModelEditor editModelWin = new ModelEditor(oldModel);

                editModelWin.ShowAll();
                editModelWin.Show();
                editModelWin.saveEdit.Clicked += (sender, e) => OverwriteModel(sender, e, modelsWindow, editModelWin);
            }
        }

        /// <summary>
        /// Re-inserts the edited bike.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        /// <param name="mainWindow">Main window.</param>
        /// <param name="editBikeWin">Edit bike window.</param>
        private static void OverwriteBike(object sender, EventArgs e, MainWindow mainWindow, BikeEditor editBikeWin) {
            try {
                Bike newBike = editBikeWin.ParseBike();
                DeleteBike(newBike.SecurityCode);

                AddToInventory(newBike.AsInventory);
                RepopulateBikeTree(inventory);

            } catch (FormatException) {
                int bikeError = editBikeWin.errorType;

                if (bikeError == BikeEditor.ERROR_YEAR) {
                    mainWindow.SendError("Please enter a valid year");
                } else {
                    mainWindow.SendError("Please fill in all mandatory fields");
                }
            } catch (IndexOutOfRangeException) { 
                mainWindow.SendError("Please select both a make and model");
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
        private static void OverwriteModel(object sender, EventArgs e, ModelsViewer modelsWindow, ModelEditor editModelWin) {
            try {
                BikeModel newModel = editModelWin.ParseModel();
                DeleteModel(editModelWin.oldModel);

                AddToModels(new List<BikeModel> { newModel });
                RepopulateModelTree(validModels);

            } catch (FormatException) {
                int bikeError = editModelWin.errorType;
                modelsWindow.SendError("Please fill in all mandatory fields");
            } finally {
                editModelWin.Destroy();
            }
        }

        /// <summary>
        /// Repopulates the bike Trees after a change in the inventory.
        /// </summary>
        /// <param name="bikeInventory">Bike inventory list.</param>
        private static void RepopulateBikeTree(Dictionary<int, Bike> bikeInventory) {
            Dictionary<string, Dictionary<string, List<Bike>>> inventoryMap = GroupBikes(bikeInventory);

            // Create bikeStore if not yet done
            if (bikeStore == null) {
                bikeStore = new TreeStore(typeof(int), typeof(string), typeof(string),
                    typeof(string), typeof(string), typeof(int));
                
                // First attribute is an int specifying the type of row
            }
            
            // Create expanded store if not yet created            
            if (bikeExpStore == null) {
                bikeExpStore = new TreeStore(typeof(int), typeof(string), typeof(string),
                    typeof(string), typeof(string), typeof(int));
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
                            inventoryBike.Type, inventoryBike.SecurityCode);

                        // Append bike to ungrouped store, as parent node
                        TreeIter storeExpBike = bikeExpStore.AppendValues(BIKE_DEPTH, 
                            inventoryBike.Make, inventoryBike.Model, inventoryBike.Year, 
                            inventoryBike.Type, inventoryBike.SecurityCode);

                        // TODO: Add Cost attribute
                    }
                }
            }

            changesSaved = false;
        }

        /// <summary>
        /// Groups the bikes by make and model, to later display in a Tree.
        /// </summary>
        /// <returns>The bikes.</returns>
        /// <param name="bikeInventory">Bike inventory.</param>
        private static Dictionary<string, Dictionary<string, List<Bike>>> GroupBikes(Dictionary<int, Bike> bikeInventory) {
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

            return inventoryMap;
        }

        /// <summary>
        /// Repopulates the model tree.
        /// </summary>
        /// <param name="modelRegistry">Model list.</param>
        private static void RepopulateModelTree(List<BikeModel> modelRegistry) {
            Dictionary<string, List<BikeModel>> modelMap = GroupModels(modelRegistry);

            // Create modelStore if not yet done
            if (modelStore == null) {
                modelStore = new TreeStore(typeof(string), typeof(string), typeof(string));
            }

            modelStore.Clear();

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
        /// Groups model list by make.
        /// </summary>
        /// <returns>The models.</returns>
        /// <param name="modelRegistry">Model registry.</param>
        private static Dictionary<string, List<BikeModel>> GroupModels(List<BikeModel> modelRegistry) {
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

            return modelMap;
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

        private static void SaveInventoryToJSON() {
            string inventoryJSON = JsonConvert.SerializeObject(inventory);
            File.WriteAllText(inventorySave, inventoryJSON);
        }

        private static void SaveModelsToJSON() {
            string modelsJSON = JsonConvert.SerializeObject(validModels);
            File.WriteAllText(modelsSave, modelsJSON);
        }
    }
}