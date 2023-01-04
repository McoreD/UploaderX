﻿using System.ComponentModel;
using ReactiveUI;

namespace UploaderX.ViewModels;

// The ViewModelBase
public abstract class ViewModelBase : INotifyPropertyChanged
{
    #region Property Changed Event Handler
    public void SetPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion Property Changed Event Handler
}

