using NextPlayerDataLayer.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace BluetoothDemo
{
    public class FileItemToShare : INotifyPropertyChanged
    {
        private String m_FileName = String.Empty;
        private String m_FilePath = String.Empty;
        private UInt64 m_FileSize = 0;
        private UInt64 m_Progress = 0;
        private FileShareStatus m_FileShareStatus = FileShareStatus.None;
        private IStorageFile m_File = null;

        public FileItemToShare(String fileName, String filePath, IStorageFile fileToShare)
        {
            this.m_FileName = fileName;
            this.m_FilePath = filePath;
            this.FileSize = fileToShare.OpenAsync(FileAccessMode.Read).AsTask().Result.Size;
            this.m_File = fileToShare;
            m_FileShareStatus = FileShareStatus.Waiting;
        }

        public FileItemToShare(string fileName, string filePath)
        {
            this.m_FileName = fileName;
            this.m_FilePath = filePath;
            this.m_FileSize = 500;
            this.m_FileShareStatus = FileShareStatus.Waiting;
            this.m_Progress = 200;
        }
        public String FileName
        {
            get
            {
                return m_FileName;
            }
            set
            {
                if (!m_FileName.Equals(value))
                {
                    m_FileName = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public String FilePath
        {
            get
            {
                return m_FilePath;
            }
            set
            {
                if (!m_FilePath.Equals(value))
                {
                    m_FilePath = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public UInt64 FileSize
        {
            get
            {
                return m_FileSize;
            }
            set
            {
                if (!m_FileSize.Equals(value))
                {
                    m_FileSize = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public UInt64 Progress
        {
            get
            {
                return m_Progress;
            }
            set
            {
                if (!m_Progress.Equals(value))
                {
                    m_Progress = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public FileShareStatus ShareStatus
        {
            get
            {
                return m_FileShareStatus;
            }
            set
            {
                if (!m_FileShareStatus.Equals(value))
                {
                    m_FileShareStatus = value;
                    RaisePropertyChangedEvent();
                }
            }
        }
        public void SetStatus(FileShareStatus status)
        {
            m_FileShareStatus = status;
        }
        public void SetProgress(ulong progress)
        {
            m_Progress = progress;
        }
        public IStorageFile FileToShare
        {
            get
            {
                return m_File;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
