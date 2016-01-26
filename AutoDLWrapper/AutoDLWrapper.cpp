// This is the main DLL file.

//*********************************
#include "stdafx.h"
#include "AutoDLWrapper.h"

#define ITEM_SEPARATOR ","
#define BOT_SEPARATOR "#"

using namespace System;
using namespace System::Collections::Generic;
using namespace AutoDL;

//Wrapper class for C# classes
static ref class Wrapper
{
public:
	static DownloadQueue^ const queue = gcnew DownloadQueue();
	static SettingsConfig^ const settings_config = gcnew SettingsConfig();
	static NicknameConfig^ const nick_config = gcnew NicknameConfig();
	static QueueConfig^ const queue_config = gcnew QueueConfig();
};

//Parses data string from mIRC into List
List<String^>^ ParseStringToList(String^% input)
{
	return gcnew List<String^>(input->Split(','));
}

//Parses packet ranges in the form of #-# into individual packets
List<int>^ ParsePacketNumbers(List<String^>^% packets)
{
	List<int>^ NewPacketList = gcnew List<int>();
	for each(String^ packet in packets)
	{
		if (!String::IsNullOrWhiteSpace(packet))
		{
			if (packet->Contains("-") == true)
			{
				array<String^>^ temp = packet->Split('-');
				int value1, value2;
				if (Int32::TryParse(temp[0], value1))
				{
					if (Int32::TryParse(temp[1], value2))
					{
						for (int i = value1; i <= value2; i++)
						{
							NewPacketList->Add(i);
						}
					}
				}
			}
			else
			{
				int value;
				if (Int32::TryParse(packet, value))
				{
					NewPacketList->Add(value);
				}
			}
		}
	}
	return NewPacketList;
}

void CopyStringToCharArray(String^% _data, char*& data)
{
	pin_ptr<const wchar_t> wch = PtrToStringChars(_data);			//pin _data to pass to wcstombs_s
	size_t convertedChars = 0;
	size_t sizeInBytes = (_data->Length + 1) * 2;
	wcstombs_s(&convertedChars, data, sizeInBytes, wch, _TRUNCATE);	//call wcstombs_s and prevent overwriting buffer
}

//Exported functions
void __stdcall LoadDll(LOADINFO* info)
{
	info->mKeep = true;
	info->mUnicode = false;
}

int __stdcall UnloadDll(int timeout)
{
	//if Dll is simply idle for 10 minutes
	if (timeout == 1)
	{
		//prevent unloading
		return 0;
	}
	//else on mIRC exit or /dll -u, unload
	return 1;
}

#pragma region Queue Functions

//Adds bot and packet(s) to queue, returns "null" on failure
mIRCFunc(Queue_Add)
{
	String^ input = (gcnew String(data))->Trim();
	String^ dataFormat = "^[^\\s,]+(,\\d+(-\\d+)?)+$";
	if (Text::RegularExpressions::Regex::IsMatch(input, dataFormat, Text::RegularExpressions::RegexOptions::ExplicitCapture))
	{
		List<String^>^ list = ParseStringToList(input);
		//Get Botname
		String^ bot = list[0];		
		//Only packets left
		list->RemoveAt(0);													
		List<int>^ parsedList = ParsePacketNumbers(list);
	    Wrapper::queue->Add(bot, parsedList);

		input = parsedList[0].ToString();
		for (int i = 1; i < parsedList->Count; i++)
		{
			input += ITEM_SEPARATOR + parsedList[i].ToString();
		}
	}
	else
	{
		input = "null";	
	}
	CopyStringToCharArray(input, data);
	return 3;
}

//Removes packet or bot from queue, returns "null" on failure
mIRCFunc(Queue_Remove)
{
	bool success = false;
	String^ input = (gcnew String(data))->Trim();
	String^ dataFormat = "^[^\\s,]+(,\\d+)?$";	
	if (Text::RegularExpressions::Regex::IsMatch(input, dataFormat, Text::RegularExpressions::RegexOptions::ExplicitCapture))
	{
		List<String^>^ _data = ParseStringToList(input);
		if (_data->Count == 2)
		{
			int packet = Int32::Parse(_data[1]);
			//Remove packet from bot
			success = Wrapper::queue->Remove(_data[0], packet);
		}
		else
		{
			//Remove bot
			success = Wrapper::queue->Remove(_data[0]);
		}
	}
	if (!success)
	{
		input = "null";
		CopyStringToCharArray(input, data);
	}
	return 3;
}

//Clears active queue
mIRCFunc(Queue_Clear)
{
	Wrapper::queue->RemoveAll();
	return 1;
}

