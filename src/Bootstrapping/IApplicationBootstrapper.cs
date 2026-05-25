namespace Bootstrapping;

public interface IApplicationBootstrapper
{
    public Task Run(string[] args);
}