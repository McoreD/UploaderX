using System;
namespace UploaderX
{
    public interface IFolderPicker
    {
        Task<string> PickFolder();
    }
}

