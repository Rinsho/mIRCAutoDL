//mIRC Wrapper for AutoDL program

#include "stdafx.h"
#include "mIRCWrapper.h"

HANDLE file;
LPSTR message;
HWND mWnd;

static private ref class Host
{
public:
	static property AutoDLMain^ Service;
	static property DownloadCallback^ DownloadClientCallback;
	static property ServiceClient::DownloadClient^ DownloadClient;
	static property ServiceClient::AliasClient^ AliasClient;
	static property ServiceClient::SettingsClient^ SettingsClient;
};

[System::ServiceModel::CallbackBehaviorAttribute(UseSynchronizationContext = false)]
ref class DownloadCallback : ServiceContracts::IDownloadCallback
{
public:
	virtual void DownloadStatusUpdate(ServiceContracts::DownloadStatus status)
	{
		//Send update to IRC
	}
	virtual void Downloading(String^ name, int packet)
	{
		//Send update to IRC
	}
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
	Host::Service = gcnew AutoDLMain(gcnew Action<AutoDL::Data::Download^>(SendDownloadInfo), SERVICE_EXTENSION);

	//Setup service clients
	Host::DownloadClientCallback = gcnew DownloadCallback();
	Host::DownloadClient = gcnew ServiceClient::DownloadClient(gcnew System::ServiceModel::InstanceContext(Host::DownloadClientCallback), SERVICE_EXTENSION);
	Host::AliasClient = gcnew ServiceClient::AliasClient(SERVICE_EXTENSION);
	Host::SettingsClient = gcnew ServiceClient::SettingsClient(SERVICE_EXTENSION);
	Host::DownloadClient->OpenClient();
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
	Host::DownloadClient->CloseClient();
	Host::AliasClient->CloseClient();
	Host::SettingsClient->CloseClient();
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

void SendErrorMessage(String^% text, char*& udata)
{
	String^ message = gcnew String("#Error,") + text;
	CopyStringToCharArray(message, udata);
}

void SendOKMessage(char*& udata)
{
	udata = new char[]{ '#', 'O', 'K' };
}

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
		System::Text::StringBuilder^ formattedAliases = gcnew System::Text::StringBuilder();
		for each (KeyValuePair<String^, String^>^ pair in aliases)
		{
			formattedAliases->Append(pair->Key);
			formattedAliases->Append(ITEM_SEPARATOR);
			formattedAliases->Append(pair->Value);
			formattedAliases->Append(GROUP_SEPARATOR);
		}
		CopyStringToCharArray(formattedAliases->ToString()->TrimEnd(GROUP_SEPARATOR), data);	
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
mIRCFunc(Download_Add)
{
	String^ mdata;
	CopyCharArrayToString(data, mdata);
	array<String^, 1>^ splitData = mdata->Split(ITEM_SEPARATOR);
	String^ name = splitData[0];
	List<int>^ packets = gcnew List<int>();
	for (int i = 1; i < splitData->Length; i++)
	{
		packets->Add(Convert::ToInt32(splitData[i]));
	}
	try
	{
		static_cast<ServiceContracts::IDownload^>(Host::Client)->Add(name, packets);
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
	catch (System::ServiceModel::FaultException<ServiceContracts::InvalidPacketFault^>^ ex)
	{
		SendErrorMessage(ex->Detail->Description, data);
	}
	return 3;
}

mIRCFunc(Download_Remove)
{
	String^ mdata;
	CopyCharArrayToString(data, mdata);
	array<String^, 1>^ splitData = mdata->Split(ITEM_SEPARATOR);
	String^ name = splitData[0];
	List<int>^ packets = gcnew List<int>();
	for (int i = 1; i < splitData->Length; i++)
	{
		packets->Add(Convert::ToInt32(splitData[i]));
	}
	try
	{
		if (packets->Count > 0)
		{
			static_cast<ServiceContracts::IDownload^>(Host::Client)->Remove(name, packets);
		}
		else
		{
			static_cast<ServiceContracts::IDownload^>(Host::Client)->Remove(name);
		}
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

mIRCFunc(Download_Clear)
{
	try
	{
		static_cast<ServiceContracts::IDownload^>(Host::Client)->Clear();
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

mIRCFunc(Download_Save)
{
	try
	{
		static_cast<ServiceContracts::IDownload^>(Host::Client)->Save();
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

mIRCFunc(Download_Load)
{
	try
	{
		System::Collections::Specialized::OrderedDictionary^ downloads = static_cast<ServiceContracts::IDownload^>(Host::Client)->Load();
		System::Text::StringBuilder^ formattedDownloads = gcnew System::Text::StringBuilder();
		for each (KeyValuePair<String^, List<int>^>^ pair in downloads)
		{
			formattedDownloads->Append(pair->Key);
			for each (int packet in pair->Value)
			{
				formattedDownloads->Append(ITEM_SEPARATOR);
				formattedDownloads->Append(packet);
			}
			formattedDownloads->Append(GROUP_SEPARATOR);
		}
		CopyStringToCharArray(formattedDownloads->ToString()->TrimEnd(GROUP_SEPARATOR), data);
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

mIRCFunc(Download_ClearSaved)
{
	try
	{
		static_cast<ServiceContracts::IDownload^>(Host::Client)->ClearSaved();
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
#pragma endregion
