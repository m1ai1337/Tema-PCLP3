using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json;
using System.Windows.Forms;
using static Builder.View;

namespace Builder
{
    public partial class View : UserControl
    {
        // ── Pan / zoom ────────────────────────────────────────────────
        private PointF _pan = new(0, 0);
        private Point _lastMousePos;
        private bool _panning = false;
        private float _scale = 1.0f;
        
        // ── Tracking interacțiuni ─────────────────────────────────────
        private _sceneNode _selectedNode = null;
        private _sceneLink _selectedLink = null;

        private _sceneNode _draggedNode = null;
        private _sceneNode _linkingNode = null;
        private int _linkingSlot = -1; // Slotul de ieșire detectat la Shift+Click
        private PointF _tempLinkPoint;

        // ── Events ────────────────────────────────────────────────────
        public event EventHandler<string> NodeDoubleClicked;
        public event EventHandler<bool> Saved;
        // ── Scene data ────────────────────────────────────────────────
        public class _sceneNode
        {
            public string id;
            public RectangleF Rect;
            public int maxOutputs = 1; // default: 1 ieșire
            public int maxInputs = 1;  // default: 1 intrare (pentru snap simetric)
            public bool isStart = false;
            public string backgroundImagePath;
            public Image backgroundImage = null;
        }

        public class _sceneLink
        {
            public string From;
            public string To;
            public int OutputSlot; // 0 = ieșirea 1, 1 = ieșirea 2
            public int InputSlot;  // Slotul de intrare pe nodul destinație
        }

        // Culori per slot de ieșire
        private static readonly Color[] SlotColors = { Color.Gray, Color.IndianRed };

        List<_sceneNode> nodes = new();
        List<_sceneLink> links = new();

        public IReadOnlyList<_sceneNode> SceneNodes => nodes.AsReadOnly();
        public IReadOnlyList<_sceneLink> SceneLinks => links.AsReadOnly();

