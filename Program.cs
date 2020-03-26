using System;
using System.Collections.Generic;
using System.IO;
using Gtk;

namespace Cyclone {
    class MainClass {
        public const int LINE_JUMP = 8;
        public const int MODEL_L_JUMP = 4;

        private const int MAKE_POSITION = 0;
        private const int MODEL_POSITION = 2;
        private const int TYPE_POSITION = 1;
        private const int YEAR_POSITION = 3;
        private const int WHEEL_S_POSITION = 4;
        private const int FORKS_POSITION = 5;
        private const int CODE_POSITION = 6;

        private const int ROW_T_P = 0;
        private const int MODEL_MAKE_P = 0;
        private const int STORE_MAKE_P = 1;
        private const int MODEL_MODEL_P = 1;
        private const int STORE_MODEL_P = 2;
        private const int MODEL_TYPE_P = 2;
        private const int STORE_TYPE_P = 4;
        private const int STORE_YEAR_P = 3;
        private const int STORE_W_S_P = 5;
        private const int STORE_FORKS_P = 6;
        private const int STORE_CODE_P = 7;

        private const int MAKE_DEPTH = 0;
        private const int MODEL_DEPTH = 1;
        private const int BIKE_DEPTH = 2;

        private const string baseInventory = "inventory.txt";
        private const string baseModels = "bikemake.txt";
        private const string expandKey = "_expanded";
        private const string groupedKey = "_grouped";
        
        static TreeStore modelStore;
        static TreeStore bikeStore;
        static TreeStore bikeExpStore;

        static Dictionary<int, Dictionary<string, TreeIter>> securityMap = new Dictionary<int, Dictionary<string, TreeIter>>();        
        static Dictionary<string, Dictionary<string, TreeIter>> bikeMap = new Dictionary<string, Dictionary<string, TreeIter>>();
        static Dictionary<string, TreeIter> modelMap = new Dictionary<string, TreeIter>();

        
        public static void Main(string[] args) {
            Application.Init();
            MainWindow mainWindow;
            mainWindow = new MainWindow();

            Dictionary<string, Dictionary<string, List<Bike>>> bikeDict = new Dictionary<string, Dictionary<string, List<Bike>>>();
            Dictionary<string, List<BikeModel>> modelsDict = new Dictionary<string, List<BikeModel>>();
            
            try {
                string[] baseInvFile = File.ReadAllLines(baseInventory);
                bikeDict = parseInventoryFile(baseInvFile);
            } catch (FormatException) {
                sendError("Inventory is not complete", mainWindow);
            } catch (FileNotFoundException) {
                sendError("Inventory was not found", mainWindow);
            }
            
            try {
                string[] baseModFile = File.ReadAllLines(baseModels);
                modelsDict = parseModelFile(baseModFile);
            } catch (FormatException) {
                sendError("Models list is not complete", mainWindow);
            } catch (FileNotFoundException) {
                sendError("Models list was not found", mainWindow);
            }

            populateModelStore(modelsDict);
            populateBikeStore(bikeDict);

          
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
        
        private static void suprModel(object o, KeyPressEventArgs args, ModelsViewer editorWindow) {
            if (args.Event.Key == Gdk.Key.Delete) {
                deleteModelTreeRow(editorWindow);
            }
        }

        private static void changeTreeStore(object sender, EventArgs e, MainWindow mainWindow) {
            if (mainWindow.bikeTree.Model == bikeStore) {
                mainWindow.bikeTree.Model = bikeExpStore;
            } else {
                mainWindow.bikeTree.Model = bikeStore;
            }
        }

        private static void updateBikeCount(MainWindow mainWindow) {
            string pluralNoun = "s";
            if (Bike.BikeCount == 1) {
                pluralNoun = "";
            }
            mainWindow.bikeAmount.Text = string.Format("{0} bike{1} in inventory", Bike.BikeCount, pluralNoun);
        }

        private static void updateModelCount(ModelsViewer editorWindow) {
            string pluralNoun = "s";
            if (Bike.BikeCount == 1) {
                pluralNoun = "";
            }
            editorWindow.modelAmount.Text = string.Format("{0} model{1} in inventory", BikeModel.ModelCount, pluralNoun);
        }

        private static void addBikeButton(object sender, EventArgs e, MainWindow mainWindow) {
            BikeEditor createBikeWin;
            createBikeWin = new BikeEditor();
    
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
                            deleteModel(rowIter);
                        } else if (rowType == MAKE_DEPTH) {
                            sendError("Can't delete make branch, instead select all models", editorWindow);
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

                editBikeWin.ShowAll();
                editBikeWin.Show();
                editBikeWin.saveBike.Clicked += (sender, e) => editBike(sender, e, mainWindow, editBikeWin, rowActive);
            }
        }
        
