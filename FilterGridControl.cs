using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;

namespace FilterGridView
{
    
    public class FilterGridControl : UserControl
    {
        protected bool loading;
        protected string filter;
        protected List<String> tempFilter;
        protected BindingSource bindingSource;
        public DataGridView gridView;
        protected Label infoLabel;
        protected Panel bottomPanel;
        protected Panel topComboPanel;
        protected Panel topTextPanel;
        protected ColumnFilter columnFilter;
        protected DataTable source;
        private List<ComboBox> savedComboBoxes;
        private List<TextBox> savedTextBoxes;
        private ArrayList columnFilters;
        private static ListSortDirection oldSortOrder;
        private static DataGridViewColumn oldSortColumn;
        private static int? lastSelectedRow;
        public bool RightClickOnMoreRows { get; set; }
        public DataTable Source
        {
            get
            {
                return source;
            }
            set
            {
                source = value;
                LoadData(source);
            }
        }
        private void Initialize()
        {
            this.SuspendLayout();
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;

            gridView = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Dock = DockStyle.Fill,
                RowHeadersVisible = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            gridView.CellMouseDown += new DataGridViewCellMouseEventHandler(this.GridView_CellMouseDown);
            gridView.ColumnWidthChanged += new DataGridViewColumnEventHandler(this.GridView_ColumnWidthChanged);
            gridView.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(this.GridView_DataBindingComplete);
            gridView.SelectionChanged += new EventHandler(this.GridView_SelectionChanged);

            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));

            topTextPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            topComboPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            bottomPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            infoLabel = new Label
            {
                Dock = DockStyle.Right,
                TextAlign = System.Drawing.ContentAlignment.MiddleRight
            };

            columnFilters = new ArrayList();

            this.Controls.Add(tableLayoutPanel);
            tableLayoutPanel.Controls.Add(topComboPanel, 0, 0);
            tableLayoutPanel.Controls.Add(topTextPanel, 0, 1);
            tableLayoutPanel.Controls.Add(gridView, 0, 2);
            tableLayoutPanel.Controls.Add(bottomPanel, 0, 3);
            bottomPanel.Controls.Add(infoLabel);

            source = new DataTable();
            tempFilter = new List<string>();
            filter = string.Empty;
            loading = false;

