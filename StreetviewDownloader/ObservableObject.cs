using System.ComponentModel;

public abstract class ObservableObject : INotifyPropertyChanged {

	/// <summary>
	/// Raises the PropertyChange event for the property specified
	/// </summary>
	/// <param name="propertyName">Property name to update. Is case-sensitive.</param>
	public virtual void RaisePropertyChanged(string propertyName) {
		OnPropertyChanged(propertyName);
	}

	/// <summary>
	/// Raised when a property on this object has a new value.
	/// </summary>
	public event PropertyChangedEventHandler PropertyChanged;

	/// <summary>
	/// Raises this object's PropertyChanged event.
	/// </summary>
	/// <param name="propertyName">The property that has a new value.</param>
	protected virtual void OnPropertyChanged(string propertyName) {

		PropertyChangedEventHandler handler = this.PropertyChanged;
		if (handler != null) {
			var e = new PropertyChangedEventArgs(propertyName);
			handler(this, e);
		}
	}

}