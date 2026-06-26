using Builder.Background;
using Builder.Dialog;
using Builder.Scene;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using System.Text;
using System.Text.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Builder
{
    internal class Block
    {
        private Control target;
        private DialogSystem dialog;
        private Dictionary<string, SceneData> data = new Dictionary<string, SceneData>();
        private SceneData currentScene;
        private GifAnimator gif;
        private Font fontName;
        private Font fontText;
        private TranslucentPanel choicePanel;
        private Vector2 size = Vector2.Zero;
        private int fadeAlpha = 0;      // 0 = transparent, 255 = negru total
        private int fadeSpeed = 50;     // Viteză (mai mare = mai rapid)
        private bool isFadingOut = false; // Dacă suntem în faza de înnegrire
        private string nextScenePending = null;

        public string startID { get; set; }
        public IReadOnlyDictionary<string, SceneData> dataR => data.AsReadOnly();

        public Block()
        {
            fontName = new Font("Verdana", 12, FontStyle.Bold);
            fontText = new Font("Verdana", 10, FontStyle.Bold);
        }
        public Block(Control target, System.Windows.Forms.Timer tmClock, Vector2 size)
        {
            
            this.target = target;
            this.size = size;
            fontName = new Font("Verdana", 12, FontStyle.Bold);
            fontText = new Font("Verdana", 10, FontStyle.Bold); 
        }

        public void Start(string pointKey)
        {
            GoToScene(pointKey);
        }

        public void Update(object sender, EventArgs e)
        {
            if (isFadingOut)
            {
                fadeAlpha += fadeSpeed;
                if (fadeAlpha >= 255)
                {
                    fadeAlpha = 255;
                    LoadScene(nextScenePending); // Aici schimbăm scena pe negru
                    nextScenePending = null;
                    isFadingOut = false; // Trecem la Fade In
                }
            }
            // 2. FADE IN (revenire)
            else if (fadeAlpha > 0)
            {
                fadeAlpha -= fadeSpeed;
                if (fadeAlpha < 0) fadeAlpha = 0;
            }
        }

        public bool HasScene(string key)
        {
            return data.ContainsKey(key);
        }

        public void AddBlock(string key, SceneData sceneData)
        {
            if (data.ContainsKey(key))
            {
                // Dacă există deja, actualizăm datele
                data[key] = sceneData;
            }
            else
            {
                // Dacă nu există, adăugăm un element nou
                data.Add(key, sceneData);
            }
        }

        private void LoadScene(string sceneName)
        {
            //MessageBox.Show($"font: {fontName?.Name} bold:{fontName?.Bold} | {fontText?.Name} bold:{fontText?.Bold}");
            var scene = data[sceneName];
            currentScene = scene;

            gif = new GifAnimator(scene.pathBackgorund);

            gif.OnFrameUpdate = () =>
            {
                target.Invalidate();
            };

            dialog = new DialogSystem(fontName, fontText);

            if (scene.lines != null)
            {
                dialog.Add(scene.lines);
                dialog.Start();
            }


            target.Invalidate();
        }

        private void GoToScene(string sceneName)
        {
            // Verificăm dacă scena există
            if (!data.ContainsKey(sceneName)) return;

            var nextScene = data[sceneName];

            // VERIFICARE: Dacă scena NU are nevoie de tranziție, încărcăm instant
            if (!nextScene.transiton)
            {
                LoadScene(sceneName);
            }
            else
            {
                // Dacă are nevoie de tranziție, pornim procesul de Fade
                if (fadeAlpha > 0) return; // Prevenim dubla pornire
                nextScenePending = sceneName;
                isFadingOut = true;
            }
        }

        private void ShowChoices(SceneData scene)
        {

            target.Invalidate();
            if (choicePanel != null)
            {
                target.Controls.Remove(choicePanel);
                choicePanel.Dispose();
            }

            choicePanel = new TranslucentPanel();
            choicePanel.Size = new Size((int)size.X, (int)size.Y);
            choicePanel.Location = new Point(0, 0);
            choicePanel.BackColor = Color.FromArgb(180, 0, 0, 0);

            int startY = 200;
            int index = 0;

            foreach (var choice in scene.choices)
            {
                System.Windows.Forms.Button btn = new System.Windows.Forms.Button();
                btn.BackColor = Color.White;
                btn.Text = choice.text;
                btn.Size = new Size(320, 60);
                btn.Location = new Point(
                    ((int)size.X - 320) / 2,
                    startY + index * 80
                );

                // IMPORTANT: capture corect în lambda
                var nextScene = choice.nextScene;

                btn.Click += (s, e) =>
                {
                    choicePanel.Dispose();
                    choicePanel = null;
                    GoToScene(nextScene);
                    //LoadScene(nextScene);
                };



                choicePanel.Controls.Add(btn);

                index++;
            }

            target.Controls.Add(choicePanel);
            choicePanel.BringToFront();
        }

        public void Resize(Vector2 size)
        {
            this.size = size;
            if (choicePanel != null)
            {
                choicePanel.Size = new Size((int)size.X, (int)size.Y);
                int startY = 200;
                int index = 0;
                foreach (Control c in choicePanel.Controls)
                {
                    if (c is System.Windows.Forms.Button btn)
                    {
                        btn.Location = new Point(((int)size.X - 320) / 2, startY + index * 80);
                        index++;
                    }
                }
            }
        }

        public void Click(object sender, EventArgs e)
        {
            dialog.Next();
            if (!dialog.IsActive)
            {
                if (dialog != null && !dialog.IsActive && currentScene != null && currentScene.hasChoice)
                {
                    ShowChoices(currentScene);
                }
                else
                {
                    if (currentScene.nextScene != null)
                    {
                        GoToScene(currentScene.nextScene);
                    }

                }
            }
        }

        public void Paint(Graphics graphics)
        {
            // Fundal negru de bază
            graphics.FillRectangle(Brushes.Black, 0, 0, size.X, size.Y);

            // ADĂUGĂM VERIFICAREA AICI:
            // Desenăm gif-ul doar dacă a fost inițializat (dacă avem o scenă încărcată)
            if (gif != null)
            {
                gif.Draw(graphics, new RectangleF(0, 0, size.X, size.Y));
            }

            // Desenăm dialogul doar dacă avem dialog activ sau scenă încărcată
            if (dialog != null)
            {
                var r = new RectangleF(15, size.Y - size.Y / 4F, size.X - 30, 150);
                dialog.Draw(graphics, r);
            }

            // Tranziția se poate desena mereu
            if (fadeAlpha > 0)
            {
                using (Brush b = new SolidBrush(Color.FromArgb(fadeAlpha, 0, 0, 0)))
                {
                    graphics.FillRectangle(b, 0, 0, size.X, size.Y);
                }
            }
        }

        public void Save(string filePath)
        {
            var json = JsonSerializer.Serialize(new { startID = this.startID, data = this.data }, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(filePath, json);
        }

        public void Load(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            string json = File.ReadAllText(filePath);

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                // Extragem startID din nodul principal
                if (doc.RootElement.TryGetProperty("startID", out JsonElement startIdElem))
                    this.startID = startIdElem.GetString();

                // Extragem dicționarul de scene din nodul "data"
                if (doc.RootElement.TryGetProperty("data", out JsonElement dataElem))
                    this.data = JsonSerializer.Deserialize<Dictionary<string, SceneData>>(dataElem.GetRawText()) ?? new Dictionary<string, SceneData>();
            }
        }

        public void SaveToArchive(string archivePath)
        {
            // Creăm un fișier ZIP
            using (FileStream zipToOpen = new FileStream(archivePath, FileMode.Create))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                {
                    var exportData = new Dictionary<string, SceneData>();

                    // NOU: Folosim un HashSet ca să ținem minte ce imagini am băgat deja în arhivă
                    var addedImages = new HashSet<string>();

                    foreach (var kvp in data)
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

                    // Salvăm și JSON-ul direct în arhivă
                    ZipArchiveEntry jsonEntry = archive.CreateEntry("scene.json");
                    using (StreamWriter writer = new StreamWriter(jsonEntry.Open()))
                    {
                        string json = JsonSerializer.Serialize(new { startID = this.startID, data = exportData }, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });
                        writer.Write(json);
                    }
                }
            }
        }

        public void LoadFromArchive(string archivePath)
        {
            if (!File.Exists(archivePath))
                return;

            // Generăm un folder temporar unic unde vom extrage arhiva
            string tempFolder = Path.Combine(Path.GetTempPath(), "VNBuilderTemp_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);

            // Extragem tot conținutul arhivei (JSON + imagini) în folderul temporar
            ZipFile.ExtractToDirectory(archivePath, tempFolder, overwriteFiles: true);

            // Citim JSON-ul extras
            string jsonPath = Path.Combine(tempFolder, "scene.json");
            if (File.Exists(jsonPath))
            {
                string json = File.ReadAllText(jsonPath);
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    if (doc.RootElement.TryGetProperty("startID", out JsonElement startIdElem))
                        this.startID = startIdElem.GetString();

                    if (doc.RootElement.TryGetProperty("data", out JsonElement dataElem))
                        this.data = JsonSerializer.Deserialize<Dictionary<string, SceneData>>(dataElem.GetRawText()) ?? new Dictionary<string, SceneData>();
                }

                // Refacem căile din dicționar ca să arate spre noile fișiere din %TEMP%
                foreach (var scene in data.Values)
                {
                    if (!string.IsNullOrEmpty(scene.pathBackgorund))
                    {
                        // Transformăm "images/golf.gif" în "C:\Users\...\AppData\Local\Temp\VNBuilderTemp_...\images\golf.gif"
                        scene.pathBackgorund = Path.Combine(tempFolder, scene.pathBackgorund);
                    }
                }
            }
        }

        public void Clear()
        {
            // 1. Curățare UI: Eliminăm panoul de alegeri dacă există
            if (choicePanel != null)
            {
                if (target != null && target.Controls.Contains(choicePanel))
                {
                    target.Controls.Remove(choicePanel);
                }
                choicePanel.Dispose();
                choicePanel = null;
            }

            // 2. Curățare resurse: Oprim gif-ul pentru a elibera memoria/fișierul
            // Presupunând că GifAnimator nu are un metodă Dispose, îl setăm pe null
            gif = null;

            // 3. Resetare logică și date
            dialog = null;
            currentScene = null;
            data.Clear(); // Ștergem toate datele din dicționar
            startID = null;

            // 4. Resetare variabile de stare
            fadeAlpha = 0;
            isFadingOut = false;
            nextScenePending = null;

            // 5. Redesenare (opțional, pentru a curăța ecranul imediat)
            if (target != null)
            {
                target.Invalidate();
            }
        }

    }
}
