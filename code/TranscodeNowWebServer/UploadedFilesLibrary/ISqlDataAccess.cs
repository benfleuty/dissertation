﻿namespace UploadFilesLibrary
{
	public interface ISqlDataAccess
	{
		Task SaveData(string storedProc, string connName, object parameters);
	}
}