// mIRCClient.h

#pragma once
#pragma comment(lib, "user32.lib")
#pragma comment(linker, "/EXPORT:LoadDll=_LoadDll@4")
#pragma comment(linker, "/EXPORT:UnloadDll=_UnloadDll@4")
#pragma comment(linker, "/EXPORT:Setting_Update=_Setting_Update@24")
#pragma comment(linker, "/EXPORT:Setting_Default=_Setting_Default@24")
#pragma comment(linker, "/EXPORT:Setting_DefaultAll=_Setting_DefaultAll@24")
#pragma comment(linker, "/EXPORT:Setting_Save=_Setting_Save@24")
#pragma comment(linker, "/EXPORT:Setting_Load=_Setting_Load@24")
#pragma comment(linker, "/EXPORT:Alias_Add=_Alias_Add@24")
#pragma comment(linker, "/EXPORT:Alias_Remove=_Alias_Remove@24")
#pragma comment(linker, "/EXPORT:Alias_Clear=_Alias_Clear@24")
#pragma comment(linker, "/EXPORT:Alias_Save=_Alias_Save@24")
#pragma comment(linker, "/EXPORT:Alias_Load=_Alias_Load@24")
#pragma comment(linker, "/EXPORT:Alias_ClearSaved=_Alias_ClearSaved@24")
#pragma comment(linker, "/EXPORT:Download_Add=_Download_Add@24")
#pragma comment(linker, "/EXPORT:Download_Remove=_Download_Remove@24")
#pragma comment(linker, "/EXPORT:Download_Clear=_Download_Clear@24")
#pragma comment(linker, "/EXPORT:Download_Save=_Download_Save@24")
#pragma comment(linker, "/EXPORT:Download_Load=_Download_Load@24")
#pragma comment(linker, "/EXPORT:Download_ClearSaved=_Download_ClearSaved@24")
#pragma comment(linker, "/EXPORT:Download_StartDownload=_Download_StartDownload@24")

using namespace System;
using namespace System::Collections::Generic;
using namespace AutoDL;
using namespace AutoDL::ServiceContracts;
using namespace AutoDL::ServiceClients;

#define MIRC_BUFFER 4096
#define MIRC_DATABUFFER 900
#define MIRC_FILEMAP L"mIRC8"
#define MIRC_FILEMAPNUM 8
#define WM_MCOMMAND WM_USER + 200
#define WM_MEVALUATE WM_USER + 201
#define ITEM_SEPARATOR ','
#define SERVICE_EXTENSION "mIRC"
#define mIRCFunc(x) int __stdcall x(HWND mWnd, HWND aWnd, char* data, char* params, bool show, bool nopause)

typedef struct {
	DWORD  mVersion;
	HWND   mHwnd;
	BOOL   mKeep;
	BOOL   mUnicode;
} LOADINFO;

extern "C"
{
	void __stdcall LoadDll(LOADINFO*);
	int __stdcall UnloadDll(int);
	mIRCFunc(Setting_Update);
	mIRCFunc(Setting_Default);
	mIRCFunc(Setting_DefaultAll);
	mIRCFunc(Setting_Save);
	mIRCFunc(Setting_Load);
	mIRCFunc(Alias_Add);
	mIRCFunc(Alias_Remove);
	mIRCFunc(Alias_Clear);
	mIRCFunc(Alias_Save);
	mIRCFunc(Alias_Load);
	mIRCFunc(Alias_ClearSaved);
	mIRCFunc(Download_Add);
	mIRCFunc(Download_Remove);
	mIRCFunc(Download_Clear);
	mIRCFunc(Download_Save);
	mIRCFunc(Download_Load);
	mIRCFunc(Download_ClearSaved);
	mIRCFunc(Download_StartDownload);
}