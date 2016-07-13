//mIRC Wrapper for ServiceClient
//Provides a communication layer between the mIRC GUI script
//and the service host.

#include "stdafx.h"
#include "mIRCClient.h"

HANDLE file;
LPSTR message;
HWND mWnd;

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
	array<wchar_t>^ charToTrim = { ' ', ',', '#' };
	mdata = (gcnew String(udata))->Trim(charToTrim);
}

/// <summary>
/// Class that implements the callback contract for the update service.
/// </summary>
[System::ServiceModel::CallbackBehaviorAttribute(UseSynchronizationContext = false)]
ref class UpdateCallback : IReceiveUpdatesCallback
{
public:
	/// <summary>
	/// Calls the mIRC GUI to update the current finished download status.
	/// </summary>
	virtual void StatusUpdate(DownloadStatus status)
	{
		char* uCommand;
		String^ mCommand = "/DownloadStatusUpdate ";
		switch (status)
		{
		case DownloadStatus::Success:
			mCommand += "Complete";
			break;
		case DownloadStatus::Fail:
			mCommand += "Failed";
			break;
		case DownloadStatus::Retry:
			mCommand += "Retrying";
			break;
		}
		CopyStringToCharArray(mCommand, uCommand);
		strncpy_s(message, MIRC_BUFFER, uCommand, _TRUNCATE);
		SendMessage(mWnd, WM_MCOMMAND, 1, MIRC_FILEMAPNUM);
	}

	/// <summary>
	/// Calls the mIRC GUI to update what download is starting.
	/// </summary>
	virtual void DownloadingNext(Download^ download)
	{
		char* uCommand;
		String^ mCommand = "/Downloading " + download->Name + " " + download->Packet;
		CopyStringToCharArray(mCommand, uCommand);
		strncpy_s(message, MIRC_BUFFER, uCommand, _TRUNCATE);
		SendMessage(mWnd, WM_MCOMMAND, 1, MIRC_FILEMAPNUM);
	}
};

/// <summary>
/// Class that holds the service clients.
/// </summary>
ref class Host
{
public:
	static UpdateCallback^ UpdateClientCallback;
	static UpdateSubscriberClient^ UpdateClient;
	static DownloadClient^ DownloadClient;
	static AliasClient^ AliasClient;
	static SettingsClient^ SettingsClient;
};

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

	//Setup service clients
	Host::UpdateClientCallback = gcnew UpdateCallback();
	Host::UpdateClient = gcnew UpdateSubscriberClient(gcnew System::ServiceModel::InstanceContext(Host::UpdateClientCallback), SERVICE_EXTENSION);
	Host::DownloadClient = gcnew DownloadClient(SERVICE_EXTENSION);
	Host::AliasClient = gcnew AliasClient(SERVICE_EXTENSION);
	Host::SettingsClient = gcnew SettingsClient(SERVICE_EXTENSION);

	//Open clients for use
	Host::DownloadClient->OpenClient();
	Host::AliasClient->OpenClient();
	Host::SettingsClient->OpenClient();
	Host::UpdateClient->OpenClient();
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
	Host::DownloadClient->CloseClient();
	Host::AliasClient->CloseClient();
	Host::SettingsClient->CloseClient();
	Host::UpdateClient->CloseClient();
	UnmapViewOfFile(message);
	CloseHandle(file);	
	return 1;
}

/// <summary>
/// Sets return data to an error message.
/// </summary>
/// <param name="udata">mIRCFunc return data variable.</param>
void SendErrorMessage(String^ text, char*& udata)
{
	String^ message = gcnew String("#Error,") + text;
	CopyStringToCharArray(message, udata);
}

/// <summary>
/// Sets return data to a success message.
/// </summary>
/// <param name="udata">mIRCFunc return data variable.</param>
void SendOKMessage(char*& udata)
{
	udata = new char[]{ '#', 'O', 'K' };
}

/// <summary>
/// Wraps client calls to handle common exceptions.
/// </summary>
/// <typeparam name="TReturn">Return type of the client call.</typeparam>
/// <typeparam name="TLambda">Implicitly-determined lambda type.</typeparam>
/// <param name="data">mIRCFunc return data variable.</param>
/// <param name="call">Client call wrapped in lambda function.</param>
template <typename TReturn = void, typename TLambda>
TReturn ClientCall(char*& data, TLambda& call)
{
	try
	{
		return (TReturn)(call)();
	}
	catch (TimeoutException^)
	{
		SendErrorMessage("Timeout exception", data);
	}
	catch (System::ServiceModel::CommunicationException^)
	{
		SendErrorMessage("Communication exception", data);
	}
}

