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
	sizeInBytes = (sizeInBytes < MIRC_DATABUFFER) ? sizeInBytes : MIRC_DATABUFFER;
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
	array<wchar_t>^ charToTrim = { ' ', ',' , '#' };
	mdata = (gcnew String(udata))->Trim(charToTrim);
}

void SendErrorMessage(String^ text, char* udata)
{
	String^ message = gcnew String("#Error,") + text;
	CopyStringToCharArray(message, udata);
}

void SendOKMessage(char*& udata)
{
	udata = new char[]{ '#', 'O', 'K' };
}

#pragma region Download Functions
void __stdcall LoadDll(LOADINFO* info)
{
	//Set mIRC LOADINFO paramaters
	info->mKeep = true;
	info->mUnicode = false;

	//Setup file mapping with mIRC
	file = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, MIRC_BUFFER, MIRC_FILEMAP);
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
	Host::Client->CloseClient();
	Host::Service->Close();
	return 1;
}

mIRCFunc(DownloadStatus)
{
	String^ mdata;
	CopyCharArrayToString(data, mdata);
	Host::Service->DownloadStatusUpdate(Convert::ToBoolean(mdata));
	return 1;
}

void SendDownloadInfo(AutoDL::Data::Download^ downloadInfo)
{
	char* dl;
	String^ command = "/msg " + downloadInfo->Name + " xdcc send " + downloadInfo->Packet;
	CopyStringToCharArray(command, dl);
	strncpy(message, dl, strlen(dl));
	SendMessage(mWnd, WM_MCOMMAND, 1, MIRC_FILEMAPNUM);
}
#pragma endregion

#pragma region UI Functions

#pragma region Settings
mIRCFunc(Settings_Update)
{
	String^ mdata;
	CopyCharArrayToString(data, mdata);
	array<String^, 1>^ splitData = mdata->Split(ITEM_SEPARATOR);
	try
	{
		static_cast<ServiceContracts::ISettings^>(Host::Client)->Update(splitData[0], splitData[1]);
		SendOKMessage(data);
	}
	catch (TimeoutException^ ex)
	{
		SendErrorMessage("Timeout exception", data);
	}
	catch (System::ServiceModel::CommunicationException^ ex)
	{
		SendErrorMessage("Communication exception", data);
	}
	catch (System::ServiceModel::FaultException<ServiceContracts::InvalidSettingFault^>^ ex)
	{
		SendErrorMessage("Cannot access configuration file", data);
	}
	return 3;
}

mIRCFunc(Settings_Default)
{
	String^ mdata;
	CopyCharArrayToString(data, mdata);
	try
	{
		static_cast<ServiceContracts::ISettings^>(Host::Client)->Default(mdata);
		SendOKMessage(data);
	}
	catch (TimeoutException^ ex)
	{
		SendErrorMessage("Timeout exception", data);
	}
	catch (System::ServiceModel::CommunicationException^ ex)
	{
		SendErrorMessage("Communication exception", data);
	}
	catch (System::ServiceModel::FaultException<ServiceContracts::InvalidSettingFault^>^ ex)
	{
		SendErrorMessage("Invalid setting value", data);
	}
	return 3;
}

mIRCFunc(Settings_DefaultAll)
{
	try
	{
		static_cast<ServiceContracts::ISettings^>(Host::Client)->DefaultAll();
		SendOKMessage(data);
	}
	catch (TimeoutException^ ex)
	{
		SendErrorMessage("Timeout exception", data);
	}
	catch (System::ServiceModel::CommunicationException^ ex)
	{
		SendErrorMessage("Communication exception", data);
	}
	return 3;
}

mIRCFunc(Settings_Save)
{
	try
	{
		static_cast<ServiceContracts::ISettings^>(Host::Client)->Save();
		SendOKMessage(data);
	}
	catch (TimeoutException^ ex)
	{
		SendErrorMessage("Timeout exception", data);
	}
	catch (System::ServiceModel::CommunicationException^ ex)
	{
		SendErrorMessage("Communication exception", data);
	}
	catch (System::ServiceModel::FaultException<ServiceContracts::ConfigurationFault^>^ ex)
	{
		SendErrorMessage("Cannot access configuration file", data);
	}
	return 3;
}

