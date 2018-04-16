﻿using System;
using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Interactivity;
using DevExpress.Xpf.Grid;
#if SILVERLIGHT
	using DevExpress.Data.Browsing;
#endif
namespace GridLayoutHelper {
	public class GridLayoutHelper: Behavior<GridControl> {
		public event EventHandler<MyEventArgs> LayoutChanged;
		List<LayoutChangedType> LayoutChangedTypes = new List<LayoutChangedType>();
		GridControl Grid { get { return AssociatedObject; } }
		bool IsLocked;

		#region DependencyPropertyDescriptors
		#if SILVERLIGHT
		DependencyPropertyDescriptor _ActualWidthDescriptor;
		DependencyPropertyDescriptor ActualWidthDescriptor {
			get {
				if (_ActualWidthDescriptor == null)
					_ActualWidthDescriptor = GetPropertyDescriptor("ActualWidth");
				return _ActualWidthDescriptor;
			}
		}
		DependencyPropertyDescriptor _VisibleIndexDescriptor;
		DependencyPropertyDescriptor VisibleIndexDescriptor {
			get {
				if (_VisibleIndexDescriptor == null)
					_VisibleIndexDescriptor =
						GetPropertyDescriptor("VisibleIndex"); return _VisibleIndexDescriptor;
			}
		}
		DependencyPropertyDescriptor _GroupIndexDescriptor;
		DependencyPropertyDescriptor GroupIndexDescriptor {
			get {
				if (_GroupIndexDescriptor == null)
					_GroupIndexDescriptor = GetPropertyDescriptor("GroupIndex");
				return _GroupIndexDescriptor;
			}
		}
		DependencyPropertyDescriptor _VisibleDescriptor;
		DependencyPropertyDescriptor VisibleDescriptor {
			get {
				if (_VisibleDescriptor == null)
					_VisibleDescriptor = GetPropertyDescriptor("Visible");
				return _VisibleDescriptor;
			}
		}
		#else
		DependencyPropertyDescriptor ActualWidthDescriptor {
			get {
				return DependencyPropertyDescriptor.FromProperty(GridColumn.ActualHeaderWidthProperty, typeof(GridColumn));
			}
		}
		DependencyPropertyDescriptor VisibleIndexDescriptor {
			get {
				return DependencyPropertyDescriptor.FromProperty(GridColumn.VisibleIndexProperty, typeof(GridColumn));
			}
		}
		DependencyPropertyDescriptor GroupIndexDescriptor {
			get {
				return DependencyPropertyDescriptor.FromProperty(GridColumn.GroupIndexProperty, typeof(GridColumn));
			}
		}
		DependencyPropertyDescriptor VisibleDescriptor {
			get {
				return DependencyPropertyDescriptor.FromProperty(GridColumn.VisibleProperty, typeof(GridColumn));
			}
		}
		#endif
		#endregion

		protected override void OnAttached() {
			base.OnAttached();
			if (Grid.Columns != null) {
				SubscribeColumns();
			}
			else
				Grid.Loaded += OnGridLoaded;
			Grid.FilterChanged += OnGridFilterChanged;
			Grid.ColumnsPopulated += OnGridColumnsPopulated;
		}
		protected override void OnDetaching() {
			UnSubscribeColumns();
			Grid.Loaded -= OnGridLoaded;
			Grid.FilterChanged -= OnGridFilterChanged;
			Grid.ColumnsPopulated -= OnGridColumnsPopulated;
			base.OnDetaching();
		}

		void SubscribeColumns() {
			Grid.Columns.CollectionChanged += ColumnsCollectionChanged;
			foreach (GridColumn column in Grid.Columns) {
				SubscribeColumn(column);
			}
		}
		void UnSubscribeColumns() {
			Grid.Columns.CollectionChanged -= ColumnsCollectionChanged;
			foreach (GridColumn column in Grid.Columns) {
				UnSubscribeColumn(column);
			}
		}
		void SubscribeColumn(GridColumn column) {
			ActualWidthDescriptor.AddValueChanged(column, OnColumnWidthChanged);
			VisibleIndexDescriptor.AddValueChanged(column, OnColumnVisibleIndexChanged);
			GroupIndexDescriptor.AddValueChanged(column, OnColumnGroupIndexChanged);
			VisibleDescriptor.AddValueChanged(column, OnColumnVisibleChanged);
		}
		void UnSubscribeColumn(GridColumn column) {
			ActualWidthDescriptor.RemoveValueChanged(column, OnColumnVisibleIndexChanged);
			VisibleIndexDescriptor.RemoveValueChanged(column, OnColumnVisibleIndexChanged);
			GroupIndexDescriptor.RemoveValueChanged(column, OnColumnGroupIndexChanged);
			VisibleDescriptor.RemoveValueChanged(column, OnColumnVisibleChanged);
		}

		void ProcessLayoutChanging(LayoutChangedType type) {
			if (!LayoutChangedTypes.Contains(type))
				LayoutChangedTypes.Add(type);
			if (IsLocked)
				return;
			IsLocked = true;
			Dispatcher.BeginInvoke(new Action(() => {
				IsLocked = false;
				LayoutChanged(this, new MyEventArgs { LayoutChangedTypes = LayoutChangedTypes });
				LayoutChangedTypes.Clear();
			}));
		}
		void OnGridLoaded(object sender, System.Windows.RoutedEventArgs e) {
			SubscribeColumns();
			Grid.Columns.CollectionChanged += ColumnsCollectionChanged;
		}
		void OnGridColumnsPopulated(object sender, RoutedEventArgs e) {
			ProcessLayoutChanging(LayoutChangedType.ColumnsCollection);
			SubscribeColumns();
		}
		void ColumnsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
			if (e.NewItems != null || e.OldItems != null) {
				ProcessLayoutChanging(LayoutChangedType.ColumnsCollection);
			}
			if (e.OldItems != null)
				foreach (GridColumn column in e.OldItems)
					UnSubscribeColumn(column);
			if (e.NewItems != null)
				foreach (GridColumn column in e.NewItems)
					SubscribeColumn(column);
		}
		void OnGridFilterChanged(object sender, RoutedEventArgs e) {
			ProcessLayoutChanging(LayoutChangedType.FilerChanged);
		}
		void OnColumnWidthChanged(object sender, EventArgs args) {
			ProcessLayoutChanging(LayoutChangedType.ColumnWidth);
		}
		void OnColumnVisibleIndexChanged(object sender, EventArgs args) {
			ProcessLayoutChanging(LayoutChangedType.ColumnVisibleIndex);
		}
		void OnColumnGroupIndexChanged(object sender, EventArgs args) {
			ProcessLayoutChanging(LayoutChangedType.ColumnGroupIndex);
		}
		void OnColumnVisibleChanged(object sender, EventArgs args) {
			ProcessLayoutChanging(LayoutChangedType.ColumnVisible);
		}
		#if SILVERLIGHT
		DependencyPropertyDescriptor GetPropertyDescriptor(string name) {
			return DependencyPropertyDescriptor.FromProperty(TypeDescriptor.GetProperties(typeof(GridColumn))[name]);
		}
		#endif
	}
	public class MyEventArgs: EventArgs {
		public List<LayoutChangedType> LayoutChangedTypes { get; set; }
	}
	public enum LayoutChangedType {
		ColumnsCollection,
		FilerChanged,
		ColumnGroupIndex,
		ColumnVisibleIndex,
		ColumnWidth,
		ColumnVisible,
		None
	}
}