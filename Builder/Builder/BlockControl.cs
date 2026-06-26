using Builder.Dialog;
using Builder.Scene;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Builder
{
    public partial class BlockControl : UserControl
    {
        //claude e idoit :))))
        private string _id;
        private string _pathBackgorund;
        private DialogLine[] _lines;
        private bool _hasChoice;
        private Choice[] _choices;
        private bool _transiton;
        private int _nrOutputs = 1;
        public event EventHandler<string> onBack;

        private const int MAX_CHOICES = 3;

        // ── ID ──────────────────────────────────────────────────────────────
        public string GetId() => _id;

        public void SetId(string value)
        {
            _id = value;
            label1.Text = "Block: " + _id;
        }

        // ── Background path ─────────────────────────────────────────────────
        public string GetPathBackground() => _pathBackgorund;

        public void SetPathBackground(string value)
        {
            _pathBackgorund = value;
            tbPath.Text = value;
        }

        // ── Dialog lines  (synced FROM the DataGridView) ────────────────────
        public DialogLine[] GetLines()
        {
            SyncLinesFromGrid();
            return _lines;
        }

        public void SetLines(DialogLine[] value)
        {
            if(value != null) {
                _lines = value;
                dialogData.Rows.Clear();

                if (value == null) return;

                foreach (var l in value)
                    dialogData.Rows.Add(l.name, l.line);
            }
        }

        // ── HasChoice ────────────────────────────────────────────────────────
        public bool GetHasChoice() => _hasChoice;

        public void SetHasChoice(bool value)
        {
            _hasChoice = value;
            checkDecizie.Checked = value;
        }

        // ── Choices  (synced FROM the DataGridView, max 3) ───────────────────
        public Choice[] GetChoices()
        {
            SyncChoicesFromGrid();
            return _choices;
        }

        public void SetChoices(Choice[] value)
        {
            _choices = value;
            decizieData.Rows.Clear();

            if (value == null) return;

            // Respect the cap even when loading saved data
            int count = Math.Min(value.Length, MAX_CHOICES);
            for (int i = 0; i < count; i++)
                decizieData.Rows.Add(i + 1, value[i].text);
        }

        // ── Transition ───────────────────────────────────────────────────────
        public bool GetTransition() => _transiton;

        public void SetTransition(bool value)
        {
            _transiton = value;
            ckTransition.Checked = value;
        }

        // ── NrOutputs ────────────────────────────────────────────────────────
        public int GetNrOutputs()
        {
            SyncChoicesFromGrid();
            return _hasChoice ? _nrOutputs : 1;
        }

        public void SetNrOutputs(int value) => _nrOutputs = value;

        // ── Constructor ──────────────────────────────────────────────────────
        public BlockControl(string id)
        {
            InitializeComponent();
            _id = id;
            label1.Text = "Block: " + _id;

            // Enforce 3-row cap: block new rows before they are committed
            decizieData.RowValidating += DecizieData_RowValidating;
            decizieData.UserAddedRow += DecizieData_UserAddedRow;
        }

        // ── Sync helpers ─────────────────────────────────────────────────────

        /// <summary>Reads the dialogData grid into _lines.</summary>
        private void SyncLinesFromGrid()
        {
            var list = new List<DialogLine>();

            foreach (DataGridViewRow row in dialogData.Rows)
            {
                if (row.IsNewRow) continue;

                string name = row.Cells["Nume"].Value?.ToString() ?? string.Empty;
                string text = row.Cells["Text"].Value?.ToString()?.Replace("\\n", Environment.NewLine) ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(text))
                    list.Add(new DialogLine(name, text));
            }

            _lines = list.ToArray();
        }

        /// <summary>Reads the decizieData grid into _choices (max 3).</summary>
        private void SyncChoicesFromGrid()
        {
            var list = new List<Choice>();
            int nr = 1;
            foreach (DataGridViewRow row in decizieData.Rows)
            {
                if (row.IsNewRow) continue;
                if (list.Count >= MAX_CHOICES) break;   // hard cap
                row.Cells["Nr"].Value = nr++;

                string text = row.Cells["Decizie"].Value?.ToString() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(text))
                   list.Add(new Choice(text, null));
            }
            
            _choices = list.ToArray();
            _nrOutputs = _choices.Length > 0 ? _choices.Length : 1;
        }

        // ── Max-3 enforcement on decizieData ─────────────────────────────────

        /// <summary>
        /// Fired just before a new row is committed.
        /// Cancels the add and warns the user when the cap is reached.
        /// </summary>
        private void DecizieData_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            // Count non-new real rows
            int realRows = 0;
            foreach (DataGridViewRow r in decizieData.Rows)
                if (!r.IsNewRow) realRows++;

            if (realRows > MAX_CHOICES)
            {
                e.Cancel = true;
                MessageBox.Show(
                    $"Maximum {MAX_CHOICES} choices are allowed.",
                    "Limit reached",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Immediately removes any row added beyond the cap
        /// (secondary safety net after RowValidating).
        /// </summary>
        private void DecizieData_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            int realRows = 0;
            foreach (DataGridViewRow r in decizieData.Rows)
                if (!r.IsNewRow) realRows++;


            int nr = 1;
            foreach (DataGridViewRow r in decizieData.Rows)
            {
                if (r.IsNewRow) continue;
                r.Cells["Nr"].Value = nr++;
            }

            if (realRows > MAX_CHOICES)
            {
                // Remove the last non-new row
                for (int i = decizieData.Rows.Count - 1; i >= 0; i--)
                {
                    if (!decizieData.Rows[i].IsNewRow)
                    {
                        decizieData.Rows.RemoveAt(i);
                        break;
                    }
                }

                MessageBox.Show(
                    $"Maximum {MAX_CHOICES} choices are allowed.",
                    "Limit reached",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        // ── UI event handlers ────────────────────────────────────────────────

        private void BlockControl_Load(object sender, EventArgs e) {
        }

        private void splitContainer1_SplitterMoved_1(object sender, SplitterEventArgs e) {
        }

        private void btClose_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            onBack?.Invoke(sender, _id);
        }

        private void btPath_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                _pathBackgorund = openFileDialog1.FileName;
                tbPath.Text = _pathBackgorund;
            }
        }

        private void checkDecizie_CheckedChanged(object sender, EventArgs e)
        {
            _hasChoice = checkDecizie.Checked;
            // Optionally show/hide the decizieData panel here
            // decizieData.Visible = _hasChoice;
        }

        private void ckTransition_CheckedChanged(object sender, EventArgs e)
        {
            _transiton = ckTransition.Checked;
        }
    }
}