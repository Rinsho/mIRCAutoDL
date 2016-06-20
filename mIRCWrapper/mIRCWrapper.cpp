//mIRC Wrapper for AutoDL program

#include "stdafx.h"
#include "mIRCWrapper.h"

HANDLE file;
LPSTR message;
HWND mWnd;

ref class Host
{
public:
	static AutoDLMain^ Service;
};

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

void CopyCharArrayToString(char*& udata, String^% mdata)
{
	array<wchar_t>^ charToTrim = { ' ', ',' , '#' };
	mdata = (gcnew String(udata))->Trim(charToTrim);
}

#pragma region Download Functions

void SendDownloadInfo(AutoDL::Data::Download^ downloadInfo)
{
	char* dl;
	String^ command = "/Start_DL " + downloadInfo->Name + " " + downloadInfo->Packet;
	CopyStringToCharArray(command, dl);
	strncpy_s(message, MIRC_BUFFER, dl, _TRUNCATE);
	SendMessage(mWnd, WM_MCOMMAND, 1, MIRC_FILEMAPNUM);
}

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
	Host::Service = gcnew AutoDLMain(gcnew Action<AutoDL::Data::Download^>(SendDownloadInfo), SERVICE_EXTENSION);
}

int __stdcall UnloadDll(int timeout)
{
	//if Dll is simply idle for 10 minutes
	if (timeout == 1)
	{
		//prevent unloading
		return 0;
	}
	//else on mIRC exit or /dll -u, clean up and unload
	UnmapViewOfFile(message);
	CloseHandle(file);
	Host::Service->Close();
	return 1;
}

mIRCFunc(AutoDL_Start)
{
	return 1;
}

mIRCFunc(DownloadStatus)
{
	String^ mdata;
	CopyCharArrayToString(data, mdata);
	Host::Service->DownloadStatusUpdate(Convert::ToBoolean(mdata));
	return 1;
}

#pragma endregion
