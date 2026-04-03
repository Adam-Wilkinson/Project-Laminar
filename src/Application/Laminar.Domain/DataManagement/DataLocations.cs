using Laminar.Domain.ValueObjects;

namespace Laminar.Domain.DataManagement;

public static class DataLocations
{
    private const string ProjectLaminar = "Project Laminar";
    
    public static readonly FileSystemPath LocalDataFolder = 
        new FileSystemPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
            .ChildPath(ProjectLaminar);
    
    public static readonly FileSystemPath RoamingDataFolder = 
        new FileSystemPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
            .ChildPath(ProjectLaminar);
}