        // ─────────────────────────────────────────────────────────────
        public View()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.Selectable |
                     ControlStyles.UserPaint, true);
            Cursor = Cursors.Default;
        }

        private void View_Load(object sender, EventArgs e)
        {
        }

        // ── Coordinate helpers & Pathing ──────────────────────────────
        PointF ScreenToWorld(Point p) => new(
            (p.X - _pan.X) / _scale,
            (p.Y - _pan.Y) / _scale);

        // NOU: Helper pentru generarea liniilor cu unghiuri de 90 de grade
        PointF[] GetOrthogonalPath(PointF p1, PointF p2)
        {
            // Punctul de la jumătatea distanței pe Y. 
            // Math.Max previne liniile care taie direct prin nodul de sus dacă nodul destinație e mai sus.
            float midY = p1.Y + Math.Max(20, (p2.Y - p1.Y) / 2f);

            return new PointF[]
            {
                p1,
                new PointF(p1.X, midY),
                new PointF(p2.X, midY),
                p2
            };
        }

        _sceneNode HitTestNode(Point screenPt)
        {
            PointF w = ScreenToWorld(screenPt);
            for (int i = nodes.Count - 1; i >= 0; i--)
                if (nodes[i].Rect.Contains(w)) return nodes[i];
            return null;
        }

        _sceneLink HitTestLink(Point screenPt)
        {
            PointF w = ScreenToWorld(screenPt);
            using var path = new GraphicsPath();

            foreach (var lnk in links)
            {
                var src = nodes.Find(n => n.id == lnk.From);
                var dst = nodes.Find(n => n.id == lnk.To);
                if (src == null || dst == null) continue;

                path.Reset();

                PointF p1 = new(src.Rect.X + src.Rect.Width * (lnk.OutputSlot + 1f) / (src.maxOutputs + 1f), src.Rect.Bottom);
                PointF p2 = new(dst.Rect.X + dst.Rect.Width * (lnk.InputSlot + 1f) / (dst.maxInputs + 1f), dst.Rect.Top);

                // NOU: Înlocuim Bezier cu linie frântă de 90 de grade
                path.AddLines(GetOrthogonalPath(p1, p2));

                using var widenPen = new Pen(Color.Black, 10f);
                if (path.IsOutlineVisible(w, widenPen))
                    return lnk;
            }
            return null;
        }

        // ── Drawing ───────────────────────────────────────────────────
        private void OnPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            DrawGrid(g);

            g.TranslateTransform(_pan.X, _pan.Y);
            g.ScaleTransform(_scale, _scale);

            DrawLinks(g);

            if (_linkingNode != null)
            {
                PointF startPt = new(_linkingNode.Rect.X + _linkingNode.Rect.Width * (_linkingSlot + 1f) / (_linkingNode.maxOutputs + 1f), _linkingNode.Rect.Bottom);
                using Pen tempPen = new Pen(SystemColors.Highlight, 2f) { DashStyle = DashStyle.Dash };

                // NOU: Tragem un path în 90 de grade și pentru legătura temporară în curs de creare
                g.DrawLines(tempPen, GetOrthogonalPath(startPt, _tempLinkPoint));
            }

            DrawNodes(g);

            g.ResetTransform();
        }

        private GraphicsPath GetRoundedRectanglePath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float diameter = radius * 2f;

            // Colțul stânga-sus
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            // Colțul dreapta-sus
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            // Colțul dreapta-jos
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            // Colțul stânga-jos
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);

            path.CloseFigure();
            return path;
        }

        void DrawNodes(Graphics g)
        {
            foreach (var n in nodes)
            {
                bool isSelected = (n == _selectedNode);

                // Observație: în codul tău ambele variante erau White, am simplificat aici.
                Brush bgBrush = Brushes.White;

                // Generăm calea cu colțuri rotunjite (raza de 3 pixeli)
                using (GraphicsPath path = GetRoundedRectanglePath(n.Rect, 3f))
                {
                    // 1. Umplem fundalul nodului
                    g.FillPath(bgBrush, path);

                    if (n.backgroundImage != null)
                    {
                        g.SetClip(path);
                        g.DrawImage(n.backgroundImage, n.Rect);
                        g.ResetClip();
                    }

                    // 2. Desenăm conturul (Border)
                    if (isSelected)
                    {
                        // Dacă e selectat, creăm un Pen albastru de sistem (cuusing pentru eliberarea memoriei)
                        // Am pus grosimea 2f pentru ca selecția să iasă mai bine în evidență.
                        using (Pen highlightPen = new Pen(SystemColors.Highlight, 1f))
                        {
                            g.DrawPath(highlightPen, path);
                        }
                    }
                    else
                    {
                        // Dacă nu e selectat, desenăm cu gri
                        g.DrawPath(Pens.Gray, path);
                    }
                }

                // 3. Desenăm textul (ID-ul)
                
               
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                SizeF textSize = g.MeasureString(n.isStart ? n.id + "*" : n.id, Font);
                RectangleF textBg = new RectangleF(
                    n.Rect.X + (n.Rect.Width - textSize.Width) / 2 - 4,
                    n.Rect.Y + (n.Rect.Height - textSize.Height) / 2 - 2,
                    textSize.Width + 8,
                    textSize.Height + 4
                );

                if(n.backgroundImage != null)
                {
                    using (GraphicsPath glasspath = GetRoundedRectanglePath(textBg, 3f))
                    {
                        // Strat 1: umbra subtila
                        using var shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0));
                        RectangleF shadowRect = new RectangleF(textBg.X + 1, textBg.Y + 2, textBg.Width, textBg.Height);
                        using var shadowPath = GetRoundedRectanglePath(shadowRect, 3f);
                        g.FillPath(shadowBrush, shadowPath);

                        // Strat 2: fundal alb semi-transparent (efectul de glass)
                        using var glassBrush = new SolidBrush(Color.FromArgb(120, 255, 255, 255));
                        g.FillPath(glassBrush, glasspath);

                        // Strat 3: highlight alb sus (iluzia de reflexie)
                        RectangleF highlightRect = new RectangleF(textBg.X + 2, textBg.Y + 1, textBg.Width - 4, textBg.Height / 2);
                        using var highlightPath = GetRoundedRectanglePath(highlightRect, 2f);
                        using var highlightBrush = new SolidBrush(Color.FromArgb(60, 255, 255, 255));
                        g.FillPath(highlightBrush, highlightPath);

                        // Strat 4: border subtil
                        using var borderPen = new Pen(Color.FromArgb(80, 255, 255, 255), 1f);
                        g.DrawPath(borderPen, glasspath);
                    }
                }
                  

                
                g.DrawString(n.isStart ? n.id + "*" : n.id, Font, Brushes.Black, n.Rect, sf);

                // 4. Desenăm sloturile
                DrawOutputSlotIndicators(g, n);
                DrawInputSlotIndicators(g, n);
            }
        }

        public void SetNodeBackground(string nodeId, string imagePath)
        {

            var node = nodes.Find(n => n.id == nodeId);
            if (node == null) return;

            if (string.IsNullOrWhiteSpace(imagePath))
                return;

            if (!File.Exists(imagePath))
                return;

            node.backgroundImagePath = imagePath; // Salvează calea aici!
            node.backgroundImage = Image.FromFile(imagePath);
            Invalidate();
        }

        public void SetNodeOutputs(string id, int nr)
        {
            var node = nodes.Find(n => n.id == id);
            if (node == null) return;

            node.maxOutputs = nr;

            // Ștergem automat conexiunile de pe sloturile de ieșire care nu mai există
            links.RemoveAll(l => l.From == id && l.OutputSlot >= nr);

            Invalidate(); // Redesenăm scena
        }

        void DrawOutputSlotIndicators(Graphics g, _sceneNode n)
        {
           
            for (int slot = 0; slot < n.maxOutputs; slot++)
            {
                float slotX = n.Rect.X + n.Rect.Width * (slot + 1f) / (n.maxOutputs + 1f);
                float slotY = n.Rect.Bottom - 3.0f;
                Color c = slot < SlotColors.Length ? SlotColors[slot] : Color.Purple;

                using var brush = new SolidBrush(c);
                //g.FillEllipse(brush, slotX - 5f, slotY - 5f, 10f, 10f);
                //g.DrawEllipse(Pens.White, slotX - 5f, slotY - 5f, 10f, 10f);
                SizeF textSize = g.MeasureString((slot + 1).ToString(), Font);
                RectangleF textBg = new RectangleF(
                    slotX - textSize.Width/2,
                    slotY - textSize.Height,
                    textSize.Width,
                    textSize.Height
                );


                if (n.backgroundImage != null)
                {
                    using (GraphicsPath glasspath = GetRoundedRectanglePath(textBg, 3f))
                    {
                        // Strat 1: umbra subtila
                        using var shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0));
                        RectangleF shadowRect = new RectangleF(textBg.X + 1, textBg.Y + 2, textBg.Width, textBg.Height);
                        using var shadowPath = GetRoundedRectanglePath(shadowRect, 3f);
                        g.FillPath(shadowBrush, shadowPath);

                        // Strat 2: fundal alb semi-transparent (efectul de glass)
                        using var glassBrush = new SolidBrush(Color.FromArgb(120, 255, 255, 255));
                        g.FillPath(glassBrush, glasspath);

                        // Strat 3: highlight alb sus (iluzia de reflexie)
                        RectangleF highlightRect = new RectangleF(textBg.X + 2, textBg.Y + 1, textBg.Width - 4, textBg.Height / 2);
                        using var highlightPath = GetRoundedRectanglePath(highlightRect, 2f);
                        using var highlightBrush = new SolidBrush(Color.FromArgb(60, 255, 255, 255));
                        g.FillPath(highlightBrush, highlightPath);

                        // Strat 4: border subtil
                        using var borderPen = new Pen(Color.FromArgb(80, 255, 255, 255), 1f);
                        g.DrawPath(borderPen, glasspath);
                    }
                }

                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
                g.DrawString((slot + 1).ToString(), new Font(Font.FontFamily, 10f), brush,
                    new PointF(slotX, slotY - textSize.Height), sf);
            }
        }

        void DrawInputSlotIndicators(Graphics g, _sceneNode n)
        {
          
            if(n.maxInputs == 1)
            {
                return;
            }
            for (int slot = 0; slot < n.maxInputs; slot++)
            {
                float slotX = n.Rect.X + n.Rect.Width * (slot + 1f) / (n.maxInputs + 1f);
                float slotY = n.Rect.Top + 1.0f;
                Color c = slot < SlotColors.Length ? SlotColors[slot] : Color.Purple;

                using var brush = new SolidBrush(c);
                //g.FillEllipse(brush, slotX - 5f, slotY - 5f, 10f, 10f);
                //g.DrawEllipse(Pens.White, slotX - 5f, slotY - 5f, 10f, 10f);
                SizeF textSize = g.MeasureString((slot + 1).ToString(), Font);
                RectangleF textBg = new RectangleF(
                    slotX - textSize.Width / 2,
                    slotY,
                    textSize.Width,
                    textSize.Height
                );

                if (n.backgroundImage != null)
                {
                    using (GraphicsPath glasspath = GetRoundedRectanglePath(textBg, 3f))
                    {
                        // Strat 1: umbra subtila
                        using var shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0));
                        RectangleF shadowRect = new RectangleF(textBg.X + 1, textBg.Y + 2, textBg.Width, textBg.Height);
                        using var shadowPath = GetRoundedRectanglePath(shadowRect, 3f);
                        g.FillPath(shadowBrush, shadowPath);

                        // Strat 2: fundal alb semi-transparent (efectul de glass)
                        using var glassBrush = new SolidBrush(Color.FromArgb(120, 255, 255, 255));
                        g.FillPath(glassBrush, glasspath);

                        // Strat 3: highlight alb sus (iluzia de reflexie)
                        RectangleF highlightRect = new RectangleF(textBg.X + 2, textBg.Y + 1, textBg.Width - 4, textBg.Height / 2);
                        using var highlightPath = GetRoundedRectanglePath(highlightRect, 2f);
                        using var highlightBrush = new SolidBrush(Color.FromArgb(60, 255, 255, 255));
                        g.FillPath(highlightBrush, highlightPath);

                        // Strat 4: border subtil
                        using var borderPen = new Pen(Color.FromArgb(80, 255, 255, 255), 1f);
                        g.DrawPath(borderPen, glasspath);
                    }
                }
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
                g.DrawString((slot + 1).ToString(), new Font(Font.FontFamily, 10f), brush,
                    new PointF(slotX, slotY), sf);
            }
           
        }

        void DrawLinks(Graphics g)
        {
            foreach (var lnk in links)
            {
                var src = nodes.Find(n => n.id == lnk.From);
                var dst = nodes.Find(n => n.id == lnk.To);
                if (src == null || dst == null) continue;

                bool isSelected = (lnk == _selectedLink);

                Color baseColor = lnk.OutputSlot < SlotColors.Length
                    ? SlotColors[lnk.OutputSlot] : Color.Purple;

                Color drawColor = isSelected ? Color.DodgerBlue : baseColor;
                float thickness = 1.0f;

                using var pen = new Pen(drawColor, thickness);

                // Mitingul la colțuri (pentru a arăta unghiul de 90 grade curat, fără capete care depășesc)
                pen.LineJoin = LineJoin.Miter;

                using var cap = BuildArrowCap(drawColor);
                pen.CustomEndCap = cap;

                PointF p1 = new(src.Rect.X + src.Rect.Width * (lnk.OutputSlot + 1f) / (src.maxOutputs + 1f), src.Rect.Bottom);
                PointF p2 = new(dst.Rect.X + dst.Rect.Width * (lnk.InputSlot + 1f) / (dst.maxInputs + 1f), dst.Rect.Top);

                // NOU: Trasare linie cu colțuri la 90 de grade
                g.DrawLines(pen, GetOrthogonalPath(p1, p2));
            }
        }

        static CustomLineCap BuildArrowCap(Color color)
        {
            var arrow = new GraphicsPath();
            arrow.AddLines(new PointF[] { new(-4, -6), new(0, 0), new(4, -6) });
            return new CustomLineCap(null, arrow);
        }

        private void DrawGrid(Graphics g)
        {
            float spacing = 20 * _scale;
            if (spacing < 6) return;

            float ox = ((_pan.X % spacing) + spacing) % spacing;
            float oy = ((_pan.Y % spacing) + spacing) % spacing;

            using var dot = new SolidBrush(Color.FromArgb(120, 120, 120));
            for (float x = ox; x < Width; x += spacing)
                for (float y = oy; y < Height; y += spacing)
                    g.FillRectangle(dot, x - 0.8f, y - 0.8f, 1.6f, 1.6f);
        }

        // ── Interacțiune Tastatură (Tasta Delete) ─────────────────────
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Delete)
            {
                if (_selectedNode != null)
                {
                    links.RemoveAll(l => l.From == _selectedNode.id || l.To == _selectedNode.id);
                    nodes.Remove(_selectedNode);
                    _selectedNode = null;
                    Invalidate();
                    return true;
                }
                else if (_selectedLink != null)
                {
                    links.Remove(_selectedLink);
                    _selectedLink = null;
                    Invalidate();
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        public void ForceSetOutput(string id, int nr)
        {
            // 1. Căutăm nodul în listă după ID-ul primit
            var targetNode = nodes.Find(n => n.id == id);

            // 2. Dacă nodul există, îi aplicăm noul număr de ieșiri (nr)
            if (targetNode != null)
            {
                targetNode.maxOutputs = nr;

                // 3. Ștergem legăturile care foloseau sloturi care acum au dispărut
                links.RemoveAll(l => l.From == id && l.OutputSlot >= targetNode.maxOutputs);

                // 4. Redesenăm scena ca să se vadă schimbarea instant
                Invalidate();
            }
        }

        // ── Mouse ─────────────────────────────────────────────────────
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            this.Focus();
            Saved?.Invoke(this, false);
            var hitNode = HitTestNode(e.Location);
            var hitLink = HitTestLink(e.Location);
            PointF worldPt = ScreenToWorld(e.Location);

            if (e.Button == MouseButtons.Left)
            {
                _selectedNode = hitNode;
                _selectedLink = hitLink;

                if (hitNode != null)
                {
                    if (ModifierKeys.HasFlag(Keys.Shift))
                    {
                        int bestSlot = -1;
                        float minDistance = float.MaxValue;

                        for (int i = 0; i < hitNode.maxOutputs; i++)
                        {
                            float slotX = hitNode.Rect.X + hitNode.Rect.Width * (i + 1f) / (hitNode.maxOutputs + 1f);
                            float slotY = hitNode.Rect.Bottom;
                            float dist = (float)Math.Sqrt(Math.Pow(worldPt.X - slotX, 2) + Math.Pow(worldPt.Y - slotY, 2));

                            if (dist < 35f && dist < minDistance)
                            {
                                minDistance = dist;
                                bestSlot = i;
                            }
                        }

                        if (bestSlot != -1)
                        {
                            _linkingNode = hitNode;
                            _linkingSlot = bestSlot;
                            _tempLinkPoint = worldPt;
                        }
                    }
                    else
                    {
                        _draggedNode = hitNode;
                        _lastMousePos = e.Location;
                    }
                }
                else if (hitLink == null)
                {
                    _panning = true;
                    _lastMousePos = e.Location;
                    Cursor = Cursors.SizeAll;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                ContextMenuStrip ctx = new ContextMenuStrip();

                if (hitNode != null)
                {
                    _selectedNode = hitNode;

                    ctx.Items.Add("Inceputul povestii", null, (s, ev) =>
                    {
                        
                        foreach (var n2 in nodes) n2.isStart = false;
                        hitNode.isStart = true;
                        Invalidate();
                    });

                    ctx.Items.Add(new ToolStripSeparator());

                    ctx.Items.Add("Șterge Nod", null, (s, ev) =>
                    {
                        links.RemoveAll(l => l.From == hitNode.id || l.To == hitNode.id);
                        nodes.Remove(hitNode);
                        _selectedNode = null;
                        Invalidate();
                    });

                    ctx.Show(this, e.Location);
                }
                else if (hitLink != null)
                {
                    _selectedLink = hitLink;

                    string slotLabel = (hitLink.OutputSlot + 1).ToString();
                    ctx.Items.Add($"Conexiune (ieșirea {slotLabel})", null, null);
                    ((ToolStripMenuItem)ctx.Items[0]).Enabled = false;
                    ctx.Items.Add(new ToolStripSeparator());

                    ctx.Items.Add("Șterge Conexiune", null, (s, ev) =>
                    {
                        links.Remove(hitLink);
                        _selectedLink = null;
                        Invalidate();
                    });

                    ctx.Show(this, e.Location);
                }
                else
                {
                    ctx.Items.Add("Adauga nod", null, (s, ev) =>
                    {
                        string newId = Guid.NewGuid().ToString().Substring(0, 6);
                        PointF worldPos = ScreenToWorld(e.Location);
                        nodes.Add(new _sceneNode
                        {
                            id = newId,
                            Rect = new RectangleF(worldPos.X, worldPos.Y, 200, 70),
                            maxOutputs = 1,
                            maxInputs = 1
                        });
                        Invalidate();
                    });

                    /*
                    ctx.Items.Add("Adaugă nod (2 ieșiri)", null, (s, ev) =>
                    {
                        string newId = Guid.NewGuid().ToString().Substring(0, 6);
                        PointF worldPos = ScreenToWorld(e.Location);
                        nodes.Add(new _sceneNode
                        {
                            id = newId,
                            Rect = new RectangleF(worldPos.X, worldPos.Y, 200, 70),
                            maxOutputs = 2,
                            maxInputs = 1
                        });
                        Invalidate();
                    });
                    */

                    ctx.Show(this, e.Location);
                }
            }

            Invalidate();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_panning)
            {
                _pan.X += e.X - _lastMousePos.X;
                _pan.Y += e.Y - _lastMousePos.Y;
                _lastMousePos = e.Location;
                Invalidate();
            }
            else if (_draggedNode != null)
            {
                float dx = (e.X - _lastMousePos.X) / _scale;
                float dy = (e.Y - _lastMousePos.Y) / _scale;
                _draggedNode.Rect.X += dx;
                _draggedNode.Rect.Y += dy;
                _lastMousePos = e.Location;
                Invalidate();
            }
            else if (_linkingNode != null)
            {
                _tempLinkPoint = ScreenToWorld(e.Location);
                Invalidate();
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (_linkingNode != null)
            {
                var dropNode = HitTestNode(e.Location);
                if (dropNode != null && dropNode != _linkingNode)
                {
                    int bestInputSlot = 0;
                    float minInputDist = 40f;
                    PointF worldMouse = ScreenToWorld(e.Location);

                    for (int i = 0; i < dropNode.maxInputs; i++)
                    {
                        float slotX = dropNode.Rect.X + dropNode.Rect.Width * (i + 1f) / (dropNode.maxInputs + 1f);
                        float slotY = dropNode.Rect.Top;
                        float dist = (float)Math.Sqrt(Math.Pow(worldMouse.X - slotX, 2) + Math.Pow(worldMouse.Y - slotY, 2));

                        if (dist < minInputDist)
                        {
                            minInputDist = dist;
                            bestInputSlot = i;
                        }
                    }

                    for (int i = 0; i < dropNode.maxInputs; i++)
                    {
                        float slotX = dropNode.Rect.X + dropNode.Rect.Width * (i + 1f) / (dropNode.maxInputs + 1f);
                        float slotY = dropNode.Rect.Top;
                        float dist = (float)Math.Sqrt(Math.Pow(worldMouse.X - slotX, 2) + Math.Pow(worldMouse.Y - slotY, 2));

                        if (dist < minInputDist)
                        {
                            minInputDist = dist;
                            bestInputSlot = i;
                        }
                    }

                    // --- ADAUGĂ ACEST BLOC NOU AICI ---
                    // Verificăm dacă slotul de intrare găsit este deja ocupat de o altă conexiune
                    bool isInputOccupied = links.Exists(l => l.To == dropNode.id && l.InputSlot == bestInputSlot);
                    if (isInputOccupied)
                    {
                        dropNode.maxInputs++; // Creștem numărul maxim de intrări pe nod
                        bestInputSlot = dropNode.maxInputs - 1; // Mutăm această nouă conexiune pe ultimul slot creat
                    }
                    // ----------------------------------

                    if (!links.Exists(l => l.From == _linkingNode.id && l.To == dropNode.id && l.OutputSlot == _linkingSlot && l.InputSlot == bestInputSlot))
                    {
                        links.RemoveAll(l => l.From == _linkingNode.id && l.OutputSlot == _linkingSlot);

                        links.Add(new _sceneLink
                        {
                            From = _linkingNode.id,
                            To = dropNode.id,
                            OutputSlot = _linkingSlot,
                            InputSlot = bestInputSlot
                        });
                    }

                }
                _linkingNode = null;
                _linkingSlot = -1;
                Invalidate();
            }

            _panning = false;
            _draggedNode = null;
            Cursor = Cursors.Default;
        }

        // ── Zoom & Toolbar ────────────────────────────────────────────
        private void OnMouseWheel(object sender, MouseEventArgs e) => Zoom(e.Delta > 0 ? 1.1f : 0.9f, e.Location);

        private void Zoom(float factor, Point center)
        {
            float oldScale = _scale;
            _scale = Math.Clamp(_scale * factor, 0.15f, 4f);
            float ratio = _scale / oldScale;

            _pan.X = center.X - (center.X - _pan.X) * ratio;
            _pan.Y = center.Y - (center.Y - _pan.Y) * ratio;
            Invalidate();
        }

        private void btZoomIn_Click(object sender, EventArgs e) => Zoom(1.1f, new Point(Width / 2, Height / 2));
        private void btZoomOut_Click(object sender, EventArgs e) => Zoom(0.9f, new Point(Width / 2, Height / 2));
        private void btFocus_Click(object sender, EventArgs e)
        {
            _scale = 1f;
            _pan = new PointF(0, 0);
            Invalidate();
        }

        // NOU: Fix dublu click - a fost schimbat EventArgs cu MouseEventArgs pentru e.Location
        private void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var node = HitTestNode(e.Location);
            if (node == null) return;

            NodeDoubleClicked?.Invoke(this, node.id);  
        }

        // ── 1. SCRIERE DIN COD (Adăugare Noduri și Legături) ───────────────────

        /// <summary>
        /// Adaugă un nod nou în scenă direct din cod, specificând poziția și proprietățile.
        /// </summary>
        public void AddNodeFromCode(string id, float x, float y, float width = 200, float height = 70, int maxInputs = 1, int maxOutputs = 1)
        {
            // Ne asigurăm că nu avem ID-uri duplicate
            if (nodes.Exists(n => n.id == id)) return;

            nodes.Add(new _sceneNode
            {
                id = id,
                Rect = new RectangleF(x, y, width, height),
                maxInputs = maxInputs,
                maxOutputs = maxOutputs
            });

            Invalidate(); // Forțează redesenarea interfeței grafice
        }

        /// <summary>
        /// Adaugă o conexiune între două noduri direct din cod.
        /// </summary>
        public bool AddLinkFromCode(string fromId, int outputSlot, string toId, int inputSlot)
        {
            // Validăm dacă ambele noduri există în scenă
            var src = nodes.Find(n => n.id == fromId);
            var dst = nodes.Find(n => n.id == toId);

            if (src == null || dst == null) return false; // Nodurile nu există

            // Verificăm dacă legătura nu există deja
            if (!links.Exists(l => l.From == fromId && l.To == toId && l.OutputSlot == outputSlot && l.InputSlot == inputSlot))
            {
                links.Add(new _sceneLink
                {
                    From = fromId,
                    To = toId,
                    OutputSlot = outputSlot,
                    InputSlot = inputSlot
                });
                Invalidate();
                return true;
            }
            return false;
        }

        public string GetStartNodeId()
        {
            return nodes.Find(n => n.isStart)?.id;
        }
        // ── 2. SALVARE ÎN FIȘIER (Export JSON) ────────────────────────────────

        public void SaveSceneToFile(string filePath)
        {
            var saveModel = new SceneSaveModel();

            // Convertim nodurile interne în formatul de salvare (păstrăm poziția X și Y)
            foreach (var n in nodes)
            {
                saveModel.Nodes.Add(new NodeSaveData
                {
                    Id = n.id,
                    X = n.Rect.X,
                    Y = n.Rect.Y,
                    Width = n.Rect.Width,
                    Height = n.Rect.Height,
                    MaxInputs = n.maxInputs,
                    MaxOutputs = n.maxOutputs, 
                    isStart = n.isStart,
                    BackgroundImagePath = n.backgroundImagePath,

                });
            }

            // Convertim legăturile
            foreach (var l in links)
            {
                saveModel.Links.Add(new LinkSaveData
                {
                    From = l.From,
                    To = l.To,
                    OutputSlot = l.OutputSlot,
                    InputSlot = l.InputSlot
                });
            }

            // Serializăm obiectul în format JSON indentat frumos (ușor de citit)
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(saveModel, options);

            // Salvăm textul în fișier
            File.WriteAllText(filePath, jsonString);
        }


        // ── 3. ÎNCĂRCARE DIN FIȘIER (Import JSON & Poziție) ───────────────────

        public void LoadSceneFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return;

            try
            {
                string jsonString = File.ReadAllText(filePath);
                var saveModel = JsonSerializer.Deserialize<SceneSaveModel>(jsonString);

                if (saveModel == null) return;

                // Ștergem ce aveam în scenă în acest moment
                nodes.Clear();
                links.Clear();

                // Reconstruim nodurile și le punem exact pe pozițiile salvate
                foreach (var n in saveModel.Nodes)
                {
                    nodes.Add(new _sceneNode
                    {
                        id = n.Id,
                        Rect = new RectangleF(n.X, n.Y, n.Width, n.Height),
                        maxInputs = n.MaxInputs,
                        maxOutputs = n.MaxOutputs,
                        isStart = n.isStart,
                        backgroundImagePath = n.BackgroundImagePath
                    });
                }

                // Reconstruim legăturile
                foreach (var l in saveModel.Links)
                {
                    links.Add(new _sceneLink
                    {
                        From = l.From,
                        To = l.To,
                        OutputSlot = l.OutputSlot,
                        InputSlot = l.InputSlot,
                        
                    });
                }

                // Resetăm selecțiile curente pentru a evita erori de tip NullReference
                _selectedNode = null;
                _selectedLink = null;

                Invalidate(); // Redesenăm toată scena actualizată
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea fișierului: {ex.Message}");
            }
        }

        public void ClearView()
        {
            // 1. Golim listele de date
            nodes.Clear();
            links.Clear();

            // 2. Resetăm selecțiile curente
            _selectedNode = null;
            _selectedLink = null;

            // 3. (Opțional) Resetăm pan-ul și zoom-ul la starea inițială
            _pan = new PointF(0, 0);
            _scale = 1.0f;

            // 4. Forțăm redesenarea interfeței
            Invalidate();
        }

    }

    public class NodeSaveData
    {
        public string Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public int MaxOutputs { get; set; }
        public int MaxInputs { get; set; }
        public bool isStart { get; set; }
        public string BackgroundImagePath { get; set; } 
    }

    public class LinkSaveData
    {
        public string From { get; set; }
        public string To { get; set; }
        public int OutputSlot { get; set; }
        public int InputSlot { get; set; }
    }

    public class SceneSaveModel
    {
        public List<NodeSaveData> Nodes { get; set; } = new();
        public List<LinkSaveData> Links { get; set; } = new();
    }

}