        private static void modelActivate(object o, RowActivatedArgs args, ModelsViewer modelsWindow) {
            TreeIter rowActive;
            modelStore.GetIter(out rowActive, args.Path);
            editModel(rowActive, modelsWindow);
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
                sendError("Can't edit more than one model at a time", editorWindow);
            }
        }

        private static void editModel(TreeIter rowActive, ModelsViewer modelsWindow) {
            int rowType = modelStore.IterDepth(rowActive);

            if (rowType == MODEL_DEPTH) {
                string make = (string)modelStore.GetValue(rowActive, MODEL_MAKE_P);
                string model = (string)modelStore.GetValue(rowActive, MODEL_MODEL_P);
                string type = (string)modelStore.GetValue(rowActive, MODEL_TYPE_P);

                ModelEditor editModelWin = new ModelEditor(make, model, type);

                editModelWin.ShowAll();
                editModelWin.Show();
                editModelWin.saveBike.Clicked += (sender, e) => editModel(sender, e, modelsWindow, editModelWin, rowActive);
            }

        }

        private static void createBike(object sender, EventArgs e, MainWindow mainWindow, BikeEditor bikeWin) {
            try {
                Bike newBike = bikeWin.getBike();
                Console.WriteLine(string.Format("Adding bike of make {0} and model {1}", newBike.Make, newBike.Model));
                
                Dictionary<string, Dictionary<string, List<Bike>>> newBikeDict = new Dictionary<string, Dictionary<string, List<Bike>>> {
                    [newBike.Make] = new Dictionary<string, List<Bike>> {
                        [newBike.Model] = new List<Bike> {newBike}
                    }
                };

                populateBikeStore(newBikeDict);

            } catch (FormatException) {
                int bikeError = bikeWin.errorType;

                if (bikeError == BikeEditor.ERROR_YEAR) {
                    sendError("Please enter a valid year", mainWindow);
                } else if (bikeError == ModelEditor.ERROR_EMPTY) {
                    sendError("Please fill in all mandatory fields", mainWindow);
                }
            }

            bikeWin.Destroy();
        }
        
        private static void createModel(object sender, EventArgs e, ModelsViewer mainWindow, ModelEditor modelWin) {
            try {
                BikeModel newBike = modelWin.getModel();
                Console.WriteLine(string.Format("Adding model {0} of make {1}", newBike.Model, newBike.Make));

                Dictionary<String, List<BikeModel>> newModelDict = new Dictionary<string, List<BikeModel>> { 
                    [newBike.Make] = new List<BikeModel> { newBike }
                };

                populateModelStore(newModelDict);

            } catch (FormatException) {
                int modelError = modelWin.errorType;

                if (modelError == ModelEditor.ERROR_EMPTY) {
                    sendError("Please fill in all mandatory fields", mainWindow);
                }
            }

            modelWin.Destroy();
        }
        
        private static void editBike(object sender, EventArgs e, MainWindow mainWindow, BikeEditor editBikeWin, TreeIter rowActive) {
            try {
                Bike newBike = editBikeWin.getBike();
                deleteElement(newBike.SecurityCode);
                
                 Dictionary<string, Dictionary<string, List<Bike>>> editedBike = new Dictionary<string, Dictionary<string, List<Bike>>> {
                    [newBike.Make] = new Dictionary<string, List<Bike>> {
                        [newBike.Model] = new List<Bike> {newBike}
                    }
                 };

                populateBikeStore(editedBike);

                /*TreeIter expIter = securityMap[newBike.SecurityCode][expandKey];
                TreeIter groupIter = securityMap[newBike.SecurityCode][groupedKey];
                
                bikeStore.SetValues(groupIter, BIKE_DEPTH, newBike.Make,
                    newBike.Model, newBike.Year, newBike.Type, newBike.WheelSize, newBike.Forks, newBike.SecurityCode);

                bikeExpStore.SetValues(expIter, BIKE_DEPTH, newBike.Make,
                    newBike.Model, newBike.Year, newBike.Type, newBike.WheelSize, newBike.Forks, newBike.SecurityCode);*/

            } catch (FormatException) {
                int bikeError = editBikeWin.errorType;

                if (bikeError == BikeEditor.ERROR_YEAR) {
                    sendError("Please enter a valid year", mainWindow);
                } else if (bikeError == ModelEditor.ERROR_EMPTY) {
                    sendError("Please fill in all mandatory fields", mainWindow);
                }
            }
              
                editBikeWin.Destroy();
        }
        
