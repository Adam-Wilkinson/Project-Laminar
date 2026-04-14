using System;
using System.Collections.Generic;
using System.Text;

namespace Laminar.Contracts.UserData.FileNavigation;

public interface ILaminarStorageFile : ILaminarStorageItem
{
    public long SizeOnDisk { get; }
}
