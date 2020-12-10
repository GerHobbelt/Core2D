﻿using System;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Core2D.Data;

namespace Core2D.Views.Data
{
    public class DatabaseView : UserControl
    {
        private TextBox _filterRecordsText;
        private DataGrid _rowsDataGrid;
        private DatabaseViewModel _databaseViewModel;
        private string _recordsFilter;
        private DataGridCollectionView _recordsView;

        public DatabaseView()
        {
            InitializeComponent();

            _filterRecordsText = this.FindControl<TextBox>("FilterRecordsTextBox");
            _filterRecordsText.GetObservable(TextBox.TextProperty).Subscribe(_ => OnFilterRecordsTextChanged());

            _rowsDataGrid = this.FindControl<DataGrid>("RowsDataGrid");
            _rowsDataGrid.DataContextChanged += RowsDataGrid_DataContextChanged;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnFilterRecordsTextChanged()
        {
            Dispatcher.UIThread.Post(() =>
            {
                _recordsFilter = _filterRecordsText?.Text;
                _recordsView?.Refresh();
            });
        }

        private bool FilterRecords(object arg)
        {
            if (!string.IsNullOrWhiteSpace(_recordsFilter) && arg is RecordViewModel record)
            {
                foreach (var value in record.Values)
                {
                    if (!string.IsNullOrWhiteSpace(value.Content))
                    {
                        if (value.Content.IndexOf(_recordsFilter, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            return true;
        }

        private void RowsDataGrid_DataContextChanged(object sender, EventArgs e)
        {
            if (_databaseViewModel != null)
            {
                _databaseViewModel.PropertyChanged -= DatabaseViewModelPropertyChanged;
                _databaseViewModel = null;
            }

            ResetRecordsView();
            ResetColumns();

            if (DataContext is DatabaseViewModel database)
            {
                _databaseViewModel = database;
                _databaseViewModel.PropertyChanged += DatabaseViewModelPropertyChanged;
                CreateColumns();
                CreateRecordsView();
            }
        }

        private void DatabaseViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DatabaseViewModel.Columns))
            {
                if (_databaseViewModel != null)
                {
                    ResetColumns();
                    CreateColumns();
                }
            }

            if (e.PropertyName == nameof(DatabaseViewModel.Records))
            {
                if (_databaseViewModel != null)
                {
                    ResetRecordsView();
                    CreateRecordsView();
                }
            }
        }

        private void Column_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ColumnViewModel.Name)
                || e.PropertyName == nameof(ColumnViewModel.IsVisible))
            {
                if (_databaseViewModel != null)
                {
                    UpdateHeaders();
                }
            }
        }

        private void CreateRecordsView()
        {
            _recordsView = new DataGridCollectionView(_databaseViewModel.Records);
            _recordsView.Filter = FilterRecords;
            _rowsDataGrid.Items = _recordsView;

            _recordsFilter = _filterRecordsText?.Text;
            _recordsView?.Refresh();
        }

        private void ResetRecordsView()
        {
            _rowsDataGrid.Items = null;
            _recordsView = null;
        }

        private void CreateColumns()
        {
            for (int i = 0; i < _databaseViewModel.Columns.Length; i++)
            {
                var column = _databaseViewModel.Columns[i];
                var dataGridTextColumn = new DataGridTextColumn()
                {
                    Header = $"{column.Name}",
                    Width = DataGridLength.Auto,
                    IsVisible = column.IsVisible,
                    Binding = new Binding($"{nameof(Record.Values)}[{i}].{nameof(ValueViewModel.Content)}"),
                    IsReadOnly = false
                };
                column.PropertyChanged += Column_PropertyChanged;
                _rowsDataGrid.Columns.Add(dataGridTextColumn);
            }
        }

        private void ResetColumns()
        {
            _rowsDataGrid.Columns.Clear();
        }

        private void UpdateHeaders()
        {
            for (int i = 0; i < _databaseViewModel.Columns.Length; i++)
            {
                var column = _databaseViewModel.Columns[i];
                _rowsDataGrid.Columns[i].Header = column.Name;
                _rowsDataGrid.Columns[i].IsVisible = column.IsVisible;
            }
        }
    }
}
