using Builder.Background;
using Builder.Dialog;
using Builder.Scene;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic; 
using System.IO;
using System.IO.Compression;     
using System.Linq;                
using System.Text.Json;        
using System.Threading.Tasks.Dataflow;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.LinkLabel;

namespace Builder
{
    public partial class winBuilder : Form
    {
        // ── Main vars ───────────────────────────────────────────────────
        private string _fileName = "";
        private bool _isSaved = true;
        private Block _b;
        private Dictionary<string, BlockControl> _blockControls = new Dictionary<string, BlockControl>();

        public winBuilder()
        {
            InitializeComponent();
            view1.NodeDoubleClicked += View1_NodeDoubleClicked;
            view1.Saved += view_isSaved;

        }

        private void winBuilder_Load(object sender, EventArgs e)
        {
            _b = new Block();
        }

        private void SyncDataBeforeSave()
        {
            foreach (var node in view1.SceneNodes)
            {
                SceneData scene = new SceneData();

                if (_blockControls.ContainsKey(node.id))
                {
                    var ctrl = _blockControls[node.id];
                    scene.pathBackgorund = ctrl.GetPathBackground();
                    var lines = ctrl.GetLines();
                    scene.lines = (lines != null && lines.Length > 0) ? lines : null;
                    scene.hasChoice = ctrl.GetHasChoice();
                    scene.transiton = ctrl.GetTransition();
                    scene.choices = ctrl.GetChoices();
                }
                var outgoingLinks = view1.SceneLinks.Where(l => l.From == node.id).ToList();

                if (scene.hasChoice && scene.choices != null)
                {
                    for (int i = 0; i < scene.choices.Length; i++)
                    {
                        var link = outgoingLinks.FirstOrDefault(l => l.OutputSlot == i);
                        scene.choices[i].nextScene = link?.To;
                    }
                    scene.nextScene = null;
                }
                else
                {
                    var link = outgoingLinks.FirstOrDefault(l => l.OutputSlot == 0)
                               ?? outgoingLinks.FirstOrDefault();
                    scene.nextScene = link?.To;
                }

                _b.AddBlock(node.id, scene);
            }
        }

        private void View1_NodeDoubleClicked(object sender, string nodeId)
        {
            if (!_blockControls.ContainsKey(nodeId))
            {
                BlockControl newBlock = new BlockControl(nodeId);
                newBlock.Dock = DockStyle.Fill;
                newBlock.Hide();

                this.Controls.Add(newBlock);

                _blockControls.Add(nodeId, newBlock);
            }

            foreach (var block in _blockControls.Values)
            {
                block.Hide();
            }

            var activeBlock = _blockControls[nodeId];
            activeBlock.onBack += BlockControl_OnBack;

            activeBlock.Show();
            activeBlock.BringToFront();
            _isSaved = false;
            updateUI();
        }

        private void view_isSaved(object sender, bool val)
        {
            _isSaved = val;
            updateUI();
        }

        private void BlockControl_OnBack(object sender, string blockId)
        {
            view1.SetNodeBackground(blockId, _blockControls[blockId].GetPathBackground());
            view1.SetNodeOutputs(blockId, _blockControls[blockId].GetNrOutputs());
        }