mIRCFunc(Settings_Load)
{
	try
	{
		Dictionary<String^, String^>^ settings = static_cast<ServiceContracts::ISettings^>(Host::Client)->Load();
		System::Text::StringBuilder^ formattedSettings = gcnew System::Text::StringBuilder();
		for each (KeyValuePair<String^, String^>^ pair in settings)
		{
			formattedSettings->Append(pair->Key);
			formattedSettings->Append(ITEM_SEPARATOR);
			formattedSettings->Append(pair->Value);
			formattedSettings->Append(GROUP_SEPARATOR);
		}
		CopyStringToCharArray(formattedSettings->ToString()->TrimEnd(GROUP_SEPARATOR), data);
	}
	catch (TimeoutException^ ex)
	{
		SendErrorMessage("Timeout exception", data);
	}	
	catch (System::ServiceModel::CommunicationException^ ex)
	{
		SendErrorMessage("Communication exception", data);
	}
	catch (System::ServiceModel::FaultException<ServiceContracts::ConfigurationFault^>^ ex)
	{
		SendErrorMessage("Cannot access configuration file", data);
	}
	return 3;
}
#pragma endregion

#pragma region Alias
mIRCFunc(Alias_Add)
{
	String^ mdata;
	CopyCharArrayToString(data, mdata);
	array<String^, 1>^ splitData = mdata->Split(ITEM_SEPARATOR);
	try
	{
		static_cast<ServiceContracts::IAlias^>(Host::Client)->Add(splitData[0], splitData[1]);
		SendOKMessage(data);
	}
	catch (TimeoutException^ ex)
	{
		SendErrorMessage("Timeout exception", data);
	}
	catch (System::ServiceModel::CommunicationException^ ex)
	{
		SendErrorMessage("Communication exception", data);
	}
	return 3;
}

mIRCFunc(Alias_Remove)
{
	String^ mdata;
	CopyCharArrayToString(data, mdata);
	try
	{
		static_cast<ServiceContracts::IAlias^>(Host::Client)->Remove(mdata);
		SendOKMessage(data);
	}
	catch (TimeoutException^ ex)
	{
		SendErrorMessage("Timeout exception", data);
	}
	catch (System::ServiceModel::CommunicationException^ ex)
	{
		SendErrorMessage("Communication exception", data);
	}
	return 3;
}

mIRCFunc(Alias_Clear)
{
	try
	{
		static_cast<ServiceContracts::IAlias^>(Host::Client)->Clear();
		SendOKMessage(data);
	}
	catch (TimeoutException^ ex)
	{
		SendErrorMessage("Timeout exception", data);
	}
	catch (System::ServiceModel::CommunicationException^ ex)
	{
		SendErrorMessage("Communication exception", data);
	}
	return 3;
}

mIRCFunc(Alias_Save)
{
	try
	{
		static_cast<ServiceContracts::IAlias^>(Host::Client)->Save();
		SendOKMessage(data);
	}
	catch (TimeoutException^ ex)
	{
		SendErrorMessage("Timeout exception", data);
	}
	catch (System::ServiceModel::CommunicationException^ ex)
	{
		SendErrorMessage("Communication exception", data);
	}
	catch (System::ServiceModel::FaultException<ServiceContracts::ConfigurationFault^>^ ex)
	{
		SendErrorMessage("Cannot access configuration file", data);
	}
	return 3;
}

mIRCFunc(Alias_Load)
{
	try
	{
		Dictionary<String^, String^>^ aliases = static_cast<ServiceContracts::IAlias^>(Host::Client)->Load();
		System::Text::StringBuilder^ formattedSettings = gcnew System::Text::StringBuilder();
		for each (KeyValuePair<String^, String^>^ pair in aliases)
		{
			formattedSettings->Append(pair->Key);
			formattedSettings->Append(ITEM_SEPARATOR);
			formattedSettings->Append(pair->Value);
			formattedSettings->Append(GROUP_SEPARATOR);
		}
		CopyStringToCharArray(formattedSettings->ToString()->TrimEnd(GROUP_SEPARATOR), data);	
	}
	catch (TimeoutException^ ex)
	{
		SendErrorMessage("Timeout exception", data);
	}
	catch (System::ServiceModel::CommunicationException^ ex)
	{
		SendErrorMessage("Communication exception", data);
	}
	catch (System::ServiceModel::FaultException<ServiceContracts::ConfigurationFault^>^ ex)
	{
		SendErrorMessage("Cannot access configuration file", data);
	}
	return 3;
}

mIRCFunc(Alias_ClearSaved)
{
	try
	{
		static_cast<ServiceContracts::IAlias^>(Host::Client)->ClearSaved();
		SendOKMessage(data);
	}
	catch (TimeoutException^ ex)
	{
		SendErrorMessage("Timeout exception", data);
	}
	catch (System::ServiceModel::CommunicationException^ ex)
	{
		SendErrorMessage("Communication exception", data);
	}
	catch (System::ServiceModel::FaultException<ServiceContracts::ConfigurationFault^>^ ex)
	{
		SendErrorMessage("Cannot access configuration file", data);
	}
	return 3;
}
#pragma endregion

#pragma region Download

//FINSH

#pragma endregion
#pragma endregion
