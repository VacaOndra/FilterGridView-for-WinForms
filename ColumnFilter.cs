using System;
using System.Data;
using System.Windows.Forms;

namespace FilterGridView
{

    public class ColumnFilter
    {
        private enum FilterTypes
        {
            None,
            Like,
            NotLike,
            Is,
            IntNotIs,
            ByteIs,
            BoolAll,
            BoolTrue,
            BoolFalse,
            Starts,
            Ends,
            Lower,
            Bigger,
            DateIs,
            DateToday,
            DateTo,
            DateFrom
        }
        public TextBox TBox { get; private set; }
        public ComboBox CBox { get; private set; }
        public Timer FilterTimer { get; set; }
        private FilterTypes FilterType 
        { 
            get
            {
                return (FilterTypes)Enum.Parse(typeof(FilterTypes), CBox.SelectedValue.ToString());
            }
        }
        public string FilterText 
        { 
            get 
            {
                if(TBox.Text.Length > 0 || FilterType == FilterTypes.BoolTrue || FilterType == FilterTypes.BoolFalse || FilterType == FilterTypes.DateToday)
                {
                    switch (FilterType)
                    {
                        case FilterTypes.Like:
                            return $"[{Name}] LIKE '%{TBox.Text}%'";
                        case FilterTypes.NotLike:
                            return $"[{Name}] NOT LIKE '%{TBox.Text}%'";
                        case FilterTypes.Is:
                            return $"[{Name}] = '{TBox.Text}'";
                        case FilterTypes.IntNotIs:
                            return $"[{Name}] <> '{TBox.Text}'";
                        case FilterTypes.ByteIs:
                            return $"[{Name}] = {TBox.Text}";
                        case FilterTypes.BoolTrue:
                            return $"[{Name}] = 1";
                        case FilterTypes.BoolFalse:
                            return $"[{Name}] = 0";
                        case FilterTypes.Starts:
                            return $"[{Name}] LIKE '{TBox.Text}%'";
                        case FilterTypes.Ends:
                            return $"[{Name}] LIKE '%{TBox.Text}'";
                        case FilterTypes.Lower:
                            return $"[{Name}] < '{TBox.Text}'";
                        case FilterTypes.Bigger:
                            return $"[{Name}] > '{TBox.Text}'";
                        case FilterTypes.DateToday:
                            return $"[{Name}] >= '{DateTime.Today}' AND [{Name}] <= '{DateTime.Today.AddDays(1)}'";
                        case FilterTypes.DateIs:
                            DateTime date;
                            DateTime.TryParse(TBox.Text, out date);
                            if (date != null)
                            {
                                return $"[{Name}] >= '{date}' AND [{Name}] <= '{date.AddDays(1)}'";
                            }
                            else
                            {
                                return "";
                            }
                        case FilterTypes.DateTo:
                            return $"[{Name}] <= '{TBox.Text}'";
                        case FilterTypes.DateFrom:
                            return $"[{Name}] >= '{TBox.Text}'";
                    }
                }
                return "";
            }
        }
        public string Name { get; private set; }
        public event EventHandler FilterChanged;
        public ColumnFilter(string name, Type columnType, int width, DockStyle dockStyle)
        {
            this.Name = name;

            this.TBox = new TextBox
            {
                Name = name,
                Width = width,
                Dock = dockStyle,
            };

            DataTable filterSettings = new DataTable();
            filterSettings.Columns.Add("value");
            filterSettings.Columns.Add("text");

            if (columnType == typeof(String))
            {
                filterSettings.Rows.Add(FilterTypes.Like, "obsahuje");
                filterSettings.Rows.Add(FilterTypes.NotLike, "neobsahuje");
                filterSettings.Rows.Add(FilterTypes.Is, "rovná se");
                filterSettings.Rows.Add(FilterTypes.Starts, "začíná na");
                filterSettings.Rows.Add(FilterTypes.Ends, "končí na");
            }
            else if (columnType == typeof(Int32) || columnType == typeof(Int64) || columnType == typeof(Decimal))
            {
                filterSettings.Rows.Add(FilterTypes.Is, "rovná se");
                filterSettings.Rows.Add(FilterTypes.IntNotIs, "nerovná se");
                filterSettings.Rows.Add(FilterTypes.Lower, "menší");
                filterSettings.Rows.Add(FilterTypes.Bigger, "větší");
            }
            else if (columnType == typeof(Boolean))
            {
                filterSettings.Rows.Add(FilterTypes.BoolAll, "vše");
                filterSettings.Rows.Add(FilterTypes.BoolTrue, "zaškrtnuto");
                filterSettings.Rows.Add(FilterTypes.BoolFalse, "nezaškrtnuto");
                TBox.Enabled = false;
            }
            else if (columnType == typeof(Byte))
            {
                filterSettings.Rows.Add(FilterTypes.ByteIs, "rovná se");
            }
            else if (columnType == typeof(DateTime))
            {
                filterSettings.Rows.Add(FilterTypes.DateIs, "rovná se");
                filterSettings.Rows.Add(FilterTypes.DateToday, "dnes");
                filterSettings.Rows.Add(FilterTypes.DateTo, "do");
                filterSettings.Rows.Add(FilterTypes.DateFrom, "od");
            }
            else if (columnType == typeof(Guid))
            {
                filterSettings.Rows.Add(FilterTypes.Is, "rovná se");
            }
            else
            {
                filterSettings.Rows.Add(FilterTypes.None, "");
                TBox.Enabled = false;
            }

            this.CBox = new ComboBox
            {
                Name = name,
                DisplayMember = "text",
                ValueMember = "value",
                DropDownStyle = ComboBoxStyle.DropDownList,
                SelectedItem = 0,
                Width = width,
                Dock = dockStyle,
                DataSource = filterSettings
            };
            this.TBox.TextChanged += TextBox_TextChanged;
            this.CBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;

            FilterTimer = new Timer
            {
                Interval = 100
            };
            FilterTimer.Tick += Timer_Tick;
        }
        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterChanging(EventArgs.Empty);
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            FilterTimer.Stop();
            FilterTimer.Start();
        }
        private void FilterChanging(EventArgs e)
        {
            FilterChanged(this, e);
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            FilterTimer.Stop();
            FilterChanging(EventArgs.Empty);
        }
    }
}