        private void debugToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_fileName))
            {
                MessageBox.Show("Cannot open file", "Încărcare poveste");
                return;
            }

            if (!_isSaved)
            {
                MessageBox.Show("File isn't saved. Există posibilitatea să se piardă din conținut.", "Atenție");  
            }
            if(_b.startID == null)
            {
                MessageBox.Show("Esti prost! Ai uitat sa pui de unde incepe povestea!!!", "Atentie");
                return;
            }
            
            Form2 f = new Form2();
            f.setPath(_fileName);
            f.Show();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ImportProjectFromZip("C:\\Users\\mnxnc\\Downloads\\estibomba.zip");
            if (!_isSaved)
            {
                DialogResult dialogRes = MessageBox.Show("Save changes?",
                    "Save changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dialogRes == DialogResult.Yes)
                {
                    smartSave();
                }
                if (dialogRes == DialogResult.Cancel)
                {
                    return;
                }
            }

            try
            {

                DialogResult dialogResult = openFile.ShowDialog();

                if (dialogResult == DialogResult.OK)
                {

                    _fileName = openFile.FileName;
                    _blockControls = new Dictionary<string, BlockControl>();
                    _b.Clear();
                    _b = new Block();
                    view1.ClearView();
                    ImportProjectFromZip(_fileName);

                    _isSaved = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot open file{ex.Message}", $"Error opening file", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                updateUI();
            }

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SyncDataBeforeSave();
            //_b.SaveToArchive("C:\\Users\\mnxnc\\Downloads\\estibomba.zip");
            //ExportToZip("C:\\Users\\mnxnc\\Downloads\\estibomba.zip");
            if (_fileName.Length == 0)
            {
                // save as
                saveAs();
                return;
            }

            // save normal
            save();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAs();
        }

        private void updateUI()
        {
            string _winName = "Builder";
            if (!_isSaved)
            {
                _winName += "*";
            }
            if (_fileName.Length > 0)
            {
                _winName += " :: " + _fileName;
            }
            this.Text = _winName;
        }

        private void save()
        {
            try
            {
                ExportToZip(_fileName);
                _isSaved = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot save file", $"Error saveing file{ex.Message}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                updateUI();
            }
        }

        private void saveAs()
        {
            DialogResult dialogResult = saveFile.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
            
                _fileName = saveFile.FileName;
                save();
            }
        }

        private void smartSave()
        {
            if (_fileName.Length > 0)
            {
                save();
                return;
            }

            saveAs();
        }

        public void ExportToZip(string archivePath)
        {
            string tempGraphPath = Path.Combine(Path.GetTempPath(), "vnb_graph_temp_" + Guid.NewGuid() + ".json");
            try
            {
                view1.SaveSceneToFile(tempGraphPath);
                using (FileStream zipToOpen = new FileStream(archivePath, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                    {
                        var exportData = new Dictionary<string, SceneData>();

                        // NOU: Folosim un HashSet ca să ținem minte ce imagini am băgat deja în arhivă
                        var addedImages = new HashSet<string>();

                        foreach (var kvp in _b.dataR)
                        {
                            var scene = kvp.Value;
                            string originalPath = scene.pathBackgorund;
                            string relativePath = "";

                            // Dacă avem o cale validă, copiem fișierul în arhivă
                            if (!string.IsNullOrEmpty(originalPath) && File.Exists(originalPath))
                            {
                                string fileName = Path.GetFileName(originalPath);
                                relativePath = "images/" + fileName;

                                // Verificăm în lista noastră dacă am adăugat deja imaginea (rezolvă eroarea)
                                if (!addedImages.Contains(relativePath))
                                {
                                    archive.CreateEntryFromFile(originalPath, relativePath);
                                    addedImages.Add(relativePath); // O marcăm ca adăugată ca să nu o mai punem a doua oară
                                }
                            }

                            // Adăugăm scena în datele de export cu calea relativă modificată
                            exportData.Add(kvp.Key, new SceneData
                            {
                                pathBackgorund = relativePath, // Salvăm "images/fisier.gif" în loc de "C:\..."
                                lines = scene.lines,
                                hasChoice = scene.hasChoice,
                                choices = scene.choices,
                                nextScene = scene.nextScene,
                                transiton = scene.transiton
                            });
                        }

                        _b.startID = view1.GetStartNodeId();
                        // Salvăm și JSON-ul direct în arhivă
                        ZipArchiveEntry jsonEntry = archive.CreateEntry("scene.json");
                        using (StreamWriter writer = new StreamWriter(jsonEntry.Open()))
                        {
                            string json = JsonSerializer.Serialize(new { startID = _b.startID, data = exportData }, new JsonSerializerOptions
                            {
                                WriteIndented = true
                            });
                            writer.Write(json);
                        }
                        if (File.Exists(tempGraphPath))
                        {
                            archive.CreateEntryFromFile(tempGraphPath, "graph.json");
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (File.Exists(tempGraphPath)) { try { File.Delete(tempGraphPath); } catch { } }
            }
        }

        private void ImportProjectFromZip(string zipFilePath)
        {
            try
            {
                string tempFolder = Path.Combine(Path.GetTempPath(), "VNBuilderTemp_" + Guid.NewGuid());
                Directory.CreateDirectory(tempFolder);

                ZipFile.ExtractToDirectory(zipFilePath, tempFolder, overwriteFiles: true);

                string graphPath = Path.Combine(tempFolder, "graph.json");
                if (File.Exists(graphPath))
                {
                    view1.LoadSceneFromFile(graphPath);
                }

                string jsonPath = Path.Combine(tempFolder, "scene.json");
                if (File.Exists(jsonPath))
                {
                    _b.Load(jsonPath);


                    foreach (var kvp in _b.dataR)
                    {
                        string nodeId = kvp.Key;
                        SceneData scene = kvp.Value;

                        if (!string.IsNullOrEmpty(scene.pathBackgorund))
                        {
                            scene.pathBackgorund = Path.Combine(tempFolder, scene.pathBackgorund);
                            view1.SetNodeBackground(nodeId, scene.pathBackgorund);
                        }

                        BlockControl newBlock = new BlockControl(nodeId) { Dock = DockStyle.Fill };
                        newBlock.Hide();
                        newBlock.SetPathBackground(scene.pathBackgorund);
                        newBlock.SetLines(scene.lines);
                        newBlock.SetHasChoice(scene.hasChoice);
                        newBlock.SetChoices(scene.choices);
                        newBlock.SetTransition(scene.transiton);

                        this.Controls.Add(newBlock);
                        _blockControls.Add(nodeId, newBlock);
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Eroare la import: {ex.Message}");
                throw new Exception(ex.Message, ex);
            }
        }

        private void openFile_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_isSaved)
            {
                DialogResult dialogRes = MessageBox.Show("Save changes?",
                    "Save changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dialogRes == DialogResult.Yes)
                {
                    smartSave();
                }
                if (dialogRes == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}