#pragma region Settings

mIRCFunc(Setting_Update)
{
	ClientCall(data, [&data]() -> void
	{
		String^ mdata;
		CopyCharArrayToString(data, mdata);
		array<String^, 1>^ splitData = mdata->Split(ITEM_SEPARATOR);
		SettingName^ name = static_cast<SettingName^>(System::Enum::Parse(SettingName::typeid, splitData[0]));
		Setting^ setting = gcnew Setting(*name, splitData[1]);
		Host::SettingsClient->Update(setting);
	});
	SendOKMessage(data);
	return 3;
}

mIRCFunc(Setting_Default)
{
	ClientCall(data, [&data]() -> void
	{
		String^ mdata;
		CopyCharArrayToString(data, mdata);
		SettingName^ name = static_cast<SettingName^>(System::Enum::Parse(SettingName::typeid, mdata));
		Host::SettingsClient->Default(*name);
	});
	SendOKMessage(data);
	return 3;
}

mIRCFunc(Setting_DefaultAll)
{
	ClientCall(data, []() -> void { Host::SettingsClient->DefaultAll(); });
	SendOKMessage(data);
	return 3;
}

mIRCFunc(Setting_Save)
{
	try
	{
		ClientCall(data, []() -> void { Host::SettingsClient->Save(); });
		SendOKMessage(data);
	}
	catch (System::ServiceModel::FaultException<ServiceContracts::ConfigurationFault^>^)
	{
		SendErrorMessage("Cannot access configuration file", data);
	}
	return 3;
}

mIRCFunc(Setting_Load)
{
	try
	{
		ClientCall(data, [&data]() -> void
		{
			array<Setting^,1>^ settings = Host::SettingsClient->Load();
			System::Text::StringBuilder^ formattedSettings = gcnew System::Text::StringBuilder();
			for each (Setting^ s in settings)
			{
				formattedSettings->Append(System::Enum::GetName(SettingName::typeid, s->Name));
				formattedSettings->Append(ITEM_SEPARATOR);
				formattedSettings->Append(s->Value);
				formattedSettings->Append(ITEM_SEPARATOR);
			}
			CopyStringToCharArray(formattedSettings->ToString()->TrimEnd(ITEM_SEPARATOR), data);
		});
	}
	catch (System::ServiceModel::FaultException<ServiceContracts::ConfigurationFault^>^)
	{
		SendErrorMessage("Cannot access configuration file", data);
	}
	return 3;
}
#pragma endregion

#pragma region Alias

mIRCFunc(Alias_Add)
{
	ClientCall(data, [&data]() -> void
	{
		String^ mdata;
		CopyCharArrayToString(data, mdata);
		array<String^, 1>^ splitData = mdata->Split(ITEM_SEPARATOR);
		Alias^ alias = gcnew Alias(splitData[0], splitData[1]);
		Host::AliasClient->Add(alias);
	});
	SendOKMessage(data);
	return 3;
}

mIRCFunc(Alias_Remove)
{
	ClientCall(data, [&data]() -> void
	{
		String^ mdata;
		CopyCharArrayToString(data, mdata);
		Host::AliasClient->Remove(mdata);
	});
	SendOKMessage(data);
	return 3;
}

mIRCFunc(Alias_Clear)
{
	ClientCall(data, []() -> void { Host::AliasClient->Clear(); });
	SendOKMessage(data);
	return 3;
}

mIRCFunc(Alias_Save)
{
	try
	{
		ClientCall(data, []() -> void { Host::AliasClient->Save(); });
		SendOKMessage(data);
	}
	catch (System::ServiceModel::FaultException<ServiceContracts::ConfigurationFault^>^)
	{
		SendErrorMessage("Cannot access configuration file", data);
	}
	return 3;
}

