namespace UploadedFilesLibrary;

public interface ISqlDataAccess
{
	Task SaveData(string storedProc, string connName, object parameters);
}