            this.ResumeLayout(false);
        }
        public FilterGridControl()
        {
            Initialize();
        }
        private void LoadData(DataTable dataTable)
        {
            Cursor.Current = Cursors.WaitCursor;
            loading = true;
            gridView.ContextMenuStrip = null;
            RightClickOnMoreRows = false;
            bindingSource = new BindingSource();
            gridView.DataSource = null;
            topTextPanel.Controls.Clear();
            topComboPanel.Controls.Clear();
            filter = string.Empty;
            infoLabel.Text = string.Empty;
            bindingSource.DataSource = dataTable;
            gridView.DataSource = bindingSource;
            LoadFilters();
            loading = false;
            Cursor.Current = Cursors.Default;
        }
        public void SaveFilters()
        {
            Cursor.Current = Cursors.WaitCursor;
            SaveSorting();
            savedComboBoxes = new List<ComboBox>();
            savedTextBoxes = new List<TextBox>();
            foreach (var control in topComboPanel.Controls)
            {
                if (control is ComboBox)
                {
                    savedComboBoxes.Add((ComboBox)control);
                }
            }
            foreach (var control in topTextPanel.Controls)
            {
                if (control is TextBox)
                {
                    savedTextBoxes.Add((TextBox)control);
                }
            }
            Cursor.Current = Cursors.Default;
        }
        public void RestoreFilters()
        {
            Cursor.Current = Cursors.WaitCursor;
            topTextPanel.Visible = false;
            topComboPanel.Visible = false;
            if (savedComboBoxes != null && savedComboBoxes.Count > 0 && savedTextBoxes != null && savedTextBoxes.Count > 0)
            {
                foreach (ComboBox comboBox in savedComboBoxes)
                {
                    foreach (var control in topComboPanel.Controls)
                    {
                        if (control is ComboBox)
                        {
                            if (((ComboBox)control).Name == comboBox.Name)
                            {
                                ((ComboBox)control).SelectedIndex = comboBox.SelectedIndex;
                                break;
                            }
                        }
                    }
                }

                foreach (TextBox textBox in savedTextBoxes)
                {
                    foreach (var control in topTextPanel.Controls)
                    {
                        if (control is TextBox)
                        {
                            if (((TextBox)control).Name == textBox.Name)
                            {
                                ((TextBox)control).Text = textBox.Text;
                                break;
                            }
                        }
                    }
                }
                savedComboBoxes.Clear();
                savedTextBoxes.Clear();
                RestoreSorting();
                topTextPanel.Visible = true;
                topComboPanel.Visible = true;
                Cursor.Current = Cursors.Default;
            }
        }
        private void SaveSorting()
        {
            oldSortOrder = gridView.SortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;
            oldSortColumn = gridView.SortedColumn;
        }
        private void RestoreSorting()
        {
            if (oldSortColumn != null)
            {
                DataGridViewColumn newCol = gridView.Columns[oldSortColumn.Name];
                gridView.Sort(newCol, oldSortOrder);
            }
            if (lastSelectedRow != null)
            {
                try
                {
                    gridView.ClearSelection();
                    gridView.Rows[(int)lastSelectedRow].Selected = true;
                    gridView.FirstDisplayedScrollingRowIndex = gridView.SelectedRows[0].Index;
                }
                catch{ }
            }
        }
        private void LoadFilters()
        {
            topTextPanel.Visible = false;
            topComboPanel.Visible = false;
            columnFilters.Clear();
            for (int i = gridView.ColumnCount - 1; i >= 0; i--)
            {
                if (gridView.Columns[i].Visible == true)
                { 
                    columnFilter = new ColumnFilter(gridView.Columns[i].Name, gridView.Columns[i].ValueType, gridView.Columns[i].Width, DockStyle.Left);
                    columnFilter.FilterChanged += SetFilter;
                    topTextPanel.Controls.Add(columnFilter.TBox);
                    topComboPanel.Controls.Add(columnFilter.CBox);
                    columnFilters.Add(columnFilter);
                }
            }
            tempFilter.Clear();
            for (int i = 0; i < gridView.ColumnCount; i++)
            {
                tempFilter.Insert(i, string.Empty);
            }
            topComboPanel.Visible = true;
            topTextPanel.Visible = true;
        }
        private void SetFilter(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            loading = true;
            List<string> listOfFilters = new List<string>();
            filter = string.Empty;

            foreach (ColumnFilter columnFilter in columnFilters)
            {
                    if (columnFilter.FilterText.Length > 0)
                    {
                        if (listOfFilters.Count > 0)
                        {
                            listOfFilters.Add(" AND ");
                        }
                        listOfFilters.Add(columnFilter.FilterText);
                    }
            }

            foreach (string s in listOfFilters)
            {
                filter += s;
            }
            try
            {
                bindingSource.Filter = filter;
            }
            catch { }
            loading = false;
            AlignFiletrs();
            Cursor.Current = Cursors.Default;
        }
        protected virtual void UpdateInfoLabel()
        {
            infoLabel.Text = $"{gridView.SelectedRows.Count} | {gridView.Rows.Count}";
        }
        private void GridView_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            AlignFiletrs();
        }
        protected virtual void GridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            Cursor.Current = Cursors.Default;
            UpdateInfoLabel();
        }
        protected virtual void GridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && !RightClickOnMoreRows)
            {
                if (e.RowIndex != -1)
                {
                    this.gridView.ClearSelection();
                    this.gridView.Rows[e.RowIndex].Selected = true;
                }
            }
            if (e.RowIndex != -1)
            {
                lastSelectedRow = e.RowIndex;
            }
        }
        protected virtual void GridView_SelectionChanged(object sender, EventArgs e)
        {
            UpdateInfoLabel();
        }
        private void AlignFiletrs()
        {
            if (!loading)
            {
                foreach (var control in topTextPanel.Controls)
                {
                    if (control is TextBox)
                    {
                        TextBox textBox = control as TextBox;
                        for (int i = gridView.ColumnCount - 1; i >= 0; i--)
                        {
                            if (textBox.Name == gridView.Columns[i].Name)
                            {
                                textBox.Width = gridView.Columns[i].Width;
                                break;
                            }
                        }
                    }
                }

                foreach (var control in topComboPanel.Controls)
                {
                    if (control is ComboBox)
                    {
                        ComboBox comboBox = control as ComboBox;
                        for (int i = gridView.ColumnCount - 1; i >= 0; i--)
                        {
                            if (comboBox.Name == gridView.Columns[i].Name)
                            {
                                comboBox.Width = gridView.Columns[i].Width;
                                break;
                            }
                        }
                    }
                }
            }
        }
        public void FillFilters(StringCollection savedFilters)
        {
            if(columnFilters.Count == savedFilters.Count)
            {
                foreach(ColumnFilter filter in columnFilters)
                {
                    foreach(string s in savedFilters)
                    {
                        string[] subArray = s.Split(',');
                        if (filter.Name == subArray[0])
                        {
                            filter.TBox.Text = subArray[1];
                            filter.CBox.SelectedIndex = int.Parse(subArray[2]);
                            break;
                        }
                    }
                }
            }
        }
        public StringCollection ReturnFilters()
        {
            StringCollection result = new StringCollection();
            foreach(ColumnFilter filter in columnFilters)
            {
                result.Add($"{filter.Name},{filter.TBox.Text},{filter.CBox.SelectedIndex}");
            }
            return result;
        }
    }
}