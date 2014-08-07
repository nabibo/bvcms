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
	[Table(Name="dbo.MobileAppPlaylistItems")]
	public partial class MobileAppPlaylistItem : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
	#region Private Fields
		
		private int _Id;
		
		private int _PlaylistID;
		
		private int _Type;
		
		private DateTime? _DateX;
		
		private string _Name;
		
		private string _Url;
		
		private string _Thumb;
		
		private string _Speaker;
		
		private string _Reference;
		
		private bool _Enabled;
		
   		
    	
	#endregion
	
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
		
		partial void OnIdChanging(int value);
		partial void OnIdChanged();
		
		partial void OnPlaylistIDChanging(int value);
		partial void OnPlaylistIDChanged();
		
		partial void OnTypeChanging(int value);
		partial void OnTypeChanged();
		
		partial void OnDateXChanging(DateTime? value);
		partial void OnDateXChanged();
		
		partial void OnNameChanging(string value);
		partial void OnNameChanged();
		
		partial void OnUrlChanging(string value);
		partial void OnUrlChanged();
		
		partial void OnThumbChanging(string value);
		partial void OnThumbChanged();
		
		partial void OnSpeakerChanging(string value);
		partial void OnSpeakerChanged();
		
		partial void OnReferenceChanging(string value);
		partial void OnReferenceChanged();
		
		partial void OnEnabledChanging(bool value);
		partial void OnEnabledChanged();
		
    #endregion
		public MobileAppPlaylistItem()
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

		
		[Column(Name="playlistID", UpdateCheck=UpdateCheck.Never, Storage="_PlaylistID", DbType="int NOT NULL")]
		public int PlaylistID
		{
			get { return this._PlaylistID; }

			set
			{
				if (this._PlaylistID != value)
				{
				
                    this.OnPlaylistIDChanging(value);
					this.SendPropertyChanging();
					this._PlaylistID = value;
					this.SendPropertyChanged("PlaylistID");
					this.OnPlaylistIDChanged();
				}

			}

		}

		
		[Column(Name="type", UpdateCheck=UpdateCheck.Never, Storage="_Type", DbType="int NOT NULL")]
		public int Type
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

		
		[Column(Name="date", UpdateCheck=UpdateCheck.Never, Storage="_DateX", DbType="datetime")]
		public DateTime? DateX
		{
			get { return this._DateX; }

			set
			{
				if (this._DateX != value)
				{
				
                    this.OnDateXChanging(value);
					this.SendPropertyChanging();
					this._DateX = value;
					this.SendPropertyChanged("DateX");
					this.OnDateXChanged();
				}

			}

		}

		
		[Column(Name="name", UpdateCheck=UpdateCheck.Never, Storage="_Name", DbType="nvarchar(100) NOT NULL")]
		public string Name
		{
			get { return this._Name; }

			set
			{
				if (this._Name != value)
				{
				
                    this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}

			}

		}

		
		[Column(Name="url", UpdateCheck=UpdateCheck.Never, Storage="_Url", DbType="nvarchar(200) NOT NULL")]
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

		
		[Column(Name="thumb", UpdateCheck=UpdateCheck.Never, Storage="_Thumb", DbType="nvarchar(200) NOT NULL")]
		public string Thumb
		{
			get { return this._Thumb; }

			set
			{
				if (this._Thumb != value)
				{
				
                    this.OnThumbChanging(value);
					this.SendPropertyChanging();
					this._Thumb = value;
					this.SendPropertyChanged("Thumb");
					this.OnThumbChanged();
				}

			}

		}

		
		[Column(Name="speaker", UpdateCheck=UpdateCheck.Never, Storage="_Speaker", DbType="nvarchar(100) NOT NULL")]
		public string Speaker
		{
			get { return this._Speaker; }

			set
			{
				if (this._Speaker != value)
				{
				
                    this.OnSpeakerChanging(value);
					this.SendPropertyChanging();
					this._Speaker = value;
					this.SendPropertyChanged("Speaker");
					this.OnSpeakerChanged();
				}

			}

		}

		
		[Column(Name="reference", UpdateCheck=UpdateCheck.Never, Storage="_Reference", DbType="nvarchar(100) NOT NULL")]
		public string Reference
		{
			get { return this._Reference; }

			set
			{
				if (this._Reference != value)
				{
				
                    this.OnReferenceChanging(value);
					this.SendPropertyChanging();
					this._Reference = value;
					this.SendPropertyChanged("Reference");
					this.OnReferenceChanged();
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

