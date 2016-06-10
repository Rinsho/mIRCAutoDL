//mIRC Wrapper for AutoDL program

#include "stdafx.h"
#include "mIRCWrapper.h"

HANDLE file;
LPSTR message;
HWND mWnd;

static ref class Host
{
public:
	static property AutoDLMain^ Service;
	static property ServiceClient::AutoDLClient^ Client;
};

/* Function: CopyStringToCharArray
 * Input: mdata
 * Output: udata
 * Description: Function takes managed string mdata and converts it into an unmanaged
 *              char* udata.
 */
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
	sizeInBytes = (sizeInBytes < MIRC_MAXBUFFER) ? sizeInBytes : MIRC_MAXBUFFER;
	udata = new char[sizeInBytes];
	wcstombs_s(&convertedChars, udata, sizeInBytes, wch, _TRUNCATE);  //call wcstombs_s and prevent overwriting buffer
}

/* Function: CopyCharArrayToString
* Input: udata
* Output: mdata
* Description: Function takes unmanaged char* udata and converts it into a managed
*              string mdata while trimming any erroneous extra separator values (space or ,).
*/
void CopyCharArrayToString(char*& udata, String^% mdata)
{
	array<wchar_t>^ charToTrim = { ' ', ',' };
	mdata = (gcnew String(udata))->Trim(charToTrim);
}

void SendDownloadInfo(AutoDL::Data::Download^ downloadInfo)
{
	char* dl;
	CopyStringToCharArray(downloadInfo->Name + ITEM_SEPARATOR + downloadInfo->Packet, dl);
	strncpy(message, dl, strlen(dl));
	SendMessage(mWnd, WM_MCOMMAND, 1, 0);
}

#pragma region Exported functions

void __stdcall LoadDll(LOADINFO* info)
{
	//Set mIRC LOADINFO paramaters
	info->mKeep = true;
	info->mUnicode = false;

	//Setup file mapping with mIRC
	file = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, MIRC_MAXBUFFER, L"mIRC");
	message = static_cast<LPSTR>(MapViewOfFile(file, FILE_MAP_ALL_ACCESS, 0, 0, 0));

	//Start AutoDL service
	Host::Service = gcnew AutoDLMain(gcnew Action<AutoDL::Data::Download^>(SendDownloadInfo), "mIRC");

	//Setup service client
	Host::Client = gcnew ServiceClient::AutoDLClient("mIRC");
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

mIRCFunc(DownloadStatus)
{
	String^ mdata = gcnew String("");
	CopyCharArrayToString(data, mdata);
	Host::Service->DownloadStatusUpdate(Convert::ToBoolean(mdata));
	return 1;
}
#pragma endregion
