using System; 
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel;

namespace CmsData
{
	[Table(Name="dbo.MobileAppActions")]
	public partial class MobileAppAction : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
	#region Private Fields
		
		private int _Id;
		
		private int _Section;
		
		private string _Type;
		
		private string _Title;
		
		private string _Url;
		
		private bool _Custom;
		
		private int _Order;
		
		private bool _Enabled;
		
   		
    	
	#endregion
	
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
		
		partial void OnIdChanging(int value);
		partial void OnIdChanged();
		
		partial void OnSectionChanging(int value);
		partial void OnSectionChanged();
		
		partial void OnTypeChanging(string value);
		partial void OnTypeChanged();
		
		partial void OnTitleChanging(string value);
		partial void OnTitleChanged();
		
		partial void OnUrlChanging(string value);
		partial void OnUrlChanged();
		
		partial void OnCustomChanging(bool value);
		partial void OnCustomChanged();
		
		partial void OnOrderChanging(int value);
		partial void OnOrderChanged();
		
		partial void OnEnabledChanging(bool value);
		partial void OnEnabledChanged();
		
    #endregion
		public MobileAppAction()
		{
			
			
			OnCreated();
		}

		
    #region Columns
		
		[Column(Name="id", UpdateCheck=UpdateCheck.Never, Storage="_Id", AutoSync=AutoSync.OnInsert, DbType="int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int Id
		{
			get { return this._Id; }

			set
			{
				if (this._Id != value)
				{
				
                    this.OnIdChanging(value);
					this.SendPropertyChanging();
					this._Id = value;
					this.SendPropertyChanged("Id");
					this.OnIdChanged();
				}

			}

		}

		
		[Column(Name="section", UpdateCheck=UpdateCheck.Never, Storage="_Section", DbType="int NOT NULL")]
		public int Section
		{
			get { return this._Section; }

			set
			{
				if (this._Section != value)
				{
				
                    this.OnSectionChanging(value);
					this.SendPropertyChanging();
					this._Section = value;
					this.SendPropertyChanged("Section");
					this.OnSectionChanged();
				}

			}

		}

		
		[Column(Name="type", UpdateCheck=UpdateCheck.Never, Storage="_Type", DbType="nvarchar(50) NOT NULL")]
		public string Type
		{
			get { return this._Type; }

			set
			{
				if (this._Type != value)
				{
				
                    this.OnTypeChanging(value);
					this.SendPropertyChanging();
					this._Type = value;
					this.SendPropertyChanged("Type");
					this.OnTypeChanged();
				}

			}

		}

		
		[Column(Name="title", UpdateCheck=UpdateCheck.Never, Storage="_Title", DbType="nvarchar(50) NOT NULL")]
		public string Title
		{
			get { return this._Title; }

			set
			{
				if (this._Title != value)
				{
				
                    this.OnTitleChanging(value);
					this.SendPropertyChanging();
					this._Title = value;
					this.SendPropertyChanged("Title");
					this.OnTitleChanged();
				}

			}

		}

		
		[Column(Name="url", UpdateCheck=UpdateCheck.Never, Storage="_Url", DbType="nvarchar NOT NULL")]
		public string Url
		{
			get { return this._Url; }

			set
			{
				if (this._Url != value)
				{
				
                    this.OnUrlChanging(value);
					this.SendPropertyChanging();
					this._Url = value;
					this.SendPropertyChanged("Url");
					this.OnUrlChanged();
				}

			}

		}

		
		[Column(Name="custom", UpdateCheck=UpdateCheck.Never, Storage="_Custom", DbType="bit NOT NULL")]
		public bool Custom
		{
			get { return this._Custom; }

			set
			{
				if (this._Custom != value)
				{
				
                    this.OnCustomChanging(value);
					this.SendPropertyChanging();
					this._Custom = value;
					this.SendPropertyChanged("Custom");
					this.OnCustomChanged();
				}

			}

		}

		
		[Column(Name="order", UpdateCheck=UpdateCheck.Never, Storage="_Order", DbType="int NOT NULL")]
		public int Order
		{
			get { return this._Order; }

			set
			{
				if (this._Order != value)
				{
				
                    this.OnOrderChanging(value);
					this.SendPropertyChanging();
					this._Order = value;
					this.SendPropertyChanged("Order");
					this.OnOrderChanged();
				}

			}

		}

		
		[Column(Name="enabled", UpdateCheck=UpdateCheck.Never, Storage="_Enabled", DbType="bit NOT NULL")]
		public bool Enabled
		{
			get { return this._Enabled; }

			set
			{
				if (this._Enabled != value)
				{
				
                    this.OnEnabledChanging(value);
					this.SendPropertyChanging();
					this._Enabled = value;
					this.SendPropertyChanged("Enabled");
					this.OnEnabledChanged();
				}

			}

		}

		
    #endregion
        
    #region Foreign Key Tables
   		
	#endregion
	
	#region Foreign Keys
    	
	#endregion
	
		public event PropertyChangingEventHandler PropertyChanging;
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
				this.PropertyChanging(this, emptyChangingEventArgs);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

   		
	}

}