mIRCFunc(Alias_Load)
{
	try
	{
		ClientCall(data, [&data]() -> void
		{
			array<Alias^, 1>^ aliases = Host::AliasClient->Load();
			System::Text::StringBuilder^ formattedAliases = gcnew System::Text::StringBuilder();
			for each (Alias^ a in aliases)
			{
				formattedAliases->Append(a->AliasName);
				formattedAliases->Append(ITEM_SEPARATOR);
				formattedAliases->Append(a->Name);
				formattedAliases->Append(ITEM_SEPARATOR);
			}
			CopyStringToCharArray(formattedAliases->ToString()->TrimEnd(ITEM_SEPARATOR), data);
		});
	}
	catch (System::ServiceModel::FaultException<ServiceContracts::ConfigurationFault^>^)
	{
		SendErrorMessage("Cannot access configuration file", data);
	}
	return 3;
}

mIRCFunc(Alias_ClearSaved)
{
	try
	{
		ClientCall(data, []() -> void { Host::AliasClient->ClearSaved(); });
		SendOKMessage(data);
	}
	catch (System::ServiceModel::FaultException<ServiceContracts::ConfigurationFault^>^)
	{
		SendErrorMessage("Cannot access configuration file", data);
	}
	return 3;
}
#pragma endregion

#pragma region Download

mIRCFunc(Download_Add)
{
	ClientCall(data, [&data]() -> void
	{
		String^ mdata;
		CopyCharArrayToString(data, mdata);
		array<String^, 1>^ splitData = mdata->Split(ITEM_SEPARATOR);
		int numberOfDownloads = splitData->Length / 2;
		array<Download^, 1>^ downloads = gcnew array<Download^>(numberOfDownloads);
		for (int i = 0; i < numberOfDownloads; i++)
		{
			downloads[i] = gcnew Download(splitData[i * 2], Int32::Parse(splitData[(i * 2) + 1]));
		}
		Host::DownloadClient->Add(downloads);
	});
	SendOKMessage(data);
	return 3;
}

mIRCFunc(Download_Remove)
{
	ClientCall(data, [&data]() -> void
	{
		String^ mdata;
		CopyCharArrayToString(data, mdata);
		array<String^, 1>^ splitData = mdata->Split(ITEM_SEPARATOR);
		int numberOfDownloads = splitData->Length / 2;
		array<Download^, 1>^ downloads = gcnew array<Download^>(numberOfDownloads);
		for (int i = 0; i < numberOfDownloads; i++)
		{
			downloads[i] = gcnew Download(splitData[i * 2], Int32::Parse(splitData[(i * 2) + 1]));
		}
		Host::DownloadClient->Remove(downloads);
	});
	SendOKMessage(data);
	return 3;
}

mIRCFunc(Download_Clear)
{
	ClientCall(data, []() -> void { Host::DownloadClient->Clear(); });
	SendOKMessage(data);
	return 3;
}

mIRCFunc(Download_Save)
{
	try
	{
		ClientCall(data, []() -> void { Host::DownloadClient->Save(); });
		SendOKMessage(data);
	}
	catch (System::ServiceModel::FaultException<ServiceContracts::ConfigurationFault^>^)
	{
		SendErrorMessage("Cannot access configuration file", data);
	}
	return 3;
}

mIRCFunc(Download_Load)
{
	try
	{
		ClientCall(data, [&data]() -> void
		{
			array<Download^, 1>^ downloads = Host::DownloadClient->Load();
			System::Text::StringBuilder^ formattedDownloads = gcnew System::Text::StringBuilder();
			for each (Download^ d in downloads)
			{
				formattedDownloads->Append(d->Name);
				formattedDownloads->Append(ITEM_SEPARATOR);
				formattedDownloads->Append(d->Packet);
				formattedDownloads->Append(ITEM_SEPARATOR);
			}
			CopyStringToCharArray(formattedDownloads->ToString()->TrimEnd(ITEM_SEPARATOR), data);
		});
	}
	catch (System::ServiceModel::FaultException<ServiceContracts::ConfigurationFault^>^)
	{
		SendErrorMessage("Cannot access configuration file", data);
	}
	return 3;
}

mIRCFunc(Download_ClearSaved)
{
	try
	{
		ClientCall(data, []() -> void { Host::DownloadClient->ClearSaved(); });
		SendOKMessage(data);
	}
	catch (System::ServiceModel::FaultException<ServiceContracts::ConfigurationFault^>^)
	{
		SendErrorMessage("Cannot access configuration file", data);
	}
	return 3;
}

mIRCFunc(Download_StartDownload)
{
	ClientCall(data, []() -> void { Host::DownloadClient->StartDownload(); });
	SendOKMessage(data);
	return 3;
}
#pragma endregion