// This is the main DLL file.

//*********************************
#include "stdafx.h"
#include <windows.h>
#include <stdio.h>
#include <vcclr.h>
#include "AutoDLWrapper.h"

#define ITEM_SEPARATOR ","
#define BOT_SEPARATOR "#"

using namespace System;
using namespace System::Collections::Generic;
using namespace AutoDL;

//Wrapper class for two main C# classes
ref class Wrapper
{
public:
	//static SettingsData^ const sdata = gcnew SettingsData();
	static DownloadQueue^ const queue = gcnew DownloadQueue();
	static SettingsData^ const sdata = gcnew SettingsData();
};

//Parses data string from mIRC into List
List<String^>^ ParseStringToList(String^% input)
{
	return gcnew List<String^>(input->Split(','));
}

//Parses packet clusters in the form of #-# into individual packets
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
		return 0; //prevent unloading
	}
	return 1; //else on mIRC exit or /dll -u, unload
}

mIRCFunc(Queue_Add)
{
	String^ input = (gcnew String(data))->Trim();
	String^ dataFormat = "^[^\\s,]+(,\\d+(-\\d+)?)+$";
	if (Text::RegularExpressions::Regex::IsMatch(input, dataFormat, Text::RegularExpressions::RegexOptions::ExplicitCapture))
	{
		List<String^>^ list = ParseStringToList(input);
		String^ bot = list[0];												//Get Botname
		list->RemoveAt(0);													//Only packets left
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
			success = Wrapper::queue->Remove(_data[0], packet);
		}
		else
		{
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

mIRCFunc(Queue_Clear)
{
	Wrapper::queue->RemoveAll();
	return 1;
}

mIRCFunc(Queue_NextItem)
{
	Download download = Wrapper::queue->NextItem();
	String^ _data = download.Bot + ITEM_SEPARATOR + download.Packet.ToString();
	CopyStringToCharArray(_data, data);
	return 3;
}

mIRCFunc(Queue_Save)
{
	Wrapper::sdata->SaveQueue(Wrapper::queue);
	return 1;
}

mIRCFunc(Queue_Load)
{
	List<String^>^ loaded = Wrapper::sdata->LoadQueue(Wrapper::queue);
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

mIRCFunc(Queue_ClearSaved)
{
	Wrapper::sdata->ClearSavedQueue();
	return 1;
}

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
		Wrapper::sdata->SaveSettings(newSettings);
	}
	return 1;
}

mIRCFunc(Settings_Load)
{
	Settings loaded;
	loaded = Wrapper::sdata->LoadSettings();
	String^ _data = loaded.Notifications.ToString() + ITEM_SEPARATOR +
		loaded.RetryFailedDownload.ToString() + ITEM_SEPARATOR +
		loaded.DownloadDelay.ToString();
	CopyStringToCharArray(_data, data);
	return 3;
}

mIRCFunc(Nick_Add)
{
	bool success = false;
	String^ input = (gcnew String(data))->Trim();
	String^ dataFormat = "^[^\\s,]+,[^\\s,]+$";
	if (Text::RegularExpressions::Regex::IsMatch(input, dataFormat))
	{
		List<String^>^ _data = ParseStringToList(input);
		Wrapper::sdata->AddNick(_data[0], _data[1]);
		success = true;
	}
	if (!success)
	{
		input = "null";
		CopyStringToCharArray(input, data);
	}
	return 3;
}

mIRCFunc(Nick_Remove)
{
	bool success = false;
	String^ input = (gcnew String(data))->Trim();
	String^ dataFormat = "^[^\\s,]+$";
	if (Text::RegularExpressions::Regex::IsMatch(input, dataFormat))
	{
		success = Wrapper::sdata->RemoveNick(input);
	}
	if (!success)
	{
		input = "null";
		CopyStringToCharArray(input, data);
	}
	return 3;
}

mIRCFunc(Nick_Clear)
{
	Wrapper::sdata->ClearNicks();
	return 1;
}

mIRCFunc(Nick_GetName)
{
	String^ input = (gcnew String(data))->Trim();
	String^ dataFormat = "^[^\\s,]+$";
	if (Text::RegularExpressions::Regex::IsMatch(input, dataFormat))
	{
		input = Wrapper::sdata->GetName(input);
	}
	else
	{
		input = "null";
	}
	CopyStringToCharArray(input, data);
	return 3;
}

mIRCFunc(Nick_GetAll)
{
	List<String^>^ keyValues = Wrapper::sdata->GetAllNamesAndNicks();
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