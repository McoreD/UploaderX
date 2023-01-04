using System;
using UploaderX.ViewModels;

namespace UploaderX
{
    // Basic Model

    public class FileObject : ViewModelBase
    {
        #region Properties
        #region Location
        private string location = string.Empty;
        public string Location
        {
            get { return location; }
            set
            {
                if (value != this.location)
                    location = value;
                this.SetPropertyChanged("Location");
            }
        }
        #endregion Location

        #region FileName
        private string fileName = string.Empty;
        public string FileName
        {
            get { return fileName; }
            set
            {
                if (value != this.fileName)
                    fileName = value;
                this.SetPropertyChanged("FileName");
            }
        }
        #endregion FileName
        #endregion Properties

        #region String Override
        public override string ToString()
        {
            //.FormatString(this string myString) is an extension.
            string returnString = string.Empty;
            if (this.fileName != string.Empty)
                returnString = String.Format("File Name: {0}.", this.fileName);
            return returnString;
        }
        #endregion String Override
    }
}