//Returns "bot,packet", bot is "null" if no item to return
mIRCFunc(Queue_NextItem)
{
	Download download = Wrapper::queue->NextItem();
	String^ _data = download.Bot + ITEM_SEPARATOR + download.Packet.ToString();
	CopyStringToCharArray(_data, data);
	return 3;
}

//Saves Queue
mIRCFunc(Queue_Save)
{
	Wrapper::queue_config->SaveQueue(Wrapper::queue);
	return 1;
}

//Loads Queue, returns "null" if no queue to load
mIRCFunc(Queue_Load)
{
	List<String^>^ loaded = Wrapper::queue_config->LoadQueue(Wrapper::queue);
	String^ _data = "null";
	
	//construct String^ from List<String^>
	if (loaded->Count > 1)
	{
		int value;
		_data = loaded[0];
		for (int i = 1; i < loaded->Count; i++)
		{
			if (Int32::TryParse(loaded[i], value))
			{
				_data += ITEM_SEPARATOR + loaded[i];
			}
			else
			{
				_data += BOT_SEPARATOR + loaded[i];
			}
		}
	}
	CopyStringToCharArray(_data, data);
	return 3;
}

//Clears queue saved in config file
mIRCFunc(Queue_ClearSaved)
{
	Wrapper::queue_config->ClearSavedQueue();
	return 1;
}

#pragma endregion

#pragma region Settings Functions

//Saves settings
mIRCFunc(Settings_Save)
{
	String^ input = (gcnew String(data))->Trim();
	String^ dataFormat = "^(True|False),(True|False),\\d+$";
	if (Text::RegularExpressions::Regex::IsMatch(input, dataFormat, Text::RegularExpressions::RegexOptions::ExplicitCapture))
	{
		List<String^>^ _data = ParseStringToList(input);

		Settings newSettings;
		newSettings.Notifications = Boolean::Parse(_data[0]);
		newSettings.RetryFailedDownload = Boolean::Parse(_data[1]);
		newSettings.DownloadDelay = Int32::Parse(_data[2]);
		Wrapper::settings_config->SaveSettings(newSettings);
	}
	return 1;
}

//Loads settings
mIRCFunc(Settings_Load)
{
	Settings loaded;
	loaded = Wrapper::settings_config->LoadSettings();
	String^ _data = loaded.Notifications.ToString() + ITEM_SEPARATOR +
		loaded.RetryFailedDownload.ToString() + ITEM_SEPARATOR +
		loaded.DownloadDelay.ToString();
	CopyStringToCharArray(_data, data);
	return 3;
}

#pragma endregion

#pragma region Nickname Functions

//Add nickname to config file, return "null" on failure
mIRCFunc(Nick_Add)
{
	bool success = false;
	String^ input = (gcnew String(data))->Trim();
	String^ dataFormat = "^[^\\s,]+,[^\\s,]+$";
	if (Text::RegularExpressions::Regex::IsMatch(input, dataFormat))
	{
		List<String^>^ _data = ParseStringToList(input);
		Wrapper::nick_config->AddNick(_data[0], _data[1]);
		success = true;
	}
	if (!success)
	{
		input = "null";
		CopyStringToCharArray(input, data);
	}
	return 3;
}

//Remove nickname from config file, return "null" on failure
mIRCFunc(Nick_Remove)
{
	bool success = false;
	String^ input = (gcnew String(data))->Trim();
	String^ dataFormat = "^[^\\s,]+$";
	if (Text::RegularExpressions::Regex::IsMatch(input, dataFormat))
	{
		success = Wrapper::nick_config->RemoveNick(input);
	}
	if (!success)
	{
		input = "null";
		CopyStringToCharArray(input, data);
	}
	return 3;
}

//Clear nicknames in config file
mIRCFunc(Nick_Clear)
{
	Wrapper::nick_config->ClearNicks();
	return 1;
}

//Given a nickname, return original bot name, return "null" on failure
mIRCFunc(Nick_GetName)
{
	String^ input = (gcnew String(data))->Trim();
	String^ dataFormat = "^[^\\s,]+$";
	if (Text::RegularExpressions::Regex::IsMatch(input, dataFormat))
	{
		input = Wrapper::nick_config->GetName(input);
	}
	else
	{
		input = "null";
	}
	CopyStringToCharArray(input, data);
	return 3;
}

//Returns all bot/nickname pairs, returns "null" if no pairs
mIRCFunc(Nick_GetAll)
{
	List<String^>^ keyValues = Wrapper::nick_config->GetAllNamesAndNicks();
	String^ _data = "null";
	if (keyValues->Count > 1)
	{
		_data = keyValues[0];
		for (int i = 1; i < keyValues->Count; i++)
		{
			_data += ITEM_SEPARATOR + keyValues[i];
		}
	}
	CopyStringToCharArray(_data, data);
	return 3;
}

#pragma endregion