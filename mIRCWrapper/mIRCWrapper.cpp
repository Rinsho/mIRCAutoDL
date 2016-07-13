//mIRC Wrapper for AutoDL program
//Provides a communication layer between the unmanaged (C-based)
//mIRC client and the managed (C#-based) service.

#include "stdafx.h"
#include "mIRCWrapper.h"

HANDLE file;
LPSTR message;
HWND mWnd;

/// <summary>
/// Class that holds the service.
/// </summary>
ref class Host
{
public:
	static AutoDLMain^ Service;
};

/// <summary>
/// Copies a managed <see cref="System.String"/> to an unmanaged <c>char</c> array.
/// </summary>
void CopyStringToCharArray(String^% mdata, char*& udata)
{
	/* Alternative:
	 * IntPtr strPtr = Marshal::StringToHGlobalAnsi(mdata);
	 * char* wch = static_cast<char*>(strPtr.ToPointer());
	 * size_t sizeInBytes = mdata->Length + 1;
	 * strncpy_s(udata, sizeInBytes, wch, _TRUNCATE);
	 * Marshal::FreeHGlobal(strPtr);
	 */
	pin_ptr<const wchar_t> wch = PtrToStringChars(mdata);			  //pin _data to pass to wcstombs_s
	size_t convertedChars = 0;
	size_t sizeInBytes = mdata->Length + 1;
	sizeInBytes = (sizeInBytes < MIRC_DATABUFFER) ? sizeInBytes : MIRC_DATABUFFER;
	udata = new char[sizeInBytes];
	wcstombs_s(&convertedChars, udata, sizeInBytes, wch, _TRUNCATE);  //call wcstombs_s and prevent overwriting buffer
}

/// <summary>
/// Copies an unmanaged <c>char</c> array to a managed <see cref="System.String"/>
/// and trims any extra separator characters.
/// </summary>
void CopyCharArrayToString(char*& udata, String^% mdata)
{
	array<wchar_t>^ charToTrim = { ' ', ',' , '#' };
	mdata = (gcnew String(udata))->Trim(charToTrim);
}

#pragma region Download Functions

/// <summary>
/// Execute download command in mIRC.
/// </summary>
/// <param name="downloadInfo">The download to be executed</param>
void SendDownloadInfo(Download^ downloadInfo)
{
	char* dl;
	String^ command = "/Start_DL " + downloadInfo->Name + " " + downloadInfo->Packet;
	CopyStringToCharArray(command, dl);
	strncpy_s(message, MIRC_BUFFER, dl, _TRUNCATE);
	SendMessage(mWnd, WM_MCOMMAND, 1, MIRC_FILEMAPNUM);
}

/// <summary>
/// DLL entry-point.
/// </summary>
/// <remarks>
/// Called by mIRC automatically when DLL is loaded.
/// </remarks>
void __stdcall LoadDll(LOADINFO* info)
{
	//Set mIRC LOADINFO paramaters
	info->mKeep = true;
	info->mUnicode = false;
	mWnd = info->mHwnd;

	//Setup file mapping with mIRC
	file = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, MIRC_BUFFER, MIRC_FILEMAP);
	message = static_cast<LPSTR>(MapViewOfFile(file, FILE_MAP_ALL_ACCESS, 0, 0, 0));

	//Start AutoDL service
	Host::Service = gcnew AutoDLMain(gcnew Action<Download^>(SendDownloadInfo), SERVICE_EXTENSION);
	Host::Service->AutoUpdate = true;
	Host::Service->Open();
}

/// <summary>
/// DLL exit-point.
/// </summary>
/// <remarks>
/// Called by mIRC automatically when DLL is unloaded.
/// </remarks>
int __stdcall UnloadDll(int timeout)
{
	//if Dll is simply idle for 10 minutes
	if (timeout == 1)
	{
		//prevent unloading
		return 0;
	}
	//else on mIRC exit or /dll -u, clean up and unload
	Host::Service->Close();
	UnmapViewOfFile(message);
	CloseHandle(file);	
	return 1;
}

/// <summary>
/// Just a pointless call to allow DLL to be loaded and setup
/// since mIRC doesn't have an explicit DLL "load" call.
/// </summary>
mIRCFunc(AutoDL_Start)
{
	return 1;
}

/// <summary>
/// Passes status of current download from mIRC to the service.
/// </summary>
mIRCFunc(RequestNextDownload)
{
	String^ mdata;
	CopyCharArrayToString(data, mdata);
	Host::Service->RequestNextDownload(Convert::ToBoolean(mdata));
	return 1;
}

#pragma endregion