        private static void editModel(object sender, EventArgs e, ModelsViewer modelsWindow, ModelEditor editModelWin, TreeIter rowActive) {
            try {
                BikeModel newModel = editModelWin.getModel();
                deleteModel(rowActive);

                 Dictionary<String, List<BikeModel>> newModelDict = new Dictionary<string, List<BikeModel>> { 
                    [newModel.Make] = new List<BikeModel> { newModel }
                };

                populateModelStore(newModelDict);

            } catch (FormatException) {
                int bikeError = editModelWin.errorType;

                if (bikeError == ModelEditor.ERROR_EMPTY) {
                    sendError("Please fill in all mandatory fields", modelsWindow);
                }
            }
              
                editModelWin.Destroy();
        }
        
        private static void sendError(string errorMsg, Window mainWindow) {
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
                    int oldBikeCount = Bike.BikeCount;
                    
                    Dictionary<string, Dictionary<string, List<Bike>>> newImportedInv = parseInventoryFile(File.ReadAllLines(importFilename));
                    populateBikeStore(newImportedInv);
                    int elementsAdded = Bike.BikeCount - oldBikeCount;

                    Console.WriteLine(String.Format("Imported {0} bikes from file {1}", elementsAdded, importFilename));
                } else {
                    sendError("File does not exist", mainWindow);
                }
            }

            importFileDialog.Destroy();    
        }
        
        private static void importModelsFile(object sender, EventArgs args, ModelsViewer editorWindow) {
            FileChooserDialog importFileDialog = new FileChooserDialog("Import an existing inventory file", 
                editorWindow, FileChooserAction.Open, 
                "Cancel",ResponseType.Cancel, "Open",ResponseType.Accept);
            
            if (importFileDialog.Run() == (int) ResponseType.Accept) {
                string importFilename = importFileDialog.Filename;
                
                if (File.Exists(importFilename)) {
                    Dictionary<string, List<BikeModel>> importedModels = new Dictionary<string, List<BikeModel>>();
                    int oldModelCount = BikeModel.ModelCount;

                    try {
                        importedModels = parseModelFile(File.ReadAllLines(importFilename));
                        populateModelStore(importedModels);

                        int elementsAdded = BikeModel.ModelCount - oldModelCount;
                        Console.WriteLine(String.Format("Imported {0} models from file {1}", elementsAdded, importFilename));

                    } catch (FormatException) {
                        sendError("Models were not complete, none imported", editorWindow);
                    }
                } else {
                    sendError("File does not exist", editorWindow);
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

        private static void editModels(object sender, EventArgs e) {
            ModelsViewer modelsWin;
            modelsWin = new ModelsViewer();
            modelsWin.modelTree.Model = modelStore;

            modelsWin.removeModelButton.Clicked += (removeSender, removeE) => removeModelButton(removeSender, removeE, modelsWin);
            modelsWin.addModelButton.Clicked += (addSender, addE) => addModelButton(addSender, addE, modelsWin);
            modelsWin.editModelButton.Clicked += (editSender, editE) => editModelButton(editSender,editE, modelsWin);

            modelsWin.modelTree.KeyPressEvent += (o, keyArgs) => suprModel(o, keyArgs, modelsWin);
            modelsWin.modelTree.RowActivated += (o, rowArgs) => modelActivate(o, rowArgs, modelsWin);

            modelsWin.importItem.Activated += (importSender, importE) => importModelsFile(importSender, importE, modelsWin);
            updateModelCount(modelsWin);
            
            modelStore.RowInserted += delegate {
                updateModelCount(modelsWin);
            };
            
            modelStore.RowDeleted += delegate {
                updateModelCount(modelsWin);
            };

            modelsWin.ShowAll();
            modelsWin.Show();
        }

        private static void populateModelStore(Dictionary<String, List<BikeModel>> modelDict) {
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
                }
            }
        }

        private static void populateBikeStore(Dictionary<string, Dictionary<string, List<Bike>>> bikeDict ) {
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
                    TreeIter makeParent = bikeStore.AppendValues(MAKE_DEPTH, make);
                    bikeMap[make] = new Dictionary<string, TreeIter>{
                        ["_ownIter"] = makeParent
                    };
                }

                TreeIter makeIter = bikeMap[make]["_ownIter"];

                foreach (string model in bikeDict[make].Keys) {
                    if (!bikeMap[make].ContainsKey(model)) {
                        TreeIter modelParent = bikeStore.AppendValues(makeIter, MODEL_DEPTH, make, model);
                        bikeMap[make][model] = modelParent;                    
                    }

                    TreeIter modelIter = bikeMap[make][model];

                    foreach (Bike bike in bikeDict[make][model]) {
                        if (!securityMap.ContainsKey(bike.SecurityCode)) {
                            TreeIter bikeChild = bikeStore.AppendValues(modelIter, BIKE_DEPTH, bike.Make,
                                bike.Model, bike.Year, bike.Type, bike.WheelSize, bike.Forks, bike.SecurityCode);

                            TreeIter expChild = bikeExpStore.AppendValues(BIKE_DEPTH, bike.Make,
                                bike.Model, bike.Year, bike.Type, bike.WheelSize, bike.Forks, bike.SecurityCode);

                            securityMap[bike.SecurityCode] = new Dictionary<string, TreeIter> {
                                [groupedKey] = bikeChild,
                                [expandKey] = expChild
                            };
                        } else {
                            Console.WriteLine(string.Format("The bike with make {0}, model {1}, & code {2} could not be added because the code already exists", 
                                bike.Make, bike.Model, bike.SecurityCode));
                            Bike.removeBike();
                        }
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
            string parentModel = (string) bikeStore.GetValue(modelParent, STORE_MODEL_P);

            // Recover grandparent before deletion
            TreeIter makeParent;
            bikeStore.IterParent(out makeParent, modelParent);
            string parentMake = (string) bikeStore.GetValue(makeParent, STORE_MAKE_P);

            // Decrease count now so that deleted event can update indicator accordingly
            Bike.removeBike();
            
            // Remove from both Stores using TreeIter reference recovered earlier
            bikeStore.Remove(ref groupedElement);
            bikeExpStore.Remove(ref expElement);
            
            // Remove saved reference
            securityMap.Remove(secCode);
            
            // If last element of parent, delete parent too
            if (bikeStore.IterNChildren(modelParent) == 0) {
                bikeStore.Remove(ref modelParent);
                bikeMap[parentMake].Remove(parentModel);
                
                if (bikeStore.IterNChildren(makeParent) == 0) {
                    bikeStore.Remove(ref makeParent);
                    bikeMap.Remove(parentMake);
                }
            }
        }

        private static void deleteModel(TreeIter modelIter) {
            // Console feedback
            Console.WriteLine(string.Format("About to delete {0}", modelStore.GetValue(modelIter, STORE_MODEL_P)));

            TreeIter makeParent;
            modelStore.IterParent(out makeParent, modelIter);
            string parentMake = (string)modelStore.GetValue(makeParent, MODEL_MAKE_P);

            // Decrease count now so that deleted event can update indicator accordingly
            BikeModel.removeBike();

            // Remove from both Stores using TreeIter reference recovered earlier
            modelStore.Remove(ref modelIter);

            // If last element of parent, delete parent too
            if (modelStore.IterNChildren(makeParent) == 0) {
                modelStore.Remove(ref makeParent);
                modelMap.Remove(parentMake);
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
        }
        
        private static Dictionary<string, Dictionary<string, List<Bike>>> parseInventoryFile(String[] invLines) {
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
                    throw new FormatException();
                }
            }

            return bikeInventory;
        }
        
        private static Dictionary<String, List<BikeModel>> parseModelFile(string[] modelFile) {
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
                    throw new FormatException();
                }
            }

            return modelDict;
        } 
    }
}
