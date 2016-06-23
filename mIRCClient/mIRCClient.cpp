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
/// Class that implements the callback contract for the download service.
/// </summary>
[System::ServiceModel::CallbackBehaviorAttribute(UseSynchronizationContext = false)]
ref class DownloadCallback : ServiceContracts::IDownloadCallback
{
public:

	/// <summary>
	/// Calls the mIRC GUI to update the current finished download status.
	/// </summary>
	virtual void DownloadStatusUpdate(ServiceContracts::DownloadStatus status)
	{
		char* uCommand;
		String^ mCommand = "/DownloadStatusUpdate ";
		switch (status)
		{
		case ServiceContracts::DownloadStatus::Success:
			mCommand += "Complete";
			break;
		case ServiceContracts::DownloadStatus::Fail:
			mCommand += "Failed";
			break;
		case ServiceContracts::DownloadStatus::Retry:
			mCommand += "Retrying";
			break;
		case ServiceContracts::DownloadStatus::QueueComplete:
			mCommand += "Finished";
			break;
		}
		CopyStringToCharArray(mCommand, uCommand);
		strncpy_s(message, MIRC_BUFFER, uCommand, _TRUNCATE);
		SendMessage(mWnd, WM_MCOMMAND, 1, MIRC_FILEMAPNUM);
	}

	/// <summary>
	/// Calls the mIRC GUI to update what download is starting.
	/// </summary>
	virtual void Downloading(String^ name, int packet)
	{
		char* uCommand;
		String^ mCommand = "/Downloading " + name + " " + packet;
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
	static DownloadCallback^ DownloadClientCallback;
	static ServiceClient::DownloadClient^ DownloadClient;
	static ServiceClient::AliasClient^ AliasClient;
	static ServiceClient::SettingsClient^ SettingsClient;
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
	Host::DownloadClientCallback = gcnew DownloadCallback();
	Host::DownloadClient = gcnew ServiceClient::DownloadClient(gcnew System::ServiceModel::InstanceContext(Host::DownloadClientCallback), SERVICE_EXTENSION);
	Host::AliasClient = gcnew ServiceClient::AliasClient(SERVICE_EXTENSION);
	Host::SettingsClient = gcnew ServiceClient::SettingsClient(SERVICE_EXTENSION);
	Host::DownloadClient->OpenClient();
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
	UnmapViewOfFile(message);
	CloseHandle(file);
	Host::DownloadClient->CloseClient();
	Host::AliasClient->CloseClient();
	Host::SettingsClient->CloseClient();
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
	try
	{
		ClientCall(data, [&data]() -> void
		{
			String^ mdata;
			CopyCharArrayToString(data, mdata);
			array<String^, 1>^ splitData = mdata->Split(ITEM_SEPARATOR);
			Host::SettingsClient->Update(splitData[0], splitData[1]);
		});
		SendOKMessage(data);
	}
	catch (System::ServiceModel::FaultException<ServiceContracts::InvalidSettingFault^>^)
	{
		SendErrorMessage("Cannot access configuration file", data);
	}
	return 3;
}

mIRCFunc(Setting_Default)
{
	try
	{
		ClientCall(data, [&data]() -> void
		{
			String^ mdata;
			CopyCharArrayToString(data, mdata);
			Host::SettingsClient->Default(mdata);
		});
		SendOKMessage(data);
	}
	catch (System::ServiceModel::FaultException<ServiceContracts::InvalidSettingFault^>^)
	{
		SendErrorMessage("Invalid setting value", data);
	}
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
			Dictionary<String^, String^>^ settings = Host::SettingsClient->Load();
			System::Text::StringBuilder^ formattedSettings = gcnew System::Text::StringBuilder();
			for each (KeyValuePair<String^, String^>^ pair in settings)
			{
				formattedSettings->Append(pair->Key);
				formattedSettings->Append(ITEM_SEPARATOR);
				formattedSettings->Append(pair->Value);
				formattedSettings->Append(GROUP_SEPARATOR);
			}
			CopyStringToCharArray(formattedSettings->ToString()->TrimEnd(GROUP_SEPARATOR), data);
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
		Host::AliasClient->Add(splitData[0], splitData[1]);
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
			Dictionary<String^, String^>^ aliases = Host::AliasClient->Load();
			System::Text::StringBuilder^ formattedAliases = gcnew System::Text::StringBuilder();
			for each (KeyValuePair<String^, String^>^ pair in aliases)
			{
				formattedAliases->Append(pair->Key);
				formattedAliases->Append(ITEM_SEPARATOR);
				formattedAliases->Append(pair->Value);
				formattedAliases->Append(GROUP_SEPARATOR);
			}
			CopyStringToCharArray(formattedAliases->ToString()->TrimEnd(GROUP_SEPARATOR), data);
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
	try
	{
		ClientCall(data, [&data]() -> void
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
			Host::DownloadClient->Add(name, packets);
		});
		SendOKMessage(data);
	}
	catch (System::ServiceModel::FaultException<ServiceContracts::InvalidPacketFault^>^ ex)
	{
		SendErrorMessage(ex->Detail->Description, data);
	}
	return 3;
}

mIRCFunc(Download_Remove)
{
	ClientCall(data, [&data]() -> void
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
		if (packets->Count > 0)
		{
			Host::DownloadClient->Remove(name, packets);
		}
		else
		{
			Host::DownloadClient->Remove(name);
		}
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
			System::Collections::Specialized::OrderedDictionary^ downloads = Host::DownloadClient->Load();
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
#pragma endregion